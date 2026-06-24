using Medium.Api.Domain.Auth.DTOs;

namespace Medium.Api.Domain.User.Dtos;

public record CreateUserRequest(
    string Name,
    string Email,
    string Password,
    string? Bio = null,
    string? AvatarUrl = null,
    IReadOnlyCollection<Guid>? RoleIds = null);

public record UpdateUserRequest(
    string Name,
    string Email,
    string? Bio = null,
    string? AvatarUrl = null);

public record UserResponse(
    Guid Id,
    string Name,
    string Email,
    string? Bio,
    string? AvatarUrl,
    IReadOnlyCollection<RoleWithPermissionsResponse> Roles,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record RoleWithPermissionsResponse(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyCollection<PermissionResponse> Permissions);

public record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);

public record UserWithRolesPermissionsData(
    Guid Id,
    string Name,
    string Email,
    string Password,
    string? Bio,
    string? AvatarUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyCollection<RoleWithPermissionsResponse> Roles);