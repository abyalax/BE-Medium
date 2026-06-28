using MediatR;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Mapper;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Domain.Article.Queries.Handlers;

public class GetPopularArticlesQueryHandler(
  ArticleQueryRepository queryRepository,
  RedisService redisService
) : IRequestHandler<GetPopularArticlesQuery, PaginationModel<ArticleDto>>
{
  private const int MaxPageSize = 100;
  private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

  public async Task<PaginationModel<ArticleDto>> Handle(GetPopularArticlesQuery query, CancellationToken cancellationToken)
  {
    var page = query.Page < 1 ? 1 : query.Page;
    var pageSize = query.PerPage < 1 ? 10 : Math.Min(query.PerPage, MaxPageSize);

    var cacheKey = $"articles:popular:{page}:{pageSize}";

    var cachedResponse = await redisService.GetAsync<PaginationModel<ArticleDto>>(cacheKey, cancellationToken);
    if (cachedResponse != null) return cachedResponse;

    var items = await queryRepository.GetPopularAsync(page, pageSize, cancellationToken);
    var totalItems = await queryRepository.CountPublishedAsync(cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    var response = new PaginationModel<ArticleDto>
    {
      Data = [.. items.Select(ArticleMapper.ToResponse)],
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