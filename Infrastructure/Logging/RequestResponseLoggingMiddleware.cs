using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;

namespace Medium.Api.Infrastructure.Logging;

public sealed class RequestResponseLoggingMiddleware
{
    private const int MaxLoggedBodyLength = 4096;
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestBody = await ReadRequestBodyAsync(context);

        var originalResponseBody = context.Response.Body;
        await using var responseBuffer = new MemoryStream();
        context.Response.Body = responseBuffer;

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            responseBuffer.Position = 0;
            await responseBuffer.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;

            var logRequest = new
            {
                method = context.Request.Method,
                path = context.Request.Path.Value,
                body = requestBody,
            };
            _logger.LogInformation("{@request_log}", logRequest);

        }
    }

    private static async Task<string?> ReadRequestBodyAsync(HttpContext context)
    {
        if (!CanLogBody(context.Request.ContentType))
        {
            return null;
        }

        context.Request.EnableBuffering();

        if (context.Request.ContentLength is null or 0)
        {
            context.Request.Body.Position = 0;
            return null;
        }

        using var reader = new StreamReader(
            context.Request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;
        return Truncate(body);
    }

    private static async Task<string?> ReadResponseBodyAsync(Stream responseBuffer, string? contentType)
    {
        if (!CanLogBody(contentType))
        {
            return null;
        }

        responseBuffer.Position = 0;
        using var reader = new StreamReader(
            responseBuffer,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync();
        return Truncate(body);
    }

    private static bool CanLogBody(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return contentType.Contains("json", StringComparison.OrdinalIgnoreCase)
               || contentType.StartsWith("text/", StringComparison.OrdinalIgnoreCase)
               || contentType.Contains("xml", StringComparison.OrdinalIgnoreCase);
    }

    private static string Truncate(string body)
    {
        if (string.IsNullOrEmpty(body))
        {
            return body;
        }

        return body.Length <= MaxLoggedBodyLength
            ? body
            : body[..MaxLoggedBodyLength] + "...(truncated)";
    }
}
