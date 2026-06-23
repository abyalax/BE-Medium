using System.Net;

namespace Medium.Api.Infrastructure.Exceptions;

public abstract class ApiException : Exception
{
    protected ApiException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}
