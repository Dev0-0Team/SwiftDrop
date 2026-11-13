using SwiftDrop.Models.Enums;

namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: Admin (PK/FK: UserId).</summary>
public class Admin
{
    public int UserId { get; set; }
    public ManagementLevel ManagementLevel { get; set; } = ManagementLevel.SuperAdmin;

    public ApplicationUser User { get; set; } = null!;

    public ICollection<Product> UploadedProducts { get; set; } = new List<Product>();
    public ICollection<DeliveryAssignment> AssignedDeliveries { get; set; } = new List<DeliveryAssignment>();
}
