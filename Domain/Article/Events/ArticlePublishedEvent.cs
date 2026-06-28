namespace Medium.Api.Domain.Article.Events;

public record ArticlePublishedEvent(Guid ArticleId, Guid AuthorId, string Title, string Slug);