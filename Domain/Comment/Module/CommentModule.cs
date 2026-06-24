using FluentValidation;

using Medium.Api.Domain.Comment.Dtos;
using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Domain.Comment.Services;

namespace Medium.Api.Domain.Comment.Module;

public static class CommentModule
{
  public static IServiceCollection AddCommentModule(this IServiceCollection services)
  {
    services.AddScoped<CommentRepository>();

    services.AddScoped<CommentService>();

    services.AddScoped<IValidator<CreateCommentRequest>, CreateCommentRequestValidator>();
    services.AddScoped<IValidator<UpdateCommentRequest>, UpdateCommentRequestValidator>();

    return services;
  }
}