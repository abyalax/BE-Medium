using MediatR;

namespace Medium.Api.Domain.Bookmark.Commands;

public record DeleteBookmarkByIdCommand(
  Guid UserId,
  Guid BookmarkId
) : IRequest;