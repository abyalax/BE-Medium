using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Domain.Notification.Mapper;
using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

namespace Medium.Api.Domain.Notification.Repositories;

public class NotificationQueryRepository(ApplicationDbContext context)
{
  public async Task<int> CountAsync(Guid userId, string? search, CancellationToken cancellationToken)
  {
    var query = context.Notification.AsNoTracking().Where(n => n.UserId == userId);

    if (!string.IsNullOrWhiteSpace(search))
      query = query.Where(n => n.Title.Contains(search) || n.Message.Contains(search));

    return await query.CountAsync(cancellationToken);
  }

  public async Task<IReadOnlyCollection<NotificationDto>> ListAsync(
        Guid userId,
        int page,
        int pageSize,
        string? search,
        string? sortBy,
        CancellationToken cancellationToken = default)
  {
    var query = context.Notification
        .AsNoTracking()
        .Where(n => n.UserId == userId);

    if (!string.IsNullOrWhiteSpace(search))
      query = query.Where(n => n.Title.Contains(search) || n.Message.Contains(search));

    query = sortBy?.ToLower() switch
    {
      "oldest" => query.OrderBy(n => n.CreatedAt),
      _ => query.OrderByDescending(n => n.CreatedAt)
    };

    var notificationEntities = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return [.. notificationEntities.Select(NotificationMapper.ToResponse)];
  }
}