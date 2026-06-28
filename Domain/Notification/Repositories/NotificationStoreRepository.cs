using Medium.Api.Infrastructure.Database;

using NotificationModel = Medium.Api.Models.Notification;

namespace Medium.Api.Domain.Notification.Repositories;

public class NotificationStoreRepository(ApplicationDbContext context)
{
  public async Task AddAsync(NotificationModel notification, CancellationToken cancellationToken = default)
  {
    await context.Notification.AddAsync(notification, cancellationToken);
  }

  public async Task AddRangeAsync(IEnumerable<NotificationModel> entities, CancellationToken cancellationToken = default)
  {
    await context.Notification.AddRangeAsync(entities, cancellationToken);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await context.SaveChangesAsync(cancellationToken);
  }

  public async Task MarkAsReadAsync(string notificationId, CancellationToken cancellationToken = default)
  {
    var notification = await context.Notification.FindAsync(new object[] { Guid.Parse(notificationId) }, cancellationToken);
    if (notification != null)
    {
      notification.IsRead = true;
      notification.ReadAt = DateTime.UtcNow;
      await context.SaveChangesAsync(cancellationToken);
    }
  }
}