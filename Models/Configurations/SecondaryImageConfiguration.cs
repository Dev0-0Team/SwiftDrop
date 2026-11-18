using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class SecondaryImageConfiguration : IEntityTypeConfiguration<SecondaryImage>
{
    public void Configure(EntityTypeBuilder<SecondaryImage> builder)
    {
        builder.ToTable("SecondaryImages");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);
    }
}
