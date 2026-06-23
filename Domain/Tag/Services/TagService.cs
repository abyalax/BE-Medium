using Medium.Api.Domain.Tag.Dtos;
using Medium.Api.Domain.Tag.Repositories;
using Medium.Api.Infrastructure.Exceptions;
using TagModel = Medium.Api.Models.Tag;

namespace Medium.Api.Domain.Tag.Services;

public class TagService
{
    private const int MaxPageSize = 100;
    private readonly TagRepository _tagRepository;
    private readonly string messageNotFound = "Tag not found";

    public TagService(TagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<TagResponse> CreateAsync(CreateTagRequest request, CancellationToken cancellationToken = default)
    {
        var slug = GenerateSlug(request.Name);

        if (await _tagRepository.ExistsBySlugAsync(slug, cancellationToken))
        {
            throw new ConflictException("A tag with this name already exists");
        }

        var tag = new TagModel
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Slug = slug
        };

        await _tagRepository.AddAsync(tag, cancellationToken);
        await _tagRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(tag);
    }

    public async Task<TagResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(messageNotFound);

        return ToResponse(tag);
    }

    public async Task<PagedTagResponse> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

        var totalItems = await _tagRepository.CountAsync(cancellationToken);
        var items = await _tagRepository.ListAsync(page, pageSize, cancellationToken);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedTagResponse(
            items.Select(ToResponse).ToList(),
            page,
            pageSize,
            totalItems,
            totalPages);
    }

    public async Task<IReadOnlyCollection<TagResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.GetAllAsync(cancellationToken);
        return tags.Select(ToResponse).ToList();
    }

    public async Task<TagResponse> UpdateAsync(Guid id, UpdateTagRequest request, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(messageNotFound);

        var newSlug = GenerateSlug(request.Name);

        if (newSlug != tag.Slug && await _tagRepository.ExistsBySlugAsync(newSlug, id, cancellationToken))
        {
            throw new ConflictException("A tag with this name already exists");
        }

        tag.Name = request.Name;
        tag.Slug = newSlug;

        await _tagRepository.SaveChangesAsync(cancellationToken);

        return ToResponse(tag);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(messageNotFound);

        if (await _tagRepository.HasArticlesAsync(id, cancellationToken))
        {
            throw new BadRequestException("Cannot delete tag that is used by articles");
        }

        _tagRepository.Remove(tag);
        await _tagRepository.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = slug.Trim('-');
        return slug;
    }

    private static TagResponse ToResponse(TagModel tag)
    {
        return new TagResponse(
            tag.Id,
            tag.Name,
            tag.Slug,
            tag.CreatedAt,
            tag.UpdatedAt);
    }
}
