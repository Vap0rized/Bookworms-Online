using Bookworms_Online.Model;
using Bookworms_Online.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;

namespace Bookworms_Online.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserDataProtectionService _protection;

        public string? FullName { get; private set; }
        public string? Email { get; private set; }
        public string? MobileNo { get; private set; }
        public string? BillingAddress { get; private set; }
        public string? ShippingAddress { get; private set; }
        public string? CreditCardNo { get; private set; }
        public string? PhotoPath { get; private set; }
        public string? BillingAddressEncrypted { get; private set; }
        public string? CreditCardEncrypted { get; private set; }

        public IndexModel(UserManager<ApplicationUser> userManager, UserDataProtectionService protection)
        {
            _userManager = userManager;
            _protection = protection;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Login");
            }

            FullName = $"{WebUtility.HtmlDecode(user.FirstName)} {WebUtility.HtmlDecode(user.LastName)}";
            Email = user.Email;
            MobileNo = WebUtility.HtmlDecode(user.MobileNo);
            BillingAddressEncrypted = user.BillingAddressEncrypted;
            CreditCardEncrypted = user.CreditCardEncrypted;
            BillingAddress = WebUtility.HtmlDecode(_protection.Unprotect(user.BillingAddressEncrypted));
            ShippingAddress = WebUtility.HtmlDecode(user.ShippingAddress);
            CreditCardNo = _protection.Unprotect(user.CreditCardEncrypted);
            PhotoPath = user.PhotoPath;

            return Page();
        }
    }
}
