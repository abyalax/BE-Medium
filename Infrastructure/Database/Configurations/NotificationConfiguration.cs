using Medium.Api.Enums;
using Medium.Api.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medium.Api.Infrastructure.Database.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
  public void Configure(EntityTypeBuilder<Notification> builder)
  {
    builder.ToTable("notifications");

    builder.HasIndex(x => x.UserId);

    builder.HasIndex(x => new
    {
      x.UserId,
      x.IsRead,
      x.CreatedAt
    });

    builder.Property(x => x.Type)
      .HasConversion(
        n => n.ToString(),
        n => (NotificationType)Enum.Parse(typeof(NotificationType), n)
      );

    builder.Property(x => x.Title)
      .HasMaxLength(200)
      .IsRequired();

    builder.Property(x => x.Message)
      .HasMaxLength(1000)
      .IsRequired();

    builder.Property(x => x.ReferenceType)
      .HasMaxLength(100);

    builder.Property(x => x.IsRead)
      .HasDefaultValue(false);

    builder.HasOne(x => x.User)
      .WithMany()
      .HasForeignKey(x => x.UserId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}