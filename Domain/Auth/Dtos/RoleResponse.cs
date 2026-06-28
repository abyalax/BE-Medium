namespace Medium.Api.Domain.Auth.Dtos;

public record RoleResponse(
  Guid Id,
  string Name,
  string? Description = null
);