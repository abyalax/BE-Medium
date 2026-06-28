using MediatR;

using Medium.Api.Domain.Comment.Dtos;
using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

using CommentModel = Medium.Api.Models.Comment;

namespace Medium.Api.Domain.Comment.Commands.Handlers;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentResponse>
{
  private readonly CommentStoreRepository _commentStoreRepository;
  private readonly CommentQueryRepository _commentQueryRepository;
  private readonly INatsPublisher _publisher;

  public CreateCommentCommandHandler(CommentStoreRepository commentStoreRepository, CommentQueryRepository commentQueryRepository, INatsPublisher publisher)
  {
    _commentStoreRepository = commentStoreRepository;
    _commentQueryRepository = commentQueryRepository;
    _publisher = publisher;
  }

  public async Task<CommentResponse> Handle(CreateCommentCommand command, CancellationToken cancellationToken = default)
  {
    var comment = new CommentModel
    {
      Id = Guid.NewGuid(),
      UserId = command.UserId,
      ArticleId = command.ArticleId,
      Content = command.Content
    };

    await _commentStoreRepository.AddAsync(comment, cancellationToken);
    await _commentStoreRepository.SaveChangesAsync(cancellationToken);

    var @event = new CommentCreatedEvent(
        comment.Id.ToString(),
        comment.ArticleId.ToString(),
        comment.UserId.ToString(),
        comment.Content,
        comment.CreatedAt
    );

    await _publisher.PublishAsync(NatsSubjects.CommentCreated, @event);

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