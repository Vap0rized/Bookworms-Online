using Bookworms_Online.Model;
using Bookworms_Online.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bookworms_Online.Model;

namespace Bookworms_Online.Pages
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AuthDbContext _dbContext;

        [BindProperty]
        public ChangePasswordInput Input { get; set; } = new();

        public ChangePasswordModel(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AuthDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
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

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Login");
            }

            if (user.LastPasswordChangedAt.HasValue && user.LastPasswordChangedAt.Value.AddMinutes(1) > DateTimeOffset.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Password was changed recently. Please wait before changing again.");
                return Page();
            }

            var recentHashes = _dbContext.PasswordHistories
                .Where(h => h.UserId == user.Id)
                .OrderByDescending(h => h.ChangedAt)
                .Take(2)
                .Select(h => h.PasswordHash)
                .ToList();

            var hasher = _userManager.PasswordHasher;
            if (recentHashes.Any(hash => hasher.VerifyHashedPassword(user, hash, Input.NewPassword) == PasswordVerificationResult.Success))
            {
                ModelState.AddModelError(string.Empty, "New password cannot match the last 2 passwords.");
                return Page();
            }

            var result = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
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
                    Action = "ChangePassword",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
                });
                await _dbContext.SaveChangesAsync();

                await _signInManager.RefreshSignInAsync(user);
                return RedirectToPage("/Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
