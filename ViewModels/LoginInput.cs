using System.ComponentModel.DataAnnotations;

namespace Bookworms_Online.ViewModels
{
    public class LoginInput
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression("^[^<>]*$", ErrorMessage = "Angle brackets are not allowed.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression("^[^<>]*$", ErrorMessage = "Angle brackets are not allowed.")]
        public string Password { get; set; } = string.Empty;

        public string? RecaptchaToken { get; set; }
    }
}
