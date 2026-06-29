using MediatR;

using Medium.Api.Domain.Bookmark.Dtos;
using Medium.Api.Domain.Bookmark.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Bookmark.Queries.Handlers;

public class GetBookmarkByIdQueryHandler(
  BookmarkQueryRepository queryRepository,
  RedisService redisService
) : IRequestHandler<GetBookmarByIdQuery, BookmarkDto>
{

  private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);

  public async Task<BookmarkDto> Handle(
      GetBookmarByIdQuery query,
      CancellationToken cancellationToken
  )
  {
    var cacheKey = $"bookmark:{query.BookmarkId}";
    var cachedBookmark = await redisService.GetAsync<BookmarkDto>(cacheKey, cancellationToken);

    if (cachedBookmark != null)
    {
      return cachedBookmark;
    }

    var bookmark = await queryRepository.GetBookmarkWithArticleAsync(query.BookmarkId, cancellationToken)
        ?? throw new NotFoundException("Bookmark not found");

    await redisService.SetAsync(cacheKey, bookmark, CacheExpiry, cancellationToken);
    return bookmark;
  }
}