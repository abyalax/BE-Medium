using Medium.Api.Infrastructure.Interface;
using Medium.Api.Infrastructure.Nats.Consumers;
using Medium.Api.Infrastructure.Nats.Services;

namespace Medium.Api.Infrastructure.Lifecycle;

public sealed class ApplicationLifecycleService(IServiceProvider serviceProvider, ILogger<ApplicationLifecycleService> logger) : IHostedService, IDisposable
{
  private readonly IServiceProvider _serviceProvider = serviceProvider;
  private readonly ILogger<ApplicationLifecycleService> _logger = logger;
  private ApplicationModule? _module;
  private IServiceScope? _scope;
  private List<IManuallyStartableService>? _consumerServices;

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
        _logger.LogError(ex, "JetStream initialization failed. The application will continue without JetStream functionality.");
        _logger.LogWarning("To enable JetStream, ensure NATS server is started with -js flag");
      }

      _logger.LogInformation("[OnStartup] ✓ Application is ready and running");

      // Start NATS consumer services after infrastructure is ready
      _consumerServices = new List<IManuallyStartableService>();
      try
      {
        var userRegisteredConsumer = _scope.ServiceProvider.GetRequiredService<UserRegisteredPushConsumer>();
        _consumerServices.Add(userRegisteredConsumer);
        _ = Task.Run(() => userRegisteredConsumer.StartAsync(cancellationToken), cancellationToken);
        _logger.LogInformation("Started UserRegisteredPushConsumer");
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Failed to start UserRegisteredPushConsumer");
      }

      try
      {
        var userLoggedInConsumer = _scope.ServiceProvider.GetRequiredService<UserLoggedInPullConsumer>();
        _consumerServices.Add(userLoggedInConsumer);
        _ = Task.Run(() => userLoggedInConsumer.StartAsync(cancellationToken), cancellationToken);
        _logger.LogInformation("Started UserLoggedInPullConsumer");
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Failed to start UserLoggedInPullConsumer");
      }

      try
      {
        var emailResponder = _scope.ServiceProvider.GetRequiredService<EmailServiceResponder>();
        _consumerServices.Add(emailResponder);
        _ = Task.Run(() => emailResponder.StartAsync(cancellationToken), cancellationToken);
        _logger.LogInformation("Started EmailServiceResponder");
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Failed to start EmailServiceResponder");
      }

      try
      {
        var articlePublishedConsumer = _scope.ServiceProvider.GetRequiredService<ArticlePublishedPushConsumer>();
        _consumerServices.Add(articlePublishedConsumer);
        _ = Task.Run(() => articlePublishedConsumer.StartAsync(cancellationToken), cancellationToken);
        _logger.LogInformation("Started ArticlePublishedPushConsumer");
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Failed to start ArticlePublishedPushConsumer");
      }

      try
      {
        var articleServiceResponder = _scope.ServiceProvider.GetRequiredService<ArticleServiceResponder>();
        _consumerServices.Add(articleServiceResponder);
        _ = Task.Run(() => articleServiceResponder.StartAsync(cancellationToken), cancellationToken);
        _logger.LogInformation("Started ArticleServiceResponder");
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Failed to start ArticleServiceResponder");
      }

      try
      {
        var aiSummarizationWorker = _scope.ServiceProvider.GetRequiredService<AiSummarizationWorker>();
        _consumerServices.Add(aiSummarizationWorker);
        _ = Task.Run(() => aiSummarizationWorker.StartAsync(cancellationToken), cancellationToken);
        _logger.LogInformation("Started AiSummarizationWorker");
      }
      catch (Exception ex)
      {
        _logger.LogWarning(ex, "Failed to start AiSummarizationWorker");
      }
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

    // Stop all consumer services
    if (_consumerServices != null)
    {
      foreach (var consumer in _consumerServices)
      {
        try
        {
          await consumer.StopAsync(cancellationToken);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error stopping consumer service");
        }
      }
      _consumerServices.Clear();
      _consumerServices = null;
    }

    if (_module != null)
    {
      ((IDisposable)_module).Dispose();
      _module = null;
    }

    // Shutdown NATS connection
    var natsLifecycle = _scope?.ServiceProvider.GetRequiredService<INatsLifecycle>();
    if (natsLifecycle != null)
      await natsLifecycle.ShutdownAsync(cancellationToken);

    _scope?.Dispose();
    _scope = null;

    _logger.LogInformation("[OnShutdown] ✓ Application gracefully shutdown");
  }

  public void Dispose()
  {
    if (_module != null)
    {
      ((IDisposable)_module).Dispose();
      _module = null;
    }
    _scope?.Dispose();
    _scope = null;
  }
}