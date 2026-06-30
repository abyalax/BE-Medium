using MediatR;

using Medium.Api.Domain.Tag.Dtos;
using Medium.Api.Domain.Tag.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Exceptions;

using TagModel = Medium.Api.Models.Tag;

namespace Medium.Api.Domain.Tag.Commands.Handlers;

public class UpdateTagHandler(
  TagStoreRepository tagStoreRepository,
  TagQueryRepository tagQueryRepository,
  RedisService redisService
) : IRequestHandler<UpdateTagCommand, TagDto>
{
  private readonly string messageNotFound = "Tag not found";

  public async Task<TagDto> Handle(UpdateTagCommand command, CancellationToken cancellationToken)
  {
    var tag = await tagQueryRepository.GetByIdAsync(command.Id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    var newSlug = GenerateSlug(command.Request.Name);

    if (newSlug != tag.Slug && await tagQueryRepository.ExistsBySlugAsync(newSlug, command.Id, cancellationToken))
    {
      throw new ConflictException("A tag with this name already exists");
    }

    tag.Name = command.Request.Name;
    tag.Slug = newSlug;

    await tagStoreRepository.SaveChangesAsync(cancellationToken);

    await InvalidateTagCacheAsync(tag.Id, tag.Slug, cancellationToken);

    return ToResponse(tag);
  }

  private async Task InvalidateTagCacheAsync(Guid tagId, string slug, CancellationToken cancellationToken)
  {
    var keys = new[]
    {
      $"tag:{tagId}",
      $"tags:all"
    };
    await redisService.DeleteAsync(keys, cancellationToken);
  }

  private static string GenerateSlug(string name)
  {
    var slug = name.ToLowerInvariant();
    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s\-]", "");
    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
    slug = slug.Trim('-');
    return slug;
  }

  private static TagDto ToResponse(TagModel tag)
  {
    return new TagDto(tag.Id, tag.Name, tag.Slug);
  }
}