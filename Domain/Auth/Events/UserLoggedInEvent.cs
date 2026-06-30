using Medium.Api.Infrastructure.Events;

namespace Medium.Api.Domain.Auth.Events;

public class UserLoggedInEvent : DomainEvent
{
  public Guid UserId { get; set; }
  public string Email { get; set; } = string.Empty;
}