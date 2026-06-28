using MediatR;

using Medium.Api.Domain.Auth.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;

using RoleModel = Medium.Api.Models.Role;

namespace Medium.Api.Domain.Auth.Commands.Handlers;

public class CreateRoleCommandHandler(
    RoleQueryRepository roleQueryRepository,
    RoleStoreRepository roleStoreRepository) : IRequestHandler<CreateRoleCommand, RoleResponse>
{
  public async Task<RoleResponse> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
  {
    if (await roleQueryRepository.ExistsByNameAsync(command.Name, cancellationToken))
    {
      throw new ConflictException("Role with this name already exists");
    }

    var role = new RoleModel
    {
      Id = Guid.NewGuid(),
      Name = command.Name,
      Description = command.Description,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow
    };

    await roleStoreRepository.AddAsync(role, cancellationToken);
    await roleStoreRepository.SaveChangesAsync(cancellationToken);

    return new RoleResponse(role.Id, role.Name, role.Description);
  }
}