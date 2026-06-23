using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace Medium.Api.Infrastructure.Auth;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userPermissions = context.User
            .FindAll(PermissionClaimTypes.Permission)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var hasPermission = userPermissions.Contains(
            requirement.Permission,
            StringComparer.OrdinalIgnoreCase);

        var permissionLog = new
        {
            required_permission = new[] { requirement.Permission },
            current_permission = userPermissions
        };

        string prettyJsonString = JsonSerializer.Serialize(permissionLog, new JsonSerializerOptions { WriteIndented = true });

        _logger.LogInformation(prettyJsonString);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
