using System.Security.Claims;
using System.Security.Cryptography;
using Bookworms_Online.Model;
using Bookworms_Online.Services;
using Bookworms_Online.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bookworms_Online.Pages
{
    public class LoginModel : PageModel
    {
        private const int ActiveSessionMinutes = 1;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _dbContext;
        private readonly RecaptchaService _recaptcha;
        private readonly IConfiguration _configuration;
        private readonly EmailSender _emailSender;

        [BindProperty]
        public LoginInput Input { get; set; } = new();

        public string RecaptchaSiteKey { get; }

        public LoginModel(SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            AuthDbContext dbContext,
            RecaptchaService recaptcha,
            IConfiguration configuration,
            EmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _dbContext = dbContext;
            _recaptcha = recaptcha;
            _configuration = configuration;
            _emailSender = emailSender;
            RecaptchaSiteKey = configuration["Recaptcha:SiteKey"] ?? string.Empty;
        }

        public void OnGet(string? reason = null)
        {
            if (!string.IsNullOrWhiteSpace(reason))
            {
                var message = reason == "session_end"
                    ? "Your session was ended due to a newer login."
                    : "Session expired or logged in from another device.";
                ModelState.AddModelError(string.Empty, message);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!await _recaptcha.VerifyAsync(Input.RecaptchaToken))
            {
                ModelState.AddModelError(string.Empty, "Captcha verification failed.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            var hasActiveSession = user.CurrentSessionIssuedAt.HasValue
                && user.CurrentSessionIssuedAt.Value.AddMinutes(ActiveSessionMinutes) > DateTimeOffset.UtcNow;

            if (hasActiveSession)
            {
                var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: true);
                if (passwordCheck.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Account locked. Try again later.");
                    return Page();
                }

                if (!passwordCheck.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }

                var code = SessionConfirmModel.GenerateCode();
                var expiresAt = SessionConfirmModel.GetExpiry();

                HttpContext.Session.SetString("PendingSessionUserId", user.Id);
                HttpContext.Session.SetString("PendingSessionCode", code);
                HttpContext.Session.SetString("PendingSessionExpires", expiresAt.ToString("O"));

                var overrideEmail = _configuration["Smtp:ToOverride"];
                var recipient = string.IsNullOrWhiteSpace(overrideEmail) ? Input.Email : overrideEmail;
                var sent = await _emailSender.SendSessionConfirmationAsync(recipient, code);
                if (!sent)
                {
                    ModelState.AddModelError(string.Empty, "Unable to send confirmation email. Check SMTP settings.");
                    return Page();
                }

                TempData["SessionConfirmMessage"] = "A confirmation code was sent to your email. Enter it to finish logging in.";
                return RedirectToPage("/SessionConfirm");
            }

            var result = await _signInManager.PasswordSignInAsync(user, Input.Password, false, lockoutOnFailure: true);
            if (result.RequiresTwoFactor)
            {
                var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultEmailProvider);
                TempData["TwoFactorCode"] = token;
                return RedirectToPage("/TwoFactor");
            }

            if (result.Succeeded)
            {
                var sessionId = Guid.NewGuid().ToString("N");
                var issuedAt = DateTimeOffset.UtcNow;
                user.CurrentSessionId = sessionId;
                user.CurrentSessionIssuedAt = issuedAt;
                await _userManager.UpdateAsync(user);

                HttpContext.Session.SetString("UserId", user.Id);

                await _dbContext.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    Action = "Login",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
                });
                await _dbContext.SaveChangesAsync();

                await _signInManager.SignInWithClaimsAsync(user, false, new[]
                {
                    new Claim("session_id", sessionId),
                    new Claim("session_issued", issuedAt.ToUnixTimeSeconds().ToString())
                });

                if (user.LastPasswordChangedAt.HasValue && user.LastPasswordChangedAt.Value.AddMinutes(999999) < DateTimeOffset.UtcNow)
                {
                    return RedirectToPage("/ChangePassword");
                }

                return RedirectToPage("/Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account locked. Try again later.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
}
