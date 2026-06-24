using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Domain.Notification.Services;
using Medium.Api.Infrastructure.Email.Models;
using Medium.Api.Infrastructure.Email.Services;
using Medium.Api.Infrastructure.Events.Events;

using Microsoft.Extensions.Logging;

namespace Medium.Api.Infrastructure.Events.Handler;

public interface IEventHandler<in T> where T : class
{
  Task HandleAsync(T @event);
}

public class ArticlePublishedEventHandler : IEventHandler<ArticlePublishedEvent>
{
  private readonly ILogger<ArticlePublishedEventHandler> _logger;
  private readonly NotificationService _notificationService;
  private readonly FollowRepository _followRepository;
  private readonly MailpitEmailService _emailService;
  private readonly EmailTemplateService _emailTemplateService;

  public ArticlePublishedEventHandler(
      ILogger<ArticlePublishedEventHandler> logger,
      NotificationService notificationService,
      FollowRepository followRepository,
      MailpitEmailService emailService,
      EmailTemplateService emailTemplateService)
  {
    _logger = logger;
    _notificationService = notificationService;
    _followRepository = followRepository;
    _emailService = emailService;
    _emailTemplateService = emailTemplateService;
  }

  public async Task HandleAsync(ArticlePublishedEvent @event)
  {
    try
    {
      _logger.LogInformation("Handling ArticlePublished: {ArticleId}", @event.ArticleId);

      // Send notification to followers via email
      var followers = await _followRepository.GetFollowersAsync(
          Guid.Parse(@event.AuthorId), 1, 100, default);

      foreach (var follow in followers)
      {
        // Create notification
        await _notificationService.CreateAsync(new CreateNotificationRequest(
            follow.FollowerId.ToString(),
            $"New Article Published",
            $"{@event.Title} has been published by an author you follow",
            NotificationType.ArticlePublished,
            @event.ArticleId,
            $"/articles/{@event.ArticleId}"
        ), default);

        // Send email notification using RazorLight template
        var emailModel = new ArticlePublishedEmailModel(
            follow.Follower.Name,
            follow.Following.Name,
            @event.Title,
            @event.Title.Length > 150 ? @event.Title.Substring(0, 150) + "..." : @event.Title,
            $"/articles/{@event.ArticleId}"
        );

        var emailHtml = await _emailTemplateService.RenderTemplateAsync("ArticlePublishedEmail", emailModel);
        await _emailService.SendAsync(follow.Follower.Email, "New Article Published", emailHtml, default);
      }

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