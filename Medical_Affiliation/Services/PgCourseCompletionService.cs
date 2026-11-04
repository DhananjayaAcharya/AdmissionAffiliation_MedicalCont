using Medical_Affiliation.DATA;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services
{
    public class PgCourseCompletionService
    {
        private readonly ApplicationDbContext _context;
        public PgCourseCompletionService(ApplicationDbContext context) => _context = context;

        // Which sections each Form-B page saves (used for the per-page ticks in the sidebar)
        public static readonly Dictionary<string, string[]> PageSections = new()
        {
            ["CourseGeneralDetails"] = new[] { "General Details" },
            ["CourseSummaryDetails"] = new[] { "Summary" },
            ["CourseInfrastructureDetails"] = new[]
            {
                "Inspection Committee", "Fee Paid", "Other Course/Observership", "OPD", "OPD Room Areas",
                "Wards", "Wards Parameters", "Operation Theatres", "Seminar Room", "Museum",
                "Library", "Research Lab", "Equipment"
            },
            ["CourseServicesWorkload"] = new[] { "Specialty Clinics", "Department Services", "Clinical Workload" },
            ["CourseStaffDetails"] = new[] { "Eligible Faculty", "PG Students by Year", "Exam Results" },
            ["CourseAcademicActivities"] = new[] { "Academic Activities", "Academic Summary" }
        };

        public sealed class Status
        {
            public Dictionary<string, List<string>> PendingByCourse { get; } =
                new(StringComparer.OrdinalIgnoreCase);

            public int Total => PendingByCourse.Count;
            public int Completed => PendingByCourse.Count(x => x.Value.Count == 0);
            public bool AllCompleted => Total > 0 && Completed == Total;

            public List<string> PendingFor(string? courseCode) =>
                courseCode != null && PendingByCourse.TryGetValue(courseCode.Trim(), out var p)
                    ? p : new List<string>();

            public bool IsCompleted(string? courseCode) =>
                courseCode != null && PendingByCourse.TryGetValue(courseCode.Trim(), out var p) && p.Count == 0;
        }

        // faculty + college + affiliation type + course level (UG/PG/SS) -> status for every course (department)
        public async Task<Status> GetStatusAsync(string f, string c, string? t, string level)
        {
            var status = new Status();
            level = (level ?? "").Trim().ToUpperInvariant();

            var courseCodes = await _context.MstMedicalCollegeCourseIntakes
                .AsNoTracking()
                .Where(cc => cc.CollCode == c && cc.UgPg != null && cc.UgPg.Trim().ToUpper() == level)
                .Select(cc => cc.CourseCode.ToString())
                .Distinct()
                .ToListAsync();

            var sections = await GetSectionCoursesAsync(f, c, t);

            foreach (var code in courseCodes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!.Trim()))
            {
                status.PendingByCourse[code] = sections
                    .Where(s => !s.Codes.Contains(code))
                    .Select(s => s.Name)
                    .ToList();
            }
            return status;
        }

        private async Task<List<(string Name, HashSet<string> Codes)>> GetSectionCoursesAsync(string f, string c, string? t)
        {
            var result = new List<(string Name, HashSet<string> Codes)>();

            async Task Add(string name, IQueryable<string?> courseCodes)
            {
                var codes = await courseCodes.Distinct().ToListAsync();
                result.Add((name, codes.Where(x => !string.IsNullOrWhiteSpace(x))
                                       .Select(x => x!.Trim())
                                       .ToHashSet(StringComparer.OrdinalIgnoreCase)));
            }

            await Add("General Details", _context.TxnPgcourseGeneralDetails.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Summary", _context.TxnPgcourseSummaryDetails.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));

            await Add("Inspection Committee", _context.LocalInspectionCommittees.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Fee Paid", _context.FeePaidDetails.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Other Course/Observership", _context.OtherCourseObserverships.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("OPD", _context.OpdDetails.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("OPD Room Areas", _context.OpdRoomAreas.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Wards", _context.WardsHeaders.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Wards Parameters", _context.WardsParameters.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Operation Theatres", _context.OperationTheatreDistributions.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Seminar Room", _context.SeminarRooms.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Museum", _context.DepartmentalMuseums.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Library", _context.LibraryFacilities.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Research Lab", _context.DepartmentalResearchLabs.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Equipment", _context.DepartmentEquipments.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));

            await Add("Specialty Clinics", _context.SpecialtyClinicDetails.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Department Services", _context.DepartmentServiceDetails.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Clinical Workload", _context.ClinicalWorkloadDetails.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));

            await Add("Eligible Faculty", _context.EligibleFacultyDetails.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("PG Students by Year", _context.PgStudentsYearWiseDetails.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Exam Results", _context.StudentExamResultDetails.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));

            await Add("Academic Activities", _context.AcademicActivityDetails.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));
            await Add("Academic Summary", _context.AcademicSummaryDetails.AsNoTracking()
                .Where(x => x.FacultyCode == f && x.CollegeCode == c && x.TypeOfAffiliation == t).Select(x => (string?)x.CourseCode));

            return result;
        }
    }
}