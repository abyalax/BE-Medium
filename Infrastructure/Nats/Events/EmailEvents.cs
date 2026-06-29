using System.Text.Json.Serialization;

namespace Medium.Api.Infrastructure.Nats.Events;

public record SendWelcomeEmailRequest(
  [property: JsonPropertyName("userId")] Guid UserId,
  [property: JsonPropertyName("email")] string Email,
  [property: JsonPropertyName("username")] string Username
);

public record SendWelcomeEmailResponse(
  [property: JsonPropertyName("success")] bool Success,
  [property: JsonPropertyName("message")] string Message
);