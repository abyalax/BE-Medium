namespace Medium.Api.Domain.Article.Dtos;

using System.Text.Json.Serialization;
using Medium.Api.Enums;

public record CreateArticleRequest(
    string Title,
    string Content,
    string? CoverImageUrl = null,
    IReadOnlyCollection<Guid>? TagIds = null);

public record UpdateArticleRequest(
    string Title,
    string Content,
    string? CoverImageUrl = null,
    IReadOnlyCollection<Guid>? TagIds = null);

public record PublishArticleRequest(
    DateTime? ScheduledAt = null
    );

public record ArticleResponse(
    Guid Id,
    Guid AuthorId,
    string AuthorName,
    string Title,
    string Slug,
    string Content,
    string? CoverImageUrl,
    ArticleStatus Status,
    DateTime? PublishedAt,
    DateTime? ScheduledAt,
    long ViewCount,
    IReadOnlyCollection<TagResponse> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record TagResponse(
    Guid Id,
    string Name,
    string Slug);

public record PagedArticleResponse(
    IReadOnlyCollection<ArticleResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public record ArticleWithAuthorTagsData(
    Guid Id,
    Guid AuthorId,
    string AuthorName,
    string Title,
    string Slug,
    string Content,
    string? CoverImageUrl,
    ArticleStatus Status,
    DateTime? PublishedAt,
    DateTime? ScheduledAt,
    long ViewCount,
    IReadOnlyCollection<TagResponse> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt);
