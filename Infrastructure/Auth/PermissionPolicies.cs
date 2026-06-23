using Microsoft.AspNetCore.Authorization;

namespace Medium.Api.Infrastructure.Auth;

public static class PermissionPolicies
{
    public static void Register(AuthorizationOptions options)
    {
        foreach (var permission in Permissions.All)
        {
            options.AddPolicy(permission, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.AddRequirements(new PermissionRequirement(permission));
            });
        }
    }
}
