namespace Medium.Api.Domain.Article.Events;

public record ArticleCreatedEvent(Guid ArticleId, Guid AuthorId, string Title, string Slug);