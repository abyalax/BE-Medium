using System.Net;

namespace Medium.Api.Infrastructure.Exceptions;

public sealed class ConflictException(string message) : ApiException(HttpStatusCode.Conflict, message) { }