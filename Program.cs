using Microsoft.EntityFrameworkCore;

using Medium.Api.Infrastructure;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Logging;
using Medium.Api.Infrastructure.Filters;
using Medium.Api.Infrastructure.Extensions;
using Medium.Api.Infrastructure.Database;
using Medium.Api.Infrastructure.Database.Seeds;
using Medium.Api.Http.Api.Version1.Article;
using Medium.Api.Http.Api.Version1.Auth;
using Medium.Api.Http.Api.Version1.Bookmark;
using Medium.Api.Http.Api.Version1.Comment;
using Medium.Api.Http.Api.Version1.Follow;
using Medium.Api.Http.Api.Version1.ReadingHistory;
using Medium.Api.Http.Api.Version1.Tag;
using Medium.Api.Http.Api.Version1.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        })
    .AddInterceptors(new PreventDeleteWithRelationsInterceptor()));

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddModule();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization(PermissionPolicies.Register);

var app = builder.Build();

// Intercept execution here if "--seed" argument is passed
if (await DatabaseSeederRunner.HandleSeedCommandAsync(args, app)) return; // Stop execution immediately, preventing the web server from starting

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<ExceptionHandling>();
app.UseAuthentication();
app.UseAuthorization();

app.Services.ConfigureSchedulers();

app.MapAuthEndpoints();
app.MapRoleEndpoints();
app.MapPermissionEndpoints();
app.MapUserEndpoints();
app.MapArticleEndpoints();
app.MapTagEndpoints();
app.MapCommentEndpoints();
app.MapBookmarkEndpoints();
app.MapFollowEndpoints();
app.MapReadingHistoryEndpoints();

app.Run();
