using Medium.Api.Domain.Follow.Dtos;
using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Infrastructure.Exceptions;

using FollowModel = Medium.Api.Models.Follow;

namespace Medium.Api.Domain.Follow.Services;

public class FollowService(FollowRepository followRepository)
{
  private const int MaxPageSize = 100;
  private readonly FollowRepository _followRepository = followRepository;
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

    if (await _followRepository.ExistsAsync(followerId, request.FollowingId, cancellationToken))
    {
      throw new ConflictException("You are already following this user");
    }

    var follow = new FollowModel
    {
      Id = Guid.NewGuid(),
      FollowerId = followerId,
      FollowingId = request.FollowingId
    };

    await _followRepository.AddAsync(follow, cancellationToken);
    await _followRepository.SaveChangesAsync(cancellationToken);

    return await GetByIdAsync(follow.Id, cancellationToken);
  }

  public async Task<FollowResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var follow = await _followRepository.GetFollowWithUsersAsync(id, cancellationToken)
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
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

    var totalItems = await _followRepository.CountFollowersAsync(userId, cancellationToken);
    var items = await _followRepository.GetFollowersAsync(userId, page, pageSize, cancellationToken);
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
    pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

    var totalItems = await _followRepository.CountFollowingAsync(userId, cancellationToken);
    var items = await _followRepository.GetFollowingAsync(userId, page, pageSize, cancellationToken);
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
    var follow = await _followRepository.GetByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(messageNotFound);

    if (follow.FollowerId != currentUserId)
    {
      throw new ForbiddenException("You can only unfollow from your own account");
    }

    _followRepository.Remove(follow);
    await _followRepository.SaveChangesAsync(cancellationToken);
  }

  public async Task UnfollowAsync(
      Guid followerId,
      Guid followingId,
      CancellationToken cancellationToken = default)
  {
    var follow = await _followRepository.GetByFollowerAndFollowingAsync(followerId, followingId, cancellationToken);

    if (follow != null)
    {
      _followRepository.Remove(follow);
      await _followRepository.SaveChangesAsync(cancellationToken);
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

  private static FollowResponse ToResponse(Models.Follow follow)
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