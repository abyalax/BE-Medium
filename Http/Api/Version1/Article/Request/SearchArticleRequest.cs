using FluentValidation;

using Medium.Api.Domain.Article.Queries;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Http.Api.Version1.Article.Request;


public class SearchArticlesQueryValidator : AbstractValidator<SearchArticlesQuery>
{
  public SearchArticlesQueryValidator()
  {
    Include(new PagedQueryValidator());
  }
}