using Medical_Affiliation.Controllers;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json; // At the top of your controller
using System.Data;


namespace Medical_Affiliation.Controllers
{

    public class Aff_AHS_ContinousApplicationController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public Aff_AHS_ContinousApplicationController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> GetDistrictsByState(string stateId)
        {
            if (string.IsNullOrEmpty(stateId))
            {
                return Json(new List<object>());
            }

            var districts = await _context.DistrictMasters
                .Where(d => d.StateId == stateId)
                .Select(d => new
                {
                    districtId = d.DistrictId,
                    districtName = d.DistrictName
                })
                .ToListAsync();

            return Json(districts);
        }

        [HttpGet]
        public async Task<IActionResult> GetTaluksByDistrict(string districtId)
        {
            if (string.IsNullOrEmpty(districtId))
            {
                return Json(new List<object>());
            }

            var taluks = await _context.TalukMasters
                .Where(t => t.DistrictId == districtId)
                .Select(t => new
                {
                    talukId = t.TalukId,
                    talukName = t.TalukName
                })
                .ToListAsync();

            return Json(taluks);
        }

        //[HttpGet]
        //public async Task<IActionResult> TrustSocietyDetails()
        //{
        //    string collegeCode = HttpContext.Session.GetString("CollegeCode");

        //    var existing = await _context.TrustSocietyIntakes
        //        .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode);

        //    string existingStateId = existing?.State ?? string.Empty;
        //    string existingDistrictId = existing?.District ?? string.Empty;

        //    var viewModel = new TrustSocietyViewModel
        //    {
        //        CollegeCode = collegeCode,

        //        CategoryOfOrganization = existing?.CategoryOfOrganisation,
        //        TypeOfOrganization = existing?.TypeOfOrganization,
        //        TrustName = existing?.TrustName,
        //        PresidentName = existing?.PresidentName,
        //        AadhaarOfPresident = existing?.AadhaarNumber,
        //        PANNumber = existing?.Pannumber,
        //        Address = existing?.Address,
        //        Village = existing?.Village,
        //        State = existing?.State,
        //        District = existing?.District,
        //        Taluk = existing?.Taluk,
        //        PinCode = existing?.PinCode,
        //        STDCode = existing?.Stdcode,
        //        Landline = existing?.Landline,
        //        Fax = existing?.Fax,
        //        Email = existing?.Email,
        //        Mobile = existing?.Mobile,
        //        RegistrationNumber = existing?.RegistrationNumber,
        //        RegistrationDate = existing?.RegistrationDate,
        //        Amendments = existing?.Amendments,
        //        ExistingTrustName = existing?.ExistingTrustName,
        //        GOKObtainedTrustName = existing?.GokobtainedTrustName,
        //        ChangesInTrustName = existing?.ChangesInTrustName,
        //        OtherNursingCollegeInCity = existing?.OtherNursingCollegeInCity,
        //        OtherPhysiotherapyCollegeInCity = existing?.OtherPhysiotherapyCollegeInCity,

        //        ContactPersonName = existing?.ContactPersonName,
        //        ContactPersonRelation = existing?.ContactPersonRelation,
        //        ContactPersonMobile = existing?.ContactPersonMobile,
        //         = existing?.CertificationNumber,

        //        StateList = await _context.StateMasters
        //            .Select(s => new SelectListItem { Value = s.StateId, Text = s.StateName })
        //            .ToListAsync(),

        //        DistrictList = await _context.DistrictMasters
        //            .Where(d => d.StateId == existingStateId)
        //            .Select(d => new SelectListItem { Value = d.DistrictId, Text = d.DistrictName })
        //            .ToListAsync(),

        //        TalukList = await _context.TalukMasters
        //            .Where(t => t.DistrictId == existingDistrictId)
        //            .Select(t => new SelectListItem { Value = t.TalukId, Text = t.TalukName })
        //            .ToListAsync()
        //    };

        //    return View(viewModel);
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> TrustSocietyDetails(TrustSocietyViewModel model)
        //{
        //    if (model == null)
        //    {
        //        TempData["Error"] = "Form submission failed. Please try again.";
        //        return RedirectToAction("TrustSocietyDetails");
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        // Repopulate dropdowns on validation failure
        //        await PopulateDropdowns(model);
        //        return View(model);
        //    }

        //    var collegeCode = HttpContext.Session.GetString("CollegeCode");

        //    // Convert file uploads to byte[]
        //    byte[] bankStatement = model.BankStatementFile != null
        //        ? await ConvertToBytes(model.BankStatementFile)
        //        : null;

        //    byte[] regCertificate = model.RegistrationCertificateFile != null
        //        ? await ConvertToBytes(model.RegistrationCertificateFile)
        //        : null;

