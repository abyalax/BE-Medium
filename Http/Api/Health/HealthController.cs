using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Medium.Api.Http.Api.Health;

[ApiController]
[Route("api/v1/health")]
[AllowAnonymous]
public class HealthCheckController(HealthCheckService healthCheckService) : ControllerBase
{
  [HttpGet("live")]
  public async Task<IActionResult> GetLive(CancellationToken cancellationToken)
  {
    var report = await healthCheckService.CheckHealthAsync(
        predicate: check => check.Tags.Contains("live"),
        cancellationToken
      );

    if (report.Status == HealthStatus.Unhealthy)
      return StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponseWriter.Error("Application is unhealthy"));

    return Ok(ApiResponseWriter.Success(new { status = report.Status.ToString() }));
  }

  [HttpGet("ready")]
  public async Task<IActionResult> GetReady(CancellationToken cancellationToken)
  {
    var report = await healthCheckService.CheckHealthAsync(
        predicate: check => check.Tags.Contains("ready"),
        cancellationToken
      );

    var response = new
    {
      status = report.Status.ToString(),
      totalDuration = report.TotalDuration,
      dependencies = report.Entries.Select(e => new
      {
        name = e.Key,
        status = e.Value.Status.ToString(),
        duration = e.Value.Duration,
        error = e.Value.Exception?.Message
      })
    };

    if (report.Status == HealthStatus.Unhealthy)
      return StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponseWriter.Error("Application does'nt ready"));

    return Ok(ApiResponseWriter.Success(response));
  }
}