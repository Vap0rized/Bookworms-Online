using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace Bookworms_Online.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public int? StatusCode { get; set; }
        public string? Message { get; set; }

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(int? statusCode = null, string? message = null)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            StatusCode = statusCode;
            Message = !string.IsNullOrWhiteSpace(message)
                ? message
                : statusCode switch
                {
                    401 => "You must log in to access this page.",
                    403 => "Access denied.",
                    404 => "Page not found.",
                    _ => "An error occurred while processing your request."
                };
        }
    }
}
