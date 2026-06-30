using MediatR;

using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Exceptions;


namespace Medium.Api.Domain.User.Commands.Handlers;

public class DeleteUserHandler(
  UserStoreRepository storeRepository,
  UserQueryRepository queryRepository
) : IRequestHandler<DeleteUserCommand, bool>
{
  public async Task<bool> Handle(DeleteUserCommand command, CancellationToken cancellationToken)
  {
    var user = await queryRepository.GetByIdAsync(command.UserId, cancellationToken)
      ?? throw new NotFoundException("User not found");

    storeRepository.Remove(user);
    await storeRepository.SaveChangesAsync(cancellationToken);

    return true;
  }
}