using Medium.Api.Domain.User.Dtos;
using Medium.Api.Domain.User.Mapper;
using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using RoleModel = Medium.Api.Models.Role;
using UserModel = Medium.Api.Models.User;

namespace Medium.Api.Domain.User.Repositories;

public class UserQueryRepository(ApplicationDbContext context)
{
  public async Task<UserModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
  }

  public async Task<UserDto?> GetByIdWWithRolePermissionAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var user = await context.Users
      .AsNoTracking()
      .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
          .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
      .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    if (user == null) return null;
    return UserMapper.ToResponse(user);
  }

  public async Task<UserDto?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
  {
    var user = await context.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    if (user == null) return null;
    return UserMapper.ToResponse(user);
  }

  public async Task<UserDto?> FindByEmailWithRolePermissionAsync(string email, CancellationToken cancellationToken = default)
  {
    var user = await context.Users
      .AsNoTracking()
      .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
          .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
      .AsSplitQuery()
      .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    if (user == null) return null;
    return UserMapper.ToResponse(user);
  }

  public async Task<UserWithPasswordDto?> FindByEmailWithRolePermissionAndPasswordAsync(string email, CancellationToken cancellationToken = default)
  {
    var user = await context.Users
      .AsNoTracking()
      .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
          .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
      .AsSplitQuery()
      .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    if (user == null) return null;
    return UserMapper.ToResponseWithPassword(user);
  }

  public async Task<List<string>> GetPermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await context.UserRoles
      .AsNoTracking()
      .Where(ur => ur.UserId == userId)
      .Join(context.RolePermissions,
        ur => ur.RoleId,
        rp => rp.RoleId,
        (ur, rp) => rp.PermissionId)
      .Join(context.Permissions,
        pId => pId,
        p => p.Id,
        (pId, p) => p.Code)
      .Distinct()
      .ToListAsync(cancellationToken);
  }

  public async Task<bool> UserHasRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
  {
    return await context.UserRoles
      .AsNoTracking()
      .Where(ur => ur.UserId == userId)
      .Join(context.Roles,
        ur => ur.RoleId,
        r => r.Id,
        (ur, r) => r.Name)
      .AnyAsync(r => r == roleName, cancellationToken);
  }

  public async Task<IReadOnlyCollection<UserDto>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
  {
    var userEntities = await context.Users
        .AsNoTracking()
        .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .OrderBy(user => user.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return [.. userEntities.Select(UserMapper.ToResponse)];
  }

  public async Task<int> CountAsync(CancellationToken cancellationToken = default)
  {
    return await context.Users.AsNoTracking().CountAsync(cancellationToken);
  }

  public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
  {
    return await context.Users
      .AsNoTracking()
      .AnyAsync(
        user => user.Email == email && (!excludeUserId.HasValue || user.Id != excludeUserId.Value),
        cancellationToken
      );
  }

  public async Task<IReadOnlyCollection<RoleModel>> GetRolesByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
  {
    var ids = roleIds.Distinct().ToArray();
    return await context.Roles.Where(role => ids.Contains(role.Id)).ToListAsync(cancellationToken);
  }

  public async Task<RoleModel?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default)
  {
    return await context.Roles
      .AsNoTracking()
      .FirstOrDefaultAsync(
        role => role.Name == name,
        cancellationToken
      );
  }

  public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Users
      .AsNoTracking()
      .AnyAsync(user => user.Id == id, cancellationToken);
  }

  public async Task<List<UserModel>> GetActiveSubscribersByFollowAsync(int page, int pageSize, CancellationToken cancellationToken)
  {
    // Filter users who have at least one record in their Following collection
    return await context.Users
      .AsNoTracking()
      .Where(user => user.Following.Count != 0)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);
  }
}