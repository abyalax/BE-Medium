using MediatR;

using Medium.Api.Domain.Comment.Dtos;

namespace Medium.Api.Domain.Comment.Commands;

public record CreateCommentCommand(
    Guid UserId,
    Guid ArticleId,
    string Content
) : IRequest<CommentDto>;