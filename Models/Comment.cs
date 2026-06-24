namespace Medium.Api.Models;

public class Comment : Entity
{
  public Guid UserId { get; set; }

  public Guid ArticleId { get; set; }

  public string Content { get; set; } = null!;

  public User User { get; set; } = null!;

  public Article Article { get; set; } = null!;
}