using System.Security.Cryptography;
using Bookworms_Online.Model;
using Bookworms_Online.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bookworms_Online.Pages
{
    public class SessionConfirmModel : PageModel
    {
        private const int CodeExpiryMinutes = 5;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        [BindProperty]
        public SessionConfirmInput Input { get; set; } = new();

        public string? Message { get; private set; }

        public SessionConfirmModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public void OnGet()
        {
            Message = TempData["SessionConfirmMessage"]?.ToString();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = HttpContext.Session.GetString("PendingSessionUserId");
            var code = HttpContext.Session.GetString("PendingSessionCode");
            var expiresRaw = HttpContext.Session.GetString("PendingSessionExpires");

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(expiresRaw))
            {
                ModelState.AddModelError(string.Empty, "Session confirmation expired. Please login again.");
                return Page();
            }

            if (!DateTimeOffset.TryParse(expiresRaw, out var expiresAt) || expiresAt < DateTimeOffset.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Confirmation code has expired. Please login again.");
                return Page();
            }

            if (!string.Equals(Input.Code, code, StringComparison.Ordinal))
            {
                ModelState.AddModelError(string.Empty, "Invalid confirmation code.");
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid user.");
                return Page();
            }

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

            HttpContext.Session.Remove("PendingSessionUserId");
            HttpContext.Session.Remove("PendingSessionCode");
            HttpContext.Session.Remove("PendingSessionExpires");

            return RedirectToPage("/Home");
        }

        public static string GenerateCode()
        {
            var bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            var value = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
            return value.ToString("D6");
        }

        public static DateTimeOffset GetExpiry() => DateTimeOffset.UtcNow.AddMinutes(CodeExpiryMinutes);
    }
}
