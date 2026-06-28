namespace Medium.Api.Domain.Comment.Commands;

public record CommentResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid ArticleId,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt);