using System.Text.Json.Serialization;

namespace Medium.Api.Infrastructure.Nats.Events;

public class UserFollowedEvent
{
  [JsonPropertyName("followerId")]
  public string FollowerId { get; set; } = string.Empty;
  
  [JsonPropertyName("followingId")]
  public string FollowingId { get; set; } = string.Empty;
  
  [JsonPropertyName("followedAt")]
  public DateTime FollowedAt { get; set; }
}