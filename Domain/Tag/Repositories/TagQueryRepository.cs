using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using TagModel = Medium.Api.Models.Tag;

namespace Medium.Api.Domain.Tag.Repositories;

public class TagQueryRepository(ApplicationDbContext context)
{
  public async Task<TagModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Tags.AsNoTracking()
        .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
  }

  public async Task<TagModel?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
  {
    return await context.Tags.AsNoTracking()
        .FirstOrDefaultAsync(t => t.Slug == slug, cancellationToken);
  }

  public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
  {
    return await context.Tags.AsNoTracking().AnyAsync(t => t.Slug == slug, cancellationToken);
  }

  public async Task<bool> ExistsBySlugAsync(string slug, Guid excludeTagId, CancellationToken cancellationToken = default)
  {
    return await context.Tags.AsNoTracking()
        .AnyAsync(t => t.Slug == slug && t.Id != excludeTagId, cancellationToken);
  }

  public async Task<bool> HasArticlesAsync(Guid tagId, CancellationToken cancellationToken = default)
  {
    return await context.ArticleTags.AsNoTracking()
        .AnyAsync(at => at.TagId == tagId, cancellationToken);
  }

  public async Task<IReadOnlyCollection<TagModel>> ListAsync(
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    return await context.Tags.AsNoTracking()
        .OrderBy(t => t.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyCollection<TagModel>> GetAllAsync(CancellationToken cancellationToken = default)
  {
    return await context.Tags.AsNoTracking()
        .OrderBy(t => t.Name)
        .ToListAsync(cancellationToken);
  }

  public async Task<int> CountAsync(CancellationToken cancellationToken = default)
  {
    return await context.Tags.AsNoTracking().CountAsync(cancellationToken);
  }

}