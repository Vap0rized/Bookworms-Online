using Microsoft.AspNetCore.DataProtection;

namespace Bookworms_Online.Services
{
    public class UserDataProtectionService
    {
        private readonly IDataProtector _protector;

        public UserDataProtectionService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("UserDataProtection");
        }

        public string Protect(string input) => _protector.Protect(input);

        public string Unprotect(string input) => _protector.Unprotect(input);
    }
}
