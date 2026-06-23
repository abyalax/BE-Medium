using System.Net;

namespace Medium.Api.Infrastructure.Exceptions;

public sealed class UnauthenticatedException : ApiException
{
    public UnauthenticatedException(string message = "Authentication is required")
        : base(HttpStatusCode.Unauthorized, message)
    {
    }
}
