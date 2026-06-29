using Medium.Api.Infrastructure.Auth;
using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;

using PermissionConstant = Medium.Api.Common.Constant.Permissions;

namespace Medium.Api.Infrastructure.Database.Seeds;

public static class DatabaseSeeder
{
  private static readonly Guid ReaderRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
  private static readonly Guid AuthorRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
  private static readonly Guid AdminRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

  public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
  {
    var context = services.GetRequiredService<ApplicationDbContext>();
    var passwordHasher = services.GetRequiredService<IPasswordHasher>();

    DatabaseMockData.Initialize(passwordHasher);

    await SeedRolesAsync(context, cancellationToken);
    await SeedPermissionsAsync(context, cancellationToken);
    await SeedRolePermissionsAsync(context, cancellationToken);
    await SeedUsersAsync(context, cancellationToken);

    await SeedTagsAsync(context, cancellationToken);
    await SeedArticlesAndTagsAsync(context, cancellationToken);
    await SeedUserInteractionsAsync(context, cancellationToken);
    await SeedNewsLetterSubscriptionsAsync(context, cancellationToken);
  }

  private static async Task SeedRolesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
  {
    var roles = new[]
    {
      new Role { Id = ReaderRoleId, Name = "Reader", Description = "Can read, bookmark, follow, comment, and view reading history" },
      new Role { Id = AuthorRoleId, Name = "Author", Description = "Reader access plus owned article authoring and analytics" },
      new Role { Id = AdminRoleId, Name = "Admin", Description = "Full system access" }
    };

    foreach (var role in roles)
    {
      if (!await context.Roles.AnyAsync(existing => existing.Id == role.Id || existing.Name == role.Name, cancellationToken))
        await context.Roles.AddAsync(role, cancellationToken);
    }

    await context.SaveChangesAsync(cancellationToken);
  }

  private static async Task SeedPermissionsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
  {
    foreach (var permissionSeed in PermissionMock.ListPermissionsSeed)
    {
      var permission = await context.Permissions.FirstOrDefaultAsync(
        existing => existing.Id == permissionSeed.Id || existing.Code == permissionSeed.Code,
        cancellationToken
      );

      if (permission is not null)
      {
        permission.Code = permissionSeed.Code;
        permission.Name = permissionSeed.Name;
        permission.Description = permissionSeed.Description;
        continue;
      }

      await context.Permissions.AddAsync(new Permission
      {
        Id = permissionSeed.Id,
        Code = permissionSeed.Code,
        Name = permissionSeed.Name,
        Description = permissionSeed.Description
      }, cancellationToken);
    }

    await context.SaveChangesAsync(cancellationToken);
  }

  private static async Task SeedRolePermissionsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
  {
    var readerPermissionCodes = new HashSet<string>
      {
        PermissionConstant.Articles.Get,
        PermissionConstant.Bookmarks.Create,
        PermissionConstant.Bookmarks.Delete,
        PermissionConstant.Authors.Follow,
        PermissionConstant.Comments.Create,
        PermissionConstant.Authors.GetProfile,
        PermissionConstant.ReadingHistory.Get
      };

    var authorPermissionCodes = readerPermissionCodes.Concat([
      PermissionConstant.Articles.Create,
      PermissionConstant.Articles.UpdateOwn,
      PermissionConstant.Articles.Publish,
      PermissionConstant.Articles.Archive,
      PermissionConstant.Articles.DeleteOwn,
      PermissionConstant.Analytics.GetOwn
    ]).ToHashSet();

    await AssignPermissionsAsync(context, ReaderRoleId, readerPermissionCodes, cancellationToken);
    await AssignPermissionsAsync(context, AuthorRoleId, authorPermissionCodes, cancellationToken);
    await AssignPermissionsAsync(context, AdminRoleId, [.. PermissionConstant.All], cancellationToken);

    await context.SaveChangesAsync(cancellationToken);
  }

