using Medium.Api.Domain.Comment.Dtos;
using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using CommentModel = Medium.Api.Models.Comment;

namespace Medium.Api.Domain.Comment.Repositories;

public class CommentRepository(ApplicationDbContext context)
{
  private readonly ApplicationDbContext _context = context;

  public async Task<CommentModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.Comments
        .Include(c => c.User)
        .Include(c => c.Article)
        .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
  }

  public async Task<IReadOnlyCollection<CommentModel>> GetByArticleAsync(
      Guid articleId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    return await _context.Comments
        .Include(c => c.User)
        .Where(c => c.ArticleId == articleId)
        .OrderByDescending(c => c.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyCollection<CommentModel>> GetByUserAsync(
      Guid userId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    return await _context.Comments
        .Include(c => c.User)
        .Include(c => c.Article)
        .Where(c => c.UserId == userId)
        .OrderByDescending(c => c.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }

  public async Task<int> CountByArticleAsync(Guid articleId, CancellationToken cancellationToken = default)
  {
    return await _context.Comments.CountAsync(c => c.ArticleId == articleId, cancellationToken);
  }

  public async Task<int> CountByUserAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await _context.Comments.CountAsync(c => c.UserId == userId, cancellationToken);
  }

  public async Task AddAsync(CommentModel comment, CancellationToken cancellationToken = default)
  {
    await _context.Comments.AddAsync(comment, cancellationToken);
  }

  public void Remove(CommentModel comment)
  {
    _context.Comments.Remove(comment);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task<CommentWithUserData?> GetCommentWithUserAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var comment = await _context.Comments
        .Include(c => c.User)
        .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    if (comment == null) return null;

    return new CommentWithUserData(
        comment.Id,
        comment.UserId,
        comment.User.Name,
        comment.ArticleId,
        comment.Content,
        comment.CreatedAt,
        comment.UpdatedAt);
  }

  public async Task<IReadOnlyCollection<CommentModel>> GetCommentsSinceAsync(DateTime since, CancellationToken cancellationToken = default)
  {
    return await _context.Comments
        .Include(c => c.User)
        .Where(c => c.CreatedAt >= since)
        .OrderByDescending(c => c.CreatedAt)
        .ToListAsync(cancellationToken);
  }

  public async Task<int> GetCommentsCountSinceAsync(DateTime since, CancellationToken cancellationToken = default)
  {
    return await _context.Comments
        .CountAsync(c => c.CreatedAt >= since, cancellationToken);
  }
}