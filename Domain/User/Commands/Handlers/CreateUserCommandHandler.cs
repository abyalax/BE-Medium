using MediatR;

using Medium.Api.Domain.User.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Exceptions;

using UserModel = Medium.Api.Models.User;

namespace Medium.Api.Domain.User.Commands.Handlers;

public class CreateUserCommandHandler(
    UserStoreRepository userStoreRepository,
    UserQueryRepository userQueryRepository,
    IPasswordHasher passwordHasher) : IRequestHandler<CreateUserCommand, UserDto?>
{

  public async Task<UserDto?> Handle(CreateUserCommand command, CancellationToken cancellationToken)
  {
    if (await userQueryRepository.ExistsByEmailAsync(command.Email, cancellationToken: cancellationToken))
    {
      throw new ConflictException("User with this email already exists");
    }

    var roleIds = await ResolveRoleIdsAsync(command.RoleIds, cancellationToken);
    var user = new UserModel
    {
      Id = Guid.NewGuid(),
      Name = command.Name,
      Email = command.Email,
      Password = passwordHasher.HashPassword(command.Password),
      Bio = command.Bio,
      AvatarUrl = command.AvatarUrl
    };

    await userStoreRepository.AddAsync(user, cancellationToken);
    await userStoreRepository.SaveChangesAsync(cancellationToken);
    await userStoreRepository.ReplaceUserRolesAsync(user.Id, roleIds, cancellationToken);
    await userStoreRepository.SaveChangesAsync(cancellationToken);

    var userWithRoles = await userQueryRepository.FindByEmailWithRolePermissionAsync(user.Email, cancellationToken);
    return userWithRoles;
  }

  private async Task<IReadOnlyCollection<Guid>> ResolveRoleIdsAsync(IReadOnlyCollection<Guid>? roleIds, CancellationToken cancellationToken)
  {
    if (roleIds is { Count: > 0 })
    {
      var roles = await userQueryRepository.GetRolesByIdsAsync(roleIds, cancellationToken);
      if (roles.Count != roleIds.Distinct().Count())
      {
        throw new NotFoundException("One or more roles were not found");
      }

      return roleIds.Distinct().ToArray();
    }

    var readerRole = await userQueryRepository.GetRoleByNameAsync("Reader", cancellationToken)
        ?? throw new NotFoundException("Reader role is not available");

    return [readerRole.Id];
  }
}