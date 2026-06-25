using Coravel.Invocable;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Domain.Article.Services;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

namespace Medium.Api.Infrastructure.Scheduler.Jobs;

public class PublishScheduledJob(
    ArticleRepository articleRepository,
    ArticleService articleService,
    INatsPublisher publisher,
    ILogger<PublishScheduledJob> logger) : IInvocable
{
  private readonly ArticleRepository _articleRepository = articleRepository;
  private readonly ArticleService _articleService = articleService;
  private readonly INatsPublisher _publisher = publisher;
  private readonly ILogger<PublishScheduledJob> _logger = logger;

  public async Task Invoke()
  {
    try
    {
      _logger.LogInformation("PublishScheduledJob running at {Time}", DateTime.UtcNow);

      // Get articles that are scheduled for publishing and whose scheduled time has passed
      var scheduledArticles = await _articleRepository.GetScheduledArticlesToPublishAsync(default);

      foreach (var article in scheduledArticles)
      {
        // Publish the article
        await _articleService.PublishAsync(
            article.Id,
            article.AuthorId,
            false, // Not admin
            new PublishArticleRequest(null), // No specific scheduled time
            default
        );

        // Publish event to NATS
        var @event = new ArticlePublishedEvent(
            article.Id.ToString(),
            article.Title,
            article.AuthorId.ToString(),
            DateTime.UtcNow
        );

        await _publisher.PublishAsync(NatsSubjects.ArticlePublished, @event);

        _logger.LogInformation("Published scheduled article {ArticleId}", article.Id);
      }

      _logger.LogInformation("PublishScheduledJob completed. Published {Count} articles", scheduledArticles.Count);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error in PublishScheduledJob");
      throw;
    }
  }
}