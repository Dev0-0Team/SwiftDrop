using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class DeliveryReviewConfiguration : IEntityTypeConfiguration<DeliveryReview>
{
    public void Configure(EntityTypeBuilder<DeliveryReview> builder)
    {
        builder.ToTable("DeliveryReviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating).IsRequired().HasColumnType("tinyint");
        builder.Property(r => r.CreatedAt).HasColumnType("smalldatetime").IsRequired();

        // One review per order
        builder.HasOne(r => r.Order)
            .WithOne(o => o.DeliveryReview)
            .HasForeignKey<DeliveryReview>(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Delivery)
            .WithMany(d => d.Reviews)
            .HasForeignKey(r => r.DeliveryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Customer)
            .WithMany(c => c.DeliveryReviews)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.OrderId).IsUnique();
        builder.HasIndex(r => r.DeliveryId);
        builder.HasIndex(r => r.CustomerId);
    }
}
