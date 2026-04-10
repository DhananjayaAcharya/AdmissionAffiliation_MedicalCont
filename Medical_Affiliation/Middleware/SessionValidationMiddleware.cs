using Microsoft.AspNetCore.Authentication;

namespace Medical_Affiliation.Utilities
{
    // ================= ADMIN MIDDLEWARE =================
    public class AdminSessionMiddleware
    {
        private readonly RequestDelegate _next;
        public AdminSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var result = await context.AuthenticateAsync("AdminAuth");
            if (result.Succeeded && result.Principal != null)
            {
                context.User = result.Principal;
                var userIP = context.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                var claimIP = context.User.FindFirst("UserIP")?.Value;
                var claimAgent = context.User.FindFirst("UserAgent")?.Value;
                if (claimIP != null && (claimIP != userIP || claimAgent != userAgent))
                {
                    await context.SignOutAsync("AdminAuth");
                    context.Response.Redirect("/Admin/UniversityLogin");
                    return;
                }
            }
            await _next(context);
        }
    }

    // ================= COLLEGE MIDDLEWARE =================
    public class CollegeSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public CollegeSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var result = await context.AuthenticateAsync("CollegeAuth");

            if (result.Succeeded && result.Principal != null)
            {
                context.User = result.Principal;

                var currentIP = context.Connection.RemoteIpAddress?.ToString();
                var currentAgent = context.Request.Headers["User-Agent"].ToString();

                var claimIP = context.User.FindFirst("UserIP")?.Value;
                var claimAgent = context.User.FindFirst("UserAgent")?.Value;

                // 🔹 DEBUG LOG (very useful in production)
                Console.WriteLine("===== COLLEGE MIDDLEWARE =====");
                Console.WriteLine($"ClaimIP: {claimIP}");
                Console.WriteLine($"CurrentIP: {currentIP}");

                // ✅ SAFE VALIDATION (more stable)
                bool isIPMismatch = !string.IsNullOrEmpty(claimIP) && claimIP != currentIP;
                bool isAgentMismatch = !string.IsNullOrEmpty(claimAgent) && claimAgent != currentAgent;

                // 🔥 IMPORTANT CHANGE:
                // Only logout if BOTH mismatch (prevents false logout)
                if (isIPMismatch && isAgentMismatch)
                {
                    await context.SignOutAsync("CollegeAuth");
                    context.Session.Clear();

                    context.Response.Redirect("/Login/Login");
                    return;
                }

                // ✅ Ensure required claims exist
                var collegeCode = context.User.FindFirst("CollegeCode")?.Value;
                var facultyCode = context.User.FindFirst("FacultyCode")?.Value;

                if (string.IsNullOrWhiteSpace(collegeCode) ||
                    string.IsNullOrWhiteSpace(facultyCode))
                {
                    await context.SignOutAsync("CollegeAuth");
                    context.Session.Clear();

                    context.Response.Redirect("/Login/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
    // ================= SECTION OFFICER MIDDLEWARE =================
    public class SectionOfficerSessionMiddleware
    {
        private readonly RequestDelegate _next;
        public SectionOfficerSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var result = await context.AuthenticateAsync("SectionOfficerAuth");
            if (result.Succeeded && result.Principal != null)
            {
                context.User = result.Principal;
                var userIP = context.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                var claimIP = context.User.FindFirst("UserIP")?.Value;
                var claimAgent = context.User.FindFirst("UserAgent")?.Value;
                if (claimIP != null && (claimIP != userIP || claimAgent != userAgent))
                {
                    await context.SignOutAsync("SectionOfficerAuth");
                    context.Response.Redirect("/Admin/UniversityLogin");
                    return;
                }
            }
            await _next(context);
        }
    }

    // ================= LIC INSPECTION MIDDLEWARE =================
    public class LicInspectionSessionMiddleware
    {
        private readonly RequestDelegate _next;
        public LicInspectionSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            var result = await context.AuthenticateAsync("LicInspectionAuth");
            if (result.Succeeded && result.Principal != null)
            {
                context.User = result.Principal;

                // Validate required claims are present
                var phone = context.User.FindFirst("PhoneNumber")?.Value;
                var memberType = context.User.FindFirst("TypeofMember")?.Value;

                if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(memberType))
                {
                    await context.SignOutAsync("LicInspectionAuth");
                    context.Response.Redirect("/LICInspection/Login");
                    return;
                }

                // IP + User-Agent hijacking protection
                var userIP = context.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.Request.Headers["User-Agent"].ToString();
                var claimIP = context.User.FindFirst("UserIP")?.Value;
                var claimAgent = context.User.FindFirst("UserAgent")?.Value;

                if (claimIP != null && (claimIP != userIP || claimAgent != userAgent))
                {
                    await context.SignOutAsync("LicInspectionAuth");
                    context.Response.Redirect("/LICInspection/Login");
                    return;
                }
            }
            await _next(context);
        }
    }
}