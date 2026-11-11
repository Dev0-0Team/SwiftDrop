using Microsoft.AspNetCore.Identity;

namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: User (Id, Email, PasswordHash come from IdentityUser).</summary>
public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Role-specific profiles (one-to-one, only one of them is normally filled)
    public Customer? Customer { get; set; }
    public Delivery? Delivery { get; set; }
    public Admin? Admin { get; set; }

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<OrderStatusHistory> OrderStatusChanges { get; set; } = new List<OrderStatusHistory>();
}
