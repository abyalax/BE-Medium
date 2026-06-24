namespace Medium.Api.Infrastructure.Auth;

public static class PermissionRules
{
  public static string GetActionForHttpMethod(string httpMethod)
  {
    return httpMethod.ToUpperInvariant() switch
    {
      "GET" => "get",
      "POST" => "create",
      "PUT" or "PATCH" => "update",
      "DELETE" => "delete",
      _ => "operation"
    };
  }

  public static string Build(string module, string httpMethod)
  {
    return $"{module}.{GetActionForHttpMethod(httpMethod)}";
  }
}