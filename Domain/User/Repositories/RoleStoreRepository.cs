using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using RoleModel = Medium.Api.Models.Role;
using RolePermissionModel = Medium.Api.Models.RolePermission;

namespace Medium.Api.Domain.User.Repositories;

public class RoleStoreRepository(ApplicationDbContext context)
{
  public async Task<RoleModel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Roles.FirstOrDefaultAsync(role => role.Id == id, cancellationToken);
  }

  public async Task AddAsync(RoleModel role, CancellationToken cancellationToken = default)
  {
    await context.Roles.AddAsync(role, cancellationToken);
  }

  public void Remove(RoleModel role)
  {
    context.Roles.Remove(role);
  }

  public async Task AddRolePermissionAsync(RolePermissionModel rolePermission, CancellationToken cancellationToken = default)
  {
    await context.RolePermissions.AddAsync(rolePermission, cancellationToken);
  }

  public async Task AddRolePermissionsAsync(IEnumerable<RolePermissionModel> rolePermissions, CancellationToken cancellationToken = default)
  {
    await context.RolePermissions.AddRangeAsync(rolePermissions, cancellationToken);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await context.SaveChangesAsync(cancellationToken);
  }
}