using MediatR;

using Medium.Api.Domain.Comment.Dtos;
using Medium.Api.Domain.Comment.Mapper;
using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Pagination;
using Medium.Api.Infrastructure.Settings.Dtos;

using Microsoft.Extensions.Options;

namespace Medium.Api.Domain.Comment.Queries.Handlers;

public class ListCommentByUserIdHandler(
  CommentQueryRepository queryRepository,
  RedisService redisService,
  IOptions<ApplicationSettings> settings
) : IRequestHandler<ListCommentByUserIdQuery, PaginationModel<CommentDto>>
{

  private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);

  public async Task<PaginationModel<CommentDto>> Handle(
    ListCommentByUserIdQuery query,
    CancellationToken cancellationToken
  )
  {

    var page = query.Page < 1 ? 1 : query.Page;
    var pageSize = query.PerPage < 1 ? 10 : Math.Min(query.PerPage, settings.Value.Pagination.MaxPageSize);

    var cacheKey = $"comments:list:{page}:{pageSize}:{query.Search}:{query.SortBy}";
    var cachedComment = await redisService.GetAsync<PaginationModel<CommentDto>>(cacheKey, cancellationToken);

    if (cachedComment != null) return cachedComment;

    var totalItems = await queryRepository.CountByUserAsync(query.UserId, cancellationToken);

    var items = await queryRepository.ListByUserAsync(
      query.UserId,
      page,
      pageSize,
      query.Search,
      query.SortBy,
      cancellationToken
    );

    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    var response = new PaginationModel<CommentDto>
    {
      Data = [.. items.Select(CommentMapper.ToResponse)],
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