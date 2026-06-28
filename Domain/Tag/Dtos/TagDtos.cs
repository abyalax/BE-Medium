namespace Medium.Api.Domain.Tag.Dtos;

public record CreateTagRequest(string Name);

public record UpdateTagRequest(string Name);

public record TagDto(
    Guid Id,
    string Name,
    string Slug
);

public record PagedTagDto(
    IReadOnlyCollection<TagDto> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);