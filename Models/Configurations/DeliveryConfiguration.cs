using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class DeliveryConfiguration : IEntityTypeConfiguration<Delivery>
{
    public void Configure(EntityTypeBuilder<Delivery> builder)
    {
        builder.ToTable("Deliveries");

        builder.HasKey(d => d.UserId);
        builder.Property(d => d.UserId).ValueGeneratedNever();

        builder.Property(d => d.Rating).HasPrecision(3, 2);

        // Delivery 1 --- 1 User
        builder.HasOne(d => d.User)
            .WithOne(u => u.Delivery)
            .HasForeignKey<Delivery>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
