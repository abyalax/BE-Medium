using Medium.Api.Domain.ReadingHistory.Repositories;
namespace Medium.Api.Domain.ReadingHistory.Module;

public static class ReadingHistoryModule
{
  public static IServiceCollection AddReadingHistoryModule(this IServiceCollection services)
  {
    services.AddScoped<ReadingHistoryQueryRepository>();
    services.AddScoped<ReadingHistoryStoreRepository>();
    return services;
  }
}