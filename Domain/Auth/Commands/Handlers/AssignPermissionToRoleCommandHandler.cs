using MediatR;

using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Auth.Commands.Handlers;

public class AssignPermissionToRoleCommandHandler(
    RoleQueryRepository roleQueryRepository,
    PermissionQueryRepository permissionQueryRepository,
    RoleStoreRepository roleStoreRepository) : IRequestHandler<AssignPermissionToRoleCommand>
{
  public async Task Handle(AssignPermissionToRoleCommand command, CancellationToken cancellationToken)
  {
    var roleExists = await roleQueryRepository.ExistsByIdAsync(command.RoleId, cancellationToken);
    var permissionExists = await permissionQueryRepository.ExistsByIdAsync(command.PermissionId, cancellationToken);

    if (!roleExists || !permissionExists)
      throw new NotFoundException("Role or permission not found");

    // TODO: add a method to check for existing assignments in the repositories

    var rolePermission = new Models.RolePermission
    {
      RoleId = command.RoleId,
      PermissionId = command.PermissionId
    };

    await roleStoreRepository.AddRolePermissionAsync(rolePermission, cancellationToken);
    await roleStoreRepository.SaveChangesAsync(cancellationToken);
  }
}