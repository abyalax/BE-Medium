using MediatR;

using Medium.Api.Domain.Article.Dtos;

namespace Medium.Api.Domain.Article.Commands;

public record UpdateArticleCommand(
    Guid ArticleId,
    Guid UserId,
    bool IsAdmin,
    string? Title = null,
    string? Content = null,
    string? Description = null,
    string? CoverImageUrl = null,
    List<Guid>? TagIds = null
) : IRequest<ArticleDto>;