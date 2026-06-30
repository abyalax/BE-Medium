using FluentValidation;

using Medium.Api.Domain.Comment.Queries;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Http.Api.Version1.Comment.Request;

public class ListCommentByUserIdRequest : AbstractValidator<ListCommentByUserIdQuery>
{
  public ListCommentByUserIdRequest()
  {
    Include(new PagedQueryValidator());
    RuleFor(x => x.UserId)
      .NotEmpty().WithMessage("User ID is required")
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("User ID must be a valid GUID");
  }
}