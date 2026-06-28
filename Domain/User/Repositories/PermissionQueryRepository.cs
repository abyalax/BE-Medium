using Medium.Api.Domain.Auth.Dtos;
using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using PermissionModel = Medium.Api.Models.Permission;

namespace Medium.Api.Domain.User.Repositories;

public class PermissionQueryRepository(ApplicationDbContext context)
{
  public async Task<PermissionModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Permissions.AsNoTracking().FirstOrDefaultAsync(permission => permission.Id == id, cancellationToken);
  }

  public async Task<IEnumerable<PermissionResponse>> GetAllAsync(CancellationToken cancellationToken = default)
  {
    return await context.Permissions
        .AsNoTracking()
        .Select(p => new PermissionResponse(p.Id, p.Code, p.Name, p.Description))
        .ToListAsync(cancellationToken);
  }

  public async Task<PermissionResponse?> GetByIdWithResponseAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Permissions
        .AsNoTracking()
        .Where(p => p.Id == id)
        .Select(p => new PermissionResponse(p.Id, p.Code, p.Name, p.Description))
        .FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<IEnumerable<PermissionResponse>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
  {
    return await context.RolePermissions
        .AsNoTracking()
        .Where(rp => rp.RoleId == roleId)
        .Join(
            context.Permissions,
            rp => rp.PermissionId,
            p => p.Id,
            (rp, p) => new PermissionResponse(p.Id, p.Code, p.Name, p.Description)
        )
        .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<PermissionResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await context.UserRoles
        .AsNoTracking()
        .Where(ur => ur.UserId == userId)
        .Join(
            context.RolePermissions,
            ur => ur.RoleId,
            rp => rp.RoleId,
            (ur, rp) => rp.PermissionId
        )
        .Join(
            context.Permissions,
            pId => pId,
            p => p.Id,
            (pId, p) => new PermissionResponse(p.Id, p.Code, p.Name, p.Description)
        )
        .Distinct()
        .ToListAsync(cancellationToken);
  }

  public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Permissions.AnyAsync(p => p.Id == id, cancellationToken);
  }
}