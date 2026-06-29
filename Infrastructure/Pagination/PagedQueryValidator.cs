using FluentValidation;

namespace Medium.Api.Infrastructure.Pagination;

public class PagedQueryValidator : AbstractValidator<PagedQuery>
{
  public PagedQueryValidator()
  {
    RuleFor(x => x.Page)
      .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1");

    RuleFor(x => x.PerPage)
      .GreaterThan(0).WithMessage("PerPage must be greater than 0")
      .LessThanOrEqualTo(100).WithMessage("PerPage cannot exceed 100");

    RuleFor(x => x.Order)
      .Must(order => order.Equals("asc", StringComparison.OrdinalIgnoreCase) ||
        order.Equals("desc", StringComparison.OrdinalIgnoreCase))
      .WithMessage("Order must be either 'asc' or 'desc'");

    RuleFor(x => x.SortBy)
      .MaximumLength(50).WithMessage("SortBy cannot exceed 50 characters")
      .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("SortBy can only contain letters, numbers, and underscores")
      .When(x => !string.IsNullOrEmpty(x.SortBy));

    RuleFor(x => x.Search)
      .MaximumLength(200).WithMessage("Search term cannot exceed 200 characters")
      .When(x => !string.IsNullOrEmpty(x.Search));
  }
}