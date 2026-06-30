using MediatR;

using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Auth.Commands.Handlers;

public class AssignRoleToUserHandler(
    UserQueryRepository userQueryRepository,
    RoleQueryRepository roleQueryRepository,
    UserStoreRepository userStoreRepository) : IRequestHandler<AssignRoleToUserCommand>
{
  public async Task Handle(AssignRoleToUserCommand command, CancellationToken cancellationToken)
  {
    var userExists = await userQueryRepository.ExistsByIdAsync(command.UserId, cancellationToken);
    var roleExists = await roleQueryRepository.ExistsByIdAsync(command.RoleId, cancellationToken);

    if (!userExists || !roleExists)
      throw new NotFoundException("User or role not found");

    // TODO: add a method to check for existing assignments in the repositories
    var userRole = new Models.UserRole
    {
      UserId = command.UserId,
      RoleId = command.RoleId
    };

    await userStoreRepository.AddUserRoleAsync(userRole, cancellationToken);
    await userStoreRepository.SaveChangesAsync(cancellationToken);
  }
}