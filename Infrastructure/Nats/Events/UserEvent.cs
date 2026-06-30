using System.Text.Json.Serialization;

namespace Medium.Api.Infrastructure.Nats.Events;

public class UserRegisteredEvent
{
  [JsonPropertyName("userId")]
  public Guid UserId { get; set; }

  [JsonPropertyName("email")]
  public string Email { get; set; } = string.Empty;

  [JsonPropertyName("username")]
  public string Username { get; set; } = string.Empty;
}

public class UserLoggedInEvent
{
  [JsonPropertyName("userId")]
  public Guid UserId { get; set; }

  [JsonPropertyName("email")]
  public string Email { get; set; } = string.Empty;

  [JsonPropertyName("loginTime")]
  public DateTime LoginTime { get; set; }
}