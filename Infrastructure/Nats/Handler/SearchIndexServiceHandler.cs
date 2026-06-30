using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Nats.Events;

namespace Medium.Api.Infrastructure.Nats.Handler;

public class SearchIndexServiceHandler(ILogger<SearchIndexServiceHandler> logger) : IEventHandler<ArticlePublishedEvent>
{
  public async Task HandleAsync(ArticlePublishedEvent @event, CancellationToken cancellationToken = default)
  {
    try
    {
      logger.LogInformation("Search Index Service: Processing ArticlePublished event for article {ArticleId}", @event.ArticleId);

      // Simulate search indexing
      // In a real implementation, this would:
      // - Index the article content
      // - Update search indexes
      // - Add to Elasticsearch/Meilisearch
      // - Update tags and metadata

      await Task.Delay(150, cancellationToken); // Simulate indexing time

      logger.LogInformation("Search Index Service: Successfully indexed article {ArticleId} '{Title}'",
        @event.ArticleId, @event.Title);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Search Index Service: Error processing ArticlePublished event for article {ArticleId}", @event.ArticleId);
      throw;
    }
  }
}