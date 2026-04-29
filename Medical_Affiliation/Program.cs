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
using System.Globalization;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 🔹 MVC + Localization
builder.Services.AddScoped<AutoProgressFilter>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AutoProgressFilter>();
})
.AddViewLocalization()
.AddDataAnnotationsLocalization();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

QuestPDF.Settings.License = LicenseType.Community;

// 🔹 Localization
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("kn");
    options.SupportedCultures = new List<CultureInfo> { new("en"), new("kn") };
    options.SupportedUICultures = options.SupportedCultures;
});

// 🔹 Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 🔹 Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 🔥 DATA PROTECTION
var keysDirectory = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");

if (!Directory.Exists(keysDirectory))
{
    Directory.CreateDirectory(keysDirectory);
}

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysDirectory))
    .SetApplicationName("MedicalAffiliation");

// 🔹 HttpContext
builder.Services.AddHttpContextAccessor();

// 🔹 Services
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
// 🔥 AUTHENTICATION (FIXED)
// =============================================
builder.Services.AddAuthentication("CollegeAuth")
    .AddCookie("CollegeAuth", options =>
    {
        options.Cookie.Name = "College.Cookie";

        options.LoginPath = "/MainDashboard/MultiLogin";
        options.LogoutPath = "/CollegeLogin/Logout";
        options.AccessDeniedPath = "/Login/AccessDenied";

        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;

        options.Cookie.HttpOnly = true;

        // ✅ FIXED (CRITICAL)
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        // ✅ FIXED (VERY IMPORTANT)
        options.Cookie.Path = "/";  // safest for all environments
    })

    // Other schemes unchanged
    .AddCookie("AdminAuth", o => { o.Cookie.Name = "Admin.Cookie"; o.LoginPath = "/Admin/UniversityLogin"; o.Cookie.Path = "/"; })
    .AddCookie("SectionOfficerAuth", o => { o.Cookie.Name = "SectionOfficer.Cookie"; o.LoginPath = "/Admin/UniversityLogin"; o.Cookie.Path = "/"; })
    .AddCookie("LicInspectionAuth", o => { o.Cookie.Name = "LicInspection.Cookie"; o.LoginPath = "/LICInspection/Login"; o.Cookie.Path = "/"; })
    .AddCookie("DirectorAuth", o => { o.Cookie.Name = "Director.Cookie"; o.Cookie.Path = "/"; })
    .AddCookie("DirectorAuth1", o => { o.Cookie.Name = "LICDirector.Cookie"; o.Cookie.Path = "/"; })
    .AddCookie("LICSectionAuth", o => { o.Cookie.Name = "Section.Cookie"; o.Cookie.Path = "/"; })
    .AddCookie("FinanceAuth", o => { o.Cookie.Name = "Finance.Cookie"; o.Cookie.Path = "/"; });

// 🔹 Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CollegeOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("CollegeCode");
        policy.RequireClaim("FacultyCode");
    });
});

// 🔹 Middleware
var app = builder.Build();

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

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
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MainDashboard}/{action=Rguhsdashboard}/{id?}");

app.Run();