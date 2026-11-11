namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: Cart.</summary>
public class Cart
{
    public int Id { get; set; }

    public Customer? Customer { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
