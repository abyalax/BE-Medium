using Microsoft.AspNetCore.Authorization;

namespace Medium.Api.Infrastructure.Auth;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
  public string Permission { get; } = permission;
}