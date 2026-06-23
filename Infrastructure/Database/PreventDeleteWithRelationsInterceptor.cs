using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Medium.Api.Infrastructure.Database;

public class PreventDeleteWithRelationsInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> SoftDeleteEntityNames =
    [
        nameof(Models.User),
        nameof(Models.Article),
        nameof(Models.Comment),
        nameof(Models.Tag)
    ];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext == null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var deletedEntries = dbContext.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in deletedEntries)
        {
            var entity = entry.Entity;
            var entityType = entry.Entity.GetType();

            // Check if the entity has any active relations
            var hasActiveRelations = CheckForActiveRelations(dbContext, entity, entityType);

            if (hasActiveRelations)
            {
                var deleteMode = SoftDeleteEntityNames.Contains(entityType.Name)
                    ? "soft delete"
                    : "hard delete";

                throw new InvalidOperationException(
                    $"Cannot delete {entityType.Name} because it has active relations. " +
                    $"Use {deleteMode} according to the entity's delete policy.");
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private bool CheckForActiveRelations(DbContext dbContext, object entity, Type entityType)
    {
        // Check for specific entity types and their relations
        var entityName = entityType.Name;

        switch (entityName)
        {
            case "User":
                var userId = (Guid)entityType.GetProperty("Id")?.GetValue(entity)!;
                return dbContext.Set<Models.Article>().Any(a => a.AuthorId == userId) ||
                       dbContext.Set<Models.Comment>().Any(c => c.UserId == userId) ||
                       dbContext.Set<Models.Bookmark>().Any(b => b.UserId == userId) ||
                       dbContext.Set<Models.Follow>().Any(f => f.FollowerId == userId || f.FollowingId == userId) ||
                       dbContext.Set<Models.ReadingHistory>().Any(r => r.UserId == userId);

            case "Article":
                var articleId = (Guid)entityType.GetProperty("Id")?.GetValue(entity)!;
                return dbContext.Set<Models.Comment>().Any(c => c.ArticleId == articleId) ||
                       dbContext.Set<Models.Bookmark>().Any(b => b.ArticleId == articleId) ||
                       dbContext.Set<Models.ReadingHistory>().Any(r => r.ArticleId == articleId) ||
                       dbContext.Set<Models.ArticleTag>().Any(at => at.ArticleId == articleId);

            case "Tag":
                var tagId = (Guid)entityType.GetProperty("Id")?.GetValue(entity)!;
                return dbContext.Set<Models.ArticleTag>().Any(at => at.TagId == tagId);

            case "Role":
                var roleId = (Guid)entityType.GetProperty("Id")?.GetValue(entity)!;
                return dbContext.Set<Models.UserRole>().Any(ur => ur.RoleId == roleId) ||
                       dbContext.Set<Models.RolePermission>().Any(rp => rp.RoleId == roleId);

            case "Permission":
                var permissionId = (Guid)entityType.GetProperty("Id")?.GetValue(entity)!;
                return dbContext.Set<Models.RolePermission>().Any(rp => rp.PermissionId == permissionId);

            default:
                return false;
        }
    }
}
