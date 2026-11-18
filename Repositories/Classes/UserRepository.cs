using Microsoft.EntityFrameworkCore;
using SwiftDrop.Helpers;
using SwiftDrop.Models.Entities;
using SwiftDrop.Repositories.Interfaces;

namespace SwiftDrop.Repositories.Classes
{
    public class UserRepository : BaseRepository<ApplicationUser>, IUserRepository
    {

       
        
            public UserRepository(ApplicationDbContext context) : base(context)
            {
            }


            public async Task<ApplicationUser?> FindUserByEmailOrUserNameAsync(string emailOrUserName, bool asTracking = true)
            {
                if (string.IsNullOrWhiteSpace(emailOrUserName))
                    return null;

                try
                {

                    string normalizedInput = emailOrUserName.ToUpperInvariant();

                    var query = _context.Users.AsQueryable();

                    if (!asTracking)
                    {
                        query = query.AsNoTracking();
                    }

                    return await query.FirstOrDefaultAsync(u =>
                        (u.NormalizedEmail == normalizedInput ||
                        u.NormalizedUserName == normalizedInput) && u.IsActive);
                }
                catch
                {
                    return null;
                }
            }
            public async Task<ApplicationUser?> FindByEmailAsync(string email, bool asTracking = true)
            {
                if (string.IsNullOrWhiteSpace(email))
                    return null;

                try
                {

                    string normalizedEmail = email.ToUpperInvariant();

                    var query = _context.Users.AsQueryable();

                    if (!asTracking)
                    {
                        query = query.AsNoTracking();
                    }

                    return await query.FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail && u.IsActive);
                }
                catch
                {
                    return null;
                }
            }
            public async Task<ApplicationUser?> FindByUsernameAsync(string username, bool asTracking = true)
            {
                if (string.IsNullOrWhiteSpace(username))
                    return null;

                try
                {
                    string normalizedUsername = username.ToUpperInvariant();

                    var query = _context.Users.AsQueryable();

                    if (!asTracking)
                    {
                        query = query.AsNoTracking();
                    }

                    return await query.FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUsername && u.IsActive);
                }
                catch
                {
                    return null;
                }
            }
            public async override Task<ApplicationUser?> GetByIdAsync(int Id, bool asTracking = true)
            {
                try
                {
                    if (asTracking)
                        return await _context.Users.FirstOrDefaultAsync(u => u.Id == Id && u.IsActive);

                    return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Id && u.IsActive);


                }
                catch
                {
                    return null;
                }
            }
            public async Task<ApplicationUser?> GetByIdAndRolesAsync(int Id, bool asTracking = true)
            {
                try
                {
                    if (asTracking)
                        return await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == Id && u.IsActive);

                    return await _context.Users.AsNoTracking().Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == Id && u.IsActive);


                }
                catch
                {
                    return null;
                }
            }

            public async Task<bool> IsEmailExistAsync(string email, int excludeId = -1)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                string normalizedEmail = email.ToUpperInvariant();
                return await _context.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail && u.Id != excludeId);
            }
            catch
            {
                return false;
            }
        }
            public async Task<bool> IsUserNameExistAsync(string userName, int excludeId = -1)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return false;

            try
            {
                string normalizedUserName = userName.ToUpperInvariant();
                return await _context.Users.AnyAsync(u => u.NormalizedUserName == normalizedUserName && u.Id != excludeId);
            }
            catch
            {
                return false;
            }
        }

             public async Task<(bool EmailExists, bool UserNameExists)> CheckIfEmailOrUsernameExistsAsync(string email, string userName) {

                email = email.ToUpperInvariant();
                userName = userName.ToUpperInvariant();


                   var matchedUsers = await _context.Users
                   .Where(u => u.NormalizedEmail == email || u.NormalizedUserName == userName)
                   .Select(u => new { u.NormalizedEmail, u.NormalizedUserName }).Take(2)
                   .ToListAsync();

                   bool emailExists = matchedUsers.Any(u => u.NormalizedEmail == email);
                   bool userNameExists = matchedUsers.Any(u => u.NormalizedUserName == userName);

                 return (emailExists, userNameExists);

        }
          
   
    
            public async Task SetLockoutNull(ApplicationUser user)
            {

                user.AccessFailedCount = 0;
                user.LockoutEnd = null;
                await _context.SaveChangesAsync();
            }
        }
    
}
