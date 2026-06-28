using FluentValidation;

using MediatR;

using Medium.Api.Domain.Article.Module;
using Medium.Api.Domain.Auth.Module;
using Medium.Api.Domain.Bookmark.Module;
using Medium.Api.Domain.Comment.Module;
using Medium.Api.Domain.Follow.Module;
using Medium.Api.Domain.Notification.Module;
using Medium.Api.Domain.ReadingHistory.Module;
using Medium.Api.Domain.Tag.Module;
using Medium.Api.Domain.User.Module;
using Medium.Api.Infrastructure.Behaviors;
using Medium.Api.Infrastructure.Cache.Module;
using Medium.Api.Infrastructure.Database;
using Medium.Api.Infrastructure.Email.Module;
using Medium.Api.Infrastructure.Events;
using Medium.Api.Infrastructure.Interface;
using Medium.Api.Infrastructure.Lifecycle;
using Medium.Api.Infrastructure.Nats.Handler;
using Medium.Api.Infrastructure.Nats.Module;
using Medium.Api.Infrastructure.Scheduler.Module;
using Medium.Api.Infrastructure.Storage.Module;

namespace Medium.Api.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    // Register lifecycle services first before other infrastructure
    services.AddSingleton<IDbConnectionLifecycle, DbConnectionLifecycle>();

    services.AddNatsInfrastructure(configuration);
    services.AddEmailInfrastructure(configuration);
    services.AddMinioInfrastructure(configuration);
    services.AddRedisInfrastructure(configuration);

    services.AddCoravelInfrastructure();
    services.AddEventHandlers();

    // Register MediatR
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

    // Register FluentValidation pipeline behavior
    services.AddValidatorsFromAssembly(typeof(Program).Assembly);
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

    services.AddScoped<ApplicationModule>();
    services.AddHostedService<ApplicationLifecycleService>();

    return services;
  }

  public static IServiceCollection AddModule(this IServiceCollection services)
  {
    services.AddArticleModule();
    services.AddAuthModule();
    services.AddBookmarkModule();
    services.AddUserModule();
    services.AddCommentModule();
    services.AddFollowModule();
    services.AddTagModule();
    services.AddReadingHistoryModule();
    services.AddNotificationModule();

    return services;
  }
}