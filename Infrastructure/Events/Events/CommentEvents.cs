using System.Text.Json.Serialization;

namespace Medium.Api.Infrastructure.Events.Events;

public record CommentCreatedEvent(
    [property: JsonPropertyName("id")] string CommentId,
    [property: JsonPropertyName("articleId")] string ArticleId,
    [property: JsonPropertyName("userId")] string UserId,
    [property: JsonPropertyName("content")] string Content,
    [property: JsonPropertyName("createdAt")] DateTime CreatedAt
);