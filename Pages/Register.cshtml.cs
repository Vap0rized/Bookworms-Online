using System.Net;
using System.Security.Claims;
using Bookworms_Online.Model;
using Bookworms_Online.Services;
using Bookworms_Online.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bookworms_Online.Model;

namespace Bookworms_Online.Pages
{
    public class RegisterModel : PageModel
    {
        private const long MaxPhotoSize = 2 * 1024 * 1024;

        private UserManager<ApplicationUser> userManager { get; }
        private SignInManager<ApplicationUser> signInManager { get; }
        private readonly AuthDbContext _dbContext;
        private readonly UserDataProtectionService _protection;
        private readonly RecaptchaService _recaptcha;
        private readonly IWebHostEnvironment _environment;

        [BindProperty]
        public Register RModel { get; set; } = new();

        public string RecaptchaSiteKey { get; }

        public RegisterModel(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            AuthDbContext dbContext,
            UserDataProtectionService protection,
            RecaptchaService recaptcha,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            _dbContext = dbContext;
            _protection = protection;
            _recaptcha = recaptcha;
            _environment = environment;
            RecaptchaSiteKey = configuration["Recaptcha:SiteKey"] ?? string.Empty;
        }

        public void OnGet()
        {
        }

        //Save data into the database
        public async Task<IActionResult> OnPostAsync()
        {
            if (!await _recaptcha.VerifyAsync(RModel.RecaptchaToken))
            {
                ModelState.AddModelError(string.Empty, "Captcha verification failed.");
            }

            var existingUser = await userManager.FindByEmailAsync(RModel.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(RModel.Email), "Email is already registered.");
            }

            if (RModel.Photo == null || RModel.Photo.Length == 0)
            {
                ModelState.AddModelError(nameof(RModel.Photo), "Photo is required.");
            }
            else if (RModel.Photo.Length > MaxPhotoSize)
            {
                ModelState.AddModelError(nameof(RModel.Photo), "Photo size must be less than 2MB.");
            }
            else if (!Path.GetExtension(RModel.Photo.FileName).Equals(".jpg", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(RModel.Photo), "Only .JPG files are allowed.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = RModel.Email,
                Email = RModel.Email,
                FirstName = WebUtility.HtmlEncode(RModel.FirstName.Trim()),
                LastName = WebUtility.HtmlEncode(RModel.LastName.Trim()),
                MobileNo = WebUtility.HtmlEncode(RModel.MobileNo.Trim()),
                BillingAddressEncrypted = _protection.Protect(WebUtility.HtmlEncode(RModel.BillingAddress.Trim())),
                ShippingAddress = WebUtility.HtmlEncode(RModel.ShippingAddress.Trim()),
                CreditCardEncrypted = _protection.Protect(RModel.CreditCardNo.Trim()),
                TwoFactorEnabled = true,
                LastPasswordChangedAt = DateTimeOffset.UtcNow
            };

            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsRoot);
            var fileName = $"{Guid.NewGuid():N}.jpg";
            var filePath = Path.Combine(uploadsRoot, fileName);
            await using (var stream = System.IO.File.Create(filePath))
            {
                await RModel.Photo.CopyToAsync(stream);
            }
            user.PhotoPath = $"/uploads/{fileName}";

            var result = await userManager.CreateAsync(user, RModel.Password);
            if (result.Succeeded)
            {
                await _dbContext.PasswordHistories.AddAsync(new PasswordHistory
                {
                    UserId = user.Id,
                    PasswordHash = user.PasswordHash ?? string.Empty,
                    ChangedAt = DateTimeOffset.UtcNow
                });

                await _dbContext.AuditLogs.AddAsync(new AuditLog
                {
                    UserId = user.Id,
                    Action = "Registration",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers.UserAgent.ToString()
                });
                await _dbContext.SaveChangesAsync();

                var sessionId = Guid.NewGuid().ToString("N");
                var issuedAt = DateTimeOffset.UtcNow;
                user.CurrentSessionId = sessionId;
                user.CurrentSessionIssuedAt = issuedAt;
                await userManager.UpdateAsync(user);

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
