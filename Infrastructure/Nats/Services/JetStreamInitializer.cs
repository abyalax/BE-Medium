using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Medium.Api.Infrastructure.Nats.Services;

public static class JetStreamInitializer
{
  public static async Task InitializeJetStreamAsync(INatsConnectionProvider connectionProvider, ILogger logger, CancellationToken cancellationToken = default)
  {
    try
    {
      var js = new NatsJSContext(connectionProvider.Connection);

      // Check if JetStream is available
      try
      {
        var accountInfo = await js.GetAccountInfoAsync(cancellationToken: cancellationToken);
        logger.LogInformation("JetStream is enabled. Streams: {StreamCount}, Consumers: {ConsumerCount}",
          accountInfo.Streams, accountInfo.Consumers);
      }
      catch (NatsJSException ex)
      {
        logger.LogWarning(ex, "JetStream is not enabled on NATS server. Skipping JetStream initialization.");
        logger.LogWarning("To enable JetStream, run NATS server with: -js or -js flag");
        return;
      }

      // Initialize MEDIUM_EVENTS stream
      var mediumSubjects = new[]
      {
        NatsSubjects.ArticlePublished,
        NatsSubjects.CommentCreated,
        NatsSubjects.UserFollowed
      };

      var mediumConfig = new StreamConfig("MEDIUM_EVENTS", mediumSubjects)
      {
        Storage = StreamConfigStorage.File,
        Retention = StreamConfigRetention.Limits,
        MaxAge = TimeSpan.FromDays(7),
        Discard = StreamConfigDiscard.Old,
        NumReplicas = 1
      };

      try
      {
        var existingStream = await js.GetStreamAsync("MEDIUM_EVENTS", cancellationToken: cancellationToken);
        logger.LogInformation("JetStream stream already exists: {StreamName}", existingStream.Info.Config.Name);
      }
      catch (NatsJSException)
      {
        var stream = await js.CreateStreamAsync(mediumConfig, cancellationToken: cancellationToken);
        logger.LogInformation("JetStream stream created: {StreamName}", stream.Info.Config.Name);
      }

      // Initialize USER_EVENTS stream
      var userSubjects = new[]
      {
        "user.registered",
        "user.logged-in"
      };

      var userConfig = new StreamConfig("USER_EVENTS", userSubjects)
      {
        Storage = StreamConfigStorage.File,
        Retention = StreamConfigRetention.Limits,
        MaxAge = TimeSpan.FromDays(7),
        Discard = StreamConfigDiscard.Old,
        NumReplicas = 1
      };

      try
      {
        var existingUserStream = await js.GetStreamAsync("USER_EVENTS", cancellationToken: cancellationToken);
        logger.LogInformation("JetStream stream already exists: {StreamName}", existingUserStream.Info.Config.Name);
      }
      catch (NatsJSException)
      {
        var userStream = await js.CreateStreamAsync(userConfig, cancellationToken: cancellationToken);
        logger.LogInformation("JetStream stream created: {StreamName}", userStream.Info.Config.Name);
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "JetStream initialization error: {Message}", ex.Message);
      throw;
    }
  }

  public static async Task InitializeConsumersAsync(INatsConnectionProvider connectionProvider, ILogger logger, CancellationToken cancellationToken = default)
  {
    try
    {
      var js = new NatsJSContext(connectionProvider.Connection);

      // Check if JetStream is available
      try
      {
        var accountInfo = await js.GetAccountInfoAsync(cancellationToken: cancellationToken);
        logger.LogInformation("JetStream is enabled. Initializing consumers...");
      }
      catch (NatsJSException ex)
      {
        logger.LogWarning(ex, "JetStream is not enabled on NATS server. Skipping consumer initialization.");
        return;
      }

      // Initialize consumers for MEDIUM_EVENTS stream
      var mediumStreamName = "MEDIUM_EVENTS";

      var mediumPushConsumerConfig = new ConsumerConfig
      {
        DurableName = "article_published_push"
      };

      var mediumPullConsumerConfig = new ConsumerConfig
      {
        DurableName = "article_published_pull",
        AckPolicy = ConsumerConfigAckPolicy.Explicit,
        AckWait = TimeSpan.FromSeconds(30),
        MaxDeliver = 5
      };

      try
      {
        await js.CreateOrUpdateConsumerAsync(mediumStreamName, mediumPushConsumerConfig, cancellationToken: cancellationToken);
        logger.LogInformation("MEDIUM_EVENTS push consumer initialized");
      }
      catch (Exception ex)
      {
        logger.LogWarning(ex, "MEDIUM_EVENTS push consumer already exists or error: {Message}", ex.Message);
      }

      try
      {
        await js.CreateOrUpdateConsumerAsync(mediumStreamName, mediumPullConsumerConfig, cancellationToken: cancellationToken);
        logger.LogInformation("MEDIUM_EVENTS pull consumer initialized");
      }
      catch (Exception ex)
      {
        logger.LogWarning(ex, "MEDIUM_EVENTS pull consumer already exists or error: {Message}", ex.Message);
      }

      // Initialize consumers for USER_EVENTS stream
      var userStreamName = "USER_EVENTS";

      var userRegisteredPushConfig = new ConsumerConfig("user-registered-push")
      {
        FilterSubject = "user.registered",
        AckPolicy = ConsumerConfigAckPolicy.Explicit
      };

      var userLoggedInPullConfig = new ConsumerConfig("user-logged-in-pull")
      {
        FilterSubject = "user.logged-in",
        AckPolicy = ConsumerConfigAckPolicy.Explicit,
        AckWait = TimeSpan.FromSeconds(30),
        MaxDeliver = 5
      };

      try
      {
        await js.CreateOrUpdateConsumerAsync(userStreamName, userRegisteredPushConfig, cancellationToken: cancellationToken);
        logger.LogInformation("USER_EVENTS user-registered-push consumer initialized");
      }
      catch (Exception ex)
      {
        logger.LogWarning(ex, "USER_EVENTS user-registered-push consumer already exists or error: {Message}", ex.Message);
      }

      try
      {
        await js.CreateOrUpdateConsumerAsync(userStreamName, userLoggedInPullConfig, cancellationToken: cancellationToken);
        logger.LogInformation("USER_EVENTS user-logged-in-pull consumer initialized");
      }
      catch (Exception ex)
      {
        logger.LogWarning(ex, "USER_EVENTS user-logged-in-pull consumer already exists or error: {Message}", ex.Message);
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Consumer initialization error: {Message}", ex.Message);
      throw;
    }
  }
}