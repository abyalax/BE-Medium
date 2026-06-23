namespace Medium.Api.Models;

public class ReadingHistory : Entity
{
    public Guid UserId { get; set; }

    public Guid ArticleId { get; set; }

    public int DurationSeconds { get; set; }

    public DateTime ReadAt { get; set; }

    public User User { get; set; } = null!;

    public Article Article { get; set; } = null!;
}