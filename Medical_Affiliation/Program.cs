using Medical_Affiliation.DATA;
using Medical_Affiliation.Controllers;
using Medical_Affiliation.Middleware;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services;
using Medical_Affiliation.Services.Faculty;
using Medical_Affiliation.Services.Handlers;
using Medical_Affiliation.Services.Handlers.Medical;
using Medical_Affiliation.Services.Interfaces;
using Medical_Affiliation.Services.UserContext;
using Medical_Affiliation.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using QuestPDF.Infrastructure;
using SecureConnectionLibrary;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);


// =============================================
// 🔹 Localization
// =============================================
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var supportedCultures = new List<CultureInfo>
{
    new CultureInfo("en"),
    new CultureInfo("kn")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("kn");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider(),
        new QueryStringRequestCultureProvider()
    };
});


// =============================================
// 🔹 MVC + AutoProgressFilter + AuditLog Filters
// =============================================
builder.Services.AddScoped<AutoProgressFilter>();
builder.Services.AddScoped<AutoProgressDentalFilter>();
builder.Services.AddScoped<AuditExceptionFilter>();
builder.Services.AddScoped<AuditActionFilter>();
builder.Services.AddScoped<ComprehensiveAuditActionFilter>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AutoProgressFilter>();
    options.Filters.Add<AutoProgressDentalFilter>();
    options.Filters.Add<AuditExceptionFilter>();              // catches all unhandled exceptions
    options.Filters.Add<ComprehensiveAuditActionFilter>();    // logs all actions comprehensively
    options.MaxModelBindingCollectionSize = int.MaxValue;
})
.AddViewLocalization()
.AddDataAnnotationsLocalization();


// =============================================
// 🔹 QuestPDF
// =============================================
QuestPDF.Settings.License = LicenseType.Community;


// =============================================
// 🔹 Database
// =============================================
var encryptedConnectionString = builder.Configuration
    .GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

var decryptedConnectionString =
    ConnectionStringSecurity.Decrypt(encryptedConnectionString);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(decryptedConnectionString);
});


// =============================================
// 🔹 Session
// =============================================
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow both HTTP and HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax;
});


// =============================================
// 🔥 DATA PROTECTION
// =============================================
var keysDirectory = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");

