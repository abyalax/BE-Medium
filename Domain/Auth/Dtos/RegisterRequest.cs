namespace Medium.Api.Domain.Auth.DTOs;

public sealed class RegisterRequest
{
  public string Name { get; init; } = default!;

  public string Email { get; init; } = default!;

  public string Password { get; init; } = default!;

  public string? Bio { get; init; }

  public string? AvatarUrl { get; init; }
}