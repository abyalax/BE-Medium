using MediatR;

using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Infrastructure.Exceptions;

using CommentModel = Medium.Api.Models.Comment;

namespace Medium.Api.Domain.Comment.Commands.Handlers;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, bool>
{
  private readonly CommentStoreRepository _commentStoreRepository;
  private readonly CommentQueryRepository _commentQueryRepository;

  public DeleteCommentCommandHandler(CommentStoreRepository commentStoreRepository, CommentQueryRepository commentQueryRepository)
  {
    _commentStoreRepository = commentStoreRepository;
    _commentQueryRepository = commentQueryRepository;
  }

  public async Task<bool> Handle(DeleteCommentCommand command, CancellationToken cancellationToken = default)
  {
    var comment = await _commentQueryRepository.GetByIdAsync(command.CommentId, cancellationToken)
        ?? throw new NotFoundException("Comment not found");

    if (!command.IsAdmin && comment.UserId != command.UserId)
    {
      throw new ForbiddenException("You can only delete your own comments");
    }

    _commentStoreRepository.Remove(comment);
    await _commentStoreRepository.SaveChangesAsync(cancellationToken);

    return true;
  }
}