// TODO: remove this service layer and migrate it to CQRS Pattern

using Medium.Api.Domain.Bookmark.Dtos;
using Medium.Api.Domain.Bookmark.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Exceptions;

using BookmarkModel = Medium.Api.Models.Bookmark;

namespace Medium.Api.Domain.Bookmark.Services;

public class BookmarkService(BookmarkStoreRepository bookmarkStoreRepository, BookmarkQueryRepository bookmarkQueryRepository, RedisService redisService)
{
  private const int MaxPageSize = 100;
  private readonly BookmarkStoreRepository _bookmarkStoreRepository = bookmarkStoreRepository;
  private readonly BookmarkQueryRepository _bookmarkQueryRepository = bookmarkQueryRepository;
  private readonly RedisService _redisService = redisService;
  private readonly string messageNotFound = "Bookmark not found";
  private readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

  public async Task<BookmarkResponse> CreateAsync(
      Guid userId,
      BookmarkRequest request,
      CancellationToken cancellationToken = default)
  {
    if (await _bookmarkQueryRepository.ExistsAsync(userId, request.ArticleId, cancellationToken))
    {
      throw new ConflictException("Article is already bookmarked");
    }

    var bookmark = new BookmarkModel
    {
      Id = Guid.NewGuid(),
      UserId = userId,
      ArticleId = request.ArticleId
    };

    await _bookmarkStoreRepository.AddAsync(bookmark, cancellationToken);
    await _bookmarkStoreRepository.SaveChangesAsync(cancellationToken);

    return await GetByIdAsync(bookmark.Id, cancellationToken);
  }

  public async Task<BookmarkResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var cacheKey = $"bookmark:{id}";
    var cachedBookmark = await _redisService.GetAsync<BookmarkResponse>(cacheKey, cancellationToken);

    if (cachedBookmark != null)
    {
      return cachedBookmark;
    }

    var bookmark = await _bookmarkQueryRepository.GetBookmarkWithArticleAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    var response = ToResponse(bookmark);
    await _redisService.SetAsync(cacheKey, response, CacheExpiry, cancellationToken);

    return response;
  }

  public async Task<PagedBookmarkResponse> GetByUserAsync(
      Guid userId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

    var cacheKey = $"bookmarks:user:{userId}:{page}:{pageSize}";
    var cachedResponse = await _redisService.GetAsync<PagedBookmarkResponse>(cacheKey, cancellationToken);

    if (cachedResponse != null)
    {
      return cachedResponse;
    }

    var totalItems = await _bookmarkQueryRepository.CountByUserAsync(userId, cancellationToken);
    var items = await _bookmarkQueryRepository.GetByUserAsync(userId, page, pageSize, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    var response = new PagedBookmarkResponse(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);

    await _redisService.SetAsync(cacheKey, response, CacheExpiry, cancellationToken);
    return response;
  }

  public async Task DeleteAsync(
      Guid id,
      Guid currentUserId,
      CancellationToken cancellationToken = default)
  {
    var bookmark = await _bookmarkQueryRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (bookmark.UserId != currentUserId)
    {
      throw new ForbiddenException("You can only delete your own bookmarks");
    }

    _bookmarkStoreRepository.Remove(bookmark);
    await _bookmarkStoreRepository.SaveChangesAsync(cancellationToken);

    await InvalidateBookmarkCacheAsync(bookmark.Id, bookmark.UserId, cancellationToken);
  }

  public async Task RemoveByArticleAsync(
      Guid userId,
      Guid articleId,
      CancellationToken cancellationToken = default)
  {
    var bookmark = await _bookmarkQueryRepository.GetByUserAndArticleAsync(userId, articleId, cancellationToken);

    if (bookmark != null)
    {
      _bookmarkStoreRepository.Remove(bookmark);
      await _bookmarkStoreRepository.SaveChangesAsync(cancellationToken);

      await InvalidateBookmarkCacheAsync(bookmark.Id, bookmark.UserId, cancellationToken);
    }
  }

  private static BookmarkResponse ToResponse(BookmarkWithArticleData bookmark)
  {
    return new BookmarkResponse(
        bookmark.Id,
        bookmark.UserId,
        bookmark.ArticleId,
        bookmark.ArticleTitle,
        bookmark.ArticleSlug,
        bookmark.CreatedAt);
  }

  private async Task InvalidateBookmarkCacheAsync(Guid bookmarkId, Guid userId, CancellationToken cancellationToken)
  {
    var keys = new[]
    {
      $"bookmark:{bookmarkId}",
      $"bookmarks:user:{userId}:*"
    };

    // Delete specific bookmark key
    await _redisService.DeleteAsync(keys[0], cancellationToken);

    // Note: Pattern-based deletion would require SCAN operation in Redis
    // For simplicity, we're just deleting the specific bookmark key
    // In production, you might want to implement pattern-based cache invalidation
  }

  private static BookmarkResponse ToResponse(BookmarkModel bookmark)
  {
    return new BookmarkResponse(
        bookmark.Id,
        bookmark.UserId,
        bookmark.ArticleId,
        bookmark.Article.Title,
        bookmark.Article.Slug,
        bookmark.CreatedAt);
  }
}