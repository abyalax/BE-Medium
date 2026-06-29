using MediatR;

using Medium.Api.Domain.Bookmark.Dtos;

namespace Medium.Api.Domain.Bookmark.Command;

public record CreateBookmarkCommand(
    Guid UserId,
    Guid ArticleId
) : IRequest<BookmarkDto>;