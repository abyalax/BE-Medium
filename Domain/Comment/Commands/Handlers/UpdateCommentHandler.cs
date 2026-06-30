using MediatR;

using Medium.Api.Domain.Comment.Dtos;
using Medium.Api.Domain.Comment.Mapper;
using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Comment.Commands.Handlers;

public class UpdateCommentHandler(
  CommentStoreRepository storeRepository,
  CommentQueryRepository queryRepository
) : IRequestHandler<UpdateCommentCommand, CommentDto>
{
  public async Task<CommentDto> Handle(UpdateCommentCommand command, CancellationToken cancellationToken)
  {
    var comment = await queryRepository.GetByIdAsync(command.CommentId, cancellationToken)
        ?? throw new NotFoundException("Comment not found");

    if (comment.UserId != command.UserId)
      throw new ForbiddenException("You can only edit your own comments");

    comment.Content = command.Content;
    comment.UpdatedAt = DateTime.UtcNow;
    await storeRepository.SaveChangesAsync(cancellationToken);

    var commentWithUser = await queryRepository.GetCommentWithUserAsync(comment.Id, cancellationToken)
      ?? throw new NotFoundException("Comment not found");
    var mappedComment = CommentMapper.ToResponse(commentWithUser);
    return mappedComment;
  }
}