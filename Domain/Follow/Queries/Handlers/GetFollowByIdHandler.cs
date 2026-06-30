using MediatR;

using Medium.Api.Domain.Follow.Dtos;
using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Infrastructure.Exceptions;

namespace Medium.Api.Domain.Follow.Queries.Handlers;

public class GetFollowByIdHandler(
  FollowQueryRepository followQueryRepository
) : IRequestHandler<GetFollowByIdQuery, FollowResponse>
{
  private readonly string messageNotFound = "Follow relationship not found";

  public async Task<FollowResponse> Handle(GetFollowByIdQuery query, CancellationToken cancellationToken)
  {
    var follow = await followQueryRepository.GetFollowWithUsersAsync(query.Id, cancellationToken)
      ?? throw new NotFoundException(messageNotFound);

    return ToResponse(follow);
  }

  private static FollowResponse ToResponse(FollowWithUsersData follow)
  {
    return new FollowResponse(
      follow.Id,
      follow.FollowerId,
      follow.FollowerName,
      follow.FollowingId,
      follow.FollowingName,
      follow.CreatedAt
    );
  }
}