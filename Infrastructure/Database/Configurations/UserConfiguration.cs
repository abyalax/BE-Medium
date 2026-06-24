using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medium.Api.Infrastructure.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.ToTable("users");

    builder.Property(u => u.Name)
        .IsRequired()
        .HasMaxLength(255);

    builder.Property(u => u.Email)
        .IsRequired()
        .HasMaxLength(255);

    builder.HasIndex(u => u.Email).IsUnique();

    builder.Property(u => u.Password)
        .IsRequired()
        .HasMaxLength(255);

    builder.Property(u => u.Bio)
        .HasMaxLength(1000);

    builder.Property(u => u.AvatarUrl)
        .HasMaxLength(500);

    builder.HasMany(u => u.UserRoles)
        .WithOne()
        .HasForeignKey(ur => ur.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(u => u.Articles)
        .WithOne(a => a.Author)
        .HasForeignKey(a => a.AuthorId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasMany(u => u.Comments)
        .WithOne(c => c.User)
        .HasForeignKey(c => c.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(u => u.Bookmarks)
        .WithOne(b => b.User)
        .HasForeignKey(b => b.UserId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(u => u.Followers)
        .WithOne(f => f.Following)
        .HasForeignKey(f => f.FollowingId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasMany(u => u.Following)
        .WithOne(f => f.Follower)
        .HasForeignKey(f => f.FollowerId)
        .OnDelete(DeleteBehavior.Restrict);

    builder.HasMany(u => u.ReadingHistories)
        .WithOne(rh => rh.User)
        .HasForeignKey(rh => rh.UserId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}