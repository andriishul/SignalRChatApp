using System.ComponentModel.DataAnnotations;

namespace SignalRChatApp.Models
{
    public class SignInViewModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
           ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character.")]
        public string? Password { get; set; }
    }
}
