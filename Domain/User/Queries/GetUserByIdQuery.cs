using MediatR;

using Medium.Api.Domain.User.Dtos;

namespace Medium.Api.Domain.User.Queries;

public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;