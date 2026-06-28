using Medium.Api.Infrastructure.Interface;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Medium.Api.Infrastructure.Database;

public class DbConnectionLifecycle : IDbConnectionLifecycle
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<DbConnectionLifecycle> _logger;

  public DbConnectionLifecycle(IServiceProvider serviceProvider, ILogger<DbConnectionLifecycle> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    _logger.LogInformation("Verifying Database connection...");
    using var scope = _serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
    if (!canConnect)
    {
      throw new InvalidOperationException("Failed to connect to the database.");
    }

    _logger.LogInformation("✓ Database connection verified");
  }

  public async Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
  {
    using var scope = _serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    return await dbContext.Database.CanConnectAsync(cancellationToken);
  }

  public Task ShutdownAsync(CancellationToken cancellationToken = default)
  {
    return Task.CompletedTask;
  }
}