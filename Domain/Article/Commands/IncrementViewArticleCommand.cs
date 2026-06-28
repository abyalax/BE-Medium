using MediatR;

namespace Medium.Api.Domain.Article.Commands;

public record IncrementViewArticleCommand(Guid ArticleId, Guid UserId, bool IsAdmin) : IRequest;