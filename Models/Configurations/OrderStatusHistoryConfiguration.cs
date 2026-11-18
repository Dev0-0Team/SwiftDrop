using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("OrderStatusHistories");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.OldStatus)
             .HasColumnType("tinyint");


        builder.Property(h => h.NewStatus)
            .IsRequired()
            .HasColumnType("tinyint");



        builder.Property(h => h.ChangedAt).HasPrecision(0).IsRequired();

        builder.HasOne(h => h.Order)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.ChangedByUser)
            .WithMany(u => u.OrderStatusChanges)
            .HasForeignKey(h => h.ChangedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(h => h.OrderId);
        builder.HasIndex(h => h.ChangedBy);
    }
}
