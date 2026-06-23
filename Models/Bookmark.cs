namespace Medium.Api.Models;

public class Bookmark : Entity
{
    public Guid UserId { get; set; }

    public Guid ArticleId { get; set; }

    public User User { get; set; } = null!;

    public Article Article { get; set; } = null!;
}