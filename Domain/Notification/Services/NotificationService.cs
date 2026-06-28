// TODO: remove this layer service and migrate it to CQRS Pattern

using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Domain.Notification.Repositories;

using NotificationModel = Medium.Api.Models.Notification;

namespace Medium.Api.Domain.Notification.Services;

public class NotificationService(NotificationStoreRepository notificationStoreRepository, NotificationQueryRepository notificationQueryRepository)
{
  private readonly NotificationStoreRepository _notificationStoreRepository = notificationStoreRepository;
  private readonly NotificationQueryRepository _notificationQueryRepository = notificationQueryRepository;

  public async Task<NotificationResponse> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
  {
    var notification = new NotificationModel
    {
      Id = Guid.NewGuid(),
      UserId = Guid.Parse(request.UserId),
      Title = request.Title,
      Message = request.Message,
      Type = (Enums.NotificationType)request.Type,
      ReferenceId = request.RelatedEntityId != null ? Guid.Parse(request.RelatedEntityId) : null,
      IsRead = false,
      CreatedAt = DateTime.UtcNow
    };

    await _notificationStoreRepository.AddAsync(notification, cancellationToken);
    await _notificationStoreRepository.SaveChangesAsync(cancellationToken);

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

  public async Task<IEnumerable<NotificationResponse>> CreateRangeAsync(
    IEnumerable<CreateNotificationRequest> requests,
    CancellationToken cancellationToken = default)
  {
    if (requests == null || !requests.Any())
    {
      return Enumerable.Empty<NotificationResponse>();
    }

    var utcNow = DateTime.UtcNow;

    // 1. Map all requests to models at once
    var notifications = requests.Select(request => new NotificationModel
    {
      Id = Guid.NewGuid(),
      UserId = Guid.Parse(request.UserId),
      Title = request.Title,
      Message = request.Message,
      Type = (Enums.NotificationType)request.Type,
      ReferenceId = request.RelatedEntityId != null ? Guid.Parse(request.RelatedEntityId) : null,
      IsRead = false,
      CreatedAt = utcNow
    }).ToList();

    // 2. Bulk insert into repository (Only 1 database roundtrip for local operations)
    await _notificationStoreRepository.AddRangeAsync(notifications, cancellationToken);
    await _notificationStoreRepository.SaveChangesAsync(cancellationToken);

    // 3. Project the saved models back to responses
    return notifications.Select(notification => new NotificationResponse(
        notification.Id.ToString(),
        notification.UserId.ToString(),
        notification.Title,
        notification.Message,
        (NotificationType)notification.Type,
        notification.ReferenceId?.ToString(),
        // Since ActionUrl might not be stored in the DB model, we pair it from the input or leave it if handled elsewhere
        requests.FirstOrDefault(r => r.UserId == notification.UserId.ToString())?.ActionUrl,
        notification.IsRead,
        notification.CreatedAt
    )).ToList();
  }

  public async Task MarkAsReadAsync(string notificationId, CancellationToken cancellationToken = default)
  {
    await _notificationStoreRepository.MarkAsReadAsync(notificationId, cancellationToken);
  }

  public async Task<List<NotificationResponse>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
  {
    var notifications = await _notificationQueryRepository.GetUserNotificationsAsync(userId, page, pageSize, cancellationToken);

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