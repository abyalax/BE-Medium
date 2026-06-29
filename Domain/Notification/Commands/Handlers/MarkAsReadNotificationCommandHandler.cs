using MediatR;

using Medium.Api.Domain.Notification.Repositories;

namespace Medium.Api.Domain.Notification.Commands.Handlers;

public class MarkAsReadNotificationCommandHandler(
    NotificationStoreRepository storeRepository
  ) : IRequestHandler<MarkAsReadNotificationCommand>
{

  public async Task Handle(MarkAsReadNotificationCommand command, CancellationToken cancellationToken)
  {
    await storeRepository.MarkAsReadAsync(command.NotificationId, cancellationToken);
  }

}