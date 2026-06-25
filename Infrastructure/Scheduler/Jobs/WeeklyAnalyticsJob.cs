using Coravel.Invocable;

using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Domain.Notification.Services;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Email.Models;
using Medium.Api.Infrastructure.Email.Services;

namespace Medium.Api.Infrastructure.Scheduler.Jobs;

public class WeeklyAnalyticsJob(
    FollowRepository followRepository,
    ArticleRepository articleRepository,
    CommentRepository commentRepository,
    UserRepository userRepository,
    NotificationService notificationService,
    MailpitEmailService emailService,
    EmailTemplateService emailTemplateService,
    ILogger<WeeklyAnalyticsJob> logger) : IInvocable
{
  private readonly FollowRepository _followRepository = followRepository;
  private readonly ArticleRepository _articleRepository = articleRepository;
  private readonly CommentRepository _commentRepository = commentRepository;
  private readonly UserRepository _userRepository = userRepository;
  private readonly NotificationService _notificationService = notificationService;
  private readonly MailpitEmailService _emailService = emailService;
  private readonly EmailTemplateService _emailTemplateService = emailTemplateService;
  private readonly ILogger<WeeklyAnalyticsJob> _logger = logger;

  public async Task Invoke()
  {
    try
    {
      _logger.LogInformation("WeeklyAnalyticsJob running at {Time}", DateTime.UtcNow);

      // Calculate weekly statistics
      var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

      // Get total user count (approximate by counting follows)
      var totalFollows = await _followRepository.GetTotalFollowsCountAsync(default);

      // Get new articles this week
      var newArticlesCount = await _articleRepository.GetArticlesCountSinceAsync(oneWeekAgo, default);

      // Get new comments this week
      var newCommentsCount = await _commentRepository.GetCommentsCountSinceAsync(oneWeekAgo, default);

      var weeklyStats = new Dictionary<string, object>
      {
        ["totalFollows"] = totalFollows,
        ["newArticles"] = newArticlesCount,
        ["newComments"] = newCommentsCount,
        ["period"] = "week",
        ["calculatedAt"] = DateTime.UtcNow
      };

      _logger.LogInformation("WeeklyAnalyticsJob completed. Stats: {@Stats}", weeklyStats);

      // Save analytics as notification for admins (in real app, would save to analytics table)
      var adminUsers = await _userRepository.ListAsync(1, 10, default);
      foreach (var admin in adminUsers)
      {
        await _notificationService.CreateAsync(new CreateNotificationRequest(
            admin.Id.ToString(),
            "Weekly Analytics Report",
            $"This week: {newArticlesCount} new articles, {newCommentsCount} new comments, {totalFollows} total follows",
            NotificationType.ArticlePublished, // Reusing existing type
            null,
            "/analytics"
        ), default);
      }

      // Send analytics report to admins via email
      var weekStart = oneWeekAgo.ToString("MMM dd, yyyy");
      var weekEnd = DateTime.UtcNow.ToString("MMM dd, yyyy");

      var topArticles = await _articleRepository.GetPublishedAsync(1, 5, default);
      var topArticleItems = new List<NewsletterArticleItem>();

      foreach (var article in topArticles)
      {
        topArticleItems.Add(new NewsletterArticleItem(
            article.Title,
            article.Author.Name,
            $"/articles/{article.Id}"
        ));
      }

      foreach (var admin in adminUsers)
      {
        var emailModel = new NewsletterEmailModel(
            weekStart,
            weekEnd,
            newArticlesCount,
            newCommentsCount,
            totalFollows,
            topArticleItems,
            "https://medium-clone.com"
        );

        var emailHtml = await _emailTemplateService.RenderTemplateAsync("NewsletterEmail", emailModel);
        await _emailService.SendAsync(admin.Email, "Weekly Analytics Report", emailHtml, default);

        _logger.LogInformation("Analytics report sent to admin {AdminEmail}", admin.Email);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error in WeeklyAnalyticsJob");
      throw;
    }
  }
}