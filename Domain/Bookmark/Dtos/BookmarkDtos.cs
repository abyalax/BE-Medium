namespace Medium.Api.Domain.Bookmark.Dtos;

public record BookmarkRequest(
    Guid ArticleId
);

public record BookmarkResponse(
    Guid Id,
    Guid UserId,
    Guid ArticleId,
    string ArticleTitle,
    string ArticleSlug,
    DateTime CreatedAt);

public record PagedBookmarkResponse(
    IReadOnlyCollection<BookmarkResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public record BookmarkWithArticleData(
    Guid Id,
    Guid UserId,
    Guid ArticleId,
    string ArticleTitle,
    string ArticleSlug,
    DateTime CreatedAt);