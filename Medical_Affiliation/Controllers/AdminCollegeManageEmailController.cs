
using ClosedXML.Excel;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    [Authorize(AuthenticationSchemes = "AdminAuth")]
    public class AdminCollegeManageEmailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminCollegeManageEmailController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // ================================
        // PAGE
        // ================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new CollegeEmailUpdateViewModel
            {
                Faculties = await GetFacultyDropdown()
            };

            return View(model);
        }


        // ================================
        // GET COLLEGES BY FACULTY
        // ================================
        [HttpGet]
        public async Task<IActionResult> GetColleges(string facultyCode)
        {
            if (string.IsNullOrWhiteSpace(facultyCode))
            {
                return Json(new List<object>());
            }

            var colleges = await _context.AffiliationCollegeMasters
                .Where(x => x.FacultyCode == facultyCode)
                .OrderBy(x => x.CollegeName)
                .Select(x => new
                {
                    collegeCode = x.CollegeCode,
                    collegeName = x.CollegeName,
                    collegeEmail = x.CollegeEmail,
                    collegePassword = x.ChangedPassword
                })
                .ToListAsync();

            return Json(colleges);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportCollegeMaster(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Please select an Excel file."
                });
            }

            var facultyCode = int.Parse(HttpContext.Session.GetString("FacultyCode"));
            if (facultyCode == 0) facultyCode = 2;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".xlsx")
            {
                return Json(new
                {
                    success = false,
                    message = "Please upload an .xlsx Excel file."
                });
            }

            try
            {
                using var stream = file.OpenReadStream();

                using var workbook = new XLWorkbook(stream);

                var worksheet = workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Excel worksheet not found."
                    });
                }

                var rows = worksheet.RowsUsed().Skip(1);

                int updated = 0;
                int skipped = 0;
                int notFound = 0;

                foreach (var row in rows)
                {
                    // Excel:
                    // Column 1 = CollegeCode
                    // Column 2 = CollegeName
                    // Column 3 = FacultyCode
                    // Column 4 = CollegeEmail
                    //
                    // Only CollegeCode and CollegeEmail are used.
                    // No other database fields are touched.

                    var collegeCode = row.Cell(1)
                        .GetString()
                        .Trim();

                    var collegeEmail = row.Cell(4)
                        .GetString()
                        .Trim();

                    if (string.IsNullOrWhiteSpace(collegeCode))
                    {
                        skipped++;
                        continue;
                    }

                    // If email is blank, don't overwrite existing email
                    if (string.IsNullOrWhiteSpace(collegeEmail))
                    {
                        skipped++;
                        continue;
                    }

                    // Match CollegeCode AND FacultyCode = 2
                    var existingCollege =
                        await _context.AffiliationCollegeMasters
                            .FirstOrDefaultAsync(x =>
                                x.CollegeCode == collegeCode &&
                                x.FacultyCode == "2");

                    // College not found
                    if (existingCollege == null)
                    {
                        notFound++;
                        continue;
                    }

                    // ONLY update CollegeEmail
                    existingCollege.CollegeEmail = collegeEmail;

                    updated++;
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "College email import completed successfully.",
                    updated,
                    skipped,
                    notFound
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }


        // ================================
        // GET COLLEGE EMAIL
        // ================================
        [HttpGet]
        public async Task<IActionResult> GetCollegeEmail(string collegeCode)
        {
            if (string.IsNullOrWhiteSpace(collegeCode))
            {
                return BadRequest();
            }

            var college = await _context.AffiliationCollegeMasters
                .Where(x => x.CollegeCode == collegeCode)
                .Select(x => new
                {
                    collegeCode = x.CollegeCode,
                    collegeName = x.CollegeName,
                    collegeEmail = x.CollegeEmail,
                    collegePassword = x.ChangedPassword
                })
                .FirstOrDefaultAsync();

            if (college == null)
            {
                return NotFound();
            }

            return Json(college);
        }


        // ================================
        // GET COLLEGES BY FACULTY
        // ================================
        [HttpGet]
        public async Task<IActionResult> GetCollegesByFaculty(string facultyCode)
        {
            if (string.IsNullOrWhiteSpace(facultyCode)) return Json(new List<object>());

            var colleges = await _context.AffiliationCollegeMasters
                .Where(e => e.FacultyCode == facultyCode)
                .OrderBy(e => e.CollegeName)
                .Select(e => new
                {
                    collegeCode = e.CollegeCode,
                    collegeName = e.CollegeName,
                    collegeEmail = e.CollegeEmail,
                    collegePassword = e.ChangedPassword
                })
                .ToListAsync();

            return Json(colleges);
        }

        // ================================
        // UPDATE EMAIL
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmail(string collegeCode, string email)
        {

            if (string.IsNullOrWhiteSpace(collegeCode))
            {
                return Json(new
                {
                    success = false,
                    message = "College code is required."
                });
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new
                {
                    success = false,
                    message = "Eamil address is required"
                });
            }

            if(!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
            {
                return Json(new
                {
                    success = false,
                    message = "Please enter a valid email address."
                });
            }




            var college = await _context.AffiliationCollegeMasters
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode 
                    );

            if (college == null)
            {
                return Json(new
                {
                    success = false,
                    message = "College not found."
                });
            }

            college.CollegeEmail = email.Trim();

            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                message = "Email updated successfully."
            });
        }


        // ================================
        // FACULTY DROPDOWN
        // ================================
        private async Task<List<SelectListItem>> GetFacultyDropdown()
        {
            return await _context.Faculties
                .Where(x => x.Status == "Active")
                .OrderBy(x => x.FacultyName)
                .Select(x => new SelectListItem
                {
                    Value = x.FacultyId.ToString(),
                    Text = x.FacultyName
                })
                .ToListAsync();
        }


        // ================================
        // COLLEGE DROPDOWN
        // ================================
        private async Task<List<SelectListItem>> GetCollegeDropdown(
            string facultyCode)
        {
            return await _context.AffiliationCollegeMasters
                .Where(x => x.FacultyCode == facultyCode)
                .OrderBy(x => x.CollegeName)
                .Select(x => new SelectListItem
                {
                    Value = x.CollegeCode,
                    Text = x.CollegeName
                })
                .ToListAsync();
        }
    }
}