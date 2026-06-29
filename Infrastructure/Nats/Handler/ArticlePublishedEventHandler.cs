using MediatR;

using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Domain.Notification.Commands;
using Medium.Api.Enums;
using Medium.Api.Infrastructure.Email.Models;
using Medium.Api.Infrastructure.Email.Services;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Nats.Events;

namespace Medium.Api.Infrastructure.Nats.Handler;

public class ArticlePublishedEventHandler(
    ILogger<ArticlePublishedEventHandler> logger,
    FollowQueryRepository followQueryRepository,
    IMediator mediator,
    MailpitEmailService emailService,
    EmailTemplateService emailTemplateService) : IEventHandler<ArticlePublishedEvent>
{
  public async Task HandleAsync(ArticlePublishedEvent @event, CancellationToken cancellationToken = default)
  {
    try
    {
      logger.LogInformation("Handling NATS Event ArticlePublished: {ArticleId}", @event.ArticleId);

      var followers = await followQueryRepository.GetFollowersAsync(Guid.Parse(@event.AuthorId), 1, 100, cancellationToken);
      if (followers == null || followers.Count == 0)
      {
        logger.LogInformation("No followers found for Author: {AuthorId}", @event.AuthorId);
        return;
      }

      var summary = @event.Title.Length > 150
          ? string.Concat(@event.Title.AsSpan(0, 150), "...")
          : @event.Title;
      var articleUrl = $"/article/{@event.ArticleId}";

      var notifications = followers.Select(follow => new CreateNotificationCommand(
          follow.FollowerId,
          "New Article Published",
          $"{@event.Title} has been published by an author you follow",
          NotificationType.ArticlePublished,
          Guid.Parse(@event.ArticleId),
          articleUrl
      )).ToList();

      var bulkNotificationCommand = new CreateRangeNotificationCommand(notifications);
      await mediator.Send(bulkNotificationCommand, cancellationToken);

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

          var emailHtml = await emailTemplateService.RenderTemplateAsync("ArticlePublishedEmail", emailModel);
          await emailService.SendAsync(follow.Follower.Email, "New Article Published", emailHtml, default);
        }
        catch (Exception ex)
        {
          logger.LogError(ex, "Failed to send email notification to follower: {FollowerId}", follow.FollowerId);
        }
      });

      await Task.WhenAll(emailTasks);

      logger.LogInformation("ArticlePublished event processed for {ArticleId}. Notified {Count} followers.",
          @event.ArticleId, followers.Count);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error handling ArticlePublished inside NATS Handler");
      throw;
    }
  }
}