using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class FacultyDetailsController : BaseController
    {

        private readonly ApplicationDbContext _context;

        public FacultyDetailsController(ApplicationDbContext context)
        {
            _context = context;
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

            var departments = _context.MstCourses
                .Where(e => e.FacultyCode.ToString() == facultyCode)
                .Select(d => new SelectListItem
                {
                    Value = d.CourseCode.ToString(),
                    Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
                })
                .ToList();

            return (subjects, designations, departments);
        }

        // ──────────────────────────────────────────────────────────
        //  GET
        // ──────────────────────────────────────────────────────────
        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public IActionResult Repo_FacultyDetails()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
            {
                TempData["Error"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            var (subjectsList, designationsList, departmentsList) = GetDropdowns(facultyCode);

            // ✅ FIX 1: Exclude already-removed records
            var facultyDetails = _context.FacultyDetails
                .Where(f => f.CollegeCode == collegeCode
                         && f.FacultyCode == facultyCode
                         && f.IsRemoved != true)
                .ToList();

            var ahsFacultyWithCollege = _context.NursingFacultyWithColleges
                .Where(f => f.CollegeCode == collegeCode
                         && f.FacultyCode.ToString() == facultyCode)
                .ToList();

            var vmList = new List<FacultyDetailsViewModel>();

            if (!facultyDetails.Any() && !ahsFacultyWithCollege.Any())
            {
                TempData["Info"] = "No faculty records found for this faculty.";
                vmList.Add(new FacultyDetailsViewModel
                {
                    Subjects = subjectsList,
                    Designations = designationsList,
                    DepartmentDetails = departmentsList
                });
                return View(vmList);
            }

            // ✅ FIX 2: Join existing DB records with college data
            vmList = (from f1 in facultyDetails
                      join f2 in ahsFacultyWithCollege
                          on new { f1.Aadhaar, f1.Pan, f1.Designation }
                          equals new
                          {
                              Aadhaar = f2.AadhaarNumber,
                              Pan = f2.Pannumber,
                              Designation = f2.Designation
                          }
                          into gj
                      from sub in gj.DefaultIfEmpty()
                      select new FacultyDetailsViewModel
                      {
                          FacultyDetailId = f1.Id,           // ✅ Always carry DB Id
                          NameOfFaculty = sub?.TeachingFacultyName ?? f1.NameOfFaculty,
                          Designation = sub?.Designation ?? f1.Designation,
                          Aadhaar = sub?.AadhaarNumber ?? f1.Aadhaar,
                          PAN = sub?.Pannumber ?? f1.Pan,
                          DepartmentDetail = f1.DepartmentDetails,
                          SelectedDepartment = f1.DepartmentDetails,
                          RecognizedPGTeacher = f1.RecognizedPgTeacher,
                          Mobile = f1.Mobile,
                          Email = f1.Email,
                          Subjects = subjectsList,
                          Designations = designationsList,
                          DepartmentDetails = departmentsList,
                          RecognizedPhDTeacher = f1.RecognizedPhDteacher,
                          LitigationPending = f1.LitigationPending,
                          PhDRecognitionDocData = f1.PhDrecognitionDocPath,
                          LitigationDocData = f1.LitigationDocPath,
                          PGRecognitionDocData = f1.GuideRecognitionDocPath,
                          IsExaminer = f1.IsExaminer,
                          ExaminerFor = f1.ExaminerFor,
                          ExaminerForList = !string.IsNullOrEmpty(f1.ExaminerFor)
                                                    ? f1.ExaminerFor.Split(',').ToList()
                                                    : new List<string>(),
                          RemoveRemarks = f1.RemoveRemarks
                      }).ToList();

            // ✅ FIX 3: For missingFaculty — try to find existing DB Id by Aadhaar+PAN
            var missingFaculty = ahsFacultyWithCollege
                .Where(f2 => !vmList.Any(v =>
                        v.Aadhaar == f2.AadhaarNumber &&
                        v.PAN == f2.Pannumber))
                .Select(f2 =>
                {
                    // Look up DB record by Aadhaar + PAN to get the Id
                    var dbRecord = facultyDetails.FirstOrDefault(f =>
                        f.Aadhaar == f2.AadhaarNumber &&
                        f.Pan == f2.Pannumber);

                    return new FacultyDetailsViewModel
                    {
                        FacultyDetailId = dbRecord?.Id ?? 0,   // ✅ Carry Id if found
                        NameOfFaculty = f2.TeachingFacultyName,
                        Designation = f2.Designation,
                        Aadhaar = f2.AadhaarNumber,
                        PAN = f2.Pannumber,
                        Subjects = subjectsList,
                        Designations = designationsList,
                        DepartmentDetails = departmentsList
                    };
                })
                .ToList();

            vmList.AddRange(missingFaculty);

            if (!vmList.Any())
            {
                vmList.Add(new FacultyDetailsViewModel
                {
                    Subjects = subjectsList,
                    Designations = designationsList,
                    DepartmentDetails = departmentsList
                });
            }

            return View(vmList);
        }

        private async Task<string?> SaveFacultyFileAsync(IFormFile? file, string subFolder)
        {
            if (file == null || file.Length == 0)
                return null;

            string basePath = @"D:\Affiliation_Medical\FacultyDetails";
            string fullFolder = Path.Combine(basePath, subFolder);

            if (!Directory.Exists(fullFolder))
                Directory.CreateDirectory(fullFolder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string fullPath = Path.Combine(fullFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fullPath;
        }

        // ──────────────────────────────────────────────────────────
        //  POST
        // ──────────────────────────────────────────────────────────
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

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var existingFaculty = _context.FacultyDetails
                    .Where(f => f.CollegeCode == collegeCode && f.FacultyCode == facultyCode)
                    .ToList();

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

                    var guidePath = await SaveFacultyFileAsync(m.GuideRecognitionDoc, "GuideDocs");
                    var phdPath = await SaveFacultyFileAsync(m.PhDRecognitionDoc, "PhDDocs");
                    var litigPath = await SaveFacultyFileAsync(m.LitigationDoc, "LitigationDocs");
                    string examinerFor = m.ExaminerForList != null && m.ExaminerForList.Any()
                                            ? string.Join(",", m.ExaminerForList)
                                            : null;

                    var existing = FindExistingRecord(existingFaculty, m);

                    if (existing != null)
                    {
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

                        if (guidePath != null) existing.GuideRecognitionDocPath = guidePath;
                        if (phdPath != null) existing.PhDrecognitionDocPath = phdPath;
                        if (litigPath != null) existing.LitigationDocPath = litigPath;

                        _context.FacultyDetails.Update(existing);
                    }
                    else
                    {
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

                _context.SaveChanges();
                transaction.Commit();

                //return RedirectToAction("Aff_HostelDetails", "ContinuesAffiliation_Facultybased");
                return RedirectToAction("Dean_DirectorDetails", "ContinuesAffiliation_Facultybased");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["Error"] = "Error saving faculty records: " + ex.Message;
                return RedirectToAction(nameof(Repo_FacultyDetails));

                //return RedirectToAction("Aff_HostelDetails", "ContinuesAffiliation_Facultybased");

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
    }
}
