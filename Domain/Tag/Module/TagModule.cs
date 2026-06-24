using FluentValidation;

using Medium.Api.Domain.Tag.Dtos;
using Medium.Api.Domain.Tag.Repositories;
using Medium.Api.Domain.Tag.Services;

namespace Medium.Api.Domain.Tag.Module;

public static class TagModule
{
  public static IServiceCollection AddTagModule(this IServiceCollection services)
  {
    services.AddScoped<TagRepository>();

    services.AddScoped<TagService>();

    services.AddScoped<IValidator<CreateTagRequest>, CreateTagRequestValidator>();
    services.AddScoped<IValidator<UpdateTagRequest>, UpdateTagRequestValidator>();

    return services;
  }
}