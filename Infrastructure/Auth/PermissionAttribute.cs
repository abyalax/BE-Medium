using Microsoft.AspNetCore.Authorization;

namespace Medium.Api.Infrastructure.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class PermissionAttribute : AuthorizeAttribute
{
  public PermissionAttribute(string policy)
  {
    Policy = policy;
  }
}