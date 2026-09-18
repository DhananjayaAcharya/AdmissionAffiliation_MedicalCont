using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Medical_Affiliation.Controllers
{
    public class FacultyDetailsController : BaseController
    {

        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public FacultyDetailsController(ApplicationDbContext context, IHttpClientFactory httpClientFactory) : base(context)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }


        //  HELPER — shared dropdown population (DRY)
        // ──────────────────────────────────────────────────────────
        private (List<SelectListItem> subjects,
                 List<SelectListItem> designations,
                 List<SelectListItem> departments)
            GetDropdowns(string facultyCode)
        {
            var subjects = _context.MstCourses
                .Where(c => c.FacultyCode.ToString() == facultyCode)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseCode.ToString(),
                    Text = c.CourseName ?? ""
                })
                .Distinct()
                .ToList();

            var designations = _context.DesignationMasters
                .Where(d => d.FacultyCode.ToString() == facultyCode)
                .Select(d => new SelectListItem
                {
                    Value = d.DesignationCode,
                    Text = d.DesignationName ?? ""
                })
                .ToList();

            //var departments = _context.MstCourses
            //    .Where(e => e.FacultyCode.ToString() == facultyCode)
            //    .Select(d => new SelectListItem
            //    {
            //        Value = d.CourseCode.ToString(),
            //        Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
            //    })
            //    .ToList();

            //var departments = _context.MstCourses
            //    .Where(e => e.FacultyCode.ToString() == facultyCode)
            //    .Select(d => new SelectListItem
            //    {
            //        Value = d.CourseCode.ToString(),   // or SubjectCode if available
            //        Text = d.SubjectName ?? ""
            //    })
            //    .Where(x => !string.IsNullOrEmpty(x.Text)) // optional: remove empty subjects
            //    .Distinct()
            //    .ToList();
                var data = _context.MstCourses
                            .Where(e => e.FacultyCode.ToString() == facultyCode && e.SubjectName != null)
                            .AsEnumerable()
                            .GroupBy(d => new { d.SubjectName, d.CourseLevel })
                            .ToList();

            // Create groups
            var groupUG = new SelectListGroup { Name = "UG" };
            var groupPG = new SelectListGroup { Name = "PG" };
            var groupSS = new SelectListGroup { Name = "SS" };

            var departments = data
                .OrderBy(g => g.Key.CourseLevel == "UG" ? 1 :
                              g.Key.CourseLevel == "PG" ? 2 :
                              g.Key.CourseLevel == "SS" ? 3 : 4)
                .ThenBy(g => g.Key.SubjectName)
                .Select(g => new SelectListItem
                {
                    Value = g.First().CourseCode.ToString(),
                    Text = g.Key.SubjectName,
                    Group = g.Key.CourseLevel == "UG" ? groupUG :
                            g.Key.CourseLevel == "PG" ? groupPG :
                            g.Key.CourseLevel == "SS" ? groupSS : null
                })
                .ToList();

            return (subjects, designations, departments);
        }

        // ──────────────────────────────────────────────────────────
        //  GET
        // ──────────────────────────────────────────────────────────
        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public async Task<IActionResult> Repo_FacultyDetails()
        {
            var collegeCode = (HttpContext.Session.GetString("CollegeCode")
                ?? User.FindFirst("CollegeCode")?.Value);
            collegeCode = collegeCode?.Trim();

            if (string.IsNullOrWhiteSpace(collegeCode))
            {
                TempData["Error"] = "Session expired. Please log in again.";
                return RedirectToAction("MultiLogin", "MainDashboard");
            }

            ViewBag.CollegeCode = collegeCode;

            var facultyCode = int.TryParse(
                HttpContext.Session.GetString("FacultyCode") ?? User.FindFirst("FacultyCode")?.Value,
                out var parsedFacultyCode)
                ? parsedFacultyCode
                : 0;

            // ── FIX: split GroupBy + ToDictionaryAsync into a translatable step + in-memory step ──
            var departmentGroups = await _context.DepartmentMastersForUgs
                .AsNoTracking()
                .Where(x => facultyCode <= 0 || x.FacultyCode == facultyCode)
                .GroupBy(x => x.DepartmentCode)
                .Select(g => new { g.Key, Name = g.Select(v => v.DepartmentName).FirstOrDefault() })
                .ToListAsync();

            var departmentNames = departmentGroups
                .ToDictionary(x => x.Key, x => x.Name ?? x.Key);

            // ── FIX: same pattern applied here ──
            var designationGroups = await _context.UgdesignationMasters
                .AsNoTracking()
                .GroupBy(x => x.DesignationId)
                .Select(g => new { g.Key, Name = g.Select(v => v.DesignationName).FirstOrDefault() })
                .ToListAsync();

            var designationNames = designationGroups
                .ToDictionary(x => x.Key, x => x.Name ?? x.Key);

            var facultyRows = await _context.UgFacultyDetails
                .AsNoTracking()
                .Where(x => x.CollegeCode == collegeCode)
                .OrderBy(x => x.DepartmentCode)
                .ThenBy(x => x.NameOftheFaculty)
                .ToListAsync();

            var faculties = facultyRows.Select(faculty => new ApiFacultyViewModel
            {
                Id = faculty.Id,
                FacultyCode = faculty.FacultyCode,
                CollegeCode = faculty.CollegeCode ?? string.Empty,
                Name = faculty.NameOftheFaculty ?? string.Empty,
                Email = faculty.EmailId ?? string.Empty,
                Mobile = faculty.MobileNo ?? string.Empty,
                Stream = faculty.ProfessionalQualification ?? string.Empty,
                DepartmentCode = faculty.DepartmentCode ?? string.Empty,
                Department = !string.IsNullOrWhiteSpace(faculty.DepartmentCode)
                    && departmentNames.TryGetValue(faculty.DepartmentCode, out var departmentName)
                    ? departmentName
                    : faculty.DepartmentCode ?? string.Empty,
                DesignationCode = faculty.DesignationCode ?? string.Empty,
                Designation = !string.IsNullOrWhiteSpace(faculty.DesignationCode)
                    && designationNames.TryGetValue(faculty.DesignationCode, out var designationName)
                    ? designationName
                    : faculty.DesignationCode ?? string.Empty,
                Dob = faculty.Dob ?? string.Empty,
                DateOfAppointment = faculty.DateOfAppointment ?? string.Empty,
                AadhaarNo = faculty.AadhaarNo ?? string.Empty,
                PanNo = faculty.Panno ?? string.Empty,
                StateCouncilRegNo = faculty.StateCouncilRegNo ?? string.Empty,
                AebasAttendId = faculty.AebasattendId ?? string.Empty,
                ProfessionalQualification = faculty.ProfessionalQualification ?? string.Empty,
                NatureOfEmployment = faculty.NatureOfEmployment ?? string.Empty,
                TeachingExpInYrs = faculty.TeachingExpInYrs ?? string.Empty,
                PhotoFilePath = faculty.PhotoFilePath ?? string.Empty,
                Status = faculty.IsDeclared == true ? "Declared" : "Active"
            }).ToList();

            return View(faculties);
        }

        private static IEnumerable<JsonElement> FindFacultyArray(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Array)
            {
                return root.EnumerateArray();
            }

            if (root.ValueKind != JsonValueKind.Object)
            {
                return Enumerable.Empty<JsonElement>();
            }

            foreach (var property in root.EnumerateObject())
            {
                 if ((property.Name.Equals("data", StringComparison.OrdinalIgnoreCase)
                     || property.Name.Equals("faculties", StringComparison.OrdinalIgnoreCase)
                     || property.Name.Equals("faculty", StringComparison.OrdinalIgnoreCase)
                     || property.Name.Equals("users", StringComparison.OrdinalIgnoreCase)
                     || property.Name.Equals("results", StringComparison.OrdinalIgnoreCase))
                    && property.Value.ValueKind == JsonValueKind.Array)
                {
                    return property.Value.EnumerateArray();
                }

                var nested = FindFacultyArray(property.Value);
                if (nested.Any())
                {
                    return nested;
                }
            }

            return Enumerable.Empty<JsonElement>();
        }

        private static string GetString(JsonElement item, string propertyName)
        {
            return item.TryGetProperty(propertyName, out var value)
                ? value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString()
                : string.Empty;
        }

        private static int GetInt(JsonElement item, string propertyName)
        {
            return item.TryGetProperty(propertyName, out var value) && value.TryGetInt32(out var number)
                ? number
                : 0;
        }

        private async Task<string?> SaveFacultyFileAsync(IFormFile? file,string subFolder,string facultyCode)
        {
            if (file == null || file.Length == 0)
                return null;

            var extension = Path.GetExtension(file.FileName)
                                .ToLowerInvariant();

            // Faculty documents are PDFs only
            if (extension != ".pdf")
            {
                throw new Exception("Only PDF files are allowed.");
            }

            if (file.ContentType != "application/pdf")
            {
                throw new Exception("Invalid file type.");
            }

            // 5 MB limit
            if (file.Length > 5 * 1024 * 1024)
            {
                throw new Exception("File size cannot exceed 5 MB.");
            }

            string rootPath = facultyCode == "2"
                ? BaseDentalPath
                : BaseMedicalPath;

            string basePath =
                Path.Combine(rootPath, "FacultyDetails");

            string fullFolder =
                Path.Combine(basePath, subFolder);

            if (!Directory.Exists(fullFolder))
                Directory.CreateDirectory(fullFolder);

            string fileName =
                $"{Guid.NewGuid()}.pdf";

            string fullPath =
                Path.Combine(fullFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fullPath;
        }

        // ──────────────────────────────────────────────────────────
        //  POST
        // ──────────────────────────────────────────────────────────
        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]   // ✅ FIX: Added missing Authorize
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Repo_FacultyDetails(IList<FacultyDetailsViewModel> model)
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
            {
                TempData["Error"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            if (model == null || !model.Any())
            {
                return RedirectToAction("Repo_ExamResults");
            }

            var activeRows = model.Where(m => string.IsNullOrWhiteSpace(m.RemoveRemarks)).ToList();
            var removedRows = model.Where(m => !string.IsNullOrWhiteSpace(m.RemoveRemarks)).ToList();

            // ✅ FIX: Use async transaction
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // ✅ FIX: Use async EF Core query
                var existingFaculty = await _context.FacultyDetails
                    .Where(f => f.CollegeCode == collegeCode && f.FacultyCode == facultyCode)
                    .ToListAsync();

                // ── Handle removed rows ──────────────────────────────────────────────
                foreach (var m in removedRows)
                {
                    var existing = FindExistingRecord(existingFaculty, m);
                    if (existing != null)
                    {
                        existing.IsRemoved = true;
                        existing.RemoveRemarks = m.RemoveRemarks.Trim();
                        _context.FacultyDetails.Update(existing);
                    }
                }

                // ── Handle active rows ───────────────────────────────────────────────
                foreach (var m in activeRows)
                {
                    string name = m.NameOfFaculty?.Trim() ?? "";
                    string designation = m.Designation?.Trim() ?? "";
                    string mobile = m.Mobile?.Trim() ?? "";
                    string email = m.Email?.Trim() ?? "";
                    string pan = m.PAN?.Trim() ?? "";
                    string aadhaar = m.Aadhaar?.Trim() ?? "";
                    string dept = m.SelectedDepartment?.Trim() ?? "";
                    string recognizedPG = m.RecognizedPGTeacher?.Trim() ?? "";
                    string recognizedPhD = m.RecognizedPhDTeacher?.Trim() ?? "";
                    string litigation = m.LitigationPending?.Trim() ?? "";
                    DateOnly? from = m.From;
                    DateOnly? to = m.To;

                    // ✅ FIX: Use local variable 'facultyCode' (not 'FacultyCode')
                    var guidePath = await SaveFacultyFileAsync(m.GuideRecognitionDoc, "GuideDocs", facultyCode);
                    var phdPath = await SaveFacultyFileAsync(m.PhDRecognitionDoc, "PhDDocs", facultyCode);
                    var litigPath = await SaveFacultyFileAsync(m.LitigationDoc, "LitigationDocs", facultyCode);

                    string examinerFor = m.ExaminerForList != null && m.ExaminerForList.Any()
                                             ? string.Join(",", m.ExaminerForList)
                                             : null;

                    var existing = FindExistingRecord(existingFaculty, m);

                    if (existing != null)
                    {
                        // ── Update existing record ───────────────────────────────────
                        existing.NameOfFaculty = name;
                        existing.Designation = designation;
                        existing.RecognizedPgTeacher = recognizedPG;
                        existing.Mobile = mobile;
                        existing.Email = email;
                        existing.Pan = pan;
                        existing.Aadhaar = aadhaar;
                        existing.DepartmentDetails = dept;
                        existing.RecognizedPhDteacher = recognizedPhD;
                        existing.LitigationPending = litigation;
                        existing.IsExaminer = m.IsExaminer;
                        existing.ExaminerFor = examinerFor;
                        existing.IsRemoved = false;
                        existing.RemoveRemarks = null;
                        existing.From = from;
                        existing.To = to;

                        // Guide doc — replace only if a new file was uploaded
                        if (guidePath != null)
                        {
                            if (!string.IsNullOrEmpty(existing.GuideRecognitionDocPath) &&
                                System.IO.File.Exists(existing.GuideRecognitionDocPath))
                            {
                                System.IO.File.Delete(existing.GuideRecognitionDocPath);
                            }
                            existing.GuideRecognitionDocPath = guidePath;
                        }

                        // PhD doc — replace only if a new file was uploaded
                        if (phdPath != null)
                        {
                            if (!string.IsNullOrEmpty(existing.PhDrecognitionDocPath) &&
                                System.IO.File.Exists(existing.PhDrecognitionDocPath))
                            {
                                System.IO.File.Delete(existing.PhDrecognitionDocPath);
                            }
                            existing.PhDrecognitionDocPath = phdPath;
                        }

                        // Litigation doc — replace only if a new file was uploaded
                        if (litigPath != null)
                        {
                            if (!string.IsNullOrEmpty(existing.LitigationDocPath) &&
                                System.IO.File.Exists(existing.LitigationDocPath))
                            {
                                System.IO.File.Delete(existing.LitigationDocPath);
                            }
                            existing.LitigationDocPath = litigPath;
                        }

                        _context.FacultyDetails.Update(existing);
                    }
                    else
                    {
                        // ── Insert new record ────────────────────────────────────────
                        var faculty = new FacultyDetail
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            NameOfFaculty = name,
                            Designation = designation,
                            RecognizedPgTeacher = recognizedPG,
                            RecognizedPhDteacher = recognizedPhD,
                            LitigationPending = litigation,
                            Mobile = mobile,
                            Email = email,
                            Pan = pan,
                            From = from,
                            To = to,
                            Aadhaar = aadhaar,
                            DepartmentDetails = dept,
                            GuideRecognitionDocPath = guidePath,
                            PhDrecognitionDocPath = phdPath,
                            LitigationDocPath = litigPath,
                            IsExaminer = m.IsExaminer,
                            ExaminerFor = examinerFor,
                            IsRemoved = false,
                            RemoveRemarks = null,
                            Subject = "N/A",
                        };
                        _context.FacultyDetails.Add(faculty);
                    }
                }

                // ✅ FIX: Use async SaveChanges and Commit
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (facultyCode == "2")
                {
                    return RedirectToAction("TeachingStaffDepartmentWise", "Dental");
                }

                return RedirectToAction("Dean_DirectorDetails", "ContinuesAffiliation_Facultybased");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Error saving faculty records: " + ex.Message;
                return RedirectToAction(nameof(Repo_FacultyDetails));
            }
        }

        private FacultyDetail FindExistingRecord(List<FacultyDetail> existingFaculty, FacultyDetailsViewModel m)
        {
            if (m.FacultyDetailId > 0)
                return existingFaculty.FirstOrDefault(f => f.Id == m.FacultyDetailId);

            string aadhaar = m.Aadhaar?.Trim();
            string pan = m.PAN?.Trim();

            if (!string.IsNullOrWhiteSpace(aadhaar) && !string.IsNullOrWhiteSpace(pan))
                return existingFaculty.FirstOrDefault(f =>
                    !string.IsNullOrWhiteSpace(f.Aadhaar) &&
                    !string.IsNullOrWhiteSpace(f.Pan) &&
                    f.Aadhaar.Trim() == aadhaar &&
                    f.Pan.Trim() == pan);

            return null;
        }

        public IActionResult ViewFacultyDocument(int id, string type, string mode = "view")
        {
            var faculty = _context.FacultyDetails.FirstOrDefault(f => f.Id == id);
            if (faculty == null)
                return NotFound();

            string filePath = null;

            switch (type.ToLower())
            {
                case "pg":
                    filePath = faculty.GuideRecognitionDocPath;
                    break;

                case "phd":
                    filePath = faculty.PhDrecognitionDocPath;
                    break;

                case "litig":
                    filePath = faculty.LitigationDocPath;
                    break;

                default:
                    return BadRequest("Invalid document type.");
            }

            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return NotFound("Document not uploaded.");

            // 🔹 Get file name
            var fileName = Path.GetFileName(filePath);

            // 🔹 Detect content type
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            if (mode == "download")
            {
                // 📥 FORCE DOWNLOAD
                return PhysicalFile(filePath, contentType, fileName);
            }

            // 👀 VIEW IN BROWSER
            return PhysicalFile(filePath, contentType);
        }
        private byte[] ConvertFileToBytes(IFormFile formFile)
        {
            using (var ms = new MemoryStream())
            {
                formFile.CopyTo(ms);
                return ms.ToArray();
            }
        }


        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public IActionResult ViewFacultyPhoto(int id)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode")
                ?? User.FindFirst("CollegeCode")?.Value;

            var faculty = _context.UgFacultyDetails
                .AsNoTracking()
                .FirstOrDefault(f => f.Id == id && f.CollegeCode == collegeCode);

            if (faculty == null || string.IsNullOrEmpty(faculty.PhotoFilePath) || !System.IO.File.Exists(faculty.PhotoFilePath))
            {
                // Serve a placeholder so broken <img> tags don't show a red X
                var placeholderPath = Path.Combine("wwwroot", "images", "no-photo.png");
                return System.IO.File.Exists(placeholderPath)
                    ? PhysicalFile(Path.GetFullPath(placeholderPath), "image/png")
                    : NotFound();
            }

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(faculty.PhotoFilePath, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(faculty.PhotoFilePath, contentType);
        }
    }
}
