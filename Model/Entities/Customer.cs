namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: Customer (PK/FK: UserId).</summary>
public class Customer
{
    public int UserId { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public int CartId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public Cart Cart { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();
    public ICollection<DeliveryReview> DeliveryReviews { get; set; } = new List<DeliveryReview>();
}
