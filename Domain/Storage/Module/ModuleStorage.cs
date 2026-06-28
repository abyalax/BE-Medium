using Medium.Api.Domain.Storage.Commands;
using Medium.Api.Domain.Storage.Queries;

namespace Medium.Api.Domain.Storage.Module;

public static class ModuleStorage
{
  public static IServiceCollection AddStorageModule(this IServiceCollection services)
  {
    // Handlers are now registered automatically via MediatR
    return services;
  }
}