namespace Medium.Api.Domain.User.Queries.Handlers;

using MediatR;

using Medium.Api.Domain.User.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Pagination;

public class ListUserHandler(
    UserQueryRepository queryRepository,
    RedisService redisService
) : IRequestHandler<ListUserQuery, PaginationModel<UserDto>>
{
  private const int MaxPageSize = 100;
  private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

  public async Task<PaginationModel<UserDto>> Handle(ListUserQuery query, CancellationToken cancellationToken)
  {
    var page = query.Page < 1 ? 1 : query.Page;
    var pageSize = query.PerPage < 1 ? 10 : Math.Min(query.PerPage, MaxPageSize);

    var cacheKey = $"users:list:{page}:{pageSize}:{query.Search}:{query.SortBy}";

    var cachedResponse = await redisService.GetAsync<PaginationModel<UserDto>>(cacheKey, cancellationToken);
    if (cachedResponse != null) return cachedResponse;

    var totalItems = await queryRepository.CountAsync(cancellationToken);
    var items = await queryRepository.ListAsync(page, pageSize, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    var response = new PaginationModel<UserDto>
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