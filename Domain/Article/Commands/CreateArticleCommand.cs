using MediatR;

using Medium.Api.Domain.Article.Dtos;

namespace Medium.Api.Domain.Article.Commands;

public record CreateArticleCommand(
    Guid AuthorId,
    string Title,
    string Content,
    string? CoverImageUrl = null,
    List<Guid>? TagIds = null
) : IRequest<ArticleDto>;