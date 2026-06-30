using FluentValidation;

using Medium.Api.Domain.Comment.Queries;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Http.Api.Version1.Comment.Request;

public class ListCommentByArticleIdRequest : AbstractValidator<ListCommentByArticleIdQuery>
{
  public ListCommentByArticleIdRequest()
  {
    Include(new PagedQueryValidator());
    RuleFor(x => x.ArticleId)
      .NotEmpty().WithMessage("Article ID is required")
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Article ID must be a valid GUID");
  }
}