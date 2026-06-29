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
  [property: JsonPropertyName("slug")] string Slug
);

public record ArticleUpdatedEvent(
  [property: JsonPropertyName("id")] string ArticleId,
  [property: JsonPropertyName("authorId")] string AuthorId,
  [property: JsonPropertyName("title")] string Title
);