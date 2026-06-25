using Medium.Api.Domain.ReadingHistory.Dtos;
using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using ReadingHistoryModel = Medium.Api.Models.ReadingHistory;

namespace Medium.Api.Domain.ReadingHistory.Repositories;

public class ReadingHistoryRepository(ApplicationDbContext context)
{
  private readonly ApplicationDbContext _context = context;

  public async Task<ReadingHistoryModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.ReadingHistories
        .Include(r => r.Article)
        .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
  }

  public async Task<IReadOnlyCollection<ReadingHistoryModel>> GetByUserAsync(
      Guid userId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    return await _context.ReadingHistories
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
    return await _context.ReadingHistories
        .Include(r => r.Article)
        .Where(r => r.UserId == userId)
        .OrderByDescending(r => r.ReadAt)
        .Take(limit)
        .ToListAsync(cancellationToken);
  }

  public async Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await _context.ReadingHistories.CountAsync(r => r.UserId == userId, cancellationToken);
  }

  public async Task AddAsync(ReadingHistoryModel readingHistory, CancellationToken cancellationToken = default)
  {
    await _context.ReadingHistories.AddAsync(readingHistory, cancellationToken);
  }

  public void Remove(ReadingHistoryModel readingHistory)
  {
    _context.ReadingHistories.Remove(readingHistory);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task<ReadingHistoryWithArticleData?> GetReadingHistoryWithArticleAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var history = await _context.ReadingHistories
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