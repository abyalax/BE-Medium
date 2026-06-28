using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using NotificationModel = Medium.Api.Models.Notification;

namespace Medium.Api.Domain.Notification.Repositories;

public class NotificationQueryRepository(ApplicationDbContext context)
{
  public async Task<List<NotificationModel>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
  {
    return await context.Notification.AsNoTracking()
      .Where(n => n.UserId == Guid.Parse(userId))
      .OrderByDescending(n => n.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);
  }
}