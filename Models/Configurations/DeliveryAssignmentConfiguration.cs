using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class DeliveryAssignmentConfiguration : IEntityTypeConfiguration<DeliveryAssignment>
{
    public void Configure(EntityTypeBuilder<DeliveryAssignment> builder)
    {
        builder.ToTable("DeliveryAssignments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AssignedAt).HasPrecision(0).IsRequired();
        builder.Property(a => a.IsChangedOrDenied).IsRequired();

        builder.HasOne(a => a.Order)
            .WithMany(o => o.DeliveryAssignments)
            .HasForeignKey(a => a.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.DeliveryAgent)
            .WithMany(d => d.Assignments)
            .HasForeignKey(a => a.DeliveryAgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AssignedByAdmin)
            .WithMany(ad => ad.AssignedDeliveries)
            .HasForeignKey(a => a.AssignedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.OrderId);
        builder.HasIndex(a => a.DeliveryAgentId);
        builder.HasIndex(a => a.AssignedByAdminId);
    }
}
