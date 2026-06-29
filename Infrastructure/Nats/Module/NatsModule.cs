using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Interface;
using Medium.Api.Infrastructure.Nats.Consumers;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Handler;
using Medium.Api.Infrastructure.Nats.Services;

namespace Medium.Api.Infrastructure.Nats.Module;

public static class NatsModule
{
  public static IServiceCollection AddNatsInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    // Connection provider as singleton with lifecycle support
    services.AddSingleton<INatsConnectionProvider, NatsConnectionProvider>();
    services.AddSingleton<INatsLifecycle>(sp => sp.GetRequiredService<INatsConnectionProvider>());

    // Publishers and subscribers as scoped
    services.AddScoped<INatsPublisher, NatsPublisher>();
    services.AddScoped<IJetStreamEventPublisher, JetStreamEventPublisher>();
    services.AddScoped<INatsSubscriber, NatsSubscriber>();

    // JetStream consumers as scoped
    services.AddScoped<IJetStreamPushConsumer, JetStreamPushConsumer>();
    services.AddScoped<IJetStreamPullConsumer, JetStreamPullConsumer>();

    services.AddScoped<IEventHandler<ArticlePublishedEvent>, ArticlePublishedEventHandler>();
    services.AddScoped<IEventHandler<CommentCreatedEvent>, CommentCreatedEventHandler>();
    services.AddScoped<IEventHandler<UserFollowedEvent>, UserFollowedEventHandler>();

    // Background consumers - registered as scoped services, started by ApplicationLifecycleService
    services.AddScoped<UserRegisteredPushConsumer>();
    services.AddScoped<UserLoggedInPullConsumer>();
    services.AddScoped<EmailServiceResponder>();

    return services;
  }
}