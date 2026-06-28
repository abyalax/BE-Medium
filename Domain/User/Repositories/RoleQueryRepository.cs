using Medium.Api.Domain.Auth.Dtos;
using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using RoleModel = Medium.Api.Models.Role;

namespace Medium.Api.Domain.User.Repositories;

public class RoleQueryRepository(ApplicationDbContext context)
{
  public async Task<RoleModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Roles.AsNoTracking().FirstOrDefaultAsync(role => role.Id == id, cancellationToken);
  }

  public async Task<RoleModel?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
  {
    return await context.Roles.AsNoTracking().FirstOrDefaultAsync(role => role.Name == name, cancellationToken);
  }

  public async Task<IEnumerable<RoleResponse>> GetAllAsync(CancellationToken cancellationToken = default)
  {
    return await context.Roles
        .AsNoTracking()
        .Select(r => new RoleResponse(r.Id, r.Name, r.Description))
        .ToListAsync(cancellationToken);
  }

  public async Task<RoleResponse?> GetByIdWithResponseAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Roles
        .AsNoTracking()
        .Where(r => r.Id == id)
        .Select(r => new RoleResponse(r.Id, r.Name, r.Description))
        .FirstOrDefaultAsync(cancellationToken);
  }

  public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
  {
    return await context.Roles.AnyAsync(r => r.Name == name, cancellationToken);
  }

  public async Task<bool> ExistsByNameAsync(string name, Guid excludeId, CancellationToken cancellationToken = default)
  {
    return await context.Roles.AnyAsync(r => r.Id != excludeId && r.Name == name, cancellationToken);
  }

  public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Roles.AnyAsync(r => r.Id == id, cancellationToken);
  }
}