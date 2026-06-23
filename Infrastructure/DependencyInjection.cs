using Medium.Api.Domain.Auth.Services;
using Medium.Api.Domain.Auth.DTOs;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Domain.Users.Services;
using Medium.Api.Domain.Users.Repositories;
using Medium.Api.Domain.Users.Dtos;
using Medium.Api.Domain.Article.Services;
using Medium.Api.Domain.Article.Repositories;
using Medium.Api.Domain.Article.Dtos;
using Medium.Api.Domain.Tag.Services;
using Medium.Api.Domain.Tag.Repositories;
using Medium.Api.Domain.Tag.Dtos;
using Medium.Api.Domain.Comment.Services;
using Medium.Api.Domain.Comment.Repositories;
using Medium.Api.Domain.Comment.Dtos;
using Medium.Api.Domain.Bookmark.Services;
using Medium.Api.Domain.Bookmark.Repositories;
using Medium.Api.Domain.Bookmark.Dtos;
using Medium.Api.Domain.Follow.Services;
using Medium.Api.Domain.Follow.Repositories;
using Medium.Api.Domain.Follow.Dtos;
using Medium.Api.Domain.ReadingHistory.Services;
using Medium.Api.Domain.ReadingHistory.Repositories;
using Medium.Api.Domain.ReadingHistory.Dtos;
using Medium.Api.Http.Api.Version1.Auth;
using Medium.Api.Http.Api.Version1.Users;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;


namespace Medium.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Register infrastructure services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<UserRepository>();
        services.AddScoped<ArticleRepository>();
        services.AddScoped<TagRepository>();
        services.AddScoped<CommentRepository>();
        services.AddScoped<BookmarkRepository>();
        services.AddScoped<FollowRepository>();
        services.AddScoped<ReadingHistoryRepository>();
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<RoleEndpoints.CreateRoleRequest>, CreateRoleRequestValidator>();
        services.AddScoped<IValidator<PermissionEndpoints.CreatePermissionRequest>, CreatePermissionRequestValidator>();
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
        services.AddScoped<IValidator<UserEndpoints.AssignUserRolesRequest>, AssignUserRolesRequestValidator>();
        services.AddScoped<IValidator<CreateArticleRequest>, CreateArticleRequestValidator>();
        services.AddScoped<IValidator<UpdateArticleRequest>, UpdateArticleRequestValidator>();
        services.AddScoped<IValidator<PublishArticleRequest>, PublishArticleRequestValidator>();
        services.AddScoped<IValidator<CreateTagRequest>, CreateTagRequestValidator>();
        services.AddScoped<IValidator<UpdateTagRequest>, UpdateTagRequestValidator>();
        services.AddScoped<IValidator<CreateCommentRequest>, CreateCommentRequestValidator>();
        services.AddScoped<IValidator<UpdateCommentRequest>, UpdateCommentRequestValidator>();
        services.AddScoped<IValidator<BookmarkRequest>, BookmarkRequestValidator>();
        services.AddScoped<IValidator<FollowRequest>, FollowRequestValidator>();
        services.AddScoped<IValidator<CreateReadingHistoryRequest>, CreateReadingHistoryRequestValidator>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }

    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Register domain services
        services.AddScoped<AuthService>();
        services.AddScoped<RoleService>();
        services.AddScoped<PermissionService>();
        services.AddScoped<UserService>();
        services.AddScoped<ArticleService>();
        services.AddScoped<TagService>();
        services.AddScoped<CommentService>();
        services.AddScoped<BookmarkService>();
        services.AddScoped<FollowService>();
        services.AddScoped<ReadingHistoryService>();

        return services;
    }
}
