using System.Net;

namespace Medium.Api.Infrastructure.Exceptions;

public sealed class ForbiddenException(string message) : ApiException(HttpStatusCode.NotFound, message) { }