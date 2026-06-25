using Medium.Api.Domain.Bookmark.Dtos;
using Medium.Api.Domain.Bookmark.Repositories;
using Medium.Api.Infrastructure.Exceptions;

using BookmarkModel = Medium.Api.Models.Bookmark;

namespace Medium.Api.Domain.Bookmark.Services;

public class BookmarkService(BookmarkRepository bookmarkRepository)
{
  private const int MaxPageSize = 100;
  private readonly BookmarkRepository _bookmarkRepository = bookmarkRepository;
  private readonly string messageNotFound = "Bookmark not found";

  public async Task<BookmarkResponse> CreateAsync(
      Guid userId,
      BookmarkRequest request,
      CancellationToken cancellationToken = default)
  {
    if (await _bookmarkRepository.ExistsAsync(userId, request.ArticleId, cancellationToken))
    {
      throw new ConflictException("Article is already bookmarked");
    }

    var bookmark = new BookmarkModel
    {
      Id = Guid.NewGuid(),
      UserId = userId,
      ArticleId = request.ArticleId
    };

    await _bookmarkRepository.AddAsync(bookmark, cancellationToken);
    await _bookmarkRepository.SaveChangesAsync(cancellationToken);

    return await GetByIdAsync(bookmark.Id, cancellationToken);
  }

  public async Task<BookmarkResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var bookmark = await _bookmarkRepository.GetBookmarkWithArticleAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    return ToResponse(bookmark);
  }

  public async Task<PagedBookmarkResponse> GetByUserAsync(
      Guid userId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

    var totalItems = await _bookmarkRepository.CountByUserAsync(userId, cancellationToken);
    var items = await _bookmarkRepository.GetByUserAsync(userId, page, pageSize, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    return new PagedBookmarkResponse(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);
  }

  public async Task DeleteAsync(
      Guid id,
      Guid currentUserId,
      CancellationToken cancellationToken = default)
  {
    var bookmark = await _bookmarkRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (bookmark.UserId != currentUserId)
    {
      throw new ForbiddenException("You can only delete your own bookmarks");
    }

    _bookmarkRepository.Remove(bookmark);
    await _bookmarkRepository.SaveChangesAsync(cancellationToken);
  }

  public async Task RemoveByArticleAsync(
      Guid userId,
      Guid articleId,
      CancellationToken cancellationToken = default)
  {
    var bookmark = await _bookmarkRepository.GetByUserAndArticleAsync(userId, articleId, cancellationToken);

    if (bookmark != null)
    {
      _bookmarkRepository.Remove(bookmark);
      await _bookmarkRepository.SaveChangesAsync(cancellationToken);
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