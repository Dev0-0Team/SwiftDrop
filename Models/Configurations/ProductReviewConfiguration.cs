using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("ProductReviews");

        // One review per customer per product
        builder.HasKey(r => new { r.ProductId, r.CustomerId });

        builder.Property(r => r.Rating)
      .IsRequired()
      .HasColumnType("tinyint");

        builder.Property(r => r.CreatedAt).HasColumnType("smalldatetime").IsRequired();
        builder.Property(r => r.Comment).HasMaxLength(2000);



        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Customer)
            .WithMany(c => c.ProductReviews)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.CustomerId);
    }
}
