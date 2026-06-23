using System.Net;

namespace Medium.Api.Infrastructure.Exceptions;

public sealed class BadRequestException : ApiException
{
    public BadRequestException(string message)
        : base(HttpStatusCode.BadRequest, message)
    {
    }
}
