using System.Text.Json;

using NATS.Client;

namespace Medium.Api.Infrastructure.Nats.Services;

public interface INatsPublisher
{
  Task PublishAsync<T>(string subject, T @event) where T : class;
}

public class NatsPublisher : INatsPublisher
{
  private readonly IConnection _connection;
  private readonly ILogger<NatsPublisher> _logger;

  public NatsPublisher(IConnection connection, ILogger<NatsPublisher> logger)
  {
    _connection = connection;
    _logger = logger;
  }

  public Task PublishAsync<T>(string subject, T @event) where T : class
  {
    try
    {
      var json = JsonSerializer.Serialize(@event);
      _connection.Publish(subject, System.Text.Encoding.UTF8.GetBytes(json));
      _logger.LogInformation("Published to {Subject}: {@Event}", subject, @event);
      return Task.CompletedTask;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error publishing to {Subject}", subject);
      throw;
    }
  }
}