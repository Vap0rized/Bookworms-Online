using System.Security.Cryptography;
using Bookworms_Online.Model;
using Bookworms_Online.Services;
using Bookworms_Online.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bookworms_Online.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private const int ResetCodeExpiryMinutes = 5;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailSender _emailSender;
        private readonly IConfiguration _configuration;

        [BindProperty]
        public ForgotPasswordInput Input { get; set; } = new();

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, EmailSender emailSender, IConfiguration configuration)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var code = GenerateResetCode();
                var expiresAt = DateTimeOffset.UtcNow.AddMinutes(ResetCodeExpiryMinutes);

                HttpContext.Session.SetString("ResetUserId", user.Id);
                HttpContext.Session.SetString("ResetToken", token);
                HttpContext.Session.SetString("ResetCode", code);
                HttpContext.Session.SetString("ResetCodeExpires", expiresAt.ToString("O"));

                var overrideEmail = _configuration["Smtp:ToOverride"];
                var recipient = string.IsNullOrWhiteSpace(overrideEmail) ? Input.Email : overrideEmail;
                var sent = await _emailSender.SendPasswordResetAsync(recipient, code);
                if (!sent)
                {
                    ModelState.AddModelError(string.Empty, "Email settings are invalid. Update SMTP settings and try again.");
                    return Page();
                }
            }

            TempData["ResetMessage"] = "A verification code has been sent to your email.";
            return RedirectToPage("/ResetPassword");
        }

        private static string GenerateResetCode()
        {
            var bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            var value = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
            return value.ToString("D6");
        }
    }
}
