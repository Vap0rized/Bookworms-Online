using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Bookworms_Online.ViewModels
{
    public class Register
    {
        [Required]
        [StringLength(50)]
        [RegularExpression("^[A-Za-z]+(?: [A-Za-z]+)*$", ErrorMessage = "First name must contain letters only.")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [RegularExpression("^[A-Za-z]+(?: [A-Za-z]+)*$", ErrorMessage = "Last name must contain letters only.")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "Mobile number must be 8 digits.")]
        [RegularExpression("^\\d{8}$", ErrorMessage = "Mobile number must be 8 digits.")]
        public string MobileNo { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [RegularExpression("^[^<>]*$", ErrorMessage = "Angle brackets are not allowed.")]
        public string BillingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Credit card number must be 16 digits.")]
        [RegularExpression("^\\d{16}$", ErrorMessage = "Credit card number must be 16 digits.")]
        public string CreditCardNo { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [RegularExpression("^[^<>]*$", ErrorMessage = "Angle brackets are not allowed.")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 12)]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).+$", ErrorMessage = "Password must include lower, upper, number, and special character.")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Password and confirmation password does not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public IFormFile? Photo { get; set; }

        public string? RecaptchaToken { get; set; }
    }
}
