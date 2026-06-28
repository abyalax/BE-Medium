using MediatR;

using Medium.Api.Domain.Article.Dtos;

namespace Medium.Api.Domain.Article.Commands;

public record ArchiveArticleCommand(Guid ArticleId, Guid UserId, bool IsAdmin) : IRequest<ArticleDto>;