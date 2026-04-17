using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class Aff_HomeopathyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Aff_HomeopathyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Homeopathy_BasicDetails()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCodeString = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(facultyCodeString))
                return RedirectToAction("Login", "Account");

            var nc = _context.NursingCollegeRegistrations
                .FirstOrDefault(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCodeString);

            var ni = _context.MedicalInstituteDetails
                .FirstOrDefault(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCodeString);

            var viewModel = new NursingInstituteDetailViewModel
            {
                CollegeCode = ni?.CollegeCode,
                FacultyCode = ni?.FacultyCode,
                InstituteName = ni?.InstituteName ?? HttpContext.Session.GetString("CollegeName"),
                InstituteAddress = ni?.InstituteAddress,
                TrustSocietyName = ni?.TrustSocietyName,
                YearOfEstablishmentOfTrust = DateOnly.TryParse(ni?.YearOfEstablishmentOfTrust, out var y1)
                                                ? y1 : (DateOnly?)null,
                YearOfEstablishmentOfCollege = DateOnly.TryParse(ni?.YearOfEstablishmentOfCollege, out var y2)
                                                ? y2 : (DateOnly?)null,
                InstitutionType = ni?.InstitutionType,
                HodofInstitution = ni?.HodofInstitution,
                Dob = ni?.Dob,
                Age = ni?.Age,
                TeachingExperience = ni?.TeachingExperience,
                Degree = (!string.IsNullOrEmpty(ni?.PgDegree))
                    ? ni.PgDegree.Split(',').Select(d => d.Trim()).ToList()
                    : new List<string>(),
                CourseCode = ni?.Course,
                CourseSelectedSpecialities = ni?.SelectedSpecialities,
                OtherDegreeText = ni?.OtherDegree,
                TrustDocData = ni?.TrustDoc,
                EstablishmentDocData = ni?.EshtablishmentDoc,

                // ── District & Taluk (edit mode restore) ──
                SelectedDistrictId = ni?.District,
                SelectedTalukId = ni?.Taluk,

                // ── District Dropdown ──
                DistrictDropdownList = _context.DistrictMasters
                    .Where(d => d.DistrictName != null && d.DistrictName != "")
                    .OrderBy(d => d.DistrictName)
                    .Select(d => new SelectListItem
                    {
                        Value = d.DistrictName,
                        Text = d.DistrictName
                    }).ToList(),

                // ── Taluk Dropdown (full list; AJAX filters by district) ──
                TalukDropdownList = _context.TalukMasters
                    .OrderBy(t => t.TalukName)
                    .Select(t => new SelectListItem
                    {
                        Value = t.TalukId.ToString(),
                        Text = t.TalukName
                    }).ToList(),
            };

            if (viewModel.CollegeCode == null && viewModel.FacultyCode == null)
            {
                viewModel = new NursingInstituteDetailViewModel
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCodeString,
                    InstituteName = HttpContext.Session.GetString("CollegeName"),
                    Degree = new List<string>(),

                    // ── District Dropdown for fresh form ──
                    DistrictDropdownList = _context.DistrictMasters
                        .Where(d => d.DistrictName != null && d.DistrictName != "")
                        .OrderBy(d => d.DistrictName)
                        .Select(d => new SelectListItem
                        {
                            Value = d.DistrictName,
                            Text = d.DistrictName
                        }).ToList(),

                    TalukDropdownList = _context.TalukMasters
                        .OrderBy(t => t.TalukName)
                        .Select(t => new SelectListItem
                        {
                            Value = t.TalukId.ToString(),
                            Text = t.TalukName
                        }).ToList(),
                };
            }

            ViewBag.Courses = _context.MstCourses
                .Where(c => c.FacultyCode.ToString() == facultyCodeString)
                .OrderBy(c => c.CourseName)
                .ToList();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult GetTaluksByDistrict(string districtName)
        {
            if (string.IsNullOrEmpty(districtName))
                return Json(new List<object>());

            try
            {
                // Step 1: Check what DistrictMasters has for this name
                var district = _context.DistrictMasters
                    .Where(d => d.DistrictName == districtName)
                    .Select(d => new { d.DistrictId, d.DistrictName })
                    .FirstOrDefault();

                if (district == null)
                    return Json(new { error = $"No district found with name '{districtName}'" });

                // Step 2: Check TalukMasters using that DistrictId
                var taluks = _context.TalukMasters
                    .Where(t => t.DistrictId == district.DistrictId)  // join via int FK
                    .OrderBy(t => t.TalukName)
                    .Select(t => new
                    {
                        value = t.TalukId.ToString(),
                        text = t.TalukName
                    })
                    .ToList();

                return Json(taluks);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Homeopathy_BasicDetails(NursingInstituteDetailViewModel model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCodeString = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(facultyCodeString))
                return RedirectToAction("Login", "Account");

            var existingEntity = _context.MedicalInstituteDetails
                .FirstOrDefault(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCodeString);

            if (existingEntity != null)
            {
                // ── Basic Info ──
                existingEntity.InstituteAddress = model.InstituteAddress;
                existingEntity.TrustSocietyName = model.TrustSocietyName;
                existingEntity.YearOfEstablishmentOfTrust = model.YearOfEstablishmentOfTrust?.ToString();
                existingEntity.YearOfEstablishmentOfCollege = model.YearOfEstablishmentOfCollege?.ToString();
                existingEntity.InstitutionType = model.InstitutionType;
                existingEntity.HodofInstitution = model.HodofInstitution;
                existingEntity.Dob = model.Dob;
                existingEntity.Age = model.Age;
                existingEntity.TeachingExperience = model.TeachingExperience;
                existingEntity.PgDegree = model.Degree != null ? string.Join(",", model.Degree) : "";
                existingEntity.SelectedSpecialities = model.CourseSelectedSpecialities;
                existingEntity.Course = model.CourseCode;
                existingEntity.OtherDegree = model.OtherDegreeText;

                // ── District & Taluk ──
                existingEntity.District = model.SelectedDistrictId;
                existingEntity.Taluk = model.SelectedTalukId;

                // ── Trust Document ──
                if (model.TrustEstablishmentDocument != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        model.TrustEstablishmentDocument.CopyTo(ms);
                        existingEntity.TrustDoc = ms.ToArray();
                    }
                }

                // ── Establishment Document ──
                if (model.CollegeEstablishmentDocument != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        model.CollegeEstablishmentDocument.CopyTo(ms);
                        existingEntity.EshtablishmentDoc = ms.ToArray();
                    }
                }

                _context.Update(existingEntity);
            }
            else
            {
                var entity = new MedicalInstituteDetail
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCodeString,
                    InstituteName = model.InstituteName,
                    InstituteAddress = model.InstituteAddress,
                    TrustSocietyName = model.TrustSocietyName,
                    YearOfEstablishmentOfTrust = model.YearOfEstablishmentOfTrust?.ToString(),
                    YearOfEstablishmentOfCollege = model.YearOfEstablishmentOfCollege?.ToString(),
                    InstitutionType = model.InstitutionType,
                    HodofInstitution = model.HodofInstitution,
                    Dob = model.Dob,
                    Age = model.Age,
                    TeachingExperience = model.TeachingExperience,
                    PgDegree = model.Degree != null ? string.Join(",", model.Degree) : "",
                    SelectedSpecialities = model.CourseSelectedSpecialities,
                    Course = model.CourseCode,
                    OtherDegree = model.OtherDegreeText,

                    // ── District & Taluk ──
                    District = model.SelectedDistrictId,
                    Taluk = model.SelectedTalukId,
                };

                // ── Trust Document ──
                if (model.TrustEstablishmentDocument != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        model.TrustEstablishmentDocument.CopyTo(ms);
                        entity.TrustDoc = ms.ToArray();
                    }
                }

                // ── Establishment Document ──
                if (model.CollegeEstablishmentDocument != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        model.CollegeEstablishmentDocument.CopyTo(ms);
                        entity.EshtablishmentDoc = ms.ToArray();
                    }
                }

                _context.Add(entity);
            }

            _context.SaveChanges();

            return RedirectToAction("Homeopathy_UGPG");
        }


        public IActionResult ViewTrustDoc()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var entity = _context.MedicalInstituteDetails
                .FirstOrDefault(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if (entity?.TrustDoc == null || entity.TrustDoc.Length == 0)
                return Content("No document uploaded.");

            return File(entity.TrustDoc, "application/pdf"); // 👈 INLINE VIEW
        }

        public IActionResult ViewEstDoc()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var entity = _context.MedicalInstituteDetails
                .FirstOrDefault(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if (entity?.EshtablishmentDoc == null || entity.EshtablishmentDoc.Length == 0)
                return Content("No document uploaded.");

            return File(entity.EshtablishmentDoc, "application/pdf"); // 👈 INLINE VIEW
        }

        [HttpGet]
        public IActionResult Homeopathy_Institute_Report()
        {
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var model = _context.NursingInstituteDetails
                .FirstOrDefault(x => x.FacultyCode == facultyCode);

            if (model == null)
                return RedirectToAction("Nursing_BasicDetails");

            return View(model);
        }

        public IActionResult PreviousStep()
        {
            // logic to go back to previous page/step
            return RedirectToAction("Dashboard");
        }

        public IActionResult NextStep()
        {
            // logic to go forward to next page/step
            return RedirectToAction("Medical_BasicDetails");
        }


        [HttpGet]
        public async Task<IActionResult> Homeopathy_UGPG()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
            var facultyCodeStr = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(facultyCodeStr) || !int.TryParse(facultyCodeStr, out int facultyCode))
            {
                TempData["ErrorMessage"] = "FacultyCode not found in session or invalid.";
                return RedirectToAction("Error"); // or return the page with the error message
            }

            // Add "Additional" to dropdown options
            ViewBag.FreshIncreaseOptions = new[] { "First Intake", "Increase", "Additional" };

            // Get distinct CourseLevels for this faculty
            var levels = await _context.MstCourses
                .Where(c => c.FacultyCode == facultyCode && !string.IsNullOrEmpty(c.CourseLevel))
                .Select(c => c.CourseLevel.Trim())
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();

            ViewBag.CourseLevels = levels;

            // ✅ Fetch records filtered by CollegeCode
            var repositoryRows = await _context.UgandPgrepositories
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode.ToString())
                .OrderByDescending(x => x.Id)
                .Take(1000)
                .ToListAsync();

            ViewBag.RepositoryRows = repositoryRows;

            return View(new Ayurveda_FacultyIntakeViewModel());
        }

        public async Task<IActionResult> DownloadDoc(int id, string type)
        {
            var row = await _context.UgandPgrepositories.FindAsync(id);
            if (row == null) return NotFound();

            byte[] fileBytes = null;
            string fileName = $"{type}_{id}.pdf"; // can infer real name and mime type if needed
            string contentType = "application/pdf"; // adjust mime type if needed

            switch (type)
            {
                case "RGUHS": fileBytes = row.Rguhsnotification; break;
                case "Ncisc": fileBytes = row.Ncisc; break;
                case "GOK": fileBytes = row.Gok; break;
            }
            if (fileBytes == null) return NotFound("File not found");

            return File(fileBytes, contentType, fileName);
        }

        // AJAX: return courses for a given level
        [HttpGet]
        public async Task<IActionResult> GetCoursesByLevel(string level)
        {
            if (string.IsNullOrWhiteSpace(level))
                return Json(new { success = false, data = new object[0], message = "Level not provided" });

            var facultyCodeStr = HttpContext.Session.GetString("FacultyCode");
            if (string.IsNullOrEmpty(facultyCodeStr) || !int.TryParse(facultyCodeStr, out int facultyCode))
                return Json(new { success = false, data = new object[0], message = "Invalid FacultyCode in session" });

            var courses = await _context.MstCourses
                .Where(c => c.CourseLevel != null &&
                            c.CourseLevel.Trim().ToUpper() == level.Trim().ToUpper() &&
                            c.FacultyCode == facultyCode)
                .Select(c => new { c.CourseCode, c.CourseName })
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            return Json(new { success = true, data = courses });
        }
        [HttpGet]
        public async Task<IActionResult> IsCourseFresh(string courseName)
        {
            var exists = await _context.UgandPgrepositories
                .AnyAsync(r => r.CourseName == courseName && r.FreshOrIncrease == "First Intake");
            return Json(new { exists });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFacultyIntake(int id)
        {
            var row = await _context.UgandPgrepositories.FindAsync(id);
            if (row == null)
                return Json(new { success = false, message = "Record not found." });

            _context.UgandPgrepositories.Remove(row);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // POST: FacultyIntake/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Homeopathy_UGPG(Ayurveda_FacultyIntakeViewModel model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            // If validation error, reload dropdowns
            if (!ModelState.IsValid)
            {
                ViewBag.CourseLevels = await _context.MstCourses
                    .Where(c => !string.IsNullOrEmpty(c.CourseLevel))
                    .Select(c => c.CourseLevel.Trim())
                    .Distinct()
                    .OrderBy(l => l)
                    .ToListAsync();

                ViewBag.FreshIncreaseOptions = new[] { "First Intake", "Increase", "Additional" };
                // Also reload table for view
                ViewBag.RepositoryRows = await _context.UgandPgrepositories
                    .OrderByDescending(x => x.Id)
                    .Take(1000)
                    .ToListAsync();

                return View(model);
            }

            // Find the selected course name for display/save
            var selectedCourseName = await _context.MstCourses
                .Where(c => c.CourseCode.ToString() == model.CourseCode)
                .Select(c => c.CourseName)
                .FirstOrDefaultAsync();

            var entity = new UgandPgrepository
            {
                IntakeDetails = model.IntakeDetails,         // textbox value
                FreshOrIncrease = model.FreshOrIncrease,     // dropdown value
                Course = model.CourseCode,                   // CourseCode from dropdown
                CollegeCode = collegeCode,                   // session college code
                FacultyCode = facultyCode,                   // session faculty code
                CourseName = selectedCourseName,             // store CourseName
            };

            // Helper to convert IFormFile to byte[]
            async Task<byte[]> ToBytes(IFormFile file)
            {
                if (file == null || file.Length == 0) return null;
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                return ms.ToArray();
            }

            entity.Rguhsnotification = await ToBytes(model.RGUHSNotificationFile);
            entity.Ncisc = await ToBytes(model.NciscUploadFile);
            entity.Gok = await ToBytes(model.GOKUploadFile);

            _context.UgandPgrepositories.Add(entity);
            await _context.SaveChangesAsync();

            //TempData["SuccessMessage"] = "Faculty intake saved successfully.";
            return RedirectToAction(nameof(Homeopathy_UGPG));
        }


        [HttpGet]
        public IActionResult Homeopathy_YearwiseMaterials()
        {
            try
            {
                var collegeCode = HttpContext.Session.GetString("CollegeCode");
                var facultyCode = HttpContext.Session.GetString("FacultyCode");

                if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                    return RedirectToAction("Login", "Account");

                // LINQ join for yearwise data
                var model = (from param in _context.MstNursingAffiliatedMaterialData
                             where param.FacultyCode == facultyCode
                             join yw in _context.NursingAffiliatedYearwiseMaterialsData
                                .Where(y => y.CollegeCode == collegeCode && y.FacultyCode == facultyCode)
                                on param.ParametersId equals yw.ParametersId into ywGroup
                             from ywItem in ywGroup.DefaultIfEmpty()
                             select new Nursing_YearwiseMaterialsDataViewModel
                             {
                                 ParametersId = param.ParametersId,
                                 ParametersName = param.ParametersName,
                                 CollegeCode = collegeCode,
                                 FacultyCode = facultyCode,
                                 Year1 = ywItem != null ? ywItem.Year1.ToString() : "0",
                                 Year2 = ywItem != null ? ywItem.Year2.ToString() : "0",
                                 Year3 = ywItem != null ? ywItem.Year3.ToString() : "0"
                             }).ToList();

                // Data table for read-only display
                ViewBag.YearwiseData = _context.NursingAffiliatedYearwiseMaterialsData
     .Where(y => y.CollegeCode == collegeCode && y.FacultyCode == facultyCode)
     .Select(y => new
     {
         CollegeCode = y.CollegeCode,              // <-- add this!
         ParametersId = y.ParametersId,
         ParametersName = y.ParametersName,
         Year1 = y.Year1,
         Year2 = y.Year2,
         Year3 = y.Year3,
         ParentHospitalName = y.ParentHospitalName,
         ParentHospitalAddress = y.ParentHospitalAddress,
         PolutionControlDoc = y.PolutionControlDoc,
         FireSafetyDoc = y.FireSafetyDoc,
         ParentHospitalKpmebedsDoc = y.ParentHospitalKpmebedsDoc,
         BiomedicalWasteManagemenDoc = y.BiomedicalWasteManagemenDoc,
         ParentHospitalPostBasicDoc = y.ParentHospitalPostBasicDoc,
         Kpmebeds = y.Kpmebeds,
         PostBasicBeds = y.PostBasicBeds,
         TotalBeds = y.TotalBeds,
         HospitalOwnerName = y.HospitalOwnerName,
         HospitalType = y.HospitalType,
         opddoc = y.OpdDocument,
         Ipddoc = y.IpdDocument,
         MajorOperationsSurgeries = y.MajorOperationsSurgeries,
         MinorOperationsSurgeries = y.MinorOperationsSurgeries
     }).ToList();


                // Dropdown definitions
                ViewBag.HospitalTypes = new List<SelectListItem>
        {
            new SelectListItem { Value = "Level 2", Text = "Level 2", Selected = true },
            new SelectListItem { Value = "Level 3", Text = "Level 3" },
            new SelectListItem { Value = "Level 4", Text = "Level 4" }
        };

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["Error"] = "Error loading data!";
                return View(new List<Nursing_YearwiseMaterialsDataViewModel>());
            }
        }

        public IActionResult DownloadDocument(int id, string type)
        {
            // Fetch the record by id
            var record = _context.NursingAffiliatedYearwiseMaterialsData.FirstOrDefault(x => x.ParametersId == id);
            if (record == null) return NotFound();

            byte[] fileBytes = null;
            string fileName = "document.pdf";

            // Choose the document based on type
            switch (type)
            {
                case "PolutionControlDoc":
                    fileBytes = record.PolutionControlDoc;
                    fileName = "PolutionControlDoc.pdf";
                    break;
                case "FireSafetyDoc":
                    fileBytes = record.FireSafetyDoc;
                    fileName = "FireSafetyDoc.pdf";
                    break;
                case "BiomedicalWasteManagemenDoc":
                    fileBytes = record.BiomedicalWasteManagemenDoc;
                    fileName = "BiomedicalWasteManagemenDoc.pdf";
                    break;
                case "PostBasic":
                    fileBytes = record.ParentHospitalPostBasicDoc;
                    fileName = "PostBasic.pdf";
                    break;
                case "KpmeBeds":
                    fileBytes = record.ParentHospitalKpmebedsDoc;
                    fileName = "KpmeBeds.pdf";
                    break;
                case "OpdDocument":
                    fileBytes = record.OpdDocument;
                    fileName = "OpdDocument.pdf";
                    break;
                case "IpdDocument":
                    fileBytes = record.IpdDocument;
                    fileName = "IpdDocument.pdf";
                    break;
                case "MajorOperationsSurgeries":
                    fileBytes = record.MajorOperationsSurgeries;
                    fileName = "MajorOperationsSurgeries.pdf";
                    break;
                case "MinorOperationsSurgeries":
                    fileBytes = record.MinorOperationsSurgeries;
                    fileName = "MinorOperationsSurgeries.pdf";
                    break;
                default:
                    return BadRequest();
            }

            if (fileBytes == null || fileBytes.Length == 0)
                return NotFound();

            return File(fileBytes, "application/pdf", fileName);
        }

        [HttpPost]
        public IActionResult DeleteYearwiseMaterial1(int parametersId)
        {
            try
            {
                var collegeCode = HttpContext.Session.GetString("CollegeCode");
                var facultyCode = HttpContext.Session.GetString("FacultyCode");

                if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                    return RedirectToAction("Login", "Account");

                var record = _context.NursingAffiliatedYearwiseMaterialsData.FirstOrDefault(x =>
                    x.ParametersId == parametersId &&
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

                if (record != null)
                {
                    _context.NursingAffiliatedYearwiseMaterialsData.Remove(record);
                    _context.SaveChanges();
                    TempData["Success"] = "Record deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Record not found!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting record: " + ex.Message;
            }

            return RedirectToAction("Homeopathy_YearwiseMaterials");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Homeopathy_YearwiseMaterials(List<YearwiseMaterialViewModel> model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
            {
                TempData["Error"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            if (model == null || model.Count == 0)
            {
                TempData["Error"] = "No data to save.";
                return RedirectToAction("Homeopathy_YearwiseMaterials");
            }

            try
            {
                var form = HttpContext.Request.Form;
                int.TryParse(form["Kpmebeds"], out int kpmeBeds);
                int.TryParse(form["PostBasicBeds"], out int postBasicBeds);
                string parentHospitalName = form["ParentHospitalName"];
                string parentHospitalAddress = form["ParentHospitalAddress"];
                string hospitalOwnerName = form["HospitalOwnerName"];
                string hospitalType = form["HospitalType"];

                // safer file getter
                IFormFile GetFileByName(string name) => HttpContext.Request.Form.Files.GetFile(name);

                byte[] kpmeBedsDoc = GetFileByName("ParentHospitalKpmebedsDocFile")?.Length > 0 ? ConvertFileToBytes(GetFileByName("ParentHospitalKpmebedsDocFile")) : null;
                byte[] postBasicDoc = GetFileByName("ParentHospitalPostBasicDocFile")?.Length > 0 ? ConvertFileToBytes(GetFileByName("ParentHospitalPostBasicDocFile")) : null;
                byte[] polutionControl = GetFileByName("PolutionControlDocFile")?.Length > 0 ? ConvertFileToBytes(GetFileByName("PolutionControlDocFile")) : null;
                byte[] fireSafety = GetFileByName("FireSafetyDocFile")?.Length > 0 ? ConvertFileToBytes(GetFileByName("FireSafetyDocFile")) : null;
                byte[] biomedicalWaste = GetFileByName("BiomedicalWasteManagemenDocFile")?.Length > 0 ? ConvertFileToBytes(GetFileByName("BiomedicalWasteManagemenDocFile")) : null;
                byte[] opdHospitalFile = GetFileByName("OPDDocFile")?.Length > 0 ? ConvertFileToBytes(GetFileByName("OPDDocFile")) : null;
                byte[] ipdHospitalFile = GetFileByName("IPDDocFile")?.Length > 0 ? ConvertFileToBytes(GetFileByName("IPDDocFile")) : null;
                byte[] majorOpsHospitalFile = GetFileByName("MajorOperationsSurgeriesDocFile")?.Length > 0 ? ConvertFileToBytes(GetFileByName("MajorOperationsSurgeriesDocFile")) : null;
                byte[] minorOpsHospitalFile = GetFileByName("MinorOperationsSurgeriesDocFile")?.Length > 0 ? ConvertFileToBytes(GetFileByName("MinorOperationsSurgeriesDocFile")) : null;

                // per-row file
                byte[] GetPerRowFile(int idx)
                {
                    var key = $"[{idx}].UploadDocument";
                    var f = GetFileByName(key);
                    return (f != null && f.Length > 0) ? ConvertFileToBytes(f) : null;
                }

                for (int i = 0; i < model.Count; i++)
                {
                    var item = model[i];
                    var parametersName = string.IsNullOrEmpty(item.ParametersName)
                        ? _context.ClinicalMaterialData.Where(p => p.ParametersId == item.ParametersId).Select(p => p.ParametersName).FirstOrDefault()
                        : item.ParametersName;

                    var existingRecord = _context.NursingAffiliatedYearwiseMaterialsData
                        .FirstOrDefault(y => y.CollegeCode == collegeCode
                                          && y.FacultyCode == facultyCode
                                          && y.ParametersId == item.ParametersId);

                    var perRowFile = GetPerRowFile(i);

                    // read optional explicit target from posted form (recommended to add in view)
                    var explicitTarget = form[$"[{i}].UploadTarget"].FirstOrDefault();

                    void AssignPerRowFileToRecord(NursingAffiliatedYearwiseMaterialsDatum rec, byte[] fileBytes)
                    {
                        if (fileBytes == null) return;

                        // if explicit target provided use it
                        if (!string.IsNullOrWhiteSpace(explicitTarget))
                        {
                            switch (explicitTarget)
                            {
                                case "OpdDocument": rec.OpdDocument = fileBytes; return;
                                case "IpdDocument": rec.IpdDocument = fileBytes; return;
                                case "MajorOperationsSurgeries": rec.MajorOperationsSurgeries = fileBytes; return;
                                case "MinorOperationsSurgeries": rec.MinorOperationsSurgeries = fileBytes; return;
                            }
                        }

                        // fallback: try to infer from parametersName
                        if (!string.IsNullOrEmpty(parametersName) && parametersName.IndexOf("OPD", StringComparison.OrdinalIgnoreCase) >= 0)
                            rec.OpdDocument = fileBytes;
                        else if (!string.IsNullOrEmpty(parametersName) && parametersName.IndexOf("IPD", StringComparison.OrdinalIgnoreCase) >= 0)
                            rec.IpdDocument = fileBytes;
                        else if (!string.IsNullOrEmpty(parametersName) && (parametersName.IndexOf("Major", StringComparison.OrdinalIgnoreCase) >= 0 || parametersName.IndexOf("Surgeries", StringComparison.OrdinalIgnoreCase) >= 0))
                            rec.MajorOperationsSurgeries = fileBytes;
                        else if (!string.IsNullOrEmpty(parametersName) && parametersName.IndexOf("Minor", StringComparison.OrdinalIgnoreCase) >= 0)
                            rec.MinorOperationsSurgeries = fileBytes;
                        else
                        {
                            // fallback: save to OpdDocument to avoid losing file (adjust policy as needed)
                            Console.WriteLine($"Per-row file for ParametersId={item.ParametersId} could not be mapped by ParametersName='{parametersName}'. Saving to OpdDocument as fallback.");
                            rec.OpdDocument = fileBytes;
                        }
                    }

                    if (existingRecord != null)
                    {
                        existingRecord.Year1 = item.Year1;
                        existingRecord.Year2 = item.Year2;
                        existingRecord.Year3 = item.Year3;
                        existingRecord.ParametersName = parametersName;
                        existingRecord.ParentHospitalName = parentHospitalName;
                        existingRecord.ParentHospitalAddress = parentHospitalAddress;
                        existingRecord.HospitalOwnerName = hospitalOwnerName;
                        existingRecord.HospitalType = hospitalType;
                        existingRecord.Kpmebeds = kpmeBeds.ToString();
                        existingRecord.PostBasicBeds = postBasicBeds.ToString();

                        if (kpmeBedsDoc != null) existingRecord.ParentHospitalKpmebedsDoc = kpmeBedsDoc;
                        if (postBasicDoc != null) existingRecord.ParentHospitalPostBasicDoc = postBasicDoc;
                        if (polutionControl != null) existingRecord.PolutionControlDoc = polutionControl;
                        if (fireSafety != null) existingRecord.FireSafetyDoc = fireSafety;
                        if (biomedicalWaste != null) existingRecord.BiomedicalWasteManagemenDoc = biomedicalWaste;

                        if (opdHospitalFile != null) existingRecord.OpdDocument = opdHospitalFile;
                        if (ipdHospitalFile != null) existingRecord.IpdDocument = ipdHospitalFile;
                        if (majorOpsHospitalFile != null) existingRecord.MajorOperationsSurgeries = majorOpsHospitalFile;
                        if (minorOpsHospitalFile != null) existingRecord.MinorOperationsSurgeries = minorOpsHospitalFile;

                        if (perRowFile != null) AssignPerRowFileToRecord(existingRecord, perRowFile);

                        _context.NursingAffiliatedYearwiseMaterialsData.Update(existingRecord);
                    }
                    else
                    {
                        var newRecord = new NursingAffiliatedYearwiseMaterialsDatum
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            ParametersId = item.ParametersId,
                            ParametersName = parametersName,
                            Year1 = item.Year1,
                            Year2 = item.Year2,
                            Year3 = item.Year3,
                            ParentHospitalName = parentHospitalName,
                            ParentHospitalAddress = parentHospitalAddress,
                            HospitalOwnerName = hospitalOwnerName,
                            HospitalType = hospitalType,
                            Kpmebeds = kpmeBeds.ToString(),
                            PostBasicBeds = postBasicBeds.ToString(),
                            ParentHospitalKpmebedsDoc = kpmeBedsDoc,
                            ParentHospitalPostBasicDoc = postBasicDoc,
                            PolutionControlDoc = polutionControl,
                            FireSafetyDoc = fireSafety,
                            BiomedicalWasteManagemenDoc = biomedicalWaste,
                            OpdDocument = opdHospitalFile,
                            IpdDocument = ipdHospitalFile,
                            MajorOperationsSurgeries = majorOpsHospitalFile,
                            MinorOperationsSurgeries = minorOpsHospitalFile
                        };

                        if (perRowFile != null) AssignPerRowFileToRecord(newRecord, perRowFile);

                        _context.NursingAffiliatedYearwiseMaterialsData.Add(newRecord);
                    }
                }

                _context.SaveChanges();
                TempData["Success"] = "Year-wise materials saved.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                TempData["Error"] = "Error saving data: " + ex.Message;
            }

            return RedirectToAction("Homeopathy_YearwiseMaterials");
        }
        public IActionResult ViewDocument(string collegeCode, string fileType)
        {
            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(fileType))
                return BadRequest("Invalid parameters.");

            var record = _context.NursingAffiliatedYearwiseMaterialsData
                .FirstOrDefault(y => y.CollegeCode == collegeCode);

            if (record == null)
                return NotFound("Record not found.");

            byte[] fileBytes = null;
            string fileName = "";
            string contentType = "application/pdf";

            switch (fileType)
            {
                case "Kpmebeds":
                    fileBytes = record.ParentHospitalKpmebedsDoc;
                    fileName = "KpmeBedsDoc.pdf";
                    break;
                case "PostBasic":
                    fileBytes = record.ParentHospitalPostBasicDoc;
                    fileName = "PostBasicDoc.pdf";
                    break;
                case "PolutionControlDoc":
                    fileBytes = record.PolutionControlDoc;
                    fileName = "PolutionControlDoc.pdf";
                    break;
                case "FireSafetyDoc":
                    fileBytes = record.FireSafetyDoc;
                    fileName = "FireSafetyDoc.pdf";
                    break;
                case "BiomedicalWasteManagemenDoc":
                    fileBytes = record.BiomedicalWasteManagemenDoc;
                    fileName = "BiomedicalWasteManagemenDoc.pdf";
                    break;
                case "OpdDocument":
                    fileBytes = record.OpdDocument;
                    fileName = "OpdDocument.pdf";
                    break;
                case "IpdDocument":
                    fileBytes = record.IpdDocument;
                    fileName = "IpdDocument.pdf";
                    break;
                case "MajorOperationsSurgeries":
                    fileBytes = record.MajorOperationsSurgeries;
                    fileName = "MajorOperationsSurgeries.pdf";
                    break;
                case "MinorOperationsSurgeries":
                    fileBytes = record.MinorOperationsSurgeries;
                    fileName = "MinorOperationsSurgeries.pdf";
                    break;
                default:
                    return BadRequest("Unknown file type.");
            }

            if (fileBytes == null || fileBytes.Length == 0)
                return NotFound("Requested document not found.");

            return File(fileBytes, contentType, fileName);
        }


        // Helper method to convert IFormFile to byte[]
        private byte[] ConvertFileToBytes(IFormFile formFile)
        {
            using (var ms = new MemoryStream())
            {
                formFile.CopyTo(ms);
                return ms.ToArray();
            }
        }

        [HttpGet]
        public IActionResult Homeopathy_ManageDesignationIntake()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var designationList = _context.DesignationMasters
                .Where(e => e.FacultyCode.ToString() == facultyCode)
                .Select(d => new CollegeDesignationDetailsViewModel
                {
                    DesignationCode = d.DesignationCode,
                    Designation = d.DesignationName,
                }).ToList();

            var savedIntake = _context.CollegeDesignationDetails
                .Where(c => c.CollegeCode == collegeCode && c.FacultyCode == facultyCode)
                .ToList();

            foreach (var item in designationList)
            {
                var exist = savedIntake.FirstOrDefault(x => x.DesignationCode == item.DesignationCode);
                if (exist != null)
                {
                    item.UGUnderRGUHSIntake = int.TryParse(exist.UgRguhsintake, out int ugRguhs) ? ugRguhs : 0;
                    item.UGPresentIntake = int.TryParse(exist.UgPresentintake, out int ugPresent) ? ugPresent : 0;
                    item.PGUnderRGUHSIntake = int.TryParse(exist.PgRguhsintake, out int pgRguhs) ? pgRguhs : 0;
                    item.PGPresentIntake = int.TryParse(exist.PgPresentintake, out int pgPresent) ? pgPresent : 0;
                    item.GokSanctioned = exist.Goksanctioned;
                    item.PGGokSanctioned = exist.Pggoksanctioned;
                }
                else
                {
                    item.UGUnderRGUHSIntake = 0;
                    item.UGPresentIntake = 0;
                    item.PGUnderRGUHSIntake = 0;
                    item.PGPresentIntake = 0;
                    item.GokSanctioned = "0";
                    item.PGGokSanctioned = "0";
                }
            }
            return View(designationList);
        }

        [HttpPost]
        public IActionResult Homeopathy_ManageDesignationIntake(List<CollegeDesignationDetailsViewModel> model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            foreach (var item in model)
            {
                var existing = _context.CollegeDesignationDetails.FirstOrDefault(x =>
                    x.CollegeCode == collegeCode && x.FacultyCode == facultyCode &&
                    x.DesignationCode == item.DesignationCode);

                if (existing != null)
                {
                    existing.UgRguhsintake = item.UGUnderRGUHSIntake.ToString();
                    existing.UgPresentintake = item.UGPresentIntake.ToString();
                    existing.PgRguhsintake = item.PGUnderRGUHSIntake.ToString();
                    existing.PgPresentintake = item.PGPresentIntake.ToString();
                    existing.Goksanctioned = item.GokSanctioned;
                    existing.Pggoksanctioned = item.PGGokSanctioned;

                    _context.Update(existing);
                }
                else
                {
                    _context.CollegeDesignationDetails.Add(new CollegeDesignationDetail
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        DesignationCode = item.DesignationCode,
                        Designation = item.Designation,
                        UgRguhsintake = item.UGUnderRGUHSIntake.ToString(),
                        UgPresentintake = item.UGPresentIntake.ToString(),
                        PgRguhsintake = item.PGUnderRGUHSIntake.ToString(),
                        PgPresentintake = item.PGPresentIntake.ToString(),
                        RequiredIntake = "0",
                        AvailableIntake = "0",
                        Goksanctioned = item.GokSanctioned,
                        Pggoksanctioned = item.PGGokSanctioned
                    });
                }
            }
            _context.SaveChanges();
            return RedirectToAction("Homeopathy_FacultyDetails");
        }


        //[HttpGet]
        //public IActionResult Homeopathy_FacultyDetails()
        //{
        //    string collegeCode = HttpContext.Session.GetString("CollegeCode");
        //    string facultyCode = HttpContext.Session.GetString("FacultyCode");

        //    if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
        //    {
        //        TempData["Error"] = "Session expired. Please log in again.";
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var subjectsList = _context.MstCourses
        //        .Where(c => c.FacultyCode.ToString() == facultyCode)
        //        .Select(c => new SelectListItem
        //        {
        //            Value = c.CourseCode.ToString(),
        //            Text = c.CourseName ?? ""
        //        })
        //        .Distinct()
        //        .ToList();

        //    //var designationsList = _context.DesignationMasters
        //    //    .Select(d => new SelectListItem
        //    //    {
        //    //        Value = d.DesignationCode,
        //    //        Text = d.DesignationName ?? ""
        //    //    })
        //    //    .ToList();

        //    var designationsList = _context.DesignationMasters
        //    .Where(c => c.FacultyCode.ToString() == facultyCode)
        //    .Select(d => new SelectListItem
        //    {
        //        Value = d.DesignationCode,
        //        Text = d.DesignationName ?? ""
        //    })
        //    .ToList();

        //    var departmentsList = _context.MstCourses
        //        .Where(e => e.FacultyCode.ToString() == facultyCode)
        //        .Select(d => new SelectListItem
        //        {
        //            Value = d.CourseCode.ToString(),
        //            Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
        //        })
        //        .ToList();

        //    var facultyDetails = _context.FacultyDetails
        //                         .Where(f => f.CollegeCode == collegeCode
        //                                     && f.FacultyCode == facultyCode
        //                                     && f.IsRemoved != true)
        //                         .ToList();


        //    var ahsFacultyWithCollege = _context.NursingFacultyWithColleges // Change table name as appropriate
        //        .Where(f => f.CollegeCode == collegeCode && f.FacultyCode.ToString() == facultyCode)
        //        .ToList();


        //    List<FacultyDetailsViewModel> vmList = new List<FacultyDetailsViewModel>();

        //    if (!facultyDetails.Any() && !ahsFacultyWithCollege.Any())
        //    {
        //        TempData["Info"] = "No faculty records found for this faculty.";
        //        vmList.Add(new FacultyDetailsViewModel
        //        {
        //            Subjects = subjectsList,
        //            Designations = designationsList,
        //            DepartmentDetails = departmentsList
        //        });
        //        return View(vmList);
        //    }

        //    // Join existing faculty details with college faculty data
        //    vmList = (from f1 in facultyDetails
        //              join f2 in ahsFacultyWithCollege
        //                  on new { f1.Aadhaar, f1.Pan, f1.Designation }
        //                  equals new { Aadhaar = f2.AadhaarNumber, Pan = f2.Pannumber, Designation = f2.Designation }
        //                  into gj
        //              from sub in gj.DefaultIfEmpty()
        //              select new FacultyDetailsViewModel
        //              {
        //                  FacultyDetailId = f1.Id,
        //                  NameOfFaculty = sub?.TeachingFacultyName ?? f1.NameOfFaculty,
        //                  Designation = sub?.Designation ?? f1.Designation,
        //                  Aadhaar = sub?.AadhaarNumber ?? f1.Aadhaar,
        //                  PAN = sub?.Pannumber ?? f1.Pan,
        //                  DepartmentDetail = f1.DepartmentDetails,
        //                  SelectedDepartment = f1.DepartmentDetails,
        //                  //Subject = f1.Subject,
        //                  RecognizedPGTeacher = f1.RecognizedPgTeacher,
        //                  Mobile = f1.Mobile,
        //                  Email = f1.Email,
        //                  Subjects = subjectsList,
        //                  Designations = designationsList,
        //                  DepartmentDetails = departmentsList,
        //                  RecognizedPhDTeacher = f1.RecognizedPhDteacher,
        //                  LitigationPending = f1.LitigationPending,
        //                  PhDRecognitionDocData = f1.PhDrecognitionDoc,
        //                  LitigationDocData = f1.LitigationDoc,
        //                  PGRecognitionDocData = f1.GuideRecognitionDoc,
        //                  IsExaminer = f1.IsExaminer,
        //                  ExaminerFor = f1.ExaminerFor,
        //                  ExaminerForList = !string.IsNullOrEmpty(f1.ExaminerFor)
        //                                      ? f1.ExaminerFor.Split(',').ToList()
        //                                      : new List<string>(),
        //                  // ⭐ ADD THIS
        //                  RemoveRemarks = f1.RemoveRemarks

        //              }).ToList();

        //    // Add missing faculty from college data
        //    var missingFaculty = ahsFacultyWithCollege
        //        .Where(f2 => !vmList.Any(v => v.Aadhaar == f2.AadhaarNumber && v.PAN == f2.Pannumber))
        //        .Select(f2 => new FacultyDetailsViewModel
        //        {
        //            NameOfFaculty = f2.TeachingFacultyName,
        //            Designation = f2.Designation,
        //            Aadhaar = f2.AadhaarNumber,
        //            PAN = f2.Pannumber,
        //            Subjects = subjectsList,
        //            Designations = designationsList,
        //            DepartmentDetails = departmentsList
        //        })
        //        .ToList();

        //    vmList.AddRange(missingFaculty);

        //    if (!vmList.Any())
        //    {
        //        vmList.Add(new FacultyDetailsViewModel
        //        {
        //            Subjects = subjectsList,
        //            Designations = designationsList,
        //            DepartmentDetails = departmentsList
        //        });
        //    }

        //    return View(vmList);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Homeopathy_FacultyDetails(IList<FacultyDetailsViewModel> model)
        //{
        //    string collegeCode = HttpContext.Session.GetString("CollegeCode");
        //    string facultyCode = HttpContext.Session.GetString("FacultyCode");

        //    // ✅ Session validation
        //    if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
        //    {
        //        TempData["Error"] = "Session expired. Please log in again.";
        //        return RedirectToAction("Login", "Account");
        //    }

        //    // ✅ Empty list validation
        //    if (model == null || !model.Any())
        //    {
        //        TempData["Error"] = "No data to save.";

        //        // Repopulate dropdown lists before returning to view
        //        model = new List<FacultyDetailsViewModel>
        //         {
        //                 new FacultyDetailsViewModel
        //                 {
        //                     Subjects = _context.MstCourses
        //                         .Where(c => c.FacultyCode.ToString() == facultyCode)
        //                         .Select(c => new SelectListItem { Value = c.CourseCode.ToString(), Text = c.CourseName ?? "" })
        //                         .Distinct()
        //                         .ToList(),
        //                     Designations = _context.DesignationMasters
        //                         .Select(d => new SelectListItem { Value = d.DesignationCode, Text = d.DesignationName ?? "" })
        //                         .ToList(),
        //                     DepartmentDetails = _context.MstCourses
        //                         .Where(e => e.FacultyCode.ToString() == facultyCode)
        //                         .Select(d => new SelectListItem
        //                         {
        //                             Value = d.CourseCode.ToString(),
        //                             Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
        //                         })
        //                         .ToList()
        //                 }
        //            };

        //        return View(model);
        //    }

        //    using (var transaction = _context.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            // Extract all IDs coming from the frontend
        //            var incomingIds = model
        //                .Where(m => m.FacultyDetailId > 0) // only valid existing IDs
        //                .Select(m => m.FacultyDetailId)
        //                .ToHashSet();

        //            // Get all existing faculty for this college/faculty code
        //            var existingFaculty = _context.FacultyDetails
        //                .Where(f => f.CollegeCode == collegeCode && f.FacultyCode == facultyCode)
        //                .ToList();

        //            // 🔹 1. DELETE records that are NOT in the incoming model
        //            //var toDelete = existingFaculty
        //            //    .Where(f => !incomingIds.Contains(f.Id))
        //            //    .ToList();

        //            //if (toDelete.Any())
        //            //{
        //            //    _context.FacultyDetails.RemoveRange(toDelete);
        //            //}

        //            // 🔹 2. ADD or UPDATE incoming data
        //            foreach (var m in model)
        //            {
        //                string name = m.NameOfFaculty?.Trim() ?? "N/A";
        //                string designation = m.Designation?.Trim() ?? "N/A";
        //                string subject = m.Subject?.Trim() ?? "N/A";
        //                string mobile = string.IsNullOrWhiteSpace(m.Mobile) ? "N/A" : m.Mobile.Trim();
        //                string email = string.IsNullOrWhiteSpace(m.Email) ? "N/A" : m.Email.Trim();
        //                string pan = m.PAN?.Trim() ?? "N/A";
        //                string aadhaar = m.Aadhaar?.Trim() ?? "N/A";
        //                string dept = m.SelectedDepartment?.Trim() ?? "N/A";
        //                string recognizedPG = m.RecognizedPGTeacher?.Trim() ?? "N/A";
        //                string recognizedPhD = m.RecognizedPhDTeacher?.Trim() ?? "N/A";
        //                string litigation = m.LitigationPending?.Trim() ?? "N/A";

        //                byte[] guideDocBytes = null;
        //                byte[] phdDocBytes = null;
        //                byte[] litigDocBytes = null;

        //                if (m.GuideRecognitionDoc != null)
        //                    guideDocBytes = ConvertFileToBytes(m.GuideRecognitionDoc);

        //                if (m.PhDRecognitionDoc != null)
        //                    phdDocBytes = ConvertFileToBytes(m.PhDRecognitionDoc);

        //                if (m.LitigationDoc != null)
        //                    litigDocBytes = ConvertFileToBytes(m.LitigationDoc);


        //                var existing = existingFaculty.FirstOrDefault(f => f.Id == m.FacultyDetailId);

        //                if (existing != null)
        //                {
        //                    // ✅ Update
        //                    existing.NameOfFaculty = name;
        //                    existing.Designation = designation;
        //                    existing.RecognizedPgTeacher = recognizedPG;
        //                    existing.Mobile = mobile;
        //                    existing.Email = email;
        //                    existing.Pan = pan;
        //                    existing.Aadhaar = aadhaar;
        //                    existing.DepartmentDetails = dept;
        //                    //existing.Subject = subject;
        //                    existing.RecognizedPhDteacher = recognizedPhD;
        //                    existing.LitigationPending = litigation;

        //                    // Only replace files if new uploads exist
        //                    if (guideDocBytes != null)
        //                        existing.GuideRecognitionDoc = guideDocBytes;

        //                    if (phdDocBytes != null)
        //                        existing.PhDrecognitionDoc = phdDocBytes;

        //                    if (litigDocBytes != null)
        //                        existing.LitigationDoc = litigDocBytes;

        //                    existing.IsExaminer = m.IsExaminer;
        //                    existing.ExaminerFor = m.ExaminerForList != null
        //                                            ? string.Join(",", m.ExaminerForList)
        //                                            : null;
        //                    if (!string.IsNullOrWhiteSpace(m.RemoveRemarks))
        //                    {
        //                        existing.IsRemoved = true;
        //                        existing.RemoveRemarks = m.RemoveRemarks;
        //                    }
        //                    _context.FacultyDetails.Update(existing);
        //                }
        //                else
        //                {
        //                    // ✅ Insert new
        //                    var faculty = new FacultyDetail
        //                    {
        //                        CollegeCode = collegeCode,
        //                        FacultyCode = facultyCode,
        //                        NameOfFaculty = name,
        //                        //Subject = subject,
        //                        Designation = designation,
        //                        RecognizedPgTeacher = recognizedPG,
        //                        RecognizedPhDteacher = recognizedPhD,
        //                        LitigationPending = litigation,
        //                        Mobile = mobile,
        //                        Email = email,
        //                        Pan = pan,
        //                        Aadhaar = aadhaar,
        //                        DepartmentDetails = dept,
        //                        GuideRecognitionDoc = guideDocBytes,
        //                        PhDrecognitionDoc = phdDocBytes,
        //                        LitigationDoc = litigDocBytes,
        //                        IsExaminer = m.IsExaminer,
        //                        ExaminerFor = m.ExaminerForList != null
        //                                    ? string.Join(",", m.ExaminerForList)
        //                                    : null,
        //                        RemoveRemarks = m.RemoveRemarks,
        //                    };

        //                    _context.FacultyDetails.Add(faculty);
        //                }
        //            }

        //            // 🔹 3. Save all changes once
        //            _context.SaveChanges();

        //            transaction.Commit();

        //            //TempData["Success"] = "Faculty records saved successfully.";
        //            return RedirectToAction("HomeopathyExamResults");
        //        }
        //        catch (Exception ex)
        //        {
        //            transaction.Rollback();

        //            // ✅ Log and show detailed error
        //            TempData["Error"] = "Error saving faculty records: " + ex.Message;

        //            // ✅ Repopulate dropdowns again so View doesn’t break
        //            foreach (var m in model)
        //            {
        //                m.Subjects = _context.MstCourses
        //                    .Where(c => c.FacultyCode.ToString() == facultyCode)
        //                    .Select(c => new SelectListItem { Value = c.CourseCode.ToString(), Text = c.CourseName ?? "" })
        //                    .Distinct()
        //                    .ToList();

        //                m.Designations = _context.DesignationMasters
        //                    .Select(d => new SelectListItem { Value = d.DesignationCode, Text = d.DesignationName ?? "" })
        //                    .ToList();

        //                m.DepartmentDetails = _context.MstCourses
        //                    .Where(e => e.FacultyCode.ToString() == facultyCode)
        //                    .Select(d => new SelectListItem
        //                    {
        //                        Value = d.CourseCode.ToString(),
        //                        Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
        //                    })
        //                    .ToList();
        //            }

        //            return View(model);
        //        }
        //    }
        //}


        //public IActionResult ViewFacultyDocument(int id, string type, string mode = "view")
        //{
        //    var faculty = _context.FacultyDetails.FirstOrDefault(f => f.Id == id);
        //    if (faculty == null)
        //        return NotFound();

        //    byte[] fileBytes = null;
        //    string fileName = $"{type}_document.pdf";

        //    switch (type.ToLower())
        //    {
        //        case "pg":
        //            fileBytes = faculty.GuideRecognitionDoc;
        //            break;

        //        case "phd":
        //            fileBytes = faculty.PhDrecognitionDoc;
        //            break;

        //        case "litig":
        //            fileBytes = faculty.LitigationDoc;
        //            break;

        //        default:
        //            return BadRequest("Invalid document type.");
        //    }

        //    if (fileBytes == null)
        //        return NotFound("Document not uploaded.");

        //    if (mode == "download")
        //    {
        //        // 📥 FORCE DOWNLOAD
        //        return File(fileBytes, "application/octet-stream", fileName);
        //    }

        //    // 👀 VIEW IN BROWSER
        //    return File(fileBytes, "application/pdf");
        //}


        [HttpGet]
        public async Task<IActionResult> HomeopathyExamResults()
        {
            var facultyCodeStr = HttpContext.Session.GetString("FacultyCode");
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            // ✅ Handle missing session
            if (string.IsNullOrEmpty(facultyCodeStr) || string.IsNullOrEmpty(collegeCode))
            {
                TempData["ErrorMessage"] = "Session expired! Please log in again.";
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(facultyCodeStr, out int facultyCode))
            {
                TempData["ErrorMessage"] = "Invalid Faculty Code!";
                return RedirectToAction("Login", "Account");
            }

            // ✅ Base model with all possible rows
            var model = new List<NursingExamResultViewModel>
    {
        new() { Course = "UG", Year = "1st Year" },
        new() { Course = "UG", Year = "2nd Year" },
        new() { Course = "UG", Year = "3rd Year" },
        new() { Course = "UG", Year = "4th Year" },
        new() { Course = "PG", Year = "1st Year" },
        new() { Course = "PG", Year = "2nd Year" }
    };

            // ✅ Get saved records from DB
            var savedResults = await _context.FacultyExamResults
                .Where(x => x.Facultycode == facultyCode.ToString() && x.Collegecode == collegeCode)
                .ToListAsync();

            // ✅ Merge DB values into default list
            foreach (var item in model)
            {
                int year = 0;
                int.TryParse(new string(item.Year.Where(char.IsDigit).ToArray()), out year);

                // Find match in DB (same course + same year)
                var existing = savedResults.FirstOrDefault(r =>
                    r.Course.Equals(item.Course, StringComparison.OrdinalIgnoreCase) &&
                    r.Year == year);

                if (existing != null)
                {
                    item.ExamAppearedCount = existing.ExamappearedCount.Value;
                    item.PassedOutCount = existing.Passedoutcount.Value;
                    item.YearOfPercentage = existing.Yearofpercentage.ToString();
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> HomeopathyExamResults(List<NursingExamResultViewModel> model)
        {
            try
            {
                var facultyCodeStr = HttpContext.Session.GetString("FacultyCode");
                var collegeCode = HttpContext.Session.GetString("CollegeCode");

                // ✅ Validate session and model
                if (model == null || !model.Any())
                {
                    TempData["ErrorMessage"] = "No data submitted!";
                    return View(model ?? new List<NursingExamResultViewModel>());
                }

                if (string.IsNullOrEmpty(facultyCodeStr) || string.IsNullOrEmpty(collegeCode))
                {
                    TempData["ErrorMessage"] = "Session expired! Please log in again.";
                    return RedirectToAction("Login", "Account");
                }

                if (!int.TryParse(facultyCodeStr, out int facultyCode))
                {
                    TempData["ErrorMessage"] = "Invalid Faculty Code!";
                    return View(model);
                }

                // ✅ Make sure DbSet is not null
                if (_context.FacultyExamResults == null)
                {
                    TempData["ErrorMessage"] = "Database context not initialized properly!";
                    return View(model);
                }

                // ✅ Remove old records safely
                var oldRecords = await _context.FacultyExamResults
                    .Where(x => x.Facultycode == facultyCode.ToString() && x.Collegecode == collegeCode)
                    .ToListAsync();

                if (oldRecords?.Any() == true)
                {
                    _context.FacultyExamResults.RemoveRange(oldRecords);
                    await _context.SaveChangesAsync();
                }

                // ✅ Prepare new data safely
                var newRecords = model
                    .Where(item => !string.IsNullOrWhiteSpace(item.Course) && !string.IsNullOrWhiteSpace(item.Year))
                    .Select(item =>
                    {
                        int.TryParse(new string(item.Year.Where(char.IsDigit).ToArray()), out int yr);

                        return new FacultyExamResult
                        {
                            Facultycode = facultyCode.ToString(),
                            Collegecode = collegeCode,
                            Course = item.Course?.Trim(),
                            Year = yr,
                            ExamappearedCount = item.ExamAppearedCount,
                            Passedoutcount = item.PassedOutCount,
                            Yearofpercentage = !string.IsNullOrWhiteSpace(item.YearOfPercentage)
                                ? decimal.TryParse(item.YearOfPercentage, out var percent) ? percent : 0
                                : 0
                        };
                    })
                    .ToList();


                // ✅ Prevent AddRangeAsync from null source
                if (newRecords.Any())
                {
                    await _context.FacultyExamResults.AddRangeAsync(newRecords);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "🌟 Success! Your records have been updated successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "No valid records found to save!";
                }

                return RedirectToAction(nameof(HomeopathyExamResults));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while saving data: {ex.Message}";
                return View(model ?? new List<NursingExamResultViewModel>());
            }
        }
    }
}
