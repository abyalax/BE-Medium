using Medium.Api.Infrastructure.Interface;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

using Microsoft.Extensions.Logging;

using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Medium.Api.Infrastructure.Nats.Consumers;

public class UserLoggedInPullConsumer(
    INatsConnectionProvider connectionProvider,
    ILogger<UserLoggedInPullConsumer> logger) : IManuallyStartableService
{
  private CancellationTokenSource? _cancellationTokenSource;

  public async Task StartAsync(CancellationToken cancellationToken = default)
  {
    logger.LogInformation("Starting UserLoggedInPullConsumer...");
    _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

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
            cancellationToken: cancellationToken);
      }
      catch (Exception)
      {
        consumer = await js.GetConsumerAsync("USER_EVENTS", "user-logged-in-pull", cancellationToken: cancellationToken);
      }

      while (!cancellationToken.IsCancellationRequested)
      {
        try
        {
          var fetchOpts = new NatsJSFetchOpts { MaxMsgs = 10, Expires = TimeSpan.FromSeconds(5) };

          await foreach (var msg in consumer.FetchAsync<UserLoggedInEvent>(opts: fetchOpts, cancellationToken: cancellationToken))
          {
            try
            {
              logger.LogInformation("Received user logged in event: {UserId} - {Email}", msg.Data?.UserId, msg.Data?.Email);
              if (msg.Data != null)
              {
                logger.LogInformation("Successfully processed user logged in event for {Email}", msg.Data.Email);
              }
              await msg.AckAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
              logger.LogError(ex, "Error processing individual user logged in event");
              await msg.NakAsync(cancellationToken: cancellationToken);
            }
          }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
          logger.LogError(ex, "Error fetching user logged in events");
        }

        await Task.Delay(1000, cancellationToken);
      }
    }
    catch (NatsJSException ex) when (!cancellationToken.IsCancellationRequested)
    {
      logger.LogWarning(ex, "JetStream is not available. UserLoggedInPullConsumer will not run.");
      logger.LogWarning("To enable JetStream, start NATS server with -js flag");
    }
    catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
    {
      logger.LogError(ex, "Error in UserLoggedInPullConsumer");
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken = default)
  {
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource?.Dispose();
    await Task.CompletedTask;
  }
}
