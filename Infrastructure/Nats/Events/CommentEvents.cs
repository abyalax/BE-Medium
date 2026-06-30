using System.Text.Json.Serialization;

namespace Medium.Api.Infrastructure.Nats.Events;

public class CommentCreatedEvent
{
  [JsonPropertyName("id")]
  public string CommentId { get; set; } = string.Empty;
  
  [JsonPropertyName("articleId")]
  public string ArticleId { get; set; } = string.Empty;
  
  [JsonPropertyName("userId")]
  public string UserId { get; set; } = string.Empty;
  
  [JsonPropertyName("content")]
  public string Content { get; set; } = string.Empty;
  
  [JsonPropertyName("createdAt")]
  public DateTime CreatedAt { get; set; }
}