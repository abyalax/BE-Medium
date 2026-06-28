using Medium.Api.Infrastructure.Interface;
using Medium.Api.Infrastructure.Nats.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NATS.Client.Core;

namespace Medium.Api.Infrastructure.Lifecycle;

public class ApplicationLifecycleService : IHostedService, IAsyncDisposable
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

      // Initialize JetStream streams
      var natsConnection = _scope.ServiceProvider.GetRequiredService<NatsConnection>();
      await JetStreamInitializer.InitializeJetStreamAsync(natsConnection, _logger);

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
      await _module.DisposeAsync();
      _module = null;
    }

    _scope?.Dispose();
    _scope = null;

    _logger.LogInformation("[OnShutdown] ✓ Application stopped cleanly");
  }

  public async ValueTask DisposeAsync()
  {
    if (_module != null)
    {
      await _module.DisposeAsync();
      _module = null;
    }
    _scope?.Dispose();
    _scope = null;
  }
}