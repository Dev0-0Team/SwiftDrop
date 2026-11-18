
using Microsoft.Extensions.Options;
using SwiftDrop.Helpers;
using SwiftDrop.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace SwiftDrop.Services.Classes
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptionsMonitor<EmailSettings> optionsMonitor, ILogger<EmailService> logger)
        {
          
            _emailSettings = optionsMonitor.CurrentValue;
            _logger = logger;
        }

        private async Task<Result> SendEmailAsync(string emailTo, string subject, string body,string goal)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_emailSettings.SenderEmail);
                    mail.To.Add(emailTo);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                        smtp.EnableSsl = true;

                        await smtp.SendMailAsync(mail);
                    }
                }
                _logger.LogInformation($"Email sending Success - Goal: {goal} , To: {emailTo}");
                return new Result(ResultStatus.Success, "تم");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Email sending failed - Goal: {goal} , To: {emailTo} , Reason: {ex.Message}");
                return new Result(ResultStatus.ValidationError, ex.Message);
            }
        }


        public async Task<Result> SendConfirmationEmailAsync(string email, string fullName, string finalLink)
        {
            var emailBody = BuildConfirmationEmailBody(fullName, finalLink);
            return await SendEmailAsync(email, "Swift Drop - Confirm Your Email", emailBody, "Confirm Email");
        }

        private static string BuildConfirmationEmailBody(string fullName, string finalLink)
        {
            return $@"
    <div style='font-family: Arial, sans-serif; background-color: #f4f7f6; padding: 30px; color: #333;'>
        <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 40px; border-radius: 8px; box-shadow: 0 4px 10px rgba(0,0,0,0.05); text-align: center;'>
            <h2 style='color: #0056b3; margin-top: 0;'>Welcome to Swift Drop!</h2>
            <p style='font-size: 16px; line-height: 1.6; color: #555;'>Hello <strong>{fullName}</strong>,</p>
            <p style='font-size: 16px; line-height: 1.6; color: #555;'>Thank you for registering with us. Please confirm your email address to activate your account by clicking the button below:</p>
            <div style='margin: 30px 0;'>
                <a href='{finalLink}' style='background-color: #0056b3; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; display: inline-block;'>Confirm Account</a>
            </div>
            <p style='font-size: 14px; color: #888; margin-top: 30px;'>If you did not create an account, ignore this email.</p>
        </div>
    </div>";
        }

        public async Task<Result> SendEmailChangeConfirmationAsync(string newEmail, string fullName, string finalLink)
        {
            var emailBody = BuildEmailChangeBody(fullName, finalLink);
            return await SendEmailAsync(newEmail, "Swift Drop - Confirm Email Change", emailBody, "Confirm Email Change");
        }

        private static string BuildEmailChangeBody(string fullName, string finalLink)
        {
            return $@"
    <div style='font-family: Arial, sans-serif; background-color: #f4f7f6; padding: 30px; color: #333;'>
        <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 40px; border-radius: 8px; box-shadow: 0 4px 10px rgba(0,0,0,0.05); text-align: center;'>
            <h2 style='color: #28a745; margin-top: 0;'>Email Change Request</h2>
            <p style='font-size: 16px; line-height: 1.6; color: #555;'>Hello <strong>{fullName}</strong>,</p>
            <p style='font-size: 16px; line-height: 1.6; color: #555;'>We received a request to change the email address associated with your Swift Drop account. Please confirm this change by clicking the button below:</p>
            <div style='margin: 30px 0;'>
                <a href='{finalLink}' style='background-color: #28a745; color: #ffffff; padding: 14px 28px; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; display: inline-block;'>Confirm New Email</a>
            </div>
            <p style='font-size: 14px; color: #888; margin-top: 30px;'>If you did not request this change, please contact our support team immediately.</p>
        </div>
    </div>";
        }


        public async Task<Result> SendResetPasswordOTPEmailAsync(string email, string fullName, string otp)
        {
            var emailBody = BuildResetPasswordOTPEmailBody(fullName, otp);
            return await SendEmailAsync(email!, "Swift Drop - Password Reset Verification Code", emailBody , "Reset Password");
        }

        private static string BuildResetPasswordOTPEmailBody(string fullName, string otpCode)
        {
            return $@"
    <div style='font-family: Arial, sans-serif; background-color: #f4f7f6; padding: 30px; color: #333;'>
        <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 40px; border-radius: 8px; box-shadow: 0 4px 10px rgba(0,0,0,0.05); text-align: center;'>
            <h2 style='color: #333; margin-top: 0;'>Password Reset Request</h2>
            <p style='font-size: 16px; line-height: 1.6; color: #555;'>Hello <strong>{fullName}</strong>,</p>
            <p style='font-size: 16px; line-height: 1.6; color: #555;'>You requested a verification code to reset your Swift Drop account password. Please use the following One-Time Password (OTP) to complete the process:</p>
            
            <div style='margin: 30px auto; padding: 15px; background-color: #f8f9fa; border: 2px dashed #0056b3; border-radius: 8px; display: inline-block; min-width: 200px;'>
                <span style='font-size: 32px; font-weight: bold; letter-spacing: 10px; color: #0056b3;'>{otpCode}</span>
            </div>
            
            <p style='font-size: 14px; color: #dc3545; font-weight: bold;'>Note: This code is valid for a single use and will expire in a few minutes.</p>
            <p style='font-size: 14px; color: #888; margin-top: 20px;'>If you did not request a password reset, you can safely ignore this email.</p>
        </div>
    </div>";
        }
     









    }


}


