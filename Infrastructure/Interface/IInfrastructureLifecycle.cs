namespace Medium.Api.Infrastructure.Interface;

public interface IInfrastructureLifecycle
{
  Task InitializeAsync(CancellationToken cancellationToken = default);
  Task ShutdownAsync(CancellationToken cancellationToken = default);
}

public interface IDbConnectionLifecycle : IInfrastructureLifecycle
{
  Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
}

public interface ICacheLifecycle : IInfrastructureLifecycle { }

public interface INatsLifecycle : IInfrastructureLifecycle { }

public interface IManuallyStartableService
{
  Task StartAsync(CancellationToken cancellationToken = default);
  Task StopAsync(CancellationToken cancellationToken = default);
}