        //    byte[] trustMemberDetails = model.RegisteredTrustMemberDetails != null
        //        ? await ConvertToBytes(model.RegisteredTrustMemberDetails)
        //        : null;

        //    var existing = await _context.TrustSocietyIntakes
        //        .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode);

        //    if (existing != null)
        //    {
        //        // UPDATE existing record
        //        existing.CollegeCode = collegeCode;
        //        existing.CategoryOfOrganisation = model.CategoryOfOrganization;
        //        existing.TypeOfOrganization = model.TypeOfOrganization;
        //        existing.TrustName = model.TrustName;
        //        existing.PresidentName = model.PresidentName;
        //        existing.AadhaarNumber = model.AadhaarNumber;
        //        existing.Pannumber = model.PANNumber;
        //        existing.Address = model.Address;
        //        existing.Village = model.Village;
        //        existing.State = model.State;
        //        existing.District = model.District;
        //        existing.Taluk = model.Taluk;
        //        existing.PinCode = model.PinCode;
        //        existing.Stdcode = model.STDCode;
        //        existing.Landline = model.Landline;
        //        existing.Fax = model.Fax;
        //        existing.Email = model.Email;
        //        existing.Mobile = model.Mobile;
        //        existing.RegistrationNumber = model.RegistrationNumber;
        //        existing.RegistrationDate = model.RegistrationDate;
        //        existing.Amendments = model.Amendments;
        //        existing.ExistingTrustName = model.ExistingTrustName;
        //        existing.GokobtainedTrustName = model.GOKObtainedTrustName;
        //        existing.ChangesInTrustName = model.ChangesInTrustName;
        //        existing.OtherNursingCollegeInCity = model.OtherNursingCollegeInCity;
        //        existing.OtherPhysiotherapyCollegeInCity = model.OtherPhysiotherapyCollegeInCity;
        //        existing.ContactPersonName = model.ContactPersonName;
        //        existing.ContactPersonRelation = model.ContactPersonRelation;
        //        existing.ContactPersonMobile = model.ContactPersonMobile;
        //        existing.CertificationNumber = model.CertificationNumber;

        //        // File updates (only if new file uploaded)
        //        if (bankStatement != null) existing.BankStatementFile = bankStatement;
        //        if (regCertificate != null) existing.RegistrationCertificateFile = regCertificate;
        //        if (trustMemberDetails != null) existing.RegisteredTrustMemberDetails = trustMemberDetails;

        //        _context.TrustSocietyIntakes.Update(existing);
        //    }
        //    else
        //    {
        //        // CREATE new record
        //        var entity = new TrustSocietyIntake
        //        {
        //            CollegeCode = collegeCode,
        //            CategoryOfOrganisation = model.CategoryOfOrganization,
        //            TypeOfOrganization = model.TypeOfOrganization,
        //            TrustName = model.TrustName,
        //            PresidentName = model.PresidentName,
        //            AadhaarNumber = model.AadhaarNumber,
        //            Pannumber = model.PANNumber,
        //            Address = model.Address,
        //            Village = model.Village,
        //            State = model.State,
        //            District = model.District,
        //            Taluk = model.Taluk,
        //            PinCode = model.PinCode,
        //            Stdcode = model.STDCode,
        //            Landline = model.Landline,
        //            Fax = model.Fax,
        //            Email = model.Email,
        //            Mobile = model.Mobile,
        //            RegistrationNumber = model.RegistrationNumber,
        //            RegistrationDate = model.RegistrationDate,
        //            Amendments = model.Amendments,
        //            ExistingTrustName = model.ExistingTrustName,
        //            GokobtainedTrustName = model.GOKObtainedTrustName,
        //            ChangesInTrustName = model.ChangesInTrustName,
        //            OtherNursingCollegeInCity = model.OtherNursingCollegeInCity,
        //            OtherPhysiotherapyCollegeInCity = model.OtherPhysiotherapyCollegeInCity,
        //            ContactPersonName = model.ContactPersonName,
        //            ContactPersonRelation = model.ContactPersonRelation,
        //            ContactPersonMobile = model.ContactPersonMobile,
        //            CertificationNumber = model.CertificationNumber,

        //            // File uploads
        //            BankStatementFile = bankStatement,
        //            RegistrationCertificateFile = regCertificate,
        //            RegisteredTrustMemberDetails = trustMemberDetails
        //        };

        //        _context.TrustSocietyIntakes.Add(entity);
        //    }

        //    await _context.SaveChangesAsync();

        //    TempData["TrustSocietyDetails"] = "Trust/Society details saved successfully!";
        //    return RedirectToAction("TrustSocietyDetails");
        //}

