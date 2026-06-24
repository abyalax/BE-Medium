using FluentValidation;

using Medium.Api.Domain.ReadingHistory.Dtos;
using Medium.Api.Domain.ReadingHistory.Repositories;
using Medium.Api.Domain.ReadingHistory.Services;

namespace Medium.Api.Domain.ReadingHistory.Module;

public static class ReadingHistoryModule
{
  public static IServiceCollection AddReadingHistoryModule(this IServiceCollection services)
  {
    services.AddScoped<ReadingHistoryRepository>();

    services.AddScoped<ReadingHistoryService>();

    services.AddScoped<IValidator<CreateReadingHistoryRequest>, CreateReadingHistoryRequestValidator>();

    return services;
  }
}