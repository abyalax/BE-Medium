using System.Net;

namespace Medium.Api.Infrastructure.Exceptions;

public abstract class ApiException(HttpStatusCode statusCode, string message) : Exception(message)
{
  public HttpStatusCode StatusCode { get; } = statusCode;
}