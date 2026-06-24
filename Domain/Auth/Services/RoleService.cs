using Medium.Api.Domain.Auth.DTOs;
using Medium.Api.Infrastructure.Database;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace Medium.Api.Domain.Auth.Services;

public class RoleService
{
  private readonly ApplicationDbContext _context;

  public RoleService(ApplicationDbContext context)
  {
    _context = context;
  }

  public async Task<RoleResponse> CreateRoleAsync(string name, string description, CancellationToken cancellationToken = default)
  {
    var existingRole = await _context.Roles
        .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

    if (existingRole != null)
    {
      throw new ConflictException("Role with this name already exists");
    }

    var role = new Role
    {
      Id = Guid.NewGuid(),
      Name = name,
      Description = description,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow
    };

    _context.Roles.Add(role);
    await _context.SaveChangesAsync(cancellationToken);

    return new RoleResponse(role.Id, role.Name, role.Description);
  }

  public async Task<IEnumerable<RoleResponse>> GetAllRolesAsync(CancellationToken cancellationToken = default)
  {
    return await _context.Roles
        .AsNoTracking()
        .Select(r => new RoleResponse(r.Id, r.Name, r.Description))
        .ToListAsync(cancellationToken);
  }

  public async Task<RoleResponse> GetRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var role = await _context.Roles
        .AsNoTracking()
        .Where(r => r.Id == id)
        .Select(r => new RoleResponse(r.Id, r.Name, r.Description))
        .FirstOrDefaultAsync(cancellationToken);

    return role ?? throw new NotFoundException("Role not found");
  }

  public async Task<RoleResponse> UpdateRoleAsync(Guid id, string name, string description, CancellationToken cancellationToken = default)
  {
    var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
        ?? throw new NotFoundException("Role not found");

    var duplicateRole = await _context.Roles
        .AnyAsync(r => r.Id != id && r.Name == name, cancellationToken);

    if (duplicateRole)
    {
      throw new ConflictException("Role with this name already exists");
    }

    role.Name = name;
    role.Description = description;
    await _context.SaveChangesAsync(cancellationToken);

    return new RoleResponse(role.Id, role.Name, role.Description);
  }

  public async Task DeleteRoleAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken)
        ?? throw new NotFoundException("Role not found");

    _context.Roles.Remove(role);
    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task AssignPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
  {
    var roleExists = await _context.Roles.AnyAsync(role => role.Id == roleId, cancellationToken);
    var permissionExists = await _context.Permissions.AnyAsync(permission => permission.Id == permissionId, cancellationToken);
    if (!roleExists || !permissionExists)
    {
      throw new NotFoundException("Role or permission not found");
    }

    var existingAssignment = await _context.RolePermissions
        .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

    if (existingAssignment != null)
    {
      throw new ConflictException("Permission already assigned to this role");
    }

    _context.RolePermissions.Add(new RolePermission
    {
      RoleId = roleId,
      PermissionId = permissionId
    });

    await _context.SaveChangesAsync(cancellationToken);
  }

  public async Task AssignRoleToUserAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
  {
    var userExists = await _context.Users.AnyAsync(user => user.Id == userId, cancellationToken);
    var roleExists = await _context.Roles.AnyAsync(role => role.Id == roleId, cancellationToken);
    if (!userExists || !roleExists)
    {
      throw new NotFoundException("User or role not found");
    }

    var existingAssignment = await _context.UserRoles
        .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);

    if (existingAssignment != null)
    {
      throw new ConflictException("User already has this role");
    }

    _context.UserRoles.Add(new UserRole
    {
      UserId = userId,
      RoleId = roleId
    });

    await _context.SaveChangesAsync(cancellationToken);
  }
}