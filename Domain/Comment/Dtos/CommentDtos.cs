namespace Medium.Api.Domain.Comment.Dtos;

public record CreateCommentRequest(
    Guid ArticleId,
    string Content
);

public record UpdateCommentRequest(
    string Content
);

public record CommentResponse(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid ArticleId,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record PagedCommentResponse(
    IReadOnlyCollection<CommentResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public record CommentWithUserData(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid ArticleId,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt);
