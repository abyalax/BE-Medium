using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using ArticleModel = Medium.Api.Models.Article;

namespace Medium.Api.Domain.Analytics.Repositories;

public class AnalyticsQueryRepository(ApplicationDbContext context)
{
  public async Task<int> GetTotalUsersAsync(CancellationToken cancellationToken = default)
  {
    return await context.Users.AsNoTracking().CountAsync(cancellationToken);
  }

  public async Task<int> GetTotalArticlesAsync(CancellationToken cancellationToken = default)
  {
    return await context.Articles.AsNoTracking().CountAsync(cancellationToken);
  }

  public async Task<int> GetTotalPublishedArticlesAsync(CancellationToken cancellationToken = default)
  {
    return await context.Articles.AsNoTracking().CountAsync(a => a.Status == Enums.ArticleStatus.Published, cancellationToken);
  }

  public async Task<int> GetTotalCommentsAsync(CancellationToken cancellationToken = default)
  {
    return await context.Comments.AsNoTracking().CountAsync(cancellationToken);
  }

  public async Task<int> GetTotalTagsAsync(CancellationToken cancellationToken = default)
  {
    return await context.Tags.AsNoTracking().CountAsync(cancellationToken);
  }

  public async Task<int> GetTotalBookmarksAsync(CancellationToken cancellationToken = default)
  {
    return await context.Bookmarks.AsNoTracking().CountAsync(cancellationToken);
  }

  public async Task<int> GetTotalFollowsAsync(CancellationToken cancellationToken = default)
  {
    return await context.Follows.AsNoTracking().CountAsync(cancellationToken);
  }

  public async Task<int> GetTotalReadingHistoryAsync(CancellationToken cancellationToken = default)
  {
    return await context.ReadingHistories.AsNoTracking().CountAsync(cancellationToken);
  }

  public async Task<int> GetAuthorTotalArticlesAsync(Guid authorId, CancellationToken cancellationToken = default)
  {
    return await context.Articles.AsNoTracking().CountAsync(a => a.AuthorId == authorId, cancellationToken);
  }

  public async Task<long> GetAuthorTotalViewsAsync(Guid authorId, CancellationToken cancellationToken = default)
  {
    return await context.Articles.AsNoTracking()
        .Where(a => a.AuthorId == authorId)
        .SumAsync(a => a.ViewCount, cancellationToken);
  }

  public async Task<int> GetAuthorTotalCommentsAsync(Guid authorId, CancellationToken cancellationToken = default)
  {
    return await context.Comments.AsNoTracking()
        .CountAsync(c => c.Article.AuthorId == authorId, cancellationToken);
  }

  public async Task<int> GetAuthorTotalBookmarksAsync(Guid authorId, CancellationToken cancellationToken = default)
  {
    return await context.Bookmarks.AsNoTracking()
        .CountAsync(b => b.Article.AuthorId == authorId, cancellationToken);
  }

  public async Task<int> GetAuthorTotalFollowersAsync(Guid authorId, CancellationToken cancellationToken = default)
  {
    return await context.Follows.AsNoTracking().CountAsync(f => f.FollowingId == authorId, cancellationToken);
  }

  public async Task<IReadOnlyCollection<ArticleModel>> GetAuthorMostViewedArticlesAsync(
      Guid authorId,
      int limit,
      CancellationToken cancellationToken = default)
  {
    return await context.Articles.AsNoTracking()
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
    return await context.Articles.AsNoTracking()
        .Include(a => a.Bookmarks)
        .Where(a => a.AuthorId == authorId)
        .OrderByDescending(a => a.Bookmarks.Count)
        .Take(limit)
        .ToListAsync(cancellationToken);
  }
}