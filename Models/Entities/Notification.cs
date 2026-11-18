using SwiftDrop.Models.Enums;

namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: Notification.</summary>
public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty; 
    public string MessageBody { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public NotificationType Type { get; set; } = NotificationType.General;

    public ApplicationUser User { get; set; } = null!;
}
