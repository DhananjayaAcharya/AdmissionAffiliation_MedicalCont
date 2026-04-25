using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json;
using System.Text.RegularExpressions;

public class AutoProgressFilter : IAsyncActionFilter
{
    private readonly ApplicationDbContext _db;

    public AutoProgressFilter(ApplicationDbContext db)
    {
        _db = db;
    }
    public class CAStep
    {
        public string Key { get; set; }
        public string Ctrl { get; set; }
        public string Act { get; set; }
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var result = await next();

        // ✅ Only POST (after save)
        if (context.HttpContext.Request.Method != "POST")
            return;

        // ✅ Only if valid
        //if (!context.ModelState.IsValid)
        //    return;

        var http = context.HttpContext;

        var collegeCode = http.Session.GetString("CollegeCode");
        var courseLevel = http.Session.GetString("CourseLevel");


        var rawLevels = http.Session.GetString("ExistingCourseLevels");

        var levels = string.IsNullOrEmpty(rawLevels)
            ? new List<string>()
            : JsonSerializer.Deserialize<List<string>>(rawLevels)
                .Select(l => l.Trim().ToUpper())
                .Distinct()
                .ToList();

        if (string.IsNullOrEmpty(collegeCode) || levels.Count == 0)
            return;

        var ctrl = context.RouteData.Values["controller"]?.ToString();
        var act = context.RouteData.Values["action"]?.ToString();

        // 🔥 STEP LIST (same as sidebar — keys must match)
        var allSteps = new List<CAStep>
        {
            new CAStep { Key="Institution", Ctrl="ContinuesAffiliation_Facultybased", Act="Institution_Details" },
            new CAStep { Key="TrustDetails", Ctrl="ContinuesAffiliation_Facultybased", Act="Aff_InstituteDetails" },
            new CAStep { Key="TrustMemberDetails", Ctrl="ContinuesAffiliation_Facultybased", Act="Aff_TrustMemberDetails" },
            // ✅ ADD THIS
            new CAStep { Key="MBBSDetails", Ctrl="ContinuesAffiliation_Facultybased", Act="Details_Of_MBBS" },

            new CAStep { Key="FacultyDetails", Ctrl="FacultyDetails", Act="Repo_FacultyDetails" },
            new CAStep { Key="DeanDetails", Ctrl="ContinuesAffiliation_Facultybased", Act="Dean_DirectorDetails" },
            new CAStep { Key="PrincipalDetails", Ctrl="ContinuesAffiliation_Facultybased", Act="Aff_PrincipalDetails" },

            new CAStep { Key="LandBuilding", Ctrl="Medical_ContinuousAffiliation", Act="Medical_LandBuildingdetails" },
            new CAStep { Key="SkillsLab", Ctrl="Medical_ContinuousAffiliation", Act="Medical_SkillsLaboratory" },
            new CAStep { Key="EquipmentDetails", Ctrl="Medical_ContinuousAffiliation", Act="Medical_EquimentDetails" },
            new CAStep { Key="EquipmentMaster", Ctrl="Medical_ContinuousAffiliation", Act="Medical_EquipmentMaster" },

            new CAStep { Key="ClinicalFacilities", Ctrl="ContinuationAffiliationClinicalFacilities", Act="SaveOperationTheatreRequirements" },
            new CAStep { Key="Vehicle", Ctrl="Aff_AHS_ContinousApplication", Act="CA_VehicleDetails" },
            new CAStep { Key="BedDistribution", Ctrl="ContinuesAffiliation_Facultybased", Act="MedicalUGBedDistribution" },

            new CAStep { Key="PgCourses", Ctrl="AffiliationPgCourse", Act="SaveOtherDeptCourses" },
            new CAStep { Key="PgAssociatedInstitutions", Ctrl="AffiliationSS", Act="AssociatedInstitutions" },

            new CAStep { Key="SsCoursesOffered", Ctrl="AffiliationSS", Act="SaveCourses" },
            new CAStep { Key="SsAssociatedInstitutions", Ctrl="AffiliationSS", Act="AssociatedInstitutions" },
            new CAStep { Key="SsCoursesApplied", Ctrl="Aff_CA_SS_CoursesAppliedSS", Act="CA_SS_CoursesApplied" },

            new CAStep { Key="AcademicMatters", Ctrl="CA_Aff_AcademicMatters", Act="AcademicMatters" },
            new CAStep{ Key="PGAcademicMatters", Ctrl="CA_Aff_AcademicMatters", Act="AcademicMattersPG" },

            new CAStep { Key="Finance", Ctrl="Aff_CA_Med_FinanceDetails", Act="Med_CA_AccountAndFeeDetails" },
            new CAStep { Key="StaffDetails", Ctrl="CA_Med_StaffDetails", Act="CA_Med_StaffDetails" },

            new CAStep { Key="Research", Ctrl="CA_Med_ResearchPublications", Act="CA_Med_ResearchPublicationsDetails" },
            new CAStep { Key="Library", Ctrl="Aff_CA_MedicalLibrary", Act="Aff_CA_Medical_LibraryDetails" },
            new CAStep { Key="LibraryServices", Ctrl="CA_Aff_MedicalLibrary", Act="MedicalLibrary" },
            new CAStep { Key = "PgAssociatedInstitutions", Ctrl = "AffiliationSS", Act = "AssociatedInstitutions" },

            new CAStep { Key="TeachingStaff", Ctrl="ContinuesAffiliation_Facultybased", Act="TeachingStaffDepartmentWise" },
            new CAStep { Key="NonTeachingStaff", Ctrl="ContinuesAffiliation_Facultybased", Act="NonTeachingStaffDepartmentwise" },

            new CAStep { Key="Hostel", Ctrl="ContinuesAffiliation_Facultybased", Act="Aff_HostelDetails" },
            //new CAStep { Key="IntakeDetails", Ctrl="ContinuousAffiliationIncreaseintake", Act="IncreaseIntake" },

            new CAStep { Key="DepartmentUnits", Ctrl="Medical_ContinuousAffiliation", Act="Medical_DepartmentOfficesAndEducationalUnit" },
            new CAStep{ Key = "SsCoursesOffered",  Ctrl = "AffiliationSS", Act = "CoursesOffered" },
            new CAStep{ Key = "AssociatedInstitutions", Ctrl = "AffiliationSS", Act = "AssociatedInstitutions" },
            //new CAStep { Key="PaymentDetails", Ctrl="AffiliationPayment", Act="Payment" },
            //new CAStep { Key="Declaration", Ctrl="AffiliationDeclaration", Act="Declaration" }
        };

        // 🔥 Find matching step dynamically
        var step = allSteps.FirstOrDefault(s =>
            string.Equals(s.Ctrl, ctrl, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(s.Act, act, StringComparison.OrdinalIgnoreCase)
        );

        if (step == null)
            return;

        var stepKey = step.Key;

        // ✅ Save to DB
        foreach (var level in levels)
        {
            var exists = await _db.CaProgresses.AnyAsync(x =>
                x.CollegeCode == collegeCode &&
                x.CourseLevel == level &&
                x.StepKey == stepKey);

            if (!exists)
            {
                _db.CaProgresses.Add(new CaProgress
                {
                    CollegeCode = collegeCode,
                    CourseLevel = level,
                    StepKey = stepKey,
                    IsCompleted = true,
                    UpdatedAt = DateTime.Now
                });
            }
        }

        await _db.SaveChangesAsync();
    }
}