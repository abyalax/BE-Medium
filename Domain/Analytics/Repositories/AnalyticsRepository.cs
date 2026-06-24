using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using ArticleModel = Medium.Api.Models.Article;

namespace Medium.Api.Domain.Analytics.Repositories;

public class AnalyticsRepository
{
  private readonly ApplicationDbContext _context;

  public AnalyticsRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<int> GetTotalUsersAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Users.CountAsync(cancellationToken);
  }

  public async Task<int> GetTotalArticlesAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Articles.CountAsync(cancellationToken);
  }

  public async Task<int> GetTotalPublishedArticlesAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Articles.CountAsync(a => a.Status == Enums.ArticleStatus.Published, cancellationToken);
  }

  public async Task<int> GetTotalCommentsAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Comments.CountAsync(cancellationToken);
  }

  public async Task<int> GetTotalTagsAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Tags.CountAsync(cancellationToken);
  }

  public async Task<int> GetTotalBookmarksAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Bookmarks.CountAsync(cancellationToken);
  }

  public async Task<int> GetTotalFollowsAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Follows.CountAsync(cancellationToken);
  }

  public async Task<int> GetTotalReadingHistoryAsync(CancellationToken cancellationToken = default)
  {
    return await _context.ReadingHistories.CountAsync(cancellationToken);
  }

  public async Task<int> GetAuthorTotalArticlesAsync(Guid authorId, CancellationToken cancellationToken = default)
  {
    return await _context.Articles.CountAsync(a => a.AuthorId == authorId, cancellationToken);
  }

  public async Task<long> GetAuthorTotalViewsAsync(Guid authorId, CancellationToken cancellationToken = default)
  {
    return await _context.Articles
        .Where(a => a.AuthorId == authorId)
        .SumAsync(a => a.ViewCount, cancellationToken);
  }

  public async Task<int> GetAuthorTotalCommentsAsync(Guid authorId, CancellationToken cancellationToken = default)
  {
    return await _context.Comments
        .CountAsync(c => c.Article.AuthorId == authorId, cancellationToken);
  }

  public async Task<int> GetAuthorTotalBookmarksAsync(Guid authorId, CancellationToken cancellationToken = default)
  {
    return await _context.Bookmarks
        .CountAsync(b => b.Article.AuthorId == authorId, cancellationToken);
  }

  public async Task<int> GetAuthorTotalFollowersAsync(Guid authorId, CancellationToken cancellationToken = default)
  {
    return await _context.Follows.CountAsync(f => f.FollowingId == authorId, cancellationToken);
  }

  public async Task<IReadOnlyCollection<ArticleModel>> GetAuthorMostViewedArticlesAsync(
      Guid authorId,
      int limit,
      CancellationToken cancellationToken = default)
  {
    return await _context.Articles
        .Where(a => a.AuthorId == authorId)
        .OrderByDescending(a => a.ViewCount)
        .Take(limit)
        .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyCollection<ArticleModel>> GetAuthorMostBookmarkedArticlesAsync(
      Guid authorId,
      int limit,
      CancellationToken cancellationToken = default)
  {
    return await _context.Articles
        .Include(a => a.Bookmarks)
        .Where(a => a.AuthorId == authorId)
        .OrderByDescending(a => a.Bookmarks.Count)
        .Take(limit)
        .ToListAsync(cancellationToken);
  }
}