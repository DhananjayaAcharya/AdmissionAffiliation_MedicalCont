using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Text.Json;
using System.Text.RegularExpressions;

public class AutoProgressDentalFilter : IAsyncActionFilter
{
    private readonly ApplicationDbContext _db;

    public AutoProgressDentalFilter(ApplicationDbContext db)
    {
        _db = db;
    }
    public class CAStep
    {
        public string Key { get; set; }
        public string Ctrl { get; set; }
        public string Act { get; set; }
    }

    public class CADentalStep : CAStep
    {
        public List<string> Acts { get; set; } = new();
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {

        var faculty = context.HttpContext.Session.GetString("FacultyCode");

        // Only for Dental
        if (faculty == "1")
        {
            await next();
            return;
        }
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
        var facultyCode = http.Session.GetString("FacultyCode");

        //code by ram

        //var rawLevels = http.Session.GetString("ExistingCourseLevels");

        //var levels = string.IsNullOrEmpty(rawLevels)
        //    ? new List<string>()
        //    : JsonSerializer.Deserialize<List<string>>(rawLevels)
        //        .Select(l => l.Trim().ToUpper())
        //        .Distinct()
        //        .ToList();

        var rawLevels = http.Session.GetString("ExistingCourseLevels");
        List<string> levels = new List<string>();

        if (!string.IsNullOrEmpty(rawLevels))
        {
            try
            {
                // Attempt to deserialize as JSON
                if (rawLevels.Trim().StartsWith("["))
                {
                    levels = JsonSerializer.Deserialize<List<string>>(rawLevels) ?? new List<string>();
                }
                else
                {
                    // Fallback: If it's just a comma-separated string, split it manually
                    levels = rawLevels.Split(',').ToList();
                }
            }
            catch
            {
                // If both fail, just leave as empty list so the page doesn't crash
                levels = new List<string>();
            }
        }

        // Fallback → get from AcademicIntake + MstCourses
        if (!levels.Any())
        {
            levels = await (
                from ai in _db.AcademicIntakes
                join cm in _db.MstCourses
                    on ai.Courses equals cm.CourseCode.ToString()
                where ai.CollegeCode == collegeCode
                      && !string.IsNullOrEmpty(ai.Courses)
                select cm.CourseLevel
            )
            .Distinct()
            .ToListAsync();
        }

        // Final cleanup
        levels = levels
            .Select(l => l.Trim().ToUpper())
            .Distinct()
            .ToList();


        if (string.IsNullOrEmpty(collegeCode) || levels.Count == 0)
            return;

        var ctrl = context.RouteData.Values["controller"]?.ToString();
        var act = context.RouteData.Values["action"]?.ToString();
        Console.WriteLine($"CTRL = [{ctrl}]");
        Console.WriteLine($"ACT = [{act}]");
        // 🔥 STEP LIST (same as sidebar — keys must match)
        var allSteps = new List<CADentalStep>
        {
            new () { Key="Institution", Ctrl="ContinuesAffiliation_Facultybased", Acts=new() {"Institution_Details" } },
            new () { Key="TrustDetails", Ctrl="ContinuesAffiliation_Facultybased", Acts=new() {"Aff_InstituteDetails" } },
            new () { Key="TrustMemberDetails", Ctrl="ContinuesAffiliation_Facultybased", Acts=new() {"Aff_TrustMemberDetails" } },

            // ✅ ADD THIS
            //new CAStep { Key="MBBSDetails", Ctrl="ContinuesAffiliation_Facultybased", Act="Details_Of_MBBS" },
            new () { Key="BDSDetails", Ctrl="ContinuesAffiliation_Facultybased", Acts=new() {"Ug_Course_Details" } },

            new () { Key="FacultyDetails", Ctrl="FacultyDetails", Acts=new() {"Repo_FacultyDetails" } },
            new () { Key="DeanDetails", Ctrl="ContinuesAffiliation_Facultybased", Acts=new() {"Dean_DirectorDetails" } },
            new () { Key="PrincipalDetails", Ctrl="ContinuesAffiliation_Facultybased", Acts=new() {"Aff_PrincipalDetails" } },
            new () { Key="DentalFacultyDetails", Ctrl="DentalRepository", Acts=new() {"TeachingFacultyDetails" } },

            //new CAStep { Key="LandBuilding", Ctrl="Medical_ContinuousAffiliation", Act="Medical_LandBuildingdetails" },
            //new CAStep { Key="SkillsLab", Ctrl="Medical_ContinuousAffiliation", Act="Medical_SkillsLaboratory" },
            //new CAStep { Key="DentalSkillsLab", Ctrl="PhysicalInfrastructure", Act="DentalSkillsLaboratory" },
            //new CAStep { Key="EquipmentDetails", Ctrl="Medical_ContinuousAffiliation", Act="Medical_EquimentDetails" },
            //new CAStep { Key="DentalEquipmentDetails", Ctrl="Dental", Act="SaveEquipment" },

            new () { Key = "BDSDetails", Ctrl = "ContinuesAffiliation_Facultybased", Acts = new(){ "Ug_Course_Details" }},

            new () { Key = "PgCourses", Ctrl = "AffiliationPgCourse", Acts = new() {"SavePgCoursesRguhs" }},

            new () { Key = "DentalEquipmentDetails", Ctrl = "Dental", Acts = new() {"SaveEquipment" } },

            new () { Key = "DentalSkillsLab", Ctrl = "PhysicalInfrastructure", Acts = new() {"DentalSkillsLaboratory" } },

            new () {  Key = "LandBuilding", Ctrl = "PhysicalInfrastructure", Acts=new() {"DentalCollegeLandBuildingDetail" } },

            new () {  Key = "WorkshopDetails", Ctrl = "WorkShopDetails", Acts=new() {"Create" } },
            new () {  Key = "AnimalHouseDetails", Ctrl = "AnimalHouseDetails", Acts=new() {"Create" } },
            new () {  Key = "DentalFieldPracticeArea", Ctrl = "DentalFieldPracticeArea", Acts=new() {"Create" } },

            new ()  { Key = "IntakeDetails", Ctrl = "ContinuousAffiliationIncreaseintake", Acts=new() {"DentalIncreaseIntake" } },

            new () { Key="ClinicalFacilities", Ctrl="ContinuationAffiliationClinicalFacilities", Acts=new() {"SaveDentalWardDistribution" } },
            new () { Key="Vehicle", Ctrl="Aff_AHS_ContinousApplication", Acts=new() {"CA_VehicleDetails" } },

            new () { Key="BedDistribution", Ctrl="ContinuesAffiliation_Facultybased", Acts=new() {"MedicalUGBedDistribution" } },
            new () { Key="ChairDistribution", Ctrl="PhysicalInfrastructure", Acts=new() {"ChairDistribution" } },

            //new CAStep { Key="PgCourses", Ctrl="AffiliationPgCourse", Act="SaveOtherDeptCourses" },


            new () { Key="AcademicMatters", Ctrl="CA_Aff_AcademicMatters", Acts=new() {"AcademicMatters" } },
            new (){ Key="PGAcademicMatters", Ctrl="CA_Aff_AcademicMatters", Acts=new() {"AcademicMattersPG" } },

            new () { Key="Finance", Ctrl="Aff_CA_Med_FinanceDetails", Acts=new() {"Med_CA_AccountAndFeeDetails" } },
            new () { Key="StaffDetails", Ctrl="CA_Med_StaffDetails", Acts=new() {"SavePayScale" } },

            new () { Key="Research", Ctrl="CA_Med_ResearchPublications", Acts=new() {"CA_Med_ResearchPublicationsDetails" } },
            new () { Key="Library", Ctrl="Aff_CA_MedicalLibrary", Acts=new() {"Aff_CA_Medical_LibraryDetails" } },
            new () { Key="Library", Ctrl="Aff_CA_MedicalLibrary", Acts=new() {"SaveFinance" } },
            new () { Key="LibraryServices", Ctrl="CA_Aff_MedicalLibrary", Acts=new() {"MedicalLibrary" } },
            new () { Key="DentalLibrary", Ctrl="DentalLibrary", Acts=new() {"Save" } },
            new () { Key="DentalLibraryStaff", Ctrl="DentalLibraryStaff", Acts=new() {"Save" } },
            new () { Key="DentalLibraryUser", Ctrl="DentalLibraryUser", Acts=new() {"Save" } },
            //new CAStep { Key = "PgAssociatedInstitutions", Ctrl = "AffiliationSS", Act = "AssociatedInstitutions" },

            new () { Key="TeachingStaff", Ctrl="ContinuesAffiliation_Facultybased", Acts=new() {"TeachingStaffDepartmentWise" } },
            new () { Key="TeachingStaff", Ctrl="Dental", Acts=new() {"TeachingStaffDepartmentWise" } },
            new () { Key="NonTeachingStaff", Ctrl="ContinuesAffiliation_Facultybased", Acts=new() {"NonTeachingStaffDepartmentwise" } },

            new () { Key="Hostel", Ctrl="ContinuesAffiliation_Facultybased", Acts=new() {"Aff_HostelDetails" }},
            new () { Key="IntakeDetails", Ctrl="ContinuousAffiliationIncreaseintake", Acts=new() {facultyCode == "1" ? "IncreaseIntake" : "DentalIncreaseIntake"} },

            new () { Key="DepartmentUnits", Ctrl="Medical_ContinuousAffiliation", Acts=new() {"Medical_DepartmentOfficesAndEducationalUnit" } },
            new (){ Key = "SsCoursesOffered",  Ctrl = "AffiliationSS", Acts=new() {"CoursesOffered" } },
            new (){ Key = "AssociatedInstitutions", Ctrl = "AffiliationSS", Acts=new() {"AssociatedInstitutions" } },
            new () { Key = "PaymentDetails", Ctrl="DentalPayment", Acts=new() {"Index" } },
            new () { Key = "ActionTakenReport", Ctrl="ActionTakenDeficiencyReport", Acts=new() { "Create", "Edit" } },
            //new CAStep { Key="Declaration", Ctrl="AffiliationDeclaration", Act="Declaration" }
        };





        // 🔥 Find matching step dynamically
        var step = allSteps.FirstOrDefault(s =>
            string.Equals(s.Ctrl, ctrl, StringComparison.OrdinalIgnoreCase) &&
            s.Acts.Any(a =>
                string.Equals(a, act, StringComparison.OrdinalIgnoreCase)
            )
        );

        if (step == null)
            return;

        var stepKey = step.Key;

        // ✅ Save to DB
        //foreach (var level in levels)
        //{
        //    var exists = await _db.CaProgresses.AnyAsync(x =>
        //        x.CollegeCode == collegeCode &&
        //        x.CourseLevel == level &&
        //        x.StepKey == stepKey);

        //    if (!exists)
        //    {
        //        _db.CaProgresses.Add(new CaProgress
        //        {
        //            CollegeCode = collegeCode,
        //            CourseLevel = level,
        //            StepKey = stepKey,
        //            IsCompleted = true,
        //            UpdatedAt = DateTime.Now
        //        });
        //    }
        //}

        // ============================================================
        // SAVE PROGRESS ONLY FOR CURRENTLY SELECTED COURSE LEVEL
        // ============================================================

        var currentLevel = courseLevel?
            .Trim()
            .ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(currentLevel))
            return;


        // Make sure the selected level is actually available
        if (!levels.Contains(currentLevel))
            return;


        // Check whether this exact college + level + step already exists
        var progress = await _db.CaProgresses
            .FirstOrDefaultAsync(x =>
                x.CollegeCode == collegeCode &&
                x.CourseLevel == currentLevel &&
                x.StepKey == stepKey);

        if (progress == null)
        {
            // First submission
            _db.CaProgresses.Add(new CaProgress
            {
                CollegeCode = collegeCode,
                CourseLevel = currentLevel,
                StepKey = stepKey,
                IsCompleted = true,
                UpdatedAt = DateTime.Now
            });
        }
        else
        {
            // Existing record
            progress.IsCompleted = true;
            progress.UpdatedAt = DateTime.Now;
        }

        await _db.SaveChangesAsync();
    }
}