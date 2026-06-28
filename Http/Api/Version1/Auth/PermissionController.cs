using MediatR;

using Medium.Api.Common.Constant;
using Medium.Api.Domain.Auth.Queries;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.Auth;

[ApiController]
[Route("api/permissions")]
[Tags("Permissions")]
public class PermissionController(IMediator mediator) : ControllerBase
{
  [HttpGet]
  [Authorize(Permissions.PermissionsModule.Get)]
  public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
  {
    var query = new GetAllPermissionsQuery();
    var result = await mediator.Send(query, cancellationToken);

    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpGet("{permissionId:guid}")]
  [Authorize(Permissions.PermissionsModule.Get)]
  public async Task<IActionResult> GetById([FromRoute] Guid permissionId, CancellationToken cancellationToken)
  {
    var query = new GetPermissionByIdQuery(permissionId);
    var result = await mediator.Send(query, cancellationToken);

    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpGet("roles/{roleId:guid}")]
  [Authorize(Permissions.PermissionsModule.Get)]
  public async Task<IActionResult> GetByRole([FromRoute] Guid roleId, CancellationToken cancellationToken)
  {
    var query = new GetPermissionsByRoleQuery(roleId);
    var result = await mediator.Send(query, cancellationToken);

    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpGet("users/{userId:guid}")]
  [Authorize(Permissions.PermissionsModule.Get)]
  public async Task<IActionResult> GetByUser([FromRoute] Guid userId, CancellationToken cancellationToken)
  {
    var query = new GetPermissionsByUserQuery(userId);
    var result = await mediator.Send(query, cancellationToken);

    return Ok(ApiResponseWriter.Success(result));
  }
}