using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Linq;

namespace Medical_Affiliation.Controllers
{
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
        public async Task<IActionResult> ApplicationStatusReport(string? search, string? filterStatus, string? sortBy)
        {
            var reportRows = await GetReportData();

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
                SortBy = sortBy
            });
        }

        private async Task<List<CollegeStatusRowVM>> GetReportData()
        {
            var allCollegesMaster = await _context.AffiliationCollegeMasters
                .Where(c => c.FacultyCode == "1")
                .ToListAsync();

            var intakeData = await _context.CollegeCourseIntakeDetails
                .Where(i => i.FacultyCode == 1)
                .ToListAsync();

            var courseMasterData = await _context.CourseMasters.ToListAsync();

            var collegeLevelMap = intakeData
                .Join(courseMasterData,
                      intake => intake.CourseCode?.Trim(),
                      master => master.CourseCode?.Trim(),
                      (intake, master) => new { intake.CollegeCode, master.CourseLevel })
                .GroupBy(x => x.CollegeCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.CourseLevel?.Trim().ToUpper()).Where(l => !string.IsNullOrEmpty(l)).Distinct().ToList()
                );

            var allProgress = await _context.CaProgresses
                .Where(x => x.IsCompleted == true)
                .ToListAsync();

            var progressGroups = allProgress
                .GroupBy(x => x.CollegeCode)
                .ToDictionary(g => g.Key, g => g.ToList());

            var reportRows = new List<CollegeStatusRowVM>();

            foreach (var colMaster in allCollegesMaster)
            {
                var collegeCode = colMaster.CollegeCode;
                progressGroups.TryGetValue(collegeCode, out var progRecords);

                List<string> levels = new List<string>();
                if (collegeLevelMap.TryGetValue(collegeCode, out var mappedLevels) && mappedLevels.Any())
                    levels = mappedLevels;
                else if (progRecords != null && progRecords.Any())
                    levels = progRecords.Select(x => x.CourseLevel?.Trim().ToUpper()).Where(l => !string.IsNullOrEmpty(l)).Distinct().ToList();
                else
                    levels = new List<string> { "UG" };

                var applicableSteps = AllSteps.Select(s => (s.Key, s.Label, s.Group)).ToList();
                foreach (var ls in LevelSteps)
                {
                    if (LevelSpecificSteps.TryGetValue(ls.Key, out var req) && levels.Contains(req))
                        applicableSteps.Add((ls.Key, ls.Label, ls.Group));
                }

                var doneKeys = progRecords != null ? progRecords.Select(x => x.StepKey).Distinct().ToHashSet() : new HashSet<string>();
                
                // --- FIX: RESTORE STEPGROUPS LOGIC ---
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

                int total = applicableSteps.Count;
                int done = applicableSteps.Count(s => doneKeys.Contains(s.Key));

                reportRows.Add(new CollegeStatusRowVM
                {
                    CollegeCode = collegeCode,
                    CollegeName = colMaster.CollegeName,
                    Levels = levels,
                    TotalSteps = total,
                    DoneSteps = done,
                    Percentage = total > 0 ? (int)Math.Round((double)done / total * 100) : 0,
                    StepGroups = stepDetails // NOW ASSIGNED
                });
            }
            return reportRows;
        }
    }
}
