using MediatR;

using Medium.Api.Domain.User.Commands;
using Medium.Api.Domain.User.Queries;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.User;

[ApiController]
[Route("api/v1/users")]
[Tags("Users")]
public class UserController(IMediator mediator, CurrentUser currentUser) : ControllerBase
{
  [HttpGet("me")]
  [Authorize]
  public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
  {
    // Get the current logged-in user's ID
    var userId = currentUser.Id;

    var query = new GetUserByIdQuery(userId);
    var result = await mediator.Send(query, cancellationToken);

    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpGet]
  [Authorize(Permissions.Users.Get)]
  public async Task<IActionResult> List(
    [FromQuery] ListUserQuery query,
    CancellationToken cancellationToken
  )
  {
    var result = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpGet("{id:guid}")]
  [Authorize(Permissions.Users.Get)]
  public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
  {
    var query = new GetUserByIdQuery(id);
    var result = await mediator.Send(query, cancellationToken);

    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpPost]
  [Authorize(Permissions.Users.Create)]
  public async Task<IActionResult> Create([FromBody] CreateUserCommand request, CancellationToken cancellationToken)
  {
    var result = await mediator.Send(request, cancellationToken);
    return StatusCode(StatusCodes.Status201Created, ApiResponseWriter.Success(result, "Created"));
  }

  [HttpPut("{id:guid}")]
  [Authorize(Permissions.Users.Update)]
  public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateUserCommand request, CancellationToken cancellationToken)
  {
    var command = new UpdateUserCommand(id, request.Name, request.Email, request.Bio, request.AvatarUrl);
    var result = await mediator.Send(command, cancellationToken);

    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpPost("{id:guid}/roles")]
  [Authorize(Permissions.Users.AssignRoles)]
  public async Task<IActionResult> AssignRoles([FromRoute] Guid id, [FromBody] AssignUserRoleCommand request, CancellationToken cancellationToken)
  {
    var command = new AssignUserRoleCommand(id, request.RoleIds);
    await mediator.Send(command, cancellationToken);

    return Ok(ApiResponseWriter.Success(null, "Assigned"));
  }

  [HttpDelete("{id:guid}")]
  [Authorize(Permissions.Users.Delete)]
  public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
  {
    var command = new DeleteUserCommand(id);
    await mediator.Send(command, cancellationToken);

    return Ok(ApiResponseWriter.Success(null, "Deleted"));
  }
}