  private static async Task AssignPermissionsAsync(
      ApplicationDbContext context,
      Guid roleId,
      HashSet<string> permissionCodes,
      CancellationToken cancellationToken
)
  {
    var permissionIds = await context.Permissions
      .Where(permission => permissionCodes.Contains(permission.Code))
      .Select(permission => permission.Id)
      .ToListAsync(cancellationToken);

    foreach (var permissionId in permissionIds)
    {
      var exists = await context.RolePermissions
        .AnyAsync(rolePermission => rolePermission.RoleId == roleId && rolePermission.PermissionId == permissionId, cancellationToken);

      if (!exists)
        await context.RolePermissions.AddAsync(new RolePermission { RoleId = roleId, PermissionId = permissionId }, cancellationToken);
    }
  }

  private static async Task SeedUsersAsync(
    ApplicationDbContext context,
    CancellationToken cancellationToken
  )
  {
    // Ambil langsung dari data yang sudah di-initialize di DatabaseMockData
    foreach (var user in DatabaseMockData.GeneratedUsers)
    {
      if (!await context.Users.AnyAsync(u => u.Email == user.Email, cancellationToken))
        await context.Users.AddAsync(user, cancellationToken);
    }

    foreach (var userRole in DatabaseMockData.GeneratedUserRoles)
    {
      if (!await context.UserRoles.AnyAsync(ur => ur.UserId == userRole.UserId && ur.RoleId == userRole.RoleId, cancellationToken))
        await context.UserRoles.AddAsync(userRole, cancellationToken);
    }

    await context.SaveChangesAsync(cancellationToken);
  }

  private static async Task SeedTagsAsync(ApplicationDbContext context, CancellationToken token)
  {
    foreach (var tag in DatabaseMockData.GeneratedTags)
    {
      if (!await context.Tags.AnyAsync(t => t.Id == tag.Id || t.Slug == tag.Slug, token))
        await context.Tags.AddAsync(tag, token);
    }
    await context.SaveChangesAsync(token);
  }

  private static async Task SeedArticlesAndTagsAsync(ApplicationDbContext context, CancellationToken token)
  {
    foreach (var article in DatabaseMockData.GeneratedArticles)
    {
      if (!await context.Articles.AnyAsync(a => a.Id == article.Id || a.Slug == article.Slug, token))
        await context.Articles.AddAsync(article, token);
    }
    await context.SaveChangesAsync(token);

    foreach (var artTag in DatabaseMockData.GeneratedArticleTags)
    {
      if (!await context.ArticleTags.AnyAsync(at => at.ArticleId == artTag.ArticleId && at.TagId == artTag.TagId, token))
        await context.ArticleTags.AddAsync(artTag, token);
    }
    await context.SaveChangesAsync(token);
  }

  private static async Task SeedUserInteractionsAsync(ApplicationDbContext context, CancellationToken token)
  {
    foreach (var follow in DatabaseMockData.GeneratedFollows)
    {
      if (!await context.Follows.AnyAsync(f => f.FollowerId == follow.FollowerId && f.FollowingId == follow.FollowingId, token))
        await context.Follows.AddAsync(follow, token);
    }

    foreach (var bmark in DatabaseMockData.GeneratedBookmarks)
    {
      if (!await context.Bookmarks.AnyAsync(b => b.UserId == bmark.UserId && b.ArticleId == bmark.ArticleId, token))
        await context.Bookmarks.AddAsync(bmark, token);
    }

    foreach (var comment in DatabaseMockData.GeneratedComments)
    {
      if (!await context.Comments.AnyAsync(c => c.Id == comment.Id, token))
        await context.Comments.AddAsync(comment, token);
    }

    foreach (var history in DatabaseMockData.GeneratedReadingHistories)
    {
      if (!await context.ReadingHistories.AnyAsync(rh => rh.Id == history.Id, token))
        await context.ReadingHistories.AddAsync(history, token);
    }

    await context.SaveChangesAsync(token);
  }

  private static async Task SeedNewsLetterSubscriptionsAsync(ApplicationDbContext context, CancellationToken token)
  {
    foreach (var sub in DatabaseMockData.GeneratedSubscriptions)
    {
      if (!await context.NewsLetterSubscriptions.AnyAsync(s => s.Email == sub.Email, token))
        await context.NewsLetterSubscriptions.AddAsync(sub, token);
    }
    await context.SaveChangesAsync(token);
  }

}