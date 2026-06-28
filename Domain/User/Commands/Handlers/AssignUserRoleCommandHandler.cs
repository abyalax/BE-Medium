using MediatR;

using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.User.Commands.Handlers;

public class AssignUserRoleCommandHandler(
    UserStoreRepository userStoreRepository,
    UserQueryRepository userQueryRepository
) : IRequestHandler<AssignUserRoleCommand>
{
  public async Task Handle(AssignUserRoleCommand command, CancellationToken cancellationToken)
  {
    _ = await userQueryRepository.GetByIdAsync(command.UserId, cancellationToken)
        ?? throw new NotFoundException("User not found");

    if (command.RoleIds.Count == 0)
      throw new BadRequestException("At least one role is required");

    var roles = await userQueryRepository.GetRolesByIdsAsync(command.RoleIds, cancellationToken);
    if (roles.Count != command.RoleIds.Distinct().Count())
      throw new NotFoundException("One or more roles were not found");

    await userStoreRepository.ReplaceUserRolesAsync(command.UserId, command.RoleIds, cancellationToken);
    await userStoreRepository.SaveChangesAsync(cancellationToken);
  }

}