using FluentValidation;

using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Infrastructure.Http;

namespace Medium.Api.Infrastructure.Filters;

public sealed class ExceptionHandling(RequestDelegate next)
{
  private readonly RequestDelegate _next = next;

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (ValidationException ex)
    {
      await ApiResponseWriter.WriteAsync(
          context,
          StatusCodes.Status400BadRequest,
          "BadRequest",
          "Validation failed",
          ex.Errors
          .GroupBy(error => error.PropertyName)
          .ToDictionary(
              group => group.Key,
              group => group.Select(error => error.ErrorMessage).ToArray()));
    }
    catch (ApiException ex)
    {
      await ApiResponseWriter.WriteAsync(
          context,
          (int)ex.StatusCode,
          ApiResponseWriter.ReasonPhrase((int)ex.StatusCode),
          ex.Message);
    }
    catch (KeyNotFoundException ex)
    {
      await ApiResponseWriter.WriteAsync(
          context,
          StatusCodes.Status404NotFound,
          "NotFound",
          ex.Message);
    }
    catch (UnauthorizedAccessException)
    {
      await ApiResponseWriter.WriteAsync(
          context,
          StatusCodes.Status401Unauthorized,
          "Unauthorized",
          "Invalid credentials");
    }
    catch (InvalidOperationException ex)
    {
      await ApiResponseWriter.WriteAsync(
          context,
          StatusCodes.Status409Conflict,
          "Conflict",
          ex.Message);
    }
    catch (ArgumentException ex)
    {
      await ApiResponseWriter.WriteAsync(
          context,
          StatusCodes.Status400BadRequest,
          "BadRequest",
          ex.Message);
    }
  }
}