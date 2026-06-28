using MediatR;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Article.Queries.Handlers;

public class GetArticleByIdQueryHandler(
    ArticleQueryRepository queryRepository,
    RedisService redisService
) : IRequestHandler<GetArticleByIdQuery, ArticleDto>
{
  private readonly ArticleQueryRepository _queryRepository = queryRepository;
  private readonly RedisService _redisService = redisService;

  private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(10);

  public async Task<ArticleDto> Handle(
      GetArticleByIdQuery query,
      CancellationToken cancellationToken
  )
  {
    var cacheKey = $"article:{query.ArticleId}";

    var cached = await _redisService.GetAsync<ArticleDto>(
      cacheKey,
      cancellationToken
    );

    if (cached is not null) return cached;

    var article = await _queryRepository.GetArticleWithAuthorTagsAsync(
        query.ArticleId,
        cancellationToken
    ) ?? throw new NotFoundException("Article not found");

    await _redisService.SetAsync(
      cacheKey,
      article,
      CacheExpiry,
      cancellationToken
    );

    return article;
  }
}