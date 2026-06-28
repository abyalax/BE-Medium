namespace Medium.Api.Infrastructure.Events;

public abstract record DomainEvent
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}