using Coravel;

using Medium.Api.Infrastructure.Scheduler.Jobs;

namespace Medium.Api.Infrastructure.Extensions;

public static class SchedulerExtensions
{
  public static void ConfigureSchedulers(
      this IServiceProvider services)
  {
    services.UseScheduler(scheduler =>
    {
      scheduler
              .Schedule<PublishScheduledJob>()
              .DailyAtHour(9);

      scheduler
              .Schedule<NewsletterQueueJob>()
              .Weekly()
              .Monday();

      scheduler
              .Schedule<WeeklyAnalyticsJob>()
              .Weekly()
              .Sunday();
    });
  }
}