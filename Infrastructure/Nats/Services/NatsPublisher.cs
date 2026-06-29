using System.Text.Json;

using NATS.Client.JetStream;

namespace Medium.Api.Infrastructure.Nats.Services;

public interface INatsPublisher
{
  Task PublishAsync<T>(string subject, T @event, CancellationToken cancellationToken = default) where T : class;
  Task<TResponse> RequestAsync<TRequest, TResponse>(string subject, TRequest request, CancellationToken cancellationToken = default)
    where TRequest : class
    where TResponse : class;
}

public class NatsPublisher(INatsConnectionProvider connectionProvider, ILogger<NatsPublisher> logger) : INatsPublisher
{
  public async Task PublishAsync<T>(string subject, T @event, CancellationToken cancellationToken = default) where T : class
  {
    try
    {
      await connectionProvider.Connection.PublishAsync(subject, @event, cancellationToken: cancellationToken);
      logger.LogInformation("Published to {Subject}", subject);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error publishing to {Subject}", subject);
      throw;
    }
  }

  public async Task<TResponse> RequestAsync<TRequest, TResponse>(string subject, TRequest request, CancellationToken cancellationToken = default)
    where TRequest : class
    where TResponse : class
  {
    try
    {
      var jsonPayload = JsonSerializer.Serialize(request);
      logger.LogInformation("Sending request on subject {Subject}", subject);

      var replyMessage = await connectionProvider.Connection.RequestAsync<string, string>(subject, jsonPayload, cancellationToken: cancellationToken);

      if (string.IsNullOrEmpty(replyMessage.Data))
        throw new InvalidOperationException($"Received empty or null reply response from subject {subject}");

      var response = JsonSerializer.Deserialize<TResponse>(replyMessage.Data)
        ?? throw new InvalidOperationException($"Failed to deserialize reply response for subject {subject}");

      logger.LogInformation("Reply received successfully for request on subject {Subject}", subject);
      return response;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed request-reply sequence on subject {Subject}", subject);
      throw;
    }
  }
}

public interface IJetStreamEventPublisher
{
  Task PublishToStreamAsync<T>(string subject, T @event, CancellationToken cancellationToken = default) where T : class;
}

public class JetStreamEventPublisher(INatsConnectionProvider connectionProvider, ILogger<JetStreamEventPublisher> logger) : IJetStreamEventPublisher
{
  public async Task PublishToStreamAsync<T>(string subject, T @event, CancellationToken cancellationToken = default) where T : class
  {
    try
    {
      var js = new NatsJSContext(connectionProvider.Connection);
      await js.PublishAsync(subject, @event, cancellationToken: cancellationToken);
      logger.LogInformation("Event published to JetStream on subject {Subject}", subject);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to publish to JetStream on subject {Subject}", subject);
      throw;
    }
  }
}