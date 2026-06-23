using System.Net;

namespace Medium.Api.Infrastructure.Http;

public static class ApiResponseWriter
{
    public static ApiResponse Success(object? data = null, string message = "Success", int statusCode = StatusCodes.Status200OK)
    {
        return new ApiResponse(
            statusCode,
            ReasonPhrase(statusCode),
            message,
            null,
            data);
    }

    public static async Task WriteAsync(
        HttpContext context,
        int statusCode,
        string error,
        string message,
        object? errors = null)
    {
        if (context.Response.HasStarted)
        {
            throw new InvalidOperationException("The response has already started.");
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new ApiResponse(
            statusCode,
            error,
            message,
            errors,
            null));
    }

    public static string ReasonPhrase(int statusCode)
    {
        return ((HttpStatusCode)statusCode).ToString();
    }
}
