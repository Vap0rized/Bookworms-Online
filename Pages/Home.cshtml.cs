using Bookworms_Online.Model;
using Bookworms_Online.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bookworms_Online.Pages
{
    [Authorize]
    public class HomeModel : PageModel
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

        public HomeModel(UserManager<ApplicationUser> userManager, UserDataProtectionService protection)
        {
            _userManager = userManager;
            _protection = protection;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return;
            }

            FullName = $"{System.Net.WebUtility.HtmlDecode(user.FirstName)} {System.Net.WebUtility.HtmlDecode(user.LastName)}";
            Email = user.Email;
            MobileNo = System.Net.WebUtility.HtmlDecode(user.MobileNo);
            BillingAddressEncrypted = user.BillingAddressEncrypted;
            CreditCardEncrypted = user.CreditCardEncrypted;
            BillingAddress = System.Net.WebUtility.HtmlDecode(_protection.Unprotect(user.BillingAddressEncrypted));
            ShippingAddress = System.Net.WebUtility.HtmlDecode(user.ShippingAddress);
            CreditCardNo = _protection.Unprotect(user.CreditCardEncrypted);
            PhotoPath = user.PhotoPath;
        }
    }
}
