using MediatR;

using Medium.Api.Domain.Follow.Dtos;
using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Infrastructure.Settings.Dtos;

using Microsoft.Extensions.Options;

using FollowModel = Medium.Api.Models.Follow;

namespace Medium.Api.Domain.Follow.Queries.Handlers;

public class GetFollowingHandler(
  FollowQueryRepository followQueryRepository,
  IOptions<ApplicationSettings> settings
) : IRequestHandler<GetFollowingQuery, PagedFollowResponse>
{
  public async Task<PagedFollowResponse> Handle(GetFollowingQuery query, CancellationToken cancellationToken)
  {
    var page = query.Page < 1 ? 1 : query.Page;
    var pageSize = query.PageSize < 1 ? 10 : Math.Min(query.PageSize, settings.Value.Pagination.MaxPageSize);

    var totalItems = await followQueryRepository.CountFollowingAsync(query.UserId, cancellationToken);
    var items = await followQueryRepository.GetFollowingAsync(query.UserId, page, pageSize, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    return new PagedFollowResponse(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);
  }

  private static FollowResponse ToResponse(FollowModel follow)
  {
    return new FollowResponse(
        follow.Id,
        follow.FollowerId,
        follow.Follower.Name,
        follow.FollowingId,
        follow.Following.Name,
        follow.CreatedAt);
  }
}