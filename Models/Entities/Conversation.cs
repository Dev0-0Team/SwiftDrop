namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: Conversation.</summary>
public class Conversation
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
