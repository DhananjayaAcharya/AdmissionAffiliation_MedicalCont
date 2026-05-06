using Medical_Affiliation.DATA;
using Medical_Affiliation.Services;
using Medical_Affiliation.Services.Faculty;
using Medical_Affiliation.Services.Handlers;
using Medical_Affiliation.Services.Handlers.Medical;
using Medical_Affiliation.Services.Interfaces;
using Medical_Affiliation.Services.UserContext;
using Medical_Affiliation.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
using QuestPDF.Infrastructure;

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
// 🔹 MVC + AutoProgressFilter
// =============================================
builder.Services.AddScoped<AutoProgressFilter>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AutoProgressFilter>();
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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// =============================================
// 🔹 Session
// =============================================
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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

builder.Services.AddScoped<LicTadaService>();
builder.Services.AddScoped<ICAInstitutionBasicDetails, CABasicDetailsService>();
builder.Services.AddScoped<IFacultyHospitalHandler, MedicalHospitalHandler>();
builder.Services.AddScoped<IHospitalService, FacultyHospitalService>();
builder.Services.AddScoped<ICAAcademicService, CAAcademicService>();
builder.Services.AddScoped<ICALibraryService, CALibraryService>();
builder.Services.AddScoped<ICAVehicleService, CAVehicleService>();
builder.Services.AddScoped<ICAHospitalAffiliationService, CAHospitalAffiliationService>();
builder.Services.AddScoped<ICALandClassEquipmentService, CALandAndEquipmentService>();
builder.Services.AddScoped<ICAPreviewService, CAPreviewService>();
builder.Services.AddScoped<ICAFinanceService, CAFinanceService>();
builder.Services.AddScoped<ICAAdminTeachAndHostel, CAAdminTeachAndHostelService>();
builder.Services.AddScoped<ICAFacultyDesigNonTeaching, CAFacultyDesigNonTeachingService>();
builder.Services.AddScoped<IUserContext, SessionUserContext>();
builder.Services.AddScoped<ICAPaymentService, CAPaymentService>();
builder.Services.AddScoped<ICADeclarationService, CADeclarationService>();


// =============================================
// 🔹 Authentication Schemes
// To add a new role: just add one line to the array below. Nothing else changes.
// =============================================
var authSchemes = new[]
{
    new { Scheme = CookieAuthenticationDefaults.AuthenticationScheme,
          Cookie = ".AspNetCore.Cookies",   Login = "/LICInspection/Login",   Logout = "/LICInspection/Logout",  AccessDenied = "/LICInspection/AccessDenied", ExpireMinutes = 60 * 24 * 7  },

    new { Scheme = "LicInspectionAuth",
          Cookie = "LicInspection.Cookie",  Login = "/LICInspection/Login",   Logout = "/LICInspection/Logout",  AccessDenied = "/LICInspection/AccessDenied", ExpireMinutes = 60 * 24 * 14 },

    new { Scheme = "SectionOfficerAuth",
          Cookie = "SectionOfficer.Cookie", Login = "/Admin/UniversityLogin",  Logout = "/SectionOfficer/Logout", AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },

    new { Scheme = "CollegeAuth",
          Cookie = "College.Cookie",        Login = "/MainDashboard/MultiLogin", Logout = "/CollegeLogin/Logout", AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },

    new { Scheme = "AdminAuth",
          Cookie = "Admin.Cookie",          Login = "/Admin/UniversityLogin",  Logout = "/Admin/Logout",          AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },

    new { Scheme = "DirectorAuth",
          Cookie = "Director.Cookie",       Login = "/Admin/AdminLogin",       Logout = "/Admin/Logout",          AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },

    new { Scheme = "DirectorAuth1",
          Cookie = "LICDirector.Cookie",    Login = "/Admin/UniversityLogin",  Logout = "/LIC_Director/Logout",   AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },

    new { Scheme = "LICSectionAuth",
          Cookie = "Section.Cookie",        Login = "/Admin/UniversityLogin",  Logout = "/LIC_Director/Logout",   AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },

    new { Scheme = "FinanceAuth",
          Cookie = "Finance.Cookie",        Login = "/Admin/AdminLogin",       Logout = "/Admin/FinanceLogout",   AccessDenied = "/Login/AccessDenied",          ExpireMinutes = 30           },
};

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
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
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.Path = "/";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(s.ExpireMinutes);
        options.SlidingExpiration = true;
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


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
// 🔹 Custom Session Middlewares
// =============================================b
app.UseMiddleware<Medical_Affiliation.Utilities.AdminSessionMiddleware>();
app.UseMiddleware<Medical_Affiliation.Utilities.CollegeSessionMiddleware>();
app.UseMiddleware<Medical_Affiliation.Utilities.SectionOfficerSessionMiddleware>();
app.UseMiddleware<Medical_Affiliation.Utilities.LicInspectionSessionMiddleware>();

app.UseAuthorization();


// =============================================
// 🔹 Routing
// =============================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MainDashboard}/{action=Rguhsdashboard}/{id?}");


app.Run();