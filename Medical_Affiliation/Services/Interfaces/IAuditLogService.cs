using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAuditAsync(
            string action,
            string? module,
            string? tableName,
            string? recordId,
            object? oldValues,
            object? newValues,
            string? description,
            bool success = true);

        Task LogExceptionAsync(
            Exception ex,
            string? module,
            string? source,
            HttpContext? httpContext);

        // Session logging (login/logout)
        Task LogSessionAsync(
            string action,
            string userId,
            string? userName,
            string sessionId,
            bool success,
            string? description,
            HttpContext? httpContext);

        // Database operation logging (Create, Update, Delete)
        Task LogDatabaseOperationAsync(
            string operation,
            string tableName,
            string recordId,
            object? oldValues,
            object? newValues,
            bool success,
            HttpContext? httpContext);

        // Custom event logging
        Task LogEventAsync(
            string eventType,
            string eventName,
            string? description,
            object? data,
            bool success,
            HttpContext? httpContext);
    }
}