using Medium.Api.Infrastructure.Interface;


namespace Medium.Api.Infrastructure.Lifecycle;

public sealed class ApplicationModule : IDisposable
{
  private readonly IDbConnectionLifecycle _dbLifecycle;
  private readonly ICacheLifecycle _cacheLifecycle;
  private readonly ILogger<ApplicationModule> _logger;

  public ApplicationModule(
    IDbConnectionLifecycle dbLifecycle,
    ICacheLifecycle cacheLifecycle,
    ILogger<ApplicationModule> logger)
  {
    _dbLifecycle = dbLifecycle;
    _cacheLifecycle = cacheLifecycle;
    _logger = logger;
    _logger.LogInformation("[ApplicationModule] Created and all lifecycle dependencies resolved");
  }

  void IDisposable.Dispose()
  {
    _logger.LogInformation("[ApplicationModule] Disposing - shutting down all infrastructure connections...");

    try
    {
      _cacheLifecycle.ShutdownAsync().GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error shutting down Cache lifecycle");
    }

    try
    {
      _dbLifecycle.ShutdownAsync().GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error shutting down Database lifecycle");
    }
  }
}