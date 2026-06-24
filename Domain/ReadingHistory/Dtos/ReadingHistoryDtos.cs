namespace Medium.Api.Domain.ReadingHistory.Dtos;

public record CreateReadingHistoryRequest(
    Guid ArticleId,
    int DurationSeconds);

public record ReadingHistoryResponse(
    Guid Id,
    Guid UserId,
    Guid ArticleId,
    string ArticleTitle,
    string ArticleSlug,
    int DurationSeconds,
    DateTime ReadAt);

public record PagedReadingHistoryResponse(
    IReadOnlyCollection<ReadingHistoryResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public record ReadingHistoryWithArticleData(
    Guid Id,
    Guid UserId,
    Guid ArticleId,
    string ArticleTitle,
    string ArticleSlug,
    int DurationSeconds,
    DateTime ReadAt);