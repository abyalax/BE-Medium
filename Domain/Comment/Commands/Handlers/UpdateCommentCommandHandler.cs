using MediatR;

using Medium.Api.Domain.Comment.Dtos;
using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Infrastructure.Exceptions;

using CommentModel = Medium.Api.Models.Comment;

namespace Medium.Api.Domain.Comment.Commands.Handlers;

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentResponse>
{
  private readonly CommentStoreRepository _commentStoreRepository;
  private readonly CommentQueryRepository _commentQueryRepository;

  public UpdateCommentCommandHandler(CommentStoreRepository commentStoreRepository, CommentQueryRepository commentQueryRepository)
  {
    _commentStoreRepository = commentStoreRepository;
    _commentQueryRepository = commentQueryRepository;
  }

  public async Task<CommentResponse> Handle(UpdateCommentCommand command, CancellationToken cancellationToken = default)
  {
    var comment = await _commentQueryRepository.GetByIdAsync(command.CommentId, cancellationToken)
        ?? throw new NotFoundException("Comment not found");

    if (!command.IsAdmin && comment.UserId != command.UserId)
    {
      throw new ForbiddenException("You can only edit your own comments");
    }

    comment.Content = command.Content;
    comment.UpdatedAt = DateTime.UtcNow;
    await _commentStoreRepository.SaveChangesAsync(cancellationToken);

    var commentWithUser = await _commentQueryRepository.GetCommentWithUserAsync(comment.Id, cancellationToken);
    return ToResponse(commentWithUser!);
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
}