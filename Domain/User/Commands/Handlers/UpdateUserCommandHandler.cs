using MediatR;

using Medium.Api.Domain.User.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.User.Commands.Handlers;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto?>
{
  private readonly UserStoreRepository _userStoreRepository;
  private readonly UserQueryRepository _userQueryRepository;

  public UpdateUserCommandHandler(
      UserStoreRepository userStoreRepository,
      UserQueryRepository userQueryRepository)
  {
    _userStoreRepository = userStoreRepository;
    _userQueryRepository = userQueryRepository;
  }

  public async Task<UserDto?> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
  {
    var user = await _userStoreRepository.GetByIdForUpdateAsync(command.UserId, cancellationToken)
        ?? throw new NotFoundException("User not found");

    if (await _userQueryRepository.ExistsByEmailAsync(command.Email, command.UserId, cancellationToken))
      throw new ConflictException("User with this email already exists");

    user.Name = command.Name;
    user.Email = command.Email;
    user.Bio = command.Bio;
    user.AvatarUrl = command.AvatarUrl;
    user.UpdatedAt = DateTime.UtcNow;

    await _userStoreRepository.SaveChangesAsync(cancellationToken);

    var userWithRoles = await _userQueryRepository.FindByEmailWithRolePermissionAsync(user.Email, cancellationToken);
    return userWithRoles;
  }
}