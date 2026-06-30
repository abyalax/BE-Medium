namespace Medium.Api.Domain.Comment.Dtos;

public record CreateCommentRequest(
    Guid ArticleId,
    string Content
);

public record UpdateCommentRequest(
    string Content
);