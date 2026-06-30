using MediatR;

using Medium.Api.Common.Constant;
using Medium.Api.Domain.Tag.Commands;
using Medium.Api.Domain.Tag.Dtos;
using Medium.Api.Domain.Tag.Queries;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.Tag;

[ApiController]
[Route("api/v1/tags")]
[Tags("Tags")]
public class TagController(IMediator mediator) : ControllerBase
{
  [HttpGet]
  [Authorize(Permissions.Tags.Read)]
  public async Task<IActionResult> List(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default
  )
  {
    var query = new ListTagsQuery(page, pageSize);
    var result = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpGet("all")]
  [AllowAnonymous]
  public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
  {
    var query = new GetAllTagsQuery();
    var result = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpGet("{id:guid}")]
  [Authorize(Permissions.Tags.Read)]
  public async Task<IActionResult> GetById(
    [FromRoute] Guid id,
    CancellationToken cancellationToken = default
  )
  {
    var query = new GetTagByIdQuery(id);
    var result = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpPost]
  [Authorize(Permissions.Tags.Create)]
  public async Task<IActionResult> Create(
    [FromBody] Domain.Tag.Dtos.CreateTagRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var command = new CreateTagCommand(request);
    var result = await mediator.Send(command, cancellationToken);
    return StatusCode(StatusCodes.Status201Created, ApiResponseWriter.Success(result, "Created"));
  }

  [HttpPut("{id:guid}")]
  [Authorize(Permissions.Tags.Update)]
  public async Task<IActionResult> Update(
    [FromRoute] Guid id,
    [FromBody] Domain.Tag.Dtos.UpdateTagRequest request,
    CancellationToken cancellationToken = default
  )
  {
    var command = new UpdateTagCommand(id, request);
    var result = await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(result));
  }

  [HttpDelete("{id:guid}")]
  [Authorize(Permissions.Tags.Delete)]
  public async Task<IActionResult> Delete(
    [FromRoute] Guid id,
    CancellationToken cancellationToken = default
  )
  {
    var command = new DeleteTagCommand(id);
    await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(null, "Deleted"));
  }
}