using Microsoft.EntityFrameworkCore;
using Medium.Api.Infrastructure.Database;
using TagModel = Medium.Api.Models.Tag;

namespace Medium.Api.Domain.Tag.Repositories;

public class TagRepository
{
    private readonly ApplicationDbContext _context;

    public TagRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TagModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<TagModel?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tags.AnyAsync(t => t.Slug == slug, cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, Guid excludeTagId, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .AnyAsync(t => t.Slug == slug && t.Id != excludeTagId, cancellationToken);
    }

    public async Task<bool> HasArticlesAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return await _context.ArticleTags
            .AnyAsync(at => at.TagId == tagId, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TagModel>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<TagModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tags.CountAsync(cancellationToken);
    }

    public async Task AddAsync(TagModel tag, CancellationToken cancellationToken = default)
    {
        await _context.Tags.AddAsync(tag, cancellationToken);
    }

    public void Remove(TagModel tag)
    {
        _context.Tags.Remove(tag);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
