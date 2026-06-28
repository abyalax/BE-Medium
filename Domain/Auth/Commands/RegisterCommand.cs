using MediatR;

using Medium.Api.Domain.Auth.Dtos;

namespace Medium.Api.Domain.Auth.Commands;

public record RegisterCommand(string Name, string Email, string Password, string? Bio = null, string? AvatarUrl = null) : IRequest<AuthDto>;