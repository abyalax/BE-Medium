using FluentValidation;

using Medium.Api.Domain.Bookmark.Dtos;

namespace Medium.Api.Domain.Bookmark.Dtos;

public class BookmarkRequestValidator : AbstractValidator<BookmarkRequest>
{
  public BookmarkRequestValidator()
  {
    RuleFor(x => x.ArticleId)
        .NotEmpty().WithMessage("Article ID is required");
  }
}