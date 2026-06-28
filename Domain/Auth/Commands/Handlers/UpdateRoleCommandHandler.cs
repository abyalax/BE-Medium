using MediatR;

using Medium.Api.Domain.Auth.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;

using RoleModel = Medium.Api.Models.Role;

namespace Medium.Api.Domain.Auth.Commands.Handlers;

public class UpdateRoleCommandHandler(
    RoleQueryRepository roleQueryRepository,
    RoleStoreRepository roleStoreRepository) : IRequestHandler<UpdateRoleCommand, RoleResponse>
{
  public async Task<RoleResponse> Handle(UpdateRoleCommand command, CancellationToken cancellationToken)
  {
    var role = await roleStoreRepository.GetByIdForUpdateAsync(command.Id, cancellationToken)
        ?? throw new NotFoundException("Role not found");

    if (await roleQueryRepository.ExistsByNameAsync(command.Name, command.Id, cancellationToken))
    {
      throw new ConflictException("Role with this name already exists");
    }

    role.Name = command.Name;
    role.Description = command.Description;
    await roleStoreRepository.SaveChangesAsync(cancellationToken);

    return new RoleResponse(role.Id, role.Name, role.Description);
  }
}