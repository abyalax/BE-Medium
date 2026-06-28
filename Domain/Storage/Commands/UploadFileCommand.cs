using MediatR;

namespace Medium.Api.Domain.Storage.Commands;

public record UploadFileCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    string BucketName,
    Dictionary<string, string>? Metadata = null
) : IRequest<UploadedFileResponse>;