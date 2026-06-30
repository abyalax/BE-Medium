using System.Text.Json.Serialization;

namespace Medium.Api.Infrastructure.Nats.Events;

public class ArticlePublishedEvent
{
  [JsonPropertyName("id")]
  public string ArticleId { get; set; } = string.Empty;
  
  [JsonPropertyName("authorId")]
  public string AuthorId { get; set; } = string.Empty;
  
  [JsonPropertyName("title")]
  public string Title { get; set; } = string.Empty;
  
  [JsonPropertyName("publishedAt")]
  public DateTime? PublishedAt { get; set; }
}

public class ArticleCreatedEvent
{
  [JsonPropertyName("id")]
  public string ArticleId { get; set; } = string.Empty;
  
  [JsonPropertyName("authorId")]
  public string AuthorId { get; set; } = string.Empty;
  
  [JsonPropertyName("title")]
  public string Title { get; set; } = string.Empty;
  
  [JsonPropertyName("slug")]
  public string Slug { get; set; } = string.Empty;
  
  [JsonPropertyName("content")]
  public string Content { get; set; } = string.Empty;
}

public class ArticleUpdatedEvent
{
  [JsonPropertyName("id")]
  public string ArticleId { get; set; } = string.Empty;
  
  [JsonPropertyName("authorId")]
  public string AuthorId { get; set; } = string.Empty;
  
  [JsonPropertyName("title")]
  public string Title { get; set; } = string.Empty;
}

public class ArticleGetRequest
{
  [JsonPropertyName("articleId")]
  public string ArticleId { get; set; } = string.Empty;
}

public class ArticleGetResponse
{
  [JsonPropertyName("id")]
  public string? Id { get; set; }
  
  [JsonPropertyName("title")]
  public string? Title { get; set; }
  
  [JsonPropertyName("content")]
  public string? Content { get; set; }
  
  [JsonPropertyName("authorId")]
  public string? AuthorId { get; set; }
  
  [JsonPropertyName("authorName")]
  public string? AuthorName { get; set; }
  
  [JsonPropertyName("slug")]
  public string? Slug { get; set; }
  
  [JsonPropertyName("status")]
  public string? Status { get; set; }
  
  [JsonPropertyName("publishedAt")]
  public DateTime? PublishedAt { get; set; }
  
  [JsonPropertyName("createdAt")]
  public DateTime? CreatedAt { get; set; }
  
  [JsonPropertyName("error")]
  public string? Error { get; set; }
}