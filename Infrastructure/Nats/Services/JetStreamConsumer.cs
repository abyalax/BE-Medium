using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Medium.Api.Infrastructure.Nats.Services;

public interface IJetStreamPushConsumer
{
  Task SubscribeAsync<T>(string stream, string subject, Func<T, Task> handler, string? durable = null, CancellationToken cancellationToken = default) where T : class;
}

public class JetStreamPushConsumer(INatsConnectionProvider connectionProvider, ILogger<JetStreamPushConsumer> logger) : IJetStreamPushConsumer
{
  public async Task SubscribeAsync<T>(string stream, string subject, Func<T, Task> handler, string? durable = null, CancellationToken cancellationToken = default) where T : class
  {
    try
    {
      var js = new NatsJSContext(connectionProvider.Connection);
      var durableName = durable ?? $"push_{Guid.NewGuid().ToString("N")[..8]}";

      var consumerConfig = new ConsumerConfig { DurableName = durableName };
      await js.CreateOrUpdateConsumerAsync(stream, consumerConfig, cancellationToken: cancellationToken);

      var consumer = await js.GetConsumerAsync(stream, durableName, cancellationToken: cancellationToken);
      await foreach (var msg in consumer.ConsumeAsync<T>(cancellationToken: cancellationToken))
      {
        try
        {
          if (msg.Data != null)
          {
            await handler(msg.Data);
            await msg.AckAsync(cancellationToken: cancellationToken);
            logger.LogInformation("Push consumer processed message from {Subject}", subject);
          }
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Error processing push consumer message from {Subject}", subject);
          await msg.NakAsync(cancellationToken: cancellationToken);
        }
      }
    }
    catch (OperationCanceledException)
    {
      logger.LogInformation("Push consumer subscription to {Subject} cancelled", subject);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error in push consumer for {Subject}", subject);
    }
  }
}

public interface IJetStreamPullConsumer
{
  Task ConsumeAsync<T>(string stream, string consumerName, Func<T, Task> handler, int batchSize = 10, CancellationToken cancellationToken = default) where T : class;
}

public class JetStreamPullConsumer(INatsConnectionProvider connectionProvider, ILogger<JetStreamPullConsumer> logger) : IJetStreamPullConsumer
{
  public async Task ConsumeAsync<T>(string stream, string consumerName, Func<T, Task> handler, int batchSize = 10, CancellationToken cancellationToken = default) where T : class
  {
    try
    {
      var js = new NatsJSContext(connectionProvider.Connection);

      var consumerConfig = new ConsumerConfig
      {
        DurableName = consumerName,
        AckPolicy = ConsumerConfigAckPolicy.Explicit,
        AckWait = TimeSpan.FromSeconds(30),
        MaxDeliver = 5
      };

      await js.CreateOrUpdateConsumerAsync(stream, consumerConfig, cancellationToken: cancellationToken);
      var consumer = await js.GetConsumerAsync(stream, consumerName, cancellationToken: cancellationToken);

      while (!cancellationToken.IsCancellationRequested)
      {
        try
        {
          var fetchOpts = new NatsJSFetchOpts { MaxMsgs = batchSize, Expires = TimeSpan.FromSeconds(5) };

          int processedCount = 0;
          await foreach (var msg in consumer.FetchAsync<T>(opts: fetchOpts, cancellationToken: cancellationToken))
          {
            try
            {
              if (msg.Data != null)
              {
                await handler(msg.Data);
                processedCount++;
              }
              await msg.AckAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
              logger.LogError(ex, "Error processing pull consumer message from {Stream}", stream);
              await msg.NakAsync(cancellationToken: cancellationToken);
            }
          }

          if (processedCount == 0)
          {
            await Task.Delay(1000, cancellationToken);
          }
        }
        catch (NatsJSException ex) when (ex.Message.Contains("409"))
        {
          logger.LogWarning("Consumer busy, retrying pull consumer for {ConsumerName}", consumerName);
          await Task.Delay(1000, cancellationToken);
        }
      }
    }
    catch (OperationCanceledException)
    {
      logger.LogInformation("Pull consumer for {ConsumerName} cancelled", consumerName);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error in pull consumer for {ConsumerName}", consumerName);
    }
  }
}