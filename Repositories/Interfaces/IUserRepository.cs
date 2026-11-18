using SwiftDrop.Helpers;
using SwiftDrop.Models.Entities;
using SwiftDrop.Repositories.Classes;

namespace SwiftDrop.Repositories.Interfaces
{
    public interface IUserRepository : IBaseRepository<ApplicationUser>
    {
      
        Task<ApplicationUser?> FindUserByEmailOrUserNameAsync(string emailOrUserName, bool asTracking = true);
        Task<ApplicationUser?> FindByEmailAsync(string email, bool asTracking = true);
        Task<ApplicationUser?> FindByUsernameAsync(string username, bool asTracking = true);

        Task<bool> IsEmailExistAsync(string email, int excludeId = -1);
        Task<bool> IsUserNameExistAsync(string userName, int excludeId = -1);
        Task<(bool EmailExists, bool UserNameExists)> CheckIfEmailOrUsernameExistsAsync(string email, string userName);
        Task SetLockoutNull(ApplicationUser user);

    }
}
