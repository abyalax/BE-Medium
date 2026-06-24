using Medium.Api.Infrastructure.Nats.Events;

namespace Medium.Api.Infrastructure.Nats.Handler;

public static class EventHandlerRegistration
{
  public static IServiceCollection AddEventHandler(this IServiceCollection services)
  {

    services.AddScoped<IEventHandler<ArticlePublishedEvent>, ArticlePublishedEventHandler>();
    services.AddScoped<IEventHandler<CommentCreatedEvent>, CommentCreatedEventHandler>();
    services.AddScoped<IEventHandler<UserFollowedEvent>, UserFollowedEventHandler>();

    return services;
  }
}