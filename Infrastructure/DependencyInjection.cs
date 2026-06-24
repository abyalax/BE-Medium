using Medium.Api.Domain.Article.Module;
using Medium.Api.Domain.Auth.Module;
using Medium.Api.Domain.Bookmark.Module;
using Medium.Api.Domain.Comment.Module;
using Medium.Api.Domain.Follow.Module;
using Medium.Api.Domain.Notification.Module;
using Medium.Api.Domain.ReadingHistory.Module;
using Medium.Api.Domain.Tag.Module;
using Medium.Api.Domain.User.Module;
using Medium.Api.Infrastructure.Email.Module;
using Medium.Api.Infrastructure.Events.Handler;
using Medium.Api.Infrastructure.Nats.Module;
using Medium.Api.Infrastructure.Scheduler.Module;

namespace Medium.Api.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddNatsInfrastructure(configuration);
    services.AddEmailInfrastructure(configuration);

    services.AddCoravelInfrastructure();
    services.AddEventHandler();

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