using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Domain.Notification.Services;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Email.Models;
using Medium.Api.Infrastructure.Email.Services;
using Medium.Api.Infrastructure.Events.Events;

using Microsoft.Extensions.Logging;

namespace Medium.Api.Infrastructure.Events.Handler;

public class UserFollowedEventHandler : IEventHandler<UserFollowedEvent>
{
  private readonly ILogger<UserFollowedEventHandler> _logger;
  private readonly NotificationService _notificationService;
  private readonly UserRepository _userRepository;
  private readonly MailpitEmailService _emailService;
  private readonly EmailTemplateService _emailTemplateService;

  public UserFollowedEventHandler(
      ILogger<UserFollowedEventHandler> logger,
      NotificationService notificationService,
      UserRepository userRepository,
      MailpitEmailService emailService,
      EmailTemplateService emailTemplateService)
  {
    _logger = logger;
    _notificationService = notificationService;
    _userRepository = userRepository;
    _emailService = emailService;
    _emailTemplateService = emailTemplateService;
  }

  public async Task HandleAsync(UserFollowedEvent @event)
  {
    try
    {
      _logger.LogInformation("Handling UserFollowed: {FollowerId} -> {FollowingId}", @event.FollowerId, @event.FollowingId);

      // Get users
      var follower = await _userRepository.GetByIdAsync(Guid.Parse(@event.FollowerId), default);
      var following = await _userRepository.GetByIdAsync(Guid.Parse(@event.FollowingId), default);

      if (follower == null || following == null)
      {
        _logger.LogWarning("User not found for follow event: {FollowerId} -> {FollowingId}", @event.FollowerId, @event.FollowingId);
        return;
      }

      // Send notification to the user being followed
      await _notificationService.CreateAsync(new CreateNotificationRequest(
          @event.FollowingId,
          "New Follower",
          $"{follower.Name} started following you",
          NotificationType.UserFollowed,
          @event.FollowerId,
          $"/users/{@event.FollowerId}"
      ), default);

      // Send email notification using RazorLight template
      var emailModel = new UserFollowedEmailModel(
          following.Name,
          follower.Name,
          $"/users/{@event.FollowerId}",
          $"/users/{@event.FollowerId}/follow"
      );

      var emailHtml = await _emailTemplateService.RenderTemplateAsync("UserFollowedEmail", emailModel);
      await _emailService.SendAsync(following.Email, "New Follower", emailHtml, default);

      _logger.LogInformation("UserFollowed event processed: {FollowerId} -> {FollowingId}", @event.FollowerId, @event.FollowingId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error handling UserFollowed");
      throw;
    }
  }
}