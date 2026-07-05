using Medical_Affiliation.DATA;
using Medical_Affiliation.Services.Interfaces;
using Medical_Affiliation.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Medical_Affiliation.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminAuth")]
    public class AffiliationAdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLogService _auditLogService;

        public AffiliationAdminController(
            ApplicationDbContext context,
            IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
        }

        // ---------------------------------------------------------
        // EXAMPLE 1: Automatic audit logging via [AuditAction] attribute.
        // No extra code needed inside the action - AuditActionFilter logs
        // the posted form data (context.ActionArguments) automatically,
        // and marks Success/Failure based on whether an exception occurred.
        // ---------------------------------------------------------
        [HttpPost]
        [AuditAction(Module = "AffiliationAdmin", Description = "Updated application status report filter")]
        public IActionResult ApplyStatusFilter(int districtId, int facultyCode)
        {
            // your existing filter logic here
            // ...
            return RedirectToAction("ApplicationStatusReport");
        }

        // ---------------------------------------------------------
        // SetAffiliationType - AffiliationType is stored in SESSION only.
        // Old value = current session value (read before overwrite)
        // New value = the value being set now
        // RecordId = CollegeCode from the authenticated user's claims,
        // so this session change is traceable to a specific college.
        // ---------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> SetAffiliationType(string affiliationType)
        {
            var collegeCode = User?.FindFirst("CollegeCode")?.Value;

            // capture the "before" state from session BEFORE overwriting it
            var oldAffiliationType = HttpContext.Session.GetString("AffiliationType");

            try
            {
                // update session as you already do
                HttpContext.Session.SetString("AffiliationType", affiliationType);

                await _auditLogService.LogAuditAsync(
                    action: "SetAffiliationType",
                    module: "AffiliationAdmin",
                    tableName: null,                 // not a DB table - session-based value
                    recordId: collegeCode,            // ties this entry to a specific college
                    oldValues: new { AffiliationType = oldAffiliationType },
                    newValues: new { AffiliationType = affiliationType },
                    description: $"Affiliation type changed in session from '{oldAffiliationType}' to '{affiliationType}'",
                    success: true);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                await _auditLogService.LogAuditAsync(
                    action: "SetAffiliationType",
                    module: "AffiliationAdmin",
                    tableName: null,
                    recordId: collegeCode,
                    oldValues: new { AffiliationType = oldAffiliationType },
                    newValues: new { affiliationType },
                    description: $"Failed to update affiliation type in session: {ex.Message}",
                    success: false);

                throw; // AuditExceptionFilter will also capture the full exception details
            }
        }

        // ---------------------------------------------------------
        // EXAMPLE: An action that deliberately throws, to verify
        // AuditExceptionFilter captures it end-to-end.
        // Remove this action once you've confirmed logging works.
        // ---------------------------------------------------------
        [HttpGet]
        public IActionResult TestException()
        {
            throw new InvalidOperationException("Test exception for audit logging verification");
        }
    }
}