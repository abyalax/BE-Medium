using MediatR;

using Medium.Api.Domain.Notification.Dtos;

namespace Medium.Api.Domain.Notification.Commands;

public record CreateRangeNotificationCommand(
  IEnumerable<CreateNotificationCommand> Notifications
) : IRequest<IReadOnlyCollection<NotificationDto>?>;