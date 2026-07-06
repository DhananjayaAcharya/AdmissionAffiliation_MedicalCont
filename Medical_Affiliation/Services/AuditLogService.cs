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

        // Session logging (login/logout)
        public async Task LogSessionAsync(
            string action,
            string userId,
            string? userName,
            string sessionId,
            bool success,
            string? description,
            HttpContext? httpContext)
        {
            var log = new AuditLog1
            {
                UserId = userId,
                UserName = userName,
                Module = "Authentication",
                Action = action,
                LogType = "Session",
                Status = success ? "Success" : "Failure",
                Description = description,
                Source = $"Session: {sessionId}",
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
                _logger.LogInformation("Session audit logged - Action: {Action}, User: {User}", action, userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save session audit log. Action: {Action}, User: {User}", action, userName);
            }
        }

        // Database operation logging
        public async Task LogDatabaseOperationAsync(
            string operation,
            string tableName,
            string recordId,
            object? oldValues,
            object? newValues,
            bool success,
            HttpContext? httpContext)
        {
            var log = new AuditLog1
            {
                UserId = httpContext?.User?.FindFirst("CollegeCode")?.Value
                         ?? httpContext?.User?.Identity?.Name,
                UserName = httpContext?.User?.Identity?.Name,
                Module = "Database",
                Action = operation,
                LogType = "DataModification",
                Status = success ? "Success" : "Failure",
                TableName = tableName,
                RecordId = recordId,
                OldValues = oldValues is null ? null : SafeSerialize(oldValues),
                NewValues = newValues is null ? null : SafeSerialize(newValues),
                Description = $"{operation} operation on table {tableName} (Record: {recordId})",
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save database operation audit log. Table: {Table}, Operation: {Operation}", tableName, operation);
            }
        }

        // Custom event logging
        public async Task LogEventAsync(
            string eventType,
            string eventName,
            string? description,
            object? data,
            bool success,
            HttpContext? httpContext)
        {
            var log = new AuditLog1
            {
                UserId = httpContext?.User?.FindFirst("CollegeCode")?.Value
                         ?? httpContext?.User?.Identity?.Name,
                UserName = httpContext?.User?.Identity?.Name,
                Module = eventType,
                Action = eventName,
                LogType = "Event",
                Status = success ? "Success" : "Failure",
                Description = description,
                OldValues = data is null ? null : SafeSerialize(data),
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save event audit log. Event: {EventType}/{EventName}", eventType, eventName);
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