using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Domain.Notification.Services;
using Medium.Api.Infrastructure.Email.Models;
using Medium.Api.Infrastructure.Email.Services;
using Medium.Api.Infrastructure.Nats.Events;

namespace Medium.Api.Infrastructure.Nats.Handler;

public interface IEventHandler<in T> where T : class
{
  Task HandleAsync(T @event);
}

public class ArticlePublishedEventHandler(
    ILogger<ArticlePublishedEventHandler> logger,
    NotificationService notificationService,
    FollowQueryRepository followQueryRepository,
    MailpitEmailService emailService,
    EmailTemplateService emailTemplateService) : IEventHandler<ArticlePublishedEvent>
{
  private readonly ILogger<ArticlePublishedEventHandler> _logger = logger;
  private readonly NotificationService _notificationService = notificationService;
  private readonly FollowQueryRepository _followQueryRepository = followQueryRepository;
  private readonly MailpitEmailService _emailService = emailService;
  private readonly EmailTemplateService _emailTemplateService = emailTemplateService;

  public async Task HandleAsync(ArticlePublishedEvent @event)
  {
    try
    {
      _logger.LogInformation("Handling ArticlePublished: {ArticleId}", @event.ArticleId);

      // Fetch followers (Consider removing pagination if you need to notify ALL followers)
      var followers = await _followQueryRepository.GetFollowersAsync(Guid.Parse(@event.AuthorId), 1, 100, default);
      if (followers == null || !followers.Any())
      {
        _logger.LogInformation("No followers found for Author: {AuthorId}", @event.AuthorId);
        return;
      }

      var summary = @event.Title.Length > 150
          ? string.Concat(@event.Title.AsSpan(0, 150), "...")
          : @event.Title;
      var articleUrl = $"/articles/{@event.ArticleId}";

      // 1. Batch Create Notifications (Fix N+1 Query)
      var notificationRequests = followers.Select(follow => new CreateNotificationRequest(
          follow.FollowerId.ToString(),
          "New Article Published",
          $"{@event.Title} has been published by an author you follow",
          NotificationType.ArticlePublished,
          @event.ArticleId,
          articleUrl
      )).ToList();

      // NOTE: Ensure your NotificationService supports batch insertion (e.g., CreateRangeAsync)
      await _notificationService.CreateRangeAsync(notificationRequests, default);

      // 2. Optimize Email Sending (Parallel Execution using Task.WhenAll)
      var emailTasks = followers.Select(async follow =>
      {
        try
        {
          var emailModel = new ArticlePublishedEmailModel(
                    follow.Follower.Name,
                    follow.Following.Name,
                    @event.Title,
                    summary,
                    articleUrl
                );

          var emailHtml = await _emailTemplateService.RenderTemplateAsync("ArticlePublishedEmail", emailModel);
          await _emailService.SendAsync(follow.Follower.Email, "New Article Published", emailHtml, default);
        }
        catch (Exception ex)
        {
          // Log individual email failure so it doesn't break the entire batch execution
          _logger.LogError(ex, "Failed to send email notification to follower: {FollowerId}", follow.FollowerId);
        }
      });

      await Task.WhenAll(emailTasks);

      _logger.LogInformation("ArticlePublished event processed for {ArticleId}. Notified {Count} followers.",
          @event.ArticleId, followers.Count);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error handling ArticlePublished");
      throw;
    }
  }
}