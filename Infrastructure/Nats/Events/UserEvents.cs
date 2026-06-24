using System.Text.Json.Serialization;

namespace Medium.Api.Infrastructure.Nats.Events;

public record UserFollowedEvent(
    [property: JsonPropertyName("followerId")] string FollowerId,
    [property: JsonPropertyName("followingId")] string FollowingId,
    [property: JsonPropertyName("followedAt")] DateTime FollowedAt
);