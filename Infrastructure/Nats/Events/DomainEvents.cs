using System.Text.Json.Serialization;

using Medium.Api.Infrastructure.Events;

namespace Medium.Api.Infrastructure.Nats.Events;

public record UserRegisteredEvent(
    [property: JsonPropertyName("userId")] Guid UserId,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("username")] string Username
) : DomainEvent;

public record UserLoggedInEvent(
    [property: JsonPropertyName("userId")] Guid UserId,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("loginTime")] DateTime LoginTime
) : DomainEvent;

public record SendWelcomeEmailRequest(
    [property: JsonPropertyName("userId")] Guid UserId,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("username")] string Username
);

public record SendWelcomeEmailResponse(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("message")] string Message
);