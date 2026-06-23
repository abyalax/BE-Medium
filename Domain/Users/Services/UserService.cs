using Medium.Api.Domain.Users.Dtos;
using Medium.Api.Domain.Users.Repositories;
using Medium.Api.Infrastructure.Auth;
using Medium.Api.Infrastructure.Exceptions;
using Medium.Api.Models;

namespace Medium.Api.Domain.Users.Services;

public class UserService
{
    private const int MaxPageSize = 100;
    private readonly UserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    private readonly string messageNotFound = "User not found";

    public UserService(UserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken: cancellationToken))
        {
            throw new ConflictException("User with this email already exists");
        }

        var roleIds = await ResolveRoleIdsAsync(request.RoleIds, cancellationToken);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Password = _passwordHasher.HashPassword(request.Password),
            Bio = request.Bio,
            AvatarUrl = request.AvatarUrl
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        await _userRepository.ReplaceUserRolesAsync(user.Id, roleIds, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return await GetByEmailOrThrowAsync(user.Email, cancellationToken);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(messageNotFound);

        return await GetByEmailOrThrowAsync(user.Email, cancellationToken);
    }

    public async Task<UserResponse> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new BadRequestException("Email is required");
        }

        return await GetByEmailOrThrowAsync(email, cancellationToken);
    }

    public async Task<PagedResponse<UserResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, MaxPageSize);

        var totalItems = await _userRepository.CountAsync(cancellationToken);
        var items = await _userRepository.ListAsync(page, pageSize, cancellationToken);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResponse<UserResponse>(
            [.. items.Select(ToResponse)],
            page,
            pageSize,
            totalItems,
            totalPages);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(messageNotFound);

        if (await _userRepository.ExistsByEmailAsync(request.Email, id, cancellationToken))
        {
            throw new ConflictException("User with this email already exists");
        }

        user.Name = request.Name;
        user.Email = request.Email;
        user.Bio = request.Bio;
        user.AvatarUrl = request.AvatarUrl;

        await _userRepository.SaveChangesAsync(cancellationToken);

        return await GetByEmailOrThrowAsync(user.Email, cancellationToken);
    }

    public async Task AssignRolesAsync(Guid userId, IReadOnlyCollection<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        _ = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(messageNotFound);

        if (roleIds.Count == 0)
        {
            throw new BadRequestException("At least one role is required");
        }

        var roles = await _userRepository.GetRolesByIdsAsync(roleIds, cancellationToken);
        if (roles.Count != roleIds.Distinct().Count())
        {
            throw new NotFoundException("One or more roles were not found");
        }

        await _userRepository.ReplaceUserRolesAsync(userId, roleIds, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(messageNotFound);

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<Guid>> ResolveRoleIdsAsync(IReadOnlyCollection<Guid>? roleIds, CancellationToken cancellationToken)
    {
        if (roleIds is { Count: > 0 })
        {
            var roles = await _userRepository.GetRolesByIdsAsync(roleIds, cancellationToken);
            if (roles.Count != roleIds.Distinct().Count())
            {
                throw new NotFoundException("One or more roles were not found");
            }

            return roleIds.Distinct().ToArray();
        }

        var readerRole = await _userRepository.GetRoleByNameAsync("Reader", cancellationToken)
            ?? throw new NotFoundException("Reader role is not available");

        return [readerRole.Id];
    }

    private async Task<UserResponse> GetByEmailOrThrowAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByEmailAsync(email, cancellationToken)
            ?? throw new NotFoundException(messageNotFound);

        return ToResponse(user);
    }

    private static UserResponse ToResponse(UserWithRolesPermissionsData user)
    {
        return new UserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.Bio,
            user.AvatarUrl,
            user.Roles,
            user.CreatedAt,
            user.UpdatedAt);
    }

}
