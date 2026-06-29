using MediatR;

using Medium.Api.Domain.Bookmark.Dtos;

namespace Medium.Api.Domain.Bookmark.Queries;

public record GetBookmarByIdQuery(Guid BookmarkId) : IRequest<BookmarkDto>;