using FluentValidation;

using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Infrastructure.Validation;

public class ValidationMiddleware
{
  private readonly RequestDelegate _next;
  private readonly IServiceProvider _serviceProvider;

  public ValidationMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
  {
    _next = next;
    _serviceProvider = serviceProvider;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    var endpoint = context.GetEndpoint();
    if (endpoint == null)
    {
      await _next(context);
      return;
    }

    // Check if the endpoint has any parameters that need validation
    var validators = GetValidatorsForEndpoint(endpoint);
    if (validators.Any())
    {
      var validationResults = new List<FluentValidation.Results.ValidationResult>();

      foreach (var validatorType in validators)
      {
        var validator = _serviceProvider.GetService(validatorType);
        if (validator != null)
        {
          var validateMethod = validatorType.GetMethod("Validate", new[] { typeof(object) });
          if (validateMethod != null)
          {
            // This is a simplified validation - in production, you'd extract the actual command/query from the request
            var result = validateMethod.Invoke(validator, new object[] { context.Request });
            if (result is FluentValidation.Results.ValidationResult vr && !vr.IsValid)
            {
              validationResults.Add(vr);
            }
          }
        }
      }

      if (validationResults.Any(vr => !vr.IsValid))
      {
        var errors = validationResults
            .SelectMany(vr => vr.Errors)
            .Select(e => new { e.PropertyName, e.ErrorMessage })
            .ToList();

        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { Errors = errors });
        return;
      }
    }

    await _next(context);
  }

  private List<Type> GetValidatorsForEndpoint(Endpoint endpoint)
  {
    // This is a simplified version - in production, you'd inspect the endpoint metadata
    // to find the actual command/query types and their validators
    return new List<Type>();
  }
}