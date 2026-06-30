using FluentValidation;

using Medium.Api.Domain.Bookmark.Commands;

namespace Medium.Api.Http.Api.Version1.Bookmark.Request;

public class DeleteBookmarkByIdValidator : AbstractValidator<DeleteBookmarkByIdCommand>
{
  public DeleteBookmarkByIdValidator()
  {
    RuleFor(x => x.BookmarkId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Bookmark ID must be valid GUID");

    RuleFor(x => x.UserId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("User ID must be valid GUID");

  }
}