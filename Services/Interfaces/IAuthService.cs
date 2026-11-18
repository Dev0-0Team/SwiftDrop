//Done
using SwiftDrop.Helpers;
using SwiftDrop.Models.Entities;
using SwiftDrop.Repositories.Interfaces;
using SwiftDrop.Repositories.Interfaces.Auth;

namespace SwiftDrop.Services.Interfaces
{
    public interface IAuthService
    {

        Task<Result<Customer>> RegisterCustomerAsync( RegisterViewModel model, IFileService fileService,  ICustomerRepository customerRepository);
        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);
        Task<Result<ApplicationUser>> ConfirmEmailAsync(string userId, string token);

        ///Rate Limit for login endpoint 10 attempts per minute 
        Task<Result<ApplicationUser>> LoginAsync(LoginViewModel model, IEmailService emailService);
        Task<Result<ApplicationUser>> ExternalLoginAsync(ICustomerRepository customerRepository);
        Task<Result> LogoutAsync(int userId);

        // Task<string> GenerateChangeEmailTokenAsync(ApplicationUser user, string newEmail);
        //  Task<Result<ApplicationUser>> ConfirmEmailChangeAsync(int userId, string newEmail, string token);


        //Task<string> GenerateForgotPasswordOTPAsync(ApplicationUser user);
        //Task<Result<string>> VerifyForgotPasswordOTPAsync(string email, string otp);
        //Task<Result<ApplicationUser>> ResetPasswordAsync(string email, string newPassword, string resetToken);
        //Task<Result<ApplicationUser>> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);


    }
}