using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Medical_Affiliation.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers
{
    public class PhysicalInfrastructureController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public PhysicalInfrastructureController(ApplicationDbContext context, IUserContext userContext)
        {
            this._context = context;
            this._userContext = userContext;
        }

        public async Task<IActionResult> ChairDistribution()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            // =========================================================
            // GET ALL ACADEMIC INTAKES
            // =========================================================

            var academicIntakes = await _context.AcademicIntakes
                .Where(x =>
                    x.FacultyCode == facultyCode.ToString()
                    && x.CollegeCode == collegeCode)
                .ToListAsync();

            // =========================================================
            // CHECK COURSE SOURCE
            // =========================================================

            var savedDentalChairs = await _context.DentalChairs
                                    .Where(x =>
                                        x.CollegeCode == collegeCode &&
                                        x.FacultyCode == facultyCode)
                                    .ToListAsync();

            var collegeCourseExists = await _context.CollegeCourseIntakeDetails
                .AnyAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

            var hospital = await _context.HospitalDetailsForAffiliations.AsNoTracking().Where(e => e.CollegeCode == collegeCode).FirstOrDefaultAsync();

            List<DentalChairVm> model = new();

            // =========================================================
            // SOURCE 1 → CollegeCourseIntakeDetails
            // =========================================================

            if (collegeCourseExists)
            {
                var courses = await (
                    from mc in _context.MstCourses

                    join cc in _context.CollegeCourseIntakeDetails
                        on mc.CourseCode.ToString() equals cc.CourseCode

                    where mc.FacultyCode == facultyCode
                          && cc.CollegeCode == collegeCode
                          && cc.FacultyCode == facultyCode

                    select mc
                ).Distinct().ToListAsync();

                foreach (var course in courses)
                {
                    // =================================================
                    // FIND COURSE INTAKE
                    // =================================================

                    var intake = academicIntakes.FirstOrDefault(x =>
                        !string.IsNullOrEmpty(x.Courses) &&
                        x.Courses.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .Contains(course.CourseCode.ToString()));

                    int seatCount = intake?.Ay2025TotalIntake ?? 0;
                    if (seatCount <= 0)
                    {
                        continue;
                    }

                    // =================================================
                    // CALCULATE SEAT SLAB
                    // =================================================

                    int seatSlab = seatCount > 0
                        ? ((seatCount - 1) / 50 + 1) * 50
                        : 0;

                    // =================================================
                    // GET SLAB MASTER
                    // =================================================

                    var seatSlabData = await _context.SeatSlabMasters
                        .FirstOrDefaultAsync(x =>
                            x.FacultyCode == facultyCode &&
                            x.SeatSlab == seatSlab);

                    // =================================================
                    // ADD TO MODEL
                    // =================================================
                    var existingChairData = savedDentalChairs.FirstOrDefault(x => x.CourseCode == course.CourseCode);


                    model.Add(new DentalChairVm
                    {
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        CourseLevel = course.CourseLevel,

                        SeatSlab = seatSlab,
                        HospitalDetailsId = hospital.HospitalDetailsId,

                        SeatSlabId = seatSlabData != null
                            ? seatSlabData.SeatSlabId
                            : string.Empty,

                        ChairsRequired = seatSlab,

                        ChairsExisting = existingChairData?.ChairsExisting ?? 0
                    });
                }
            }

            // =========================================================
            // FALLBACK → AcademicIntake.Courses
            // =========================================================

            else
            {
                var courseCodes = academicIntakes
                    .Where(x => !string.IsNullOrEmpty(x.Courses))
                    .SelectMany(x => x.Courses!
                        .Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

                var courses = await _context.MstCourses
                    .Where(x =>
                        x.FacultyCode == facultyCode &&
                        courseCodes.Contains(x.CourseCode.ToString()))
                    .ToListAsync();

                foreach (var course in courses)
                {
                    // =================================================
                    // FIND COURSE INTAKE
                    // =================================================

                    var intake = academicIntakes.FirstOrDefault(x =>
                        !string.IsNullOrEmpty(x.Courses) &&
                        x.Courses.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(c => c.Trim())
                            .Contains(course.CourseCode.ToString()));

                    int seatCount = intake?.Ay2025TotalIntake ?? 0;

                    // =================================================
                    // CALCULATE SEAT SLAB
                    // =================================================

                    if (seatCount <= 0)
                    {
                        continue;
                    }
                    int seatSlab = seatCount > 0
                        ? ((seatCount - 1) / 50 + 1) * 50
                        : 0;

                    // =================================================
                    // GET SLAB MASTER
                    // =================================================

                    var seatSlabData = await _context.SeatSlabMasters
                        .FirstOrDefaultAsync(x =>
                            x.FacultyCode == facultyCode &&
                            x.SeatSlab == seatSlab);

                    // =================================================
                    // ADD TO MODEL
                    // =================================================
                    var existingChairData = savedDentalChairs.FirstOrDefault(x => x.CourseCode == course.CourseCode);


                    model.Add(new DentalChairVm
                    {
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        CourseLevel = course.CourseLevel,

                        SeatSlab = seatSlab,

                        SeatSlabId = seatSlabData != null
                            ? seatSlabData.SeatSlabId
                            : string.Empty,

                        ChairsRequired = seatSlab,

                        ChairsExisting = existingChairData?.ChairsExisting ?? 0
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChairDistribution(List<DentalChairVm> model)
        {
            try
            {
                // ============================================
                // NULL / EMPTY CHECK
                // ============================================

                if (model == null || !model.Any())
                {
                    TempData["Error"] = "No chair data submitted.";
                    return RedirectToAction(nameof(ChairDistribution));
                }

                // ============================================
                // MODEL VALIDATION
                // ============================================

                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Invalid form submission.";
                    return View(model);
                }

                var collegeCode = _userContext.CollegeCode;
                var facultyCode = _userContext.FacultyId;

                foreach (var item in model)
                {
                    // ============================================
                    // COURSE VALIDATION
                    // ============================================

                    if (item.CourseCode <= 0)
                    {
                        TempData["Error"] = "Invalid course detected.";
                        return View(model);
                    }

                    // ============================================
                    // NEGATIVE VALUE CHECK
                    // ============================================

                    if (item.ChairsExisting < 0)
                    {
                        TempData["Error"] = $"Chair count cannot be negative for {item.CourseName}.";
                        return View(model);
                    }

                    // ============================================
                    // CHECK EXISTING RECORD
                    // ============================================

                    var existingChair = await _context.DentalChairs
                        .FirstOrDefaultAsync(x =>
                            x.CollegeCode == collegeCode &&
                            x.FacultyCode == facultyCode &&
                            x.CourseCode == item.CourseCode);

                    // ============================================
                    // UPDATE
                    // ============================================

                    if (existingChair != null)
                    {
                        existingChair.ChairsExisting = item.ChairsExisting;
                        existingChair.ChairsRequired = item.ChairsRequired;
                        existingChair.SeatSlab = item.SeatSlab;
                        existingChair.SeatSlabId = item.SeatSlabId;
                    }

                    // ============================================
                    // INSERT
                    // ============================================

                    else
                    {
                        _context.DentalChairs.Add(new DentalChair
                        {
                            FacultyCode = facultyCode,
                            CollegeCode = collegeCode,

                            CourseCode = item.CourseCode,
                            CourseName = item.CourseName,
                            CourseLevel = item.CourseLevel,

                            SeatSlab = item.SeatSlab,
                            SeatSlabId = item.SeatSlabId,

                            ChairsRequired = item.ChairsRequired,
                            ChairsExisting = item.ChairsExisting
                        });
                    }
                }

                // ============================================
                // SAVE
                // ============================================

                var rows = await _context.SaveChangesAsync();

                if (rows > 0)
                {
                    TempData["Success"] = "Chair details saved successfully.";
                }
                else
                {
                    TempData["Error"] = "No changes were made.";
                }

                return RedirectToAction(nameof(ChairDistribution));
            }

            // ============================================
            // DATABASE ERROR
            // ============================================

            catch (DbUpdateException)
            {
                TempData["Error"] = "Database error occurred while saving chair details.";
                return RedirectToAction(nameof(ChairDistribution));
            }

            // ============================================
            // GENERAL ERROR
            // ============================================

            catch (Exception)
            {
                TempData["Error"] = "Unexpected error occurred.";
                return RedirectToAction(nameof(ChairDistribution));
            }
        }

        [HttpGet]
        public async Task<IActionResult> DentalCollegeLandBuildingDetail()
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCodeString = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCodeString))
            {
                return RedirectToAction("Login", "Account");
            }

            int facultyCode = Convert.ToInt32(facultyCodeString);

            // Academic Intake
            var academicIntake = await _context.AcademicIntakes
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode.ToString());

            if (academicIntake == null)
            {
                TempData["Error"] = "Academic intake details not found.";
                return RedirectToAction("IncreaseIntake","ContinuousAffiliationIncreaseIntake");
            }

            // Seat Intake
            int seatIntake = academicIntake.Ay2026TotalIntake;

            // Seat Slab
            int seatSlab = GetSeatSlab(seatIntake);

            // Fetch Master Norms
            var slabNorm = await _context.UgSeatSlabNormMasters
                .FirstOrDefaultAsync(x =>
                    x.FacultyCode == facultyCode &&
                    x.SeatSlab == seatSlab);

            if (slabNorm == null)
            {
                TempData["Error"] = "Seat slab norms not configured.";
                return RedirectToAction("Index");
            }

            // ======================================================
            // PRE CLINICAL & SKILL LABS
            // ======================================================

            // Master Data
            var labMasters = await _context
                .MstDentalPreClinicalAndSkillsLaboratoryAreaReqs
                .Where(x =>
                    x.FacultyCode == facultyCode &&
                    x.SeatIntake == seatSlab)
                .ToListAsync();

            // Existing Saved Data
            var savedLabs = await _context
                .DentalPreClinicalAndSkillsLabAreaReqs
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.SeatIntake == seatIntake &&
                    x.IsActive)
                .ToListAsync();

            // Existing Submitted Data
            var existingData = await _context.DentalCollegeLandBuildingDetails
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);


            // ======================================================
            // DENTAL INFRASTRUCTURE
            // ======================================================

            // Master Infrastructure Requirements
            var infraMasters = await _context.MstDentalInfrastructures
                .Where(x =>
                    x.FacultyCode == facultyCode &&
                    x.SeatSlab == seatSlab)
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            // Existing Saved Infrastructure
            var savedInfrastructure = await _context.DentalInfrastructures
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode &&
                    x.SeatSlab == seatSlab)
                .ToListAsync();

            // Build ViewModel
            var model = new DentalCollegeLandBuildingViewModel
            {
                CollegeCode = collegeCode,

                FacultyCode = facultyCode,

                SeatIntake = seatIntake,

                SeatSlab = seatSlab,
                InfrastructureDetails = infraMasters.Select(m =>
                {
                    var saved = savedInfrastructure
                        .FirstOrDefault(x => x.RequirementId == m.Id);

                    return new DentalInfrastructureVM
                    {
                        Id = saved?.Id ?? 0,

                        RequirementId = m.Id,

                        SlNo = m.SlNo,

                        RequirementName = m.RequirementName,

                        RequirementDescription = m.RequirementDescription,

                        SeatSlab = m.SeatSlab,

                        RequiredAreaSqFt = m.RequiredAreaSqFt,

                        AvailableAreaSqFt = saved?.AvailableAreaSqFt ?? 0
                    };
                }).ToList(),


            };


            // Populate Existing Data
            if (existingData != null)
            {
                model.Id = existingData.Id;

                model.LandCategory =
                    existingData.LandCategory;

                model.TotalLandAreaAcres =
                    existingData.TotalLandAreaAcres;

                model.LandOwnershipType =
                    existingData.LandOwnershipType;

                model.HasFutureExpansionSpace =
                    existingData.HasFutureExpansionSpace;

                model.TotalBuiltupAreaSqm =
                    existingData.TotalBuiltupAreaSqm;

                model.LectureHallCount =
                    existingData.LectureHallCount;

                model.LectureHallAreaSqm =
                    existingData.LectureHallAreaSqm;

                model.LectureHallSeatingCapacity =
                    existingData.LectureHallSeatingCapacity;

                model.ExaminationHallAreaSqm =
                    existingData.ExaminationHallAreaSqm;

                model.LibraryAreaSqm =
                    existingData.LibraryAreaSqm;

                model.HospitalAreaSqm =
                    existingData.HospitalAreaSqm;

                model.MuseumDemoRoomsAreaSqm =
                    existingData.MuseumDemoRoomsAreaSqm;

                model.DepartmentWiseAreaSqm =
                    existingData.DepartmentWiseAreaSqm;

                model.PreclinicalSkillLabAreaSqm =
                    existingData.PreclinicalSkillLabAreaSqm;

                model.Remarks =
                    existingData.Remarks;

                // ==========================
                // DOCUMENT PATHS
                // ==========================

                model.SaleDeedDocumentPath =
                    existingData.SaleDeedDocumentPath;

                model.EncumbranceCertificateDocumentPath =
                    existingData.EncumbranceCertificateDocumentPath;

                model.LandUseCertificateDocumentPath =
                    existingData.LandUseCertificateDocumentPath;

                model.ApprovedLayoutPlanDocumentPath =
                    existingData.ApprovedLayoutPlanDocumentPath;

                model.LandSketchDocumentPath =
                    existingData.LandSketchDocumentPath;

                model.DistanceCertificateDocumentPath =
                    existingData.DistanceCertificateDocumentPath;

                model.ApprovedBuildingPlanDocumentPath =
                    existingData.ApprovedBuildingPlanDocumentPath;

                model.CompletionCertificateDocumentPath =
                    existingData.CompletionCertificateDocumentPath;

                model.StructuralStabilityCertificateDocumentPath =
                    existingData.StructuralStabilityCertificateDocumentPath;

                model.FireSafetyNocDocumentPath =
                    existingData.FireSafetyNocDocumentPath;

                model.LiftLicenseDocumentPath =
                    existingData.LiftLicenseDocumentPath;

                model.ElectricalSafetyCertificateDocumentPath =
                    existingData.ElectricalSafetyCertificateDocumentPath;

                model.WaterSupplyCertificateDocumentPath =
                    existingData.WaterSupplyCertificateDocumentPath;

                model.SewageSanitationApprovalDocumentPath =
                    existingData.SewageSanitationApprovalDocumentPath;
            }

            PopulateNorms(model, slabNorm);
            return View(model);
        }

        private int GetSeatSlab(int totalIntake)
        {
            if (totalIntake <= 50)
                return 50;

            if (totalIntake <= 100)
                return 100;

            if (totalIntake <= 150)
                return 150;

            return 0;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DentalCollegeLandBuildingDetail( DentalCollegeLandBuildingViewModel model)
        {
            var collegeCode = HttpContext.Session.GetString("CollegeCode");
            var facultyCodeString = HttpContext.Session.GetString("FacultyCode");

            if (string.IsNullOrEmpty(collegeCode) || string.IsNullOrEmpty(facultyCodeString))
            {
                return RedirectToAction("Login", "Account");
            }

            int facultyCode = Convert.ToInt32(facultyCodeString);

            // ==============================
            // FETCH ACADEMIC INTAKE
            // ==============================

            var academicIntake = await _context.AcademicIntakes
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode.ToString());

            int seatIntake = academicIntake.Ay2026TotalIntake;

            int seatSlab = GetSeatSlab(seatIntake);
            var norm = await _context.UgSeatSlabNormMasters.FirstOrDefaultAsync(x => x.FacultyCode == facultyCode && x.SeatSlab == seatSlab);
            if (academicIntake == null)
            {
                TempData["Error"] = "Academic intake details not found.";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.LandCategory))
            {
                TempData["Error"] = "Please select land category.";
                PopulateNorms(model, norm);
                return View(model);
            }


            if (norm == null)
            {
                TempData["Error"] = "Seat slab norms not configured.";
                return View(model);
            }

            var entity = await _context.DentalCollegeLandBuildingDetails
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyCode == facultyCode);

            if (entity == null)
            {
                entity = new DentalCollegeLandBuildingDetail();

                entity.CollegeCode = collegeCode;
                entity.FacultyCode = facultyCode;

                entity.CreatedOn = DateTime.Now;

                _context.DentalCollegeLandBuildingDetails.Add(entity);
            }

            // ==============================
            // BASIC DETAILS
            // ==============================

            entity.SeatIntake = seatIntake;
            entity.SeatSlab = seatSlab;
            entity.TotalLandAreaAcres = model.TotalLandAreaAcres;
            entity.LandCategory = model.LandCategory;
            entity.LandOwnershipType = model.LandOwnershipType;
            entity.HasFutureExpansionSpace = model.HasFutureExpansionSpace;
            entity.TotalBuiltupAreaSqm = model.TotalBuiltupAreaSqm;
            entity.LectureHallAreaSqm = model.LectureHallAreaSqm;
            entity.LectureHallSeatingCapacity = model.LectureHallSeatingCapacity;
            entity.ExaminationHallAreaSqm = model.ExaminationHallAreaSqm;
            entity.LibraryAreaSqm = model.LibraryAreaSqm;
            entity.LectureHallCount = model.LectureHallCount;
            entity.MuseumDemoRoomsAreaSqm = model.MuseumDemoRoomsAreaSqm;
            entity.PreclinicalSkillLabAreaSqm = model.PreclinicalSkillLabAreaSqm;
            entity.DepartmentWiseAreaSqm = model.DepartmentWiseAreaSqm;
            entity.HospitalAreaSqm = model.HospitalAreaSqm;
            entity.Remarks = model.Remarks;
            entity.ModifiedOn = DateTime.Now;

            // ==============================
            // DOCUMENT UPLOAD BASE PATH
            // ==============================

            string baseFolder = Path.Combine( "D:\\DentalCollegeDetails\\LandAndBuildingDetails", collegeCode);

            if (!Directory.Exists(baseFolder))
            {
                Directory.CreateDirectory(baseFolder);
            }

            // ==============================
            // FILE UPLOADS
            // ==============================

            var saleDeedPath = await SaveFile( model.SaleDeedDocument, baseFolder, collegeCode, "SaleDeed");

            if (!string.IsNullOrEmpty(saleDeedPath))
            {
                entity.SaleDeedDocumentPath = saleDeedPath;
            }

            var encumbrancePath = await SaveFile( model.EncumbranceCertificateDocument, baseFolder, collegeCode, "EncumbranceCertificate");

            if (!string.IsNullOrEmpty(encumbrancePath))
            {
                entity.EncumbranceCertificateDocumentPath = encumbrancePath;
            }

            var landUseCertificateDocumentPath = await SaveFile(model.LandUseCertificateDocument, baseFolder, collegeCode, "LandUseCertificate");

            if (!string.IsNullOrEmpty(landUseCertificateDocumentPath))
            {
                entity.LandUseCertificateDocumentPath = landUseCertificateDocumentPath;
            }

            var approvedLayoutPlanDocumentPath = await SaveFile(model.ApprovedLayoutPlanDocument, baseFolder, collegeCode, "ApprovedLayoutPlanCertificate");

            if (!string.IsNullOrEmpty(approvedLayoutPlanDocumentPath))
            {
                entity.ApprovedLayoutPlanDocumentPath = approvedLayoutPlanDocumentPath;
            }


            var completionCertificateDocumentPath = await SaveFile(model.CompletionCertificateDocument, baseFolder, collegeCode, "CompletionCertificate");

            if (!string.IsNullOrEmpty(completionCertificateDocumentPath))
            {
                entity.CompletionCertificateDocumentPath = completionCertificateDocumentPath;
            }

            var structuralStabilityCertificateDocumentPath = await SaveFile(model.StructuralStabilityCertificateDocument, baseFolder, collegeCode, "StructuralStabilityCertificate");

            if (!string.IsNullOrEmpty(structuralStabilityCertificateDocumentPath))
            {
                entity.StructuralStabilityCertificateDocumentPath = structuralStabilityCertificateDocumentPath;
            }

            var fireSafetyNocDocumentPath = await SaveFile(model.FireSafetyNocDocument, baseFolder, collegeCode, "FireSafetyNocCertificate");

            if (!string.IsNullOrEmpty(fireSafetyNocDocumentPath))
            {
                entity.FireSafetyNocDocumentPath = fireSafetyNocDocumentPath;
            }

            var landSketchDocumentPath = await SaveFile( model.LandSketchDocument, baseFolder, collegeCode, "LandSketch");

            if (!string.IsNullOrEmpty(landSketchDocumentPath))
            {
                entity.LandSketchDocumentPath = landSketchDocumentPath;
            }

            var distanceCertificateDocumentPath = await SaveFile( model.DistanceCertificateDocument, baseFolder, collegeCode, "DistanceCertificate");

            if (!string.IsNullOrEmpty(distanceCertificateDocumentPath))
            {
                entity.DistanceCertificateDocumentPath = distanceCertificateDocumentPath;
            }

            var approvedBuildingPlanDocumentPath = await SaveFile( model.ApprovedBuildingPlanDocument, baseFolder, collegeCode, "ApprovedBuildingPlan");

            if (!string.IsNullOrEmpty(approvedBuildingPlanDocumentPath))
            {
                entity.ApprovedBuildingPlanDocumentPath = approvedBuildingPlanDocumentPath;
            }

            var liftLicenseDocumentPath = await SaveFile( model.LiftLicenseDocument, baseFolder,collegeCode, "LiftLicense");

            if (!string.IsNullOrEmpty(liftLicenseDocumentPath))
            {
                entity.LiftLicenseDocumentPath = liftLicenseDocumentPath;
            }

            var electricalSafetyCertificateDocumentPath = await SaveFile( model.ElectricalSafetyCertificateDocument, baseFolder, collegeCode, "ElectricalSafetyCertificate");

            if (!string.IsNullOrEmpty(electricalSafetyCertificateDocumentPath))
            {
                entity.ElectricalSafetyCertificateDocumentPath = electricalSafetyCertificateDocumentPath;
            }

            var waterSupplyCertificateDocumentPath = await SaveFile( model.WaterSupplyCertificateDocument, baseFolder, collegeCode, "WaterSupplyCertificate");

            if (!string.IsNullOrEmpty(waterSupplyCertificateDocumentPath))
            {
                entity.WaterSupplyCertificateDocumentPath = waterSupplyCertificateDocumentPath;
            }

            var sewageSanitationApprovalDocumentPath = await SaveFile( model.SewageSanitationApprovalDocument, baseFolder, collegeCode,"SewageSanitationApproval");

            if (!string.IsNullOrEmpty(sewageSanitationApprovalDocumentPath))
            {
                entity.SewageSanitationApprovalDocumentPath = sewageSanitationApprovalDocumentPath;
            }

            var hospital = await _context.HospitalDetailsForAffiliations.FirstOrDefaultAsync(x => x.CollegeCode == collegeCode && x.FacultyCode == facultyCode.ToString());


            // ==============================
            // SAVE DENTAL INFRASTRUCTURE
            // ==============================

            if (model.InfrastructureDetails != null &&
                model.InfrastructureDetails.Any())
            {
                var existingInfrastructure = await _context.DentalInfrastructures
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode == facultyCode)
                    .ToListAsync();

                foreach (var item in model.InfrastructureDetails)
                {
                    var existingInfra = existingInfrastructure
                        .FirstOrDefault(x =>
                            x.RequirementId == item.RequirementId &&
                            x.SeatSlab == item.SeatSlab);

                    if (existingInfra == null)
                    {
                        existingInfra = new DentalInfrastructure
                        {
                            FacultyCode = facultyCode,

                            AffiliationTypeId = hospital.AffiliationTypeId, // update dynamically if needed

                            CollegeCode = collegeCode,

                            HospitalDetailsId = hospital.HospitalDetailsId, // update dynamically if needed

                            RequirementId = item.RequirementId,

                            SeatSlab = item.SeatSlab,

                            CreatedOn = DateTime.Now
                        };

                        _context.DentalInfrastructures.Add(existingInfra);
                    }

                    existingInfra.RequiredAreaSqFt =
                        item.RequiredAreaSqFt;

                    existingInfra.AvailableAreaSqFt =
                        item.AvailableAreaSqFt;

                    existingInfra.ModifiedOn = DateTime.Now;
                }
            }

            // ==============================
            // SAVE
            // ==============================

            try
            {
                await _context.SaveChangesAsync();

                TempData["Success"] = "Land & Building details saved successfully.";

                return RedirectToAction("DentalCollegeLandBuildingDetail");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                PopulateNorms(model, norm);
                return View(model);
            }
        }

        private void PopulateNorms( DentalCollegeLandBuildingViewModel model, UgSeatSlabNormMaster norm)
        {
            model.RequiredBuiltupAreaSqm = norm.CollegeBuiltupAreaSqm;
            model.RequiredLectureHallAreaSqm = norm.LectureHallAreaSqm;
            model.RequiredLectureHallCapacity = norm.LectureHallCapacity;
            model.RequiredExamHallAreaSqm = norm.ExaminationHallAreaSqm;
            model.RequiredLibraryAreaSqm = norm.LibraryAreaSqm;
            model.RequiredHospitalAreaSqm = norm.DentalHospitalAreaSqm;
            model.RequiredLectureHallCount = norm.LectureHallCount;
            // ==========================
            // LAND REQUIREMENT
            // ==========================

            model.RequiredLandRequirementText = null;

            if (model.LandCategory == "Metro / Tier 1")
            {
                model.RequiredLandRequirementText = "As per approved constructed area norms";
            }
            else if (model.LandCategory == "Tier 2 / Hilly / North Eastern States")
            {
                model.RequiredLandAcres =  norm.LandTier2Acres;
            }
            else if (model.LandCategory ==  "Any Other Area")
            {
                model.RequiredLandAcres = norm.LandOtherAreaAcres;
            }
            else
            {
                // Default first-load behavior
                model.RequiredLandAcres = norm.LandOtherAreaAcres;
            }
        }

        private async Task<string?> SaveFile(IFormFile? file, string folderPath, string collegeCode, string documentType)
        {
            if (file == null || file.Length == 0)
                return null;


            const long maxFileSize = 2 * 1024 * 1024;

            if (file.Length > maxFileSize)
            {
                throw new Exception("File size must not exceed 2 MB.");
            }

            string extension = Path.GetExtension(file.FileName);
            var allowedExtensions = ".pdf";
            //var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };

            if (allowedExtensions != extension.ToLower())
            {
                return null;
            }


            string fileName = $"{collegeCode}_{documentType}{extension}";

            string fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fullPath;
        }


        public IActionResult ViewDocument(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return NotFound();
            }

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var contentType = "application/pdf";

            return PhysicalFile(filePath, contentType);
        }




        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpGet]
        public async Task<IActionResult> DentalSkillsLaboratory()
        {
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrEmpty(facultyCode))
                return RedirectToAction("Login", "Account");

            var lab = await _context.MedicalSkillsLaboratories
                                    .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode);

            if (lab == null)
            {
                var emptyVm = new SkillsLabViewModel();

                // =========================================
                // DENTAL LAB MASTER DATA
                // =========================================
                if (facultyCode == "2")
                {
                    int intake = 50;

                    var masterLabs = await _context
                        .MstDentalPreClinicalAndSkillsLaboratoryAreaReqs
                        .Where(x => x.FacultyCode == 2 &&
                                    x.SeatIntake == intake &&
                                    x.IsActive)
                        .OrderBy(x => x.SectionCode)
                        .ThenBy(x => x.LaboratoryName)
                        .ToListAsync();

                    emptyVm.PreClinicalAndSkillsLabs = masterLabs
                        .Select(x => new DentalPreClinicalAndSkillsLabAreaReqVM
                        {
                            LabId = x.Id,
                            FacultyCode = x.FacultyCode,
                            SeatIntake = x.SeatIntake,

                            LabName = x.LaboratoryName,

                            SectionCode = x.SectionCode,
                            LaboratorySection = x.LaboratorySection,

                            RequiredAreaSqFt = x.AreaRequiredSqFt
                        })
                        .ToList();
                }

                return View(emptyVm);
            }

            var vm = new SkillsLabViewModel
            {
                AnnualMbbsIntake = lab.AnnualMbbsIntake,

                TotalAreaAvailableSqm = lab.TotalAreaAvailableSqm,
                TotalAreaRequiredSqm = lab.TotalAreaRequiredSqm,
                TotalAreaDeficiencySqm = lab.TotalAreaDeficiencySqm,

                SixWeeksTrainingCompletedBeforeClinical = lab.SixWeeksTrainingCompletedBeforeClinical,

                NumberOfExaminationRooms = lab.NumberOfExaminationRooms,
                HasMinFourExamRooms = lab.HasMinFourExamRooms,
                HasDemoRoomSmallGroups = lab.HasDemoRoomSmallGroups,
                HasDebriefArea = lab.HasDebriefArea,
                HasFacultyCoordinatorRoom = lab.HasFacultyCoordinatorRoom,
                HasSupportStaffRoom = lab.HasSupportStaffRoom,
                HasStorageForMannequins = lab.HasStorageForMannequins,
                HasVideoRecordingFacility = lab.HasVideoRecordingFacility,

                NumberOfSkillStations = lab.NumberOfSkillStations,
                HasGroupAndIndividualStations = lab.HasGroupAndIndividualStations,
                HasRequiredTrainersAndMannequins = lab.HasRequiredTrainersAndMannequins,
                HasDedicatedTechnicalOfficer = lab.HasDedicatedTechnicalOfficer,
                HasAdequateSupportStaff = lab.HasAdequateSupportStaff,

                TeachingAreasHaveAV = lab.TeachingAreasHaveAv,
                TeachingAreasHaveInternet = lab.TeachingAreasHaveInternet,
                SkillsLabEnabledForELearning = lab.SkillsLabEnabledForElearning
            };

            // =========================================
            // DENTAL LAB SAVED DATA
            // =========================================
            if (facultyCode == "2")
            {
                var savedLabs = await (
                    from t in _context.DentalPreClinicalAndSkillsLabAreaReqs

                    join m in _context.MstDentalPreClinicalAndSkillsLaboratoryAreaReqs
                        on t.LabId equals m.Id

                    where t.CollegeCode == collegeCode
                          && t.FacultyCode == 2

                    orderby m.SectionCode, m.LaboratoryName

                    select new DentalPreClinicalAndSkillsLabAreaReqVM
                    {
                        Id = t.Id,

                        CollegeCode = t.CollegeCode,
                        FacultyCode = t.FacultyCode,

                        LabId = t.LabId,

                        SeatIntake = m.SeatIntake,

                        LabName = m.LaboratoryName,

                        SectionCode = m.SectionCode,
                        LaboratorySection = m.LaboratorySection,

                        RequiredAreaSqFt = m.AreaRequiredSqFt,

                        ExistingAreaSqFt = t.ExistingAreaSqFt
                    }
                ).ToListAsync();

                // =========================================
                // IF SAVED DATA EXISTS
                // =========================================
                if (savedLabs.Any())
                {
                    vm.PreClinicalAndSkillsLabs = savedLabs;
                }
                else
                {
                    // =========================================
                    // LOAD MASTER DATA
                    // =========================================

                    int intake = lab.AnnualMbbsIntake;

                    var masterLabs = await _context
                        .MstDentalPreClinicalAndSkillsLaboratoryAreaReqs
                        .Where(x => x.FacultyCode == 2 &&
                                    x.SeatIntake == intake &&
                                    x.IsActive)
                        .OrderBy(x => x.SectionCode)
                        .ThenBy(x => x.LaboratoryName)
                        .ToListAsync();

                    vm.PreClinicalAndSkillsLabs = masterLabs
                        .Select(x => new DentalPreClinicalAndSkillsLabAreaReqVM
                        {
                            LabId = x.Id,

                            FacultyCode = x.FacultyCode,

                            SeatIntake = x.SeatIntake,

                            LabName = x.LaboratoryName,

                            SectionCode = x.SectionCode,

                            LaboratorySection = x.LaboratorySection,

                            RequiredAreaSqFt = x.AreaRequiredSqFt
                        })
                        .ToList();
                }
            }

            return View(vm);
        }


        [Authorize(AuthenticationSchemes = "CollegeAuth", Policy = "CollegeOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DentalSkillsLaboratory(SkillsLabViewModel model)
        {
            var facultyCode = FacultyCode;
            var collegeCode = CollegeCode;

            if (string.IsNullOrEmpty(facultyCode) || string.IsNullOrWhiteSpace(collegeCode))
                return RedirectToAction("Login", "Account");

            //if (!ModelState.IsValid)
            //    return View(model);

            // ================================
            // SERVER-SIDE CALCULATION
            // ================================
            model.TotalAreaRequiredSqm = model.AnnualMbbsIntake * 1.2m;
            model.TotalAreaDeficiencySqm =
                Math.Max(0, model.TotalAreaRequiredSqm - model.TotalAreaAvailableSqm);

            var lab = await _context.MedicalSkillsLaboratories
                                    .FirstOrDefaultAsync(x => x.FacultyCode == facultyCode && x.CollegeCode == collegeCode);

            if (lab == null)
            {
                lab = new MedicalSkillsLaboratory
                {
                    FacultyCode = facultyCode,
                    CollegeCode = collegeCode,
                };
                _context.MedicalSkillsLaboratories.Add(lab);
            }

            // ================================
            // UPDATE FIELDS
            // ================================
            lab.AnnualMbbsIntake = model.AnnualMbbsIntake;
            lab.TotalAreaAvailableSqm = model.TotalAreaAvailableSqm;
            lab.TotalAreaRequiredSqm = model.TotalAreaRequiredSqm;
            lab.TotalAreaDeficiencySqm = model.TotalAreaDeficiencySqm;

            lab.SixWeeksTrainingCompletedBeforeClinical =
                model.SixWeeksTrainingCompletedBeforeClinical ?? false;

            lab.NumberOfExaminationRooms = model.NumberOfExaminationRooms;
            lab.HasMinFourExamRooms = model.HasMinFourExamRooms ?? false;
            lab.HasDemoRoomSmallGroups = model.HasDemoRoomSmallGroups ?? false;
            lab.HasDebriefArea = model.HasDebriefArea ?? false;
            lab.HasFacultyCoordinatorRoom = model.HasFacultyCoordinatorRoom ?? false;
            lab.HasSupportStaffRoom = model.HasSupportStaffRoom ?? false;
            lab.HasStorageForMannequins = model.HasStorageForMannequins ?? false;
            lab.HasVideoRecordingFacility = model.HasVideoRecordingFacility ?? false;

            lab.NumberOfSkillStations = model.NumberOfSkillStations;
            lab.HasGroupAndIndividualStations = model.HasGroupAndIndividualStations ?? false;
            lab.HasRequiredTrainersAndMannequins = model.HasRequiredTrainersAndMannequins ?? false;
            lab.HasDedicatedTechnicalOfficer = model.HasDedicatedTechnicalOfficer ?? false;
            lab.HasAdequateSupportStaff = model.HasAdequateSupportStaff ?? false;

            lab.TeachingAreasHaveAv = model.TeachingAreasHaveAV ?? false;
            lab.TeachingAreasHaveInternet = model.TeachingAreasHaveInternet ?? false;
            lab.SkillsLabEnabledForElearning = model.SkillsLabEnabledForELearning ?? false;

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.SaveChangesAsync();
                // ============================================
                // DENTAL LAB AREA TABLE
                // ============================================

                if (facultyCode == "2" &&
                    model.PreClinicalAndSkillsLabs != null &&
                    model.PreClinicalAndSkillsLabs.Any())
                {
                    var existingLabs = await _context
                        .DentalPreClinicalAndSkillsLabAreaReqs
                        .Where(x =>
                            x.CollegeCode == collegeCode &&
                            x.FacultyCode == 2)
                        .ToListAsync();

                    foreach (var item in model.PreClinicalAndSkillsLabs)
                    {
                        var existing = existingLabs
                            .FirstOrDefault(x =>
                                x.LabId == item.LabId);

                        if (existing != null)
                        {
                            // UPDATE
                            existing.ExistingAreaSqFt =
                                item.ExistingAreaSqFt;

                            existing.UpdatedOn = DateTime.Now;
                        }
                        else
                        {
                            // INSERT
                            _context.DentalPreClinicalAndSkillsLabAreaReqs
                                .Add(new DentalPreClinicalAndSkillsLabAreaReq
                                {
                                    CollegeCode = collegeCode,

                                    FacultyCode = 2,

                                    SeatIntake = item.SeatIntake,

                                    LabId = item.LabId,

                                    LabName = item.LabName,

                                    RequiredAreaSqFt =
                                        item.RequiredAreaSqFt,

                                    ExistingAreaSqFt =
                                        item.ExistingAreaSqFt,

                                    IsActive = true,

                                    CreatedOn = DateTime.Now
                                });
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                TempData["Success"] = "Saved successfully!";
                return RedirectToAction(nameof(DentalSkillsLaboratory));
            }
            catch
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", "Error while saving data");
                return View(model);
            }
        }


    }
}
