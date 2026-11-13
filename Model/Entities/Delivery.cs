namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: Delivery (PK/FK: UserId). Represents a delivery agent.</summary>
public class Delivery
{
    public int UserId { get; set; }
    public decimal Rating { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<DeliveryAssignment> Assignments { get; set; } = new List<DeliveryAssignment>();
    public ICollection<DeliveryReview> Reviews { get; set; } = new List<DeliveryReview>();
}
