using Medium.Api.Domain.Auth.DTOs;
using Medium.Api.Domain.User.Dtos;
using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using RoleModel = Medium.Api.Models.Role;
using UserModel = Medium.Api.Models.User;
using UserRoleModel = Medium.Api.Models.UserRole;

namespace Medium.Api.Domain.User.Repositories;

public class UserRepository
{
  private readonly ApplicationDbContext _context;

  public UserRepository(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task AddAsync(UserModel user, CancellationToken cancellationToken = default)
  {
    await _context.Users.AddAsync(user, cancellationToken);
  }

  public async Task<UserModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _context.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
  }

  public async Task<UserWithRolesPermissionsData?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
  {
    return await FindByEmailWithRolePermissionAsync(email, cancellationToken);
  }

  public async Task<UserWithRolesPermissionsData?> FindByEmailWithRolePermissionAsync(string email, CancellationToken cancellationToken = default)
  {
    var users = await _context.Users
        .AsNoTracking()
        .Where(user => user.Email == email)
        .Select(user => new UserProjection(
            user.Id,
            user.Name,
            user.Email,
            user.Password,
            user.Bio,
            user.AvatarUrl,
            user.CreatedAt,
            user.UpdatedAt))
        .ToListAsync(cancellationToken);

    return (await MapDataAsync(users, cancellationToken)).SingleOrDefault();
  }

  public async Task<IReadOnlyCollection<UserWithRolesPermissionsData>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
  {
    var users = await _context.Users
        .AsNoTracking()
        .OrderBy(user => user.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(user => new UserProjection(
            user.Id,
            user.Name,
            user.Email,
            user.Password,
            user.Bio,
            user.AvatarUrl,
            user.CreatedAt,
            user.UpdatedAt))
        .ToListAsync(cancellationToken);

    return await MapDataAsync(users, cancellationToken);
  }

  public async Task<int> CountAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Users.AsNoTracking().CountAsync(cancellationToken);
  }

  public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
  {
    return await _context.Users.AnyAsync(
        user => user.Email == email && (!excludeUserId.HasValue || user.Id != excludeUserId.Value),
        cancellationToken);
  }

  public async Task<IReadOnlyCollection<RoleModel>> GetRolesByIdsAsync(IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
  {
    var ids = roleIds.Distinct().ToArray();
    return await _context.Roles.Where(role => ids.Contains(role.Id)).ToListAsync(cancellationToken);
  }

  public async Task<RoleModel?> GetRoleByNameAsync(string name, CancellationToken cancellationToken = default)
  {
    return await _context.Roles.FirstOrDefaultAsync(role => role.Name == name, cancellationToken);
  }

  public async Task ReplaceUserRolesAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
  {
    var existingRoles = await _context.UserRoles
        .Where(userRole => userRole.UserId == userId)
        .ToListAsync(cancellationToken);

    _context.UserRoles.RemoveRange(existingRoles);

    await _context.UserRoles.AddRangeAsync(
        roleIds.Distinct().Select(roleId => new UserRoleModel { UserId = userId, RoleId = roleId }),
        cancellationToken);
  }

  public void Remove(UserModel user)
  {
    _context.Users.Remove(user);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await _context.SaveChangesAsync(cancellationToken);
  }

  private async Task<IReadOnlyCollection<UserWithRolesPermissionsData>> MapDataAsync(
      IReadOnlyCollection<UserProjection> userRows,
      CancellationToken cancellationToken)
  {
    var userIds = userRows.Select(x => x.Id).ToArray();
    if (userIds.Length == 0)
      return [];

    var roleRows = await _context.UserRoles
        .AsNoTracking()
        .Where(ur => userIds.Contains(ur.UserId))
        .Join(_context.Roles.AsNoTracking(),
            ur => ur.RoleId,
            r => r.Id,
            (ur, r) => new
            {
              ur.UserId,
              Role = new RoleProjection(r.Id, r.Name, r.Description)
            })
        .ToListAsync(cancellationToken);

    var permissionRows = await _context.UserRoles
        .AsNoTracking()
        .Where(ur => userIds.Contains(ur.UserId))
        .Join(_context.RolePermissions.AsNoTracking(),
            ur => ur.RoleId,
            rp => rp.RoleId,
            (ur, rp) => new { ur.UserId, rp.RoleId, rp.PermissionId })
        .Join(_context.Permissions.AsNoTracking(),
            x => x.PermissionId,
            p => p.Id,
            (x, p) => new
            {
              x.UserId,
              x.RoleId,
              Permission = new PermissionResponse(
                    p.Id,
                    p.Code,
                    p.Name,
                    p.Description)
            })
        .Distinct()
        .ToListAsync(cancellationToken);

    var permissionsByUserRole = permissionRows
        .GroupBy(x => (x.UserId, x.RoleId))
        .ToDictionary(
            g => g.Key,
            g => g.Select(x => x.Permission).ToArray());

    var rolesByUser = roleRows
        .GroupBy(x => x.UserId)
        .ToDictionary(
            g => g.Key,
            g => g
                .GroupBy(x => x.Role.Id)
                .Select(rg =>
                {
                  var key = (UserId: g.Key, RoleId: rg.Key);

                  permissionsByUserRole.TryGetValue(key, out var permissions);

                  return new RoleWithPermissionsResponse(
                          rg.Key,
                          rg.First().Role.Name,
                          rg.First().Role.Description,
                          permissions ?? []);
                })
                .ToArray() as IReadOnlyCollection<RoleWithPermissionsResponse>
        );

    return userRows
        .Select(user => new UserWithRolesPermissionsData(
            user.Id,
            user.Name,
            user.Email,
            user.Password,
            user.Bio,
            user.AvatarUrl,
            user.CreatedAt,
            user.UpdatedAt,
            rolesByUser.GetValueOrDefault(user.Id, [])))
        .ToArray();
  }

  private sealed record UserProjection(
      Guid Id,
      string Name,
      string Email,
      string Password,
      string? Bio,
      string? AvatarUrl,
      DateTime CreatedAt,
      DateTime UpdatedAt);

  private sealed record RoleProjection(
      Guid Id,
      string Name,
      string Description);
}