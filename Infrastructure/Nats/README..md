```csharp
using Medium.Api.Infrastructure.Nats.Services;

namespace Medium.Api.Infrastructure.Nats;

/// <summary>
/// Extension methods for adding NATS services to the dependency injection container
/// </summary>
public static class NatsServiceCollectionExtensions
{
  public static IServiceCollection AddNatsServices(this IServiceCollection services)
  {
    // Connection provider
    services.AddSingleton<INatsConnectionProvider, NatsConnectionProvider>();

    // Publishers
    services.AddSingleton<INatsPublisher, NatsPublisher>();
    services.AddSingleton<IJetStreamEventPublisher, JetStreamEventPublisher>();

    // Subscribers
    services.AddSingleton<INatsSubscriber, NatsSubscriber>();

    // JetStream Consumers
    services.AddSingleton<IJetStreamPushConsumer, JetStreamPushConsumer>();
    services.AddSingleton<IJetStreamPullConsumer, JetStreamPullConsumer>();

    return services;
  }
}

/// <summary>
/// Usage Examples
/// </summary>
public static class NatsUsageExamples
{
  /// <summary>
  /// Example 1: Basic Pub/Sub
  /// </summary>
  public static async Task BasicPubSubExample(IServiceProvider serviceProvider)
  {
    var connectionProvider = serviceProvider.GetRequiredService<INatsConnectionProvider>();
    var publisher = serviceProvider.GetRequiredService<INatsPublisher>();
    var subscriber = serviceProvider.GetRequiredService<INatsSubscriber>();

    // Subscribe first
    var cts = new CancellationTokenSource();
    _ = subscriber.SubscribeAsync<ArticlePublishedEvent>(
      NatsSubjects.ArticlePublished,
      async @event =>
      {
        Console.WriteLine($"Received article: {@event.Title}");
        await Task.CompletedTask;
      },
      cts.Token
    );

    // Publish
    await publisher.PublishAsync(
      NatsSubjects.ArticlePublished,
      new ArticlePublishedEvent { Title = "Hello NATS", Author = "John" }
    );
  }

  /// <summary>
  /// Example 2: Request/Reply Pattern
  /// </summary>
  public static async Task RequestReplyExample(IServiceProvider serviceProvider)
  {
    var publisher = serviceProvider.GetRequiredService<INatsPublisher>();
    var subscriber = serviceProvider.GetRequiredService<INatsSubscriber>();

    // Setup responder
    var cts = new CancellationTokenSource();
    _ = subscriber.SubscribeToRequestAsync<GetUserRequest, GetUserResponse>(
      "user.get",
      async request =>
      {
        return new GetUserResponse { UserId = request.UserId, Name = "John Doe" };
      },
      cts.Token
    );

    // Send request and wait for reply
    var response = await publisher.RequestAsync<GetUserRequest, GetUserResponse>(
      "user.get",
      new GetUserRequest { UserId = 1 }
    );

    Console.WriteLine($"Got user: {response.Name}");
  }

  /// <summary>
  /// Example 3: JetStream Push Consumer
  /// </summary>
  public static async Task JetStreamPushConsumerExample(IServiceProvider serviceProvider, ILogger logger)
  {
    var connectionProvider = serviceProvider.GetRequiredService<INatsConnectionProvider>();
    var pushConsumer = serviceProvider.GetRequiredService<IJetStreamPushConsumer>();
    var jetStreamPublisher = serviceProvider.GetRequiredService<IJetStreamEventPublisher>();

    // Initialize JetStream
    await JetStreamInitializer.InitializeJetStreamAsync(connectionProvider, logger);
    await JetStreamInitializer.InitializeConsumersAsync(connectionProvider, logger);

    var cts = new CancellationTokenSource();

    // Subscribe to stream events with push consumer
    _ = pushConsumer.SubscribeAsync<ArticlePublishedEvent>(
      "MEDIUM_EVENTS",
      NatsSubjects.ArticlePublished,
      async @event =>
      {
        Console.WriteLine($"Push consumer received: {@event.Title}");
        await Task.CompletedTask;
      },
      cts.Token
    );

    // Publish to stream
    await jetStreamPublisher.PublishToStreamAsync(
      NatsSubjects.ArticlePublished,
      new ArticlePublishedEvent { Title = "Push Consumer Test", Author = "Jane" }
    );
  }

  /// <summary>
  /// Example 4: JetStream Pull Consumer
  /// </summary>
  public static async Task JetStreamPullConsumerExample(IServiceProvider serviceProvider, ILogger logger)
  {
    var connectionProvider = serviceProvider.GetRequiredService<INatsConnectionProvider>();
    var pullConsumer = serviceProvider.GetRequiredService<IJetStreamPullConsumer>();
    var jetStreamPublisher = serviceProvider.GetRequiredService<IJetStreamEventPublisher>();

    // Initialize JetStream
    await JetStreamInitializer.InitializeJetStreamAsync(connectionProvider, logger);
    await JetStreamInitializer.InitializeConsumersAsync(connectionProvider, logger);

    var cts = new CancellationTokenSource();

    // Start pull consumer (typically in background)
    _ = pullConsumer.ConsumeAsync<ArticlePublishedEvent>(
      "MEDIUM_EVENTS",
      "article_published_pull",
      batchSize: 10,
      cts.Token
    );

    // Publish messages to stream
    for (int i = 0; i < 5; i++)
    {
      await jetStreamPublisher.PublishToStreamAsync(
        NatsSubjects.ArticlePublished,
        new ArticlePublishedEvent { Title = $"Article {i}", Author = "Test" }
      );
    }
  }

  /// <summary>
  /// Startup initialization in Program.cs
  /// </summary>
  public static async Task ConfigureNatsStartup(IApplicationBuilder app)
  {
    var connectionProvider = app.ApplicationServices.GetRequiredService<INatsConnectionProvider>();
    var logger = app.ApplicationServices.GetRequiredService<ILogger<Program>>();

    // Initialize NATS connection
    await connectionProvider.InitializeAsync();

    // Initialize JetStream
    await JetStreamInitializer.InitializeJetStreamAsync(connectionProvider, logger);
    await JetStreamInitializer.InitializeConsumersAsync(connectionProvider, logger);

    // Setup graceful shutdown
    var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStopping.Register(async () =>
    {
      await connectionProvider.ShutdownAsync();
    });
  }
}

// Event Models
public class ArticlePublishedEvent
{
  public string Title { get; set; } = string.Empty;
  public string Author { get; set; } = string.Empty;
}

public class GetUserRequest
{
  public int UserId { get; set; }
}

public class GetUserResponse
{
  public int UserId { get; set; }
  public string Name { get; set; } = string.Empty;
}
```
