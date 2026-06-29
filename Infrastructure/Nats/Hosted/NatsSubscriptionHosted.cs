using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

namespace Medium.Api.Infrastructure.Nats.Hosted;

public class NatsSubscriptionHostedService(IServiceProvider serviceProvider) : IHostedService
{
  private readonly IServiceProvider _serviceProvider = serviceProvider;
  private IServiceScope? _scope;

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    _scope = _serviceProvider.CreateScope();

    var subscriber = _scope.ServiceProvider.GetRequiredService<INatsSubscriber>();

    // Commenting out NATS subscription handlers for now to focus on CQRS auth testing
    var articleHandler = _scope.ServiceProvider.GetRequiredService<IEventHandler<ArticlePublishedEvent>>();
    var commentHandler = _scope.ServiceProvider.GetRequiredService<IEventHandler<CommentCreatedEvent>>();
    var followHandler = _scope.ServiceProvider.GetRequiredService<IEventHandler<UserFollowedEvent>>();

    await subscriber.SubscribeAsync<ArticlePublishedEvent>(
      NatsSubjects.ArticlePublished,
      @event => articleHandler.HandleAsync(@event, cancellationToken),
      cancellationToken
    );

    await subscriber.SubscribeAsync<CommentCreatedEvent>(
      NatsSubjects.CommentCreated,
      @event => commentHandler.HandleAsync(@event, cancellationToken),
      cancellationToken
    );

    await subscriber.SubscribeAsync<UserFollowedEvent>(
      NatsSubjects.UserFollowed,
      @event => followHandler.HandleAsync(@event, cancellationToken),
      cancellationToken
    );
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _scope?.Dispose();
    return Task.CompletedTask;
  }
}