using MediatR;

using Medium.Api.Common.Constant;
using Medium.Api.Domain.Auth.Commands;
using Medium.Api.Domain.Auth.Queries;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.Auth;

[ApiController]
[Route("api/roles")]
[Tags("Roles")]
public class RoleController(IMediator mediator) : ControllerBase
{
  [HttpPost]
  [Authorize(Permissions.Roles.Create)]
  public async Task<IActionResult> Create([FromBody] CreateRoleCommand request, CancellationToken cancellationToken)
  {
    var command = new CreateRoleCommand(request.Name, request.Description);
    var result = await mediator.Send(command, cancellationToken);

    return StatusCode(StatusCodes.Status201Created, ApiResponseWriter.Success(result, "Created"));
  }

  [HttpGet]
  [Authorize(Permissions.Roles.Get)]
  public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
  {
    var query = new GetAllRolesQuery();
    var result = await mediator.Send(query, cancellationToken);

    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpGet("{roleId:guid}")]
  [Authorize(Permissions.Roles.Get)]
  public async Task<IActionResult> GetById([FromRoute] Guid roleId, CancellationToken cancellationToken)
  {
    var query = new GetRoleByIdQuery(roleId);
    var result = await mediator.Send(query, cancellationToken);

    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpPut("{roleId:guid}")]
  [Authorize(Permissions.Roles.Update)]
  public async Task<IActionResult> Update(
      [FromRoute] Guid roleId,
      [FromBody] CreateRoleCommand request,
      CancellationToken cancellationToken)
  {
    var command = new UpdateRoleCommand(roleId, request.Name, request.Description);
    var result = await mediator.Send(command, cancellationToken);

    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpDelete("{roleId:guid}")]
  [Authorize(Permissions.Roles.Delete)]
  public async Task<IActionResult> Delete([FromRoute] Guid roleId, CancellationToken cancellationToken)
  {
    var command = new DeleteRoleCommand(roleId);
    await mediator.Send(command, cancellationToken);

    return Ok(ApiResponseWriter.Success(null, "Deleted"));
  }

  [HttpPost("{roleId:guid}/permissions/{permissionId:guid}")]
  [Authorize(Permissions.Roles.AssignPermissions)]
  public async Task<IActionResult> AssignPermission(
      [FromRoute] Guid roleId,
      [FromRoute] Guid permissionId,
      CancellationToken cancellationToken)
  {
    var command = new AssignPermissionToRoleCommand(roleId, permissionId);
    await mediator.Send(command, cancellationToken);

    return Ok(ApiResponseWriter.Success(null, "Assigned"));
  }

  [HttpPost("users/{userId:guid}/roles/{roleId:guid}")]
  [Authorize(Permissions.Roles.AssignUsers)]
  public async Task<IActionResult> AssignRole(
      [FromRoute] Guid userId,
      [FromRoute] Guid roleId,
      CancellationToken cancellationToken)
  {
    var command = new AssignRoleToUserCommand(userId, roleId);
    await mediator.Send(command, cancellationToken);

    return Ok(ApiResponseWriter.Success(null, "Assigned"));
  }
}