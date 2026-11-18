using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class ProductSecondaryImageConfiguration : IEntityTypeConfiguration<ProductSecondaryImage>
{
    public void Configure(EntityTypeBuilder<ProductSecondaryImage> builder)
    {
        builder.ToTable("ProductSecondaryImages");

        builder.HasKey(psi => new { psi.ProductId, psi.SecondaryImageId });

        // Product M --- M SecondaryImage
        builder.HasOne(psi => psi.Product)
            .WithMany(p => p.ProductSecondaryImages)
            .HasForeignKey(psi => psi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(psi => psi.SecondaryImage)
            .WithMany(i => i.ProductSecondaryImages)
            .HasForeignKey(psi => psi.SecondaryImageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(psi => psi.SecondaryImageId);
    }
}
