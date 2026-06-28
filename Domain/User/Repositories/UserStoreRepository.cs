using Medium.Api.Infrastructure.Database;

using Microsoft.EntityFrameworkCore;

using UserModel = Medium.Api.Models.User;
using UserRoleModel = Medium.Api.Models.UserRole;

namespace Medium.Api.Domain.User.Repositories;

public class UserStoreRepository(ApplicationDbContext context)
{
  public async Task<UserModel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await context.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
  }

  public async Task AddAsync(UserModel user, CancellationToken cancellationToken = default)
  {
    await context.Users.AddAsync(user, cancellationToken);
  }

  public async Task AddUserRoleAsync(UserRoleModel userRole, CancellationToken cancellationToken = default)
  {
    await context.UserRoles.AddAsync(userRole, cancellationToken);
  }

  public async Task ReplaceUserRolesAsync(Guid userId, IEnumerable<Guid> roleIds, CancellationToken cancellationToken = default)
  {
    var existingRoles = await context.UserRoles
      .Where(userRole => userRole.UserId == userId)
      .ToListAsync(cancellationToken);

    context.UserRoles.RemoveRange(existingRoles);

    await context.UserRoles.AddRangeAsync(
      roleIds.Distinct().Select(roleId => new UserRoleModel { UserId = userId, RoleId = roleId }),
      cancellationToken);
  }

  public void Remove(UserModel user)
  {
    context.Users.Remove(user);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await context.SaveChangesAsync(cancellationToken);
  }
}