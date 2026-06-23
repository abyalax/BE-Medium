namespace Medium.Api.Models;

using Medium.Api.Enums;

public class Article : Entity
{
    public Guid AuthorId { get; set; }

    public string Title { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? CoverImageUrl { get; set; }

    public ArticleStatus Status { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public long ViewCount { get; set; }

    // Navigation
    public User Author { get; set; } = null!;

    public ICollection<ArticleTag> ArticleTags { get; set; } = [];

    public ICollection<Comment> Comments { get; set; } = [];

    public ICollection<Bookmark> Bookmarks { get; set; } = [];

    public ICollection<ReadingHistory> ReadingHistories { get; set; } = [];
}