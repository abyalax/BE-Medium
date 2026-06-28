using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Domain.Article.Queries;

public record GetTrendingArticlesQuery() : PagedQuery<PaginationModel<ArticleDto>>;