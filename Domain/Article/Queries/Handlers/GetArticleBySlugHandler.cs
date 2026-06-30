using MediatR;

using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Article.Mapper;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Article.Queries.Handlers;

public class GetArticleBySlugHandler(
  ArticleQueryRepository queryRepository,
  RedisService redisService
) : IRequestHandler<GetArticleBySlugQuery, ArticleDto>
{
  private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

  public async Task<ArticleDto> Handle(GetArticleBySlugQuery query, CancellationToken cancellationToken)
  {
    var cacheKey = $"article:slug:{query.Slug}";
    var cachedArticle = await redisService.GetAsync<ArticleDto>(cacheKey, cancellationToken);

    if (cachedArticle != null) return cachedArticle;

    var article = await queryRepository.GetBySlugAsync(query.Slug, cancellationToken)
        ?? throw new NotFoundException("Article not found");

    var response = ArticleMapper.ToResponse(article);

    await redisService.SetAsync(cacheKey, response, CacheExpiry, cancellationToken);
    return response;
  }
}