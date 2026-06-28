namespace Medium.Api.Domain.Article.Events;

public record ArticleUpdatedEvent(Guid ArticleId, Guid AuthorId, string Title);