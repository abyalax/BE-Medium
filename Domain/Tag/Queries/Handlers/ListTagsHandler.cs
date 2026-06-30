using MediatR;

using Medium.Api.Domain.Tag.Dtos;
using Medium.Api.Domain.Tag.Repositories;
using Medium.Api.Infrastructure.Cache.Services;

namespace Medium.Api.Domain.Tag.Queries.Handlers;

public class ListTagsHandler(
  TagQueryRepository tagQueryRepository,
  RedisService redisService
) : IRequestHandler<ListTagsQuery, PagedTagDto>
{
  private const int MaxPageSize = 100;
  private readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(30);

  public async Task<PagedTagDto> Handle(ListTagsQuery query, CancellationToken cancellationToken)
  {
    var page = query.Page < 1 ? 1 : query.Page;
    var pageSize = query.PageSize < 1 ? 10 : Math.Min(query.PageSize, MaxPageSize);

    var cacheKey = $"tags:list:{page}:{pageSize}";
    var cachedResponse = await redisService.GetAsync<PagedTagDto>(cacheKey, cancellationToken);

    if (cachedResponse != null) return cachedResponse;

    var totalItems = await tagQueryRepository.CountAsync(cancellationToken);
    var items = await tagQueryRepository.ListAsync(page, pageSize, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    var response = new PagedTagDto(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);

    await redisService.SetAsync(cacheKey, response, CacheExpiry, cancellationToken);
    return response;
  }

  private static TagDto ToResponse(Medium.Api.Models.Tag tag)
  {
    return new TagDto(tag.Id, tag.Name, tag.Slug);
  }
}