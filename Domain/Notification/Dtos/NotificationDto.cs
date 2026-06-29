using Medium.Api.Enums;

namespace Medium.Api.Domain.Notification.Dtos;

public record NotificationDto(
  Guid Id,
  Guid UserId,
  string Title,
  string Message,
  NotificationType Type,
  Guid? RelatedEntityId,
  string? ActionUrl,
  bool IsRead,
  DateTime CreatedAt
);

public record UpdateNotificationRequest(
    bool IsRead
);