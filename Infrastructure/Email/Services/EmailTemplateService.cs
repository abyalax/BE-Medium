using Medium.Api.Infrastructure.Email.Config;

using RazorLight;

namespace Medium.Api.Infrastructure.Email.Services;

public class EmailTemplateService
{
  private readonly RazorLightEngine _engine;

  public EmailTemplateService(EmailConfiguration configuration)
  {
    var templateRoot = Path.Combine(Directory.GetCurrentDirectory(), "Infrastructure", "Email", "Templates");
    _engine = new RazorLightEngineBuilder()
        .UseFileSystemProject(templateRoot)
        .UseMemoryCachingProvider()
        .Build();
  }

  public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
  {
    var templatePath = $"{templateName}.cshtml";
    return await _engine.CompileRenderAsync(templatePath, model);
  }
}