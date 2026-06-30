using FluentValidation;

using Medium.Api.Infrastructure;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Behaviors;
using Medium.Api.Infrastructure.Database;
using Medium.Api.Infrastructure.Database.Seeds;
using Medium.Api.Infrastructure.Extensions;
using Medium.Api.Infrastructure.Filters;
using Medium.Api.Infrastructure.Logging;

using Microsoft.EntityFrameworkCore;

namespace Medium.Api;

public class Program
{
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.ConfigureApplicationSettings(builder.Configuration);

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

    // Scan all command and pair their validators automatically via abstract validator
    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
    // Register MediatR and validation Behavior
    builder.Services.AddMediatR(cfg =>
    {
      cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
      // Register middleware validation before handler execution 
      cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    });

    // Infrastructure setup includes lifecycle service registration
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddModule();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddAuthorization(PermissionPolicies.Register);

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<CurrentUser>();

    var app = builder.Build();
    // Intercept execution here if "--seed" argument is passed
    if (DatabaseSeederRunner.HandleSeedCommandAsync(args, app).GetAwaiter().GetResult()) return; // Stop execution immediately, preventing the web server from starting

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

    app.MapControllers();

    app.Services.ConfigureSchedulers();

    app.Run();
  }
}