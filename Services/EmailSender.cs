using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace Bookworms_Online.Services
{
    public class EmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendPasswordResetAsync(string toEmail, string resetCode)
        {
            var host = _configuration["Smtp:Host"];
            var portValue = _configuration["Smtp:Port"];
            var user = _configuration["Smtp:User"];
            var password = _configuration["Smtp:Password"];
            var from = _configuration["Smtp:From"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            {
                _logger.LogWarning("SMTP host/from not configured.");
                return false;
            }

            if (!MailAddress.TryCreate(from, out var fromAddress))
            {
                _logger.LogWarning("SMTP from address invalid: {From}", from);
                return false;
            }

            if (!MailAddress.TryCreate(toEmail, out var toAddress))
            {
                _logger.LogWarning("SMTP to address invalid.");
                return false;
            }

            var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 587;
            var safeCode = WebUtility.HtmlEncode(resetCode);
            var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = "Password Reset Code",
                Body = $@"<h2>Password Reset</h2>
<p>Use the verification code below to reset your password:</p>
<h3 style=""letter-spacing:2px;"">{safeCode}</h3>
<p>This code expires soon. If you did not request this, you can ignore this email.</p>",
                IsBodyHtml = true
            };

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password))
            {
                client.Credentials = new NetworkCredential(user, password);
            }

            try
            {
                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP send failed for password reset.");
                return false;
            }
        }

        public async Task<bool> SendSessionConfirmationAsync(string toEmail, string code)
        {
            var host = _configuration["Smtp:Host"];
            var portValue = _configuration["Smtp:Port"];
            var user = _configuration["Smtp:User"];
            var password = _configuration["Smtp:Password"];
            var from = _configuration["Smtp:From"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
            {
                _logger.LogWarning("SMTP host/from not configured.");
                return false;
            }

            if (!MailAddress.TryCreate(from, out var fromAddress))
            {
                _logger.LogWarning("SMTP from address invalid: {From}", from);
                return false;
            }

            if (!MailAddress.TryCreate(toEmail, out var toAddress))
            {
                _logger.LogWarning("SMTP to address invalid.");
                return false;
            }

            var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 587;
            var safeCode = WebUtility.HtmlEncode(code);
            var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = "Confirm New Login",
                Body = $@"<h2>Confirm New Login</h2>
<p>Use the code below to confirm the new login:</p>
<h3 style=""letter-spacing:2px;"">{safeCode}</h3>
<p>If you did not attempt to log in, ignore this email.</p>",
                IsBodyHtml = true
            };

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password))
            {
                client.Credentials = new NetworkCredential(user, password);
            }

            try
            {
                await client.SendMailAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP send failed for session confirmation.");
                return false;
            }
        }
    }
}
