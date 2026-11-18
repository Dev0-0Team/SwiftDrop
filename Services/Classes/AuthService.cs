//Done

using Microsoft.AspNetCore.Identity;
using SwiftDrop.Helpers;
using SwiftDrop.Models.Entities;
using SwiftDrop.Models.Enums;
using SwiftDrop.Repositories.Classes;
using SwiftDrop.Repositories.Interfaces;
using SwiftDrop.Repositories.Interfaces.Auth;
using SwiftDrop.Services.Classes;
using SwiftDrop.Services.Interfaces;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SwiftDrop.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;
        private readonly IAuditRepository _auditRepository;
        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
             IUserService userService,
             IUserRepository userRepository,
             ILogger<AuthService> logger,
             IAuditRepository auditRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userService = userService;
            _userRepository = userRepository;
            _auditRepository = auditRepository;
            _logger = logger;
        }

        public async Task<Result<Customer>> RegisterCustomerAsync(
            RegisterViewModel model,
            IFileService fileService,
            ICustomerRepository customerRepository)
        {
            string? profilePictureUrl = null;

            try
            {
                await _userRepository.BeginTransactionAsync();
                profilePictureUrl = await SaveProfilePictureAsync(model.ProfilePicture, fileService);
                    
                var createCustomerResult = await CreateCustomerAsync(model, profilePictureUrl, customerRepository);
                if (!createCustomerResult.IsSuccess)
                {
                    await _userRepository.RollbackTransactionAsync();
                    if (profilePictureUrl != null) 
                    await fileService.DeletePublicImageAsync(profilePictureUrl);

                      _logger.LogError("Failed to create customer: {Error}", createCustomerResult.Message);

                    return new Result<Customer>(ResultStatus.ValidationError, null!,
                            "Failed to create account.", createCustomerResult.ValidationErrors)!;
                   
                }

  

                await _userRepository.CommitTransactionAsync();

                _logger.LogInformation("Successfully created customer");
                await AuditAction(createCustomerResult.Data!.User.Id, "Customer Registration Successfully", nameof(Customer));


                return new Result<Customer>(ResultStatus.Success, createCustomerResult.Data!,
                    "Successfully registered, please check your email to confirm your account.")!;
            }
            catch (Exception ex)
            {
                await _userRepository.RollbackTransactionAsync();
                if (profilePictureUrl != null) await fileService.DeletePublicImageAsync(profilePictureUrl);

                _logger.LogError("Failed to create customer: {Error}", ex.Message);

                return new Result<Customer>(ResultStatus.DatabaseError, null!,
                    "An unexpected error occurred during the registration process, please try again later.")!;
            }
        }



        private async Task<string?> SaveProfilePictureAsync(IFormFile? profilePicture, IFileService fileService)
        {
            if (profilePicture == null) return null;
            return (await fileService.SavePublicImageAsync(profilePicture)).Data;
        }
        private async Task<Result<Customer>> CreateCustomerAsync(
            RegisterViewModel model, string? profilePictureUrl, ICustomerRepository customerRepository)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName,
                ProfileImageUrl = profilePictureUrl,
            };


            var createUserResult = await _userManager.CreateAsync(user, model.Password);

            if (!createUserResult.Succeeded)
                return new Result<Customer>(ResultStatus.ValidationError, null!, "Failed to create user", createUserResult.Errors.Select(e => e.Description).ToList());


            var addRoleResult = await _userManager.AddToRoleAsync(user, Role.Customer.ToString());

            if (!addRoleResult.Succeeded)
                return new Result<Customer>(ResultStatus.ValidationError, null!, "Failed to assign role to user", addRoleResult.Errors.Select(e => e.Description).ToList());

            var Cart = new Cart();
            var Customer = new Customer
            {
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                UserId = user.Id,
                Cart = Cart
            };

            customerRepository.Create(Customer);
            bool res = await customerRepository.SaveChangesAsync();

            if (!res)
                return new Result<Customer>(ResultStatus.ValidationError, null!, "Failed to create customer profile", null);


            return new Result<Customer>(ResultStatus.Success, Customer, "Customer profile created successfully.");
        }




        public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return token;
        }
        public async Task<Result<ApplicationUser>> ConfirmEmailAsync(string userId, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token) || !int.TryParse(userId, out var Id))
                    return new Result<ApplicationUser>(ResultStatus.ValidationError, null!, "Confirmation data is incomplete.");

                var user = await _userService.GetByIdAsync(Id, true);
                if (user == null)
                    return new Result<ApplicationUser>(ResultStatus.NotFound, null!, "User not found.");

                if (user.EmailConfirmed)
                    return new Result<ApplicationUser>(ResultStatus.Success, user, "Your account has already been confirmed. You can log in.");


                if (await _userManager.IsLockedOutAsync(user))
                    return new Result<ApplicationUser>(ResultStatus.ValidationError, user, "Your account is locked temporarily , please try again later.");
                return await ConfirmEmailCoreAsync(user, token)!;
            }
            catch
            {
                return new Result<ApplicationUser>(ResultStatus.DatabaseError, null!,
                    "An unexpected error occurred while confirming the account.");
            }
        }

        private async Task<Result<ApplicationUser>> ConfirmEmailCoreAsync(ApplicationUser user, string token)
        {
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {

                return new Result<ApplicationUser>(ResultStatus.Success, user,
                    "Email confirmed successfully.");
            }

            var errors = result.Errors.Select(e => e.Description).ToList();

            return new Result<ApplicationUser>(ResultStatus.ValidationError, null!,
                "The confirmation link is invalid or has expired.", errors);
        }

        ///Rate Limit for login endpoint 10 attempts per minute 
        public async Task<Result<ApplicationUser>> LoginAsync(LoginViewModel model,IEmailService emailService)
        {
            try
            {


             
                var user = await _userRepository.FindUserByEmailOrUserNameAsync(model.EmailOrUserName,true);
                if (user == null)
                    return new Result<ApplicationUser>(ResultStatus.Unauthorized, null!,
                        "Invalid username/email or password.")!;

                return await SignInUserAsync(user, model, emailService);
            }
            catch
            {
                _logger.LogError("An unexpected error occurred during the login process for user {EmailOrUserName}.", model.EmailOrUserName);
                return new Result<ApplicationUser>(ResultStatus.DatabaseError, null!,
                    "Please try again later.")!;
            }
        }


        private async Task<Result<ApplicationUser>> SignInUserAsync(ApplicationUser user, LoginViewModel model, IEmailService emailService)
        {

            if (await _userManager.IsLockedOutAsync(user))
            {
                return new Result<ApplicationUser>(ResultStatus.Unauthorized, null!, "Your account is locked temporarily , please try again later.")!;

            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
         
            if (!isPasswordValid)
            {
                _logger.LogCritical("Failed login attempt for user {UserId} with username/email {EmailOrUserName}.", user.Id, model.EmailOrUserName);
                await AuditAction(user.Id, $"Failed login attempt for user {user.Id} with username/email {model.EmailOrUserName}.", nameof(ApplicationUser));
                await _userManager.AccessFailedAsync(user);

              
                if (await _userManager.IsLockedOutAsync(user))
                {
                
                    return new Result<ApplicationUser>(ResultStatus.Unauthorized, null!, "Your account is locked temporarily , please try again later.")!;

                }

                return new Result<ApplicationUser>(ResultStatus.Unauthorized, null!,
                    "Invalid username/email or password.")!;
            }

            if (user.AccessFailedCount > 0 || user.LockoutEnd != null)
            {
               await _userRepository.SetLockoutNull(user);
            }

            _logger.LogInformation("Successfully logged in for user {UserId}", user.Id);
            await AuditAction(user.Id, $"Successfully logged in for user {user.Id}", nameof(ApplicationUser));

            if (!await _signInManager.CanSignInAsync(user))
            {
                return new Result<ApplicationUser>(ResultStatus.EmailNotConfirmed, user,
                    "The password is correct, but please confirm your email to be able to log in.")!;
            }

            await _signInManager.SignInAsync(user, model.RememberMe);

            return new Result<ApplicationUser>(ResultStatus.Success, user, "Successfully logged in.")!;
        }





        public async Task<Result<ApplicationUser>> ExternalLoginAsync(ICustomerRepository customerRepository)
        {
         
            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                    return Fail("Failed to get external login information.");

                var res = await HandleExistingExternalUserAsync(info);


                if (res.IsSuccess || res.StatusType == ResultStatus.Unauthorized)
                    return res;

                return await HandleNewExternalUser(info, customerRepository);
            }
            catch
            {
                _logger.LogError("An unexpected error occurred while signing in with Google.");
                return Fail("An unexpected error occurred while signing in with Google.");
            }
        }

     
        private async Task<Result<ApplicationUser>> HandleExistingExternalUserAsync(ExternalLoginInfo info)
        {
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (user == null)
            {
                return Fail("Not Found", ResultStatus.NotFound);
            }

            else if (!user.IsActive)
            {
                _logger.LogWarning("Attempted login for suspended or unavailable account: {UserId}", user.Id);
                await AuditAction(user.Id, $"Attempted login for suspended or unavailable account: {user.Id}", nameof(ApplicationUser));
                return Fail("Sorry, this account is suspended or currently unavailable.", ResultStatus.Unauthorized);
            }
            else if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Attempted login for locked out account: {UserId}", user.Id);
                await AuditAction(user.Id, $"Attempted login for locked out account: {user.Id}", nameof(ApplicationUser));
                return new Result<ApplicationUser>(ResultStatus.Unauthorized, null!, "Your account is locked temporarily , please try again later.")!;

            }
            if (user.AccessFailedCount > 0 || user.LockoutEnd != null)
            {
                await _userRepository.SetLockoutNull(user);
            }
            _logger.LogInformation("Successfully logged in for user {UserId} via external login", user.Id);
            await AuditAction(user.Id, $"Successfully logged in for user {user.Id} via external login", nameof(ApplicationUser));
            await _signInManager.SignInAsync(user, isPersistent: true);
            return new Result<ApplicationUser>(ResultStatus.Success, user, "Login successful.");
        }

        private async Task<Result<ApplicationUser>> HandleNewExternalUser(ExternalLoginInfo info, ICustomerRepository customerRepository)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrWhiteSpace(email))
                return Fail("Failed to get the email from the Google account.", ResultStatus.ValidationError);

            var existingUser = await _userService.FindByEmailAsync(email, true);
            if (existingUser != null)
                return await HandleExistingUserGoogleLinkAsync(existingUser, info);

            return await CreateExternalUserAsync(email, info, customerRepository);
        }


   

        private async Task<Result<ApplicationUser>> HandleExistingUserGoogleLinkAsync(
            ApplicationUser user, ExternalLoginInfo info)
        {
            if (!user.IsActive)
            {
                _logger.LogWarning("Attempted Google login for suspended or unavailable account: {UserId}", user.Id);
                await AuditAction(user.Id, $"Attempted Google login for suspended or unavailable account: {user.Id}", nameof(ApplicationUser));
                return Fail("Sorry, the account associated with this email is suspended or currently unavailable.", ResultStatus.Unauthorized);
            }
            if (user.EmailConfirmed == false)
                await _userManager.ConfirmEmailAsync(user, await _userManager.GenerateEmailConfirmationTokenAsync(user));

            if (await _userManager.IsLockedOutAsync(user)) { 
                _logger.LogWarning("Attempted Google login for locked out account: {UserId}", user.Id);
                 await AuditAction(user.Id, $"Attempted Google login for locked out account: {user.Id}", nameof(ApplicationUser));
                return Fail("Your account is locked temporarily , please try again later.", ResultStatus.Unauthorized)!;
                }


            if (user.AccessFailedCount > 0 || user.LockoutEnd != null)
                await _userRepository.SetLockoutNull(user);

            await _userManager.AddLoginAsync(user, info);
            await _signInManager.SignInAsync(user, isPersistent: true);

            _logger.LogInformation("Successfully linked Google account and logged in for user {UserId}", user.Id);
            await AuditAction(user.Id, $"Successfully linked Google account and logged in for user {user.Id}", nameof(ApplicationUser));
            return new Result<ApplicationUser>(ResultStatus.Success, user,
                "Google account linked and login successful.");
        }


        private async Task<Result<ApplicationUser>> CreateExternalUserAsync(string email, ExternalLoginInfo info, ICustomerRepository customerRepository)
        {
            try
            {

                await _userRepository.BeginTransactionAsync();

                var user = BuildExternalUserAsync(email, info);

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    await _userRepository.RollbackTransactionAsync();
                    var errors = createResult.Errors.Select(e => e.Description).ToList();
                    _logger.LogError("Failed to create user from external login");
                    await AuditAction(user.Id, $"Failed to create user from external login for email {email}", nameof(ApplicationUser));
                    return new Result<ApplicationUser>(ResultStatus.ValidationError, null!,
                        "Failed to create the account.", errors);
                }

                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (!addLoginResult.Succeeded)
                {
                    _logger.LogError("Failed to link Google account to new user {UserId}", user.Id);
                    await AuditAction(user.Id, $"Failed to link Google account to new user {user.Id}", nameof(ApplicationUser));
                    await _userRepository.RollbackTransactionAsync();
                    return Fail("Failed to link Google account to your new account.");
                }

                var roleResult = await _userManager.AddToRoleAsync(user, Role.Customer.ToString());


                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to assign role to new user {UserId}", user.Id);
                    await AuditAction(user.Id, $"Failed to assign role to new user {user.Id}", nameof(ApplicationUser));
                    await _userRepository.RollbackTransactionAsync();
                    return new Result<ApplicationUser>(ResultStatus.ValidationError, null!, "Failed to assign role to user", roleResult.Errors.Select(e => e.Description).ToList());
                }

                var Cart = new Cart();
                var Customer = new Customer
                {
                    UserId = user.Id,
                    Address = null,
                    PhoneNumber = null,
                    Cart = Cart
                };



                customerRepository.Create(Customer);
                bool res = await customerRepository.SaveChangesAsync();

                if (!res)
                {
                    _logger.LogError("Failed to create customer profile for user {UserId}", user.Id);
                    await AuditAction(user.Id, $"Failed to create customer profile for user {user.Id}", nameof(ApplicationUser));
                    return new Result<ApplicationUser>(ResultStatus.ValidationError, null!, "Failed to create customer profile", null);
                }

                await _userRepository.CommitTransactionAsync();

                _logger.LogInformation("Successfully created new user {UserId} from external login", user.Id);
                await AuditAction(user.Id, $"Successfully created new user {user.Id} from external login", nameof(ApplicationUser));
                await _signInManager.SignInAsync(user, isPersistent: true);

                return new Result<ApplicationUser>(ResultStatus.ExternalNewUser, user,
                    "Account created successfully, please complete your profile.");
            }
            catch
            {
                _logger.LogError("An unexpected error occurred while setting up the Google account for email {Email}.", email);
         
                await _userRepository.RollbackTransactionAsync();

                return Fail("An unexpected error occurred while setting up the Google account.");
            }
        }

        private ApplicationUser BuildExternalUserAsync(string email, ExternalLoginInfo info)
        {
            var user = new ApplicationUser
            {
                UserName = GenerateUniqueUserNameFromEmail(email),
                Email = email,
                FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email,
                ProfileImageUrl = null,
                EmailConfirmed = true,


            };

            return user;



        }
   
  


        private Result<ApplicationUser> Fail(string message,
            ResultStatus status = ResultStatus.DatabaseError) =>
            new(status, null!, message);


        private string GenerateUniqueUserNameFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                throw new ArgumentException("Invalid email");

            string baseName = email.Split('@')[0];
            baseName = Regex.Replace(baseName, @"[^a-zA-Z0-9]", "");

            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "user";

            string shortGuid = Guid.NewGuid().ToString("N").Substring(0, 3);

            return $"{baseName}_{shortGuid}";
        }



        public async Task<Result> LogoutAsync(int userId)
        {
            try
            {
           
                await _signInManager.SignOutAsync();

                _logger.LogInformation("User {UserId} is logging out.", userId);
                await AuditAction(userId, $"User {userId} logged out.", nameof(ApplicationUser));

                return new Result(ResultStatus.Success, "Successfully logged out.");

               
            }
            catch
            {
                return new Result(ResultStatus.DatabaseError, "Please try again later.");
            }
        }

        private async Task AuditAction(int userId , string action , string entityName)
        {
            var AuditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = nameof(Customer),
                CreatedAt = DateTime.UtcNow,

            };

            _auditRepository.Create(AuditLog);
            await _auditRepository.SaveChangesAsync();


        }





        //public async Task<string> GenerateChangeEmailTokenAsync(
        //    ApplicationUser user, string newEmail)
        //{
        //    var token = await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
        //    return token;

        //}
        //public async Task<Result<ApplicationUser>> ConfirmEmailChangeAsync(int userId, string newEmail, string token)
        //{
        //    try
        //    {
        //        if (userId <= 0
        //            || string.IsNullOrWhiteSpace(newEmail)
        //            || string.IsNullOrWhiteSpace(token))
        //            return new Result<ApplicationUser>(ResultStatus.ValidationError, null!, "Confirmation data is incomplete.");

        //        var user = await _userService.GetByIdAsync(userId, true);
        //        if (user == null)
        //            return new Result<ApplicationUser>(ResultStatus.NotFound, null!, "User not found.");
        //        return await ApplyEmailChangeAsync(user, newEmail, token);
        //    }
        //    catch
        //    {
        //        return new Result<ApplicationUser>(ResultStatus.DatabaseError, null!,
        //            "An unexpected error occurred while confirming the email change.");
        //    }
        //}

        //private async Task<Result<ApplicationUser>> ApplyEmailChangeAsync(ApplicationUser user, string newEmail, string token)
        //{
        //    try
        //    {

        //        if (user.Email == newEmail)
        //            return new Result<ApplicationUser>(ResultStatus.Success, user, "Your email has been changed successfully.");

        //        await _userRepository.BeginTransactionAsync();


        //        var result = await _userManager.ChangeEmailAsync(user, newEmail, token);

        //        if (!result.Succeeded)
        //        {

        //            await _userRepository.RollbackTransactionAsync();
        //            var errors = result.Errors.Select(e => e.Description).ToList();
        //            return new Result<ApplicationUser>(ResultStatus.ValidationError, null!,
        //                "The confirmation link is invalid or has expired.", errors);
        //        }


        //        var userLogins = await _userManager.GetLoginsAsync(user);
        //        foreach (var login in userLogins)
        //        {
        //            var removeResult = await _userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
        //            if (!removeResult.Succeeded)
        //            {

        //                await _userRepository.RollbackTransactionAsync();
        //                return new Result<ApplicationUser>(ResultStatus.DatabaseError, null!,
        //                    "An error occurred while unlinking external accounts.");
        //            }
        //        }

        //        await _userRepository.CommitTransactionAsync();

        //        return new Result<ApplicationUser>(ResultStatus.Success, user, "Your email has been changed successfully.");
        //    }
        //    catch
        //    {

        //        await _userRepository.RollbackTransactionAsync();

        //        return new Result<ApplicationUser>(ResultStatus.DatabaseError, null!,
        //            "An unexpected error occurred while saving the changes.");
        //    }
        //}


        //public async Task<string> GenerateForgotPasswordOTPAsync(ApplicationUser user)
        //{
        //    return await _userManager.GenerateUserTokenAsync(user, "Email", "PasswordResetOTP");
        //}

        //public async Task<Result<string>> VerifyForgotPasswordOTPAsync(string email, string otp)
        //{
        //    try
        //    {
        //        var user = await _userService.FindByEmailAsync(email,true);
        //        if (user == null)
        //            return new Result<string>(ResultStatus.ValidationError, null!, "Invalid data.");

        //        bool isValid = await _userManager.VerifyUserTokenAsync(user, "Email", "PasswordResetOTP", otp);
        //        if (!isValid)
        //            return new Result<string>(ResultStatus.ValidationError,null!,
        //                "The verification code is incorrect or has expired.");

        //        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        //        return new Result<string>(ResultStatus.Success, resetToken, "Verification successful.");
        //    }
        //    catch
        //    {
        //        return new Result<string>(ResultStatus.DatabaseError,null!,
        //            "An unexpected error occurred, please try again later.");
        //    }
        //}

        //public async Task<Result<ApplicationUser>> ResetPasswordAsync(string email, string newPassword, string resetToken)
        //{
        //    try
        //    {
        //        var user = await _userService.FindByEmailAsync(email, true);
        //        if (user == null)
        //            return new Result<ApplicationUser>(ResultStatus.ValidationError, null!, "Invalid data.");


        //        if (await _userManager.IsLockedOutAsync(user))
        //        {
        //            return new Result<ApplicationUser>(ResultStatus.ValidationError, null!, "Your account is locked temporarily , please try again later.");

        //        }
        //        return await ApplyPasswordResetAsync(user, newPassword, resetToken);
        //    }
        //    catch
        //    {
        //        return new Result<ApplicationUser>(ResultStatus.DatabaseError, null!,
        //            "An unexpected error occurred, please try again later.");
        //    }
        //}

        //private async Task<Result<ApplicationUser>> ApplyPasswordResetAsync(ApplicationUser user, string newPassword, string resetToken)
        //{
        //    var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

        //    if (result.Succeeded)
        //    {

        //        return new Result<ApplicationUser>(ResultStatus.Success, user, "Password reset successfully.");
        //    }

        //    var errors = result.Errors.Select(e => e.Description).ToList();
        //    return new Result<ApplicationUser>(ResultStatus.ValidationError, null!, "Failed to change password.", errors);
        //}

        //public async Task<Result<ApplicationUser>> ChangePasswordAsync(
        //   ApplicationUser user, string currentPassword, string newPassword)
        //{
        //    try
        //    {
        //        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        //        if (result.Succeeded)
        //        {

        //            return new Result<ApplicationUser>(ResultStatus.Success, user, "Password changed successfully.")!;
        //        }

        //        var errors = result.Errors.Select(e => e.Description).ToList();
        //        return new Result<ApplicationUser>(ResultStatus.ValidationError, null!, "Failed to change password.", errors)!;
        //    }
        //    catch
        //    {
        //        return new Result<ApplicationUser>(ResultStatus.DatabaseError, null!,
        //            "An unexpected error occurred while changing the password.")!;
        //    }
        //}











    }
}