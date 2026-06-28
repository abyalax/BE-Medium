using Microsoft.Extensions.DependencyInjection;

namespace Medium.Api.Infrastructure.Events;

public static class EventHandlerExtensions
{
  public static IServiceCollection AddEventHandlers(this IServiceCollection services)
  {
    services.AddSingleton<IEventHandlerResolver, ReflectionEventHandlerResolver>();
    return services;
  }
}