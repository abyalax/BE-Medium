using System.Net;

namespace Medium.Api.Infrastructure.Exceptions;

public sealed class NotFoundException(string message) : ApiException(HttpStatusCode.NotFound, message) { }