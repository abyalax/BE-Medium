using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using NotificationModel = Medium.Api.Models.Notification;

namespace Medium.Api.Domain.Notification.Repositories;

public class NotificationRepository
{
  private readonly ApplicationDbContext _context;

  public NotificationRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task AddAsync(NotificationModel notification, CancellationToken cancellationToken = default)
  {
    await _context.Notification.AddAsync(notification, cancellationToken);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task MarkAsReadAsync(string notificationId, CancellationToken cancellationToken = default)
  {
    var notification = await _context.Notification.FindAsync(new object[] { Guid.Parse(notificationId) }, cancellationToken);
    if (notification != null)
    {
      notification.IsRead = true;
      notification.ReadAt = DateTime.UtcNow;
      await _context.SaveChangesAsync(cancellationToken);
    }
  }

  public async Task<List<NotificationModel>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
  {
    return await _context.Notification
        .Where(n => n.UserId == Guid.Parse(userId))
        .OrderByDescending(n => n.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }
}