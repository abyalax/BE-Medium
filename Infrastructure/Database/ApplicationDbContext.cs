using System.Linq.Expressions;

using Medium.Api.Infrastructure.Database.Configurations;
using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace Medium.Api.Infrastructure.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
  private static readonly HashSet<Type> SoftDeleteEntityTypes =
  [
      typeof(User),
        typeof(Article),
        typeof(Follow),
        typeof(Notification),
        typeof(Comment),
        typeof(Tag)
  ];

  // Core Entities
  public DbSet<User> Users => Set<User>();
  public DbSet<Article> Articles => Set<Article>();
  public DbSet<Tag> Tags => Set<Tag>();
  public DbSet<Comment> Comments => Set<Comment>();
  public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
  public DbSet<Follow> Follows => Set<Follow>();
  public DbSet<ReadingHistory> ReadingHistories => Set<ReadingHistory>();
  public DbSet<NewsLetterSubscription> NewsLetterSubscriptions => Set<NewsLetterSubscription>();
  public DbSet<Notification> Notification => Set<Notification>();

  // RBAC Entities
  public DbSet<Role> Roles => Set<Role>();
  public DbSet<Permission> Permissions => Set<Permission>();
  public DbSet<UserRole> UserRoles => Set<UserRole>();
  public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

  // Junction Tables
  public DbSet<ArticleTag> ArticleTags => Set<ArticleTag>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Apply all entity configurations
    modelBuilder.ApplyConfiguration(new UserConfiguration());
    modelBuilder.ApplyConfiguration(new ArticleConfiguration());
    modelBuilder.ApplyConfiguration(new TagConfiguration());
    modelBuilder.ApplyConfiguration(new CommentConfiguration());
    modelBuilder.ApplyConfiguration(new BookmarkConfiguration());
    modelBuilder.ApplyConfiguration(new FollowConfiguration());
    modelBuilder.ApplyConfiguration(new ReadingHistoryConfiguration());
    modelBuilder.ApplyConfiguration(new NewsLetterSubscriptionConfiguration());
    modelBuilder.ApplyConfiguration(new RoleConfiguration());
    modelBuilder.ApplyConfiguration(new PermissionConfiguration());
    modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
    modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
    modelBuilder.ApplyConfiguration(new ArticleTagConfiguration());
    modelBuilder.ApplyConfiguration(new NotificationConfiguration());

    // Configure snake_case naming convention
    ConfigureSnakeCaseNaming(modelBuilder);

    // Apply soft delete global query filter only to soft-deletable entities
    ApplySoftDeleteQueryFilter(modelBuilder);
  }

  private static void ConfigureSnakeCaseNaming(ModelBuilder modelBuilder)
  {
    foreach (var entity in modelBuilder.Model.GetEntityTypes())
    {
      foreach (var property in entity.GetProperties())
      {
        property.SetColumnName(ToSnakeCase(property.Name));
      }

      foreach (var key in entity.GetKeys())
      {
        key.SetName(ToSnakeCase(key.GetName()!));
      }

      foreach (var foreignKey in entity.GetForeignKeys())
      {
        foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName()!));
      }

      foreach (var index in entity.GetIndexes())
      {
        index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
      }
    }
  }

  private static void ApplySoftDeleteQueryFilter(ModelBuilder modelBuilder)
  {
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
      // 1. Direct soft delete filter for registered entities
      if (typeof(Entity).IsAssignableFrom(entityType.ClrType) &&
          SoftDeleteEntityTypes.Contains(entityType.ClrType))
      {
        var parameter = Expression.Parameter(entityType.ClrType, "e");
        var property = Expression.Property(parameter, nameof(Entity.DeletedAt));
        var condition = Expression.Equal(property, Expression.Constant(null, typeof(DateTime?)));
        var lambda = Expression.Lambda(condition, parameter);

        modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        continue;
      }

      // 2. Automated filter for child/dependent entities to resolve EF Core model validation warnings
      // Check if this entity has a required foreign key pointing to a soft-deletable parent entity
      var requiredForeignKeysWithSoftDelete = entityType.GetForeignKeys()
          .Where(fk => fk.IsRequired && SoftDeleteEntityTypes.Contains(fk.PrincipalEntityType.ClrType));

      if (requiredForeignKeysWithSoftDelete.Any())
      {
        var parameter = Expression.Parameter(entityType.ClrType, "child");
        Expression? finalExpression = null;

        foreach (var fk in requiredForeignKeysWithSoftDelete)
        {
          // Fetch the navigation property name to the parent (e.g., Bookmark.Article)
          var navigationName = fk.DependentToPrincipal?.Name;
          if (string.IsNullOrEmpty(navigationName)) continue;

          // Build the expression: child.Parent.DeletedAt == null
          var navigationProperty = Expression.Property(parameter, navigationName);
          var parentDeletedAtProperty = Expression.Property(navigationProperty, nameof(Entity.DeletedAt));
          var isNotDeletedExpression = Expression.Equal(parentDeletedAtProperty, Expression.Constant(null, typeof(DateTime?)));

          // Combine multiple parent conditions with AND if necessary
          finalExpression = finalExpression == null
              ? isNotDeletedExpression
              : Expression.AndAlso(finalExpression, isNotDeletedExpression);
        }

        if (finalExpression != null)
        {
          var lambda = Expression.Lambda(finalExpression, parameter);
          modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
        }
      }
    }
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    var entries = ChangeTracker.Entries<Entity>();

    foreach (var entry in entries)
    {
      if (entry.State == EntityState.Added)
      {
        entry.Entity.CreatedAt = DateTime.UtcNow;
        entry.Entity.UpdatedAt = DateTime.UtcNow;
      }
      else if (entry.State == EntityState.Modified)
      {
        entry.Entity.UpdatedAt = DateTime.UtcNow;
      }
    }

    return await base.SaveChangesAsync(cancellationToken);
  }

  private static string ToSnakeCase(string input)
  {
    if (string.IsNullOrEmpty(input))
      return input;

    return string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
  }
}