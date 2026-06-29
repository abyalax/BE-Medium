using MediatR;

using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Domain.Notification.Repositories;

using NotificationModel = Medium.Api.Models.Notification;

namespace Medium.Api.Domain.Notification.Commands.Handlers;

public class CreateNotificationCommandHandler(
    NotificationStoreRepository storeRepository
  ) : IRequestHandler<CreateNotificationCommand, NotificationDto?>
{

  public async Task<NotificationDto?> Handle(CreateNotificationCommand command, CancellationToken cancellationToken)
  {
    var notification = new NotificationModel
    {
      Id = Guid.NewGuid(),
      UserId = command.UserId,
      Title = command.Title,
      Message = command.Message,
      Type = (Enums.NotificationType)command.Type,
      ReferenceId = command.RelatedEntityId,
      IsRead = false,
      CreatedAt = DateTime.UtcNow
    };

    await storeRepository.AddAsync(notification, cancellationToken);
    await storeRepository.SaveChangesAsync(cancellationToken);

    return new NotificationDto(
      notification.Id,
      notification.UserId,
      notification.Title,
      notification.Message,
      notification.Type,
      notification.ReferenceId,
      command.ActionUrl,
      notification.IsRead,
      notification.CreatedAt
    );
  }

}