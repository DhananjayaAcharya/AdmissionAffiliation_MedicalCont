using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Text.Json;

namespace Medical_Affiliation.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminAuth")]
    public class CA_ApplicationStatusReportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public static readonly List<(string Key, string Label, string Group)> AllSteps =
            new()
            {
                ("Institution",           "Institution Details",              "Basic Details"),
                ("TrustDetails",          "Trust Details",                    "Basic Details"),
                ("TrustMemberDetails",    "Trust Member Details",             "Basic Details"),
                ("DeanDetails",           "Dean / Director Details",          "Basic Details"),
                ("PrincipalDetails",      "Principal Details",                "Basic Details"),
                ("LandBuilding",          "Land & Building",                  "Physical Infrastructure"),
                ("SkillsLab",             "Classroom & Laboratory",           "Physical Infrastructure"),
                ("EquipmentDetails",      "Equipment Details",                "Physical Infrastructure"),
                ("EquipmentMaster",       "Additional Equipment Details",     "Physical Infrastructure"),
                ("ClinicalFacilities",    "Clinical Facilities",              "Physical Infrastructure"),
                ("Vehicle",               "Vehicle Details",                  "Physical Infrastructure"),
                ("BedDistribution",       "Bed Distribution",                 "Physical Infrastructure"),
                ("DepartmentUnits",       "Department / Units",               "Physical Infrastructure"),
                ("Hostel",                "Hostel Details",                   "Physical Infrastructure"),
                ("AssociatedInstitutions","Associated Institutions",          "Physical Infrastructure"),
                ("AcademicMatters",       "UG Academic Performance",          "Academic & Admin"),
                ("Finance",               "Financial Details",                "Academic & Admin"),
                ("StaffDetails",          "Staff Particular Details",         "Academic & Admin"),
                ("Research",              "Research & Publications",          "Research & Library"),
                ("Library",               "Library Details",                  "Research & Library"),
                ("LibraryServices",       "Library Services",                 "Research & Library"),
                ("FacultyDetails",        "Faculty Details",                  "Human Resources"),
                ("TeachingStaff",         "Teaching Staff Dept Wise",         "Human Resources"),
                ("NonTeachingStaff",      "Non-Teaching Staff Dept Wise",     "Human Resources"),
                ("PreviewMode",           "Preview",                          "Final Preview"),
            };

        private static readonly Dictionary<string, string> LevelSpecificSteps = new()
        {
            { "MBBSDetails",     "UG" },
            { "PgCourses",       "PG" },
            { "PGAcademicMatters","PG" },
            { "SsCoursesApplied","SS" },
            { "SsCoursesOffered","SS" },
        };

        private static readonly List<(string Key, string Label, string Group)> LevelSteps = new()
        {
            ("MBBSDetails",       "UG Course Details",          "Intake Details"),
            ("PgCourses",         "PG Course Details",          "Intake Details"),
            ("PGAcademicMatters", "PG Academic Performance",    "Academic & Admin"),
            ("SsCoursesApplied",  "SS Course Details",          "Intake Details"),
            ("SsCoursesOffered",  "Courses Offered",            "Intake Details"),
        };

        public CA_ApplicationStatusReportController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = "AdminAuth,VCAuth")]
        public async Task<IActionResult> ApplicationStatusReport(
            string? search,
            string? filterStatus,
            string? sortBy,
            string? selectedFaculty,
            string? selectedCollege)
        {

            var facultyList = await _context.Faculties
            .Where(f => f.Status == "Active" &&
                   (f.FacultyId == 1 || f.FacultyId == 2))
            .OrderBy(f => f.FacultyId)
            .Select(f => new SelectListItem
            {
                Value = f.FacultyId.ToString(),
                Text = f.FacultyName
            })
            .ToListAsync();

            var collegeDropdown = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "ALL",
                    Text = "All Colleges"
                }
            };

            if (!string.IsNullOrEmpty(selectedFaculty))
            {
                var colleges = await _context.AffiliationCollegeMasters
                    .Where(c => c.FacultyCode == selectedFaculty)
                    .OrderBy(c => c.CollegeName)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CollegeCode,
                        Text = c.CollegeName
                    })
                    .ToListAsync();

                collegeDropdown.AddRange(colleges);
            }
            List<CollegeStatusRowVM> reportRows;

            if (!string.IsNullOrEmpty(selectedFaculty))
            {
                reportRows = await GetReportData(selectedFaculty, selectedCollege);
            }
            else
            {
                reportRows = await GetReportData(null, null);
            }

            int globalTotal = reportRows.Count;
            int globalComplete = reportRows.Count(r => r.Percentage == 100);
            int globalInProgress = reportRows.Count(r => r.Percentage > 0 && r.Percentage < 100);
            int globalNotStarted = reportRows.Count(r => r.Percentage == 0);

            var filteredRows = reportRows.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                filteredRows = filteredRows.Where(r => r.CollegeName.ToLower().Contains(s) || r.CollegeCode.ToLower().Contains(s));
            }
            if (!string.IsNullOrWhiteSpace(filterStatus))
            {
                filteredRows = filterStatus switch
                {
                    "complete" => filteredRows.Where(r => r.Percentage == 100),
                    "inprogress" => filteredRows.Where(r => r.Percentage > 0 && r.Percentage < 100),
                    "notstarted" => filteredRows.Where(r => r.Percentage == 0),
                    _ => filteredRows
                };
            }

            var finalList = sortBy switch
            {
                "name" => filteredRows.OrderBy(r => r.CollegeName).ToList(),
                "pct_asc" => filteredRows.OrderBy(r => r.Percentage).ToList(),
                "pct_desc" => filteredRows.OrderByDescending(r => r.Percentage).ToList(),
                _ => filteredRows.OrderByDescending(r => r.Percentage).ToList()
            };

            return View(new ApplicationStatusReportVM
            {
                Colleges = finalList,
                TotalColleges = globalTotal,
                FullyCompleted = globalComplete,
                InProgress = globalInProgress,
                NotStarted = globalNotStarted,
                Search = search,
                FilterStatus = filterStatus,
                SortBy = sortBy,

                SelectedFaculty = selectedFaculty,
                SelectedCollege = selectedCollege,
                Faculties = facultyList,
                CollegesDropdown = collegeDropdown,
            });
        }

        //CODE ADDED BY RAM ON 21-05-2026



        // 1. Modified GetReportData: Now lightweight
        private async Task<List<CollegeStatusRowVM>> GetReportData(string? selectedFaculty, string? selectedCollege)
        {
            // 1. Filter Colleges
            var collegesQuery = _context.AffiliationCollegeMasters.AsNoTracking();
            if (!string.IsNullOrEmpty(selectedFaculty))
                collegesQuery = collegesQuery.Where(c => c.FacultyCode == selectedFaculty);

            if (!string.IsNullOrEmpty(selectedCollege) && selectedCollege != "ALL")
                collegesQuery = collegesQuery.Where(c => c.CollegeCode == selectedCollege);

            var allCollegesMaster = await collegesQuery.OrderBy(c => c.CollegeName).ToListAsync();
            var collegeCodes = allCollegesMaster.Select(c => c.CollegeCode).ToList();

            // 2. FAIL-SAFE LEVEL DETECTION using Mst_Course
            var intakeData = await _context.CollegeCourseIntakeDetails
                .Where(i => collegeCodes.Contains(i.CollegeCode))
                .Select(i => new { i.CollegeCode, i.CourseCode })
                .ToListAsync();

            var courseMasters = await _context.MstCourses
                .Select(m => new { m.CourseCode, m.CourseLevel })
                .ToListAsync();

            var courseCodeToLevel = courseMasters
                .Where(m => m.CourseCode != null)
                .ToDictionary(
                    m => m.CourseCode.ToString().Trim().ToUpper(),
                    m => m.CourseLevel?.Trim().ToUpper()
                );

            var levelDict = new Dictionary<string, List<string>>();
            foreach (var intake in intakeData)
            {
                if (intake.CourseCode == null) continue;
                var cleanedCode = intake.CourseCode.ToString().Trim().ToUpper();

                if (courseCodeToLevel.TryGetValue(cleanedCode, out var level))
                {
                    if (!levelDict.ContainsKey(intake.CollegeCode))
                        levelDict[intake.CollegeCode] = new List<string>();

                    if (!string.IsNullOrEmpty(level) && !levelDict[intake.CollegeCode].Contains(level))
                        levelDict[intake.CollegeCode].Add(level);
                }
            }

            // 3. Get ALL completed records
            var allDoneRecords = await _context.CaProgresses
                .Where(x => x.IsCompleted == true && collegeCodes.Contains(x.CollegeCode))
                .Select(x => new { x.CollegeCode, x.StepKey })
                .ToListAsync();

            var reportRows = new List<CollegeStatusRowVM>();

            // THE BASE COMMON STEPS (Removed PreviewMode -> now 25 items)
            var commonSteps = new HashSet<string> {
        "Institution", "TrustDetails", "TrustMemberDetails", "DeanDetails", "PrincipalDetails",
        "LandBuilding", "SkillsLab", "EquipmentDetails", "EquipmentMaster", "ClinicalFacilities",
        "Vehicle", "BedDistribution", "DepartmentUnits", "Hostel", "AssociatedInstitutions",
        "AcademicMatters", "Finance", "StaffDetails", "Research", "Library",
        "LibraryServices", "FacultyDetails", "TeachingStaff", "NonTeachingStaff",
        "IntakeDetails"
    };

            foreach (var colMaster in allCollegesMaster)
            {
                var collegeCode = colMaster.CollegeCode;
                var completedByCollege = allDoneRecords
                    .Where(x => x.CollegeCode == collegeCode)
                    .Select(x => x.StepKey)
                    .ToHashSet();

                List<string> levels = levelDict.ContainsKey(collegeCode) ? levelDict[collegeCode] : new List<string>();

                // BUILD APPLICABLE STEPS based on Level
                var applicableSteps = new HashSet<string>(commonSteps);
                if (levels.Contains("UG")) applicableSteps.Add("MBBSDetails");
                if (levels.Contains("PG")) { applicableSteps.Add("PgCourses"); applicableSteps.Add("PGAcademicMatters"); }
                if (levels.Contains("SS")) { applicableSteps.Add("SsCoursesApplied"); applicableSteps.Add("SsCoursesOffered"); }

                // SIDEBAR RULE: 'IntakeDetails' is NOT counted in the progress percentage
                var progressApplicableSteps = new HashSet<string>(applicableSteps);
                progressApplicableSteps.Remove("IntakeDetails");

                // Calculate DONE based on the specific applicable list for this college
                int done = progressApplicableSteps.Count(stepKey => completedByCollege.Contains(stepKey));

                // DYNAMIC TOTAL: Now applied to ALL colleges regardless of whether they have started (done > 0)
                int total = progressApplicableSteps.Count;

                reportRows.Add(new CollegeStatusRowVM
                {
                    CollegeCode = collegeCode,
                    CollegeName = colMaster.CollegeName,
                    TotalSteps = total,
                    DoneSteps = done,
                    Percentage = total > 0 ? (int)Math.Round((double)done / total * 100) : 0,
                    StepGroups = new List<StepGroupVM>(),
                    PhoneNumber = selectedFaculty == "2" ? colMaster.Password ?? "" : "",
                });
            }
            return reportRows;
        }


        // 2. NEW METHOD: Fetches details only when requested
        [HttpGet]
        public async Task<IActionResult> GetCollegeDetails(string collegeCode)
        {
            // 1. Fail-Safe Level Detection using Mst_Course
            var intakeData = await _context.CollegeCourseIntakeDetails
                .Where(i => i.CollegeCode == collegeCode)
                .Select(i => new { i.CourseCode })
                .ToListAsync();

            var courseMasters = await _context.MstCourses
                .Select(m => new { m.CourseCode, m.CourseLevel })
                .ToListAsync();

            var levels = new List<string>();
            foreach (var intake in intakeData)
            {
                var match = courseMasters.FirstOrDefault(m =>
                    m.CourseCode.ToString().Trim().ToUpper() == intake.CourseCode.ToString().Trim().ToUpper());

                if (match != null && !string.IsNullOrEmpty(match.CourseLevel))
                {
                    var lvl = match.CourseLevel.Trim().ToUpper();
                    if (!levels.Contains(lvl)) levels.Add(lvl);
                }
            }
            if (!levels.Any()) levels.Add("UG");

            // 2. Get completed steps
            var doneKeys = await _context.CaProgresses
                .Where(x => x.CollegeCode == collegeCode && x.IsCompleted == true)
                .Select(x => x.StepKey)
                .ToListAsync();

            // 3. Build the list of steps
            // FIX: Added .Where(s => s.Key != "PreviewMode") to remove the Final Preview page
            var applicableSteps = AllSteps
                .Where(s => s.Key != "PreviewMode")
                .Select(s => (s.Key, s.Label, s.Group))
                .ToList();

            if (levels.Contains("UG"))
                applicableSteps.Add(("MBBSDetails", "UG Course Details", "Intake Details"));

            if (levels.Contains("PG"))
            {
                applicableSteps.Add(("PgCourses", "PG Course Details", "Intake Details"));
                applicableSteps.Add(("PGAcademicMatters", "PG Academic Performance", "Academic & Admin"));
            }

            if (levels.Contains("SS"))
            {
                applicableSteps.Add(("SsCoursesApplied", "SS Course Details", "Intake Details"));
                applicableSteps.Add(("SsCoursesOffered", "Courses Offered", "Intake Details"));
            }

            var stepDetails = applicableSteps
                .GroupBy(s => s.Group)
                .Select(grp => new StepGroupVM
                {
                    GroupName = grp.Key,
                    Steps = grp.Select(s => new StepDetailVM
                    {
                        Key = s.Key,
                        Label = s.Label,
                        Completed = doneKeys.Contains(s.Key)
                    }).ToList()
                }).ToList();

            return Json(stepDetails);
        }





    }
}
