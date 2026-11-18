using SwiftDrop.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SwiftDrop.Models.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CreatedAt).HasColumnType("smalldatetime").IsRequired();

        // One conversation per order
        builder.HasOne(c => c.Order)
            .WithOne(o => o.Conversation)
            .HasForeignKey<Conversation>(c => c.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.OrderId).IsUnique();
    }
}
