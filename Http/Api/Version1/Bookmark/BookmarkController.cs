
using MediatR;

using Medium.Api.Common.Constant;
using Medium.Api.Domain.Bookmark.Command;
using Medium.Api.Domain.Bookmark.Queries;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.Bookmark;

[ApiController]
[Route("api/v1/bookmark")]
public class BookmarkController(IMediator mediator, CurrentUser currentUser) : ControllerBase
{

  [HttpPost]
  [Authorize(Permissions.Bookmarks.Create)]
  public async Task<IActionResult> Create([FromBody] CreateBookmarkCommand command, CancellationToken cancellationToken)
  {
    var response = await mediator.Send(
      new CreateBookmarkCommand(
        UserId: currentUser.Id,
        ArticleId: command.ArticleId
      ),
      cancellationToken
    );
    return StatusCode(StatusCodes.Status201Created, ApiResponseWriter.Success(response, "Created"));
  }

  [HttpDelete("{id:guid}")]
  [Authorize(Permissions.Bookmarks.Delete)]
  public async Task<IActionResult> DeleteById(Guid id, CancellationToken cancellationToken)
  {
    var command = new DeleteBookmarkByIdCommand(
      UserId: currentUser.Id,
      id
    );
    await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(true, "Deleted"));
  }

  [HttpDelete("/article/{id:guid}")]
  [Authorize]
  public async Task<IActionResult> DeleteByArticle(Guid id, CancellationToken cancellationToken)
  {
    var command = new DeleteBookmarkByArticleCommand(
      UserId: currentUser.Id,
      ArticleId: id
    );
    await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(true, "Deleted"));
  }

  [HttpGet("{id:guid}")]
  [Authorize(Permissions.Bookmarks.Read)]
  public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
  {
    var command = new GetBookmarByIdQuery(
      BookmarkId: id
    );
    var response = await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }

  [HttpGet("/user")]
  [Authorize(Permissions.Bookmarks.Read)]
  public async Task<IActionResult> GetByUser([FromQuery] GetBookmarkByUserQuery query, CancellationToken cancellationToken)
  {
    var response = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }



}