using MediatR;

using Medium.Api.Domain.User.Dtos;

namespace Medium.Api.Domain.User.Commands;

public record CreateUserCommand(
    string Name,
    string Email,
    string Password,
    string? Bio = null,
    string? AvatarUrl = null,
    IReadOnlyCollection<Guid>? RoleIds = null
) : IRequest<UserDto?>;