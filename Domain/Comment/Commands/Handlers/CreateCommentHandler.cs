using MediatR;

using Medium.Api.Domain.Comment.Dtos;
using Medium.Api.Domain.Comment.Mapper;
using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

using CommentModel = Medium.Api.Models.Comment;

namespace Medium.Api.Domain.Comment.Commands.Handlers;

public class CreateCommentHandler(
  CommentStoreRepository commentStoreRepository,
  CommentQueryRepository commentQueryRepository,
  INatsPublisher publisher
) : IRequestHandler<CreateCommentCommand, CommentDto>
{

  public async Task<CommentDto> Handle(CreateCommentCommand command, CancellationToken cancellationToken)
  {
    var comment = new CommentModel
    {
      Id = Guid.NewGuid(),
      UserId = command.UserId,
      ArticleId = command.ArticleId,
      Content = command.Content
    };

    await commentStoreRepository.AddAsync(comment, cancellationToken);
    await commentStoreRepository.SaveChangesAsync(cancellationToken);

    var @event = new CommentCreatedEvent
    {
      CommentId = comment.Id.ToString(),
      ArticleId = comment.ArticleId.ToString(),
      UserId = comment.UserId.ToString(),
      Content = comment.Content,
      CreatedAt = comment.CreatedAt
    };

    await publisher.PublishAsync(NatsSubjects.CommentCreated, @event);

    var commentWithUser = await commentQueryRepository.GetCommentWithUserAsync(comment.Id, cancellationToken)
      ?? throw new NotFoundException("Comment not found");
    var mappedComment = CommentMapper.ToResponse(commentWithUser);
    return mappedComment;
  }
}