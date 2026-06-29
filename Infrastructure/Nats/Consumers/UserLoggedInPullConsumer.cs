using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Medium.Api.Infrastructure.Nats.Consumers;

public class UserLoggedInPullConsumer(
    INatsConnectionProvider connectionProvider,
    ILogger<UserLoggedInPullConsumer> logger) : BackgroundService
{
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    logger.LogInformation("Starting UserLoggedInPullConsumer...");
    
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
            new ConsumerConfig("user-logged-in-pull")
            {
              FilterSubject = "user.logged-in",
              AckPolicy = ConsumerConfigAckPolicy.Explicit
            },
            cancellationToken: stoppingToken);
      }
      catch (Exception)
      {
        consumer = await js.GetConsumerAsync("USER_EVENTS", "user-logged-in-pull", cancellationToken: stoppingToken);
      }

      while (!stoppingToken.IsCancellationRequested)
      {
        try
        {
          var fetchOpts = new NatsJSFetchOpts { MaxMsgs = 10, Expires = TimeSpan.FromSeconds(5) };

          await foreach (var msg in consumer.FetchAsync<UserLoggedInEvent>(opts: fetchOpts, cancellationToken: stoppingToken))
          {
            try
            {
              logger.LogInformation("Received user logged in event: {UserId} - {Email}", msg.Data?.UserId, msg.Data?.Email);
              if (msg.Data != null)
              {
                logger.LogInformation("Successfully processed user logged in event for {Email}", msg.Data.Email);
              }
              await msg.AckAsync(cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
              logger.LogError(ex, "Error processing individual user logged in event");
              await msg.NakAsync(cancellationToken: stoppingToken);
            }
          }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
          logger.LogError(ex, "Error fetching user logged in events");
        }

        await Task.Delay(1000, stoppingToken);
      }
    }
    catch (NatsJSException ex) when (!stoppingToken.IsCancellationRequested)
    {
      logger.LogWarning(ex, "JetStream is not available. UserLoggedInPullConsumer will not run.");
      logger.LogWarning("To enable JetStream, start NATS server with -js flag");
    }
    catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
    {
      logger.LogError(ex, "Error in UserLoggedInPullConsumer");
    }
  }
}