using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Domain.Article.Queries;

public record SearchArticlesQuery(
  string SearchTerm
) : PagedQuery<PaginationModel<ArticleDto>>;