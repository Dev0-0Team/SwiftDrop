namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: DeliveryReview.</summary>
public class DeliveryReview
{
    public int Id { get; set; }
    public int DeliveryId { get; set; }          // FK -> Delivery
    public int CustomerId { get; set; }          // FK -> Customer
    public int OrderId { get; set; }             // FK -> Order
    public byte Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Delivery Delivery { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public Order Order { get; set; } = null!;
}
