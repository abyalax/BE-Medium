using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Interface;
using Medium.Api.Infrastructure.Nats.Events;
using Medium.Api.Infrastructure.Nats.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Medium.Api.Infrastructure.Nats.Consumers;

public class ArticlePublishedPushConsumer(
  IJetStreamPushConsumer pushConsumer,
  IServiceProvider serviceProvider,
  ILogger<ArticlePublishedPushConsumer> logger) : IManuallyStartableService
{
  private readonly IJetStreamPushConsumer _pushConsumer = pushConsumer;
  private readonly IServiceProvider _serviceProvider = serviceProvider;
  private readonly ILogger<ArticlePublishedPushConsumer> _logger = logger;
  private CancellationTokenSource? _cancellationTokenSource;

  public async Task StartAsync(CancellationToken cancellationToken = default)
  {
    _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    var linkedCancellationToken = _cancellationTokenSource.Token;

    try
    {
      _logger.LogInformation("Starting ArticlePublishedPushConsumer for subject {Subject}", NatsSubjects.ArticlePublished);

      await _pushConsumer.SubscribeAsync<ArticlePublishedEvent>(
        "MEDIUM_EVENTS",
        NatsSubjects.ArticlePublished,
        async @event =>
        {
          using var scope = _serviceProvider.CreateScope();
          var handlers = scope.ServiceProvider.GetServices<IEventHandler<ArticlePublishedEvent>>();

          var tasks = handlers.Select(handler => handler.HandleAsync(@event, linkedCancellationToken));
          await Task.WhenAll(tasks);
        },
        "article_published_subscribers",
        linkedCancellationToken
      );
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "ArticlePublishedPushConsumer failed");
      throw;
    }
  }

  public async Task StopAsync(CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Stopping ArticlePublishedPushConsumer");
    _cancellationTokenSource?.Cancel();
    await Task.CompletedTask;
  }
}