using MediatR;

using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Follow.Commands.Handlers;

public class DeleteFollowHandler(
  FollowStoreRepository followStoreRepository,
  FollowQueryRepository followQueryRepository
) : IRequestHandler<DeleteFollowCommand>
{
  private readonly string messageNotFound = "Follow relationship not found";

  public async Task Handle(DeleteFollowCommand command, CancellationToken cancellationToken)
  {
    var follow = await followQueryRepository.GetByIdAsync(command.Id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (follow.FollowerId != command.CurrentUserId)
    {
      throw new ForbiddenException("You can only unfollow from your own account");
    }

    followStoreRepository.Remove(follow);
    await followStoreRepository.SaveChangesAsync(cancellationToken);
  }
}