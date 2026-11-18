using SwiftDrop.Models.Enums;

namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: OrderStatusHistory.</summary>
public class OrderStatusHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public OrderStatus? OldStatus { get; set; }  // null for the very first status
    public OrderStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public int ChangedBy { get; set; }           // FK -> User

    public Order Order { get; set; } = null!;
    public ApplicationUser ChangedByUser { get; set; } = null!;
}
