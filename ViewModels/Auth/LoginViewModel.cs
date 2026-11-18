using SwiftDrop.Helpers;
using System.ComponentModel.DataAnnotations;

namespace SwiftDrop.Repositories.Interfaces.Auth
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username or email is required")]
        public string EmailOrUserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public AlertMessage? Alert { get; set; }
    }
}
