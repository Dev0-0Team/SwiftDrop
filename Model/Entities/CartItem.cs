namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: CartItem (PK: CartId + ProductId).</summary>
public class CartItem
{
    public int CartId { get; set; }
    public int ProductId { get; set; }

    public Cart Cart { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
