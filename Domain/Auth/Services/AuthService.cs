using Medium.Api.Domain.Auth.DTOs;
using Medium.Api.Domain.User.Dtos;
using Medium.Api.Domain.User.Repositories;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Database;
using Medium.Api.Infrastructure.Exceptions;

using Microsoft.EntityFrameworkCore;

using UserModel = Medium.Api.Models.User;
using UserRoleModel = Medium.Api.Models.UserRole;

namespace Medium.Api.Domain.Auth.Services;

public class AuthService(
    ApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    UserRepository userRepository)
{
  private readonly ApplicationDbContext _context = context;
  private readonly IPasswordHasher _passwordHasher = passwordHasher;
  private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
  private readonly UserRepository _userRepository = userRepository;

  public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
  {
    var existingUser = await _context.Users
        .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

    if (existingUser != null)
    {
      throw new ConflictException("User with this email already exists");
    }

    var user = new UserModel
    {
      Id = Guid.NewGuid(),
      Name = request.Name,
      Email = request.Email,
      Password = _passwordHasher.HashPassword(request.Password),
      Bio = request.Bio,
      AvatarUrl = request.AvatarUrl,
      CreatedAt = DateTime.UtcNow,
      UpdatedAt = DateTime.UtcNow
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync(cancellationToken);

    var defaultRole = await _context.Roles
        .FirstOrDefaultAsync(r => r.Name == "Reader", cancellationToken);

    if (defaultRole != null)
    {
      _context.UserRoles.Add(new UserRoleModel
      {
        UserId = user.Id,
        RoleId = defaultRole.Id
      });
      await _context.SaveChangesAsync(cancellationToken);
    }

    var userWithRoles = await _userRepository.FindByEmailWithRolePermissionAsync(user.Email, cancellationToken);
    var token = _jwtTokenGenerator.GenerateToken(
        user.Id,
        user.Email,
        userWithRoles?.Roles.Select(role => role.Name) ?? [],
        GetPermissions(userWithRoles?.Roles ?? []));

    return new AuthResponse(
        user.Id,
        user.Name,
        user.Email,
        token,
        userWithRoles?.Roles ?? [],
        user.Bio,
        user.AvatarUrl
    );
  }

  public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
  {
    var user = await _userRepository.FindByEmailWithRolePermissionAsync(request.Email, cancellationToken);

    if (user == null)
    {
      throw new UnauthenticatedException("Invalid email or password");
    }

    if (!_passwordHasher.VerifyPassword(request.Password, user.Password))
    {
      throw new UnauthenticatedException("Invalid email or password");
    }

    var roles = user.Roles.Select(role => role.Name);
    var token = _jwtTokenGenerator.GenerateToken(user.Id, user.Email, roles, GetPermissions(user.Roles));

    return new AuthResponse(
        user.Id,
        user.Name,
        user.Email,
        token,
        user.Roles,
        user.Bio,
        user.AvatarUrl
    );
  }

  public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default)
  {
    var userPermissions = await _context.UserRoles
        .Where(ur => ur.UserId == userId)
        .Join(
            _context.RolePermissions,
            ur => ur.RoleId,
            rp => rp.RoleId,
            (ur, rp) => rp.PermissionId
        )
        .Join(
            _context.Permissions,
            pId => pId,
            p => p.Id,
            (pId, p) => p.Code
        )
        .Distinct()
        .ToListAsync(cancellationToken);

    return userPermissions.Contains(permissionCode);
  }

  public async Task<bool> HasRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
  {
    return await _context.UserRoles
        .Where(ur => ur.UserId == userId)
        .Join(
            _context.Roles,
            ur => ur.RoleId,
            r => r.Id,
            (ur, r) => r.Name
        )
        .AnyAsync(r => r == roleName, cancellationToken);
  }

  private static IEnumerable<string> GetPermissions(IReadOnlyCollection<RoleWithPermissionsResponse> roles)
  {
    return roles
        .SelectMany(role => role.Permissions)
        .Select(permission => permission.Code)
        .Distinct(StringComparer.OrdinalIgnoreCase);
  }

}