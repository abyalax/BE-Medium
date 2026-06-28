using Medium.Api.Infrastructure.Interface;

using Microsoft.Extensions.Logging;

namespace Medium.Api.Infrastructure.Lifecycle;

public class ApplicationModule : IAsyncDisposable
{
  private readonly IDbConnectionLifecycle _dbLifecycle;
  private readonly ICacheLifecycle _cacheLifecycle;
  private readonly INatsLifecycle _natsLifecycle;
  private readonly ILogger<ApplicationModule> _logger;

  public ApplicationModule(
    IDbConnectionLifecycle dbLifecycle,
    ICacheLifecycle cacheLifecycle,
    INatsLifecycle natsLifecycle,
    ILogger<ApplicationModule> logger)
  {
    _dbLifecycle = dbLifecycle;
    _cacheLifecycle = cacheLifecycle;
    _natsLifecycle = natsLifecycle;
    _logger = logger;
    _logger.LogInformation("[ApplicationModule] Created and all lifecycle dependencies resolved");
  }

  public async ValueTask DisposeAsync()
  {
    _logger.LogInformation("[ApplicationModule] Disposing - shutting down all infrastructure connections...");

    try
    {
      await _natsLifecycle.ShutdownAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error shutting down NATS lifecycle");
    }

    try
    {
      await _cacheLifecycle.ShutdownAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error shutting down Cache lifecycle");
    }

    try
    {
      await _dbLifecycle.ShutdownAsync();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error shutting down Database lifecycle");
    }
  }
}