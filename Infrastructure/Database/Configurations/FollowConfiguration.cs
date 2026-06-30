using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medium.Api.Infrastructure.Database.Configurations;

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
  public void Configure(EntityTypeBuilder<Follow> builder)
  {
    builder.ToTable("follows");

    builder.HasIndex(f => new { f.FollowerId, f.FollowingId }).IsUnique();

    builder.HasOne(f => f.Follower)
      .WithMany(u => u.Following)
      .HasForeignKey(f => f.FollowerId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(f => f.Following)
      .WithMany(u => u.Followers)
      .HasForeignKey(f => f.FollowingId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}