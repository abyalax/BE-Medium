using System.Text.Json;

using NATS.Client.Core;

namespace Medium.Api.Infrastructure.Nats.Services;

public interface INatsSubscriber
{
  Task SubscribeAsync<T>(string subject, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class;
  Task SubscribeToRequestAsync<TRequest, TResponse>(string subject, Func<TRequest, Task<TResponse>> handler, CancellationToken cancellationToken = default)
    where TRequest : class
    where TResponse : class;
}

public class NatsSubscriber(INatsConnectionProvider connectionProvider, ILogger<NatsSubscriber> logger) : INatsSubscriber
{
  private readonly INatsConnectionProvider _connectionProvider = connectionProvider;
  private readonly ILogger<NatsSubscriber> _logger = logger;

  public async Task SubscribeAsync<T>(string subject, Func<T, Task> handler, CancellationToken cancellationToken = default) where T : class
  {
    try
    {
      await foreach (var msg in _connectionProvider.Connection.SubscribeAsync<T>(subject, cancellationToken: cancellationToken))
      {
        try
        {
          if (msg.Data != null)
          {
            await handler(msg.Data);
            _logger.LogInformation("Handled message from {Subject}", subject);
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error handling message from {Subject}", subject);
        }
      }
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Subscription to {Subject} cancelled", subject);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error processing subscription to {Subject}", subject);
    }
  }

  public async Task SubscribeToRequestAsync<TRequest, TResponse>(string subject, Func<TRequest, Task<TResponse>> handler, CancellationToken cancellationToken = default)
    where TRequest : class
    where TResponse : class
  {
    try
    {
      await foreach (var msg in _connectionProvider.Connection.SubscribeAsync<TRequest>(subject, cancellationToken: cancellationToken))
      {
        try
        {
          if (msg.Data != null)
          {
            var response = await handler(msg.Data);
            var serialized = JsonSerializer.Serialize(response);
            await msg.ReplyAsync(serialized);
            _logger.LogInformation("Handled request-reply from {Subject}", subject);
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error handling request from {Subject}", subject);
          if (msg.ReplyTo != null)
          {
            await _connectionProvider.Connection.PublishAsync(msg.ReplyTo, $"{{\"error\":\"{ex.Message}\"}}");
          }
        }
      }
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("Request subscription to {Subject} cancelled", subject);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error processing request subscription to {Subject}", subject);
    }
  }
}