using Medium.Api.Models;

namespace Medium.Api.Domain.User.Dtos;

public record PermissionDto(
    Guid Id,
    string Code,
    string Name,
    string? Description
);

public record RoleDto(
  Guid Id,
  string Name,
  string? Description
);

public record RoleWithPermissionsDto(
  Guid Id,
  string Name,
  string? Description,
  IReadOnlyCollection<PermissionDto> Permissions
);

public record UserDto(
  Guid Id,
  string Name,
  string Email,
  string? Bio,
  string? AvatarUrl,
  IReadOnlyCollection<RoleWithPermissionsDto> Roles,
  DateTime CreatedAt,
  DateTime UpdatedAt
);

public record UserWithPasswordDto(
    Guid Id,
    string Name,
    string Email,
    string Password,
    string? Bio,
    string? AvatarUrl,
    IReadOnlyCollection<RoleWithPermissionsDto> Roles,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record UserListDto(
    Guid Id,
    string Name,
    string Email,
    string? Bio,
    string? AvatarUrl,
    IReadOnlyCollection<Role> Roles,
    DateTime CreatedAt,
    DateTime UpdatedAt
);