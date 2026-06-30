using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medium.Api.Infrastructure.Database.Configurations;

public class ReadingHistoryConfiguration : IEntityTypeConfiguration<ReadingHistory>
{
  public void Configure(EntityTypeBuilder<ReadingHistory> builder)
  {
    builder.ToTable("reading_histories");

    builder.Property(rh => rh.DurationSeconds)
      .IsRequired();

    builder.Property(rh => rh.ReadAt)
      .IsRequired()
      .HasDefaultValueSql("GETUTCDATE()");

    builder.HasOne(rh => rh.User)
      .WithMany(u => u.ReadingHistories)
      .HasForeignKey(rh => rh.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(rh => rh.Article)
      .WithMany(a => a.ReadingHistories)
      .HasForeignKey(rh => rh.ArticleId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}