using System.ComponentModel.DataAnnotations;

namespace Bookworms_Online.ViewModels
{
    public class TwoFactorInput
    {
        [Required]
        [StringLength(10, MinimumLength = 4)]
        public string Code { get; set; } = string.Empty;
    }
}
