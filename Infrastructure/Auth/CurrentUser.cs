using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Medium.Api.Infrastructure.Auth;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor)
{
  private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;
  // Checks if the user is authenticated
  public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;

  // Gets the UserId from the 'sub' claim
  public Guid Id
  {
    get
    {
      var claimValue = _user?.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? _user?.FindFirstValue(ClaimTypes.NameIdentifier);
      if (!Guid.TryParse(claimValue, out var userId))
        throw new UnauthorizedAccessException("User is unauthenticated.");
      return userId;
    }
  }

  public string? Email => _user?.FindFirstValue(JwtRegisteredClaimNames.Email)
    ?? _user?.FindFirstValue(ClaimTypes.Email);

  // Gets all roles from the 'role' claims
  public IEnumerable<string> Roles => _user?.FindAll(ClaimTypes.Role)
    .Select(c => c.Value)
    ?? [];

  // Gets all permissions from the custom 'permission' claims
  // Ensure "PermissionClaimTypes.Permission" matches the literal string used in your generator (e.g., "permission")
  public IEnumerable<string> Permissions => _user?.FindAll("permission").Select(c => c.Value) ?? [];
}