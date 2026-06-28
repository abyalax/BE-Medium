using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Medium.Api.Infrastructure.Events;

public class ReflectionEventHandlerResolver : IEventHandlerResolver
{
  private readonly IServiceProvider _serviceProvider;
  private readonly Dictionary<Type, List<Type>> _eventHandlerMappings;

  public ReflectionEventHandlerResolver(IServiceProvider serviceProvider)
  {
    _serviceProvider = serviceProvider;
    _eventHandlerMappings = new Dictionary<Type, List<Type>>();
    DiscoverEventHandlers();
  }

  private void DiscoverEventHandlers()
  {
    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
    var eventHandlerInterfaceType = typeof(IEventHandler<>);

    foreach (var assembly in assemblies)
    {
      try
      {
        var types = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == eventHandlerInterfaceType)
                .Select(i => new
                {
                  HandlerType = t,
                  EventType = i.GetGenericArguments()[0]
                }))
            .ToList();

        foreach (var mapping in types)
        {
          if (!_eventHandlerMappings.ContainsKey(mapping.EventType))
          {
            _eventHandlerMappings[mapping.EventType] = new List<Type>();
          }
          _eventHandlerMappings[mapping.EventType].Add(mapping.HandlerType);
        }
      }
      catch
      {
        // Skip assemblies that can't be loaded
        continue;
      }
    }
  }

  public async Task HandleAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
  {
    var eventType = typeof(T);

    if (!_eventHandlerMappings.ContainsKey(eventType))
    {
      return; // No handlers registered for this event
    }

    var handlerTypes = _eventHandlerMappings[eventType];

    using var scope = _serviceProvider.CreateScope();

    var tasks = handlerTypes.Select(async handlerType =>
    {
      try
      {
        var handler = scope.ServiceProvider.GetService(handlerType);
        if (handler != null)
        {
          var handleMethod = handlerType.GetMethod(nameof(IEventHandler<T>.HandleAsync));
          if (handleMethod != null)
          {
            var task = (Task?)handleMethod.Invoke(handler, new object[] { @event, cancellationToken });
            if (task != null)
            {
              await task;
            }
          }
        }
      }
      catch (Exception ex)
      {
        // Log error but continue with other handlers
        Console.WriteLine($"Error handling event {eventType.Name} with handler {handlerType.Name}: {ex.Message}");
      }
    });

    await Task.WhenAll(tasks);
  }
}