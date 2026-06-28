using System.Diagnostics;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Medium.Api.Infrastructure.Logging;

// Using C# 12 Primary Constructor parameters directly throughout the class
public sealed class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
  private const int MaxLoggedBodyLength = 4096;

  public async Task InvokeAsync(HttpContext context)
  {
    var stopwatch = Stopwatch.StartNew();

    // 1. Read request body safely
    var requestBody = await ReadRequestBodyAsync(context);

    var originalResponseBody = context.Response.Body;
    await using var responseBuffer = new MemoryStream();
    context.Response.Body = responseBuffer;

    try
    {
      await next(context);
    }
    finally
    {
      stopwatch.Stop();

      // 2. Read response body before restoring the original stream
      var responseBody = await ReadResponseBodyAsync(responseBuffer, context.Response.ContentType);

      // 3. Ensure stream is copied back and original reference is restored safely
      responseBuffer.Position = 0;
      await responseBuffer.CopyToAsync(originalResponseBody);
      context.Response.Body = originalResponseBody;

      // 4. Structured logging
      var logMetadata = new
      {
        method = context.Request.Method,
        path = context.Request.Path.Value,
        statusCode = context.Response.StatusCode,
        elapsedMs = stopwatch.ElapsedMilliseconds,
        request = requestBody,
        response = responseBody
      };

      logger.LogInformation("HTTP {@HttpLog}", logMetadata);
    }
  }

  private static async Task<string?> ReadRequestBodyAsync(HttpContext context)
  {
    if (!CanLogBody(context.Request.ContentType)) return null;

    context.Request.EnableBuffering();

    if (context.Request.ContentLength is null or 0)
    {
      return null;
    }

    context.Request.Body.Position = 0;
    var body = await ReadStreamChunkAsync(context.Request.Body);
    context.Request.Body.Position = 0;

    return body;
  }

  private static async Task<string?> ReadResponseBodyAsync(MemoryStream responseBuffer, string? contentType)
  {
    if (!CanLogBody(contentType) || responseBuffer.Length == 0)
    {
      return null;
    }

    responseBuffer.Position = 0;
    var body = await ReadStreamChunkAsync(responseBuffer);

    return body;
  }

  // Optimized helper to read only up to MaxLoggedBodyLength to save memory allocations
  private static async Task<string> ReadStreamChunkAsync(Stream stream)
  {
    using var reader = new StreamReader(
        stream,
        Encoding.UTF8,
        detectEncodingFromByteOrderMarks: false,
        bufferSize: 1024,
        leaveOpen: true);

    var buffer = new char[MaxLoggedBodyLength];
    var bytesRead = await reader.ReadBlockAsync(buffer, 0, MaxLoggedBodyLength);

    var result = new string(buffer, 0, bytesRead);

    // Check if there is more data left in the stream to determine truncation
    if (stream.Position < stream.Length)
    {
      result += "...(truncated)";
    }

    return result;
  }

  private static bool CanLogBody(string? contentType)
  {
    if (string.IsNullOrWhiteSpace(contentType)) return false;

    return contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
           || contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
           || contentType.Contains("xml", StringComparison.OrdinalIgnoreCase);
  }
}