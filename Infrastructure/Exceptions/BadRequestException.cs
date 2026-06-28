using System.Net;

namespace Medium.Api.Infrastructure.Exceptions;

public sealed class BadRequestException(string message) : ApiException(HttpStatusCode.BadRequest, message) { }