
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
                    collegeEmail = x.CollegeEmail
                })
                .ToListAsync();

            return Json(colleges);
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
                    collegeEmail = x.CollegeEmail
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
                    collegeEmail = e.CollegeEmail
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