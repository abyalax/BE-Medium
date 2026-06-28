using Medium.Api.Domain.User.Dtos;

namespace Medium.Api.Domain.Auth.Dtos;

public record AuthDto(
  Guid UserId,
  string Name,
  string Email,
  string Token,
  IReadOnlyCollection<RoleWithPermissionsDto> Roles,
  string? Bio = null,
  string? AvatarUrl = null
);