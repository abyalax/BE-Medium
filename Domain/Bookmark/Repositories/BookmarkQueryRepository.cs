using Medium.Api.Domain.Bookmark.Dtos;
using Medium.Api.Domain.Bookmark.Mapper;
using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using BookmarkModel = Medium.Api.Models.Bookmark;

namespace Medium.Api.Domain.Bookmark.Repositories;

public class BookmarkQueryRepository(ApplicationDbContext context)
{
  public async Task<BookmarkModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Bookmarks.AsNoTracking()
        .Include(b => b.Article)
        .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
  }

  public async Task<BookmarkModel?> GetByUserAndArticleAsync(
      Guid userId,
      Guid articleId,
      CancellationToken cancellationToken = default)
  {
    return await context.Bookmarks.AsNoTracking()
        .Include(b => b.Article)
        .FirstOrDefaultAsync(b => b.UserId == userId && b.ArticleId == articleId, cancellationToken);
  }

  public async Task<IReadOnlyCollection<BookmarkModel>> GetByUserAsync(
      Guid userId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    return await context.Bookmarks.AsNoTracking()
        .Include(b => b.Article)
        .Where(b => b.UserId == userId)
        .OrderByDescending(b => b.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }

  public async Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await context.Bookmarks.AsNoTracking().CountAsync(b => b.UserId == userId, cancellationToken);
  }

  public async Task<bool> ExistsAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default)
  {
    return await context.Bookmarks.AsNoTracking()
        .AnyAsync(b => b.UserId == userId && b.ArticleId == articleId, cancellationToken);
  }

  public async Task<BookmarkDto?> GetBookmarkWithArticleAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var bookmark = await context.Bookmarks.AsNoTracking()
        .Include(b => b.Article)
        .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    if (bookmark == null) return null;
    return BookmarkMapper.ToResponse(bookmark);
  }
}