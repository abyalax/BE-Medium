using MediatR;

using Medium.Api.Domain.Follow.Repositories;

namespace Medium.Api.Domain.Follow.Commands.Handlers;

public class UnfollowHandler(
  FollowStoreRepository followStoreRepository,
  FollowQueryRepository followQueryRepository
) : IRequestHandler<UnfollowCommand>
{
  public async Task Handle(UnfollowCommand command, CancellationToken cancellationToken)
  {
    var follow = await followQueryRepository.GetByFollowerAndFollowingAsync(command.FollowerId, command.FollowingId, cancellationToken);

    if (follow != null)
    {
      followStoreRepository.Remove(follow);
      await followStoreRepository.SaveChangesAsync(cancellationToken);
    }
  }
}