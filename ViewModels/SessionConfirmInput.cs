using System.ComponentModel.DataAnnotations;

namespace Bookworms_Online.ViewModels
{
    public class SessionConfirmInput
    {
        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits.")]
        public string Code { get; set; } = string.Empty;
    }
}
