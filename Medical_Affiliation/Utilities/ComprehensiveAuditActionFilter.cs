using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Medical_Affiliation.Utilities
{
    /// <summary>
    /// Comprehensive audit filter that logs all controller actions
    /// </summary>
    public class ComprehensiveAuditActionFilter : IAsyncActionFilter
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<ComprehensiveAuditActionFilter> _logger;

        public ComprehensiveAuditActionFilter(
            IAuditLogService auditLogService,
            ILogger<ComprehensiveAuditActionFilter> logger)
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controller = context.RouteData.Values["controller"]?.ToString() ?? "Unknown";
            var action = context.RouteData.Values["action"]?.ToString() ?? "Unknown";
            var startTime = DateTime.UtcNow;

            try
            {
                // Execute the action
                var executedContext = await next();

                // Log the successful action
                var success = executedContext.Exception == null;
                var actionArgs = SerializeActionArguments(context.ActionArguments);

                await _auditLogService.LogAuditAsync(
                    action: action,
                    module: controller,
                    tableName: null,
                    recordId: null,
                    oldValues: null,
                    newValues: actionArgs,
                    description: $"Action executed: {controller}/{action}",
                    success: success);

                _logger.LogInformation(
                    "Action executed - Controller: {Controller}, Action: {Action}, Success: {Success}, Duration: {Duration}ms",
                    controller,
                    action,
                    success,
                    (DateTime.UtcNow - startTime).TotalMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in audit filter for {Controller}/{Action}", controller, action);

                await _auditLogService.LogExceptionAsync(
                    ex,
                    module: controller,
                    source: $"{controller}/{action}",
                    context.HttpContext);

                throw;
            }
        }

        /// <summary>
        /// Serialize action arguments for logging, excluding sensitive data
        /// </summary>
        private object SerializeActionArguments(IDictionary<string, object?> arguments)
        {
            var sensitiveFields = new[] { "password", "token", "secret", "apikey", "creditcard", "ssn" };

            var sanitized = arguments
                .Where(kvp => !sensitiveFields.Any(field => kvp.Key.IndexOf(field, StringComparison.OrdinalIgnoreCase) >= 0))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return sanitized.Any() ? (object)sanitized : "(no arguments)";
        }
    }
}
