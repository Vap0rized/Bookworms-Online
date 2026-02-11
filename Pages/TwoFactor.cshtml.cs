using Bookworms_Online.Model;
using Bookworms_Online.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bookworms_Online.Pages
{
    public class TwoFactorModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty]
        public TwoFactorInput Input { get; set; } = new();

        public string? DemoCode => TempData["TwoFactorCode"]?.ToString();

        public TwoFactorModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
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

            var result = await _signInManager.TwoFactorSignInAsync(TokenOptions.DefaultEmailProvider, Input.Code, false, false);
            if (result.Succeeded)
            {
                var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                if (user != null)
                {
                    var sessionId = Guid.NewGuid().ToString("N");
                    var issuedAt = DateTimeOffset.UtcNow;
                    user.CurrentSessionId = sessionId;
                    user.CurrentSessionIssuedAt = issuedAt;
                    await _userManager.UpdateAsync(user);
                    await _signInManager.SignInWithClaimsAsync(user, false, new[]
                    {
                        new System.Security.Claims.Claim("session_id", sessionId),
                        new System.Security.Claims.Claim("session_issued", issuedAt.ToUnixTimeSeconds().ToString())
                    });
                }

                return RedirectToPage("/Index");
            }

            ModelState.AddModelError(string.Empty, "Invalid 2FA code.");
            return Page();
        }
    }
}
