using MediatR;

using Medium.Api.Domain.User.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.User.Commands.Handlers;

public class UpdateUserHandler(
  UserStoreRepository storeRepository,
  UserQueryRepository queryRepository
) : IRequestHandler<UpdateUserCommand, UserDto?>
{

  public async Task<UserDto?> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
  {
    var user = await storeRepository.GetByIdForUpdateAsync(command.UserId, cancellationToken)
      ?? throw new NotFoundException("User not found");

    if (await queryRepository.ExistsByEmailAsync(command.Email, command.UserId, cancellationToken))
      throw new ConflictException("User with this email already exists");

    user.Name = command.Name;
    user.Email = command.Email;
    user.Bio = command.Bio;
    user.AvatarUrl = command.AvatarUrl;
    user.UpdatedAt = DateTime.UtcNow;

    await storeRepository.SaveChangesAsync(cancellationToken);

    var userWithRoles = await queryRepository.FindByEmailWithRolePermissionAsync(user.Email, cancellationToken);
    return userWithRoles;
  }
}