if (!Directory.Exists(keysDirectory))
{
    Directory.CreateDirectory(keysDirectory);
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
    .SetApplicationName("MedicalAffiliation");


// =============================================
// 🔹 HttpContext + Services
// =============================================
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("RguhsFacultyApi", client =>
{
    client.BaseAddress = new Uri("https://beta.rguhsqp.com/");
    client.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.AddScoped<GeoPhotoModule.Services.IGeoPhotoRepository>(_ =>
    new GeoPhotoModule.Services.GeoPhotoRepository(decryptedConnectionString));

builder.Services.AddScoped<LicTadaService>();
builder.Services.AddScoped<ICAInstitutionBasicDetails, CABasicDetailsService>();
builder.Services.AddScoped<IFacultyHospitalHandler, MedicalHospitalHandler>();
builder.Services.AddScoped<IFacultyHospitalHandler, DentalHospitalHandler>();
builder.Services.AddScoped<IHospitalService, FacultyHospitalService>();
builder.Services.AddScoped<ICAAcademicService, CAAcademicService>();
builder.Services.AddScoped<ICADentalBedDistributionService, CADentalBedDistributionService>();
builder.Services.AddScoped<ICADepartmentOfficesMeuService, CADepartmentOfficesMeuService>();

builder.Services.AddScoped<ICAAcademicIntakeService, CAAcademicIntakeService>();
builder.Services.AddScoped<IInstitutionPreviewService, InstitutionPreviewService>();
builder.Services.AddScoped<ICATrustMemberDetailsPreviewService, CATrustMemberDetailsPreviewService>();
builder.Services.AddScoped<ICADentalLandBuildingPreviewService, CADentalLandBuildingPreviewService>();
builder.Services.AddScoped<ICATrustDetailsService, CATrustDetailsService>();
builder.Services.AddScoped<IUGPgIntakeDetailsService, UGPgIntakeDetailsService>();
builder.Services.AddScoped<ICAHostelPreviewService, CAHostelPreviewService>();
builder.Services.AddScoped<ICAPgCourseService, CAPgCourseService>();
builder.Services.AddScoped<ICADentalChairDistributionPreviewService, CADentalChairDistributionPreviewService>();
builder.Services.AddScoped<ICAVehiclePreviewService, CAVehiclePreviewService>();
builder.Services.AddScoped<ICADentalPreviewService, CAPreviewDentalService>();
builder.Services.AddScoped<IHumanResourcesPreviewService, HumanResourcesPreviewService>();
builder.Services.AddScoped<ITeachingFacultyDetailsService, TeachingFacultyDetailsService>();
builder.Services.AddScoped<IWorkShopDetailsService, WorkShopDetailsService>();
builder.Services.AddScoped<IAnimalHouseService, AnimalHouseService>();
builder.Services.AddScoped<ICAEquipmentPreviewService, CAEquipmentPreviewService>();
builder.Services.AddScoped<ICADentalFieldPracticeAreaService, CADentalFieldPracticeAreaService>();

builder.Services.AddScoped<ICALibraryService, CALibraryService>();
builder.Services.AddScoped<ICADentalLibraryService, CADentalLibraryService>();
builder.Services.AddScoped<ICAVehicleService, CAVehicleService>();
builder.Services.AddScoped<ICAHospitalAffiliationService, CAHospitalAffiliationService>();
builder.Services.AddScoped<ICALandClassEquipmentService, CALandAndEquipmentService>();
builder.Services.AddScoped<ICAPreviewService, CAPreviewService>();
builder.Services.AddScoped<ICADentalPreviewService, CAPreviewDentalService>();
builder.Services.AddScoped<ICADentalHospitalAffiliationService, CADentalHospitalAffiliationService>();
builder.Services.AddScoped<ICAFinanceService, CAFinanceService>();
builder.Services.AddScoped<ICAAcademicPerformancePreviewService, CAAcademicPerformancePreviewService>();
builder.Services.AddScoped<ICAAdminTeachAndHostel, CAAdminTeachAndHostelService>();
builder.Services.AddScoped<ICAFacultyDesigNonTeaching, CAFacultyDesigNonTeachingService>();
builder.Services.AddScoped<IUserContext, SessionUserContext>();
builder.Services.AddScoped<ICAPaymentService, CAPaymentService>();
builder.Services.AddScoped<ICADentalPaymentService, DentalPaymentService>();
builder.Services.AddScoped<PaymentCalculationController>();
builder.Services.AddScoped<ICADeclarationService, CADeclarationService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ICADentalStaffDetailsPreviewService, CADentalStaffDetailsPreviewService>();

builder.Services.Configure<WhatsAppSettings>(builder.Configuration.GetSection("WhatsAppSettings"));
builder.Services.AddHttpClient<IWhatsAppService, WhatsAppService>();
builder.Services.AddScoped<IPrincipalContactLookupService, PrincipalContactLookupService>();
builder.Services.AddScoped<IPaymentReceiptPdfService, PaymentReceiptPdfService>();

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
// =============================================
// 🔹 Authentication Schemes
// To add a new role: just add one line to the array below. Nothing else changes.
// =============================================
var authSchemes = new[]
{
    new { Scheme = CookieAuthenticationDefaults.AuthenticationScheme,
          Cookie = ".AspNetCore.Cookies",   Login = "/LICInspection/Login",      Logout = "/LICInspection/Logout",    AccessDenied = "/LICInspection/AccessDenied", ExpireMinutes = 60 * 24 * 7  },

    new { Scheme = "LicInspectionAuth",
          Cookie = "LicInspection.Cookie",  Login = "/LICInspection/Login",      Logout = "/LICInspection/Logout",    AccessDenied = "/LICInspection/AccessDenied", ExpireMinutes = 60 * 24 * 14 },

    new { Scheme = "SectionOfficerAuth",
          Cookie = "SectionOfficer.Cookie", Login = "/Admin/UniversityLogin",    Logout = "/SectionOfficer/Logout",   AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },

    new { Scheme = "CollegeAuth",
          Cookie = "College.Cookie",        Login = "/MainDashboard/MultiLogin", Logout = "/CollegeLogin/Logout",     AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },

    new { Scheme = "AdminAuth",
          Cookie = "Admin.Cookie",          Login = "/Admin/UniversityLogin",    Logout = "/Admin/Logout",            AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },

    new { Scheme = "DirectorAuth",
          Cookie = "Director.Cookie",       Login = "/Admin/AdminLogin",         Logout = "/Admin/Logout",            AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },

    new { Scheme = "DirectorAuth1",
          Cookie = "LICDirector.Cookie",    Login = "/Admin/UniversityLogin",    Logout = "/LIC_Director/Logout",     AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },

    new { Scheme = "LICSectionAuth",
          Cookie = "Section.Cookie",        Login = "/Admin/UniversityLogin",    Logout = "/LIC_Director/Logout",     AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },

    new { Scheme = "FinanceAuth",
          Cookie = "Finance.Cookie",        Login = "/Admin/AdminLogin",         Logout = "/Admin/FinanceLogout",     AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },

    new { Scheme = "VCAuth",
          Cookie = ".VCAuth",               Login = "/MainDashboard/MultiLogin", Logout = "/MainDashboard/Logout",    AccessDenied = "/MainDashboard/MultiLogin",    ExpireMinutes = 60           },
};

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "CollegeAuth";
    options.DefaultChallengeScheme = "CollegeAuth";
    options.DefaultForbidScheme = "CollegeAuth";
    options.DefaultSignInScheme = "CollegeAuth";
    options.DefaultSignOutScheme = "CollegeAuth";
});

foreach (var s in authSchemes)
{
    authBuilder.AddCookie(s.Scheme, options =>
    {
        options.LoginPath = s.Login;
        options.LogoutPath = s.Logout;
        options.AccessDeniedPath = s.AccessDenied;

        options.Cookie.Name = s.Cookie;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.Path = "/";

        options.ExpireTimeSpan = TimeSpan.FromMinutes(s.ExpireMinutes);
        options.SlidingExpiration = true;

        // 🔹 NEW: For AJAX/fetch requests, return 401/403 instead of redirecting
        // to the login page. Without this, an expired session (or missing
        // claims) on a fetch() call gets a 302 to the login page, the browser
        // follows it and returns login-page HTML, and client-side
        // response.json() breaks with "Unexpected token '<'" — masking the
        // real "session expired" condition as a JSON parse error.
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                if (IsAjaxRequest(context.Request))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                if (IsAjaxRequest(context.Request))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }
                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });
}


