using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.AI;
using Medium.Api.Infrastructure.Interface;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

namespace Medium.Api.Infrastructure.Nats.Consumers;

public class AiSummarizationWorker(
  IJetStreamPullConsumer pullConsumer,
  ArticleQueryRepository articleQueryRepository,
  ArticleStoreRepository articleStoreRepository,
  IOnnxAISummarizationService aiSummarizationService,
  ILogger<AiSummarizationWorker> logger) : IManuallyStartableService
{
  private readonly IJetStreamPullConsumer _pullConsumer = pullConsumer;
  private readonly ArticleQueryRepository _articleQueryRepository = articleQueryRepository;
  private readonly ArticleStoreRepository _articleStoreRepository = articleStoreRepository;
  private readonly IOnnxAISummarizationService _aiSummarizationService = aiSummarizationService;
  private readonly ILogger<AiSummarizationWorker> _logger = logger;
  private CancellationTokenSource? _cancellationTokenSource;

  public async Task StartAsync(CancellationToken cancellationToken = default)
  {
    _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    var linkedCancellationToken = _cancellationTokenSource.Token;

    try
    {
      _logger.LogInformation("Starting AI Summarization Worker for ARTICLES stream");

      await _pullConsumer.ConsumeAsync<ArticleCreatedEvent>(
        "ARTICLES",
        "ai-summarization-pull",
        async @event =>
        {
          try
          {
            _logger.LogInformation("AI Worker: Processing article {ArticleId} for summarization", @event.ArticleId);

            var articleId = Guid.Parse(@event.ArticleId);
            var article = await _articleQueryRepository.GetByIdAsync(articleId, linkedCancellationToken);

            if (article == null)
            {
              _logger.LogWarning("AI Worker: Article {ArticleId} not found, skipping summarization", @event.ArticleId);
              return;
            }

            // Generate AI summary using ONNX model
            var summary = await _aiSummarizationService.GenerateSummaryAsync(@event.Content, linkedCancellationToken);

            // Update article with summary - need to get the actual entity
            var articleEntity = await _articleStoreRepository.GetByIdAsync(articleId, linkedCancellationToken);
            if (articleEntity != null)
            {
              articleEntity.Summary = summary;
              await _articleStoreRepository.SaveChangesAsync(linkedCancellationToken);
            }

            _logger.LogInformation("AI Worker: Successfully generated summary for article {ArticleId}", @event.ArticleId);
          }
          catch (Exception ex)
          {
            _logger.LogError(ex, "AI Worker: Error processing article {ArticleId}", @event.ArticleId);
            throw; // Will trigger Nak (negative acknowledgement) for retry
          }
        },
        batchSize: 5,
        linkedCancellationToken
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "AI Summarization Worker failed");
      throw;
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Stopping AI Summarization Worker");
    _cancellationTokenSource?.Cancel();
    await Task.CompletedTask;
  }
}
