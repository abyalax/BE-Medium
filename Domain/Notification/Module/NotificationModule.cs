
using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Domain.Notification.Repositories;
using Medium.Api.Domain.Notification.Services;

namespace Medium.Api.Domain.Notification.Module;

public static class NotificationModule
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services)
    {
        services.AddScoped<NotificationRepository>();
        services.AddScoped<NotificationService>();

        return services;
    }
}