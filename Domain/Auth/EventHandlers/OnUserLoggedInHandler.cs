using Medium.Api.Domain.Auth.Events;
using Medium.Api.Infrastructure.Events;

using Microsoft.Extensions.Logging;

namespace Medium.Api.Domain.Auth.EventHandlers;

public class OnUserLoggedInHandler(
    ILogger<OnUserLoggedInHandler> logger) : IEventHandler<UserLoggedInEvent>
{
  public async Task HandleAsync(UserLoggedInEvent @event, CancellationToken cancellationToken = default)
  {
    logger.LogInformation("User logged in: {UserId} - {Email}", @event.UserId, @event.Email);

    // TODO: Publish to NATS for analytics when NATS v2 integration is complete
  }
}