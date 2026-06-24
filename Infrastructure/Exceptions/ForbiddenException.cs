using System.Net;

namespace Medium.Api.Infrastructure.Exceptions;

public sealed class ForbiddenException : ApiException
{
  public ForbiddenException(string message)
      : base(HttpStatusCode.NotFound, message)
  {
  }
}