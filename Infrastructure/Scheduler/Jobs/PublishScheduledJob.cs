using Coravel.Invocable;

using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

namespace Medium.Api.Infrastructure.Scheduler.Jobs;

public class PublishScheduledJob(
    ArticleQueryRepository articleQueryRepository,
    ArticleStoreRepository articleStoreRepository,
    IJetStreamEventPublisher jetStreamPublisher,
    IHostApplicationLifetime appLifetime,
    ILogger<PublishScheduledJob> logger) : IInvocable
{
  public async Task Invoke()
  {
    // Fetch the application cancellation token to support graceful shutdowns
    var cancellationToken = appLifetime.ApplicationStopping;
    try
    {
      logger.LogInformation("PublishScheduledJob running at {Time}", DateTime.UtcNow);

      // Get articles that are scheduled for publishing and whose scheduled time has passed
      var scheduledArticles = await articleQueryRepository.GetScheduledArticlesToPublishAsync(cancellationToken);
      if (scheduledArticles == null || !scheduledArticles.Any())
      {
        logger.LogInformation("No scheduled articles found to be published.");
        return;
      }

      foreach (var article in scheduledArticles)
      {
        // Ensure the loop stops immediately if a cancellation has been requested
        cancellationToken.ThrowIfCancellationRequested();
        // Update article status directly via repository
        article.Status = Enums.ArticleStatus.Published;
        article.PublishedAt = DateTime.UtcNow;
        await articleStoreRepository.SaveChangesAsync(cancellationToken);
        // Build event payload matching the standard format
        var @event = new ArticlePublishedEvent(
            article.Id.ToString(),
            article.AuthorId.ToString(),
            article.Title,
            DateTime.UtcNow
        );
        // Publish event to NATS JetStream instead of standard core NATS publisher
        await jetStreamPublisher.PublishToStreamAsync(NatsSubjects.ArticlePublished, @event, cancellationToken);

        logger.LogInformation("Successfully published scheduled article {ArticleId}", article.Id);
      }
      logger.LogInformation("PublishScheduledJob completed. Published {Count} articles", scheduledArticles.Count);
    }
    catch (OperationCanceledException)
    {
      logger.LogWarning("PublishScheduledJob execution was canceled gracefully due to application shutdown.");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error occurred inside PublishScheduledJob");
      throw;
    }
  }
}