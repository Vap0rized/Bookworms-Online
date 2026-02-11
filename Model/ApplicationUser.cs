using Microsoft.AspNetCore.Identity;

namespace Bookworms_Online.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string BillingAddressEncrypted { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string CreditCardEncrypted { get; set; } = string.Empty;
        public string? PhotoPath { get; set; }
        public string? CurrentSessionId { get; set; }
        public DateTimeOffset? CurrentSessionIssuedAt { get; set; }
        public DateTimeOffset? LastPasswordChangedAt { get; set; }
    }
}
