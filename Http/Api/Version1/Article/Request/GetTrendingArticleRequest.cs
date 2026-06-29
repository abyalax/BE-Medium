using FluentValidation;

using Medium.Api.Domain.Article.Queries;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Http.Api.Version1.Article.Request;

public class GetTrendingArticlesRequest : AbstractValidator<GetPopularArticlesQuery>
{
  public GetTrendingArticlesRequest()
  {
    Include(new PagedQueryValidator());
  }
}