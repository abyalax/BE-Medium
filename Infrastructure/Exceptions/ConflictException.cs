using System.Net;

namespace Medium.Api.Infrastructure.Exceptions;

public sealed class ConflictException : ApiException
{
    public ConflictException(string message)
        : base(HttpStatusCode.Conflict, message)
    {
    }
}
