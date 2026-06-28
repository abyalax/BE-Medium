
using MediatR;

using Medium.Api.Common.Constant;
using Medium.Api.Domain.Article.Commands;
using Medium.Api.Domain.Article.Queries;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Http.Api.Version1.Article;

[ApiController]
[Route("api/v1/article")]
public class ArticleController(IMediator mediator, CurrentUser currentUser) : ControllerBase
{

  [HttpPost]
  [Authorize]
  public async Task<IActionResult> Create([FromBody] CreateArticleCommand command, CancellationToken cancellationToken)
  {
    var userId = currentUser.Id;
    var commandWithAuthor = command with { AuthorId = userId };
    var response = await mediator.Send(commandWithAuthor, cancellationToken);
    return StatusCode(StatusCodes.Status201Created, ApiResponseWriter.Success(response, "Created"));
  }

  [HttpPut("{id:guid}")]
  [Authorize]
  public async Task<IActionResult> Update(Guid id, [FromBody] UpdateArticleCommand command, CancellationToken cancellationToken)
  {
    var userId = currentUser.Id;
    var isAdmin = User.IsInRole(Roles.Admin);
    var commandWithUser = command with { ArticleId = id, UserId = userId, IsAdmin = isAdmin };
    var response = await mediator.Send(commandWithUser, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }

  [HttpDelete("{id:guid}")]
  [Authorize]
  public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
  {
    var userId = currentUser.Id;
    var isAdmin = User.IsInRole(Roles.Admin);
    var command = new DeleteArticleCommand(id, userId, isAdmin);
    await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(true, "Deleted"));
  }

  [HttpPost("{id:guid}/publish")]
  [Authorize]
  public async Task<IActionResult> Publish(Guid id, CancellationToken cancellationToken)
  {
    var userId = currentUser.Id;
    var isAdmin = User.IsInRole(Roles.Admin);
    var command = new PublishArticleCommand(id, userId, isAdmin);
    var response = await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }

  [HttpPost("{id:guid}/unpublish")]
  [Authorize]
  public async Task<IActionResult> Unpublish(Guid id, CancellationToken cancellationToken)
  {
    var userId = currentUser.Id;
    var isAdmin = User.IsInRole(Roles.Admin);
    var command = new UnpublishArticleCommand(id, userId, isAdmin);
    var response = await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }

  [HttpPost("{id:guid}/archive")]
  [Authorize]
  public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
  {
    var userId = currentUser.Id;
    var isAdmin = User.IsInRole(Roles.Admin);
    var command = new ArchiveArticleCommand(id, userId, isAdmin);
    var response = await mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }

  [HttpGet("{id:guid}")]
  [Authorize]
  public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
  {
    var query = new GetArticleByIdQuery(id);
    var response = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }

  [HttpGet]
  [Authorize]
  public async Task<IActionResult> List(
    [FromQuery] ListArticlesQuery query,
    CancellationToken cancellationToken = default
  )
  {
    var response = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }

  [HttpGet("search")]
  [AllowAnonymous]
  public async Task<IActionResult> Search([FromQuery] string searchTerm, [FromQuery] int page = 1, [FromQuery] int perPage = 10, CancellationToken cancellationToken = default)
  {
    var query = new SearchArticlesQuery(searchTerm)
    {
      Page = page,
      PerPage = perPage
    };
    var response = await mediator.Send(query, cancellationToken);
    return Ok(ApiResponseWriter.Success(response));
  }
}