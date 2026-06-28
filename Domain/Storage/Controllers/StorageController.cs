using MediatR;

using Medium.Api.Domain.Storage.Commands;
using Medium.Api.Domain.Storage.Queries;
using Medium.Api.Infrastructure.Http;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Medium.Api.Domain.Storage.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class StorageController : ControllerBase
{
  private readonly IMediator _mediator;

  public StorageController(IMediator mediator)
  {
    _mediator = mediator;
  }

  [HttpPost("upload")]
  [Authorize]
  public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string bucketName = "medium-storage", CancellationToken cancellationToken = default)
  {
    if (file == null || file.Length == 0)
    {
      return BadRequest(ApiResponseWriter.Error("No file uploaded"));
    }

    var command = new UploadFileCommand(
        file.OpenReadStream(),
        file.FileName,
        file.ContentType,
        bucketName
    );

    var response = await _mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(response, "File uploaded successfully"));
  }

  [HttpGet("download")]
  [Authorize]
  public async Task<IActionResult> DownloadFile([FromQuery] string bucketName, [FromQuery] string objectName, CancellationToken cancellationToken = default)
  {
    var query = new DownloadFileQuery(bucketName, objectName);
    var stream = await _mediator.Send(query, cancellationToken);

    return File(stream, "application/octet-stream", objectName);
  }

  [HttpDelete("delete")]
  [Authorize]
  public async Task<IActionResult> DeleteFile([FromQuery] string bucketName, [FromQuery] string objectName, CancellationToken cancellationToken = default)
  {
    var command = new DeleteFileCommand(bucketName, objectName);
    await _mediator.Send(command, cancellationToken);
    return Ok(ApiResponseWriter.Success(true, "File deleted successfully"));
  }
}