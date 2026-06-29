using Microsoft.Extensions.Diagnostics.HealthChecks;

using Medium.Api.Infrastructure.Nats.Services;

namespace Medium.Api.Infrastructure.Nats;

public class NatsHealthCheck(INatsConnectionProvider connectionProvider) : IHealthCheck
{
  private readonly INatsConnectionProvider _connectionProvider = connectionProvider;

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    try
    {
      var connection = _connectionProvider.Connection;

      // Check if connection is connected
      if (connection != null)
      {
        return HealthCheckResult.Healthy("NATS connection is healthy");
      }

      return HealthCheckResult.Unhealthy("NATS connection is not initialized");
    }
    catch (InvalidOperationException ex)
    {
      return HealthCheckResult.Unhealthy("NATS connection has not been initialized", ex);
    }
    catch (Exception ex)
    {
      return HealthCheckResult.Unhealthy("NATS connection failed", ex);
    }
  }
}
