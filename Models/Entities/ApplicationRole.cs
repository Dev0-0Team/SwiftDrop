using Microsoft.AspNetCore.Identity;

namespace SwiftDrop.Models.Entities;

/// <summary>ERD table: Role (Id, Name come from IdentityRole).</summary>
public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();
}
