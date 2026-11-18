namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: OrderItem (PK: OrderId + ProductId).</summary>
public class OrderItem
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
