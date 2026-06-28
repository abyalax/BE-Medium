namespace Medium.Api.Domain.Auth.Events;

public class SendWelcomeEmailRequest
{
  public Guid UserId { get; set; }
  public string Email { get; set; } = string.Empty;
  public string Username { get; set; } = string.Empty;
}

public class SendWelcomeEmailResponse
{
  public bool Success { get; set; }
  public string Message { get; set; } = string.Empty;
}