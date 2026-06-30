using System.Text.Json.Serialization;

namespace Medium.Api.Infrastructure.Nats.Events;

public class SendWelcomeEmailRequest
{
  [JsonPropertyName("userId")]
  public Guid UserId { get; set; }

  [JsonPropertyName("email")]
  public string Email { get; set; } = string.Empty;

  [JsonPropertyName("username")]
  public string Username { get; set; } = string.Empty;
}

public class SendWelcomeEmailResponse
{
  [JsonPropertyName("success")]
  public bool Success { get; set; }

  [JsonPropertyName("message")]
  public string Message { get; set; } = string.Empty;
}