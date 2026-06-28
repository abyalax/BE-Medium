using System.Net;

namespace Medium.Api.Infrastructure.Exceptions;

public sealed class UnauthenticatedException(string message = "Authentication is required")
  : ApiException(HttpStatusCode.Unauthorized, message)
{ }