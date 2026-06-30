
using MediatR;

using Medium.Api.Common.Constant;
using Medium.Api.Domain.Comment.Commands;
using Medium.Api.Domain.Comment.Queries;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.Comment;

[ApiController]
[Route("api/v1/comment")]
public class CommentController(IMediator mediator, CurrentUser currentUser) : ControllerBase
{

  [HttpPost]
  [Authorize]
  public async Task<IActionResult> Create(
    [FromBody] CreateCommentCommand command,
    CancellationToken cancellationToken
  )
  {
    var userId = currentUser.Id;
    var commandWithAuthor = command with { UserId = userId };
    var response = await mediator.Send(commandWithAuthor, cancellationToken);
    return StatusCode(StatusCodes.Status201Created, ApiResponseWriter.Success(response, "Created"));
  }

  [HttpPut("{id:guid}")]
  [Authorize]
  public async Task<IActionResult> Update(
    Guid id,
    [FromBody] UpdateCommentCommand command,
    CancellationToken cancellationToken
  )
  {
    var userId = currentUser.Id;
    var commandWithUser = command with
    {
      CommentId = id,
      UserId = userId
    };
    var response = await mediator.Send(commandWithUser, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }

  [HttpDelete("{id:guid}")]
  [Authorize]
  public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
  {
    var userId = currentUser.Id;
    var isAdmin = User.IsInRole(Roles.Admin);
    var command = new DeleteCommentCommand(id, userId, isAdmin);
    await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(true, "Deleted"));
  }

  [HttpGet("{id:guid}")]
  [Authorize]
  public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
  {
    var query = new GetCommentByIdQuery(id);
    var response = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }

  [HttpGet("user/{id:guid}")]
  [Authorize]
  public async Task<IActionResult> ListByUser(
    Guid id,
    CancellationToken cancellationToken = default
  )
  {
    var query = new ListCommentByUserIdQuery(id);
    var response = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }

  [HttpGet("article/{id:guid}")]
  [Authorize]
  public async Task<IActionResult> ListByArticle(
    Guid id,
    CancellationToken cancellationToken = default
  )
  {
    var query = new ListCommentByArticleIdQuery(id);
    var response = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }

}