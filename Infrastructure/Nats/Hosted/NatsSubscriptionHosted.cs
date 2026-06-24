using Medium.Api.Infrastructure.Events.Events;
using Medium.Api.Infrastructure.Events.Handler;
using Medium.Api.Infrastructure.Nats.Services;

namespace Medium.Api.Infrastructure.Nats.Hosted;

public class NatsSubscriptionHostedService : IHostedService
{
  private readonly IServiceProvider _serviceProvider;

  public NatsSubscriptionHostedService(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
  }

  public async Task StartAsync(
      CancellationToken cancellationToken)
  {
    using var scope = _serviceProvider.CreateScope();

    var subscriber = scope.ServiceProvider.GetRequiredService<INatsSubscriber>();

    var articleHandler = scope.ServiceProvider.GetRequiredService<IEventHandler<ArticlePublishedEvent>>();
    var commentHandler = scope.ServiceProvider.GetRequiredService<IEventHandler<CommentCreatedEvent>>();
    var followHandler = scope.ServiceProvider.GetRequiredService<IEventHandler<UserFollowedEvent>>();

    await subscriber.SubscribeAsync<ArticlePublishedEvent>(
            NatsSubjects.ArticlePublished,
            articleHandler.HandleAsync
        );

    await subscriber.SubscribeAsync<CommentCreatedEvent>(
            NatsSubjects.CommentCreated,
            commentHandler.HandleAsync
        );

    await subscriber.SubscribeAsync<UserFollowedEvent>(
            NatsSubjects.UserFollowed,
            followHandler.HandleAsync
        );
  }

  public Task StopAsync(
      CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}