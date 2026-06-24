namespace Medium.Api.Domain.Auth.DTOs;

public sealed class LoginRequest
{
  public string Email { get; init; } = default!;

  public string Password { get; init; } = default!;
}