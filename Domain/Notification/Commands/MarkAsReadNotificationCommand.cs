using MediatR;

namespace Medium.Api.Domain.Notification.Commands;

public record MarkAsReadNotificationCommand(
  string NotificationId
) : IRequest;