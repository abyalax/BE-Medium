
using MediatR;

using Medium.Api.Common.Constant;
using Medium.Api.Domain.ReadingHistory.Command;
using Medium.Api.Domain.ReadingHistory.Queries;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.ReadingHistory;

[ApiController]
[Route("api/v1/reading-history")]
public class ReadingHistoryController(IMediator mediator, CurrentUser currentUser) : ControllerBase
{

  [HttpPost]
  [Authorize(Permissions.ReadingHistory.Create)]
  public async Task<IActionResult> Create([FromBody] CreateReadingHistoryCommand command, CancellationToken cancellationToken)
  {
    var response = await mediator.Send(
      new CreateReadingHistoryCommand(
        UserId: currentUser.Id,
        ArticleId: command.ArticleId,
        DurationSeconds: command.DurationSeconds
      ),
      cancellationToken
    );
    return StatusCode(StatusCodes.Status201Created, ApiResponseWriter.Success(response, "Created Reading History"));
  }

  [HttpDelete("{id:guid}")]
  [Authorize(Permissions.ReadingHistory.Delete)]
  public async Task<IActionResult> DeleteById(Guid id, CancellationToken cancellationToken)
  {
    var command = new DeleteReadingHistoryCommand(
      UserId: currentUser.Id,
      ReadingHistoryId: id,
      IsAdmin: User.IsInRole(Roles.Admin)
    );
    await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(true, "Deleted Reading History"));
  }

  [HttpGet("{id:guid}")]
  [Authorize(Permissions.ReadingHistory.Read)]
  public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
  {
    var command = new GetReadingHistoryByIdQuery(
      ReadingHistoryId: id
    );
    var response = await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }

  [HttpGet("user/{id:guid}")]
  [Authorize]
  public async Task<IActionResult> ListByUser(
   Guid id,
   CancellationToken cancellationToken = default
 )
  {
    var query = new ListReadingHistoryByUserQuery(id);
    var response = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }
}