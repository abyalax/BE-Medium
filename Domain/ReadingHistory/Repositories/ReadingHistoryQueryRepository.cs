using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using ReadingHistoryModel = Medium.Api.Models.ReadingHistory;

namespace Medium.Api.Domain.ReadingHistory.Repositories;

public class ReadingHistoryQueryRepository(ApplicationDbContext context)
{
  public async Task<ReadingHistoryModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.ReadingHistories.AsNoTracking()
      .Include(r => r.Article)
      .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
  }

  public async Task<IReadOnlyCollection<ReadingHistoryModel>> GetByUserAsync(
      Guid userId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    return await context.ReadingHistories.AsNoTracking()
      .Include(r => r.Article)
      .Where(r => r.UserId == userId)
      .OrderByDescending(r => r.ReadAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);
  }

  public async Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await context.ReadingHistories.AsNoTracking().CountAsync(r => r.UserId == userId, cancellationToken);
  }

  public async Task<IReadOnlyCollection<ReadingHistoryModel>> ListAsync(
    Guid? userId,
    int page,
    int pageSize,
    string? search = null,
    string? sortBy = null,
    CancellationToken cancellationToken = default
  )
  {
    var query = context.ReadingHistories.AsNoTracking()
      .Where(r => r.UserId == userId)
      .Include(a => a.User)
      .Include(a => a.Article)
      .AsQueryable();

    if (!string.IsNullOrEmpty(search))
      query = query.Where(
        a => a.User.Name.Contains(search) ||
        a.Article.Title.Contains(search) ||
        a.Article.Content.Contains(search)
      );

    query = sortBy?.ToLower() switch
    {
      "updated" => query.OrderByDescending(a => a.UpdatedAt),
      "created" => query.OrderByDescending(a => a.CreatedAt),
      _ => query.OrderByDescending(a => a.CreatedAt)
    };

    return await query
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);
  }
}