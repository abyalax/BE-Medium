namespace Medium.Api.Domain.Follow.Dtos;

public record FollowRequest(
    Guid FollowingId
);

public record FollowResponse(
    Guid Id,
    Guid FollowerId,
    string FollowerName,
    Guid FollowingId,
    string FollowingName,
    DateTime CreatedAt);

public record PagedFollowResponse(
    IReadOnlyCollection<FollowResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public record FollowWithUsersData(
    Guid Id,
    Guid FollowerId,
    string FollowerName,
    Guid FollowingId,
    string FollowingName,
    DateTime CreatedAt);