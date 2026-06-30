using Medium.Api.Infrastructure.Nats.Events;

namespace Medium.Api.Infrastructure.Nats.Services;

public interface IArticleGatewayService
{
  Task<ArticleGetResponse?> GetArticleAsync(string articleId, CancellationToken cancellationToken = default);
}

public class ArticleGatewayService(
  INatsPublisher natsPublisher,
  ILogger<ArticleGatewayService> logger) : IArticleGatewayService
{
  private readonly INatsPublisher _natsPublisher = natsPublisher;
  private readonly ILogger<ArticleGatewayService> _logger = logger;

  public async Task<ArticleGetResponse?> GetArticleAsync(string articleId, CancellationToken cancellationToken = default)
  {
    try
    {
      _logger.LogInformation("API Gateway: Requesting article {ArticleId} via NATS", articleId);

      var request = new ArticleGetRequest { ArticleId = articleId };

      // Use timeout for the request-reply pattern
      using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
      using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
        cancellationToken,
        timeoutCts.Token
      );

      var response = await _natsPublisher.RequestAsync<ArticleGetRequest, ArticleGetResponse>(
        NatsSubjects.ArticleGet,
        request,
        linkedCts.Token
      );

      if (response == null)
      {
        _logger.LogWarning("API Gateway: No response received for article {ArticleId}", articleId);
        return null;
      }

      if (!string.IsNullOrEmpty(response.Error))
      {
        _logger.LogWarning("API Gateway: Error response for article {ArticleId}: {Error}", articleId, response.Error);
        return response;
      }

      _logger.LogInformation("API Gateway: Successfully received article {ArticleId}", articleId);
      return response;
    }
    catch (OperationCanceledException)
    {
      _logger.LogWarning("API Gateway: Request timeout for article {ArticleId}", articleId);
      return new ArticleGetResponse
      {
        Error = "request timeout"
      };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "API Gateway: Error requesting article {ArticleId}", articleId);
      return new ArticleGetResponse
      {
        Error = $"Error: {ex.Message}"
      };
    }
  }
}