        //code added by ram on 08-12-25

        [HttpGet]
        public IActionResult CA_LibraryDetails()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");

            var existing = _context.CaLibraryDetails
                .FirstOrDefault(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if (existing == null)
            {
                return View(new CA_LibraryDetailsViewModel
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,

                });
            }

            var vm = new CA_LibraryDetailsViewModel
            {
                RegistrationNo = existing.RegistrationNo,
                CollegeCode = existing.CollegeCode,
                FacultyCode = existing.FacultyCode,

                TotalNursingBooks = existing.TotalNursingBooks,
                TotalNursingJournals = existing.TotalNursingJournals,
                InternetFacility = existing.InternetFacility,
                TotalThesis = existing.TotalThesis,
                TotalEBooks = existing.TotalEbooks,
                BooksPurchasedLastYear = existing.BooksPurchasedLastYear,
                TotalBudget = existing.TotalBudget,
                IndependentBuilding = existing.IndependentBuilding,
                TotalFloorAreaSqFt = existing.TotalFloorAreaSqFt,
            };

            return View(vm);
            //return View("~/Views/Aff_CA_Nursing/CA_LibraryDetails.cshtml", vm);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CA_LibraryDetails(CA_LibraryDetailsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // This will show per-field errors thanks to <span asp-validation-for>
                return View(model);
            }

            // ... rest of your save logic (unchanged)
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");

            var existing = _context.CaLibraryDetails
                .FirstOrDefault(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if (existing == null)
            {
                existing = new CaLibraryDetail
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode
                };
                _context.CaLibraryDetails.Add(existing);
            }

            // Assign values
            existing.TotalNursingBooks = model.TotalNursingBooks;
            existing.TotalNursingJournals = model.TotalNursingJournals;
            existing.InternetFacility = model.InternetFacility;
            existing.TotalThesis = model.TotalThesis;
            existing.TotalEbooks = model.TotalEBooks;
            existing.BooksPurchasedLastYear = model.BooksPurchasedLastYear;
            existing.TotalBudget = model.TotalBudget;
            existing.IndependentBuilding = model.IndependentBuilding;
            existing.TotalFloorAreaSqFt = model.TotalFloorAreaSqFt;

            _context.SaveChanges();

            TempData["Success"] = "Library Details Saved Successfully!";
            return RedirectToAction("CA_LibraryStaffDetails", "Aff_AHS_ContinousApplication");
        }

        //------------------------------2nd page--------------------------------

        [HttpGet]
        public async Task<IActionResult> CA_LibraryStaffDetails()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            var existingList = await _context.CaLibraryStaffDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .Select(x => new CA_LibraryStaffDetailsViewModel
                {
                    Id = x.Id,
                    StaffName = x.StaffName,
                    Qualification = x.Qualification,
                    ExperienceYears = x.ExperienceYears,
                    Remarks = x.Remarks
                })
                .ToListAsync();

            var vm = new CA_LibraryStaffDetailsViewModel
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                ExistingStaff = existingList
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> CA_LibraryStaffDetails(CA_LibraryStaffDetailsViewModel model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            // Trim all string inputs
            model.StaffName = model.StaffName?.Trim();
            model.Qualification = model.Qualification?.Trim();
            model.Remarks = model.Remarks?.Trim();

            // If all fields are empty or whitespace → treat as "no action", just reload
            if (string.IsNullOrWhiteSpace(model.StaffName) &&
                string.IsNullOrWhiteSpace(model.Qualification) &&
                !model.ExperienceYears.HasValue &&
                string.IsNullOrWhiteSpace(model.Remarks))
            {
                return RedirectToAction("CA_LibraryStaffDetails");
            }

            // Run full validation
            if (!ModelState.IsValid)
            {
                // Add a custom general message
                ModelState.AddModelError("", "Please enter the details completely for all fields.");

                // Reload existing staff so table remains populated
                var existingList = await _context.CaLibraryStaffDetails
                    .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                    .Select(x => new CA_LibraryStaffDetailsViewModel
                    {
                        Id = x.Id,
                        StaffName = x.StaffName,
                        Qualification = x.Qualification,
                        ExperienceYears = x.ExperienceYears,
                        Remarks = x.Remarks
                    })
                    .ToListAsync();

                model.ExistingStaff = existingList;

                return View(model);
            }

            // All valid → Save
            var entity = new CaLibraryStaffDetail
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                RegistrationNo = model.RegistrationNo,
                StaffName = model.StaffName,
                Qualification = model.Qualification,
                ExperienceYears = model.ExperienceYears,
                Remarks = model.Remarks
            };

            _context.CaLibraryStaffDetails.Add(entity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Library staff added successfully!";
            return RedirectToAction("CA_LibraryStaffDetails");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CA_UpdateLibraryStaff([FromBody] CA_LibraryStaffDetailsViewModel model)
        {
            if (model == null || !model.Id.HasValue || model.Id <= 0)
            {
                return Json(new { success = false, message = "Invalid request. Staff ID is missing." });
            }

            // === Manual Server-Side Validation (same as Add) ===
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(model.StaffName))
                errors.Add("Staff Name is required.");

            if (string.IsNullOrWhiteSpace(model.Qualification))
                errors.Add("Qualification is required.");

            if (!model.ExperienceYears.HasValue || model.ExperienceYears < 0 || model.ExperienceYears > 60)
                errors.Add("Experience must be between 0 and 60 years.");

            if (string.IsNullOrWhiteSpace(model.Remarks))
                errors.Add("Remarks are required.");

            if (errors.Any())
            {
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            // === Find and Update Entity ===
            var existing = await _context.CaLibraryStaffDetails
                .FirstOrDefaultAsync(x => x.Id == model.Id.Value);

            if (existing == null)
            {
                return Json(new { success = false, message = "Staff record not found." });
            }

            existing.StaffName = model.StaffName?.Trim();
            existing.Qualification = model.Qualification?.Trim();
            existing.ExperienceYears = model.ExperienceYears;
            existing.Remarks = model.Remarks?.Trim();

            // Reliable update
            _context.Entry(existing).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Staff details updated successfully!" });
        }



        public async Task<IActionResult> CA_DeleteLibraryStaff(int id)
        {
            var item = await _context.CaLibraryStaffDetails.FindAsync(id);
            if (item != null)
            {
                _context.CaLibraryStaffDetails.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("CA_LibraryStaffDetails");
        }


        //----------------------------3rd page --------------------------------
        [HttpGet]
        public async Task<IActionResult> CA_DepartmentLibraryDetails()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");

            int facultyCodeInt = Convert.ToInt32(facultyCode);

            var deptList = await _context.MstCourses
                .Where(x => x.FacultyCode == facultyCodeInt)
                .Select(x => new SelectListItem
                {
                    Value = x.CourseCode.ToString(),
                    Text = x.CourseName
                })
                .ToListAsync();

            var existingList = await _context.CaDepartmentLibraryDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .Join(_context.MstCourses,
                      dep => dep.DepartmentCode,
                      course => course.CourseCode.ToString(),
                      (dep, course) => new CA_DepartmentLibraryRowVM  // ← Use RowVM
                      {
                          Id = dep.Id,
                          DepartmentCode = dep.DepartmentCode,
                          DepartmentName = course.CourseName,
                          TotalBooks = dep.TotalBooks,
                          BooksAdded = dep.BooksAdded,
                          CurrentJournals = dep.CurrentJournals
                      })
                .ToListAsync();

            var vm = new CA_DepartmentLibraryDetailsViewModel
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                DepartmentList = deptList,
                ExistingList = existingList
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CA_DepartmentLibraryDetails(CA_DepartmentLibraryDetailsViewModel model)
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");

            // Server-side validation
            if (!ModelState.IsValid)
            {
                // Repopulate dropdown + existing list on error
                await RepopulateViewModel(model, collegeCode, facultyCode);
                return View(model);
            }

            // Check if record for this department already exists → Update instead of Add
            var existing = await _context.CaDepartmentLibraryDetails
                .FirstOrDefaultAsync(x => x.CollegeCode == collegeCode &&
                                          x.FacultyCode == facultyCode &&
                                          x.DepartmentCode == model.DepartmentCode);

            if (existing != null)
            {
                // UPDATE existing row
                existing.TotalBooks = model.TotalBooks;
                existing.BooksAdded = model.BooksAdded;
                existing.CurrentJournals = model.CurrentJournals;
                // No need for Update() call — entity is tracked
            }
            else
            {
                // ADD new row
                var newEntity = new CaDepartmentLibraryDetail
                {
                    CollegeCode = collegeCode,
                    FacultyCode = facultyCode,
                    DepartmentCode = model.DepartmentCode,
                    TotalBooks = model.TotalBooks ?? 0,
                    BooksAdded = model.BooksAdded ?? 0,
                    CurrentJournals = model.CurrentJournals ?? 0
                };
                _context.CaDepartmentLibraryDetails.Add(newEntity);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Department library details saved successfully!";
            return RedirectToAction("CA_DepartmentLibraryDetails");
        }

        // Helper to repopulate dropdown + existing list (used on validation error)
        private async Task RepopulateViewModel(CA_DepartmentLibraryDetailsViewModel model, string collegeCode, string facultyCode)
        {
            int facultyCodeInt = Convert.ToInt32(facultyCode);

            model.DepartmentList = await _context.MstCourses
                .Where(x => x.FacultyCode == facultyCodeInt)
                .Select(x => new SelectListItem
                {
                    Value = x.CourseCode.ToString(),
                    Text = x.CourseName
                })
                .ToListAsync();

            model.ExistingList = await _context.CaDepartmentLibraryDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .Join(_context.MstCourses,
                      dep => dep.DepartmentCode,
                      course => course.CourseCode.ToString(),
                      (dep, course) => new CA_DepartmentLibraryRowVM  // ← Use RowVM here
                      {
                          Id = dep.Id,
                          DepartmentCode = dep.DepartmentCode,
                          DepartmentName = course.CourseName,
                          TotalBooks = dep.TotalBooks,
                          BooksAdded = dep.BooksAdded,
                          CurrentJournals = dep.CurrentJournals
                      })
                .ToListAsync();
        }


        public async Task<IActionResult> CA_DeleteDepartmentLibrary(int id)
        {
            var item = await _context.CaDepartmentLibraryDetails.FindAsync(id);
            if (item != null)
            {
                _context.CaDepartmentLibraryDetails.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("CA_DepartmentLibraryDetails");
        }

        [HttpPost]
        public async Task<IActionResult> CA_UpdateDepartmentLibrary([FromBody] CA_DepartmentLibraryDetailsViewModel model)
        {
            if (model == null || model.Id <= 0)
                return Json(new { success = false, message = "Invalid data." });

            var existing = await _context.CaDepartmentLibraryDetails.FindAsync(model.Id);
            if (existing == null)
                return Json(new { success = false, message = "Record not found." });

            existing.DepartmentCode = model.DepartmentCode;
            existing.TotalBooks = model.TotalBooks ?? 0;
            existing.BooksAdded = model.BooksAdded ?? 0;
            existing.CurrentJournals = model.CurrentJournals ?? 0;

            _context.Entry(existing).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        //----------------------------------4th page----------------------------
        //[HttpGet]
        //public async Task<IActionResult> CA_Nursing_CollectionDevelopment()
        //{
        //    string collegeCode = HttpContext.Session.GetString("CollegeCode");
        //    string facultyCode = HttpContext.Session.GetString("FacultyCode");

        //    if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
        //        return RedirectToAction("Login", "Account");

        //    var existing = await _context.CaNursingCollectionDevelopments
        //        .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
        //        .ToListAsync();

        //    if (!existing.Any())
        //    {
        //        var defaultDocs = new List<string>
        //{
        //    "Books",
        //    "Current Journals (No. of Titles)",
        //    "Bound Volumes of Journals",
        //    "Monographs",
        //    "Govt. Publications",
        //    "Thesis / Dissertation",
        //    "Reports / Pamphlets",
        //    "Microfilms / Microfiche",
        //    "Slides",
        //    "Audio Cassettes",
        //    "Video Cassettes"
        //};

        //        foreach (var doc in defaultDocs)
        //        {
        //            _context.CaNursingCollectionDevelopments.Add(new CaNursingCollectionDevelopment
        //            {
        //                CollegeCode = collegeCode,
        //                FacultyCode = facultyCode,
        //                DocumentType = doc,
        //                TotalCurrentYear = 0,
        //                AddedPreviousYear = 0
        //            });
        //        }
        //        await _context.SaveChangesAsync();

        //        existing = await _context.CaNursingCollectionDevelopments
        //            .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
        //            .ToListAsync();
        //    }

        //    var vm = new CA_Nursing_CollectionDevelopmentViewModel
        //    {
        //        CollegeCode = collegeCode,
        //        FacultyCode = facultyCode,
        //        ExistingList = existing.Select(x => new CA_Nursing_CollectionDevelopmentRowVM
        //        {
        //            Id = x.Id,
        //            DocumentType = x.DocumentType,
        //            TotalCurrentYear = x.TotalCurrentYear,
        //            AddedPreviousYear = x.AddedPreviousYear
        //        }).ToList()
        //    };

        //    return View(vm);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CA_Nursing_CollectionDevelopment(
        //CA_Nursing_CollectionDevelopmentViewModel model)
        //{
        //    string collegeCode = HttpContext.Session.GetString("CollegeCode");
        //    string facultyCode = HttpContext.Session.GetString("FacultyCode");

        //    if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
        //        return RedirectToAction("Login", "Account");

        //    // === SERVER-SIDE VALIDATION ===
        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    // === SAVE USING PROVEN PATTERN ===
        //    foreach (var row in model.ExistingList)
        //    {
        //        // FIXED: Filter by Id + CollegeCode + FacultyCode
        //        var entity = await _context.CaNursingCollectionDevelopments
        //            .FirstOrDefaultAsync(x => x.Id == row.Id &&
        //                                      x.CollegeCode == collegeCode &&
        //                                      x.FacultyCode == facultyCode);

        //        if (entity != null)
        //        {
        //            entity.TotalCurrentYear = row.TotalCurrentYear ?? 0;
        //            entity.AddedPreviousYear = row.AddedPreviousYear ?? 0;

        //            // Critical fix
        //            _context.Entry(entity).State = EntityState.Modified;
        //        }
        //    }

        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = "Collection development details saved successfully!";

        //    return RedirectToAction("CA_Nursing_LibraryEquipments");
        //}




        //--------------------------5th page------------------------

        [HttpGet]
        //public async Task<IActionResult> CA_Nursing_LibraryEquipments()
        //{
        //    string collegeCode = HttpContext.Session.GetString("CollegeCode");
        //    string facultyCode = HttpContext.Session.GetString("FacultyCode");

        //    int facultyCodeInt = Convert.ToInt32(facultyCode);

        //    var master = await _context.CaMstLibraryEquipmentsTypes
        //        .Where(x => x.FacultyCode == facultyCodeInt)
        //        .ToListAsync();

        //    var existing = await _context.CaNursingLibraryEquipments
        //        .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
        //        .ToListAsync();

        //    foreach (var m in master)
        //    {
        //        if (!existing.Any(e => e.EquipmentType == m.TypeOfEquipment))
        //        {
        //            _context.CaNursingLibraryEquipments.Add(new CaNursingLibraryEquipment
        //            {
        //                CollegeCode = collegeCode,
        //                FacultyCode = facultyCode,
        //                RegistrationNo = null,
        //                EquipmentType = m.TypeOfEquipment,
        //                SAvailable = "N"
        //            });
        //        }
        //    }

        //    await _context.SaveChangesAsync();

        //    existing = await _context.CaNursingLibraryEquipments
        //        .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
        //        .ToListAsync();

        //    var vm = new CA_Nursing_LibraryEquipmentsViewModel
        //    {
        //        ExistingList = existing
        //            .Select(x => new CA_Nursing_LibraryEquipmentsViewModel
        //            {
        //                Id = x.Id,
        //                EquipmentType = x.EquipmentType,
        //                IsAvailable = x.SAvailable
        //            }).ToList()
        //    };

        //    return View(vm);
        //}




        //[HttpPost]
        //public async Task<IActionResult> CA_Nursing_LibraryEquipments(CA_Nursing_LibraryEquipmentsViewModel model)
        //{
        //    if (model.ExistingList != null)
        //    {
        //        foreach (var item in model.ExistingList)
        //        {
        //            var exist = await _context.CaNursingLibraryEquipments.FindAsync(item.Id);

        //            if (exist != null)
        //            {
        //                exist.SAvailable = item.IsAvailable;
        //                _context.Update(exist);
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //    }

        //    return RedirectToAction("CA_LibraryServices");
        //}

        // Helper method to load existing list
        //private async Task<List<CA_Nursing_LibraryEquipmentsRowVM>> LoadExistingEquipments(string collegeCode, string facultyCode)
        //{
        //    var existing = await _context.CaNursingLibraryEquipments
        //        .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
        //        .ToListAsync();

        //    return existing.Select(x => new CA_Nursing_LibraryEquipmentsRowVM
        //    {
        //        Id = x.Id,
        //        EquipmentType = x.EquipmentType,
        //        IsAvailable = x.SAvailable
        //    }).ToList();
        //}


        //---------------------------6th page--------------------------

        [HttpGet]
        public async Task<IActionResult> CA_LibraryServices()
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");
            int facultyCodeInt = Convert.ToInt32(facultyCode);

            // Master list
            var masterList = await _context.CaMstLibraryServicesLists
                .Where(x => x.FacultyCode == facultyCodeInt)
                .ToListAsync();

            // Existing records
            var existing = await _context.CaLibraryServices
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();

            // Insert missing rows
            foreach (var m in masterList)
            {
                if (!existing.Any(e => e.ServiceName == m.ServiceName))
                {
                    _context.CaLibraryServices.Add(new CaLibraryService
                    {
                        CollegeCode = collegeCode,
                        FacultyCode = facultyCode,
                        ServiceName = m.ServiceName,
                        Specify = null
                    });
                }
            }

            await _context.SaveChangesAsync();

            // Reload
            existing = await _context.CaLibraryServices
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
                .ToListAsync();

            var vm = new CA_LibraryServicesViewModel
            {
                ExistingList = existing.Select(x => new CA_LibraryServicesViewModel
                {
                    Id = x.Id,
                    ServiceName = x.ServiceName,
                    Specify = x.Specify
                }).ToList()
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CA_LibraryServices(CA_LibraryServicesViewModel model)
        {
            string collegeCode = HttpContext.Session.GetString("CollegeCode");
            string facultyCode = HttpContext.Session.GetString("FacultyCode");

            // 🔴 MANUAL validation for list items
            if (model.ExistingList.Any(x => string.IsNullOrEmpty(x.Specify)))
            {
                ModelState.AddModelError("", "Please select Yes or No for all services.");
                return View(model);
            }

            foreach (var item in model.ExistingList)
            {
                var entity = await _context.CaLibraryServices.FindAsync(item.Id);
                if (entity != null)
                {
                    entity.Specify = item.Specify;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "CA_UserDetails",
                "Aff_AHS_ContinousApplication"
            );
        }

        //------------------------7th page------------------------

        //[HttpGet]
        //public async Task<IActionResult> CA_UserDetails()
        //{
        //    string collegeCode = HttpContext.Session.GetString("CollegeCode");
        //    string facultyCode = HttpContext.Session.GetString("FacultyCode");

        //    if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
        //        return RedirectToAction("Login", "Account");

        //    int facultyCodeInt = Convert.ToInt32(facultyCode);

        //    var masterList = await _context.CaMstUserDetails
        //        .Where(x => x.FacultyCode == facultyCodeInt.ToString())
        //        .ToListAsync();

        //    var existing = await _context.CaUserDetails
        //        .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
        //        .ToListAsync();

        //    foreach (var item in masterList)
        //    {
        //        if (!existing.Any(x => x.CategoryName == item.CategoryName))
        //        {
        //            _context.CaUserDetails.Add(new CaUserDetail
        //            {
        //                CollegeCode = collegeCode,
        //                FacultyCode = facultyCode,
        //                CategoryName = item.CategoryName,
        //                TotalNumber = 0
        //            });
        //        }
        //    }

        //    await _context.SaveChangesAsync();

        //    existing = await _context.CaUserDetails
        //        .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode)
        //        .ToListAsync();

        //    var vm = new CA_UserDetailsViewModel
        //    {
        //        ExistingList = existing.Select(x => new CA_UserDetailsViewModel
        //        {
        //            Id = x.Id,
        //            CategoryName = x.CategoryName,
        //            TotalNumber = x.TotalNumber
        //        }).ToList()
        //    };

        //    return View(vm);
        //}

        //[HttpPost]
        //public async Task<IActionResult> CA_UserDetails(CA_UserDetailsViewModel model)
        //{
        //    string collegeCode = HttpContext.Session.GetString("CollegeCode");
        //    string facultyCode = HttpContext.Session.GetString("FacultyCode");

        //    if (model.ExistingList != null)
        //    {
        //        foreach (var item in model.ExistingList)
        //        {
        //            var row = await _context.CaUserDetails.FindAsync(item.Id);
        //            if (row != null)
        //            {
        //                row.TotalNumber = item.TotalNumber;
        //                _context.CaUserDetails.Update(row);
        //            }
        //        }
        //        await _context.SaveChangesAsync();
        //    }

        //    TempData["Success"] = "User Details Saved Successfully!";
        //    return RedirectToAction("CA_VehicleDetails");
        //}

        //------------------------------8th page------------------------------

        [HttpGet]
        public async Task<IActionResult> CA_VehicleDetails()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var regNo = HttpContext.Session.GetString("RegistrationNo"); // may be null

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            // Create empty ViewModel first
            var vm = new CA_VehicleDetailsViewModel
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                RegistrationNo = regNo
            };

            // Load all required data (dropdown + existing records)
            await LoadViewData(vm, collegeCode, facultyCode, regNo);

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CA_VehicleDetails(CA_VehicleDetailsViewModel model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var regNo = HttpContext.Session.GetString("RegistrationNo");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            // Trim inputs
            model.VehicleRegNo = model.VehicleRegNo?.Trim();

            // Run validation
            if (!ModelState.IsValid)
            {
                // Repopulate dropdown and existing list
                await LoadViewData(model, collegeCode, facultyCode, regNo);
                return View(model);
            }

            // Save new vehicle
            var entity = new CaVehicleDetail
            {
                CollegeCode = collegeCode,
                FacultyCode = facultyCode,
                RegistrationNo = regNo,
                VehicleRegNo = model.VehicleRegNo,
                VehicleForCode = model.VehicleForCode,
                SeatingCapacity = model.SeatingCapacity,
                ValidityDate = model.ValidityDate.HasValue
                    ? DateOnly.FromDateTime(model.ValidityDate.Value)
                    : (DateOnly?)null,
                RcBookStatus = model.RcBookStatus ?? "N",
                InsuranceStatus = model.InsuranceStatus ?? "N",
                DrivingLicenseStatus = model.DrivingLicenseStatus ?? "N"
            };

            _context.CaVehicleDetails.Add(entity);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Vehicle added successfully!";
            return RedirectToAction("CA_VehicleDetails");
        }

        // Helper to reload dropdown + existing list (used on error)
        private async Task LoadViewData(CA_VehicleDetailsViewModel model, string collegeCode, string facultyCode, string? regNo)
        {
            int facultyCodeInt = Convert.ToInt32(facultyCode);

            model.VehicleForList = await _context.CaMstVdVehicleFors
                .Where(v => v.FacultyCode == facultyCodeInt)
                .Select(v => new SelectListItem
                {
                    Value = v.VehicleForCode,
                    Text = v.VehicleForName
                })
                .ToListAsync();

            var query = _context.CaVehicleDetails
                .Where(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode);

            if (!string.IsNullOrEmpty(regNo))
                query = query.Where(x => x.RegistrationNo == regNo);

            var existing = await query.ToListAsync();

            model.ExistingList = existing.Select(x => new CA_VehicleDetailsViewModel
            {
                Id = x.Id,
                VehicleRegNo = x.VehicleRegNo,
                VehicleForCode = x.VehicleForCode,
                SeatingCapacity = x.SeatingCapacity,
                ValidityDate = x.ValidityDate.HasValue
                    ? x.ValidityDate.Value.ToDateTime(TimeOnly.MinValue)
                    : (DateTime?)null,
                RcBookStatus = x.RcBookStatus,
                InsuranceStatus = x.InsuranceStatus,
                DrivingLicenseStatus = x.DrivingLicenseStatus
            }).ToList();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CA_UpdateVehicleDetails([FromBody] CA_VehicleDetailsViewModel model)
        {
            if (model == null || model.Id == null || model.Id <= 0)
                return Json(new { success = false, message = "Invalid request. Vehicle ID is missing." });

            // === Server-Side Validation ===
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(model.VehicleRegNo))
                errors.Add("Vehicle Registration Number is required.");

            if (string.IsNullOrWhiteSpace(model.VehicleForCode))
                errors.Add("Please select Vehicle For.");

            if (!model.SeatingCapacity.HasValue || model.SeatingCapacity < 1 || model.SeatingCapacity > 100)
                errors.Add("Seating capacity must be between 1 and 100.");

            if (!model.ValidityDate.HasValue)
                errors.Add("FC Validity Date is required.");

            if (string.IsNullOrWhiteSpace(model.RcBookStatus))
                errors.Add("Please select RC Book status.");

            if (string.IsNullOrWhiteSpace(model.InsuranceStatus))
                errors.Add("Please select Insurance status.");

            if (string.IsNullOrWhiteSpace(model.DrivingLicenseStatus))
                errors.Add("Please select Driving Licence status.");

            if (errors.Any())
            {
                return Json(new { success = false, message = string.Join("<br>", errors) });
            }

            var existing = await _context.CaVehicleDetails.FindAsync(model.Id.Value);
            if (existing == null)
                return Json(new { success = false, message = "Vehicle record not found." });

            // Update fields
            existing.VehicleRegNo = model.VehicleRegNo?.Trim();
            existing.VehicleForCode = model.VehicleForCode;
            existing.SeatingCapacity = model.SeatingCapacity;
            existing.ValidityDate = model.ValidityDate.HasValue
                ? DateOnly.FromDateTime(model.ValidityDate.Value)
                : (DateOnly?)null;
            existing.RcBookStatus = model.RcBookStatus;
            existing.InsuranceStatus = model.InsuranceStatus;
            existing.DrivingLicenseStatus = model.DrivingLicenseStatus;

            // Reliable update
            _context.Entry(existing).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Vehicle details updated successfully!" });
        }

        public async Task<IActionResult> CA_DeleteVehicleDetails(int id)
        {
            var existing = await _context.CaVehicleDetails.FindAsync(id);
            if (existing != null)
            {
                _context.CaVehicleDetails.Remove(existing);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("CA_VehicleDetails");
        }


        private async Task<byte[]> ToBytesAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }


        private byte[] ConvertToBytes(IFormFile file)
        {
            using var ms = new MemoryStream();
            file.CopyTo(ms);
            return ms.ToArray();
        }

        async Task<byte[]> ToBytes(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return ms.ToArray();
        }
    }


}