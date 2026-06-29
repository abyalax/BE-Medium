using Medium.Api.Infrastructure.Interface;
using Medium.Api.Infrastructure.Nats.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Medium.Api.Infrastructure.Lifecycle;

public class ApplicationLifecycleService : IHostedService, IDisposable
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<ApplicationLifecycleService> _logger;
  private ApplicationModule? _module;
  private IServiceScope? _scope;

  public ApplicationLifecycleService(IServiceProvider serviceProvider, ILogger<ApplicationLifecycleService> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("--- [OnStartup] Starting Application and connecting Infrastructure ---");

    try
    {
      _scope = _serviceProvider.CreateScope();

      // Initialize database connection
      var dbLifecycle = _scope.ServiceProvider.GetRequiredService<IDbConnectionLifecycle>();
      await dbLifecycle.InitializeAsync(cancellationToken);

      // Initialize cache connection
      var cacheLifecycle = _scope.ServiceProvider.GetRequiredService<ICacheLifecycle>();
      await cacheLifecycle.InitializeAsync(cancellationToken);

      // Initialize NATS connection
      var natsLifecycle = _scope.ServiceProvider.GetRequiredService<INatsLifecycle>();
      await natsLifecycle.InitializeAsync(cancellationToken);

      // Create application module once infrastructure is ready
      _module = _scope.ServiceProvider.GetRequiredService<ApplicationModule>();

      // Initialize JetStream streams and consumers (optional)
      var natsConnectionProvider = _scope.ServiceProvider.GetRequiredService<INatsConnectionProvider>();
      try
      {
        await JetStreamInitializer.InitializeJetStreamAsync(natsConnectionProvider, _logger, cancellationToken);
        await JetStreamInitializer.InitializeConsumersAsync(natsConnectionProvider, _logger, cancellationToken);
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "JetStream initialization failed. The application will continue without JetStream functionality.");
        _logger.LogWarning("To enable JetStream, ensure NATS server is started with -js flag");
      }

      _logger.LogInformation("[OnStartup] ✓ Application is ready and running");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "[OnStartup] ✗ Application startup failed");
      throw;
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("--- [OnShutdown] Stopping Application ---");

    if (_module != null)
    {
      _module.Dispose();
      _module = null;
    }

    // Shutdown NATS connection
    var natsLifecycle = _scope?.ServiceProvider.GetRequiredService<INatsLifecycle>();
    if (natsLifecycle != null)
    {
      await natsLifecycle.ShutdownAsync(cancellationToken);
    }

    _scope?.Dispose();
    _scope = null;

    _logger.LogInformation("[OnShutdown] ✓ Application stopped cleanly");
  }

  public void Dispose()
  {
    if (_module != null)
    {
      _module.Dispose();
      _module = null;
    }
    _scope?.Dispose();
    _scope = null;
  }
}