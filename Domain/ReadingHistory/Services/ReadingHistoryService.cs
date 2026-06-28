// TODO: remove this layer service and migrate it to CQRS Pattern
using Medium.Api.Domain.ReadingHistory.Dtos;
using Medium.Api.Domain.ReadingHistory.Repositories;
using Medium.Api.Infrastructure.Exceptions;

using ReadingHistoryModel = Medium.Api.Models.ReadingHistory;

namespace Medium.Api.Domain.ReadingHistory.Services;

public class ReadingHistoryService(ReadingHistoryStoreRepository readingHistoryStoreRepository, ReadingHistoryQueryRepository readingHistoryQueryRepository)
{
  private const int MaxPageSize = 100;
  private readonly ReadingHistoryStoreRepository _readingHistoryStoreRepository = readingHistoryStoreRepository;
  private readonly ReadingHistoryQueryRepository _readingHistoryQueryRepository = readingHistoryQueryRepository;
  private readonly string messageNotFound = "Reading history not found";

  public async Task<ReadingHistoryResponse> CreateAsync(
      Guid userId,
      CreateReadingHistoryRequest request,
      CancellationToken cancellationToken = default)
  {
    var readingHistory = new ReadingHistoryModel
    {
      Id = Guid.NewGuid(),
      UserId = userId,
      ArticleId = request.ArticleId,
      DurationSeconds = request.DurationSeconds,
      ReadAt = DateTime.UtcNow
    };

    await _readingHistoryStoreRepository.AddAsync(readingHistory, cancellationToken);
    await _readingHistoryStoreRepository.SaveChangesAsync(cancellationToken);

    return await GetByIdAsync(readingHistory.Id, cancellationToken);
  }

  public async Task<ReadingHistoryResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var history = await _readingHistoryQueryRepository.GetReadingHistoryWithArticleAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    return ToResponse(history);
  }

  public async Task<PagedReadingHistoryResponse> GetByUserAsync(
      Guid userId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

    var totalItems = await _readingHistoryQueryRepository.CountByUserAsync(userId, cancellationToken);
    var items = await _readingHistoryQueryRepository.GetByUserAsync(userId, page, pageSize, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    return new PagedReadingHistoryResponse(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);
  }

  public async Task<IReadOnlyCollection<ReadingHistoryResponse>> GetRecentByUserAsync(
      Guid userId,
      int limit,
      CancellationToken cancellationToken = default)
  {
    var items = await _readingHistoryQueryRepository.GetRecentByUserAsync(userId, limit, cancellationToken);
    return items.Select(ToResponse).ToList();
  }

  public async Task DeleteAsync(
      Guid id,
      Guid currentUserId,
      CancellationToken cancellationToken = default)
  {
    var history = await _readingHistoryQueryRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (history.UserId != currentUserId)
    {
      throw new ForbiddenException("You can only delete your own reading history");
    }

    _readingHistoryStoreRepository.Remove(history);
    await _readingHistoryStoreRepository.SaveChangesAsync(cancellationToken);
  }

  private static ReadingHistoryResponse ToResponse(ReadingHistoryWithArticleData history)
  {
    return new ReadingHistoryResponse(
        history.Id,
        history.UserId,
        history.ArticleId,
        history.ArticleTitle,
        history.ArticleSlug,
        history.DurationSeconds,
        history.ReadAt);
  }

  private static ReadingHistoryResponse ToResponse(ReadingHistoryModel history)
  {
    return new ReadingHistoryResponse(
        history.Id,
        history.UserId,
        history.ArticleId,
        history.Article.Title,
        history.Article.Slug,
        history.DurationSeconds,
        history.ReadAt);
  }
}