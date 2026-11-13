using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SettingName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Value)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.HasIndex(s => s.SettingName).IsUnique();
    }
}
