using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");

        builder.Ignore(u => u.PhoneNumber);
        builder.Ignore(u => u.PhoneNumberConfirmed);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(500);

        builder.Property(u => u.IsActive)
            .IsRequired();


    }
}
