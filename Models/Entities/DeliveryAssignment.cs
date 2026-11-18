namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: DeliveryAssignment.</summary>
public class DeliveryAssignment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int DeliveryAgentId { get; set; }     // FK -> Delivery
    public int AssignedByAdminId { get; set; }   // FK -> Admin
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsChangedOrDenied { get; set; }

    public Order Order { get; set; } = null!;
    public Delivery DeliveryAgent { get; set; } = null!;
    public Admin AssignedByAdmin { get; set; } = null!;
}
