using MediatR;

using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Enums;

namespace Medium.Api.Domain.Notification.Commands;

public record CreateNotificationCommand(
  Guid UserId,
  string Title,
  string Message,
  NotificationType Type,
  Guid? RelatedEntityId = null,
  string? ActionUrl = null
) : IRequest<NotificationDto?>;