using FluentValidation;

using Medium.Api.Domain.Article.Queries;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Http.Api.Version1.Article.Request;

public class ListArticlesQueryRequest : AbstractValidator<ListArticlesQuery>
{
  public ListArticlesQueryRequest()
  {
    Include(new PagedQueryValidator());
    RuleFor(x => x.AuthorId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .When(x => x.AuthorId.HasValue)
      .WithMessage("Author ID must be a valid GUID");

    RuleFor(x => x.TagSlug)
      .MaximumLength(100).WithMessage("Tag slug cannot exceed 100 characters")
      .Matches(@"^[a-z0-9-]+$").WithMessage("Tag slug can only contain lowercase letters, numbers, and hyphens")
      .When(x => !string.IsNullOrEmpty(x.TagSlug));

    RuleFor(x => x.Status)
      .IsInEnum().WithMessage("Invalid article status")
      .When(x => x.Status.HasValue);
  }
}