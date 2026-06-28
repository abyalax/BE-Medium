using Medium.Api.Domain.Article.Events;
using Medium.Api.Infrastructure.Email.Services;
using Medium.Api.Infrastructure.Events;

using Microsoft.Extensions.Logging;

namespace Medium.Api.Domain.Article.Events.Handlers;

public class OnArticlePublishedHandler(
    MailpitEmailService emailService,
    ILogger<OnArticlePublishedHandler> logger) : IEventHandler<ArticlePublishedEvent>
{
  public async Task HandleAsync(ArticlePublishedEvent @event, CancellationToken cancellationToken = default)
  {
    logger.LogInformation("Article published: {ArticleId} - {Title}", @event.ArticleId, @event.Title);

    // Send email notification (in a real scenario, to followers)
    try
    {
      // await emailService.SendAsync(...);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to send email notification for article {ArticleId}", @event.ArticleId);
    }
  }
}