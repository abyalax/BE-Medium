using System.Text.Json.Serialization;

namespace Medium.Api.Infrastructure.Events.Events;

public record ArticlePublishedEvent(
    [property: JsonPropertyName("id")] string ArticleId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("authorId")] string AuthorId,
    [property: JsonPropertyName("publishedAt")] DateTime PublishedAt
);