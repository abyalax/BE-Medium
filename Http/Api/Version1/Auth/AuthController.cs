using MediatR;

using Medium.Api.Domain.Auth.Commands;
using Medium.Api.Domain.Auth.Queries;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.Auth;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator, CurrentUser currentUser) : ControllerBase
{
  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
  {
    var response = await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
  {
    var response = await mediator.Send(command, cancellationToken);
    return StatusCode(StatusCodes.Status201Created, ApiResponseWriter.Success(response, "Created"));
  }

  [HttpGet("profile")]
  [Authorize]
  public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
  {
    var userId = currentUser.Id;
    var query = new GetUserByIdQuery(userId);
    var response = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }
}