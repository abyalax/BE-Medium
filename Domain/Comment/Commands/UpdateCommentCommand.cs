using MediatR;

using Medium.Api.Domain.Comment.Dtos;

namespace Medium.Api.Domain.Comment.Commands;

public record UpdateCommentCommand(
    Guid CommentId,
    Guid UserId,
    string Content
) : IRequest<CommentDto>;