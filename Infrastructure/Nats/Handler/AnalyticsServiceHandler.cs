using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Nats.Events;

namespace Medium.Api.Infrastructure.Nats.Handler;

public class AnalyticsServiceHandler(ILogger<AnalyticsServiceHandler> logger) : IEventHandler<ArticlePublishedEvent>
{
  public async Task HandleAsync(ArticlePublishedEvent @event, CancellationToken cancellationToken = default)
  {
    try
    {
      logger.LogInformation("Analytics Service: Processing ArticlePublished event for article {ArticleId}", @event.ArticleId);

      // Simulate analytics processing
      // In a real implementation, this would:
      // - Increment view counters
      // - Track publication metrics
      // - Update author statistics
      // - Record engagement data

      await Task.Delay(100, cancellationToken); // Simulate processing time

      logger.LogInformation("Analytics Service: Successfully processed article {ArticleId} by author {AuthorId}",
        @event.ArticleId, @event.AuthorId);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Analytics Service: Error processing ArticlePublished event for article {ArticleId}", @event.ArticleId);
      throw;
    }
  }
}