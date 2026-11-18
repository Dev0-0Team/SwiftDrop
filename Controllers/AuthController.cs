using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SwiftDrop.Helpers;
using SwiftDrop.Models.Entities;
using SwiftDrop.Repositories.Interfaces;
using SwiftDrop.Repositories.Interfaces.Auth;
using SwiftDrop.Services.Interfaces;
using System.Text.RegularExpressions;

namespace SwiftDrop.Controllers
{
    [Route("auth")]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly ITimeLimitedDataProtector _protector;
        private readonly IEmailService _emailService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        public AuthController( 
            IAuthService authService,
            IUserService userService,
            IDataProtectionProvider dataProtectionProvider,
            IEmailService emailService,
            SignInManager<ApplicationUser> signInManager)
        {
            _authService = authService;
            _userService = userService;
            _emailService = emailService;
            _signInManager = signInManager;
            _protector = dataProtectionProvider.CreateProtector("SwiftDrop.SecureCookies").ToTimeLimitedDataProtector();
        }




        [HttpGet("register")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Register(string? returnUrl = "/Home/Home")
        {

            if (User.Identity?.IsAuthenticated == true)
                return RedirectToHome(false, false);

            ViewBag.ReturnUrl = returnUrl;
            return View(new RegisterViewModel());
        }



        [HttpPost("register")]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("RegisterLimiting")]

        public async Task<IActionResult> Register([FromForm] RegisterViewModel model, [FromServices] IFileService fileService , [FromServices] ICustomerRepository customerRepository)
        {


            if (User.Identity?.IsAuthenticated == true)
                return RedirectToHome(true, false);

            if (!ModelState.IsValid)
                return ValidationErrorJson("Invalid input data.");


            var (emailExists, userNameExists) = await _userService.CheckUserExistsAsync(model.Email, model.UserName);

            if (emailExists)
                return FailureJson("This email is already associated with an account.", null, null);

            if (userNameExists)
                return FailureJson("This username is already taken.", null, null);

            var result = await _authService.RegisterCustomerAsync(model, fileService, customerRepository);

            if (!result.IsSuccess)
                return FailureJson(result.Message ?? "An error occurred while registering.", null, result.ValidationErrors);

            var user = result.Data!.User!;


            var rawToken = await _authService.GenerateEmailConfirmationTokenAsync(user);

            string finalLink = Url.Action(
                action: nameof(ConfirmEmail),
                controller: "Auth",
                values: new { userId = user.Id, token = rawToken },
                protocol: Request.Scheme)!;

            _ = _emailService.SendConfirmationEmailAsync(model.Email, model.FullName, finalLink);

            SetEncryptedCookie("PendingEmail", model.Email, 45);

            return SuccessJson("Registration successful, please check your email.", Url.Action(
                action: nameof(PleaseConfirmEmail),
                controller: "Auth"
                ));
        }



        [HttpGet("pending-confirm-email")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult PleaseConfirmEmail()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToHome(false, false);

            var email = GetDecryptedCookie("PendingEmail");

            if (string.IsNullOrWhiteSpace(email) || !IsValidEmailWithRegex(email))
                return RedirectToLoginWithAlert(AlertMessageType.Warning, "Please Enter Email", false, false);


            return View("PleaseConfirmEmail", email);
        }


