using Medium.Api.Domain.Follow.Dtos;
using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using FollowModel = Medium.Api.Models.Follow;

namespace Medium.Api.Domain.Follow.Repositories;

public class FollowQueryRepository(ApplicationDbContext context)
{
  public async Task<FollowModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Follows.AsNoTracking()
      .Include(f => f.Follower)
      .Include(f => f.Following)
      .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
  }

  public async Task<FollowModel?> GetByFollowerAndFollowingAsync(
    Guid followerId,
    Guid followingId,
    CancellationToken cancellationToken = default
  )
  {
    return await context.Follows.AsNoTracking()
        .Include(f => f.Follower)
        .Include(f => f.Following)
        .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId, cancellationToken);
  }

  public async Task<IReadOnlyCollection<FollowModel>> GetFollowersAsync(
    Guid userId,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default
  )
  {
    return await context.Follows.AsNoTracking()
      .Include(f => f.Follower)
      .Include(f => f.Following)
      .Where(f => f.FollowingId == userId)
      .OrderByDescending(f => f.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);
  }

  public async Task<IReadOnlyCollection<FollowModel>> GetFollowingAsync(
      Guid userId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    return await context.Follows.AsNoTracking()
      .Include(f => f.Follower)
      .Include(f => f.Following)
      .Where(f => f.FollowerId == userId)
      .OrderByDescending(f => f.CreatedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);
  }

  public async Task<int> CountFollowersAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await context.Follows.AsNoTracking().CountAsync(f => f.FollowingId == userId, cancellationToken);
  }

  public async Task<int> CountFollowingAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await context.Follows.AsNoTracking().CountAsync(f => f.FollowerId == userId, cancellationToken);
  }

  public async Task<bool> ExistsAsync(Guid followerId, Guid followingId, CancellationToken cancellationToken = default)
  {
    return await context.Follows.AsNoTracking()
      .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId, cancellationToken);
  }

  public async Task<FollowWithUsersData?> GetFollowWithUsersAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var follow = await context.Follows.AsNoTracking()
      .Include(f => f.Follower)
      .Include(f => f.Following)
      .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    if (follow == null) return null;

    return new FollowWithUsersData(
      follow.Id,
      follow.FollowerId,
      follow.Follower.Name,
      follow.FollowingId,
      follow.Following.Name,
      follow.CreatedAt
    );
  }

  public async Task<int> GetTotalFollowsCountAsync(CancellationToken cancellationToken = default)
  {
    return await context.Follows.AsNoTracking().CountAsync(cancellationToken);
  }
}