using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Medical_Affiliation.Utilities
{
    public class AuditExceptionFilter : IAsyncExceptionFilter
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<AuditExceptionFilter> _logger;

        public AuditExceptionFilter(IAuditLogService auditLogService, ILogger<AuditExceptionFilter> logger)
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();

            _logger.LogError(context.Exception, "Unhandled exception in {Controller}/{Action}", controller, action);

            try
            {
                await _auditLogService.LogExceptionAsync(
                    context.Exception,
                    module: controller,
                    source: $"{controller}/{action}",
                    context.HttpContext);
            }
            catch (Exception logEx)
            {
                _logger.LogCritical(logEx, "Failed to write audit exception log");
            }

            // Leaving ExceptionHandled = false so your existing
            // UseDeveloperExceptionPage / UseExceptionHandler still shows the error page.
        }
    }
}