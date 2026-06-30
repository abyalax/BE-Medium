using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using MediatR;

using Medium.Api.Common.Constant;
using Medium.Api.Domain.Follow.Commands;
using Medium.Api.Domain.Follow.Dtos;
using Medium.Api.Domain.Follow.Queries;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.Follow;

[ApiController]
[Route("api/v1/follows")]
[Tags("Follows")]
public class FollowController(IMediator mediator, CurrentUser currentUser) : ControllerBase
{
  [HttpGet("followers/{userId:guid}")]
  [Authorize(Permissions.Authors.Follow)]
  public async Task<IActionResult> GetFollowers(
    [FromRoute] Guid userId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default
  )
  {
    var query = new GetFollowersQuery(userId, page, pageSize);
    var result = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpGet("following/{userId:guid}")]
  [Authorize(Permissions.Authors.Follow)]
  public async Task<IActionResult> GetFollowing(
    [FromRoute] Guid userId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default
  )
  {
    var query = new GetFollowingQuery(userId, page, pageSize);
    var result = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpPost]
  [Authorize(Permissions.Authors.Follow)]
  public async Task<IActionResult> Create(
    [FromBody] Domain.Follow.Dtos.FollowRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var followerId = currentUser.Id;
    var command = new CreateFollowCommand(followerId, request);
    var result = await mediator.Send(command, cancellationToken);
    return StatusCode(StatusCodes.Status201Created, ApiResponseWriter.Success(result, "Created"));
  }

  [HttpDelete("{id:guid}")]
  [Authorize(Permissions.Authors.Follow)]
  public async Task<IActionResult> Delete(
    [FromRoute] Guid id,
    CancellationToken cancellationToken = default
  )
  {
    var currentUserId = currentUser.Id;
    var command = new DeleteFollowCommand(id, currentUserId);
    await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(null, "Unfollowed"));
  }

  [HttpDelete("by-user/{followingId:guid}")]
  [Authorize(Permissions.Authors.Follow)]
  public async Task<IActionResult> UnfollowByUser(
    [FromRoute] Guid followingId,
    CancellationToken cancellationToken = default
  )
  {
    var followerId = currentUser.Id;
    var command = new UnfollowCommand(followerId, followingId);
    await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(null, "Unfollowed"));
  }
}