        [HttpPost("resend-confirmation-email")]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("EmailConfirmationLimiting")]
        public async Task<IActionResult> ResendConfirmationEmail()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToHome(true, false);


            var email = GetDecryptedCookie("PendingEmail");


            if (string.IsNullOrWhiteSpace(email) || !IsValidEmailWithRegex(email))
                return FailureJson("Please enter a valid email address.", null, null);

            var user = await _userService.FindByEmailAsync(email, true);

            if (user == null)
            {
                return SuccessJson("The confirmation link has been resent successfully. Please check your email.", null);
            }

            if (user.EmailConfirmed)
            {
                RemoveEncryptedCookie("PendingEmail");
                return RedirectToLoginWithAlert(AlertMessageType.Info, "This account has already been confirmed. You can log in.", true, true);
            }

            if (await _userService.IsLockedOutAsync(user))
                return RedirectToLoginWithAlert(AlertMessageType.Danger, "Your account is locked temporarily , please try again later.", true, false);

            var rawToken = await _authService.GenerateEmailConfirmationTokenAsync(user);

            string finalLink = Url.Action(
                action: nameof(ConfirmEmail),
                controller: "Auth",
                values: new { userId = user.Id, token = rawToken },
                protocol: Request.Scheme)!;

            _ = _emailService.SendConfirmationEmailAsync(user.Email!, user.FullName, finalLink);

            SetEncryptedCookie("PendingEmail", email, 45);

            return SuccessJson("The confirmation link has been resent successfully. Please check your email.", null);
        }



        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return RedirectToLoginWithAlert(AlertMessageType.Danger, "The confirmation link is invalid.", false);

            var result = await _authService.ConfirmEmailAsync(userId, token);

            if (!result.IsSuccess)
                return RedirectToLoginWithAlert(AlertMessageType.Danger, result.Message!, false);

            await _signInManager.SignInAsync(result.Data!, isPersistent: true);

            RemoveEncryptedCookie("PendingEmail");

            return RedirectToHome(false, true);
        }



        [HttpGet("login")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Login(
            string? returnUrl = "/Home/Home",
            AlertMessageType? alertType = null,
            string? alertText = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToHome(false, false);

            var model = new LoginViewModel ();

            ViewBag.ReturnUrl = returnUrl;

            if (alertType.HasValue && !string.IsNullOrEmpty(alertText))
                model.Alert = new AlertMessage { Type = alertType.Value, Text = alertText };

            return View(model);
        }


        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("LoggingLimiting")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToHome(true, false);

            if (!ModelState.IsValid)
                return ValidationErrorJson("Invalid input data.");


            var result = await _authService.LoginAsync(model, _emailService);


            if (result.StatusType == ResultStatus.EmailNotConfirmed)
            {
                SetEncryptedCookie("PendingEmail", result.Data?.Email!, 45);
                return SuccessJson(null, Url.Action(nameof(PleaseConfirmEmail), "Account"));
            }

            if (!result.IsSuccess)
                return FailureJson(result.Message ?? "Login failed.", null, null);


            string finalRedirectUrl  = Url.Action(AppConstants.HomePage.Action, AppConstants.HomePage.Controller)!;

            return SuccessJson("Login successful.", finalRedirectUrl);


        }


        [HttpPost("login-with-google")]
        [ValidateAntiForgeryToken]
        public IActionResult LoginWithGoogle(string? returnUrl)
        {

            var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth", new { returnUrl }, Request.Scheme);

            var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            properties.SetParameter("prompt", "select_account");

            return Challenge(properties, "Google");
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback([FromServices] ICustomerRepository customerRepository,string? returnUrl = null, string? remoteError = null)
        {
            if (remoteError != null)
                return RedirectToLoginWithAlert(AlertMessageType.Danger, $"Error from Google: {remoteError}", false, false);

            var result = await _authService.ExternalLoginAsync(customerRepository);

            if (!result.IsSuccess)
                return RedirectToLoginWithAlert(AlertMessageType.Danger, result.Message!, false, false);

         
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToHome(false, true);
        }

        [HttpPost("logout")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var res = await _authService.LogoutAsync(base.GetNumericUserId()!.Value);
           
            return RedirectToHome(false, true);

        }














        //[HttpGet("request-email-change")]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //[Authorize]
        //public IActionResult RequestEmailChange(string? returnUrl = "/users/profile")
        //{
        //    ViewBag.ReturnUrl = returnUrl;
        //    return View(new RequestEmailChangeViewModel());
        //}



        //[HttpPut("request-email-change")]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //[EnableRateLimiting("EmailConfirmationLimiting")]

        //public async Task<IActionResult> RequestEmailChange([FromBody] RequestEmailChangeViewModel requestEmailChangeModel)
        //{
        //    if (!ModelState.IsValid)
        //        return ValidationErrorJson("Please ensure the input data is valid.");


        //    var user = await _userService.GetByIdAsync(GetNumericUserId()!.Value, false);
        //    if (user == null) return FailureJson("User not found.", null, null);


        //    if (user.Email == requestEmailChangeModel.Email) return FailureJson("The new email is the same as the current one.", null, null);

        //    bool emailExists = await _userService.IsEmailExistAsync(requestEmailChangeModel.Email);
        //    if (emailExists) return FailureJson("This email is already in use.", null, null);

        //    var token = await _authService.GenerateChangeEmailTokenAsync(user, requestEmailChangeModel.Email);
        //    var confirmationLink = Url.Action(
        //        nameof(ConfirmEmailChange), "Account",
        //        new { userId = user.Id, newEmail = requestEmailChangeModel.Email, token },
        //        Request.Scheme)!;

        //    _ = _emailService.SendEmailChangeConfirmationAsync(
        //        requestEmailChangeModel.Email,
        //        user.FullName,
        //        confirmationLink);

        //    SetEncryptedCookie("PendingEmailChange", requestEmailChangeModel.Email, 45);

        //    return SuccessJson("The confirmation link has been sent to the new email. Please confirm it.", Url.Action(
        //        action: nameof(PleaseConfirmEmailChange),
        //        controller: "Account")!);
        //}



        //[HttpGet("pending-confirm-email-change")]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //[Authorize]
        //public IActionResult PleaseConfirmEmailChange()
        //{

        //    var email = GetDecryptedCookie("PendingEmailChange");

        //    if (string.IsNullOrWhiteSpace(email) || !IsValidEmailWithRegex(email))
        //        return RedirectToAction(AppConstants.ProfilePage.Controller, AppConstants.ProfilePage.Action, new { alertType = EnAlertMessageType.Warning, alertText = "Please Enter Email" });

        //    return View("PleaseConfirmEmailChange", email);
        //}


        //[HttpPut("resend-confirmation-email-change")]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //[EnableRateLimiting("EmailConfirmationLimiting")]
        //public async Task<IActionResult> ResendConfirmationEmailChange()
        //{

        //    var email = GetDecryptedCookie("PendingEmailChange");

        //    if (string.IsNullOrWhiteSpace(email) || !IsValidEmailWithRegex(email)) return FailureJson("Please enter a valid email.", null, null);

        //    var user = await _userService.GetByIdAsync(GetNumericUserId().Value, false);
        //    if (user == null) return FailureJson("User not found.", null, null);

        //    var rawToken = await _authService.GenerateChangeEmailTokenAsync(user, email);

        //    string finalLink = Url.Action(
        //        action: nameof(ConfirmEmailChange),
        //        controller: "Account",
        //        values: new { userId = user.Id, newEmail = email, token = rawToken },
        //        protocol: Request.Scheme)!;

        //    _ = _emailService.SendEmailChangeConfirmationAsync(email, user.FullName, finalLink);

        //    SetEncryptedCookie("PendingEmailChange", email, 45);


        //    return SuccessJson("The confirmation link has been resent successfully. Please check your email.", null);
        //}


        //[HttpGet("confirm-email-change")]
        //public async Task<IActionResult> ConfirmEmailChange(int userId, string newEmail, string token, [FromServices] UserManager<ApplicationUser> userManager)
        //{
        //    int? currentUserId = GetNumericUserId();

        //    if (userId <= 0 || string.IsNullOrWhiteSpace(newEmail) || string.IsNullOrWhiteSpace(token) || !IsValidEmailWithRegex(newEmail))
        //    {

        //        if (User.Identity?.IsAuthenticated == false)
        //            return RedirectToLoginWithAlert(EnAlertMessageType.Danger, "The confirmation link is invalid.", false, false);

        //        else if (currentUserId != null && userId == currentUserId)
        //            return RedirectToAction(AppConstants.ProfilePage.Action, AppConstants.ProfilePage.Controller, new { alertType = EnAlertMessageType.Danger, alertText = "The confirmation link is invalid." });

        //        return RedirectToHome(false, false);
        //    }

        //    var result = await _authService.ConfirmEmailChangeAsync(userId, newEmail, token);


        //    if (!result.Status)
        //    {

        //        if (User.Identity?.IsAuthenticated == false)
        //            return RedirectToLoginWithAlert(EnAlertMessageType.Danger, result.Message ?? "The confirmation link is invalid.", false, false);

        //        else if (userId == currentUserId)
        //            return RedirectToAction(AppConstants.ProfilePage.Action, AppConstants.ProfilePage.Controller, new { alertType = EnAlertMessageType.Danger, alertText = result.Message ?? "The confirmation link is invalid." });

        //        return RedirectToHome(false, false);


        //    }


        //    await userManager.UpdateSecurityStampAsync(result.Data!);


        //    if ((User.Identity?.IsAuthenticated == false) || (User.Identity?.IsAuthenticated == true && currentUserId != userId))
        //    {
        //        RemoveUserInterestsCookie();
        //        await _signInManager.SignInAsync(result.Data!, isPersistent: true);

        //    }
        //    else
        //        await _signInManager.RefreshSignInAsync(result.Data!);



        //    RemoveEncryptedCookie("PendingEmailChange");



        //    return RedirectToAction(AppConstants.ProfilePage.Action, AppConstants.ProfilePage.Controller, new { alertType = EnAlertMessageType.Success, alertText = result.Message! });




        //}



        //[HttpGet("forgot-password")]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult ForgotPassword()
        //{
        //    if (User.Identity?.IsAuthenticated == true)
        //        return RedirectToHome(false, false);

        //    return View(new ForgotPasswordViewModel());
        //}

        //[HttpPost("forgot-password")]
        //[ValidateAntiForgeryToken]
        //[EnableRateLimiting("ForgetPasswordOTPLimiting")]
        //public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel? model)
        //{
        //    if (User.Identity?.IsAuthenticated == false && (model == null || model.Email == null))
        //        return FailureJson("Please enter your email.", null);

        //    ApplicationUser? user = null;
        //    string? email = null;

        //    if (User.Identity?.IsAuthenticated == true)
        //    {
        //        user = await _userService.GetByIdAsync(GetNumericUserId().Value, false);
        //        email = user?.Email;
        //    }
        //    else
        //    {
        //        user = await _userService.FindByEmailAsync(model?.Email!, false);
        //        email = model?.Email;

        //    }

        //    if (email == null)
        //        email = model?.Email ?? "**************@gmail.com";

        //    if (user == null)
        //    {
        //        SetEncryptedCookie("ResetPasswordEmail", email, 45);

        //        return SuccessJson($"A message will be sent to {email} shortly.",
        //       Url.Action(
        //          action: nameof(VerifyOtp),
        //          controller: "Account")
        //        );

        //    }

        //    if (await _authService.IsLockedOutAsync(user))
        //    {
        //        return FailureJson("Your account is locked temporarily , please try again later.", null);

        //    }
        //    var otpCode = await _authService.GenerateForgotPasswordOTPAsync(user);
        //    _ = _emailService.SendResetPasswordOTPEmailAsync(email, user.FullName, otpCode);

        //    SetEncryptedCookie("ResetPasswordEmail", email, 45);

        //    return SuccessJson($"A message will be sent to {email} shortly.",
        // Url.Action(
        //    action: nameof(VerifyOtp),
        //    controller: "Account"));
        //}



        //[HttpGet("verify-otp")]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult VerifyOtp()
        //{
        //    var email = GetDecryptedCookie("ResetPasswordEmail");

        //    if (string.IsNullOrWhiteSpace(email) || !IsValidEmailWithRegex(email))
        //        return RedirectToAction(nameof(ForgotPassword));

        //    return View("VerifyOtp", email);
        //}


        //[HttpPost("verify-otp")]
        //[ValidateAntiForgeryToken]
        //[EnableRateLimiting("VerityOTPLimiting")]// 10 attempts per minute
        //public async Task<IActionResult> VerifyOtp([FromBody] string otp)
        //{
        //    var email = GetDecryptedCookie("ResetPasswordEmail");

        //    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp) || !IsValidEmailWithRegex(email))
        //        return ValidationErrorJson("Please ensure the entered data is correct.");

        //    var result = await _authService.VerifyForgotPasswordOTPAsync(email, otp);

        //    if (!result.Status)
        //    {
        //        return FailureJson(result.Message ?? "The verification code is incorrect.", null, null);
        //    }

        //    return SuccessJson(null, Url.Action(nameof(ResetPassword), "Account", new { resetToken = result.Data }));

        //}

        //[HttpPost("resend-forgot-password-otp")]
        //[ValidateAntiForgeryToken]
        //[EnableRateLimiting("ForgetPasswordOTPLimiting")]
        //public async Task<IActionResult> ResendForgotPasswordOtp()
        //{
        //    var email = GetDecryptedCookie("ResetPasswordEmail");

        //    if (string.IsNullOrWhiteSpace(email) || !IsValidEmailWithRegex(email))
        //        return ValidationErrorJson("Please ensure the entered data is correct.");


        //    var user = await _userService.FindByEmailAsync(email, true);


        //    SetEncryptedCookie("ResetPasswordEmail", email, 45);

        //    if (user == null)
        //        return SuccessJson("The code has been resent successfully.", null);

        //    if (await _authService.IsLockedOutAsync(user))
        //    {
        //        return FailureJson("Your account is locked temporarily , please try again later.", null);

        //    }

        //    var otpCode = await _authService.GenerateForgotPasswordOTPAsync(user);
        //    _ = _emailService.SendResetPasswordOTPEmailAsync(user.Email!, user.FullName, otpCode);


        //    return SuccessJson("The code has been resent successfully.", null);

        //}

        //[HttpGet("reset-password")]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult ResetPassword(string resetToken)
        //{
        //    var email = GetDecryptedCookie("ResetPasswordEmail");

        //    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(resetToken) || !IsValidEmailWithRegex(email))
        //        return RedirectToAction(nameof(ForgotPassword));

        //    return View(new ResetPasswordViewModel { Email = email, ResetToken = resetToken });
        //}

        //[HttpPut("reset-password")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordViewModel model, [FromServices] UserManager<ApplicationUser> userManager)
        //{
        //    var email = GetDecryptedCookie("ResetPasswordEmail");


        //    if (!ModelState.IsValid)
        //        return ValidationErrorJson("Please ensure the entered data is correct.");

        //    if (email != model.Email)
        //        return FailureJson("An error occurred while resetting the password.", null, null);

        //    var result = await _authService.ResetPasswordAsync(
        //        model.Email, model.NewPassword, model.ResetToken);

        //    if (!result.Status)
        //        return FailureJson(result.Message ?? "An error occurred while resetting the password.", null, null);

        //    var user = result.Data!;

        //    if (user.LockoutEnd != null || user.AccessFailedCount > 0)
        //        await _userService.SetLockoutNull(user);

        //    RemoveEncryptedCookie("ResetPasswordEmail");

        //    if (User.Identity?.IsAuthenticated == true)
        //    {
        //        await _signInManager.RefreshSignInAsync(user);
        //        return SuccessJson("The password has been reset successfully.", Url.Action(AppConstants.ProfilePage.Action, AppConstants.ProfilePage.Controller, new { alertType = EnAlertMessageType.Info, alertText = "The password has been reset successfully." }));
        //    }
        //    return RedirectToLoginWithAlert(EnAlertMessageType.Success,
        //        "The password has been reset successfully. You can now log in.", true, true);


        //}


        //[HttpGet("change-password")]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //[Authorize]
        //public IActionResult ChangePassword(string? returnUrl = "/users/profile")
        //{
        //    ViewBag.ReturnUrl = returnUrl;
        //    return View(new ChangePasswordViewModel());
        //}

        //[HttpPut("change-password")]
        //[Authorize]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model, [FromServices] UserManager<ApplicationUser> userManager)
        //{
        //    if (!ModelState.IsValid)
        //        return ValidationErrorJson("Please ensure the entered data is correct.");

        //    var user = await _userService.GetByIdAsync(GetNumericUserId()!.Value, true);
        //    if (user == null) return FailureJson("The user does not exist.", null, null);
        //    if (await _authService.IsLockedOutAsync(user))
        //    {
        //        return FailureJson("Your account is locked temporarily , please try again later.", null, null);

        //    }


        //    var result = await _authService.ChangePasswordAsync(
        //        user, model.CurrentPassword, model.NewPassword);

        //    if (result.Status)
        //    {
        //        if (user.LockoutEnd != null || user.AccessFailedCount > 0)
        //            await _userService.SetLockoutNull(user);

        //        await _signInManager.RefreshSignInAsync(user);

        //        return SuccessJson(result.Message, Url.Action(AppConstants.ProfilePage.Action, AppConstants.ProfilePage.Controller, new { alertType = EnAlertMessageType.Info, alertText = "The password has been reset successfully." }));
        //    }
        //    return FailureJson(result.Message ?? "An error occurred while changing the password.", null, null);

        //}


        //[HttpGet("access-denied")]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult AccessDenied()
        //{
        //    return View();
        //}






        private void SetEncryptedCookie(string key, string value, double expireMinutes)
        {
            if (string.IsNullOrEmpty(value)) return;


            var encryptedValue = _protector.Protect(value, TimeSpan.FromMinutes(expireMinutes));

            CookieOptions options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true
            };
            Response.Cookies.Append(key, encryptedValue, options);
        }
        private string? GetDecryptedCookie(string key)
        {
            var encryptedValue = Request.Cookies[key];

            if (string.IsNullOrWhiteSpace(encryptedValue))
                return null;

            try
            {

                return _protector.Unprotect(encryptedValue);
            }
            catch
            {

                Response.Cookies.Delete(key);
                return null;
            }
        }
        private void RemoveEncryptedCookie(string key)
        {
            Response.Cookies.Delete(key);
        }
        private bool IsValidEmailWithRegex(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {

                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";


                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {

                return false;
            }
        }

















    }
}
