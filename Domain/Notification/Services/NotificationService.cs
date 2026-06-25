using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Domain.Notification.Repositories;

using NotificationModel = Medium.Api.Models.Notification;

namespace Medium.Api.Domain.Notification.Services;

public class NotificationService(NotificationRepository notificationRepository)
{
  private readonly NotificationRepository _notificationRepository = notificationRepository;

  public async Task<NotificationResponse> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
  {
    var notification = new NotificationModel
    {
      Id = Guid.NewGuid(),
      UserId = Guid.Parse(request.UserId),
      Title = request.Title,
      Message = request.Message,
      Type = (Medium.Api.Enums.NotificationType)request.Type,
      ReferenceId = request.RelatedEntityId != null ? Guid.Parse(request.RelatedEntityId) : null,
      IsRead = false,
      CreatedAt = DateTime.UtcNow
    };

    await _notificationRepository.AddAsync(notification, cancellationToken);
    await _notificationRepository.SaveChangesAsync(cancellationToken);

    return new NotificationResponse(
        notification.Id.ToString(),
        notification.UserId.ToString(),
        notification.Title,
        notification.Message,
        (NotificationType)notification.Type,
        notification.ReferenceId?.ToString(),
        request.ActionUrl,
        notification.IsRead,
notification.CreatedAt
    );
  }

  public async Task MarkAsReadAsync(string notificationId, CancellationToken cancellationToken = default)
  {
    await _notificationRepository.MarkAsReadAsync(notificationId, cancellationToken);
  }

  public async Task<List<NotificationResponse>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
  {
    var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, page, pageSize, cancellationToken);

    return notifications.Select(n => new NotificationResponse(
        n.Id.ToString(),
        n.UserId.ToString(),
        n.Title,
        n.Message,
        (NotificationType)n.Type,
        n.ReferenceId?.ToString(),
        null, // ActionUrl not stored in model
        n.IsRead,
n.CreatedAt
    )).ToList();
  }
}