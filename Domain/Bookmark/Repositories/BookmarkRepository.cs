using Medium.Api.Domain.Bookmark.Dtos;
using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using BookmarkModel = Medium.Api.Models.Bookmark;

namespace Medium.Api.Domain.Bookmark.Repositories;

public class BookmarkRepository
{
  private readonly ApplicationDbContext _context;

  public BookmarkRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<BookmarkModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.Bookmarks
        .Include(b => b.Article)
        .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
  }

  public async Task<BookmarkModel?> GetByUserAndArticleAsync(
      Guid userId,
      Guid articleId,
      CancellationToken cancellationToken = default)
  {
    return await _context.Bookmarks
        .Include(b => b.Article)
        .FirstOrDefaultAsync(b => b.UserId == userId && b.ArticleId == articleId, cancellationToken);
  }

  public async Task<IReadOnlyCollection<BookmarkModel>> GetByUserAsync(
      Guid userId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    return await _context.Bookmarks
        .Include(b => b.Article)
        .Where(b => b.UserId == userId)
        .OrderByDescending(b => b.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }

  public async Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await _context.Bookmarks.CountAsync(b => b.UserId == userId, cancellationToken);
  }

  public async Task<bool> ExistsAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default)
  {
    return await _context.Bookmarks
        .AnyAsync(b => b.UserId == userId && b.ArticleId == articleId, cancellationToken);
  }

  public async Task AddAsync(BookmarkModel bookmark, CancellationToken cancellationToken = default)
  {
    await _context.Bookmarks.AddAsync(bookmark, cancellationToken);
  }

  public void Remove(BookmarkModel bookmark)
  {
    _context.Bookmarks.Remove(bookmark);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task<BookmarkWithArticleData?> GetBookmarkWithArticleAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var bookmark = await _context.Bookmarks
        .Include(b => b.Article)
        .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    if (bookmark == null) return null;

    return new BookmarkWithArticleData(
        bookmark.Id,
        bookmark.UserId,
        bookmark.ArticleId,
        bookmark.Article.Title,
        bookmark.Article.Slug,
        bookmark.CreatedAt);
  }
}