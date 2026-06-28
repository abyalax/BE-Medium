using MediatR;

using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;

using RoleModel = Medium.Api.Models.Role;

namespace Medium.Api.Domain.Auth.Commands.Handlers;

public class DeleteRoleCommandHandler(
    RoleQueryRepository roleQueryRepository,
    RoleStoreRepository roleStoreRepository) : IRequestHandler<DeleteRoleCommand, bool>
{
  public async Task<bool> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
  {
    var role = await roleQueryRepository.GetByIdAsync(command.Id, cancellationToken)
        ?? throw new NotFoundException("Role not found");

    roleStoreRepository.Remove(role);
    await roleStoreRepository.SaveChangesAsync(cancellationToken);

    return true;
  }
}