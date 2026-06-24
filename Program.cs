using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Medium.Api.Infrastructure;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Database;
using Medium.Api.Infrastructure.Extensions;
using Medium.Api.Infrastructure.Filters;
using Medium.Api.Infrastructure.Logging;
using Medium.Api.Http.Api.Version1.Article;
using Medium.Api.Http.Api.Version1.Auth;
using Medium.Api.Http.Api.Version1.Bookmark;
using Medium.Api.Http.Api.Version1.Comment;
using Medium.Api.Http.Api.Version1.Follow;
using Medium.Api.Http.Api.Version1.ReadingHistory;
using Medium.Api.Http.Api.Version1.Tag;
using Medium.Api.Http.Api.Version1.Users;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
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

builder.Services.AddModule(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization(PermissionPolicies.Register);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
}

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
