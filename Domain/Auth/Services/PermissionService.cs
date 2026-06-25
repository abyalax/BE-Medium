using Medium.Api.Domain.Auth.DTOs;
using Medium.Api.Infrastructure.Database;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace Medium.Api.Domain.Auth.Services;

public class PermissionService(ApplicationDbContext context)
{
  private readonly ApplicationDbContext _context = context;

  public async Task<PermissionResponse> CreatePermissionAsync(string code, string name, string description, CancellationToken cancellationToken = default)
  {
    var existingPermission = await _context.Permissions
        .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);

    if (existingPermission != null)
    {
      throw new ConflictException("Permission with this code already exists");
    }

    var permission = new Permission
    {
      Id = Guid.NewGuid(),
      Code = code,
      Name = name,
      Description = description,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow
    };

    _context.Permissions.Add(permission);
    await _context.SaveChangesAsync(cancellationToken);

    return new PermissionResponse(permission.Id, permission.Code, permission.Name, permission.Description);
  }

  public async Task<IEnumerable<PermissionResponse>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Permissions
        .AsNoTracking()
        .Select(p => new PermissionResponse(p.Id, p.Code, p.Name, p.Description))
        .ToListAsync(cancellationToken);
  }

  public async Task<PermissionResponse> GetPermissionByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var permission = await _context.Permissions
        .AsNoTracking()
        .Where(p => p.Id == id)
        .Select(p => new PermissionResponse(p.Id, p.Code, p.Name, p.Description))
        .FirstOrDefaultAsync(cancellationToken);

    return permission ?? throw new NotFoundException("Permission not found");
  }

  public async Task<PermissionResponse> UpdatePermissionAsync(Guid id, string code, string name, string description, CancellationToken cancellationToken = default)
  {
    var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
        ?? throw new NotFoundException("Permission not found");

    var duplicatePermission = await _context.Permissions
        .AnyAsync(p => p.Id != id && p.Code == code, cancellationToken);

    if (duplicatePermission)
    {
      throw new ConflictException("Permission with this code already exists");
    }

    permission.Code = code;
    permission.Name = name;
    permission.Description = description;
    await _context.SaveChangesAsync(cancellationToken);

    return new PermissionResponse(permission.Id, permission.Code, permission.Name, permission.Description);
  }

  public async Task DeletePermissionAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
        ?? throw new NotFoundException("Permission not found");

    _context.Permissions.Remove(permission);
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task<IEnumerable<PermissionResponse>> GetPermissionsByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
  {
    return await _context.RolePermissions
        .AsNoTracking()
        .Where(rp => rp.RoleId == roleId)
        .Join(
            _context.Permissions,
            rp => rp.PermissionId,
            p => p.Id,
            (rp, p) => new PermissionResponse(p.Id, p.Code, p.Name, p.Description)
        )
        .ToListAsync(cancellationToken);
  }

  public async Task<IEnumerable<PermissionResponse>> GetPermissionsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    return await _context.UserRoles
        .AsNoTracking()
        .Where(ur => ur.UserId == userId)
        .Join(
            _context.RolePermissions,
            ur => ur.RoleId,
            rp => rp.RoleId,
            (ur, rp) => rp.PermissionId
        )
        .Join(
            _context.Permissions,
            pId => pId,
            p => p.Id,
            (pId, p) => new PermissionResponse(p.Id, p.Code, p.Name, p.Description)
        )
        .Distinct()
        .ToListAsync(cancellationToken);
  }
}