using Medium.Api.Infrastructure.Events.Events;
using Medium.Api.Infrastructure.Events.Handler;

namespace Medium.Api.Infrastructure.Events.Module;

public static class EventModule
{
  public static IServiceCollection AddEventHandler(
      this IServiceCollection services)
  {
    services.AddScoped<IEventHandler<ArticlePublishedEvent>, ArticlePublishedEventHandler>();
    services.AddScoped<IEventHandler<CommentCreatedEvent>, CommentCreatedEventHandler>();
    services.AddScoped<IEventHandler<UserFollowedEvent>, UserFollowedEventHandler>();

    return services;
  }
}