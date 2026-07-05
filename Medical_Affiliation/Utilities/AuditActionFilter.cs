using Medical_Affiliation.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace Medical_Affiliation.Utilities
{
    public class AuditActionFilter : IAsyncActionFilter
    {
        private readonly IAuditLogService _auditLogService;

        public AuditActionFilter(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next(); // run the actual action first

            var attr = (context.ActionDescriptor as ControllerActionDescriptor)?
                .MethodInfo.GetCustomAttributes(typeof(AuditActionAttribute), true)
                .FirstOrDefault() as AuditActionAttribute;

            if (attr == null) return; // only log actions explicitly marked

            var controller = context.RouteData.Values["controller"]?.ToString();
            var action = context.RouteData.Values["action"]?.ToString();
            bool success = executedContext.Exception == null;

            await _auditLogService.LogAuditAsync(
                action: action ?? "Unknown",
                module: attr.Module ?? controller,
                tableName: null,
                recordId: null,
                oldValues: null,
                newValues: context.ActionArguments,
                description: attr.Description ?? $"{controller}/{action} executed",
                success: success);
        }
    }
}