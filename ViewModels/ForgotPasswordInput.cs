using System.ComponentModel.DataAnnotations;

namespace Bookworms_Online.ViewModels
{
    public class ForgotPasswordInput
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;
    }
}
