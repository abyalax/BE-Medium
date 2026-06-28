using Medium.Api.Domain.ReadingHistory.Dtos;
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

  public async Task<IReadOnlyCollection<ReadingHistoryModel>> GetRecentByUserAsync(
      Guid userId,
      int limit,
      CancellationToken cancellationToken = default)
  {
    return await context.ReadingHistories.AsNoTracking()
        .Include(r => r.Article)
        .Where(r => r.UserId == userId)
        .OrderByDescending(r => r.ReadAt)
        .Take(limit)
        .ToListAsync(cancellationToken);
  }

  public async Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await context.ReadingHistories.AsNoTracking().CountAsync(r => r.UserId == userId, cancellationToken);
  }

  public async Task<ReadingHistoryWithArticleData?> GetReadingHistoryWithArticleAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var history = await context.ReadingHistories.AsNoTracking()
        .Include(r => r.Article)
        .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    if (history == null) return null;

    return new ReadingHistoryWithArticleData(
        history.Id,
        history.UserId,
        history.ArticleId,
        history.Article.Title,
        history.Article.Slug,
        history.DurationSeconds,
        history.ReadAt);
  }
}