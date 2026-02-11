using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bookworms_Online.Model
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IConfiguration _configuration;

        public AuthDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = _configuration.GetConnectionString("AuthConnectionString");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}
