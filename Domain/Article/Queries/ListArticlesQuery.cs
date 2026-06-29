using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Domain.Article.Queries;

public record ListArticlesQuery(
    Guid? AuthorId = null,
    string? TagSlug = null,
    Enums.ArticleStatus? Status = null
) : PagedQuery<PaginationModel<ArticleDto>>;