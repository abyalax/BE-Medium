using MediatR;

namespace Medium.Api.Domain.Comment.Commands;

public record UpdateCommentCommand(
    Guid CommentId,
    Guid UserId,
    bool IsAdmin,
    string Content
) : IRequest<CommentResponse>;