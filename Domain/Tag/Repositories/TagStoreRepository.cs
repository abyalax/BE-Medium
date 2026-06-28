using Medium.Api.Infrastructure.Database;

using TagModel = Medium.Api.Models.Tag;

namespace Medium.Api.Domain.Tag.Repositories;

public class TagStoreRepository(ApplicationDbContext context)
{

  public async Task AddAsync(TagModel tag, CancellationToken cancellationToken = default)
  {
    await context.Tags.AddAsync(tag, cancellationToken);
  }

  public void Remove(TagModel tag)
  {
    context.Tags.Remove(tag);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await context.SaveChangesAsync(cancellationToken);
  }
}