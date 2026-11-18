using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.MessageBody)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.IsRead).IsRequired();

        builder.Property(n => n.CreatedAt)
    .IsRequired()
    .HasColumnType("smalldatetime");

        builder.Property(n => n.Type)
            .IsRequired()
            .HasColumnType("tinyint");

        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.UserId, n.CreatedAt })
          .IsDescending(false, true);
    }
}
