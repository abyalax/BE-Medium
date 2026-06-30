using MediatR;

using Medium.Api.Domain.Bookmark.Dtos;

namespace Medium.Api.Domain.Bookmark.Commands;

public record CreateBookmarkCommand(
  Guid UserId,
  Guid ArticleId
) : IRequest<BookmarkDto>;