using MediatR;

using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Comment.Commands.Handlers;

public class DeleteCommentHandler(
  CommentStoreRepository storeRepository,
  CommentQueryRepository queryRepository
) : IRequestHandler<DeleteCommentCommand, bool>
{

  public async Task<bool> Handle(DeleteCommentCommand command, CancellationToken cancellationToken)
  {
    var comment = await queryRepository.GetByIdAsync(command.CommentId, cancellationToken)
      ?? throw new NotFoundException("Comment not found");

    if (!command.IsAdmin && comment.UserId != command.UserId)
      throw new ForbiddenException("You can only delete your own comments");

    storeRepository.Remove(comment);
    await storeRepository.SaveChangesAsync(cancellationToken);

    return true;
  }
}