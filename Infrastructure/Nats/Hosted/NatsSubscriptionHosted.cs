using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Handler;
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

    var articleHandler = _scope.ServiceProvider.GetRequiredService<IEventHandler<ArticlePublishedEvent>>();
    var commentHandler = _scope.ServiceProvider.GetRequiredService<IEventHandler<CommentCreatedEvent>>();
    var followHandler = _scope.ServiceProvider.GetRequiredService<IEventHandler<UserFollowedEvent>>();

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

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _scope?.Dispose();
    return Task.CompletedTask;
  }
}