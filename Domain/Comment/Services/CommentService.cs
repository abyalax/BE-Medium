using Medium.Api.Domain.Comment.Dtos;
using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Infrastructure.Exceptions;

using CommentModel = Medium.Api.Models.Comment;

namespace Medium.Api.Domain.Comment.Services;

public class CommentService(CommentRepository commentRepository)
{
  private const int MaxPageSize = 100;
  private readonly CommentRepository _commentRepository = commentRepository;
  private readonly string messageNotFound = "Comment not found";

  public async Task<CommentResponse> CreateAsync(
      Guid userId,
      CreateCommentRequest request,
      CancellationToken cancellationToken = default)
  {
    var comment = new CommentModel
    {
      Id = Guid.NewGuid(),
      UserId = userId,
      ArticleId = request.ArticleId,
      Content = request.Content
    };

    await _commentRepository.AddAsync(comment, cancellationToken);
    await _commentRepository.SaveChangesAsync(cancellationToken);

    return await GetByIdAsync(comment.Id, cancellationToken);
  }

  public async Task<CommentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var comment = await _commentRepository.GetCommentWithUserAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    return ToResponse(comment);
  }

  public async Task<PagedCommentResponse> GetByArticleAsync(
      Guid articleId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

    var totalItems = await _commentRepository.CountByArticleAsync(articleId, cancellationToken);
    var items = await _commentRepository.GetByArticleAsync(articleId, page, pageSize, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    return new PagedCommentResponse(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);
  }

  public async Task<PagedCommentResponse> GetByUserAsync(
      Guid userId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

    var totalItems = await _commentRepository.CountByUserAsync(userId, cancellationToken);
    var items = await _commentRepository.GetByUserAsync(userId, page, pageSize, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    return new PagedCommentResponse(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);
  }

  public async Task<CommentResponse> UpdateAsync(
      Guid id,
      Guid currentUserId,
      bool isAdmin,
      UpdateCommentRequest request,
      CancellationToken cancellationToken = default)
  {
    var comment = await _commentRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (!isAdmin && comment.UserId != currentUserId)
    {
      throw new ForbiddenException("You can only edit your own comments");
    }

    comment.Content = request.Content;
    await _commentRepository.SaveChangesAsync(cancellationToken);

    return await GetByIdAsync(id, cancellationToken);
  }

  public async Task DeleteAsync(
      Guid id,
      Guid currentUserId,
      bool isAdmin,
      CancellationToken cancellationToken = default)
  {
    var comment = await _commentRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (!isAdmin && comment.UserId != currentUserId)
    {
      throw new ForbiddenException("You can only delete your own comments");
    }

    _commentRepository.Remove(comment);
    await _commentRepository.SaveChangesAsync(cancellationToken);
  }

  private static CommentResponse ToResponse(CommentWithUserData comment)
  {
    return new CommentResponse(
        comment.Id,
        comment.UserId,
        comment.UserName,
        comment.ArticleId,
        comment.Content,
        comment.CreatedAt,
        comment.UpdatedAt);
  }

  private static CommentResponse ToResponse(CommentModel comment)
  {
    return new CommentResponse(
        comment.Id,
        comment.UserId,
        comment.User.Name,
        comment.ArticleId,
        comment.Content,
        comment.CreatedAt,
        comment.UpdatedAt);
  }
}