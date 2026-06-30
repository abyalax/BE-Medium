// TODO: remove this layer service and migrate it to CQRS Pattern

using Medium.Api.Domain.Follow.Dtos;
using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;
using Medium.Api.Infrastructure.Settings.Dtos;

using Microsoft.Extensions.Options;

using FollowModel = Medium.Api.Models.Follow;

namespace Medium.Api.Domain.Follow.Services;

public class FollowService(
  FollowStoreRepository followStoreRepository,
  FollowQueryRepository followQueryRepository,
  IOptions<ApplicationSettings> settings,
  INatsPublisher publisher
)
{
  private readonly string messageNotFound = "Follow relationship not found";

  public async Task<FollowResponse> CreateAsync(
      Guid followerId,
      FollowRequest request,
      CancellationToken cancellationToken = default)
  {
    if (followerId == request.FollowingId)
    {
      throw new BadRequestException("You cannot follow yourself");
    }

    if (await followQueryRepository.ExistsAsync(followerId, request.FollowingId, cancellationToken))
    {
      throw new ConflictException("You are already following this user");
    }

    var follow = new FollowModel
    {
      Id = Guid.NewGuid(),
      FollowerId = followerId,
      FollowingId = request.FollowingId
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

    return await GetByIdAsync(follow.Id, cancellationToken);
  }

  public async Task<FollowResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var follow = await followQueryRepository.GetFollowWithUsersAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    return ToResponse(follow);
  }

  public async Task<PagedFollowResponse> GetFollowersAsync(
      Guid userId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, settings.Value.Pagination.MaxPageSize);

    var totalItems = await followQueryRepository.CountFollowersAsync(userId, cancellationToken);
    var items = await followQueryRepository.GetFollowersAsync(userId, page, pageSize, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    return new PagedFollowResponse(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);
  }

  public async Task<PagedFollowResponse> GetFollowingAsync(
      Guid userId,
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, settings.Value.Pagination.MaxPageSize);

    var totalItems = await followQueryRepository.CountFollowingAsync(userId, cancellationToken);
    var items = await followQueryRepository.GetFollowingAsync(userId, page, pageSize, cancellationToken);
    var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

    return new PagedFollowResponse(
        items.Select(ToResponse).ToList(),
        page,
        pageSize,
        totalItems,
        totalPages);
  }

  public async Task DeleteAsync(
      Guid id,
      Guid currentUserId,
      CancellationToken cancellationToken = default)
  {
    var follow = await followQueryRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (follow.FollowerId != currentUserId)
    {
      throw new ForbiddenException("You can only unfollow from your own account");
    }

    followStoreRepository.Remove(follow);
    await followStoreRepository.SaveChangesAsync(cancellationToken);
  }

  public async Task UnfollowAsync(
      Guid followerId,
      Guid followingId,
      CancellationToken cancellationToken = default)
  {
    var follow = await followQueryRepository.GetByFollowerAndFollowingAsync(followerId, followingId, cancellationToken);

    if (follow != null)
    {
      followStoreRepository.Remove(follow);
      await followStoreRepository.SaveChangesAsync(cancellationToken);
    }
  }

  private static FollowResponse ToResponse(FollowWithUsersData follow)
  {
    return new FollowResponse(
        follow.Id,
        follow.FollowerId,
        follow.FollowerName,
        follow.FollowingId,
        follow.FollowingName,
        follow.CreatedAt);
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