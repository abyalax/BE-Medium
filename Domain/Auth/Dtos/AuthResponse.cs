using Medium.Api.Domain.Users.Dtos;

namespace Medium.Api.Domain.Auth.DTOs;

public record AuthResponse(
    Guid UserId,
    string Name,
    string Email,
    string Token,
    IReadOnlyCollection<RoleWithPermissionsResponse> Roles,
    string? Bio = null,
    string? AvatarUrl = null);
