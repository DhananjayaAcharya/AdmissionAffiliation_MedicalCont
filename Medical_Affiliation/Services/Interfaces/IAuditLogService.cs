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
    }
}