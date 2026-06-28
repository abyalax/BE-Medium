using System.Text.Json;

using Microsoft.Extensions.Logging;

using NATS.Client.Core;
using NATS.Client.JetStream;

namespace Medium.Api.Infrastructure.Nats.Services;

public interface INatsPublisher
{
  Task PublishAsync<T>(string subject, T @event) where T : class;
  Task<TResponse> RequestAsync<TRequest, TResponse>(string subject, TRequest request)
      where TRequest : class where TResponse : class;
}

public class NatsPublisher(NatsConnection connection, ILogger<NatsPublisher> logger) : INatsPublisher
{
  private readonly NatsConnection _connection = connection;
  private readonly ILogger<NatsPublisher> _logger = logger;

  public async Task PublishAsync<T>(string subject, T @event) where T : class
  {
    try
    {
      var json = JsonSerializer.Serialize(@event);
      await _connection.PublishAsync(subject, json);
      _logger.LogInformation("Published to {Subject}: {Json}", subject, json);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error publishing to {Subject}", subject);
      throw;
    }
  }

  public async Task<TResponse> RequestAsync<TRequest, TResponse>(string subject, TRequest request)
      where TRequest : class where TResponse : class
  {
    try
    {
      var json = JsonSerializer.Serialize(request);
      var reply = await _connection.RequestAsync<string, string>(subject, json);
      var replyData = reply.Data;

      if (string.IsNullOrEmpty(replyData))
        throw new InvalidOperationException($"Empty reply received for request on subject {subject}");

      var response = JsonSerializer.Deserialize<TResponse>(replyData)
                  ?? throw new InvalidOperationException($"Failed to deserialize reply response for subject {subject}");

      _logger.LogInformation("Request-Reply completed on {Subject}", subject);
      return response;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed request-reply on {Subject}", subject);
      throw;
    }
  }
}

public interface IJetStreamEventPublisher
{
  Task PublishToStreamAsync<T>(string streamName, string subject, T @event) where T : class;
}

public class JetStreamEventPublisher(NatsConnection connection, ILogger<JetStreamEventPublisher> logger) : IJetStreamEventPublisher
{
  private readonly NatsConnection _connection = connection;
  private readonly ILogger<JetStreamEventPublisher> _logger = logger;

  public async Task PublishToStreamAsync<T>(string streamName, string subject, T @event) where T : class
  {
    try
    {
      var js = new NatsJSContext(_connection);
      var json = JsonSerializer.Serialize(@event);
      await js.PublishAsync(subject, json);
      _logger.LogInformation("Event published to JetStream {StreamName}: {Subject} -> {Json}", streamName, subject, json);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to publish to JetStream stream {StreamName} on subject {Subject}", streamName, subject);
      throw;
    }
  }
}