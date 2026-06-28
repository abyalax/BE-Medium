namespace Medium.Api.Models;

using Medium.Api.Enums;

public class ObjectStorage : Entity
{
  public Guid AuthorId { get; set; }

  public Guid? ArticleId { get; set; }

  public string Bucket { get; set; } = null!;

  public string ObjectKey { get; set; } = null!;

  public string MimeType { get; set; } = null!;

  public string OriginalName { get; set; } = null!;

  public int? Size { get; set; }

  public FileAccessType AccessTypes { get; set; }

  public User Author { get; set; } = null!;

  public Article? Article { get; set; }

}