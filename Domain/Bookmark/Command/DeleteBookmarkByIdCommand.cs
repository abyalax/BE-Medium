using MediatR;

namespace Medium.Api.Domain.Bookmark.Command;

public record DeleteBookmarkByIdCommand(
  Guid UserId,
  Guid BookmarkId
) : IRequest;