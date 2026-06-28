namespace Medium.Api.Infrastructure.Events;

public interface IEventHandler
{
}

public interface IEventHandler<T> : IEventHandler where T : class
{
  Task HandleAsync(T @event, CancellationToken cancellationToken = default);
}