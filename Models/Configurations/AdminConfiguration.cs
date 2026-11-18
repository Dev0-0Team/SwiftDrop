using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;


public class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("Admins");

        builder.HasKey(a => a.UserId);
        builder.Property(a => a.UserId).ValueGeneratedNever();

        builder.Property(a => a.ManagementLevel)
        .IsRequired()
        .HasColumnType("tinyint");

        // Admin 1 --- 1 User
        builder.HasOne(a => a.User)
            .WithOne(u => u.Admin)
            .HasForeignKey<Admin>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
