using MediatR;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Mapper;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Pagination;
using Medium.Api.Infrastructure.Settings.Dtos;

using Microsoft.Extensions.Options;

namespace Medium.Api.Domain.Article.Queries.Handlers;

public class SearchArticlesHandler(
  ArticleQueryRepository queryRepository,
  RedisService redisService,
  IOptions<ApplicationSettings> settings
) : IRequestHandler<SearchArticlesQuery, PaginationModel<ArticleDto>>
{
  private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

  public async Task<PaginationModel<ArticleDto>> Handle(SearchArticlesQuery query, CancellationToken cancellationToken)
  {
    var page = query.Page < 1 ? 1 : query.Page;
    var pageSize = query.PerPage < 1 ? 10 : Math.Min(query.PerPage, settings.Value.Pagination.MaxPageSize);

    var cacheKey = $"articles:search:{query.Search}:{page}:{pageSize}";

    var cachedResponse = await redisService.GetAsync<PaginationModel<ArticleDto>>(cacheKey, cancellationToken);
    if (cachedResponse != null)
      return cachedResponse;

    var totalItems = await queryRepository.CountSearchPublishedAsync(query.Search, cancellationToken);
    var items = await queryRepository.SearchPublishedAsync(query.Search, page, pageSize, cancellationToken);
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