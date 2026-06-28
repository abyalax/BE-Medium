// TODO: remove this layer service and migrate it to CQRS Pattern
using Medium.Api.Domain.Tag.Dtos;
using Medium.Api.Domain.Tag.Repositories;
using Medium.Api.Infrastructure.Cache.Services;
using Medium.Api.Infrastructure.Exceptions;

using TagModel = Medium.Api.Models.Tag;

namespace Medium.Api.Domain.Tag.Services;

public class TagService(TagStoreRepository tagStoreRepository, TagQueryRepository tagQueryRepository, RedisService redisService)
{
  private const int MaxPageSize = 100;
  private readonly TagStoreRepository _tagStoreRepository = tagStoreRepository;
  private readonly TagQueryRepository _tagQueryRepository = tagQueryRepository;
  private readonly RedisService _redisService = redisService;
  private readonly string messageNotFound = "Tag not found";
  private readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(30);

  public async Task<TagDto> CreateAsync(CreateTagRequest request, CancellationToken cancellationToken = default)
  {
    var slug = GenerateSlug(request.Name);

    if (await _tagQueryRepository.ExistsBySlugAsync(slug, cancellationToken))
    {
      throw new ConflictException("A tag with this name already exists");
    }

    var tag = new TagModel
    {
      Id = Guid.NewGuid(),
      Name = request.Name,
      Slug = slug
    };

    await _tagStoreRepository.AddAsync(tag, cancellationToken);
    await _tagStoreRepository.SaveChangesAsync(cancellationToken);

    return ToResponse(tag);
  }

  public async Task<TagDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var cacheKey = $"tag:{id}";
    var cachedTag = await _redisService.GetAsync<TagDto>(cacheKey, cancellationToken);

    if (cachedTag != null)
    {
      return cachedTag;
    }

    var tag = await _tagQueryRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    var response = ToResponse(tag);
    await _redisService.SetAsync(cacheKey, response, CacheExpiry, cancellationToken);

    return response;
  }

  public async Task<PagedTagDto> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

    var cacheKey = $"tags:list:{page}:{pageSize}";
    var cachedResponse = await _redisService.GetAsync<PagedTagDto>(cacheKey, cancellationToken);

    if (cachedResponse != null)
    {
      return cachedResponse;
    }

    var totalItems = await _tagQueryRepository.CountAsync(cancellationToken);
    var items = await _tagQueryRepository.ListAsync(page, pageSize, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    var response = new PagedTagDto(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);

    await _redisService.SetAsync(cacheKey, response, CacheExpiry, cancellationToken);
    return response;
  }

  public async Task<IReadOnlyCollection<TagDto>> GetAllAsync(CancellationToken cancellationToken = default)
  {
    var cacheKey = "tags:all";
    var cachedTags = await _redisService.GetAsync<IReadOnlyCollection<TagDto>>(cacheKey, cancellationToken);

    if (cachedTags != null)
    {
      return cachedTags;
    }

    var tags = await _tagQueryRepository.GetAllAsync(cancellationToken);
    var response = tags.Select(ToResponse).ToList();

    await _redisService.SetAsync(cacheKey, response, CacheExpiry, cancellationToken);
    return response;
  }

  public async Task<TagDto> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken = default)
  {
    var tag = await _tagQueryRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    var newSlug = GenerateSlug(request.Name);

    if (newSlug != tag.Slug && await _tagQueryRepository.ExistsBySlugAsync(newSlug, id, cancellationToken))
    {
      throw new ConflictException("A tag with this name already exists");
    }

    tag.Name = request.Name;
    tag.Slug = newSlug;

    await _tagStoreRepository.SaveChangesAsync(cancellationToken);

    return ToResponse(tag);
  }

  public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var tag = await _tagQueryRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (await _tagQueryRepository.HasArticlesAsync(id, cancellationToken))
    {
      throw new BadRequestException("Cannot delete tag that is used by articles");
    }

    _tagStoreRepository.Remove(tag);
    await _tagStoreRepository.SaveChangesAsync(cancellationToken);

    await InvalidateTagCacheAsync(tag.Id, tag.Slug, cancellationToken);
  }

  private async Task InvalidateTagCacheAsync(Guid tagId, string slug, CancellationToken cancellationToken)
  {
    var keys = new[]
    {
      $"tag:{tagId}",
      $"tags:all"
    };
    await _redisService.DeleteAsync(keys, cancellationToken);
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