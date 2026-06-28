using MediatR;

using Medium.Api.Domain.Auth.Dtos;

namespace Medium.Api.Domain.Auth.Queries;

public record GetPermissionsByUserQuery(Guid UserId) : IRequest<IEnumerable<PermissionResponse>>;