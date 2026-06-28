using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Medium.Api.Infrastructure.Nats.Consumers;

/// <summary>
/// Placeholder until JetStream pull consumer API is aligned with NATS.Client.JetStream 2.x.
/// </summary>
public class UserLoggedInPullConsumer(ILogger<UserLoggedInPullConsumer> logger) : BackgroundService
{
  protected override Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("UserLoggedInPullConsumer is disabled pending JetStream API migration");
    return Task.Delay(Timeout.Infinite, stoppingToken);
  }
}
