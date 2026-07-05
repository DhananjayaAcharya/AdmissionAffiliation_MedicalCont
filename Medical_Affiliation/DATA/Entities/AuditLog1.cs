using System;

namespace Medical_Affiliation.DATA.Entities
{
    public class AuditLog1
    {
        public long AuditLogId { get; set; }

        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Module { get; set; }
        public string Action { get; set; } = string.Empty;

        public string LogType { get; set; } = "Audit";   // Audit | Error | Exception | Info
        public string Status { get; set; } = "Success";  // Success | Failure

        public string? TableName { get; set; }
        public string? RecordId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }

        public string? ExceptionType { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? StackTrace { get; set; }
        public string? Source { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }

        public string? Description { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}