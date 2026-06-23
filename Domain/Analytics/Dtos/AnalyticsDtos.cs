namespace Medium.Api.Domain.Analytics.Dtos;

public record AuthorAnalyticsResponse(
    Guid AuthorId,
    string AuthorName,
    int TotalArticles,
    long TotalViews,
    int TotalComments,
    int TotalBookmarks,
    int TotalFollowers,
    IReadOnlyCollection<ArticleStatsResponse> MostViewedArticles,
    IReadOnlyCollection<ArticleStatsResponse> MostBookmarkedArticles);

public record ArticleStatsResponse(
    Guid ArticleId,
    string ArticleTitle,
    string ArticleSlug,
    long ViewCount,
    int BookmarkCount,
    int CommentCount);

public record PlatformAnalyticsResponse(
    int TotalUsers,
    int TotalArticles,
    int TotalPublishedArticles,
    int TotalComments,
    int TotalTags,
    int TotalBookmarks,
    int TotalFollows,
    int TotalReadingHistory);
