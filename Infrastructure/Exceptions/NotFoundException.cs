using System.Net;

namespace Medium.Api.Infrastructure.Exceptions;

public sealed class NotFoundException : ApiException
{
    public NotFoundException(string message)
        : base(HttpStatusCode.NotFound, message)
    {
    }
}
