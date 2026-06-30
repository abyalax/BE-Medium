using MediatR;

using Medium.Api.Domain.Tag.Dtos;
using Medium.Api.Domain.Tag.Repositories;
using Medium.Api.Infrastructure.Exceptions;

using TagModel = Medium.Api.Models.Tag;

namespace Medium.Api.Domain.Tag.Commands.Handlers;

public class CreateTagHandler(
  TagStoreRepository tagStoreRepository,
  TagQueryRepository tagQueryRepository
) : IRequestHandler<CreateTagCommand, TagDto>
{
  public async Task<TagDto> Handle(CreateTagCommand command, CancellationToken cancellationToken)
  {
    var slug = GenerateSlug(command.Request.Name);

    if (await tagQueryRepository.ExistsBySlugAsync(slug, cancellationToken))
    {
      throw new ConflictException("A tag with this name already exists");
    }

    var tag = new TagModel
    {
      Id = Guid.NewGuid(),
      Name = command.Request.Name,
      Slug = slug
    };

    await tagStoreRepository.AddAsync(tag, cancellationToken);
    await tagStoreRepository.SaveChangesAsync(cancellationToken);

    return ToResponse(tag);
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