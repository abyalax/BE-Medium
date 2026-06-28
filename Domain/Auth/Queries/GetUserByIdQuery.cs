using MediatR;

using Medium.Api.Domain.User.Dtos;

namespace Medium.Api.Domain.Auth.Queries;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;