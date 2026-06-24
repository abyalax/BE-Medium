namespace Medium.Api.Infrastructure.Nats.Services;

public static class NatsSubjects
{
  public const string ArticlePublished = "article.published";
  public const string CommentCreated = "comment.created";
  public const string UserFollowed = "user.followed";
}