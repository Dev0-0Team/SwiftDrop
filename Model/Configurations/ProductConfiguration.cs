using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasPrecision(9, 2);

        builder.Property(p => p.Description)
            .HasMaxLength(4000);

        builder.Property(p => p.MainImageUrl)
            .HasMaxLength(500);

        builder.Property(p => p.StockQuantity).IsRequired();

        builder.Property(p => p.Rating).HasPrecision(3, 2);

        builder.Property(p => p.IsActive).IsRequired();

        // Product M --- 1 Admin (uploader)
        builder.HasOne(p => p.Uploader)
            .WithMany(u => u.UploadedProducts)
            .HasForeignKey(p => p.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.UploadedBy);
    }
}
