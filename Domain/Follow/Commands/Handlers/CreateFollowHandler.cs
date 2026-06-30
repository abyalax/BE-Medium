using MediatR;

using Medium.Api.Domain.Follow.Dtos;
using Medium.Api.Domain.Follow.Queries;
using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

using FollowModel = Medium.Api.Models.Follow;

namespace Medium.Api.Domain.Follow.Commands.Handlers;

public class CreateFollowHandler(
  FollowStoreRepository followStoreRepository,
  FollowQueryRepository followQueryRepository,
  INatsPublisher publisher,
  ISender sender
) : IRequestHandler<CreateFollowCommand, FollowResponse>
{
  public async Task<FollowResponse> Handle(CreateFollowCommand command, CancellationToken cancellationToken)
  {
    if (command.FollowerId == command.Request.FollowingId)
    {
      throw new BadRequestException("You cannot follow yourself");
    }

    if (await followQueryRepository.ExistsAsync(command.FollowerId, command.Request.FollowingId, cancellationToken))
    {
      throw new ConflictException("You are already following this user");
    }

    var follow = new FollowModel
    {
      Id = Guid.NewGuid(),
      FollowerId = command.FollowerId,
      FollowingId = command.Request.FollowingId
    };

    await followStoreRepository.AddAsync(follow, cancellationToken);
    await followStoreRepository.SaveChangesAsync(cancellationToken);

    var @event = new UserFollowedEvent
    {
      FollowerId = follow.FollowerId.ToString(),
      FollowingId = follow.FollowingId.ToString(),
      FollowedAt = follow.CreatedAt
    };

    await publisher.PublishAsync(NatsSubjects.UserFollowed, @event);

    return await sender.Send(new GetFollowByIdQuery(follow.Id), cancellationToken);
  }
}