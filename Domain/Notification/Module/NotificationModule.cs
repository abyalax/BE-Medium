
using Medium.Api.Domain.Notification.Repositories;
namespace Medium.Api.Domain.Notification.Module;

public static class NotificationModule
{
  public static IServiceCollection AddNotificationModule(this IServiceCollection services)
  {
    services.AddScoped<NotificationQueryRepository>();
    services.AddScoped<NotificationStoreRepository>();

    return services;
  }
}