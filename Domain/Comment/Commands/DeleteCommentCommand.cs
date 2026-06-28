using MediatR;

namespace Medium.Api.Domain.Comment.Commands;

public record DeleteCommentCommand(
    Guid CommentId,
    Guid UserId,
    bool IsAdmin
) : IRequest<bool>;