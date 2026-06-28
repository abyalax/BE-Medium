using MediatR;

using Medium.Api.Domain.Auth.Dtos;

namespace Medium.Api.Domain.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthDto>;