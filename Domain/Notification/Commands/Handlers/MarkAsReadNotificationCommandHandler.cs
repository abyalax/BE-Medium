using MediatR;

using Medium.Api.Domain.Notification.Repositories;

namespace Medium.Api.Domain.Notification.Commands.Handlers;

public class MarkAsReadNotificationHandler(
    NotificationStoreRepository storeRepository
  ) : IRequestHandler<MarkAsReadNotificationCommand>
{

  public async Task Handle(MarkAsReadNotificationCommand command, CancellationToken cancellationToken)
  {
    await storeRepository.MarkAsReadAsync(command.NotificationId, cancellationToken);
  }

}