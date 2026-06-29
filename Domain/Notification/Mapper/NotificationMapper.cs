using Medium.Api.Domain.Notification.Dtos;

using NotificationModel = Medium.Api.Models.Notification;

namespace Medium.Api.Domain.Notification.Mapper;

public static class NotificationMapper
{
  public static NotificationDto ToResponse(NotificationModel notification)
  {
    return new NotificationDto(
      notification.Id,
      notification.UserId,
      notification.Title,
      notification.Message,
      notification.Type,
      null,
      null,
      notification.IsRead,
      notification.CreatedAt
    );
  }
}