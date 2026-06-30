using System.Text.Json.Serialization;

namespace Medium.Api.Infrastructure.Nats.Events;

public record ArticlePublishedEvent(
    [property: JsonPropertyName("id")] string ArticleId,
    [property: JsonPropertyName("authorId")] string AuthorId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("publishedAt")] DateTime? PublishedAt
);

public record ArticleCreatedEvent(
  [property: JsonPropertyName("id")] string ArticleId,
  [property: JsonPropertyName("authorId")] string AuthorId,
  [property: JsonPropertyName("title")] string Title,
  [property: JsonPropertyName("slug")] string Slug,
  [property: JsonPropertyName("content")] string Content
);

public record ArticleUpdatedEvent(
  [property: JsonPropertyName("id")] string ArticleId,
  [property: JsonPropertyName("authorId")] string AuthorId,
  [property: JsonPropertyName("title")] string Title
);

public record ArticleGetRequest(
  [property: JsonPropertyName("articleId")] string ArticleId
);

public record ArticleGetResponse(
  [property: JsonPropertyName("id")] string? Id,
  [property: JsonPropertyName("title")] string? Title,
  [property: JsonPropertyName("content")] string? Content,
  [property: JsonPropertyName("authorId")] string? AuthorId,
  [property: JsonPropertyName("authorName")] string? AuthorName,
  [property: JsonPropertyName("slug")] string? Slug,
  [property: JsonPropertyName("status")] string? Status,
  [property: JsonPropertyName("publishedAt")] DateTime? PublishedAt,
  [property: JsonPropertyName("createdAt")] DateTime? CreatedAt,
  [property: JsonPropertyName("error")] string? Error
);