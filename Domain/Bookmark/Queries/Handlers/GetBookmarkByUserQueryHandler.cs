using MediatR;

using Medium.Api.Domain.Bookmark.Dtos;
using Medium.Api.Domain.Bookmark.Mapper;
using Medium.Api.Domain.Bookmark.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Domain.Bookmark.Queries.Handlers;

public class GetBookmarkByUserQueryHandler(
    BookmarkQueryRepository queryRepository,
    RedisService redisService
) : IRequestHandler<GetBookmarkByUserQuery, PaginationModel<BookmarkDto>>
{
  private const int MaxPageSize = 100;
  private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

  public async Task<PaginationModel<BookmarkDto>> Handle(GetBookmarkByUserQuery query, CancellationToken cancellationToken)
  {
    var page = query.Page < 1 ? 1 : query.Page;
    var pageSize = query.PerPage < 1 ? 10 : Math.Min(query.PerPage, MaxPageSize);

    var cacheKey = $"bookmarks:list:{page}:{pageSize}:{query.UserId}:{query.Search}:{query.SortBy}";

    var cachedResponse = await redisService.GetAsync<PaginationModel<BookmarkDto>>(cacheKey, cancellationToken);
    if (cachedResponse != null) return cachedResponse;

    var totalItems = await queryRepository.CountByUserAsync(query.UserId, cancellationToken);
    var items = await queryRepository.GetByUserAsync(query.UserId, page, pageSize, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    var response = new PaginationModel<BookmarkDto>
    {
      Data = [.. items.Select(BookmarkMapper.ToResponse)],
      Meta = new PaginationMeta
      {
        CurrentPage = page,
        PerPage = pageSize,
        TotalCount = totalItems,
        TotalPages = totalPages
      }
    };

    await redisService.SetAsync(cacheKey, response, CacheExpiry, cancellationToken);
    return response;
  }
}