using SwiftDrop.Models.Enums;

namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: Oder (Order).</summary>
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }          // FK -> Customer
    public int? DeliveryId { get; set; }         // FK -> Delivery (null until an agent is assigned)
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public Customer Customer { get; set; } = null!;
    public Delivery? Delivery { get; set; }
    public Conversation? Conversation { get; set; }
    public DeliveryReview? DeliveryReview { get; set; }
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    public ICollection<DeliveryAssignment> DeliveryAssignments { get; set; } = new List<DeliveryAssignment>();
}
