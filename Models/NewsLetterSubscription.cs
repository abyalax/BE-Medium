namespace Medium.Api.Models;

public class NewsLetterSubscription : Entity
{
  public string Email { get; set; } = null!;

  public bool IsActive { get; set; }

  public DateTime? SubscribedAt { get; set; }

  public DateTime? UnsubscribedAt { get; set; }
}