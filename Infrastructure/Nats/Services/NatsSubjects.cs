namespace Medium.Api.Infrastructure.Nats.Services;

public static class NatsSubjects
{
  public const string ArticlePublished = "article.published";
  public const string ArticleCreated = "article.created";
  public const string ArticleGet = "article.get";
  public const string CommentCreated = "comment.created";
  public const string UserFollowed = "user.followed";
}