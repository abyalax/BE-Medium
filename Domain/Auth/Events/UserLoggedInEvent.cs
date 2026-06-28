using Medium.Api.Infrastructure.Events;

namespace Medium.Api.Domain.Auth.Events;

public record UserLoggedInEvent(Guid UserId, string Email) : DomainEvent;