
namespace Medium.Api.Infrastructure.AI;

public static class AIModule
{
  public static IServiceCollection AddAIInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    services.AddSingleton<IOnnxAISummarizationService, OnnxAISummarizationService>();

    return services;
  }
}