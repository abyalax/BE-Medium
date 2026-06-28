using Medium.Api.Infrastructure.Database;

using FollowModel = Medium.Api.Models.Follow;

namespace Medium.Api.Domain.Follow.Repositories;

public class FollowStoreRepository(ApplicationDbContext context)
{

  public async Task AddAsync(FollowModel follow, CancellationToken cancellationToken = default)
  {
    await context.Follows.AddAsync(follow, cancellationToken);
  }

  public void Remove(FollowModel follow)
  {
    context.Follows.Remove(follow);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await context.SaveChangesAsync(cancellationToken);
  }

}