using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Interface;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

namespace Medium.Api.Infrastructure.Nats.Consumers;

public class ArticleServiceResponder(
  INatsSubscriber subscriber,
  ArticleQueryRepository articleQueryRepository,
  ILogger<ArticleServiceResponder> logger) : IManuallyStartableService
{
  private readonly INatsSubscriber _subscriber = subscriber;
  private readonly ArticleQueryRepository _articleQueryRepository = articleQueryRepository;
  private readonly ILogger<ArticleServiceResponder> _logger = logger;
  private CancellationTokenSource? _cancellationTokenSource;

  public async Task StartAsync(CancellationToken cancellationToken = default)
  {
    _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    var linkedCancellationToken = _cancellationTokenSource.Token;

    try
    {
      _logger.LogInformation("Starting ArticleServiceResponder for subject {Subject}", NatsSubjects.ArticleGet);

      await _subscriber.SubscribeToRequestAsync<ArticleGetRequest, ArticleGetResponse>(
        NatsSubjects.ArticleGet,
        async request =>
        {
          try
          {
            _logger.LogInformation("Received article.get request for article {ArticleId}", request.ArticleId);

            var articleId = Guid.Parse(request.ArticleId);
            var article = await _articleQueryRepository.GetArticleWithAuthorTagsAsync(articleId, linkedCancellationToken);

            if (article == null)
            {
              _logger.LogWarning("Article {ArticleId} not found", request.ArticleId);
              return new ArticleGetResponse
              {
                Error = "Article not found"
              };
            }

            _logger.LogInformation("Successfully retrieved article {ArticleId}", request.ArticleId);

            return new ArticleGetResponse
            {
              Id = article.Id.ToString(),
              Title = article.Title,
              Content = article.Content,
              AuthorId = article.AuthorId.ToString(),
              AuthorName = article.Author.Name,
              Slug = article.Slug,
              Status = article.Status.ToString(),
              PublishedAt = article.PublishedAt,
              CreatedAt = article.CreatedAt,
              Error = null
            };
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "Error processing article.get request for article {ArticleId}", request.ArticleId);
            return new ArticleGetResponse
            {
              Error = $"Error: {ex.Message}"
            };
          }
        },
        linkedCancellationToken
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "ArticleServiceResponder failed");
      throw;
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Stopping ArticleServiceResponder");
    _cancellationTokenSource?.Cancel();
    await Task.CompletedTask;
  }
}