using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SwiftDrop.Repositories.Interfaces.Auth
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(150)]
        public string FullName { get; set; }


        [Required(ErrorMessage = "Username is required")]
        [Remote(action: "IsUserNameInUse", controller: "User", ErrorMessage = "This username is already taken, please choose another one.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Invalid username. Only English letters, numbers, and underscores (_) are allowed.")]
        [MaxLength(256)]
        public string UserName { get; set; }


        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [MaxLength(256)]
        [Remote(action: "IsEmailInUse", controller: "User", ErrorMessage = "This email is already registered, please choose another one.")]
        public string Email { get; set; }




        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z]).+$", ErrorMessage = "Password must contain at least one lowercase letter and one uppercase letter.")]
        [DataType(DataType.Password)]

        public string Password { get; set; }


        [Required(ErrorMessage = "Password confirmation is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        public IFormFile? ProfilePicture { get; set; }//must be at most 3 MB in size and must be an image file of this formate ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".tiff", ".oip" ,".svg"

        [MaxLength(500)]
        public string? Address { get; set; }

        [RegularExpression(@"^(\+|00)?[0-9]{7,15}$", ErrorMessage = "Invalid phone number format")]
        [StringLength(17, MinimumLength = 7, ErrorMessage = "Phone number must be between 7 and 17 characters")]
        [Phone]
        public string? PhoneNumber { get; set; }

    }
}
