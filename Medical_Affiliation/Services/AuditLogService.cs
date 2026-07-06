using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Medical_Affiliation.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuditLogService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task LogAuditAsync(
            string action,
            string? module,
            string? tableName,
            string? recordId,
            object? oldValues,
            object? newValues,
            string? description,
            bool success = true)
        {
            var httpCtx = _httpContextAccessor.HttpContext;

            var log = new AuditLog1
            {
                UserId = httpCtx?.User?.FindFirst("CollegeCode")?.Value
                         ?? httpCtx?.User?.Identity?.Name,
                UserName = httpCtx?.User?.Identity?.Name,
                Module = module,
                Action = action,
                LogType = "Audit",
                Status = success ? "Success" : "Failure",
                TableName = tableName,
                RecordId = recordId,
                OldValues = oldValues is null ? null : SafeSerialize(oldValues),
                NewValues = newValues is null ? null : SafeSerialize(newValues),
                Description = description,
                Ipaddress = httpCtx?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpCtx?.Request?.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _context.AuditLogs1.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to save audit log. Action: {Action}, Module: {Module}, Table: {TableName}",
                    action,
                    module,
                    tableName);
            }
        }

        public async Task LogExceptionAsync(
            Exception ex,
            string? module,
            string? source,
            HttpContext? httpContext)
        {
            var log = new AuditLog1
            {
                UserId = httpContext?.User?.FindFirst("CollegeCode")?.Value
                         ?? httpContext?.User?.Identity?.Name,
                UserName = httpContext?.User?.Identity?.Name,
                Module = module,
                Action = "UnhandledException",
                LogType = "Exception",
                Status = "Failure",
                ExceptionType = ex.GetType().FullName,
                ExceptionMessage = ex.Message,
                StackTrace = ex.ToString(),
                Source = source,
                RequestPath = httpContext?.Request?.Path,
                RequestMethod = httpContext?.Request?.Method,
                Ipaddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _context.AuditLogs1.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx,
                    "Failed to save exception audit log. Module: {Module}, Source: {Source}",
                    module,
                    source);
            }
        }

        private static string SafeSerialize(object value)
        {
            try
            {
                return JsonSerializer.Serialize(value);
            }
            catch
            {
                return value?.ToString() ?? string.Empty;
            }
        }
    }
}