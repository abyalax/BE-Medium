using Medium.Api.Infrastructure.Interface;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;


using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Medium.Api.Infrastructure.Nats.Consumers;

public class UserRegisteredPushConsumer(
    INatsConnectionProvider connectionProvider,
    ILogger<UserRegisteredPushConsumer> logger) : IManuallyStartableService
{
  private CancellationTokenSource? _cancellationTokenSource;

  public async Task StartAsync(CancellationToken cancellationToken = default)
  {
    logger.LogInformation("Starting UserRegisteredPushConsumer...");
    _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

    try
    {
      var js = new NatsJSContext(connectionProvider.Connection);

      INatsJSConsumer consumer;
      try
      {
        consumer = await js.CreateConsumerAsync(
            "USER_EVENTS",
            new ConsumerConfig("user-registered-push")
            {
              FilterSubject = "user.registered",
              AckPolicy = ConsumerConfigAckPolicy.Explicit
            },
            cancellationToken: cancellationToken);
      }
      catch (Exception)
      {
        consumer = await js.GetConsumerAsync("USER_EVENTS", "user-registered-push", cancellationToken: cancellationToken);
      }

      await foreach (var msg in consumer.ConsumeAsync<UserRegisteredEvent>(cancellationToken: cancellationToken))
      {
        try
        {
          logger.LogInformation("Received user registered event: {UserId} - {Email}", msg.Data?.UserId, msg.Data?.Email);
          if (msg.Data != null)
          {
            logger.LogInformation("Successfully processed user registered event for {Email}", msg.Data.Email);
          }
          await msg.AckAsync(cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Error processing user registered event");
          await msg.NakAsync(cancellationToken: cancellationToken);
        }
      }
    }
    catch (NatsJSException ex) when (!cancellationToken.IsCancellationRequested)
    {
      logger.LogError(ex, "JetStream is not available. UserRegisteredPushConsumer will not run.");
      logger.LogWarning("To enable JetStream, start NATS server with -js flag");
    }
    catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
    {
      logger.LogError(ex, "Error in UserRegisteredPushConsumer");
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken = default)
  {
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource?.Dispose();
    await Task.CompletedTask;
  }
}