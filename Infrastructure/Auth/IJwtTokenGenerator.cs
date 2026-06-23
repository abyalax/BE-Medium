namespace Medium.Api.Infrastructure.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string email, IEnumerable<string> roles, IEnumerable<string> permissions);
}
