using MediatR;

using Medium.Api.Domain.Auth.Dtos;

namespace Medium.Api.Domain.Auth.Queries;

public record GetAllRolesQuery() : IRequest<IEnumerable<RoleResponse>>;