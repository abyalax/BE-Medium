namespace Medium.Api.Domain.Notification.Queries.Handlers;

using MediatR;

using Medium.Api.Domain.Notification.Dtos;
using Medium.Api.Domain.Notification.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Pagination;

public class ListNotificationQueryHandler(
    NotificationQueryRepository queryRepository,
    RedisService redisService
) : IRequestHandler<ListUserNotificationQuery, PaginationModel<NotificationDto>>
{
  private const int MaxPageSize = 100;
  private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

  public async Task<PaginationModel<NotificationDto>> Handle(ListUserNotificationQuery query, CancellationToken cancellationToken)
  {
    var page = query.Page < 1 ? 1 : query.Page;
    var pageSize = query.PerPage < 1 ? 10 : Math.Min(query.PerPage, MaxPageSize);

    var cacheKey = $"Notifications:list:{page}:{pageSize}:{query.Search}:{query.SortBy}";

    var cachedResponse = await redisService.GetAsync<PaginationModel<NotificationDto>>(cacheKey, cancellationToken);
    if (cachedResponse != null) return cachedResponse;

    var totalItems = await queryRepository.CountAsync(query.UserId, query.Search, cancellationToken);
    var items = await queryRepository.ListAsync(query.UserId, page, pageSize, query.Search, query.SortBy, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    var response = new PaginationModel<NotificationDto>
    {
      Data = [.. items],
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