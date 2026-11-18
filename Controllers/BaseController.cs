using Microsoft.AspNetCore.Mvc;
using SwiftDrop.Helpers;
using SwiftDrop.Models.Enums;
using SwiftDrop.ViewModels;
using System.Security.Claims;

namespace SwiftDrop.Controllers
{
    public class BaseController : Controller
    {
        protected int ValidatePageNumber(int page, int defaultValue = 1)
        {
            return page < 1 ? defaultValue : page;
        }
   
        protected int? GetNumericUserId()
        {
            string? userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdStr == null) return null;
            return int.TryParse(userIdStr, out int numericUserId) ? numericUserId : null;
        }

        protected string? GetUsername()
        {
            return User.FindFirstValue(ClaimTypes.Name);

        }
        protected bool IsAdmin()
        {
            return User.Identity?.IsAuthenticated == true && User.IsInRole(Role.Admin.ToString());
        }

        protected bool isDelivery()
        {
            return User.Identity?.IsAuthenticated == true && User.IsInRole(Role.Delivery.ToString());
        }

         protected bool IsCustomer()
        {
            return User.Identity?.IsAuthenticated == true && User.IsInRole(Role.Customer.ToString());
        }

        protected IActionResult RedirectToHome(bool json, bool status = true)
        {
            if (json && status)
                return SuccessJson(null, Url.Action(AppConstants.HomePage.Action, AppConstants.HomePage.Controller));

            if (json)
                return FailureJson(null, Url.Action(AppConstants.HomePage.Action, AppConstants.HomePage.Controller), null);

            return RedirectToAction(AppConstants.HomePage.Action, AppConstants.HomePage.Controller);
        }

        protected IActionResult RedirectToLoginWithAlert(AlertMessageType type, string text, bool json, bool status = true)
        {
            if (json && status)
                return SuccessJson(null, Url.Action(AppConstants.LoginPage.Action, AppConstants.LoginPage.Controller, new { alertType = type, alertText = text }));

            else if (json)
                return FailureJson(null, Url.Action(AppConstants.LoginPage.Action, AppConstants.LoginPage.Controller, new { alertType = type, alertText = text }), null);

            return RedirectToAction(AppConstants.LoginPage.Action, AppConstants.LoginPage.Controller, new { alertType = type, alertText = text });
        }

        protected BadRequestObjectResult ValidationErrorJson(string message)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return FailureJson(message, null, errors);
        }

        protected BadRequestObjectResult FailureJson(string? message, string? returnUrl, IEnumerable<string>? errors = null) =>
          BadRequest(new JsonResponseViewModel(message, returnUrl, errors));

        protected OkObjectResult SuccessJson(string? message, string? returnUrl) =>
            Ok(new JsonResponseViewModel(message, returnUrl, null));

    }
}
