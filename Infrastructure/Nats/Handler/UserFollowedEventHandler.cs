using MediatR;

using Medium.Api.Domain.Notification.Commands;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Enums;
using Medium.Api.Infrastructure.Email.Models;
using Medium.Api.Infrastructure.Email.Services;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Nats.Events;

namespace Medium.Api.Infrastructure.Nats.Handler;

public class UserFollowedEventHandler(
    ILogger<UserFollowedEventHandler> logger,
    IMediator mediator,
    UserQueryRepository userQueryRepository,
    MailpitEmailService emailService,
    EmailTemplateService emailTemplateService
) : IEventHandler<UserFollowedEvent>
{
  public async Task HandleAsync(UserFollowedEvent @event, CancellationToken cancellationToken = default)
  {
    try
    {
      logger.LogInformation("Handling UserFollowed via NATS: {FollowerId} -> {FollowingId}", @event.FollowerId, @event.FollowingId);

      var follower = await userQueryRepository.GetByIdAsync(Guid.Parse(@event.FollowerId), cancellationToken);
      var following = await userQueryRepository.GetByIdAsync(Guid.Parse(@event.FollowingId), cancellationToken);

      if (follower == null || following == null)
      {
        logger.LogWarning("User not found for follow event: {FollowerId} -> {FollowingId}", @event.FollowerId, @event.FollowingId);
        return;
      }

      var followerUrl = $"/users/{@event.FollowerId}";

      var notificationCommand = new CreateNotificationCommand(
          Guid.Parse(@event.FollowingId),
          "New Follower",
          $"{follower.Name} started following you",
          NotificationType.UserFollowed,
          Guid.Parse(@event.FollowingId),
          followerUrl
      );

      var bulkCommand = new CreateRangeNotificationCommand([notificationCommand]);
      await mediator.Send(bulkCommand, cancellationToken);

      var emailModel = new UserFollowedEmailModel(
          following.Name,
          follower.Name,
          followerUrl,
          $"/users/{@event.FollowerId}/follow"
      );

      var emailHtml = await emailTemplateService.RenderTemplateAsync("UserFollowedEmail", emailModel);
      await emailService.SendAsync(following.Email, "New Follower", emailHtml, cancellationToken);

      logger.LogInformation("UserFollowed event processed successfully: {FollowerId} -> {FollowingId}", @event.FollowerId, @event.FollowingId);
    }
    catch (OperationCanceledException)
    {
      logger.LogWarning("UserFollowed event processing was canceled gracefully.");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error handling UserFollowed inside NATS Handler");
      throw;
    }
  }
}