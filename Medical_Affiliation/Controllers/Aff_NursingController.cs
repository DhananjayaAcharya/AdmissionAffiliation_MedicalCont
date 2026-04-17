using Medical_Affiliation.Controllers;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Newtonsoft.Json; // At the top of your controller


namespace Medical_Affiliation.Controllers
{
    public class Aff_NursingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Aff_NursingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Nursing_BasicDetails()
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
        public IActionResult Nursing_BasicDetails(NursingInstituteDetailViewModel model)
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

            return RedirectToAction("Nursing_UGPG");
        }

        [HttpGet]
        public IActionResult Nursing_Institute_Report()
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
        public IActionResult Nursing_ManageCourses()
        {
            string facultyCode = HttpContext.Session.GetString("FacultyCode");
            string collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
                return RedirectToAction("Login", "CollegeLogin");

            int facultyCodeInt = int.Parse(facultyCode);

            var ugCourses = _context.MstCourses
                .Where(c => c.CourseLevel == "UG" && c.FacultyCode == facultyCodeInt)
                .Select(c => new CourseDropdownVM { CourseCode = c.CourseCode.ToString(), CourseName = c.CourseName })
                .ToList();

            var pgCourses = _context.MstCourses
                .Where(c => c.CourseLevel == "PG" && c.FacultyCode == facultyCodeInt)
                .Select(c => new CourseDropdownVM { CourseCode = c.CourseCode.ToString(), CourseName = c.CourseName })
                .ToList();

            var savedUG = (from u in _context.NursingUgpgdetails
                           join c in _context.MstCourses on u.Course equals c.CourseCode.ToString()
                           where u.FacultyCode == facultyCode &&
                                 u.CollegeCode == collegeCode &&
                                 c.CourseLevel == "UG"
                           select new UGDetailsViewModel
                           {
                               Id = u.Id,
                               Course = u.Course,
                               UGIntake = u.IntakeDetails,
                               FreshOrIncrease = u.FreshOrIncrease,
                               FirstLOPDate = u.FirstLopdate,
                               NumberOfSeats = u.NumberOfSeats,
                               PermittedYear = u.PermittedYear,
                               RecognizedYear = u.RecognizedYear,
                               RGUHSNotificationData = u.Rguhsnotification,
                               GMCData = u.Inc,
                               NMCData = u.Knmc,
                               GokData = u.Gok,

                               // Assuming files are stored as byte[] and don't need to be loaded
                           }).ToList();

            var savedPG = (from u in _context.NursingUgpgdetails
                           join c in _context.MstCourses on u.Course equals c.CourseCode.ToString()
                           where u.FacultyCode == facultyCode &&
                                 u.CollegeCode == collegeCode &&
                                 c.CourseLevel == "PG"
                           select new PGDetailsViewModel
                           {
                               Id = u.Id,
                               Course = u.Course,
                               PGIntake = u.IntakeDetails,
                               FreshOrIncrease = u.FreshOrIncrease,
                               FirstLOPDate = u.FirstLopdate,
                               NumberOfSeats = u.NumberOfSeats,
                               PermittedYear = u.PermittedYear,
                               RecognizedYear = u.RecognizedYear,
                               RGUHSNotificationData = u.Rguhsnotification,
                               GMCData = u.Inc,
                               NMCData = u.Knmc,
                               GokData = u.Gok,
                           }).ToList();

            var vm = new UGPGSSDetailsFormViewModel
            {
                FacultyCode = facultyCode,
                CollegeCode = collegeCode,
                UGCourses = ugCourses,
                PGCourses = pgCourses,
                UGDetailsList = savedUG.Any()
                    ? savedUG
                    : new List<UGDetailsViewModel> { new UGDetailsViewModel() },

                PGDetailsList = savedPG.Any() ? savedPG : new List<PGDetailsViewModel> { new PGDetailsViewModel() }
            };

            ViewBag.UGIntakes = _context.CollegeCourseIntakeDetails
                .Where(i => i.CollegeCode == collegeCode && ugCourses.Select(c => c.CourseCode).Contains(i.CourseCode))
                .ToDictionary(i => i.CourseCode, i => i.ExistingIntake ?? 0);

            ViewBag.PGIntakes = _context.CollegeCourseIntakeDetails
                .Where(i => i.CollegeCode == collegeCode && pgCourses.Select(c => c.CourseCode).Contains(i.CourseCode))
                .ToDictionary(i => i.CourseCode, i => i.ExistingIntake ?? 0);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Nursing_ManageCourses(UGPGSSDetailsFormViewModel model)
        {
            string facultyCode = HttpContext.Session.GetString("FacultyCode");
            string collegeCode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrEmpty(collegeCode))
                return RedirectToAction("Login", "CollegeLogin");

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // ---------- Save UG ----------
                    foreach (var item in model.UGDetailsList)
                    {
                        if (!string.IsNullOrEmpty(item.Course) && !string.IsNullOrEmpty(item.UGIntake))
                        {
                            var existing = _context.NursingUgpgdetails
                                .FirstOrDefault(u => u.FacultyCode == facultyCode
                                                  && u.CollegeCode == collegeCode
                                                  && u.Course == item.Course
                                                  && u.IntakeDetails == item.UGIntake);

                            if (existing != null)
                            {
                                existing.FreshOrIncrease = item.FreshOrIncrease;
                                existing.FirstLopdate = item.FirstLOPDate;
                                existing.NumberOfSeats = item.NumberOfSeats;
                                existing.PermittedYear = item.PermittedYear;
                                existing.RecognizedYear = item.RecognizedYear;

                                if (item.RGUHSNotificationFile != null)
                                    existing.Rguhsnotification = ConvertToBytes(item.RGUHSNotificationFile);

                                if (item.GMCFile != null)
                                    existing.Knmc = ConvertToBytes(item.GMCFile);

                                if (item.NMCFile != null)
                                    existing.Inc = ConvertToBytes(item.NMCFile);

                                if (item.Gok != null)
                                    existing.Gok = ConvertToBytes(item.Gok);
                            }
                            else
                            {
                                var entity = new NursingUgpgdetail
                                {
                                    FacultyCode = facultyCode,
                                    CollegeCode = collegeCode,
                                    IntakeDetails = item.UGIntake,
                                    Course = item.Course,
                                    FreshOrIncrease = item.FreshOrIncrease,
                                    FirstLopdate = item.FirstLOPDate,
                                    NumberOfSeats = item.NumberOfSeats,
                                    PermittedYear = item.PermittedYear,
                                    RecognizedYear = item.RecognizedYear,
                                    Rguhsnotification = item.RGUHSNotificationFile != null ? ConvertToBytes(item.RGUHSNotificationFile) : null,
                                    Knmc = item.GMCFile != null ? ConvertToBytes(item.GMCFile) : null,
                                    Inc = item.NMCFile != null ? ConvertToBytes(item.NMCFile) : null,
                                    Gok = item.Gok != null ? ConvertToBytes(item.Gok) : null
                                };
                                _context.NursingUgpgdetails.Add(entity);
                            }
                        }
                    }

                    // ---------- Save PG ----------
                    foreach (var item in model.PGDetailsList)
                    {
                        if (!string.IsNullOrEmpty(item.Course) && !string.IsNullOrEmpty(item.PGIntake))
                        {
                            var existing = _context.NursingUgpgdetails
                                .FirstOrDefault(u => u.FacultyCode == facultyCode
                                                  && u.CollegeCode == collegeCode
                                                  && u.Course == item.Course
                                                  && u.IntakeDetails == item.PGIntake);

                            if (existing != null)
                            {
                                existing.FreshOrIncrease = item.FreshOrIncrease;
                                existing.FirstLopdate = item.FirstLOPDate;
                                existing.NumberOfSeats = item.NumberOfSeats;
                                existing.PermittedYear = item.PermittedYear;
                                existing.RecognizedYear = item.RecognizedYear;

                                if (item.RGUHSNotificationFile != null)
                                    existing.Rguhsnotification = ConvertToBytes(item.RGUHSNotificationFile);

                                if (item.GMCFile != null)
                                    existing.Knmc = ConvertToBytes(item.GMCFile);

                                if (item.NMCFile != null)
                                    existing.Inc = ConvertToBytes(item.NMCFile);

                                if (item.Gok != null)
                                    existing.Gok = ConvertToBytes(item.Gok);
                            }
                            else
                            {
                                var entity = new NursingUgpgdetail
                                {
                                    FacultyCode = facultyCode,
                                    CollegeCode = collegeCode,
                                    IntakeDetails = item.PGIntake,
                                    Course = item.Course,
                                    FreshOrIncrease = item.FreshOrIncrease,
                                    FirstLopdate = item.FirstLOPDate,
                                    NumberOfSeats = item.NumberOfSeats,
                                    PermittedYear = item.PermittedYear,
                                    RecognizedYear = item.RecognizedYear,
                                    Rguhsnotification = item.RGUHSNotificationFile != null ? ConvertToBytes(item.RGUHSNotificationFile) : null,
                                    Knmc = item.GMCFile != null ? ConvertToBytes(item.GMCFile) : null,
                                    Inc = item.NMCFile != null ? ConvertToBytes(item.NMCFile) : null,
                                    Gok = item.Gok != null ? ConvertToBytes(item.Gok) : null
                                };
                                _context.NursingUgpgdetails.Add(entity);
                            }
                        }
                    }
                    // Save all changes
                    _context.SaveChanges();

                    // Commit if all good
                    transaction.Commit();
                    return RedirectToAction("Nursing_YearwiseMaterials");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ViewBag.ErrorMessage = "An error occurred while saving data. Please try again.";
                    Console.WriteLine($"Error: {ex.Message}");
                    return View(model);
                }
            }
        }

        public async Task<IActionResult> ViewFile(int id, string type)
        {
            var ug = await _context.NursingUgpgdetails.FindAsync(id);
            if (ug == null) return NotFound();

            byte[] fileData = type.ToUpper() switch
            {
                "RGUHS" => ug.Rguhsnotification,
                "GOK" => ug.Gok,
                "KSNC" => ug.Knmc,
                "INC" => ug.Inc,
                _ => null
            };

            if (fileData == null)
                return NotFound();

            return File(fileData, "application/pdf");
        }

        private byte[] ConvertToBytes(IFormFile file)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            return ms.ToArray();
        }

        [HttpGet]
        public IActionResult Nursing_YearwiseMaterials()
        {
            try
            {
                var collegeCode = HttpContext.Session.GetString("CollegeCode");
                var facultyCode = HttpContext.Session.GetString("FacultyCode");

                if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                    return RedirectToAction("Login", "Account");

                // Fetch clinical parameters
                var parameters = _context.ClinicalMaterialData
                    .Where(p => p.FacultyCode == facultyCode)
                    .ToList();

                // Fetch existing yearwise materials
                var yearwiseList = _context.YearwiseMaterialsData
                    .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                    .ToList();

                // Dictionary for fast lookup
                var yearwiseDict = yearwiseList.ToDictionary(y => y.ParametersId);

                // Build ViewModel for form
                var model = parameters.Select(p => new Nursing_YearwiseMaterialsDataViewModel
                {
                    ParametersId = p.ParametersId,
                    ParametersName = p.ParametersName,
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    Year1 = yearwiseDict.ContainsKey(p.ParametersId) ? yearwiseDict[p.ParametersId].Year1?.ToString() ?? "0" : "0",
                    Year2 = yearwiseDict.ContainsKey(p.ParametersId) ? yearwiseDict[p.ParametersId].Year2?.ToString() ?? "0" : "0",
                    Year3 = yearwiseDict.ContainsKey(p.ParametersId) ? yearwiseDict[p.ParametersId].Year3?.ToString() ?? "0" : "0"
                }).ToList();

                // Corrected ViewBag for Razor table (property names exactly match your view)
                ViewBag.YearwiseData = yearwiseList.Select(y => new
                {
                    ParametersId = y.ParametersId, // ✅ Add this
                    ParametersName = y.ParametersName,
                    Year1 = y.Year1,
                    Year2 = y.Year2,
                    Year3 = y.Year3,
                    parentHospitalName = y.ParentHospitalName,
                    parentHospitalAddress = y.ParentHospitalAddress,
                    parentHospitalMOUdoc = y.ParentHospitalMoudoc,
                    parentHospitalOwnerNameDoc = y.ParentHospitalOwnerNameDoc,
                    parentHospitalKPMEbedsDoc = y.ParentHospitalKpmebedsDoc,
                    parentHospitalPostBasicDoc = y.ParentHospitalPostBasicDoc,
                    KPMEBeds = y.Kpmebeds,
                    PostBasicBeds = y.PostBasicBeds,
                    TotalBeds = y.TotalBeds,
                    HospitalOwnerName = y.HospitalOwnerName
                }).ToList();


                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["Error"] = "Error loading data!";
                return View(new List<Nursing_YearwiseMaterialsDataViewModel>());
            }
        }

        public IActionResult YearwiseMaterials()
        {
            var materialsData = _context.YearwiseMaterialsData
                                .Take(1000)
                                .ToList();

            ViewBag.YearwiseData = materialsData;

            // Pass your existing model List<Nursing_YearwiseMaterialsDataViewModel> as usual to the view
            var model = YearwiseMaterials(); // Your logic here
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteYearwiseMaterial(int parametersId)
        {
            try
            {
                var collegeCode = HttpContext.Session.GetString("CollegeCode");
                var facultyCode = HttpContext.Session.GetString("FacultyCode");

                if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                    return RedirectToAction("Login", "Account");

                var record = _context.YearwiseMaterialsData.FirstOrDefault(x =>
                    x.ParametersId == parametersId &&
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

                if (record != null)
                {
                    _context.YearwiseMaterialsData.Remove(record);
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

            return RedirectToAction("Nursing_YearwiseMaterials");
        }

        [HttpPost]
        public IActionResult Nursing_YearwiseMaterials(List<Nursing_YearwiseMaterialsDataViewModel> model)
        {
            try
            {
                var collegeCode = HttpContext.Session.GetString("CollegeCode");
                var facultyCode = HttpContext.Session.GetString("FacultyCode");

                if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                    return RedirectToAction("Login", "Account");

                if (model == null || !model.Any())
                {
                    TempData["Error"] = "No data found to save!";
                    return RedirectToAction("Nursing_YearwiseMaterials");
                }

                // ✅ Read Parent Hospital form values
                var form = HttpContext.Request.Form;
                string parentHospitalName = form["ParentHospitalName"];
                string parentHospitalAddress = form["ParentHospitalAddress"];
                string hospitalOwnerName = form["HospitalOwnerName"];
                string kpmeBeds = form["Kpmebeds"];
                string postBasicBeds = form["PostBasicBeds"];
                string totalBeds = form["TotalBeds"];

                // ✅ Read Uploaded files
                var files = HttpContext.Request.Form.Files;
                var kpmeDoc = files["ParentHospitalKpmebedsDocFile"];
                var mouDoc = files["ParentHospitalMoudocFile"];
                var ownerDoc = files["ParentHospitalOwnerNameDocFile"];
                var postBasicDoc = files["ParentHospitalPostBasicDocFile"];

                foreach (var item in model)
                {
                    var existingRecord = _context.YearwiseMaterialsData.FirstOrDefault(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.ParametersId == item.ParametersId);

                    if (existingRecord != null)
                    {
                        // ✅ Update existing record
                        existingRecord.Year1 = item.Year1;
                        existingRecord.Year2 = item.Year2;
                        existingRecord.Year3 = item.Year3;

                        existingRecord.ParentHospitalName = parentHospitalName;
                        existingRecord.ParentHospitalAddress = parentHospitalAddress;
                        existingRecord.HospitalOwnerName = hospitalOwnerName;
                        existingRecord.Kpmebeds = kpmeBeds;
                        existingRecord.PostBasicBeds = postBasicBeds;
                        existingRecord.TotalBeds = totalBeds;

                        if (kpmeDoc?.Length > 0) existingRecord.ParentHospitalKpmebedsDoc = ConvertFileToBytes(kpmeDoc);
                        if (mouDoc?.Length > 0) existingRecord.ParentHospitalMoudoc = ConvertFileToBytes(mouDoc);
                        if (ownerDoc?.Length > 0) existingRecord.ParentHospitalOwnerNameDoc = ConvertFileToBytes(ownerDoc);
                        if (postBasicDoc?.Length > 0) existingRecord.ParentHospitalPostBasicDoc = ConvertFileToBytes(postBasicDoc);
                    }
                    else
                    {
                        // ✅ Insert new record
                        var newData = new YearwiseMaterialsDatum
                        {
                            CollegeCode = collegeCode,
                            FacultyCode = facultyCode,
                            ParametersId = item.ParametersId,
                            ParametersName = item.ParametersName,
                            Year1 = item.Year1,
                            Year2 = item.Year2,
                            Year3 = item.Year3,

                            ParentHospitalName = parentHospitalName,
                            ParentHospitalAddress = parentHospitalAddress,
                            HospitalOwnerName = hospitalOwnerName,
                            Kpmebeds = kpmeBeds,
                            PostBasicBeds = postBasicBeds,
                            TotalBeds = totalBeds,

                            ParentHospitalKpmebedsDoc = kpmeDoc?.Length > 0 ? ConvertFileToBytes(kpmeDoc) : null,
                            ParentHospitalMoudoc = mouDoc?.Length > 0 ? ConvertFileToBytes(mouDoc) : null,
                            ParentHospitalOwnerNameDoc = ownerDoc?.Length > 0 ? ConvertFileToBytes(ownerDoc) : null,
                            ParentHospitalPostBasicDoc = postBasicDoc?.Length > 0 ? ConvertFileToBytes(postBasicDoc) : null
                        };

                        _context.YearwiseMaterialsData.Add(newData);
                    }
                }

                _context.SaveChanges();
                TempData["Success"] = "Data saved successfully!";
                return RedirectToAction("Nursing_YearwiseMaterials"); // ✅ go back to the same page
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TempData["Error"] = "Error while saving data!";
                return RedirectToAction("Nursing_YearwiseMaterials");
            }
        }

        public IActionResult ViewDocument(string collegeCode, string fileType)
        {
            var record = _context.YearwiseMaterialsData
                .FirstOrDefault(y => y.CollegeCode == collegeCode);

            if (record == null)
                return NotFound();

            byte[] fileBytes = null;
            string fileName = "";
            string contentType = "application/octet-stream";

            switch (fileType)
            {
                case "Kpmebeds":
                    fileBytes = record.ParentHospitalKpmebedsDoc;
                    fileName = "KpmeBedsDoc.pdf"; // adjust file name and type if known
                    contentType = "application/pdf"; // adjust accordingly
                    break;
                case "MOU":
                    fileBytes = record.ParentHospitalMoudoc;
                    fileName = "MouDoc.pdf";
                    contentType = "application/pdf";
                    break;
                case "OwnerName":
                    fileBytes = record.ParentHospitalOwnerNameDoc;
                    fileName = "OwnerNameDoc.pdf";
                    contentType = "application/pdf";
                    break;
                case "PostBasic":
                    fileBytes = record.ParentHospitalPostBasicDoc;
                    fileName = "PostBasicDoc.pdf";
                    contentType = "application/pdf";
                    break;
            }

            if (fileBytes == null)
                return NotFound();

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
        public IActionResult Nursing_Affiliated_YearwiseMaterials()
        {
            try
            {
                var collegeCode = HttpContext.Session.GetString("CollegeCode");
                var facultyCode = HttpContext.Session.GetString("FacultyCode");

                if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                    return RedirectToAction("Login", "Account");

                // Normalize comparisons: some DB FacultyCode may be int; compare by string to avoid mismatches
                var model = (from param in _context.MstNursingAffiliatedMaterialData
                             where param.FacultyCode.ToString() == facultyCode
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
                                 Year1 = ywItem != null ? (ywItem.Year1 ?? "0") : "0",
                                 Year2 = ywItem != null ? (ywItem.Year2 ?? "0") : "0",
                                 Year3 = ywItem != null ? (ywItem.Year3 ?? "0") : "0",
                                 ParentHospitalName = ywItem != null ? ywItem.ParentHospitalName : null,
                                 ParentHospitalAddress = ywItem != null ? ywItem.ParentHospitalAddress : null
                             }).ToList();

                // Provide saved rows with Id and document presence flags so view can render "View" links
                // 1. Get saved data
                var savedRows = _context.NursingAffiliatedYearwiseMaterialsData
                    .Where(y => y.CollegeCode == collegeCode && y.FacultyCode == facultyCode)
                    .Join(_context.MstNursingAffiliatedMaterialData,
                          y => y.ParametersId,
                          p => p.ParametersId,
                          (y, p) => new
                          {
                              y.Id,
                              y.ParametersId,
                              ParametersName = p.ParametersName,
                              y.Year1,
                              y.Year2,
                              y.Year3,
                              y.ParentHospitalName,
                              y.ParentHospitalAddress,
                              y.Kpmebeds,
                              y.PostBasicBeds,
                              y.TotalBeds,
                              y.HospitalOwnerName,
                              y.HospitalType,
                              ParentHospitalMoudocPresent = y.ParentHospitalMoudoc != null,
                              ParentHospitalOwnerNameDocPresent = y.ParentHospitalOwnerNameDoc != null,
                              ParentHospitalKpmebedsDocPresent = y.ParentHospitalKpmebedsDoc != null,
                              ParentHospitalPostBasicDocPresent = y.ParentHospitalPostBasicDoc != null
                          })
                    .ToList();


                // 2️⃣ Unique parameters (guaranteed non-null)
                var parametersList = savedRows.Select(x => x.ParametersName).Distinct().ToList();

                // 3️⃣ Pivot data: group by hospital (Id), each hospital has a dictionary of parameters → Year1/2/3
                var pivotData = savedRows
                    .GroupBy(x => new { x.HospitalType, x.ParentHospitalName })
                    .Select(g => new YearwisePivotRecord
                    {
                        HospitalType = g.Key.HospitalType,
                        ParentHospitalName = g.Key.ParentHospitalName,
                        ParentHospitalAddress = g.First().ParentHospitalAddress,
                        Kpmebeds = g.First().Kpmebeds,
                        PostBasicBeds = g.First().PostBasicBeds,
                        TotalBeds = g.First().TotalBeds,
                        HospitalOwnerName = g.First().HospitalOwnerName,
                        ParentHospitalMoudocPresent = g.First().ParentHospitalMoudocPresent,
                        ParentHospitalOwnerNameDocPresent = g.First().ParentHospitalOwnerNameDocPresent,
                        ParentHospitalKpmebedsDocPresent = g.First().ParentHospitalKpmebedsDocPresent,
                        ParentHospitalPostBasicDocPresent = g.First().ParentHospitalPostBasicDocPresent,

                        Parameters = g.GroupBy(r => r.ParametersName)
                                      .ToDictionary(
                                          grp => grp.Key,
                                          grp => new YearwiseValues
                                          {
                                              ParametersId = grp.First().ParametersId,
                                              Year1 = grp.First().Year1,
                                              Year2 = grp.First().Year2,
                                              Year3 = grp.First().Year3
                                          })
                    })
                    .ToList();



                // 4️⃣ Pass to ViewBag (guaranteed non-null)
                ViewBag.ParametersList = parametersList ?? new List<string>();
                ViewBag.PivotData = pivotData;
                ViewBag.YearwiseData = savedRows;
                ViewBag.YearwiseDataCount = savedRows.Count;

                var hospitalTypes = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "ParentHospital", Text = "Parent Hospital" },
                        new SelectListItem { Value = "AffiliatedHospital", Text = "Affiliated Hospital" }
                    };

                // Check if ParentHospital already exists
                bool parentHospitalExists = _context.NursingAffiliatedYearwiseMaterialsData
                    .Any(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode && x.HospitalType == "ParentHospital");

                if (parentHospitalExists)
                {
                    var parentItem = hospitalTypes.FirstOrDefault(ht => ht.Value == "ParentHospital");
                    if (parentItem != null) parentItem.Disabled = true;
                }

                ViewBag.HospitalTypes = hospitalTypes;


                // Debug log for quick verification (remove in production)
                Console.WriteLine($"Nursing_Affiliated_YearwiseMaterials GET: parameters={model.Count}, savedRows={savedRows.Count}, college={collegeCode}, faculty={facultyCode}");

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
                case "MOU":
                    fileBytes = record.ParentHospitalMoudoc;
                    fileName = "MOU.pdf";
                    break;
                case "Owner":
                    fileBytes = record.ParentHospitalOwnerNameDoc;
                    fileName = "Owner.pdf";
                    break;
                case "KPME":
                    fileBytes = record.ParentHospitalKpmebedsDoc;
                    fileName = "KPME.pdf";
                    break;
                case "PostBasic":
                    fileBytes = record.ParentHospitalPostBasicDoc;
                    fileName = "PostBasic.pdf";
                    break;
                default:
                    return BadRequest();
            }

            if (fileBytes == null || fileBytes.Length == 0)
                return NotFound();

            return File(fileBytes, "application/pdf", fileName);
        }

        [HttpPost]
        public IActionResult DeleteYearwiseMaterial1(int id)
        {
            try
            {
                var collegeCode = HttpContext.Session.GetString("CollegeCode");
                var facultyCode = HttpContext.Session.GetString("FacultyCode");

                if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                    return RedirectToAction("Login", "Account");

                // 1️⃣ Fetch the selected record by its Id
                var selectedRecord = _context.NursingAffiliatedYearwiseMaterialsData
                    .FirstOrDefault(x => x.Id == id);

                if (selectedRecord == null)
                {
                    TempData["Error"] = "Record not found!";
                    return RedirectToAction("Nursing_Affiliated_YearwiseMaterials");
                }

                // Extract group keys for deletion
                var hospitalType = selectedRecord.HospitalType;
                var hospitalName = selectedRecord.ParentHospitalName;

                // 2️⃣ Delete ALL records for this hospital (same college, hospital type, and name)
                var recordsToDelete = _context.NursingAffiliatedYearwiseMaterialsData
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode &&
                        x.HospitalType == hospitalType &&
                        x.ParentHospitalName == hospitalName
                    )
                    .ToList();

                if (recordsToDelete.Any())
                {
                    _context.NursingAffiliatedYearwiseMaterialsData.RemoveRange(recordsToDelete);
                    _context.SaveChanges();
                    TempData["Success"] = "Hospital records deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "No records found for this hospital!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting record: " + ex.Message;
            }

            return RedirectToAction("Nursing_Affiliated_YearwiseMaterials");
        }

        [HttpPost]
        public IActionResult Nursing_Affiliated_YearwiseMaterials(List<YearwiseMaterialViewModel> model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (model == null || model.Count == 0)
            {
                TempData["Error"] = "No data to save.";
                return RedirectToAction("Nursing_Affiliated_YearwiseMaterials");
            }

            var form = HttpContext.Request.Form;

            // Read hospital fields from form, including HospitalType dropdown
            string parentHospitalName = form["ParentHospitalName"];
            string parentHospitalAddress = form["ParentHospitalAddress"];
            string hospitalOwnerName = form["HospitalOwnerName"];
            string hospitalType = form["HospitalType"]; // <-- read selected HospitalType here

            int.TryParse(form["Kpmebeds"], out int kpmeBeds);
            int.TryParse(form["PostBasicBeds"], out int postBasicBeds);
            int.TryParse(form["TotalBeds"], out int totalBeds);

            var files = HttpContext.Request.Form.Files;
            byte[] kpmeBedsDoc = files["ParentHospitalKpmebedsDocFile"] != null ? ConvertFileToBytes(files["ParentHospitalKpmebedsDocFile"]) : null;
            byte[] mouDoc = files["ParentHospitalMoudocFile"] != null ? ConvertFileToBytes(files["ParentHospitalMoudocFile"]) : null;
            byte[] ownerDoc = files["ParentHospitalOwnerNameDocFile"] != null ? ConvertFileToBytes(files["ParentHospitalOwnerNameDocFile"]) : null;
            byte[] postBasicDoc = files["ParentHospitalPostBasicDocFile"] != null ? ConvertFileToBytes(files["ParentHospitalPostBasicDocFile"]) : null;

            if (hospitalType == "AffiliatedHospital")
            {
                var existingHospitalTypes = _context.NursingAffiliatedYearwiseMaterialsData
                    .Where(x => x.HospitalType.StartsWith("AffiliatedHospital"))
                    .Select(x => x.HospitalType)
                    .ToList();

                int nextIndex = 0;
                if (existingHospitalTypes.Any())
                {
                    var numbers = existingHospitalTypes
                        .Select(ht => ht.Substring("AffiliatedHospital".Length))
                        .Select(sfx => int.TryParse(sfx, out int n) ? n : 0)
                        .ToList();

                    nextIndex = numbers.Max() + 1;
                }

                hospitalType = nextIndex == 0 ? "AffiliatedHospital1" : "AffiliatedHospital" + nextIndex;
            }

            foreach (var item in model)
            {
                var parametersName = string.IsNullOrEmpty(item.ParametersName)
                    ? _context.ClinicalMaterialData.Where(p => p.ParametersId == item.ParametersId).Select(p => p.ParametersName).FirstOrDefault()
                    : item.ParametersName;

                var existingRecord = _context.NursingAffiliatedYearwiseMaterialsData
                    .FirstOrDefault(y => y.CollegeCode == collegeCode
                                      && y.FacultyCode == facultyCode
                                      && y.ParametersId == item.ParametersId
                                      && y.ParentHospitalName == parentHospitalName);

                if (existingRecord != null)
                {
                    existingRecord.Year1 = item.Year1;
                    existingRecord.Year2 = item.Year2;
                    existingRecord.Year3 = item.Year3;
                    existingRecord.ParametersName = parametersName;

                    existingRecord.ParentHospitalName = parentHospitalName;
                    existingRecord.ParentHospitalAddress = parentHospitalAddress;
                    existingRecord.HospitalOwnerName = hospitalOwnerName;
                    existingRecord.HospitalType = hospitalType; // <-- assign hospital type here
                    existingRecord.Kpmebeds = kpmeBeds.ToString();
                    existingRecord.PostBasicBeds = postBasicBeds.ToString();
                    existingRecord.TotalBeds = totalBeds.ToString();

                    if (kpmeBedsDoc != null) existingRecord.ParentHospitalKpmebedsDoc = kpmeBedsDoc;
                    if (mouDoc != null) existingRecord.ParentHospitalMoudoc = mouDoc;
                    if (ownerDoc != null) existingRecord.ParentHospitalOwnerNameDoc = ownerDoc;
                    if (postBasicDoc != null) existingRecord.ParentHospitalPostBasicDoc = postBasicDoc;
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
                        HospitalType = hospitalType, // <-- assign hospital type here
                        Kpmebeds = kpmeBeds.ToString(),
                        PostBasicBeds = postBasicBeds.ToString(),
                        TotalBeds = totalBeds.ToString(),

                        ParentHospitalKpmebedsDoc = kpmeBedsDoc,
                        ParentHospitalMoudoc = mouDoc,
                        ParentHospitalOwnerNameDoc = ownerDoc,
                        ParentHospitalPostBasicDoc = postBasicDoc
                    };

                    _context.NursingAffiliatedYearwiseMaterialsData.Add(newRecord);
                }
            }

            _context.SaveChanges();
            TempData["Success"] = "Data saved successfully!";
            return RedirectToAction("Nursing_Affiliated_YearwiseMaterials");
        }

        [HttpGet]
        //[Authorize(AuthenticationSchemes = "CollegeAuth")]
        public async Task<IActionResult> Nursing_TeachingFacultyDetails()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            // 1. Faculty name from session code
            var facultyName = await _context.Faculties
                .Where(f => f.FacultyId.ToString() == facultyCode)
                .Select(f => f.FacultyName)
                .FirstOrDefaultAsync();

            // 2. Departments from MstCourseMaster (FacultyId = 3)
            var departments = await _context.MstCourses
                .Where(c => c.FacultyCode.ToString() == facultyCode)
                .Select(c => new { c.CourseCode, c.CourseName })
                .ToListAsync();

            // 3. Designations from DesignationMaster
            var designations = await _context.DesignationMasters
                .Where(f => f.FacultyCode.ToString() == facultyCode)
                .Select(d => new { d.DesignationCode, d.DesignationName })
                .ToListAsync();

            // 4. Build TeachingFacultyViewModel list
            var intakeDetails = (from dep in departments
                                 from des in designations
                                 select new TeachingFacultyViewModel
                                 {
                                     CollegeCode = collegeCode,
                                     DepartmentCode = dep.CourseCode.ToString(),
                                     DepartmentName = dep.CourseName,
                                     FacultyCode = facultyCode,
                                     Faculty = facultyName,
                                     DesignationCode = des.DesignationCode,
                                     DesignationName = des.DesignationName,
                                     ExistingSeatIntake = "0",   // default
                                     PresentSeatIntake = "0"    // default
                                 }).ToList();

            // 5. Overlay with already saved data
            var existingRecords = await _context.CollegeDesignationDetails
                .Where(c => c.CollegeCode == collegeCode)
                .ToListAsync();

            foreach (var item in intakeDetails)
            {
                var existing = existingRecords.FirstOrDefault(e =>
                    e.CollegeCode == collegeCode &&
                    e.DepartmentCode == item.DepartmentCode &&
                    e.DesignationCode == item.DesignationCode);

                if (existing != null)
                {
                    item.ExistingSeatIntake = existing.RequiredIntake;
                    item.PresentSeatIntake = existing.AvailableIntake;
                }
            }

            return View(intakeDetails);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        //[Authorize(AuthenticationSchemes = "CollegeAuth")]
        public async Task<IActionResult> Nursing_TeachingFacultyDetails(List<TeachingFacultyViewModel> model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            foreach (var item in model)
            {
                // Default to "0" if user leaves blank
                var requiredIntake = string.IsNullOrEmpty(item.ExistingSeatIntake) ? "0" : item.ExistingSeatIntake;
                var availableIntake = string.IsNullOrEmpty(item.PresentSeatIntake) ? "0" : item.PresentSeatIntake;

                // Check if record exists
                var existingRecord = await _context.CollegeDesignationDetails
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.DepartmentCode == item.DepartmentCode &&
                        x.DesignationCode == item.DesignationCode);

                if (existingRecord != null)
                {
                    // Update existing record
                    existingRecord.FacultyCode = item.FacultyCode;
                    existingRecord.Designation = item.DesignationName;
                    existingRecord.Department = item.DepartmentName;
                    existingRecord.RequiredIntake = requiredIntake;
                    existingRecord.AvailableIntake = availableIntake;

                    _context.CollegeDesignationDetails.Update(existingRecord);
                }
                else
                {
                    // Insert new record
                    var newRecord = new CollegeDesignationDetail
                    {
                        FacultyCode = item.FacultyCode,
                        CollegeCode = collegeCode!,
                        Designation = item.DesignationName,
                        DesignationCode = item.DesignationCode,
                        Department = item.DepartmentName,
                        DepartmentCode = item.DepartmentCode,
                        RequiredIntake = requiredIntake,
                        AvailableIntake = availableIntake
                    };

                    _context.CollegeDesignationDetails.Add(newRecord);
                }
            }

            await _context.SaveChangesAsync();

            //TempData["Success"] = "Teaching Faculty Details saved successfully!";
            return RedirectToAction("Nursing_FacultyDetails"); // reload page
        }

        // Helper method to get dropdown data
        private (List<SelectListItem> subjects, List<SelectListItem> designations, List<SelectListItem> departments) GetDropdownLists(string facultyCode)
        {
            var subjectsList = _context.MstCourses
                .Where(c => c.FacultyCode.ToString() == facultyCode)
                .Select(c => new SelectListItem
                {
                    Value = c.CourseCode.ToString(),
                    Text = c.CourseName ?? ""
                })
                .Distinct()
                .ToList();

            //var designationsList = _context.DesignationMasters
            //    .Select(d => new SelectListItem
            //    {
            //        Value = d.DesignationCode,
            //        Text = d.DesignationName ?? ""
            //    })
            //    .ToList();

            var designationsList = _context.DesignationMasters
            .Where(c => c.FacultyCode.ToString() == facultyCode)
            .Select(d => new SelectListItem
            {
                Value = d.DesignationCode,
                Text = d.DesignationName ?? ""
            })
            .ToList();

            var departmentsList = _context.MstCourses
                .Where(e => e.FacultyCode.ToString() == facultyCode)
                .Select(d => new SelectListItem
                {
                    Value = d.CourseCode.ToString(),
                    Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
                })
                .Distinct()
                .ToList();

            return (subjectsList, designationsList, departmentsList);
        }

        //   [HttpGet]
        //   public IActionResult Nursing_FacultyDetails()
        //   {
        //       string collegeCode = HttpContext.Session.GetString("CollegeCode");
        //       string facultyCode = HttpContext.Session.GetString("FacultyCode");

        //       if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
        //       {
        //           TempData["Error"] = "Session expired. Please log in again.";
        //           return RedirectToAction("Login", "Account");
        //       }

        //       var subjectsList = _context.MstCourses
        //           .Where(c => c.FacultyCode.ToString() == facultyCode)
        //           .Select(c => new SelectListItem
        //           {
        //               Value = c.CourseCode.ToString(),
        //               Text = c.CourseName ?? ""
        //           })
        //           .Distinct()
        //           .ToList();

        //       var designationsList = _context.DesignationMasters
        //           .Where(e => e.FacultyCode.ToString() == facultyCode)
        //           .Select(d => new SelectListItem
        //           {
        //               Value = d.DesignationCode,
        //               Text = d.DesignationName ?? ""
        //           })
        //           .ToList();

        //       var departmentsList = _context.MstCourses
        //           .Where(e => e.FacultyCode.ToString() == facultyCode)
        //           .Select(d => new SelectListItem
        //           {
        //               Value = d.CourseCode.ToString(),
        //               Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
        //           })
        //           .ToList();

        //       var facultyDetails = _context.FacultyDetails
        //           .Where(f => f.CollegeCode == collegeCode && f.FacultyCode == facultyCode)
        //           .ToList();

        //       var ahsFacultyWithCollege = _context.NursingFacultyWithColleges // Change table name as appropriate
        //           .Where(f => f.CollegeCode == collegeCode && f.FacultyCode.ToString() == facultyCode)
        //           .ToList();

        //       List<FacultyDetailsViewModel> vmList = new List<FacultyDetailsViewModel>();

        //       if (!facultyDetails.Any() && !ahsFacultyWithCollege.Any())
        //       {
        //           TempData["Info"] = "No faculty records found for this faculty.";
        //           vmList.Add(new FacultyDetailsViewModel
        //           {
        //               Subjects = subjectsList,
        //               Designations = designationsList,
        //               DepartmentDetails = departmentsList
        //           });
        //           return View(vmList);
        //       }

        //       // Join existing faculty details with college faculty data
        //       vmList = (from f1 in facultyDetails
        //                 join f2 in ahsFacultyWithCollege
        //                     on new { f1.Aadhaar, f1.Pan, f1.Designation }
        //                     equals new { Aadhaar = f2.AadhaarNumber, Pan = f2.Pannumber, Designation = f2.Designation }
        //                     into gj
        //                 from sub in gj.DefaultIfEmpty()
        //                 select new FacultyDetailsViewModel
        //                 {
        //                     FacultyDetailId = f1.Id,
        //                     NameOfFaculty = sub?.TeachingFacultyName ?? f1.NameOfFaculty,
        //                     Designation = sub?.Designation ?? f1.Designation,
        //                     Aadhaar = sub?.AadhaarNumber ?? f1.Aadhaar,
        //                     PAN = sub?.Pannumber ?? f1.Pan,
        //                     DepartmentDetail = f1.DepartmentDetails,
        //                     SelectedDepartment = f1.DepartmentDetails,
        //                     Subject = f1.Subject,
        //                     RecognizedPGTeacher = f1.RecognizedPgTeacher,
        //                     Mobile = f1.Mobile,
        //                     Email = f1.Email,
        //                     Subjects = subjectsList,
        //                     Designations = designationsList,
        //                     DepartmentDetails = departmentsList
        //                 }).ToList();

        //       // Add missing faculty from college data
        //       var missingFaculty = ahsFacultyWithCollege
        //           .Where(f2 => !vmList.Any(v => v.Aadhaar == f2.AadhaarNumber && v.PAN == f2.Pannumber))
        //           .Select(f2 => new FacultyDetailsViewModel
        //           {
        //               NameOfFaculty = f2.TeachingFacultyName,
        //               Designation = f2.Designation,
        //               Aadhaar = f2.AadhaarNumber,
        //               PAN = f2.Pannumber,
        //               Subjects = subjectsList,
        //               Designations = designationsList,
        //               DepartmentDetails = departmentsList
        //           })
        //           .ToList();

        //       vmList.AddRange(missingFaculty);

        //       if (!vmList.Any())
        //       {
        //           vmList.Add(new FacultyDetailsViewModel
        //           {
        //               Subjects = subjectsList,
        //               Designations = designationsList,
        //               DepartmentDetails = departmentsList
        //           });
        //       }

        //       return View(vmList);
        //   }

        //   [HttpPost]
        //   [ValidateAntiForgeryToken]
        //   public IActionResult Nursing_FacultyDetails(IList<FacultyDetailsViewModel> model)
        //   {
        //       string collegeCode = HttpContext.Session.GetString("CollegeCode");
        //       string facultyCode = HttpContext.Session.GetString("FacultyCode");

        //       // ✅ Session validation
        //       if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
        //       {
        //           TempData["Error"] = "Session expired. Please log in again.";
        //           return RedirectToAction("Login", "Account");
        //       }

        //       // ✅ Empty list validation
        //       if (model == null || !model.Any())
        //       {
        //           TempData["Error"] = "No data to save.";

        //           // Repopulate dropdown lists before returning to view
        //           model = new List<FacultyDetailsViewModel>
        //{
        //     new FacultyDetailsViewModel
        //     {
        //         Subjects = _context.MstCourses
        //             .Where(c => c.FacultyCode.ToString() == facultyCode)
        //             .Select(c => new SelectListItem { Value = c.CourseCode.ToString(), Text = c.CourseName ?? "" })
        //             .Distinct()
        //             .ToList(),
        //         Designations = _context.DesignationMasters
        //             .Select(d => new SelectListItem { Value = d.DesignationCode, Text = d.DesignationName ?? "" })
        //             .ToList(),
        //         DepartmentDetails = _context.MstCourses
        //             .Where(e => e.FacultyCode.ToString() == facultyCode)
        //             .Select(d => new SelectListItem
        //             {
        //                 Value = d.CourseCode.ToString(),
        //                 Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
        //             })
        //             .ToList()
        //     }
        //};

        //           return View(model);
        //       }

        //       using (var transaction = _context.Database.BeginTransaction())
        //       {
        //           try
        //           {
        //               // Extract all IDs coming from the frontend
        //               var incomingIds = model
        //                   .Where(m => m.FacultyDetailId > 0) // only valid existing IDs
        //                   .Select(m => m.FacultyDetailId)
        //                   .ToHashSet();

        //               // Get all existing faculty for this college/faculty code
        //               var existingFaculty = _context.FacultyDetails
        //                   .Where(f => f.CollegeCode == collegeCode && f.FacultyCode == facultyCode)
        //                   .ToList();

        //               // 🔹 1. DELETE records that are NOT in the incoming model
        //               var toDelete = existingFaculty
        //                   .Where(f => !incomingIds.Contains(f.Id))
        //                   .ToList();

        //               if (toDelete.Any())
        //               {
        //                   _context.FacultyDetails.RemoveRange(toDelete);
        //               }

        //               // 🔹 2. ADD or UPDATE incoming data
        //               foreach (var m in model)
        //               {
        //                   string name = m.NameOfFaculty?.Trim() ?? "N/A";
        //                   string designation = m.Designation?.Trim() ?? "N/A";
        //                   string subject = m.Subject?.Trim() ?? "N/A";
        //                   string mobile = string.IsNullOrWhiteSpace(m.Mobile) ? "N/A" : m.Mobile.Trim();
        //                   string email = string.IsNullOrWhiteSpace(m.Email) ? "N/A" : m.Email.Trim();
        //                   string pan = m.PAN?.Trim() ?? "N/A";
        //                   string aadhaar = m.Aadhaar?.Trim() ?? "N/A";
        //                   string dept = m.SelectedDepartment?.Trim() ?? "N/A";
        //                   string recognizedPG = m.RecognizedPGTeacher?.Trim() ?? "N/A";

        //                   byte[] guideRecognitionDocBytes = null;
        //                   if (m.GuideRecognitionDoc != null)
        //                   {
        //                       guideRecognitionDocBytes = ConvertFileToBytes(m.GuideRecognitionDoc);
        //                   }

        //                   var existing = existingFaculty.FirstOrDefault(f => f.Id == m.FacultyDetailId);

        //                   if (existing != null)
        //                   {
        //                       // ✅ Update
        //                       existing.NameOfFaculty = name;
        //                       existing.Designation = designation;
        //                       existing.RecognizedPgTeacher = recognizedPG;
        //                       existing.Mobile = mobile;
        //                       existing.Email = email;
        //                       existing.Pan = pan;
        //                       existing.Aadhaar = aadhaar;
        //                       existing.DepartmentDetails = dept;
        //                       existing.Subject = subject;

        //                       if (guideRecognitionDocBytes != null)
        //                           existing.GuideRecognitionDoc = guideRecognitionDocBytes;

        //                       _context.FacultyDetails.Update(existing);
        //                   }
        //                   else
        //                   {
        //                       // ✅ Insert new
        //                       var faculty = new FacultyDetail
        //                       {
        //                           CollegeCode = collegeCode,
        //                           FacultyCode = facultyCode,
        //                           NameOfFaculty = name,
        //                           Subject = subject,
        //                           Designation = designation,
        //                           RecognizedPgTeacher = recognizedPG,
        //                           Mobile = mobile,
        //                           Email = email,
        //                           Pan = pan,
        //                           Aadhaar = aadhaar,
        //                           DepartmentDetails = dept,
        //                           GuideRecognitionDoc = guideRecognitionDocBytes
        //                       };

        //                       _context.FacultyDetails.Add(faculty);
        //                   }
        //               }

        //               // 🔹 3. Save all changes once
        //               _context.SaveChanges();

        //               transaction.Commit();

        //               //TempData["Success"] = "Faculty records saved successfully!";
        //               return RedirectToAction("NursingExamResults");
        //           }
        //           catch (Exception ex)
        //           {
        //               transaction.Rollback();

        //               // ✅ Log and show detailed error
        //               TempData["Error"] = "Error saving faculty records: " + ex.Message;

        //               // ✅ Repopulate dropdowns again so View doesn’t break
        //               foreach (var m in model)
        //               {
        //                   m.Subjects = _context.MstCourses
        //                       .Where(c => c.FacultyCode.ToString() == facultyCode)
        //                       .Select(c => new SelectListItem { Value = c.CourseCode.ToString(), Text = c.CourseName ?? "" })
        //                       .Distinct()
        //                       .ToList();

        //                   m.Designations = _context.DesignationMasters
        //                       .Select(d => new SelectListItem { Value = d.DesignationCode, Text = d.DesignationName ?? "" })
        //                       .ToList();

        //                   m.DepartmentDetails = _context.MstCourses
        //                       .Where(e => e.FacultyCode.ToString() == facultyCode)
        //                       .Select(d => new SelectListItem
        //                       {
        //                           Value = d.CourseCode.ToString(),
        //                           Text = (d.CoursePrefix ?? "") + " " + (d.SubjectName ?? "")
        //                       })
        //                       .ToList();
        //               }

        //               return View(model);
        //           }
        //       }
        //   }


        //[HttpGet]
        //public IActionResult Nursing_FacultyDetails()
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
        //        .Where(c => c.FacultyCode.ToString() == facultyCode)
        //        .Select(d => new SelectListItem
        //        {
        //            Value = d.DesignationCode,
        //            Text = d.DesignationName ?? ""
        //        })
        //        .ToList();

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
        //public IActionResult Nursing_FacultyDetails(IList<FacultyDetailsViewModel> model)
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
        //            return RedirectToAction("NursingExamResults");
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


        // Utility method for converting uploaded files to bytes
        //private byte[] ConvertFileToBytes(IFormFile file)
        //{
        //    if (file == null) return null;
        //    using (var ms = new MemoryStream())
        //    {
        //        file.CopyTo(ms);
        //        return ms.ToArray();
        //    }
        //}

        [HttpGet]
        public IActionResult Nursing_Nursing()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
            {
                return View(new List<NursingFacultyViewModel>());
            }

            // Fetch subjects for this faculty
            var subjectsList = _context.MstCourses
               .Where(c => c.FacultyCode.ToString() == facultyCode)
               .Select(c => new SelectListItem
               {
                   Value = c.CourseCode.ToString(),
                   Text = c.CourseName
               })
               .Distinct()
               .ToList();

            // Fetch faculty list
            var facultyList = _context.NursingFacultyWithColleges
                .Where(f => f.CollegeCode == collegeCode && f.FacultyCode.ToString() == facultyCode)
                .Select(f => new NursingFacultyViewModel
                {
                    //Id = f.Id,
                    TeachingFacultyName = f.TeachingFacultyName,
                    Designation = f.Designation,
                    AadhaarNumber = f.AadhaarNumber,
                    PANNumber = f.Pannumber,

                    // New fields
                    Subject = "", // initial value, can be updated
                    Mobile = "",
                    Email = "",
                    RecognizedPgTeacherDate = null,

                    // **Important: assign Subjects list for each row**
                    Subjects = subjectsList
                })
                .ToList();

            return View(facultyList);
        }

        [HttpPost]
        public IActionResult Nursing_Nursing(List<NursingFacultyViewModel> FacultyList)
        {
            if (FacultyList == null || !FacultyList.Any())
                return RedirectToAction("Nursing_Nursing");

            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Nursing_Nursing"); // session issue

            foreach (var faculty in FacultyList)
            {
                // Check if record already exists (you can match by Id or AadhaarNumber/PANNumber)
                var existingFaculty = _context.NursingFacultyDetails
                    .FirstOrDefault(x => x.Aadhaar == faculty.AadhaarNumber
                                      && x.CollegeCode == collegeCode
                                      && x.FacultyCode == facultyCode);

                if (existingFaculty != null)
                {
                    // ✅ UPDATE existing record
                    existingFaculty.NameOfFaculty = faculty.TeachingFacultyName;
                    existingFaculty.Designation = faculty.Designation;
                    existingFaculty.Aadhaar = faculty.AadhaarNumber;
                    existingFaculty.Pan = faculty.PANNumber;
                    existingFaculty.Subject = faculty.Subject ?? "N/A";
                    existingFaculty.Mobile = string.IsNullOrWhiteSpace(faculty.Mobile) ? "N/A" : faculty.Mobile;
                    existingFaculty.Email = faculty.Email;
                    existingFaculty.RecognizedPgTeacher = faculty.RecognizedPgTeacherDate?.ToString("yyyy-MM-dd");
                    //existingFaculty.DepartmentDetails = faculty.DepartmentDetails;
                    //existingFaculty.LastUpdatedDate = DateTime.Now; // optional column
                }
                else
                {
                    // ✅ INSERT new record
                    var newFaculty = new NursingFacultyDetail
                    {
                        NameOfFaculty = faculty.TeachingFacultyName,
                        Designation = faculty.Designation,
                        Aadhaar = faculty.AadhaarNumber,
                        Pan = faculty.PANNumber,
                        Subject = faculty.Subject ?? "N/A",
                        Mobile = string.IsNullOrWhiteSpace(faculty.Mobile) ? "N/A" : faculty.Mobile,
                        Email = faculty.Email,
                        RecognizedPgTeacher = faculty.RecognizedPgTeacherDate?.ToString("yyyy-MM-dd"),
                        //DepartmentDetails = faculty.DepartmentDetails,
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        //CreatedDate = DateTime.Now // optional column
                    };

                    _context.NursingFacultyDetails.Add(newFaculty);
                }
            }

            _context.SaveChanges();

            TempData["Success"] = "Faculty records saved/updated successfully!";
            return RedirectToAction("Nursing_Nursing");
        }


        [HttpGet]
        public IActionResult FacultyExamResults()
        {
            int currentYear = DateTime.Now.Year;

            // Define course durations
            var courseDurations = new Dictionary<string, int>
    {
        { "BSc", 4 },
        { "PBSc", 3 },
        { "MSc", 2 }
    };

            // Fetch existing exam results from DB for this college
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var existingResults = _context.FacultyExamResults
                                          .Where(f => f.Collegecode == collegeCode)
                                          .ToList();

            var model = new List<CourseExamStatsViewModel>();

            foreach (var course in courseDurations)
            {
                for (int i = 0; i < course.Value; i++)
                {
                    int year = currentYear - i;

                    // Try to find existing record for this course/year
                    var existing = existingResults
                        .FirstOrDefault(f => f.Course == course.Key && f.Year == year);

                    // Safely calculate percentage
                    string percentage = "";
                    if (existing != null && (existing.ExamappearedCount ?? 0) > 0)
                    {
                        double pct = (double)(existing.Passedoutcount ?? 0) / existing.ExamappearedCount.Value * 100;
                        percentage = pct.ToString("0.00");
                    }

                    model.Add(new CourseExamStatsViewModel
                    {
                        CourseName = course.Key,
                        Year = year,
                        ExamAppearedOut = existing?.ExamappearedCount ?? 0,
                        PassedOutCount = existing?.Passedoutcount ?? 0,
                        Percentage = percentage
                    });
                }
            }

            return View(model);
        }


        // ✅ POST Method — Save to DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FacultyExamResults(List<CourseExamStatsViewModel> model)
        {
            //if (ModelState.IsValid)
            //{
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            foreach (var item in model)
            {
                var data = new FacultyExamResult
                {
                    Collegecode = collegeCode,
                    Facultycode = facultyCode,
                    Course = item.CourseName,
                    Year = item.Year,
                    ExamappearedCount = item.ExamAppearedOut,
                    Passedoutcount = item.PassedOutCount,
                    Yearofpercentage = item.PassedOutCount != 0 && item.ExamAppearedOut != 0
                        ? (decimal)item.PassedOutCount / item.ExamAppearedOut * 100
                        : 0
                };

                _context.FacultyExamResults.Add(data);
            }

            _context.SaveChanges();
            TempData["Success"] = "Application Saved successfully!";
            return RedirectToAction("FacultyExamResults");
        }

        //return View(model);
        //}
        // GET: FacultyIntake/Create
        // GET: FacultyIntake/Create
        [HttpGet]
        public async Task<IActionResult> Nursing_UGPG()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode") ?? "";
            var facultyCodeStr = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(facultyCodeStr) || !int.TryParse(facultyCodeStr, out int facultyCode))
            {
                TempData["ErrorMessage"] = "FacultyCode not found in session or invalid.";
                return RedirectToAction("Error"); // or return the page with the error message
            }

            // Add "Additional" to dropdown options
            ViewBag.FreshIncreaseOptions = new[] { "Fresh", "Increase", "Additional" };

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
                .Where(x => x.CollegeCode == collegeCode)     // Filter data by logged-in CollegeCode
                .OrderByDescending(x => x.Id)
                .Take(1000)
                .ToListAsync();

            ViewBag.RepositoryRows = repositoryRows;

            return View(new FacultyIntakeViewModel());
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
                case "INC": fileBytes = row.Inc; break;
                case "KNMC": fileBytes = row.Knmc; break;
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
                .AnyAsync(r => r.CourseName == courseName && r.FreshOrIncrease == "Fresh");
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
        public async Task<IActionResult> Nursing_UGPG(FacultyIntakeViewModel model)
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

                ViewBag.FreshIncreaseOptions = new[] { "Fresh", "Increase", "Additional" };
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
            entity.Inc = await ToBytes(model.INCUploadFile);
            entity.Knmc = await ToBytes(model.KNMCUploadFile);
            entity.Gok = await ToBytes(model.GOKUploadFile);

            _context.UgandPgrepositories.Add(entity);
            await _context.SaveChangesAsync();

            //TempData["SuccessMessage"] = "Faculty intake saved successfully.";
            return RedirectToAction(nameof(Nursing_UGPG));
        }

        [HttpGet]
        public IActionResult ManageDesignationIntake()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            // Get all designations from master table
            var designationList = _context.DesignationMasters
                .Where(e => e.FacultyCode.ToString() == facultyCode)
                .Select(d => new CollegeDesignationDetailsViewModel
                {
                    DesignationCode = d.DesignationCode,
                    Designation = d.DesignationName,

                }).ToList();

            // Optionally fetch already saved intake to pre-fill (if updating)
            var savedIntake = _context.CollegeDesignationDetails
                .Where(c => c.CollegeCode == collegeCode && c.FacultyCode == facultyCode)
                .ToList();

            foreach (var item in designationList)
            {
                var exist = savedIntake.FirstOrDefault(x => x.DesignationCode == item.DesignationCode);

                if (exist != null && int.TryParse(exist.AvailableIntake, out int intake))
                    item.PresentIntake = intake;
                else
                    item.PresentIntake = 0;
            }
            return View(designationList);
        }

        [HttpPost]
        public IActionResult ManageDesignationIntake(List<CollegeDesignationDetailsViewModel> model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            foreach (var item in model)
            {
                var existing = _context.CollegeDesignationDetails.FirstOrDefault(x =>
                    x.CollegeCode == collegeCode && x.FacultyCode == facultyCode &&
                    x.DesignationCode == item.DesignationCode);

                string requiredIntakeValue = item.PresentIntake.ToString() ?? "0";

                if (existing != null)
                {
                    existing.AvailableIntake = item.PresentIntake.ToString();
                    existing.RequiredIntake = requiredIntakeValue;
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
                        AvailableIntake = item.PresentIntake.ToString(),
                        RequiredIntake = requiredIntakeValue,

                    });
                }
            }


            _context.SaveChanges();
            return RedirectToAction("Nursing_FacultyDetails");
        }

        [HttpGet]
        public async Task<IActionResult> NursingExamResults()
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
        new() { Course = "B.Sc", Year = "1st Year" },
        new() { Course = "B.Sc", Year = "2nd Year" },
        new() { Course = "B.Sc", Year = "3rd Year" },
        new() { Course = "B.Sc", Year = "4th Year" },
        new() { Course = "P.B.B.Sc", Year = "1st Year" },
        new() { Course = "P.B.B.Sc", Year = "2nd Year" },
        new() { Course = "M.Sc", Year = "1st Year" },
        new() { Course = "M.Sc", Year = "2nd Year" }
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
        public async Task<IActionResult> NursingExamResults(List<NursingExamResultViewModel> model)
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

                return RedirectToAction(nameof(NursingExamResults));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while saving data: {ex.Message}";
                return View(model ?? new List<NursingExamResultViewModel>());
            }
        }

    }
}