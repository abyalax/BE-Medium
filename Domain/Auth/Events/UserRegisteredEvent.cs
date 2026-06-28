using Medium.Api.Infrastructure.Events;

namespace Medium.Api.Domain.Auth.Events;

public record UserRegisteredEvent(Guid UserId, string Email, string Name) : DomainEvent;