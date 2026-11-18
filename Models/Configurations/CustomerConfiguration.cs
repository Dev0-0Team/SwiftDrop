using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        // PK is also the FK to Users (shared primary key)
        builder.HasKey(c => c.UserId);
        builder.Property(c => c.UserId).ValueGeneratedNever();

        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.PhoneNumber).HasMaxLength(20);

        // Customer 1 --- 1 User
        builder.HasOne(c => c.User)
            .WithOne(u => u.Customer)
            .HasForeignKey<Customer>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Customer 1 --- 1 Cart
        builder.HasOne(c => c.Cart)
            .WithOne(cart => cart.Customer)
            .HasForeignKey<Customer>(c => c.CartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.CartId).IsUnique();
    }
}
