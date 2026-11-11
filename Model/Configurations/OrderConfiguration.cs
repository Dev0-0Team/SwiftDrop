using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        // Enum stored as readable text
        builder.Property(o => o.Status)
            .IsRequired()
            .HasColumnType("tinyint"); 

        builder.Property(o => o.TotalPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(o => o.CreatedAt).IsRequired().HasPrecision(0);

        builder.Property(o => o.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(o => o.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        // Order M --- 1 Customer
        builder.HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order M --- 0..1 Delivery (agent assigned later)
        builder.HasOne(o => o.Delivery)
            .WithMany(d => d.Orders)
            .HasForeignKey(o => o.DeliveryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.DeliveryId);
   
    }
}
