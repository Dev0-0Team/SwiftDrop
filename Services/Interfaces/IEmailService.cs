using SwiftDrop.Helpers;

namespace SwiftDrop.Services.Interfaces
{
    public interface IEmailService
    {
        Task<Result> SendConfirmationEmailAsync(string email, string fullName, string finalLink);
        Task<Result> SendEmailChangeConfirmationAsync(string newEmail, string fullName, string finalLink);
        Task<Result> SendResetPasswordOTPEmailAsync(string email, string fullName, string otp);
  
    }

}
