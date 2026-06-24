namespace Medium.Api.Infrastructure.Nats.Services;

using System.Text.Json;
using Microsoft.Extensions.Logging;
using NATS.Client;

public interface INatsSubscriber
{
    Task SubscribeAsync<T>(string subject, Func<T, Task> handler) where T : class;
}

public class NatsSubscriber : INatsSubscriber
{
    private readonly IConnection _connection;
    private readonly ILogger<NatsSubscriber> _logger;

    public NatsSubscriber(IConnection connection, ILogger<NatsSubscriber> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public Task SubscribeAsync<T>(string subject, Func<T, Task> handler) where T : class
    {
        try
        {
            var sub = _connection.SubscribeAsync(subject);

            // Background subscription handler
            _ = Task.Run(() =>
            {
                sub.MessageHandler += (sender, args) =>
                {
                    try
                    {
                        var json = System.Text.Encoding.UTF8.GetString(args.Message.Data);
                        var @event = JsonSerializer.Deserialize<T>(json)
                            ?? throw new InvalidOperationException($"Failed to deserialize {subject}");

                        Task.Run(() => handler(@event));
                        _logger.LogInformation("Handled message from {Subject}", subject);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling message from {Subject}", subject);
                    }
                };
                
                sub.Start();
            });

            _logger.LogInformation("Subscribed to {Subject}", subject);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to {Subject}", subject);
            throw;
        }
    }
}