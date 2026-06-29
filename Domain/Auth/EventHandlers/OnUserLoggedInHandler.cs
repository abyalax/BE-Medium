using Medium.Api.Domain.Auth.Events;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Nats.Services;

using NatsUserLoggedInEvent = Medium.Api.Infrastructure.Nats.Events.UserLoggedInEvent;

namespace Medium.Api.Domain.Auth.EventHandlers;

public class OnUserLoggedInHandler(
    IJetStreamEventPublisher jsPublisher,
    ILogger<OnUserLoggedInHandler> logger) : IEventHandler<UserLoggedInEvent>
{
  public async Task HandleAsync(UserLoggedInEvent @event, CancellationToken cancellationToken = default)
  {
    logger.LogInformation("User logged in: {UserId} - {Email}", @event.UserId, @event.Email);

    try
    {
      var natsEvent = new NatsUserLoggedInEvent(@event.UserId, @event.Email, DateTime.UtcNow);
      await jsPublisher.PublishToStreamAsync("user.logged-in", natsEvent, cancellationToken);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Failed to publish UserLoggedIn event to NATS JetStream");
    }
  }
}