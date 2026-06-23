namespace Medium.Api.Domain.Auth.DTOs;

public record PermissionResponse(Guid Id, string Code, string Name, string Description);
