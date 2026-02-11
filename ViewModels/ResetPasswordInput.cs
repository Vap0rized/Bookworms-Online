using System.ComponentModel.DataAnnotations;

namespace Bookworms_Online.ViewModels
{
    public class ResetPasswordInput
    {
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits.")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 12)]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).+$", ErrorMessage = "Password must include lower, upper, number, and special character.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Password and confirmation password does not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
