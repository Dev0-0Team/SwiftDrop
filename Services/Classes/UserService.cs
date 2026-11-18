using Microsoft.AspNetCore.Identity;
using SwiftDrop.Helpers;
using SwiftDrop.Models.Entities;
using SwiftDrop.Models.Enums;
using SwiftDrop.Repositories.Interfaces;
using SwiftDrop.Services.Interfaces;

namespace SwiftDrop.Services.Classes
{
    public class UserService : IUserService 
    {

        private readonly IUserRepository _userRepository;
        private UserManager<ApplicationUser> _userManager;
        public UserService(IUserRepository userRepository, UserManager<ApplicationUser> userManager, ICacheService cacheService)
        {

            _userRepository = userRepository;
            _userManager = userManager;


        }




        public async Task<ApplicationUser?> FindByEmailAsync(string email, bool asTracking)
        {
            return await _userRepository.FindByEmailAsync(email, asTracking);
        }

        public async Task<ApplicationUser?> FindByUsernameAsync(string username, bool asTracking)
        {
            return await _userRepository.FindByUsernameAsync(username, asTracking);
        }

        public async Task<ApplicationUser?> GetByIdAsync(int userId, bool asTracking)
        {

            return await _userRepository.GetByIdAsync(userId, asTracking);


        }
        public async Task<(bool EmailExists, bool UserNameExists)> CheckUserExistsAsync(string email, string userName)
        {
            return await _userRepository.CheckIfEmailOrUsernameExistsAsync(email, userName);
        }
        public async Task<bool> IsEmailExistAsync(string email, int excludeId = -1)
        {
            return await _userRepository.IsEmailExistAsync(email, excludeId);
        }
        public async Task<bool> IsUserNameExistAsync(string userName, int excludeId = -1)
        {
            return await _userRepository.IsUserNameExistAsync(userName, excludeId);
        }
  
       
        public async Task SetLockoutNull(ApplicationUser user)
        {
            await _userRepository.SetLockoutNull(user);
        }

        public Task<bool> IsLockedOutAsync(ApplicationUser user)
        {
            return _userManager.IsLockedOutAsync(user);
        }


    }

}
