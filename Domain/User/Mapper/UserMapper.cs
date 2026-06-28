using Medium.Api.Domain.User.Dtos;

using PermissionModel = Medium.Api.Models.Permission;
using RoleModel = Medium.Api.Models.Role;
using UserModel = Medium.Api.Models.User;

namespace Medium.Api.Domain.User.Mapper;

public static class UserMapper
{
  public static UserDto ToResponse(UserModel user)
  {
    var roles = user.UserRoles?
        .Select(ur => ur.Role)
        .Where(r => r != null)
        .Select(ToRoleDto)
        .ToList() ?? [];

    return new UserDto(
        user.Id,
        user.Name,
        user.Email,
        user.Bio,
        user.AvatarUrl,
        [.. roles],
        user.CreatedAt,
        user.UpdatedAt
    );
  }

  public static UserWithPasswordDto ToResponseWithPassword(UserModel user)
  {
    var roles = user.UserRoles?
        .Select(ur => ur.Role)
        .Where(r => r != null)
        .Select(ToRoleDto)
        .ToList() ?? [];

    return new UserWithPasswordDto(
        user.Id,
        user.Name,
        user.Email,
        user.Password,
        user.Bio,
        user.AvatarUrl,
        [.. roles],
        user.CreatedAt,
        user.UpdatedAt
    );
  }

  public static IReadOnlyCollection<UserDto> ToResponseList(IEnumerable<UserModel> users)
  {
    return [.. users.Select(ToResponse)];
  }

  private static RoleWithPermissionsDto ToRoleDto(RoleModel role)
  {
    var permissions = role.RolePermissions?
        .Select(rp => rp.Permission)
        .Where(p => p != null)
        .Select(ToPermissionDto)
        .ToList() ?? [];

    return new RoleWithPermissionsDto(
        role.Id,
        role.Name,
        role.Description,
        [.. permissions]
    );
  }

  private static PermissionDto ToPermissionDto(PermissionModel permission)
  {
    return new PermissionDto(
        permission.Id,
        permission.Code,
        permission.Name,
        permission.Description
    );
  }
}