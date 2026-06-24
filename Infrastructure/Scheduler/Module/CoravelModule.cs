using Coravel;
using Medium.Api.Infrastructure.Jobs;

namespace Medium.Api.Infrastructure.Scheduler.Module;

public static class CoravelModule
{
    public static IServiceCollection AddCoravelInfrastructure(
        this IServiceCollection services)
    {
        services.AddScheduler();
        
        // Register jobs
        services.AddScoped<PublishScheduledJob>();
        services.AddScoped<NewsletterQueueJob>();
        services.AddScoped<WeeklyAnalyticsJob>();

        return services;
    }
}
