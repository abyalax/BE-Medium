using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using CommentModel = Medium.Api.Models.Comment;

namespace Medium.Api.Domain.Comment.Repositories;

public class CommentQueryRepository(ApplicationDbContext context)
{
  public async Task<CommentModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Comments.AsNoTracking()
      .Include(c => c.User)
      .Include(c => c.Article)
      .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
  }

  public async Task<IReadOnlyCollection<CommentModel>> ListByUserAsync(
    Guid userId,
    int page,
    int pageSize,
    string? search = null,
    string? sortBy = null,
    CancellationToken cancellationToken = default
  )
  {
    var query = context.Comments.AsNoTracking()
      .Where(c => c.UserId == userId)
      .Include(c => c.Article)
      .Include(c => c.Content)
      .Include(c => c.User)
      .AsSplitQuery()
      .AsQueryable();

    if (!string.IsNullOrEmpty(search))
      query = query.Where(c => c.Content.Contains(search));

    query = sortBy?.ToLower() switch
    {
      "updated" => query.OrderByDescending(c => c.UpdatedAt),
      "created" => query.OrderByDescending(c => c.CreatedAt),
      _ => query.OrderByDescending(c => c.CreatedAt)
    };

    return await query
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyCollection<CommentModel>> ListByArticleAsync(
    Guid articleId,
    int page,
    int pageSize,
    string? search = null,
    string? sortBy = null,
    CancellationToken cancellationToken = default
  )
  {
    var query = context.Comments.AsNoTracking()
      .Where(c => c.ArticleId == articleId)
      .Include(c => c.Article)
      .Include(c => c.Content)
      .Include(c => c.User)
      .AsSplitQuery()
      .AsQueryable();

    if (!string.IsNullOrEmpty(search))
      query = query.Where(c => c.Content.Contains(search));

    query = sortBy?.ToLower() switch
    {
      "updated" => query.OrderByDescending(c => c.UpdatedAt),
      "created" => query.OrderByDescending(c => c.CreatedAt),
      _ => query.OrderByDescending(c => c.CreatedAt)
    };

    return await query
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);
  }

  public async Task<int> CountByArticleAsync(Guid articleId, CancellationToken cancellationToken = default)
  {
    return await context.Comments.AsNoTracking().CountAsync(c => c.ArticleId == articleId, cancellationToken);
  }

  public async Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await context.Comments.AsNoTracking().CountAsync(c => c.UserId == userId, cancellationToken);
  }

  public async Task<int> CountAsync(CancellationToken cancellationToken = default)
  {
    return await context.Articles.AsNoTracking().CountAsync(cancellationToken);
  }

  public async Task<CommentModel?> GetCommentWithUserAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var comment = await context.Comments.AsNoTracking()
      .Include(c => c.User)
      .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    if (comment == null) return null;
    return comment;

  }

  public async Task<IReadOnlyCollection<CommentModel>> GetCommentsSinceAsync(DateTime since, CancellationToken cancellationToken = default)
  {
    return await context.Comments.AsNoTracking()
      .Include(c => c.User)
      .Where(c => c.CreatedAt >= since)
      .OrderByDescending(c => c.CreatedAt)
      .ToListAsync(cancellationToken);
  }

  public async Task<int> GetCommentsCountSinceAsync(DateTime since, CancellationToken cancellationToken = default)
  {
    return await context.Comments.AsNoTracking()
      .CountAsync(c => c.CreatedAt >= since, cancellationToken);
  }

}