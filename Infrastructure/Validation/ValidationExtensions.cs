using FluentValidation;

using Microsoft.Extensions.DependencyInjection;

namespace Medium.Api.Infrastructure.Validation;

public static class ValidationExtensions
{
  public static IServiceCollection AddGlobalValidation(this IServiceCollection services)
  {
    services.AddValidatorsFromAssemblyContaining<Program>();
    return services;
  }
}