// =============================================
// 🔹 Authorization
// =============================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CollegeOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("CollegeCode");
        policy.RequireClaim("FacultyCode");
    });
});


// =============================================
// 🔹 IIS / Request Body Size
// =============================================
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 104_857_600; // 100 MB
});


// =============================================
// ✅ FIX: Large Form Binding (60+ rows × 15 fields)
// Default MVC limit = 1024 values → HTTP 400 with large tables
// =============================================
builder.Services.Configure<FormOptions>(options =>
{
    options.ValueCountLimit = int.MaxValue;         // ✅ unlimited field count
    options.ValueLengthLimit = int.MaxValue;        // ✅ unlimited field value length
    options.MultipartBodyLengthLimit = 104_857_600; // ✅ 100 MB for file uploads
    options.MultipartHeadersCountLimit = int.MaxValue;
    options.MultipartBoundaryLengthLimit = int.MaxValue;
});


// =============================================
// 🔹 Forwarded Headers
// =============================================
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});


// =============================================
// 🔹 Cookie Policy
// =============================================
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.Secure = CookieSecurePolicy.Always;
    options.HttpOnly = HttpOnlyPolicy.Always;
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});


// =============================================
// 🔹 Build App
// =============================================
var app = builder.Build();


// =============================================
// 🔹 Prevent Browser Cache After Logout
// =============================================
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, private";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseDeveloperExceptionPage(); // TEMPORARY — swap to UseExceptionHandler in production
    // app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();


