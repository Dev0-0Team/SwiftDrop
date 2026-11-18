namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: ProductSecondryImage (PK: ProductId + SecondaryImageId).</summary>
public class ProductSecondaryImage
{
    public int ProductId { get; set; }
    public int SecondaryImageId { get; set; }

    public Product Product { get; set; } = null!;
    public SecondaryImage SecondaryImage { get; set; } = null!;
}
