using System.Text.Json;

using NATS.Client;

namespace Medium.Api.Infrastructure.Nats.Services;

public interface INatsPublisher
{
  Task PublishAsync<T>(string subject, T @event) where T : class;
}

public class NatsPublisher(IConnection connection, ILogger<NatsPublisher> logger) : INatsPublisher
{

  public Task PublishAsync<T>(string subject, T @event) where T : class
  {
    try
    {
      var json = JsonSerializer.Serialize(@event);
      connection.Publish(subject, System.Text.Encoding.UTF8.GetBytes(json));
      logger.LogInformation("Published to {Subject}: {@Event}", subject, @event);
      return Task.CompletedTask;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error publishing to {Subject}", subject);
      throw;
    }
  }
}