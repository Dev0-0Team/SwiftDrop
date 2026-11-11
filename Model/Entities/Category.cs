namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: Category.</summary>
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}