// =============================================
// 🔹 Localization Middleware
// =============================================
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
app.UseRequestLocalization(locOptions.Value);

app.UseCookiePolicy();

app.UseRouting();

app.UseSession();


// =============================================
// 🔹 AdmissionAffiliation Path Redirect
// =============================================
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;  

    if (!string.IsNullOrEmpty(path) && path.StartsWith("/AdmissionAffiliation"))
    {
        var lower = path.Replace("/AdmissionAffiliation", "/admissionaffiliation");
        context.Response.Redirect(lower + context.Request.QueryString, true);
        return;
    }

    await next();
});


app.UseAuthentication();


// =============================================
// Lock the college application after declaration consent.
// The readonly preview remains available, while all edit routes and POST
// requests are redirected back to it for the current session.
// =============================================
app.Use(async (context, next) =>
{
    var isReadOnly = string.Equals(
        context.Session.GetString("CAApplicationReadOnly"),
        "true",
        StringComparison.OrdinalIgnoreCase);

    var controller = context.Request.RouteValues["controller"]?.ToString() ?? string.Empty;
    var action = context.Request.RouteValues["action"]?.ToString() ?? string.Empty;
    var isPreview = controller.Equals("CAPreview", StringComparison.OrdinalIgnoreCase);
    var isLogout = action.Contains("Logout", StringComparison.OrdinalIgnoreCase)
        || context.Request.Path.Value?.Contains("/Logout", StringComparison.OrdinalIgnoreCase) == true;

    if (isReadOnly && !isPreview && !isLogout
        && !HttpMethods.IsGet(context.Request.Method)
        && !HttpMethods.IsHead(context.Request.Method))
    {
        context.Response.Redirect("/CAPreview/Preview");
        return;
    }

    await next();
});


// =============================================
// 🔹 Audit & Session Tracking Middlewares
// =============================================
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<SessionTrackingMiddleware>();


// =============================================
// 🔹 Custom Session Middlewares
// =============================================
app.UseMiddleware<Medical_Affiliation.Utilities.AdminSessionMiddleware>();
app.UseMiddleware<Medical_Affiliation.Utilities.CollegeSessionMiddleware>();
app.UseMiddleware<Medical_Affiliation.Utilities.SectionOfficerSessionMiddleware>();
app.UseMiddleware<Medical_Affiliation.Utilities.LicInspectionSessionMiddleware>();

app.UseAuthorization();


// =============================================
// 🔹 Static Files (wwwroot + MedicalUGFacultyList)
// =============================================
app.UseStaticFiles();

// ===== D:\MedicalUGFacultyList Mapping =====
var medicalPath = Directory.Exists(@"E:\")
                    ? @"E:\MedicalUGFacultyList"
                    : @"D:\MedicalUGFacultyList";

if (!Directory.Exists(medicalPath))
    Directory.CreateDirectory(medicalPath);

if (!Directory.Exists(Path.Combine(medicalPath, "Photos")))
    Directory.CreateDirectory(Path.Combine(medicalPath, "Photos"));

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(medicalPath),
    RequestPath = "/MedicalUGFacultyList"
});


// =============================================
// 🔹 Routing
// =============================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MainDashboard}/{action=Rguhsdashboard}/{id?}");


app.Run();


// =============================================
// 🔹 Local Helpers
// =============================================
// Detects fetch()/XHR/JSON-expecting requests so cookie auth can return a
// plain 401/403 status instead of a login-page redirect for them. Browser
// navigations (full page loads, e.g. clicking a normal <a> link) are left
// alone and still redirect to the login page as before.
static bool IsAjaxRequest(HttpRequest request)
{
    return request.Headers["X-Requested-With"] == "XMLHttpRequest"
        || request.Headers.Accept.Any(h => h != null && h.Contains("application/json"))
        || request.Path.StartsWithSegments("/PaymentDocument");
}