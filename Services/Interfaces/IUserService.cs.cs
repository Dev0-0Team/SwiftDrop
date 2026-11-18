using SwiftDrop.Helpers;
using SwiftDrop.Models.Entities;
using SwiftDrop.Models.Enums;

namespace SwiftDrop.Services.Interfaces
{
    public interface IUserService
    {
     
        Task<ApplicationUser?> FindByEmailAsync(string email, bool asTracking);
        Task<ApplicationUser?> FindByUsernameAsync(string username, bool asTracking);
        Task<ApplicationUser?> GetByIdAsync(int userId, bool asTracking);
        Task<(bool EmailExists, bool UserNameExists)> CheckUserExistsAsync(string email, string userName);
        Task<bool> IsEmailExistAsync(string email, int excludeId = -1);
        Task<bool> IsUserNameExistAsync(string userName, int excludeId = -1);
        Task SetLockoutNull(ApplicationUser user);

        Task<bool> IsLockedOutAsync(ApplicationUser user);

    }
}
