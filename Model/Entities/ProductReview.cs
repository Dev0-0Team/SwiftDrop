namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: ProductReview (PK: ProductId + CustomerId).</summary>
public class ProductReview
{
    public int ProductId { get; set; }
    public int CustomerId { get; set; }
    public byte Rating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Comment { get; set; }

    public Product Product { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
}
