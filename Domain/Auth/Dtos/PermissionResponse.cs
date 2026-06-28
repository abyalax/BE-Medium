namespace Medium.Api.Domain.Auth.Dtos;

public record PermissionResponse(
  Guid Id,
  string Code,
  string Name,
  string? Description = null
);