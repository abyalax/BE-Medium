using MediatR;

using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Domain.Notification.Repositories;

using NotificationModel = Medium.Api.Models.Notification;

namespace Medium.Api.Domain.Notification.Commands.Handlers;

public class CreateRangeNotificationHandler(
    NotificationStoreRepository storeRepository
  ) : IRequestHandler<CreateRangeNotificationCommand, IReadOnlyCollection<NotificationDto>?>
{

  public async Task<IReadOnlyCollection<NotificationDto>?> Handle(CreateRangeNotificationCommand command, CancellationToken cancellationToken)
  {
    if (command == null) return [];
    var utcNow = DateTime.UtcNow;

    // 1. Map all command to models at once
    var notifications = command.Notifications.Select(request => new NotificationModel
    {
      Id = Guid.NewGuid(),
      UserId = request.UserId,
      Title = request.Title,
      Message = request.Message,
      Type = request.Type,
      ReferenceId = request.RelatedEntityId,
      IsRead = false,
      CreatedAt = utcNow
    }).ToList();

    // 2. Bulk insert into repository (Only 1 database roundtrip for local operations)
    await storeRepository.AddRangeAsync(notifications, cancellationToken);
    await storeRepository.SaveChangesAsync(cancellationToken);

    // 3. Project the saved models back to responses
    return [.. notifications.Select(notification => new NotificationDto(
        notification.Id,
        notification.UserId,
        notification.Title,
        notification.Message,
        notification.Type,
        notification.ReferenceId,
        command.Notifications.FirstOrDefault(r => r.UserId == notification.UserId)?.ActionUrl,
        notification.IsRead,
        notification.CreatedAt
    ))];
  }

}