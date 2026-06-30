
using System.Security.Claims;
using System.Text;

using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Medium.Api.Infrastructure.Extensions;

public static class AuthenticationExtensions
{
  public static IServiceCollection AddJwtAuthentication(
      this IServiceCollection services,
      IConfiguration config)
  {
    var jwtKey = config["AppSettings:Jwt:Key"] ?? throw new InvalidOperationException("AppSettings:Jwt:Key is not configured");
    var jwtIssuer = config["AppSettings:Jwt:Issuer"] ?? "Medium.Api";
    var jwtAudience = config["AppSettings:Jwt:Audience"] ?? "Medium.Api";

    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.MapInboundClaims = false;

      options.TokenValidationParameters = new TokenValidationParameters
      {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
      };

      options.Events = new JwtBearerEvents
      {
        OnChallenge = async context =>
          {
            context.HandleResponse();
            await ApiResponseWriter.WriteAsync(
              context.HttpContext,
              StatusCodes.Status401Unauthorized,
              "Unauthorized",
              "Authentication is required"
            );
          },
        OnForbidden = async context =>
          {
            await ApiResponseWriter.WriteAsync(
              context.HttpContext,
              StatusCodes.Status403Forbidden,
              "Forbidden",
              "You don't have permission"
            );
          }
      };
    });

    return services;
  }
}