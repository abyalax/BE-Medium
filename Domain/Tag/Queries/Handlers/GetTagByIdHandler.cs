using MediatR;

using Medium.Api.Domain.Tag.Dtos;
using Medium.Api.Domain.Tag.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Tag.Queries.Handlers;

public class GetTagByIdHandler(
  TagQueryRepository tagQueryRepository,
  RedisService redisService
) : IRequestHandler<GetTagByIdQuery, TagDto>
{
  private readonly string messageNotFound = "Tag not found";
  private readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(30);

  public async Task<TagDto> Handle(GetTagByIdQuery query, CancellationToken cancellationToken)
  {
    var cacheKey = $"tag:{query.Id}";
    var cachedTag = await redisService.GetAsync<TagDto>(cacheKey, cancellationToken);

    if (cachedTag != null)
    {
      return cachedTag;
    }

    var tag = await tagQueryRepository.GetByIdAsync(query.Id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    var response = ToResponse(tag);
    await redisService.SetAsync(cacheKey, response, CacheExpiry, cancellationToken);

    return response;
  }

  private static TagDto ToResponse(Medium.Api.Models.Tag tag)
  {
    return new TagDto(tag.Id, tag.Name, tag.Slug);
  }
}