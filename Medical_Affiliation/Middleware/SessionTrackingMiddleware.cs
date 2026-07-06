using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Medical_Affiliation.Middleware
{
    /// <summary>
    /// Middleware to track user sessions (login, logout, activity)
    /// </summary>
    public class SessionTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionTrackingMiddleware> _logger;
        private const string UserIdKey = "AuditUserId";
        private const string SessionStartTimeKey = "SessionStartTime";
        private const string LastActivityKey = "LastActivity";

        public SessionTrackingMiddleware(RequestDelegate next, ILogger<SessionTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
        {
            var currentUserId = context.User?.Identity?.Name;
            var sessionId = context.Session?.Id ?? "Unknown";

            // Check if user is authenticated
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                // Track user login (first request after authentication)
                if (!context.Session.Keys.Contains(UserIdKey))
                {
                    context.Session.SetString(UserIdKey, currentUserId ?? "Unknown");
                    context.Session.SetString(SessionStartTimeKey, DateTime.UtcNow.ToString("O"));

                    await auditLogService.LogSessionAsync(
                        action: "Login",
                        userId: currentUserId ?? "Unknown",
                        userName: currentUserId,
                        sessionId: sessionId,
                        success: true,
                        description: $"User logged in. Session ID: {sessionId}",
                        httpContext: context);

                    _logger.LogInformation("User login tracked - User: {User}, Session: {Session}", currentUserId, sessionId);
                }

                // Update last activity timestamp
                context.Session.SetString(LastActivityKey, DateTime.UtcNow.ToString("O"));
            }

            // Continue with next middleware
            await _next(context);

            // Track user logout if session ended
            if (context.User?.Identity?.IsAuthenticated == false && context.Session.Keys.Contains(UserIdKey))
            {
                var userId = context.Session.GetString(UserIdKey);
                
                await auditLogService.LogSessionAsync(
                    action: "Logout",
                    userId: userId ?? "Unknown",
                    userName: userId,
                    sessionId: sessionId,
                    success: true,
                    description: $"User logged out. Session ID: {sessionId}",
                    httpContext: context);

                context.Session.Remove(UserIdKey);

                _logger.LogInformation("User logout tracked - User: {User}, Session: {Session}", userId, sessionId);
            }
        }
    }
}
