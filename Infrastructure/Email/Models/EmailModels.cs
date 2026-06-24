namespace Medium.Api.Infrastructure.Email.Models;

public record ArticlePublishedEmailModel(
    string FollowerName,
    string AuthorName,
    string ArticleTitle,
    string ArticlePreview,
    string ArticleUrl
);

public record CommentCreatedEmailModel(
    string AuthorName,
    string CommenterName,
    string ArticleTitle,
    string CommentContent,
    string CommentUrl
);

public record UserFollowedEmailModel(
    string FollowingName,
    string FollowerName,
    string FollowerProfileUrl,
    string FollowBackUrl
);

public record NewsletterArticleItem(
    string Title,
    string AuthorName,
    string Url
);

public record NewsletterEmailModel(
    string WeekStartDate,
    string WeekEndDate,
    int NewArticlesCount,
    int NewCommentsCount,
    int NewFollowersCount,
    List<NewsletterArticleItem> TopArticles,
    string SiteUrl
);