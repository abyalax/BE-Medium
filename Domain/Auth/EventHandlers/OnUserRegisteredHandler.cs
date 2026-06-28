using Medium.Api.Domain.Auth.Events;
using Medium.Api.Infrastructure.Email.Services;
using Medium.Api.Infrastructure.Events;

using Microsoft.Extensions.Logging;

namespace Medium.Api.Domain.Auth.EventHandlers;

public class OnUserRegisteredHandler(
    MailpitEmailService emailService,
    ILogger<OnUserRegisteredHandler> logger) : IEventHandler<UserRegisteredEvent>
{
  public async Task HandleAsync(UserRegisteredEvent @event, CancellationToken cancellationToken = default)
  {
    logger.LogInformation("User registered: {UserId} - {Email}", @event.UserId, @event.Email);

    // Send welcome email
    try
    {
      await emailService.SendAsync(
          @event.Email,
          "Welcome to Medium!",
          $"Hello {@event.Name}, welcome to Medium! We're excited to have you on board.",
          cancellationToken);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to send welcome email to {Email}", @event.Email);
    }
  }
}