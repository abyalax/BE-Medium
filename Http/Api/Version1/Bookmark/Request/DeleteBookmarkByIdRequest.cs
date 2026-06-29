using FluentValidation;

using Medium.Api.Domain.Bookmark.Command;

namespace Medium.Api.Http.Api.Version1.Bookmark.Request;

public class DeleteBookmarkByIdRequest : AbstractValidator<DeleteBookmarkByIdCommand>
{
  public DeleteBookmarkByIdRequest()
  {
    RuleFor(x => x.BookmarkId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("Bookmark ID must be valid GUID");

    RuleFor(x => x.UserId)
      .Must(guid => Guid.TryParse(guid.ToString(), out _))
      .WithMessage("User ID must be valid GUID");

  }
}