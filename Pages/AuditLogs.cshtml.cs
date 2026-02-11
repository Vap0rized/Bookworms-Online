using Bookworms_Online.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Bookworms_Online.Pages
{
    [Authorize]
    public class AuditLogsModel : PageModel
    {
        private readonly AuthDbContext _dbContext;

        public List<AuditLog> Logs { get; private set; } = new();

        public AuditLogsModel(AuthDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task OnGetAsync()
        {
            Logs = await _dbContext.AuditLogs
                .OrderByDescending(log => log.Timestamp)
                .ToListAsync();
        }
    }
}
