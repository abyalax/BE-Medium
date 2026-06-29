namespace Medium.Api.Infrastructure.Events;

public interface IEventHandler
{
}

public interface IEventHandler<in T> : IEventHandler where T : class
{
  Task HandleAsync(T @event, CancellationToken cancellationToken = default);
}