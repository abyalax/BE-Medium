using MediatR;

namespace Medium.Api.Domain.Article.Commands;

public record DeleteArticleCommand(Guid ArticleId, Guid UserId, bool IsAdmin) : IRequest<bool>;