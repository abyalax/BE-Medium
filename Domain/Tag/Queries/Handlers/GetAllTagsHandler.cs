using MediatR;

using Medium.Api.Domain.Tag.Dtos;
using Medium.Api.Domain.Tag.Repositories;
using Medium.Api.Infrastructure.Cache.Services;

namespace Medium.Api.Domain.Tag.Queries.Handlers;

public class GetAllTagsHandler(
  TagQueryRepository tagQueryRepository,
  RedisService redisService
) : IRequestHandler<GetAllTagsQuery, IReadOnlyCollection<TagDto>>
{
  private readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(30);

  public async Task<IReadOnlyCollection<TagDto>> Handle(GetAllTagsQuery query, CancellationToken cancellationToken)
  {
    var cacheKey = "tags:all";
    var cachedTags = await redisService.GetAsync<IReadOnlyCollection<TagDto>>(cacheKey, cancellationToken);

    if (cachedTags != null)
    {
      return cachedTags;
    }

    var tags = await tagQueryRepository.GetAllAsync(cancellationToken);
    var response = tags.Select(ToResponse).ToList();

    await redisService.SetAsync(cacheKey, response, CacheExpiry, cancellationToken);
    return response;
  }

  private static TagDto ToResponse(Medium.Api.Models.Tag tag)
  {
    return new TagDto(tag.Id, tag.Name, tag.Slug);
  }
}