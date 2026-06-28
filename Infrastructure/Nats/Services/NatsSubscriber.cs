using System.Text.Json;

using Microsoft.Extensions.Logging;

using NATS.Client.Core;

namespace Medium.Api.Infrastructure.Nats.Services;

public interface INatsSubscriber
{
  Task SubscribeAsync<T>(string subject, Func<T, Task> handler) where T : class;
}

public class NatsSubscriber(NatsConnection connection, ILogger<NatsSubscriber> logger) : INatsSubscriber
{
  private readonly NatsConnection _connection = connection;
  private readonly ILogger<NatsSubscriber> _logger = logger;

  public Task SubscribeAsync<T>(string subject, Func<T, Task> handler) where T : class
  {
    _ = Task.Run(async () =>
    {
      try
      {
        await foreach (var msg in _connection.SubscribeAsync<string>(subject))
        {
          try
          {
            if (string.IsNullOrEmpty(msg.Data)) continue;
            var @event = JsonSerializer.Deserialize<T>(msg.Data);
            if (@event != null)
            {
              await handler(@event);
              _logger.LogInformation("Handled message from {Subject}", subject);
            }
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "Error handling message from {Subject}", subject);
          }
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error processing subscription to {Subject}", subject);
      }
    });

    _logger.LogInformation("Subscribed to {Subject} (background loop started)", subject);
    return Task.CompletedTask;
  }
}