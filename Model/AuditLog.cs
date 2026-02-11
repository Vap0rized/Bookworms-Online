namespace Bookworms_Online.Model
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
