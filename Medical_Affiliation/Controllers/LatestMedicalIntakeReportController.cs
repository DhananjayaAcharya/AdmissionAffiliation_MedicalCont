//using Medical_Affiliation.DATA;
//using Medical_Affiliation.Models; // ← for AcademicReportViewModel
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace Medical_Affiliation.Controllers
//{
//    public class LatestMedicalIntakeReportController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public LatestMedicalIntakeReportController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        [HttpGet]
//        public async Task<IActionResult> LatestMedicalReport(string selectedCollege = "All")
//        {
//            var colleges = await _context.AcademicIntakes
//                .Select(x => x.CollegeCode)
//                .Distinct()
//                .OrderBy(x => x)
//                .ToListAsync();

//            var query =
//                from ai in _context.AcademicIntakes.AsNoTracking()
//                join c in _context.AffiliationCollegeMasters
//                    on ai.CollegeCode equals c.CollegeCode
//                join cr in _context.Mst_Course
//                    on ai.Courses equals cr.CourseCode
//                select new AcademicIntakeReportRow
//                {
//                    CollegeCode = ai.CollegeCode,
//                    CollegeName = c.CollegeName,

//                    CourseCode = ai.Courses,
//                    CourseName = cr.CourseName,   // ✅ THIS IS KEY

//                    Ay2024ExistingIntake = ai.Ay2024ExistingIntake,
//                    Ay2024IncreaseIntake = ai.Ay2024IncreaseIntake,
//                    Ay2024TotalIntake = ai.Ay2024TotalIntake,

//                    Ay2025ExistingIntake = ai.Ay2025ExistingIntake,
//                    Ay2025LopNmcIntake = ai.Ay2025LopNmcIntake,
//                    Ay2025TotalIntake = ai.Ay2025TotalIntake,
//                    Ay2025NmcDocument = ai.Ay2025NmcDocument,
//                    Ay2025LopDate = ai.Ay2025LopDate,

//                    Ay2026ExistingIntake = ai.Ay2026ExistingIntake,
//                    Ay2026AddRequestedIntake = ai.Ay2026AddRequestedIntake,
//                    Ay2026TotalIntake = ai.Ay2026TotalIntake
//                };

//            if (selectedCollege != "All")
//            {
//                query = query.Where(x => x.CollegeCode == selectedCollege);
//            }

//            var intakeData = await query
//                .OrderBy(x => x.CollegeName)
//                .ThenBy(x => x.CourseName)
//                .ToListAsync();

//            var model = new AcademicReportViewModel
//            {
//                SelectedCollege = selectedCollege,
//                Colleges = colleges,
//                IntakeData = intakeData
//            };

//            if (model.IsAllColleges)
//            {
//                model.GroupedByCollege = intakeData
//                    .GroupBy(x => x.CollegeName)   // ✅ group by NAME
//                    .ToDictionary(g => g.Key, g => g.ToList());
//            }

//            return View(model);
//        }




//        // POST - Apply filter (redirects to GET with query string)
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult FilterReport(string selectedCollege)
//        {
//            return RedirectToAction(nameof(LatestMedicalReport), new { selectedCollege });
//        }

//        // View / Download NMC Document
//        [HttpGet]
//        public async Task<IActionResult> ViewNmcDocument(string collegeCode, string courseCode)
//        {
//            if (string.IsNullOrWhiteSpace(collegeCode) || string.IsNullOrWhiteSpace(courseCode))
//            {
//                return BadRequest("Invalid college or course code.");
//            }

//            var record = await _context.AcademicIntakes
//                .AsNoTracking()
//                .Where(x => x.CollegeCode == collegeCode && x.Courses == courseCode)
//                .Select(x => new { x.Ay2025NmcDocument }) // Only load the binary column
//                .FirstOrDefaultAsync();

//            if (record == null || record.Ay2025NmcDocument == null || record.Ay2025NmcDocument.Length == 0)
//            {
//                return NotFound("No NMC document found for this course.");
//            }

//            return File(
//                fileContents: record.Ay2025NmcDocument,
//                contentType: "application/pdf",
//                fileDownloadName: $"NMC_Document_{courseCode}_{collegeCode}_{DateTime.Now:yyyyMMdd}.pdf"
//            );
//        }
//    }

//    // ViewModel for the report
//    public class AcademicReportViewModel
//    {
//        public string SelectedCollege { get; set; } = "All";
//        public List<string> Colleges { get; set; } = new List<string>();
//        public List<AcademicIntake> IntakeData { get; set; } = new List<AcademicIntake>();

//        // For "All Colleges" grouping in view
//        public Dictionary<string, List<AcademicIntake>> GroupedByCollege { get; set; }
//            = new Dictionary<string, List<AcademicIntake>>();

//        // Helper property
//        public bool IsAllColleges => SelectedCollege == "All";
//    }
//}