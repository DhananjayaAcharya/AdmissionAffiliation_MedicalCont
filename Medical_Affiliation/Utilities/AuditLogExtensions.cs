using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Text.Json;

namespace Medical_Affiliation.Utilities
{
    /// <summary>
    /// Extension methods for audit logging
    /// </summary>
    public static class AuditLogExtensions
    {
        /// <summary>
        /// Serialize an object to a dictionary for audit logging
        /// </summary>
        public static Dictionary<string, object> ToDictionary(this object obj)
        {
            if (obj == null) return new Dictionary<string, object>();

            var properties = new Dictionary<string, object>();
            var type = obj.GetType();

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    var value = prop.GetValue(obj);
                    properties[prop.Name] = value ?? "null";
                }
                catch
                {
                    properties[prop.Name] = "error reading value";
                }
            }

            return properties;
        }

        /// <summary>
        /// Log a Create operation
        /// </summary>
        public static async Task LogCreateAsync(
            this IAuditLogService auditLogService,
            string tableName,
            string recordId,
            object newValues,
            HttpContext httpContext,
            bool success = true)
        {
            await auditLogService.LogDatabaseOperationAsync(
                operation: "CREATE",
                tableName: tableName,
                recordId: recordId,
                oldValues: null,
                newValues: newValues,
                success: success,
                httpContext: httpContext);
        }

        /// <summary>
        /// Log an Update operation
        /// </summary>
        public static async Task LogUpdateAsync(
            this IAuditLogService auditLogService,
            string tableName,
            string recordId,
            object oldValues,
            object newValues,
            HttpContext httpContext,
            bool success = true)
        {
            await auditLogService.LogDatabaseOperationAsync(
                operation: "UPDATE",
                tableName: tableName,
                recordId: recordId,
                oldValues: oldValues,
                newValues: newValues,
                success: success,
                httpContext: httpContext);
        }

        /// <summary>
        /// Log a Delete operation
        /// </summary>
        public static async Task LogDeleteAsync(
            this IAuditLogService auditLogService,
            string tableName,
            string recordId,
            object oldValues,
            HttpContext httpContext,
            bool success = true)
        {
            await auditLogService.LogDatabaseOperationAsync(
                operation: "DELETE",
                tableName: tableName,
                recordId: recordId,
                oldValues: oldValues,
                newValues: null,
                success: success,
                httpContext: httpContext);
        }

        /// <summary>
        /// Log a file operation
        /// </summary>
        public static async Task LogFileOperationAsync(
            this IAuditLogService auditLogService,
            string operation,
            string fileName,
            long fileSizeBytes,
            string? description,
            HttpContext httpContext,
            bool success = true)
        {
            await auditLogService.LogEventAsync(
                eventType: "File",
                eventName: $"{operation} - {fileName}",
                description: description ?? $"File {operation}: {fileName} ({FormatBytes(fileSizeBytes)})",
                data: new { FileName = fileName, FileSizeBytes = fileSizeBytes, Operation = operation },
                success: success,
                httpContext: httpContext);
        }

        /// <summary>
        /// Log a report generation event
        /// </summary>
        public static async Task LogReportAsync(
            this IAuditLogService auditLogService,
            string reportName,
            string? filter,
            string? format,
            HttpContext httpContext,
            bool success = true)
        {
            await auditLogService.LogEventAsync(
                eventType: "Report",
                eventName: reportName,
                description: $"Report generated: {reportName} (Format: {format ?? "PDF"})",
                data: new { ReportName = reportName, Filter = filter, Format = format },
                success: success,
                httpContext: httpContext);
        }

        /// <summary>
        /// Log a payment transaction
        /// </summary>
        public static async Task LogPaymentAsync(
            this IAuditLogService auditLogService,
            string transactionId,
            decimal amount,
            string status,
            string? paymentMethod,
            string? description,
            HttpContext httpContext)
        {
            await auditLogService.LogEventAsync(
                eventType: "Payment",
                eventName: $"Payment - {transactionId}",
                description: description ?? $"Payment {status}: {amount}",
                data: new { TransactionId = transactionId, Amount = amount, Status = status, PaymentMethod = paymentMethod },
                success: status.Equals("Success", StringComparison.OrdinalIgnoreCase),
                httpContext: httpContext);
        }

        /// <summary>
        /// Log an approval/rejection action
        /// </summary>
        public static async Task LogApprovalAsync(
            this IAuditLogService auditLogService,
            string action,
            string recordType,
            string recordId,
            string? reason,
            HttpContext httpContext,
            bool success = true)
        {
            await auditLogService.LogEventAsync(
                eventType: "Approval",
                eventName: $"{action} - {recordType}",
                description: $"{action} on {recordType} ({recordId}). Reason: {reason ?? "N/A"}",
                data: new { RecordType = recordType, RecordId = recordId, Action = action, Reason = reason },
                success: success,
                httpContext: httpContext);
        }

        /// <summary>
        /// Format bytes to human-readable format
        /// </summary>
        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
