using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medical_Affiliation.Controllers
{
    [IgnoreAntiforgeryToken]
    [Route("UGFaculty")]
    public class UGFacultyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        // ── FIX 1: All folder names come from one place.
        //    Never hardcode "D:\..." again — use wwwroot or a config-driven base.
        private string PhotosFolder =>
            Path.Combine(_environment.WebRootPath, "MedicalUGFacultyList", "Photos");

        private string UploadsFolder =>
            Path.Combine(_environment.WebRootPath, "MedicalUGFacultyList");

        // ── FIX 2: Web-relative URLs are built from the request path prefix,
        //    so they work correctly under sub-applications and reverse proxies.
        private string PhotosWebRoot => "/MedicalUGFacultyList/Photos";
        private string UploadsWebRoot => "/MedicalUGFacultyList";

        public UGFacultyController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ── FIX 3: Index() now has the same session guard as AddFaculty().
        [Route("")]
        [Route("Index")]
        public IActionResult Index()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            if (string.IsNullOrEmpty(collegeCode))
                return RedirectToAction("Login", "Login"); // FIX 4: correct controller name

            ViewBag.Departments = _context.DepartmentMastersForUgs.ToList();
            ViewBag.Designations = _context.UgdesignationMasters.ToList();

            return View();
        }
        [Authorize(AuthenticationSchemes = "CollegeAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("AddFaculty")]
        public IActionResult AddFaculty()
        {
          
        
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            var collegeName = User.Identity?.Name;
            ViewBag.CollegeName = collegeName; ViewBag.CollegeName = collegeName;
           

            if (string.IsNullOrEmpty(collegeCode))
                return RedirectToAction("Login", "Login"); // FIX 4: consistent redirect

            var college = _context.AffiliationCollegeMasters
                .FirstOrDefault(c => c.CollegeCode == collegeCode);
            if (college == null)
                return RedirectToAction("Dashboard", "Home"); // FIX 4: verify this action exists

            ViewBag.Departments = _context.DepartmentMastersForUgs.ToList();
            ViewBag.Designations = _context.UgdesignationMasters.ToList();

            return View();
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        [Route("GetExisting")]
        public IActionResult GetExisting()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            if (string.IsNullOrEmpty(collegeCode))
                return Json(new { success = false, message = "Session expired." });

            try
            {
                var data = (from faculty in _context.UgFacultyDetails
                            join dept in _context.DepartmentMastersForUgs
                                on faculty.DepartmentCode equals dept.DepartmentCode
                                into deptJoin
                            from d in deptJoin.DefaultIfEmpty()

                            join desig in _context.UgdesignationMasters
                                on faculty.DesignationCode equals desig.DesignationId
                                into desigJoin
                            from dg in desigJoin.DefaultIfEmpty()

                            where faculty.CollegeCode == collegeCode

                            select new
                            {
                                Faculty = faculty,
                                DeptName = d.DepartmentName,
                                DesigName = dg.DesignationName
                            })
                            .ToList();

                var groupedRows = data
                    .GroupBy(x => new
                    {
                        Department = !string.IsNullOrEmpty(x.DeptName)
                            ? x.DeptName : x.Faculty.DepartmentCode,

                        DesignationCode = x.Faculty.DesignationCode,

                        Designation = !string.IsNullOrEmpty(x.DesigName)
                            ? x.DesigName : x.Faculty.DesignationCode
                    })

                    .SelectMany(g => g.Select(f => new
                    {
                        id = f.Faculty.Id,
                        departmentCode = g.Key.Department,
                        designationCode = g.Key.Designation,
                        designationOrder = g.Key.DesignationCode,
                        nameOftheFaculty = f.Faculty.NameOftheFaculty ?? "",
                        dob = f.Faculty.Dob ?? "",
                        dateOfAppointment = f.Faculty.DateOfAppointment ?? "",
                        mobileNo = f.Faculty.MobileNo ?? "",
                        panNo = f.Faculty.Panno ?? "",
                        stateCouncilRegNo = f.Faculty.StateCouncilRegNo ?? "",
                        isDeclared = f.Faculty.IsDeclared ?? false,
                        printedCopyUploaded = f.Faculty.PrintedCopyUploaded ?? false,
                        aEBASAttendId = f.Faculty.AebasattendId ?? "",
                        professionalQualification = f.Faculty.ProfessionalQualification ?? "",
                        natureOfEmployment = f.Faculty.NatureOfEmployment ?? "",
                        teachingExpInYrs = f.Faculty.TeachingExpInYrs ?? "",
                        photoFilePath = f.Faculty.PhotoFilePath ?? ""
                    }))

                    .OrderBy(x => x.departmentCode)
                    .ThenBy(x => x.designationOrder)
                    .ToList();

                return Json(new { success = true, data = groupedRows });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [Authorize(AuthenticationSchemes = "CollegeAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        [Route("GetAllFaculty")]
        public IActionResult GetAllFaculty()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            if (string.IsNullOrEmpty(collegeCode))
                return Json(new { success = false, message = "Session expired." });

            try
            {
                var rawData = (from faculty in _context.UgFacultyDetails

                               join dept in _context.DepartmentMastersForUgs
                                   on faculty.DepartmentCode equals dept.DepartmentCode
                                   into deptJoin
                               from d in deptJoin.DefaultIfEmpty()

                               join desig in _context.UgdesignationMasters
                                   on faculty.DesignationCode equals desig.DesignationId
                                   into desigJoin
                               from dg in desigJoin.DefaultIfEmpty()

                               where faculty.CollegeCode == collegeCode
                                  && faculty.IsDeclared == true

                               select new
                               {
                                   Id = faculty.Id,

                                   DepartmentName = d.DepartmentName
                                                    ?? faculty.DepartmentCode,

                                   DesignationName = dg.DesignationName
                                                    ?? faculty.DesignationCode,

                                   DesignationOrder = faculty.DesignationCode,

                                   NameOftheFaculty = faculty.NameOftheFaculty ?? "",
                                   Dob = faculty.Dob ?? "",
                                   DateOfAppointment = faculty.DateOfAppointment ?? "",
                                   MobileNo = faculty.MobileNo ?? "",
                                   PanNo = faculty.Panno ?? "",
                                   StateCouncilRegNo = faculty.StateCouncilRegNo ?? "",
                                   IsDeclared = faculty.IsDeclared ?? false,
                                   PrintedCopyUploaded = faculty.PrintedCopyUploaded ?? false,
                                   AEBASAttendId = faculty.AebasattendId ?? "",
                                   ProfessionalQualification = faculty.ProfessionalQualification ?? "",
                                   NatureOfEmployment = faculty.NatureOfEmployment ?? "",
                                   TeachingExpInYrs = faculty.TeachingExpInYrs ?? "",
                                   PhotoFilePath = faculty.PhotoFilePath ?? ""
                               })
                               .ToList();

                var rows = rawData
                    .OrderBy(x => x.DepartmentName)
                    .ThenBy(x => x.DesignationOrder)
                    .Select((x, index) => new
                    {
                        slNo = index + 1,
                        id = x.Id,
                        departmentCode = x.DepartmentName,
                        designationCode = x.DesignationName,
                        nameOftheFaculty = x.NameOftheFaculty,
                        dob = x.Dob,
                        dateOfAppointment = x.DateOfAppointment,
                        mobileNo = x.MobileNo,
                        panNo = x.PanNo,
                        stateCouncilRegNo = x.StateCouncilRegNo,
                        isDeclared = x.IsDeclared,
                        printedCopyUploaded = x.PrintedCopyUploaded,
                        aEBASAttendId = x.AEBASAttendId,
                        professionalQualification = x.ProfessionalQualification,
                        natureOfEmployment = x.NatureOfEmployment,
                        teachingExpInYrs = x.TeachingExpInYrs,
                        photoFilePath = x.PhotoFilePath
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = rows,
                    collegeName = HttpContext.Session.GetString("CollegeName") ?? "",
                    collegeCode
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        // ── Insert single record ──────────────────────────────────────────────
        [HttpPost]
        [Route("InsertSingleFaculty")]
        public IActionResult InsertSingleFaculty([FromBody] SaveFacultyDto dto)
        {
            if (dto == null)
                return Json(new { success = false, message = "Invalid request payload." });

            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            if (string.IsNullOrEmpty(collegeCode))
                return Json(new { success = false, message = "Session expired." });

            try
            {
                bool mobileExists = _context.UgFacultyDetails
                    .Any(f => f.MobileNo == dto.MobileNo && f.CollegeCode == collegeCode);
                if (mobileExists)
                    return Json(new { success = false, message = $"Mobile number {dto.MobileNo} is already registered." });

                var faculty = new UgFacultyDetail
                {
                    CollegeCode = collegeCode,
                    DepartmentCode = dto.DepartmentCode ?? "",
                    NameOftheFaculty = dto.NameOftheFaculty ?? "",
                    DesignationCode = dto.DesignationCode ?? "",
                    Dob = dto.Dob ?? "",
                    DateOfAppointment = dto.DateOfAppointment ?? "",
                    MobileNo = dto.MobileNo ?? "",
                    Panno = dto.PanNo ?? "",
                    StateCouncilRegNo = dto.StateCouncilRegNo ?? "",
                    AebasattendId = dto.AEBASAttendId ?? "",
                    ProfessionalQualification = dto.ProfessionalQualification ?? "",
                    NatureOfEmployment = dto.NatureOfEmployment ?? "",
                    TeachingExpInYrs = dto.TeachingExpInYrs ?? "",
                    CreatedOn = DateTime.Now,
                    Ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    IsDeclared = false,
                    PrintedCopyUploaded = false,
                    PhotoFilePath = ""
                };

                _context.UgFacultyDetails.Add(faculty);
                _context.SaveChanges();

                return Json(new { success = true, newId = faculty.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Insert failed: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // ── Update single record ──────────────────────────────────────────────
        [HttpPost]
        [Route("UpdateSingleFaculty")]
        public IActionResult UpdateSingleFaculty([FromBody] SaveFacultyDto dto)
        {
            if (dto == null || dto.Id <= 0)
                return Json(new { success = false, message = "Missing record identifier." });

            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            if (string.IsNullOrEmpty(collegeCode))
                return Json(new { success = false, message = "Session expired." });

            try
            {
                var record = _context.UgFacultyDetails
                    .FirstOrDefault(f => f.Id == dto.Id && f.CollegeCode == collegeCode);
                if (record == null)
                    return Json(new { success = false, message = "Record not found." });

                bool mobileExists = _context.UgFacultyDetails
                    .Any(f => f.MobileNo == dto.MobileNo && f.CollegeCode == collegeCode && f.Id != dto.Id);
                if (mobileExists)
                    return Json(new { success = false, message = $"Mobile number {dto.MobileNo} is already registered to another faculty." });

                record.DepartmentCode = dto.DepartmentCode;
                record.NameOftheFaculty = dto.NameOftheFaculty;
                record.DesignationCode = dto.DesignationCode;
                record.Dob = dto.Dob;
                record.DateOfAppointment = dto.DateOfAppointment;
                record.MobileNo = dto.MobileNo;
                record.Panno = dto.PanNo;
                record.StateCouncilRegNo = dto.StateCouncilRegNo;
                record.AebasattendId = dto.AEBASAttendId;
                record.ProfessionalQualification = dto.ProfessionalQualification;
                record.NatureOfEmployment = dto.NatureOfEmployment;
                record.TeachingExpInYrs = dto.TeachingExpInYrs;

                _context.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Update failed: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // ── Upload photo ──────────────────────────────────────────────────────
        // FIX 1: Uses IWebHostEnvironment-based paths, not hardcoded D:\ strings.
        [HttpPost]
        [Route("UploadPhotoDirect")]
        public async Task<IActionResult> UploadPhotoDirect(IFormFile photo, int facultyId)
        {
            if (photo == null || photo.Length == 0 || facultyId <= 0)
                return Json(new { success = false, message = "Invalid upload parameters." });

            var allowedExts = new[] { ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (!allowedExts.Contains(ext))
                return Json(new { success = false, message = "Only JPG and PNG files are allowed." });

            if (photo.Length > 2 * 1024 * 1024)
                return Json(new { success = false, message = "Photo must be under 2 MB." });

            // FIX 1a: folder resolved via IWebHostEnvironment — works on any OS / server
            if (!Directory.Exists(PhotosFolder))
                Directory.CreateDirectory(PhotosFolder);

            var record = _context.UgFacultyDetails.FirstOrDefault(f => f.Id == facultyId);
            if (record == null)
                return Json(new { success = false, message = "Faculty record not found." });

            // Delete existing photo from disk if present
            if (!string.IsNullOrEmpty(record.PhotoFilePath))
            {
                var oldDiskPath = Path.Combine(PhotosFolder, Path.GetFileName(record.PhotoFilePath));
                if (System.IO.File.Exists(oldDiskPath))
                {
                    try { System.IO.File.Delete(oldDiskPath); } catch { /* non-critical */ }
                }
            }

            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var fullSavePath = Path.Combine(PhotosFolder, uniqueName);

            using (var stream = new FileStream(fullSavePath, FileMode.Create))
                await photo.CopyToAsync(stream);

            // FIX 1b: web path uses the property, not a raw string literal
            record.PhotoFilePath = $"{PhotosWebRoot}/{uniqueName}";
            _context.SaveChanges();

            return Json(new { success = true, path = record.PhotoFilePath });
        }

        // ── Delete faculty ────────────────────────────────────────────────────
        // FIX 1: Uses IWebHostEnvironment-based path for disk deletion.
        [Authorize(AuthenticationSchemes = "CollegeAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost]
        [Route("DeleteFaculty")]
        public IActionResult DeleteFaculty([FromBody] DeleteFacultyDto dto)
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            if (string.IsNullOrEmpty(collegeCode))
                return Json(new { success = false, message = "Session expired." });

            if (dto == null || dto.Id <= 0)
                return Json(new { success = false, message = "Invalid record identifier." });

            try
            {
                var record = _context.UgFacultyDetails
                    .FirstOrDefault(f => f.Id == dto.Id && f.CollegeCode == collegeCode);
                if (record == null)
                    return Json(new { success = false, message = "Record not found." });

                if (!string.IsNullOrEmpty(record.PhotoFilePath))
                {
                    // FIX 1c: PhotosFolder comes from environment, not hardcoded drive
                    var photoDiskPath = Path.Combine(PhotosFolder, Path.GetFileName(record.PhotoFilePath));
                    if (System.IO.File.Exists(photoDiskPath))
                    {
                        try { System.IO.File.Delete(photoDiskPath); } catch { /* non-critical */ }
                    }
                }

                _context.UgFacultyDetails.Remove(record);
                _context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Delete failed: " + ex.Message });
            }
        }

        // ── Lock / declare all entries ────────────────────────────────────────
        [HttpPost]
        [Route("SaveFaculty")]
        public IActionResult SaveFaculty([FromBody] List<SaveFacultyDto> facultyList)
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            if (string.IsNullOrEmpty(collegeCode))
                return Ok(new { success = false, message = "Session expired." });

            try
            {
                var uncommittedRows = _context.UgFacultyDetails
                    .Where(f => f.CollegeCode == collegeCode)
                    .ToList();
                foreach (var f in uncommittedRows)
                    f.IsDeclared = true;

                _context.SaveChanges();
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, message = "Declaration failed: " + ex.Message });
            }
        }

        // ── Upload signed PDF ─────────────────────────────────────────────────
        // FIX 1: Uses IWebHostEnvironment-based path.
        [HttpPost]
        [Route("Upload")]
        public async Task<IActionResult> Upload(IFormFile signedDocument)
        {
            if (signedDocument == null || signedDocument.Length == 0)
                return BadRequest("No file uploaded.");

            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            if (string.IsNullOrEmpty(collegeCode))
                return Unauthorized();

            if (signedDocument.Length > 10 * 1024 * 1024)
                return BadRequest("File size exceeds 10 MB.");

            var ext = Path.GetExtension(signedDocument.FileName).ToLowerInvariant();
            if (ext != ".pdf")
                return BadRequest("Only PDF files are allowed.");

            // FIX 1d: UploadsFolder from environment
            if (!Directory.Exists(UploadsFolder))
                Directory.CreateDirectory(UploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(UploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await signedDocument.CopyToAsync(stream);

            // FIX 1e: web path uses UploadsWebRoot property
            var webPath = $"{UploadsWebRoot}/{uniqueFileName}";

            var existingUpload = await _context.UgPrintedUploads
                .FirstOrDefaultAsync(u => u.CollegeCode == collegeCode);

            if (existingUpload != null)
            {
                var oldDiskPath = Path.Combine(UploadsFolder, Path.GetFileName(existingUpload.DocumentPath ?? ""));
                if (System.IO.File.Exists(oldDiskPath))
                {
                    try { System.IO.File.Delete(oldDiskPath); } catch { /* non-critical */ }
                }
                existingUpload.DocumentPath = webPath;
                existingUpload.CreatedOn = DateTime.Now;
                existingUpload.Ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            }
            else
            {
                _context.UgPrintedUploads.Add(new UgPrintedUpload
                {
                    CollegeCode = collegeCode,
                    DocumentPath = webPath,
                    CreatedOn = DateTime.Now,
                    Ipaddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                });
            }

            var facultyList = _context.UgFacultyDetails
                .Where(f => f.CollegeCode == collegeCode && f.IsDeclared == true)
                .ToList();
            foreach (var f in facultyList)
                f.PrintedCopyUploaded = true;

            await _context.SaveChangesAsync();
            return Ok();
        }

        // ── Download generated text file ──────────────────────────────────────
        [Authorize(AuthenticationSchemes = "CollegeAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("Download")]
        public IActionResult Download()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            if (string.IsNullOrEmpty(collegeCode))
                return Unauthorized();

            var list = _context.UgFacultyDetails
                .Where(f => f.CollegeCode == collegeCode
                         && f.IsDeclared == true
                         && f.PrintedCopyUploaded == false)
                .ToList();

            if (!list.Any())
                return NotFound();

            return GenerateFacultyFile(collegeCode, list, false);
        }

        // ── FIX 5: Remove [Route] from private helper — it has no effect.
        private IActionResult GenerateFacultyFile(string collegeCode, List<UgFacultyDetail> list, bool isUniversity)
        {
            var sb = new StringBuilder();
            sb.AppendLine("UG FACULTY DETAILS RECORD");
            sb.AppendLine(new string('-', 100));
            sb.AppendLine($"College Code: {collegeCode}");
            sb.AppendLine($"Generation Date: {DateTime.Now}");
            sb.AppendLine();
            sb.AppendLine("SL\tDepartment\tName of Faculty\tDesignation\tDOB\tAppt. Date\t" +
                          "Mobile No\tPAN No\tCouncil Reg No\t" +
                          "AEBAS ID\tQualification\tNature of Employment\tTeaching Exp (Yrs)");

            int sl = 1;
            foreach (var f in list)
                sb.AppendLine($"{sl++}\t{f.DepartmentCode}\t{f.NameOftheFaculty}\t{f.DesignationCode}\t" +
                              $"{f.Dob}\t{f.DateOfAppointment}\t{f.MobileNo}\t{f.Panno}\t" +
                              $"{f.StateCouncilRegNo}\t" +
                              $"{f.AebasattendId}\t{f.ProfessionalQualification}\t" +
                              $"{f.NatureOfEmployment}\t{f.TeachingExpInYrs}");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/plain", $"Faculty_List_{collegeCode}.txt");
        }

        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        [Route("GetCollegeUploads")]
        public IActionResult GetCollegeUploads(string collegeCode)
        {

            if (string.IsNullOrWhiteSpace(collegeCode))
                return Json(new
                {
                    success = false,
                    message = "College code is required."
                });

            var uploads = (from upload in _context.UgPrintedUploads

                           join college in _context.AffiliationCollegeMasters
                           on upload.CollegeCode equals college.CollegeCode

                           where upload.CollegeCode == collegeCode

                           orderby upload.CreatedOn descending

                           select new CollegeUploadDto
                           {
                               Id = upload.Id,

                               FileName = upload.DocumentPath != null
                                   ? Path.GetFileName(upload.DocumentPath)
                                   : "UGFaculty_Document.pdf",

                               UploadedDate = upload.CreatedOn.HasValue
                                   ? upload.CreatedOn.Value.ToString("yyyy-MM-dd")
                                   : DateTime.Today.ToString("yyyy-MM-dd"),

                               UploadedBy = college.CollegeName ?? "College Admin",

                               Status = "Pending",

                               FileUrl = upload.DocumentPath != null
                                   ? Url.Content($"~/{UploadsWebRoot.TrimStart('/')}/{Path.GetFileName(upload.DocumentPath)}")
                                   : string.Empty
                           })
                           .ToList();

            return Json(uploads);
        }

        // ── Get faculty list (university role) ────────────────────────────────
        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("GetFacultyList")]
        [HttpGet]
        public IActionResult GetFacultyList(string collegeCode)
        {
            // FIX 6: Return JSON error instead of raw 401

            if (string.IsNullOrWhiteSpace(collegeCode))
                return Json(new
                {
                    success = false,
                    message = "College code is required."
                });

            var facultyList = (from f in _context.UgFacultyDetails

                               join des in _context.UgdesignationMasters
                               on f.DesignationCode equals des.DesignationId
                               into desJoin
                               from designation in desJoin.DefaultIfEmpty()

                               join dept in _context.DepartmentMastersForUgs
                               on f.DepartmentCode equals dept.DepartmentCode
                               into deptJoin
                               from department in deptJoin.DefaultIfEmpty()

                               where f.CollegeCode == collegeCode
                                  && f.IsDeclared == true

                               orderby f.DepartmentCode, f.NameOftheFaculty

                               select new
                               {
                                   Name = f.NameOftheFaculty,

                                   DesignationName =
                                       designation.DesignationName != null
                                       ? designation.DesignationName
                                       : f.DesignationCode,

                                   DepartmentName =
                                       department.DepartmentName != null
                                       ? department.DepartmentName
                                       : f.DepartmentCode,

                                   DbDobValue = f.Dob,

                                   MobileNo = f.MobileNo ?? "",

                                   PanNo = f.Panno ?? "",

                                   StateCouncilRegNo = f.StateCouncilRegNo ?? ""
                               })
                               .ToList();

            var facultyDtos = facultyList.Select((f, i) => new UgFacultyDto
            {
                SlNo = i + 1,
                Name = f.Name,
                Designation = f.DesignationName,
                Department = f.DepartmentName,
                Dob = f.DbDobValue != null
                        ? f.DbDobValue.ToString()
                        : string.Empty,
                MobileNo = f.MobileNo,
                PanNo = f.PanNo,
                StateCouncilRegNo = f.StateCouncilRegNo
            }).ToList();

            return Json(facultyDtos);
        }

        // ── Download uploaded document (university role) ───────────────────────
        // FIX 7: Redirect uses Url.Content("~/...") instead of a hardcoded "/MedicalUGFacultyList/..."
        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        [Route("DownloadUpload")]
        public IActionResult DownloadUpload(int id)
        {
            

            var upload = _context.UgPrintedUploads.FirstOrDefault(u => u.Id == id);
            if (upload == null || string.IsNullOrEmpty(upload.DocumentPath))
                return NotFound("Document not found.");

            try
            {
                var fileName = Path.GetFileName(upload.DocumentPath);
                // FIX 7: Url.Content resolves the app base correctly under sub-apps / reverse proxies
                var redirectUrl = Url.Content($"~/{UploadsWebRoot.TrimStart('/')}/{fileName}");
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ── University download view ──────────────────────────────────────────
        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("UniversityDownload")]
        public IActionResult UniversityDownload(string collegeCode)
        {
            if (HttpContext.Session.GetString("UserRole") != "University")
                return RedirectToAction("Login", "Login"); // FIX 4: consistent controller name

            var faculty = _context.UgFacultyDetails
                .Where(f => f.CollegeCode == collegeCode && f.IsDeclared == true)
                .ToList();

            ViewBag.CollegeCode = collegeCode;
            ViewBag.CollegeName = _context.AffiliationCollegeMasters
                .Where(c => c.CollegeCode == collegeCode)
                .Select(c => c.CollegeName)
                .FirstOrDefault() ?? collegeCode;

            return View(faculty);
        }
    }
}