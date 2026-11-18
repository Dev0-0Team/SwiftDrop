using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftDrop.Services.Interfaces;

namespace SwiftDrop.Controllers
{
    [Route("users")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;

        }

        [AcceptVerbs("GET", "POST")]
        [Route("is-username-in-use")]
        [AllowAnonymous]
        public async Task<IActionResult> IsUserNameInUse(string userName, int userId = 0)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return Json(true);

            bool isExist = await _userService.IsUserNameExistAsync(userName, userId > 0 ? userId : -1);

            return Json(!isExist);
        }

        [AcceptVerbs("GET", "POST")]
        [Route("is-email-in-use")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInUse(string email, int userId = 0)
        {


            if (string.IsNullOrWhiteSpace(email))
                return Json(true);

            bool isExist = await _userService.IsEmailExistAsync(email, userId > 0 ? userId : -1);

            return Json(!isExist);
        }

    }
}
