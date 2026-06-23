namespace Medium.Api.Infrastructure.Http;

public sealed record ApiResponse(
    int Status,
    string Error,
    string Message,
    object? Errors = null,
    object? Data = null);
