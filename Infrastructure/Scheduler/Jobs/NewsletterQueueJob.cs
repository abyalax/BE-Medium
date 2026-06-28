using Coravel.Invocable;

using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Email.Models;
using Medium.Api.Infrastructure.Email.Services;
using Medium.Api.Infrastructure.Nats.Services;

namespace Medium.Api.Infrastructure.Scheduler.Jobs;

public class NewsletterQueueJob(
    CommentQueryRepository commentQueryRepository,
    ArticleQueryRepository articleQueryRepository,
    FollowQueryRepository followQueryRepository,
    UserQueryRepository userQueryRepository,
    INatsPublisher publisher,
    MailpitEmailService emailService,
    EmailTemplateService emailTemplateService,
    ILogger<NewsletterQueueJob> logger) : IInvocable
{
  private readonly CommentQueryRepository _commentQueryRepository = commentQueryRepository;
  private readonly ArticleQueryRepository _articleQueryRepository = articleQueryRepository;
  private readonly FollowQueryRepository _followQueryRepository = followQueryRepository;
  private readonly UserQueryRepository _userQueryRepository = userQueryRepository;
  private readonly INatsPublisher _publisher = publisher;
  private readonly MailpitEmailService _emailService = emailService;
  private readonly EmailTemplateService _emailTemplateService = emailTemplateService;
  private readonly ILogger<NewsletterQueueJob> _logger = logger;

  public async Task Invoke()
  {
    try
    {
      _logger.LogInformation("NewsletterQueueJob running at {Time}", DateTime.UtcNow);

      // Calculate week date range
      var now = DateTime.UtcNow;
      var weekStart = now.AddDays(-(int)now.DayOfWeek);
      var weekEnd = weekStart.AddDays(6);

      // Get weekly statistics
      var newArticlesCount = await _articleQueryRepository.GetArticlesCountSinceAsync(weekStart, default);
      var newCommentsCount = await _commentQueryRepository.GetCommentsCountSinceAsync(weekStart, default);
      var newFollowersCount = await _followQueryRepository.GetTotalFollowsCountAsync(default);

      // Get top articles this week
      var topArticles = await _articleQueryRepository.GetPublishedAsync(1, 5, default);
      var topArticleItems = new List<NewsletterArticleItem>();

      foreach (var article in topArticles)
      {
        topArticleItems.Add(new NewsletterArticleItem(
            article.Title,
            article.Author.Name,
            $"/articles/{article.Id}"
        ));
      }

      // Get all users for newsletter (TODO: would only get subscribers)
      var users = await _userQueryRepository.ListAsync(1, 100, default);

      foreach (var user in users)
      {
        // Create newsletter email model
        var emailModel = new NewsletterEmailModel(
            weekStart.ToString("MMM dd, yyyy"),
            weekEnd.ToString("MMM dd, yyyy"),
            newArticlesCount,
            newCommentsCount,
            newFollowersCount,
            topArticleItems,
            "https://medium-clone.com" // TODO: change to real url apps
        );

        var emailHtml = await _emailTemplateService.RenderTemplateAsync("NewsletterEmail", emailModel);
        await _emailService.SendAsync(user.Email, "Weekly Newsletter", emailHtml, default);

        _logger.LogInformation("Newsletter sent to {UserEmail}", user.Email);
      }

      _logger.LogInformation("NewsletterQueueJob completed. Sent {Count} newsletters", users.Count);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error in NewsletterQueueJob");
      throw;
    }
  }
}