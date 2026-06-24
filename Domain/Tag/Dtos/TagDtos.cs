namespace Medium.Api.Domain.Tag.Dtos;

public record CreateTagRequest(string Name);

public record UpdateTagRequest(string Name);

public record TagResponse(
    Guid Id,
    string Name,
    string Slug,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record PagedTagResponse(
    IReadOnlyCollection<TagResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);