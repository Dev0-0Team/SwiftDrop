namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: Product.</summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? MainImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public int UploadedBy { get; set; }          // FK -> Admin
    public decimal Rating { get; set; }
    public bool IsActive { get; set; } = true;

    public Admin Uploader { get; set; } = null!;
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
    public ICollection<ProductSecondaryImage> ProductSecondaryImages { get; set; } = new List<ProductSecondaryImage>();
    public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
