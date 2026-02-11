using Bookworms_Online.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bookworms_Online.Pages
{
    [Authorize]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AuthDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public LogoutModel(SignInManager<ApplicationUser> signInManager, AuthDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                user.CurrentSessionId = null;
                user.CurrentSessionIssuedAt = null;
                await _userManager.UpdateAsync(user);

                await _dbContext.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    Action = "Logout",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
                });
                await _dbContext.SaveChangesAsync();
            }

            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Login");
        }
    }
}
