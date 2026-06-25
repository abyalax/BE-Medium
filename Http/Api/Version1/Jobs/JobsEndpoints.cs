using Coravel;

using Medium.Api.Infrastructure.Scheduler.Jobs;

using Microsoft.AspNetCore.Authorization;

namespace Medium.Api.Http.Api.Version1.Jobs;

public static class JobsEndpoints
{
  public static void MapJobsEndpoints(this IEndpointRouteBuilder app)
  {
    var group = app.MapGroup("/api/jobs")
        .WithTags("Jobs")
        .RequireAuthorization();

    group.MapPost("/publish-scheduled", async (
        PublishScheduledJob job,
        CancellationToken cancellationToken) =>
    {
      await job.Invoke();
      return Results.Ok(new { message = "PublishScheduledJob executed successfully" });
    })
    .WithName("TriggerPublishScheduledJob")
    .WithOpenApi();

    group.MapPost("/newsletter-dispatch", async (
        NewsletterQueueJob job,
        CancellationToken cancellationToken) =>
    {
      await job.Invoke();
      return Results.Ok(new { message = "NewsletterQueueJob executed successfully" });
    })
    .WithName("TriggerNewsletterDispatchJob")
    .WithOpenApi();

    group.MapPost("/weekly-analytics", async (
        WeeklyAnalyticsJob job,
        CancellationToken cancellationToken) =>
    {
      await job.Invoke();
      return Results.Ok(new { message = "WeeklyAnalyticsJob executed successfully" });
    })
    .WithName("TriggerWeeklyAnalyticsJob")
    .WithOpenApi();
  }
}