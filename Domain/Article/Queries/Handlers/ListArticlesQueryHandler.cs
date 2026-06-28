using MediatR;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Mapper;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Pagination;

namespace Medium.Api.Domain.Article.Queries.Handlers;

public class ListArticlesQueryHandler(
    ArticleQueryRepository queryRepository,
    RedisService redisService
) : IRequestHandler<ListArticlesQuery, PaginationModel<ArticleDto>>
{
  private const int MaxPageSize = 100;
  private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

  public async Task<PaginationModel<ArticleDto>> Handle(ListArticlesQuery query, CancellationToken cancellationToken)
  {
    var page = query.Page < 1 ? 1 : query.Page;
    var pageSize = query.PerPage < 1 ? 10 : Math.Min(query.PerPage, MaxPageSize);

    var cacheKey = $"articles:list:{page}:{pageSize}:{query.AuthorId}:{query.TagSlug}:{query.Search}:{query.Status}:{query.SortBy}";

    var cachedResponse = await redisService.GetAsync<PaginationModel<ArticleDto>>(cacheKey, cancellationToken);
    if (cachedResponse != null)
    {
      return cachedResponse;
    }

    var totalItems = query.AuthorId.HasValue
        ? await queryRepository.CountByAuthorAsync(query.AuthorId.Value, cancellationToken)
        : await queryRepository.CountAsync(cancellationToken);

    var items = await queryRepository.ListAsync(
        page,
        pageSize,
        query.AuthorId,
        query.TagSlug,
        query.Search,
        query.Status,
        query.SortBy,
        cancellationToken);

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