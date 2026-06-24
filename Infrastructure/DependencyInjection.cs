using Medium.Api.Domain.Bookmark.Module;
using Medium.Api.Domain.Article.Module;
using Medium.Api.Domain.Auth.Module;
using Medium.Api.Domain.User.Module;
using Medium.Api.Domain.Comment.Module;
using Medium.Api.Domain.Follow.Module;
using Medium.Api.Domain.Tag.Module;
using Medium.Api.Domain.ReadingHistory.Module;

namespace Medium.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddArticleModule();
        services.AddAuthModule();
        services.AddBookmarkModule();
        services.AddUserModule();
        services.AddCommentModule();
        services.AddFollowModule();
        services.AddTagModule();
        services.AddReadingHistoryModule();

        return services;
    }
}
