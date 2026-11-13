namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: SecondryImage.</summary>
public class SecondaryImage
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    public ICollection<ProductSecondaryImage> ProductSecondaryImages { get; set; } = new List<ProductSecondaryImage>();
}
