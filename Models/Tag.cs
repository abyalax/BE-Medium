namespace Medium.Api.Models;

public class Tag : Entity
{
  public string Name { get; set; } = null!;

  public string Slug { get; set; } = null!;

  public ICollection<ArticleTag> ArticleTags { get; set; } = [];
}