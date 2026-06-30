using MediatR;

using Medium.Api.Domain.ReadingHistory.Dtos;
using Medium.Api.Domain.ReadingHistory.Mapper;
using Medium.Api.Domain.ReadingHistory.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Pagination;
using Medium.Api.Infrastructure.Settings.Dtos;

using Microsoft.Extensions.Options;

namespace Medium.Api.Domain.ReadingHistory.Queries.Handlers;

public class ListReadingHistoryByUserHandler(
  ReadingHistoryQueryRepository queryRepository,
  RedisService redisService,
  IOptions<ApplicationSettings> settings
) : IRequestHandler<ListReadingHistoryByUserQuery, PaginationModel<ReadingHistoryDto>>
{
  private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

  public async Task<PaginationModel<ReadingHistoryDto>> Handle(ListReadingHistoryByUserQuery query, CancellationToken cancellationToken)
  {
    var page = query.Page < 1 ? 1 : query.Page;
    var pageSize = query.PerPage < 1 ? 10 : Math.Min(query.PerPage, settings.Value.Pagination.MaxPageSize);

    var cacheKey = $"readingHistory:list:{page}:{pageSize}:{query.UserId}:{query.Search}:{query.SortBy}";

    var cachedResponse = await redisService.GetAsync<PaginationModel<ReadingHistoryDto>>(cacheKey, cancellationToken);
    if (cachedResponse != null) return cachedResponse;

    var totalItems = await queryRepository.CountByUserAsync(query.UserId, cancellationToken);
    var items = await queryRepository.ListAsync(
      query.UserId,
      page,
      pageSize,
      query.Search,
      query.SortBy,
      cancellationToken
    );

    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    var response = new PaginationModel<ReadingHistoryDto>
    {
      Data = [.. items.Select(ReadingHistoryMapper.ToResponse)],
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