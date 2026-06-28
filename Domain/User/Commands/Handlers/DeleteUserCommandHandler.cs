using MediatR;

using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;

using UserModel = Medium.Api.Models.User;

namespace Medium.Api.Domain.User.Commands.Handlers;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
{
  private readonly UserStoreRepository _userStoreRepository;
  private readonly UserQueryRepository _userQueryRepository;

  public DeleteUserCommandHandler(
      UserStoreRepository userStoreRepository,
      UserQueryRepository userQueryRepository)
  {
    _userStoreRepository = userStoreRepository;
    _userQueryRepository = userQueryRepository;
  }

  public async Task<bool> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
  {
    var user = await _userQueryRepository.GetByIdAsync(command.UserId, cancellationToken)
        ?? throw new NotFoundException("User not found");

    _userStoreRepository.Remove(user);
    await _userStoreRepository.SaveChangesAsync(cancellationToken);

    return true;
  }
}