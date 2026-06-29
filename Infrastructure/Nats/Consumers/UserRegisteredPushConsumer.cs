using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Medium.Api.Infrastructure.Nats.Consumers;

public class UserRegisteredPushConsumer(
    INatsConnectionProvider connectionProvider,
    ILogger<UserRegisteredPushConsumer> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Starting UserRegisteredPushConsumer...");
    
    // Wait for NATS connection to be ready
    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        var _ = connectionProvider.Connection;
        break;
      }
      catch (InvalidOperationException)
      {
        logger.LogInformation("Waiting for NATS connection to be initialized...");
        await Task.Delay(1000, stoppingToken);
      }
    }
    
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
            cancellationToken: stoppingToken);
      }
      catch (Exception)
      {
        consumer = await js.GetConsumerAsync("USER_EVENTS", "user-registered-push", cancellationToken: stoppingToken);
      }

      await foreach (var msg in consumer.ConsumeAsync<UserRegisteredEvent>(cancellationToken: stoppingToken))
      {
        try
        {
          logger.LogInformation("Received user registered event: {UserId} - {Email}", msg.Data?.UserId, msg.Data?.Email);
          if (msg.Data != null)
          {
            logger.LogInformation("Successfully processed user registered event for {Email}", msg.Data.Email);
          }
          await msg.AckAsync(cancellationToken: stoppingToken);
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Error processing user registered event");
          await msg.NakAsync(cancellationToken: stoppingToken);
        }
      }
    }
    catch (NatsJSException ex) when (!stoppingToken.IsCancellationRequested)
    {
      logger.LogWarning(ex, "JetStream is not available. UserRegisteredPushConsumer will not run.");
      logger.LogWarning("To enable JetStream, start NATS server with -js flag");
    }
    catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
    {
      logger.LogError(ex, "Error in UserRegisteredPushConsumer");
    }
  }
}