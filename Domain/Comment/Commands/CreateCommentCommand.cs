using MediatR;

namespace Medium.Api.Domain.Comment.Commands;

public record CreateCommentCommand(
    Guid UserId,
    Guid ArticleId,
    string Content
) : IRequest<CommentResponse>;