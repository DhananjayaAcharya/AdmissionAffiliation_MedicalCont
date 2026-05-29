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

        // ===== BASE STORAGE PATH (outside wwwroot — secure) =====
        private readonly string BaseFolder = @"E:\MedicalUGFacultyList";

        // ===== PHOTO FOLDER =====
        private string PhotosFolder => Path.Combine(BaseFolder, "Photos");

        // ===== PDF / DOCUMENT FOLDER =====
        private string UploadsFolder => BaseFolder;

        // ===== WEB ROUTE PREFIX (used only for PDF links) =====
        private string UploadsWebRoot => "/MedicalUGFacultyList";

        public UGFacultyController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ─────────────────────────────────────────────────────────────────────
        // NEW: Serve faculty photos through controller (bypasses IIS static file
        //      restriction for files stored outside wwwroot)
        // URL: /UGFaculty/Photo?file=guid.jpg
        // ─────────────────────────────────────────────────────────────────────
        [HttpGet]
        [Route("Photo")]
        public IActionResult Photo(string file)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(file))
                return NotFound();

            // Normalize slashes
            file = file.Replace("\\", "/");

            // Extract only filename
            // Handles:
            // MedicalUGFacultyList/Photos/guid.jpg
            // E:/MedicalUGFacultyList/Photos/guid.jpg
            // guid.jpg
            string safeFileName = Path.GetFileName(file);

            if (string.IsNullOrWhiteSpace(safeFileName))
                return NotFound();

            // Allowed extensions
            string[] allowedExts = { ".jpg", ".jpeg", ".png" };

            string ext = Path.GetExtension(safeFileName).ToLowerInvariant();

            if (!allowedExts.Contains(ext))
                return NotFound();

            // Physical file path
            string fullPath = Path.Combine(PhotosFolder, safeFileName);

            // File check
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            // MIME type
            string mimeType = ext switch
            {
                ".png" => "image/png",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                _ => "application/octet-stream"
            };

            // Return image
            return PhysicalFile(fullPath, mimeType);
        }

        // ─────────────────────────────────────────────────────────────────────
        [Authorize(AuthenticationSchemes = "CollegeAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("AddFaculty")]
        public IActionResult AddFaculty()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            var collegeName = User.Identity?.Name;
            ViewBag.CollegeName = collegeName;

            if (string.IsNullOrEmpty(collegeCode))
                return RedirectToAction("Login", "Login");

            var college = _context.AffiliationCollegeMasters
                .FirstOrDefault(c => c.CollegeCode == collegeCode);
            if (college == null)
                return RedirectToAction("Dashboard", "Home");

            bool isLocked = _context.UgPrintedUploads
                .Any(x => x.CollegeCode == collegeCode);

            ViewBag.IsLocked = isLocked;
            ViewBag.Departments = _context.DepartmentMastersForUgs.ToList();
            ViewBag.Designations = _context.UgdesignationMasters.ToList();

            return View();
        }

        // ─────────────────────────────────────────────────────────────────────
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
                            join desig in _context.UgdesignationMasters
                                on faculty.DesignationCode equals desig.DesignationId
                            where faculty.CollegeCode == collegeCode
                            select new
                            {
                                Faculty = faculty,
                                DeptName = dept.DepartmentName,
                                DesigName = desig.DesignationName
                            })
                            .ToList().Distinct();

                var rows = data
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
                        // departmentCode = g.Key.Department,
                        departmentCode = f.Faculty.DepartmentCode,
                        departmentName = g.Key.Department,

                        designationCode = f.Faculty.DesignationCode,
                        designationName = g.Key.Designation,
                        // designationCode = g.Key.Designation,
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
                        // KEY FIX: return a controller-served URL, not the raw file path
                        photoFilePath = BuildPhotoUrl(f.Faculty.PhotoFilePath)
                    }))
                    .OrderBy(x => x.departmentCode)
                    .ThenBy(x => x.designationOrder)
                    .ToList();

                return Json(new { success = true, data = rows });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
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
                               join desig in _context.UgdesignationMasters
                                   on faculty.DesignationCode equals desig.DesignationId
                               where faculty.CollegeCode == collegeCode
                                  && faculty.IsDeclared == true
                               select new
                               {
                                   Id = faculty.Id,

                                   DepartmentCode = faculty.DepartmentCode,
                                   DepartmentName = dept.DepartmentName ?? faculty.DepartmentCode,

                                   DesignationCode = faculty.DesignationCode,
                                   DesignationName = desig.DesignationName ?? faculty.DesignationCode,

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
                               .ToList().Distinct();

                var rows = rawData
                    .OrderBy(x => x.DepartmentName)
                    .ThenBy(x => x.DesignationOrder)
                    .Select((x, index) => new
                    {
                        slNo = index + 1,
                        id = x.Id,
                        departmentCode = x.DepartmentCode,
                        departmentName = x.DepartmentName,

                        designationCode = x.DesignationCode,
                        designationName = x.DesignationName,
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
                        // KEY FIX: return a controller-served URL, not the raw file path
                        photoFilePath = BuildPhotoUrl(x.PhotoFilePath)
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
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────────────
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

            if (photo.Length > 1 * 1024 * 1024)
                return Json(new { success = false, message = "Photo must be under 1 MB." });

            if (!Directory.Exists(PhotosFolder))
                Directory.CreateDirectory(PhotosFolder);

            var record = _context.UgFacultyDetails.FirstOrDefault(f => f.Id == facultyId);
            if (record == null)
                return Json(new { success = false, message = "Faculty record not found." });

            // Delete old photo file from disk if one exists
            if (!string.IsNullOrEmpty(record.PhotoFilePath))
            {
                var oldFileName = Path.GetFileName(record.PhotoFilePath);
                var oldDiskPath = Path.Combine(PhotosFolder, oldFileName);
                if (System.IO.File.Exists(oldDiskPath))
                {
                    try { System.IO.File.Delete(oldDiskPath); } catch { /* non-critical */ }
                }
            }

            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var fullSavePath = Path.Combine(PhotosFolder, uniqueName);

            using (var stream = new FileStream(fullSavePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            // KEY FIX: store only the filename in the DB — not a path or URL.
            // The Photo() action above serves it via /UGFaculty/Photo?file=guid.jpg
            record.PhotoFilePath = uniqueName;
            _context.SaveChanges();

            return Json(new
            {
                success = true,
                // Return the ready-to-use URL so the JS can display it immediately
                path = Url.Action("Photo", "UGFaculty", new { file = uniqueName }, Request.Scheme)
            });
        }

        // ─────────────────────────────────────────────────────────────────────
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
                    var safeFileName = Path.GetFileName(record.PhotoFilePath);
                    var photoDiskPath = Path.Combine(PhotosFolder, safeFileName);
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

        // ─────────────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────────────
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

            if (!Directory.Exists(UploadsFolder))
                Directory.CreateDirectory(UploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(UploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await signedDocument.CopyToAsync(stream);
            }

            var webPath = $"{UploadsWebRoot}/{uniqueFileName}";

            var existingUpload = await _context.UgPrintedUploads
                .FirstOrDefaultAsync(u => u.CollegeCode == collegeCode);

            if (existingUpload != null)
            {
                var oldFileName = Path.GetFileName(existingUpload.DocumentPath ?? "");
                var oldDiskPath = Path.Combine(UploadsFolder, oldFileName);
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

        // ─────────────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────────────
        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpGet]
        [Route("GetCollegeUploads")]
        public IActionResult GetCollegeUploads(string collegeCode)
        {
            if (string.IsNullOrWhiteSpace(collegeCode))
                return Json(new { success = false, message = "College code is required." });

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

        // ─────────────────────────────────────────────────────────────────────
        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("GetFacultyList")]
        [HttpGet]
        public IActionResult GetFacultyList(string collegeCode)
        {
            if (string.IsNullOrWhiteSpace(collegeCode))
                return Json(new { success = false, message = "College code is required." });

            var rawFaculty = (from f in _context.UgFacultyDetails.AsNoTracking()
                              join des in _context.UgdesignationMasters.AsNoTracking()
                                  on f.DesignationCode equals des.DesignationId into desJoin
                              from des in desJoin.DefaultIfEmpty()
                              join dept in _context.DepartmentMastersForUgs.AsNoTracking()
                                  on f.DepartmentCode equals dept.DepartmentCode into deptJoin
                              from dept in deptJoin.DefaultIfEmpty()
                              where f.CollegeCode == collegeCode && f.IsDeclared == true
                              orderby dept.DepartmentName, f.DesignationCode, f.NameOftheFaculty
                              select new
                              {
                                  f.NameOftheFaculty,
                                  Designation = des != null ? des.DesignationName : null,
                                  f.DesignationCode,
                                  Department = dept != null ? dept.DepartmentName : null,
                                  f.DepartmentCode,
                                  f.AebasattendId,
                                  f.StateCouncilRegNo,
                                  f.PhotoFilePath,
                                  f.Dob
                              }).ToList();

            var facultyDtos = rawFaculty
      .GroupBy(f => new
      {
          Name = (f.NameOftheFaculty ?? "").Trim(),
          DesignationCode = (f.DesignationCode ?? "").Trim(),
          DepartmentCode = (f.DepartmentCode ?? "").Trim(),
          AEBASAttendId = (f.AebasattendId ?? "").Trim(),
          StateCouncilRegNo = (f.StateCouncilRegNo ?? "").Trim(),
          Dob = (f.Dob ?? "").Trim()
      })
      .Select(g => g.First())
      .OrderBy(f => f.Department ?? f.DepartmentCode)
      .ThenBy(f => f.DesignationCode)
      .ThenBy(f => f.NameOftheFaculty)
      .Select((f, i) => new
      {
          SlNo = i + 1,
          Name = f.NameOftheFaculty ?? "",
          Designation = f.Designation ?? f.DesignationCode ?? "",
          DesignationCode = f.DesignationCode ?? "",
          Department = f.Department ?? f.DepartmentCode ?? "",
          AEBASAttendId = f.AebasattendId ?? "",
          StateCouncilRegNo = f.StateCouncilRegNo ?? "",
          PhotoFilePath = BuildPhotoUrl(f.PhotoFilePath),
          Dob = f.Dob ?? ""
      })
      .ToList();

            return Json(facultyDtos);
        }
        // ─────────────────────────────────────────────────────────────────────
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
                var redirectUrl = Url.Content($"~/{UploadsWebRoot.TrimStart('/')}/{fileName}");
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("UniversityDownload")]
        public IActionResult UniversityDownload(string collegeCode)
        {
            if (HttpContext.Session.GetString("UserRole") != "University")
                return RedirectToAction("Login", "Login");

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

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts whatever is stored in PhotoFilePath to a usable URL.
        /// Handles three cases:
        ///   1. Already a full URL          → return as-is
        ///   2. Just a filename (new style) → /UGFaculty/Photo?file=guid.jpg
        ///   3. Old absolute Windows path   → extract filename, return Photo action URL
        /// </summary>
        private string BuildPhotoUrl(string storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
                return string.Empty;

            // Normalize slashes
            storedPath = storedPath.Replace("\\", "/").Trim();

            // Extract filename only
            string fileName = Path.GetFileName(storedPath);

            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            // Return controller URL
            return Url.Action(
                action: "Photo",
                controller: "UGFaculty",
                values: new { file = fileName }
            ) ?? string.Empty;
        }
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
                              $"{f.StateCouncilRegNo}\t{f.AebasattendId}\t{f.ProfessionalQualification}\t" +
                              $"{f.NatureOfEmployment}\t{f.TeachingExpInYrs}");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/plain", $"Faculty_List_{collegeCode}.txt");
        }


        [Authorize(AuthenticationSchemes = "AdminAuth")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost]
        [Route("SaveReferenceId")]
        public IActionResult SaveReferenceId([FromBody] SaveReferenceDto dto)
        {
            if (dto == null
                || dto.Id <= 0
                || string.IsNullOrWhiteSpace(dto.CollegeCode)
                || string.IsNullOrWhiteSpace(dto.ReferenceId)
                || string.IsNullOrWhiteSpace(dto.EofficeNo))
            {
                return Json(new { success = false, message = "Invalid payload." });
            }

            var upload = _context.UgPrintedUploads
                .FirstOrDefault(x => x.Id == dto.Id && x.CollegeCode == dto.CollegeCode);

            if (upload == null)
                return Json(new { success = false, message = "Upload record not found for this college." });

            upload.ReferenceId = dto.ReferenceId.Trim();
            upload.EofficeNo = dto.EofficeNo.Trim();   // NEW
            _context.SaveChanges();

            return Json(new { success = true });
        }

        public class SaveReferenceDto
        {
            public int Id { get; set; }
            public string? CollegeCode { get; set; }
            public string? ReferenceId { get; set; }
            public string? EofficeNo { get; set; }
        }


        public IActionResult FacultyList()
        {
            ViewBag.CollegeName =
                HttpContext.Session.GetString("CollegeName") ?? "";

            ViewBag.CollegeCode =
                HttpContext.Session.GetString("CollegeCode") ?? "";

            return View();

        }
    }
}