using Medium.Api.Domain.Bookmark.Dtos;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Domain.Bookmark.Queries;

public record GetBookmarkByUserQuery(
    Guid UserId
) : PagedQuery<PaginationModel<BookmarkDto>>;