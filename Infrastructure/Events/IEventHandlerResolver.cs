namespace Medium.Api.Infrastructure.Events;

public interface IEventHandlerResolver
{
  Task HandleAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}