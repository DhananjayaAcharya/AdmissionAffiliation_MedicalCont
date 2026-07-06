using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Medical_Affiliation.Middleware
{
    /// <summary>
    /// Middleware to log all HTTP requests and responses
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
        {
            var startTime = DateTime.UtcNow;

            // Log request details
            var userId = context.User?.Identity?.Name ?? "Anonymous";
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();

            try
            {
                // Call the next middleware/endpoint
                await _next(context);

                var duration = DateTime.UtcNow - startTime;
                var statusCode = context.Response.StatusCode;
                var success = statusCode >= 200 && statusCode < 300;

                // Log request completion
                _logger.LogInformation(
                    "HTTP Request - Method: {Method}, Path: {Path}, StatusCode: {StatusCode}, " +
                    "User: {User}, IP: {IP}, Duration: {Duration}ms",
                    requestMethod,
                    requestPath,
                    statusCode,
                    userId,
                    remoteIp,
                    duration.TotalMilliseconds);

                // Log to audit database (only for non-static content)
                if (ShouldLogPath(requestPath))
                {
                    await auditLogService.LogEventAsync(
                        eventType: "HTTP",
                        eventName: $"{requestMethod} {requestPath}",
                        description: $"HTTP {requestMethod} request to {requestPath} completed with status {statusCode}",
                        data: new
                        {
                            Method = requestMethod,
                            Path = requestPath,
                            StatusCode = statusCode,
                            Duration = duration.TotalMilliseconds,
                            UserAgent = context.Request.Headers["User-Agent"].ToString()
                        },
                        success: success,
                        httpContext: context);
                }
            }
            catch (Exception ex)
            {
                var duration = DateTime.UtcNow - startTime;

                _logger.LogError(
                    ex,
                    "HTTP Request failed - Method: {Method}, Path: {Path}, User: {User}, IP: {IP}, Duration: {Duration}ms",
                    requestMethod,
                    requestPath,
                    userId,
                    remoteIp,
                    duration.TotalMilliseconds);

                // Log the error
                if (ShouldLogPath(requestPath))
                {
                    await auditLogService.LogEventAsync(
                        eventType: "HTTP",
                        eventName: $"{requestMethod} {requestPath}",
                        description: $"HTTP {requestMethod} request to {requestPath} failed",
                        data: new { Error = ex.Message },
                        success: false,
                        httpContext: context);
                }

                throw;
            }
        }

        /// <summary>
        /// Determines if a request path should be logged
        /// </summary>
        private bool ShouldLogPath(PathString path)
        {
            var pathStr = path.Value?.ToLowerInvariant() ?? "";

            // Exclude static files and health checks
            var excludedPaths = new[] { "/favicon", "/static", "/css", "/js", "/img", "/images", "/lib", "/health", "/ping" };

            foreach (var excluded in excludedPaths)
            {
                if (pathStr.StartsWith(excluded))
                    return false;
            }

            return true;
        }
    }
}
