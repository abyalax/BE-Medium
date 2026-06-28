using MediatR;

using Medium.Api.Domain.User.Dtos;

namespace Medium.Api.Domain.User.Commands;

public record UpdateUserCommand(
    Guid UserId,
    string Name,
    string Email,
    string? Bio = null,
    string? AvatarUrl = null
) : IRequest<UserDto?>;