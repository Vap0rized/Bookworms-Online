using Bookworms_Online.Model;
using Bookworms_Online.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bookworms_Online.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _dbContext;

        [BindProperty]
        public ResetPasswordInput Input { get; set; } = new();

        public string? Message { get; set; }

        public ResetPasswordModel(UserManager<ApplicationUser> userManager, AuthDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public void OnGet()
        {
            Message = TempData["ResetMessage"]?.ToString();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = HttpContext.Session.GetString("ResetUserId");
            var token = HttpContext.Session.GetString("ResetToken");
            var code = HttpContext.Session.GetString("ResetCode");
            var expiresRaw = HttpContext.Session.GetString("ResetCodeExpires");

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(expiresRaw))
            {
                ModelState.AddModelError(string.Empty, "Reset session expired. Request a new code.");
                return Page();
            }

            if (!DateTimeOffset.TryParse(expiresRaw, out var expiresAt) || expiresAt < DateTimeOffset.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Reset code has expired. Request a new code.");
                return Page();
            }

            if (!string.Equals(Input.Code, code, StringComparison.Ordinal))
            {
                ModelState.AddModelError(string.Empty, "Invalid reset code.");
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid user.");
                return Page();
            }

            var result = await _userManager.ResetPasswordAsync(user, token, Input.NewPassword);
            if (result.Succeeded)
            {
                user.LastPasswordChangedAt = DateTimeOffset.UtcNow;
                await _userManager.UpdateAsync(user);

                await _dbContext.PasswordHistories.AddAsync(new PasswordHistory
                {
                    UserId = user.Id,
                    PasswordHash = user.PasswordHash ?? string.Empty,
                    ChangedAt = DateTimeOffset.UtcNow
                });

                await _dbContext.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    Action = "ResetPassword",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
                });

                await _dbContext.SaveChangesAsync();

                HttpContext.Session.Remove("ResetUserId");
                HttpContext.Session.Remove("ResetToken");
                HttpContext.Session.Remove("ResetCode");
                HttpContext.Session.Remove("ResetCodeExpires");

                return RedirectToPage("/Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
