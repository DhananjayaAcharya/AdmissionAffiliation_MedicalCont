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
//builder.Services.AddControllersWithViews()
//    .AddViewLocalization()
//    .AddDataAnnotationsLocalization();

builder.Services.AddScoped<AutoProgressFilter>();   // ✅ ADD THIS

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AutoProgressFilter>();      // ✅ ADD THIS
})
.AddViewLocalization()
.AddDataAnnotationsLocalization();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

QuestPDF.Settings.License = LicenseType.Community;

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

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IHospitalService, FacultyHospitalService>();
builder.Services.AddScoped<IFacultyHospitalHandler, MedicalHospitalHandler>();
builder.Services.AddScoped<IUserContext, SessionUserContext>();
builder.Services.AddScoped<LicTadaService>();
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

// 🔹 HttpContextAccessor
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<LicTadaService>();
builder.Services.AddScoped<ICAInstitutionBasicDetails, CABasicDetailsService>();
//builder.Services.AddScoped<IFacultyHospitalHandler, NursingHospitalHandler>();
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
builder.Services.AddScoped<IFacultyHospitalHandler, MedicalHospitalHandler>();
builder.Services.AddScoped<IHospitalService, FacultyHospitalService>();
builder.Services.AddScoped<IUserContext, SessionUserContext>();
builder.Services.AddScoped<ICAPreviewService, CAPreviewService>();

// =============================================
// 🔥 AUTHENTICATION - DYNAMIC COOKIE PATH (Fixed for Local + Server)
// =============================================
var cookiePath = builder.Environment.IsDevelopment() ? "/" : "/admissionaffiiliation";

builder.Services.AddAuthentication("CollegeAuth")
    .AddCookie("CollegeAuth", options =>
    {
        options.Cookie.Name = "College.Cookie";
        options.LoginPath = "/Login/Login";
        options.LogoutPath = "/CollegeLogin/Logout";
        options.AccessDeniedPath = "/Login/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.Path = cookiePath;               // ← Dynamic Fix
    })
    .AddCookie("AdminAuth", options =>
    {
        options.Cookie.Name = "Admin.Cookie";
        options.LoginPath = "/Admin/UniversityLogin";
        options.Cookie.Path = cookiePath;
    })
    .AddCookie("SectionOfficerAuth", options =>
    {
        options.Cookie.Name = "SectionOfficer.Cookie";
        options.LoginPath = "/Admin/UniversityLogin";
        options.Cookie.Path = cookiePath;
    })
    .AddCookie("LicInspectionAuth", options =>
    {
        options.Cookie.Name = "LicInspection.Cookie";
        options.LoginPath = "/LICInspection/Login";
        options.Cookie.Path = cookiePath;
    })
    .AddCookie("DirectorAuth", options =>
    {
        options.Cookie.Name = "Director.Cookie";
        options.Cookie.Path = cookiePath;
    })
    .AddCookie("DirectorAuth1", options =>
    {
        options.Cookie.Name = "LICDirector.Cookie";
        options.Cookie.Path = cookiePath;
    })
    .AddCookie("LICSectionAuth", options =>
    {
        options.Cookie.Name = "Section.Cookie";
        options.Cookie.Path = cookiePath;
    })
    .AddCookie("FinanceAuth", options =>
    {
        options.Cookie.Name = "Finance.Cookie";
        options.Cookie.Path = cookiePath;
    });

// Authorization Policy
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CollegeOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("CollegeCode");
        policy.RequireClaim("FacultyCode");
    });
});

// Forwarded Headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Middleware Pipeline
app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=MainDashboard}/{action=Rguhsdashboard}/{id?}");

app.Run();