using MediatR;

using Medium.Api.Domain.Article.Dtos;

namespace Medium.Api.Domain.Article.Commands;

public record PublishArticleCommand(Guid ArticleId, Guid UserId, bool IsAdmin, DateTime? ScheduledAt = null) : IRequest<ArticleDto>;