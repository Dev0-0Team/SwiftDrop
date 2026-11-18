using Microsoft.AspNetCore.Identity;

namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: UserRole (PK: UserId + RoleId).</summary>
public class ApplicationUserRole : IdentityUserRole<int>
{
    public ApplicationUser User { get; set; } = null!;
    public ApplicationRole Role { get; set; } = null!;
}
