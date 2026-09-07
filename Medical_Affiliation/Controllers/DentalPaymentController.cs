using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Medical_Affiliation.Controllers
{
    public class DentalPaymentController : BaseController
    {
        private const string PaymentAcademicYear = "2025-26";
        //private readonly ApplicationDbContext _context;
        private readonly string _connectionString;
        private const int _facultyCode = 2;

        private int AffTypeId
        {
            get
            {
                var value = HttpContext?.Session?.GetString("TypeOfAffiliationId")
                            ?? HttpContext?.Session?.GetString("AffiliationTypeId");

                return int.TryParse(value, out var id) ? id : 0;
            }
        }


        public DentalPaymentController(ApplicationDbContext context) : base(context)
        {
            _connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("The application's database connection string is not configured.");
        }

        // GET: /PaymentCalculation
        [HttpGet]
        public async Task<IActionResult> Index()
        {

            if (AffTypeId <= 0)
            {
                return BadRequest("Invalid Affiliation Type.");
            }

            var collegeCode = HttpContext.Session.GetString("CollegeCode");

            var affiliationType = await _context.MstDentalAffiliationTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.DentalAffiliationTypeId == AffTypeId &&
                    e.FacultyCode == _facultyCode
                );

            if (affiliationType == null) return NotFound("Affiliation Type Not found.");

            //var requiredCourseLevel =  NormalizeCourseLevel(affiliationType.CourseLevelGroup);
            var requiredCourseLevel = HttpContext.Session.GetString("CourseLevel");

            if (string.IsNullOrWhiteSpace(requiredCourseLevel))
            {
                return BadRequest("Course level is not configured for this affiliation type.");
            }

            // ===========================================
            // FETCH ACADEMIC INTAKE FOR DENTAL FACULTY 
            // ===========================================

            var academicIntake = await _context.AcademicIntakes
                .AsNoTracking()
                .Where(e =>
                    e.CollegeCode == collegeCode &&
                    e.FacultyCode == _facultyCode.ToString()
                ).ToListAsync();

            // ===========================================
            // GET ALL DENTAL COURSES
            // ===========================================

            var dentalCourses = await _context.MstCourses
                .AsNoTracking()
                .Where(e =>
                    e.FacultyCode == _facultyCode
                )
                .ToListAsync();


            // ==========================================
            // MAP COLLEGE COURSES WITH AY 2026 INTAKE 
            // ==========================================

            var collegeCourses = new List<DentalCollegeCourseViewModel>();

            foreach(var intake in academicIntake)
            {
                if (string.IsNullOrWhiteSpace(intake.Courses))
                    continue;

                if (!int.TryParse(intake.Courses.Trim(), out int courseCode))
                    continue;

                var course = dentalCourses.FirstOrDefault(e =>
                        e.CourseCode == courseCode &&
                        string.Equals(
                            NormalizeCourseLevel(e.CourseLevel),
                            NormalizeCourseLevel(requiredCourseLevel),
                            StringComparison.OrdinalIgnoreCase
                        )
                    );

                if (course == null ) continue;

                if (!collegeCourses.Any(e => e.CourseCode == course.CourseCode))
                {
                    collegeCourses.Add(new DentalCollegeCourseViewModel
                    {
                        CourseCode = course.CourseCode,
                        CourseName = course.CourseName,
                        CourseLevel = course.CourseLevel,
                        Ay2026TotalIntake = intake.Ay2026TotalIntake
                    });
                }
            }

            // =========================================================
            // Get Dental Fee Types
            // =========================================================

            var feeTypes = await _context.MstDentalFeeTypes
                .AsNoTracking()
                .Where(e =>
                    e.FacultyCode == _facultyCode &&
                    e.AffiliationTypeId == AffTypeId &&
                    e.IsActive
                )
                .OrderBy(e=> e.DisplayOrder)
                .ToListAsync();

            // =========================================================
            // Get Dental Master Fee Structure
            // =========================================================

            var feeStructures = await _context.MstDentalFeeStructures
                .AsNoTracking()
                .Where(e =>
                    e.FacultyCode == _facultyCode &&
                    e.AffiliationTypeId == AffTypeId &&
                    e.IsActive
                )
                .ToListAsync();

            // ==========================================
            // Get Saved Fee Structure
            // ==========================================

            var savedFeeStructure = await _context.TxnDentalFeeStructures
                .AsNoTracking()
                .Where(e =>
                    e.CollegeCode == collegeCode &&
                    e.FacultyCode == _facultyCode &&
                    e.AffiliationTypeId == AffTypeId &&
                    e.IsActive
                ).ToListAsync();

            // =====================================================
            // GET EXISTING PAYMENT / TRANSACTION
            // =====================================================

            var savedPayment = await _context.TxnDentalPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.CollegeCode == collegeCode && 
                    e.FacultyCode == _facultyCode && 
                    e.AffiliationTypeId == AffTypeId &&
                    e.CourseLevel == requiredCourseLevel &&
                    e.IsActive
                );

            var model = new DentalFeeStructureViewModel
            {
                CollegeCode = collegeCode,
                FacultyCode = _facultyCode,
                AffiliationTypeId = AffTypeId,
                AffiliationCategory = affiliationType.AffiliationCategory,

                // =================================================
                // EXISTING PAYMENT DETAILS
                // =================================================

                PaymentId = savedPayment?.Id,

                ApplicableCourses = collegeCourses,

                TransactionId = savedPayment?.TransactionId,
                TransactionReceiptPath = savedPayment?.TransactionReceiptPath,
                AmountPaid = savedPayment?.AmountPaid ?? 0m,

                FeeTypes = feeTypes.Select(feeType =>
                {
                    var courseFees = new List<DentalCourseFeeViewModel>();

                    var applicableMasterFees = feeStructures
                        .Where(e => e.FeeTypeId == feeType.Id)
                        .ToList();

                    foreach (var masterFee in applicableMasterFees)
                    {
                        // =====================================================
                        // IMPORTANT:
                        // DENTAL FEES SHOULD PRIMARILY DEPEND ON COURSE LEVEL.
                        //
                        // Example:
                        // UG -> BDS
                        // PG -> All MDS Courses
                        //
                        // Therefore, do not depend on CourseCode for
                        // CourseLevel based fee structures.
                        // =====================================================

                        var calculationType = masterFee.CalculationType?.Trim();

                        // =====================================================
                        // COURSE LEVEL BASED FEE
                        //
                        // UG -> BDS
                        // PG -> All MDS Courses
                        //
                        // IMPORTANT:
                        // One master fee should create ONLY ONE fee row.
                        //
                        // Example:
                        //
                        // Application Fee
                        // PG -> ₹3,000 Fixed
                        //
                        // Annual Fee
                        // PG -> ₹4,500 Per Seat
                        // Total Intake = Sum of all PG course intakes
                        //
                        // Per Course
                        // PG -> ₹10,000
                        // Total = Number of PG courses × ₹10,000
                        // =====================================================

                        if (!string.IsNullOrWhiteSpace(masterFee.CourseLevel))
                        {
                            var masterCourseLevel =
                                NormalizeCourseLevel(masterFee.CourseLevel);

                            var matchedCourses = collegeCourses
                                .Where(e =>
                                    string.Equals(
                                        NormalizeCourseLevel(e.CourseLevel),
                                        masterCourseLevel,
                                        StringComparison.OrdinalIgnoreCase
                                    )
                                )
                                .ToList();

                            if (!matchedCourses.Any())
                            {
                                continue;
                            }

                            // =====================================================
                            // GET SAVED TRANSACTION FEE
                            //
                            // Since this is a COURSE LEVEL fee,
                            // do not depend on individual CourseCode.
                            // =====================================================

                            var savedFees = savedFeeStructure
                                .Where(e =>
                                    e.FeeTypeId == feeType.Id &&
                                    e.DentalFeeStructureId == masterFee.Id
                                )
                                .ToList();

                            var savedFee = savedFees.FirstOrDefault();

                            // =====================================================
                            // AMOUNT & CALCULATION TYPE
                            // =====================================================

                            var amountToBePaid = savedFee?.AmountToBePaid ?? masterFee.AmountToBePaid;

                            var actualCalculationType = savedFee?.CalculationType ?? masterFee.CalculationType;

                            // =====================================================
                            // CALCULATE TOTAL PG / UG INTAKE
                            //
                            // Example PG:
                            //
                            // MDS Prosthodontics -> 50
                            // MDS Periodontics   -> 3
                            // MDS Paedodontics   -> 3
                            //
                            // Total PG Intake = 56
                            // =====================================================

                            var totalAcademicIntake = matchedCourses.Sum(e => e.Ay2026TotalIntake  );

                            // =====================================================
                            // TOTAL NUMBER OF COURSES
                            //
                            // Example:
                            //
                            // PG has 4 MDS courses
                            // =====================================================

                            var totalCourseCount = matchedCourses.Count;

                            // =====================================================
                            // CALCULATE FINAL AMOUNT
                            // =====================================================

                            decimal calculatedAmount;

                            if (string.Equals(
                                actualCalculationType,
                                "Per Seat",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                calculatedAmount =
                                    savedFees.Any()
                                        ? savedFees.Sum(e => e.CalculatedAmount)
                                        : amountToBePaid * totalAcademicIntake;
                            }
                            else if (string.Equals(
                                actualCalculationType,
                                "Per Course",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                calculatedAmount =
                                    savedFees.Any()
                                        ? savedFees.Sum(e => e.CalculatedAmount)
                                        : amountToBePaid * totalCourseCount;
                            }
                            else
                            {
                                // Fixed fee should only be counted once
                                calculatedAmount =
                                    savedFees.Any()
                                        ? savedFees.Sum(e => e.CalculatedAmount)
                                        : amountToBePaid;
                            }

                            // =====================================================
                            // MULTIPLIER FOR UI
                            //
                            // Per Seat   -> Total Intake
                            // Per Course -> Number of Courses
                            // Fixed      -> 1
                            // =====================================================

                            var multiplier =
                                string.Equals(
                                    actualCalculationType,
                                    "Per Seat",
                                    StringComparison.OrdinalIgnoreCase
                                )
                                ? totalAcademicIntake

                                : string.Equals(
                                    actualCalculationType,
                                    "Per Course",
                                    StringComparison.OrdinalIgnoreCase
                                )
                                ? totalCourseCount

                                : 1;

                            // =====================================================
                            // ADD ONLY ONE COMBINED ROW
                            // =====================================================

                            courseFees.Add(new DentalCourseFeeViewModel
                            {
                                Id = savedFee?.Id,

                                DentalFeeStructureId = masterFee.Id,

                                // Do not show individual MDS subject
                                CourseName =
                                    $"All {masterFee.CourseLevel} Courses",

                                CourseCode = null,

                                CourseLevel = masterFee.CourseLevel,

                                AmountToBePaid = amountToBePaid,

                                // Store multiplier value for UI
                                AcademicIntake2026 = multiplier,

                                CalculationType = actualCalculationType,

                                CalculatedAmount = calculatedAmount,

                                IsApplicable = true
                            });

                            continue;
                        }

                        // =====================================================
                        // COURSE NAME BASED FEE
                        //
                        // Use this only when CourseLevel is NOT configured.
                        // =====================================================

                        if (!string.IsNullOrWhiteSpace(masterFee.CourseName))
                        {
                            var matchedCourse = collegeCourses
                                .FirstOrDefault(e =>
                                    !string.IsNullOrWhiteSpace(e.CourseName) &&
                                    string.Equals(
                                        e.CourseName.Trim(),
                                        masterFee.CourseName.Trim(),
                                        StringComparison.OrdinalIgnoreCase
                                    )
                                );

                            if (matchedCourse == null)
                            {
                                continue;
                            }

                            var savedFee = savedFeeStructure
                                .FirstOrDefault(e =>
                                    e.FeeTypeId == feeType.Id &&
                                    (
                                        e.DentalFeeStructureId == masterFee.Id ||
                                        e.CourseCode == matchedCourse.CourseCode
                                    )
                                );

                            var amountToBePaid =
                                savedFee?.AmountToBePaid ?? masterFee.AmountToBePaid;

                            var actualCalculationType =
                                savedFee?.CalculationType
                                ?? masterFee.CalculationType;

                            var academicIntake2026 =
                                savedFee?.AcademicIntake2026
                                ?? matchedCourse.Ay2026TotalIntake;

                            var calculatedAmount =
                                savedFee?.CalculatedAmount
                                ?? CalculateFee(
                                    amountToBePaid,
                                    actualCalculationType,
                                    academicIntake2026
                                );

                            courseFees.Add(new DentalCourseFeeViewModel
                            {
                                Id = savedFee?.Id,

                                DentalFeeStructureId = masterFee.Id,

                                CourseName = matchedCourse.CourseName,

                                CourseCode = matchedCourse.CourseCode,

                                CourseLevel = matchedCourse.CourseLevel,

                                AmountToBePaid = amountToBePaid,

                                AcademicIntake2026 = academicIntake2026,

                                CalculationType = actualCalculationType,

                                CalculatedAmount = calculatedAmount,

                                IsApplicable = true
                            });

                            continue;
                        }


                        // =====================================================
                        // COURSE CODE BASED FEE
                        //
                        // Use only as the last fallback.
                        // =====================================================

                        if (masterFee.CourseCode.HasValue)
                        {
                            var matchedCourse = collegeCourses
                                .FirstOrDefault(e =>
                                    e.CourseCode == masterFee.CourseCode.Value
                                );

                            if (matchedCourse == null)
                            {
                                continue;
                            }

                            var savedFees = savedFeeStructure
                                .Where(e =>
                                    e.FeeTypeId == feeType.Id &&
                                    (
                                        e.DentalFeeStructureId == masterFee.Id ||
                                        e.CourseCode == matchedCourse.CourseCode
                                    )
                                ).ToList();

                            var savedFee = savedFees.FirstOrDefault();

                            var amountToBePaid = savedFee?.AmountToBePaid ?? masterFee.AmountToBePaid;

                            var actualCalculationType = savedFee?.CalculationType ?? masterFee.CalculationType;

                            var academicIntake2026 = savedFee?.AcademicIntake2026 ?? matchedCourse.Ay2026TotalIntake;

                            var calculatedAmount =
                                savedFee?.CalculatedAmount
                                ?? CalculateFee(
                                    amountToBePaid,
                                    actualCalculationType,
                                    academicIntake2026
                                );

                            courseFees.Add(new DentalCourseFeeViewModel
                            {
                                Id = savedFee?.Id,

                                DentalFeeStructureId = masterFee.Id,

                                CourseName = matchedCourse.CourseName,

                                CourseCode = matchedCourse.CourseCode,

                                CourseLevel = matchedCourse.CourseLevel,

                                AmountToBePaid = amountToBePaid,

                                AcademicIntake2026 = academicIntake2026,

                                CalculationType = actualCalculationType,

                                CalculatedAmount = calculatedAmount,

                                IsApplicable = true
                            });
                        }
                    }

                    return new DentalFeeTypeRowViewModel
                    {
                        FeeTypeId = feeType.Id,
                        FeeType = feeType.FeeType,
                        DisplayOrder = feeType.DisplayOrder,
                        CourseFees = courseFees
                    };

                }).ToList(),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DentalPaymentSubmitViewModel model)
        {

            // =====================================================
            // VALIDATE AFFILIATION TYPE
            // =====================================================

            if (model.AffiliationTypeId <= 0) return BadRequest("Invalid Affiliation Type.");

            var collegecode = HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrWhiteSpace(collegecode)) return BadRequest("College not found.");

            var requiredCourseLevel =
                NormalizeCourseLevel(
                    HttpContext.Session.GetString("CourseLevel")
                );

            if (string.IsNullOrWhiteSpace(requiredCourseLevel))
            {
                return BadRequest("Course level is not configured.");
            }

            // =====================================================
            // VALIDATE TRANSACTION ID
            // =====================================================

            if (string.IsNullOrWhiteSpace(model.TransactionId))
            {
                TempData["ErrorMessage"] = "Please enter Transaction ID.";

                return RedirectToAction(nameof(Index), new { AffTypeId = model.AffiliationTypeId });
            }


            // =====================================================
            // GET EXISTING PAYMENT
            // =====================================================

            var existingPayment = await _context.TxnDentalPayments
                .FirstOrDefaultAsync(e =>
                    e.CollegeCode == collegecode &&
                    e.FacultyCode == _facultyCode &&
                    e.AffiliationTypeId == model.AffiliationTypeId &&
                    e.CourseLevel == requiredCourseLevel &&
                    e.IsActive
                );

            // =====================================================
            // VALIDATE RECEIPT
            //
            // Receipt is required only for first submission.
            // =====================================================


            if (existingPayment == null && (model.TransactionReceipt == null || model.TransactionReceipt.Length == 0))
            {
                TempData["ErrorMessage"] = "Please upload the transaction receipt.";
                return RedirectToAction(nameof(Index), new { AffTypeId = model.AffiliationTypeId });
            }

            // =====================================================
            // FILE VALIDATION
            // =====================================================
            if (model.TransactionReceipt != null && model.TransactionReceipt.Length > 0)
            {

                // =====================================================
                // Maximum allowed size = 1 MB
                // =====================================================

                if (model.TransactionReceipt.Length > 1 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "Transaction receipt should not exceed 1 MB.";
                    return RedirectToAction(nameof(Index), new { AffTypeId = model.AffiliationTypeId });
                }


                // =================================================
                // ALLOWED FILE TYPES
                // =================================================

                var allowedExtensions = new[]
                {
                    ".pdf",
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

                var extension = Path.GetExtension(model.TransactionReceipt.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = "Only PDF, JPG, JPEG and PNG files are allowed.";
                    return RedirectToAction(nameof(Index), new { AffTypeId = model.AffiliationTypeId });
                }
            }

            await using var transaction =  await _context.Database.BeginTransactionAsync();

            try
            {

                // =====================================================
                // GET SAVED FEE DETAILS
                // =====================================================

                var feeDetails = await _context.TxnDentalFeeStructures
                    .Where(e =>
                        e.CollegeCode == collegecode &&
                        e.FacultyCode == _facultyCode &&
                        e.AffiliationTypeId == model.AffiliationTypeId &&
                        e.CourseLevel == requiredCourseLevel &&
                        e.IsActive
                    )
                    .ToListAsync();

                // =====================================================
                // IF NOT FOUND, GENERATE FEE STRUCTURE
                // =====================================================

                if (!feeDetails.Any())
                {
                    feeDetails = await GenerateDentalFeeStructure( collegecode, model.AffiliationTypeId );

                    if (!feeDetails.Any())
                    {
                        TempData["ErrorMessage"] =  "Fee Details could not be generated for the selected affiliation.";

                        return RedirectToAction(nameof(Index), new { AffTypeId = model.AffiliationTypeId }
                        );
                    }

                    // =================================================
                    // SAVE GENERATED FEE STRUCTURE
                    // =================================================

                    await _context.TxnDentalFeeStructures.AddRangeAsync(feeDetails);

                }

                // =====================================================
                // GRAND TOTAL
                // =====================================================

                var grandTotal =
                    feeDetails.Sum(e => e.CalculatedAmount);

                if (grandTotal <= 0)
                {
                    TempData["ErrorMessage"] =  "Invalid Payment Amount.";

                    return RedirectToAction( nameof(Index), new { AffTypeId = model.AffiliationTypeId }  );
                }


                // =====================================================
                // UPLOAD RECEIPT
                // =====================================================

                string? receiptPath = existingPayment?.TransactionReceiptPath;

                if(model.TransactionReceipt != null && model.TransactionReceipt.Length > 0)
                {
                    // Folder structure:
                    //
                    // E:\Affiliation_Dental\TransactionReceipts\
                    //
                    // or
                    //
                    // D:\Affiliation_Dental\TransactionReceipts\

                    receiptPath = await SaveFileAndReturnPath(
                        model.TransactionReceipt,
                        "TransactionReceipts",
                        $"DentalPayment_{collegecode}_{model.AffiliationTypeId}"
                    );
                }

                // =====================================================
                // SAVE NEW PAYMENT
                // =====================================================

                if (existingPayment == null)
                {
                    var payment = new TxnDentalPayment
                    {
                        CollegeCode = collegecode,
                        FacultyCode = _facultyCode,
                        CourseLevel = requiredCourseLevel,
                        AffiliationTypeId = model.AffiliationTypeId,
                        TransactionId = model.TransactionId.Trim(),
                        TransactionReceiptPath = receiptPath ?? string.Empty,
                        AmountPaid = grandTotal,
                        IsActive = true,
                        CreatedBy = collegecode,
                        CreatedDate = DateTime.Now
                    };

                    _context.TxnDentalPayments.Add(payment);
                }
                else
                {

                    // =================================================
                    // UPDATE EXISTING PAYMENT
                    // =================================================
                    existingPayment.TransactionId = model.TransactionId.Trim();
                    existingPayment.AmountPaid = grandTotal;
                    existingPayment.CourseLevel = requiredCourseLevel;


                    // Only replace receipt if a new one is uploaded

                    if (!string.IsNullOrWhiteSpace(receiptPath)) existingPayment.TransactionReceiptPath = receiptPath;

                    existingPayment.IsActive = true;
                    existingPayment.ModifiedBy = collegecode;
                    existingPayment.ModifiedDate = DateTime.Now;
                    _context.TxnDentalPayments.Update(existingPayment);
                }

                // =====================================================
                // SAVE ALL CHANGES
                // =====================================================

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Transaction Details saved successfully.";

                return RedirectToAction(nameof(Index), new { AffTypeId = model.AffiliationTypeId });

            }

            catch
            {
                await transaction.RollbackAsync();

                TempData["ErrorMessage"] = "Unable to save payment details. Please try again.";

                return RedirectToAction(nameof(Index), new { AffTypeId = model.AffiliationTypeId });
            }

            
        }

        private decimal CalculateFee( decimal amountToBePaid, string? calculationType, int academicIntake2026)
        {
            return calculationType?.Trim().ToLower() switch
            {
                "per seat" => amountToBePaid * academicIntake2026,

                "per course" => amountToBePaid,

                "fixed" => amountToBePaid,

                _ => amountToBePaid
            };
        }


        // =====================================================
        // VIEW TRANSACTION RECEIPT
        //
        // Returns the receipt file to the browser.
        // PDF files will open in the browser's built-in PDF viewer,
        // while JPG/PNG files will open normally.
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> ViewReceipt(int paymentId)
        {
            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrWhiteSpace(collegeCode))
                return Unauthorized();

            var payment = await _context.TxnDentalPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.Id == paymentId &&
                    e.CollegeCode == collegeCode &&
                    e.FacultyCode == _facultyCode &&
                    e.IsActive);

            if (payment == null) return NotFound();

            if (string.IsNullOrWhiteSpace( payment.TransactionReceiptPath)) return NotFound("Receipt not found.");

            if (!System.IO.File.Exists(payment.TransactionReceiptPath)) return NotFound("Receipt file not found.");

            var extension =
                Path.GetExtension(
                    payment.TransactionReceiptPath)
                .ToLowerInvariant();

            var model = new DentalDocumentViewerViewModel
            {
                FileUrl = Url.Action( nameof(GetReceiptFile), "DentalPayment", new { paymentId = payment.Id })!,
                FileType = extension
            };

            return PartialView( "_DocumentViewer", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetReceiptFile(int paymentId)
        {
            var collegeCode =
                HttpContext.Session.GetString("CollegeCode");

            if (string.IsNullOrWhiteSpace(collegeCode))
                return Unauthorized();

            var payment = await _context.TxnDentalPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.Id == paymentId &&
                    e.CollegeCode == collegeCode &&
                    e.FacultyCode == _facultyCode &&
                    e.IsActive);

            if (payment == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(
                payment.TransactionReceiptPath))
                return NotFound();

            if (!System.IO.File.Exists(
                payment.TransactionReceiptPath))
                return NotFound();

            var extension =
                Path.GetExtension(
                    payment.TransactionReceiptPath)
                .ToLowerInvariant();

            var contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };

            return PhysicalFile(
                payment.TransactionReceiptPath, contentType);
        }

        private async Task<List<TxnDentalFeeStructure>> GenerateDentalFeeStructure( string collegeCode, int affiliationTypeId)
        {
            // =====================================================
            // GET SELECTED COURSE LEVEL
            // =====================================================

            var requiredCourseLevel =
                HttpContext.Session.GetString("CourseLevel");

            if (string.IsNullOrWhiteSpace(requiredCourseLevel))
            {
                throw new InvalidOperationException(
                    "Course level is not configured.");
            }

            // =====================================================
            // GET ACADEMIC INTAKE
            // =====================================================

            var academicIntake = await _context.AcademicIntakes
                .AsNoTracking()
                .Where(e =>
                    e.CollegeCode == collegeCode &&
                    e.FacultyCode == _facultyCode.ToString()
                )
                .ToListAsync();

            // =====================================================
            // GET ALL DENTAL COURSES
            // =====================================================

            var dentalCourses = await _context.MstCourses
                .AsNoTracking()
                .Where(e =>
                    e.FacultyCode == _facultyCode)
                .ToListAsync();

            // =====================================================
            // MAP COLLEGE COURSES WITH AY2026 INTAKE
            // =====================================================

            var collegeCourses = new List<DentalCollegeCourseViewModel>();

            foreach (var intake in academicIntake)
            {
                if (string.IsNullOrWhiteSpace(intake.Courses)) continue;

                if (!int.TryParse( intake.Courses.Trim(), out int courseCode))
                {
                    continue;
                }

                var course = dentalCourses.FirstOrDefault(e =>
                    e.CourseCode == courseCode &&
                    string.Equals(
                        NormalizeCourseLevel(e.CourseLevel),
                        NormalizeCourseLevel(requiredCourseLevel),
                        StringComparison.OrdinalIgnoreCase
                    )
                );

                if (course == null)
                    continue;

                if (!collegeCourses.Any(e => e.CourseCode == course.CourseCode))
                {
                    collegeCourses.Add(
                        new DentalCollegeCourseViewModel
                        {
                            CourseCode = course.CourseCode,
                            CourseName = course.CourseName,
                            CourseLevel = course.CourseLevel,
                            Ay2026TotalIntake =
                                intake.Ay2026TotalIntake
                        });
                }
            }

            // =====================================================
            // GET DENTAL FEE TYPES
            // =====================================================

            var feeTypes = await _context.MstDentalFeeTypes
                .AsNoTracking()
                .Where(e =>
                    e.FacultyCode == _facultyCode &&
                    e.AffiliationTypeId == affiliationTypeId &&
                    e.IsActive
                )
                .OrderBy(e => e.DisplayOrder)
                .ToListAsync();

            // =====================================================
            // GET MASTER FEE STRUCTURE
            // =====================================================

            var feeStructures =
                await _context.MstDentalFeeStructures
                    .AsNoTracking()
                    .Where(e =>
                        e.FacultyCode == _facultyCode &&
                        e.AffiliationTypeId == affiliationTypeId &&
                        e.IsActive
                    )
                    .ToListAsync();

            // =====================================================
            // RESULT
            // =====================================================

            var feeDetails =  new List<TxnDentalFeeStructure>();

            // =====================================================
            // GENERATE FEE STRUCTURE
            // =====================================================

            foreach (var feeType in feeTypes)
            {
                var applicableMasterFees =
                    feeStructures
                        .Where(e =>
                            e.FeeTypeId == feeType.Id)
                        .ToList();

                foreach (var masterFee in applicableMasterFees)
                {
                    var calculationType = masterFee.CalculationType?.Trim();

                    // =================================================
                    // COURSE LEVEL BASED FEE
                    //
                    // Example:
                    //
                    // UG -> All UG Courses
                    // PG -> All PG / MDS Courses
                    //
                    // ONE combined record is created.
                    // =================================================

                    // =================================================
                    // COURSE LEVEL BASED FEE
                    //
                    // UI will show:
                    //     All PG Courses
                    //
                    // DATABASE will save:
                    //     Actual CourseCode
                    //     Actual CourseName
                    //     Actual CourseLevel
                    //     Actual Intake
                    //
                    // Calculation remains correct.
                    // =================================================

                    if (!string.IsNullOrWhiteSpace(masterFee.CourseLevel))
                    {
                        var masterCourseLevel =
                            NormalizeCourseLevel(masterFee.CourseLevel);

                        var matchedCourses = collegeCourses
                            .Where(e =>
                                string.Equals(
                                    NormalizeCourseLevel(e.CourseLevel),
                                    masterCourseLevel,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                            .ToList();

                        if (!matchedCourses.Any())
                            continue;

                        // =================================================
                        // SAVE ONE RECORD FOR EACH ACTUAL COURSE
                        // =================================================

                        for (int i = 0; i < matchedCourses.Count; i++)
                        {
                            var matchedCourse = matchedCourses[i];

                            decimal calculatedAmount;

                            // =================================================
                            // PER SEAT
                            //
                            // Example:
                            //
                            // Prosthodontics = 3 seats
                            // ₹4,500 × 3 = ₹13,500
                            // =================================================

                            if (string.Equals(
                                calculationType,
                                "Per Seat",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                calculatedAmount =
                                    masterFee.AmountToBePaid *
                                    matchedCourse.Ay2026TotalIntake;
                            }

                            // =================================================
                            // PER COURSE
                            //
                            // Each actual course gets one fee.
                            // =================================================

                            else if (string.Equals(
                                calculationType,
                                "Per Course",
                                StringComparison.OrdinalIgnoreCase))
                            {
                                calculatedAmount =
                                    masterFee.AmountToBePaid;
                            }

                            // =================================================
                            // FIXED
                            //
                            // Save fixed fee only once.
                            // =================================================

                            else
                            {
                                if (i > 0)
                                    continue;

                                calculatedAmount =
                                    masterFee.AmountToBePaid;
                            }

                            // =================================================
                            // SAVE ACTUAL COURSE DETAILS
                            // =================================================

                            feeDetails.Add(
                                new TxnDentalFeeStructure
                                {
                                    CollegeCode = collegeCode,

                                    FacultyCode = _facultyCode,

                                    AffiliationTypeId =
                                        affiliationTypeId,

                                    FeeTypeId =
                                        feeType.Id,

                                    DentalFeeStructureId =
                                        masterFee.Id,

                                    // ACTUAL COURSE CODE
                                    CourseCode =
                                        matchedCourse.CourseCode,

                                    // ACTUAL COURSE NAME
                                    CourseName =
                                        matchedCourse.CourseName,

                                    // ACTUAL COURSE LEVEL
                                    CourseLevel =
                                        matchedCourse.CourseLevel,

                                    // MASTER UNIT FEE
                                    AmountToBePaid =
                                        masterFee.AmountToBePaid,

                                    // ACTUAL COURSE INTAKE
                                    AcademicIntake2026 =
                                        matchedCourse.Ay2026TotalIntake,

                                    CalculationType =
                                        calculationType,

                                    // COURSE-WISE CALCULATED AMOUNT
                                    CalculatedAmount =
                                        calculatedAmount,

                                    IsActive = true,

                                    CreatedBy = collegeCode,

                                    CreatedDate = DateTime.Now
                                });
                        }

                        continue;
                    }

                    // =================================================
                    // COURSE NAME BASED FEE
                    // =================================================

                    if (!string.IsNullOrWhiteSpace(
                        masterFee.CourseName))
                    {
                        var matchedCourse =
                            collegeCourses.FirstOrDefault(e =>
                                !string.IsNullOrWhiteSpace(
                                    e.CourseName) &&
                                string.Equals(
                                    e.CourseName.Trim(),
                                    masterFee.CourseName.Trim(),
                                    StringComparison
                                        .OrdinalIgnoreCase
                                )
                            );

                        if (matchedCourse == null)
                            continue;

                        var academicIntake2026 =
                            matchedCourse.Ay2026TotalIntake;

                        var calculatedAmount =
                            CalculateFee(
                                masterFee.AmountToBePaid,
                                calculationType,
                                academicIntake2026
                            );

                        feeDetails.Add(
                            new TxnDentalFeeStructure
                            {
                                CollegeCode = collegeCode,

                                FacultyCode = _facultyCode,

                                AffiliationTypeId = affiliationTypeId,

                                FeeTypeId = feeType.Id,

                                DentalFeeStructureId = masterFee.Id,

                                CourseCode = matchedCourse.CourseCode,

                                CourseName = matchedCourse.CourseName,

                                CourseLevel =  matchedCourse.CourseLevel,

                                AmountToBePaid = masterFee.AmountToBePaid,

                                AcademicIntake2026 = academicIntake2026,

                                CalculationType = calculationType,

                                CalculatedAmount = calculatedAmount,

                                IsActive = true,

                                CreatedBy = collegeCode,

                                CreatedDate = DateTime.Now
                            });

                        continue;
                    }

                    // =================================================
                    // COURSE CODE BASED FEE
                    // =================================================

                    if (masterFee.CourseCode.HasValue)
                    {
                        var matchedCourse =
                            collegeCourses.FirstOrDefault(e =>
                                e.CourseCode ==
                                masterFee.CourseCode.Value);

                        if (matchedCourse == null)
                            continue;

                        var academicIntake2026 =
                            matchedCourse.Ay2026TotalIntake;

                        var calculatedAmount =
                            CalculateFee(
                                masterFee.AmountToBePaid,
                                calculationType,
                                academicIntake2026
                            );

                        feeDetails.Add(
                            new TxnDentalFeeStructure
                            {
                                CollegeCode = collegeCode,

                                FacultyCode = _facultyCode,

                                AffiliationTypeId = affiliationTypeId,

                                FeeTypeId = feeType.Id,

                                DentalFeeStructureId = masterFee.Id,

                                CourseCode = matchedCourse.CourseCode,

                                CourseName = matchedCourse.CourseName,

                                CourseLevel = matchedCourse.CourseLevel,

                                AmountToBePaid = masterFee.AmountToBePaid,

                                AcademicIntake2026 = academicIntake2026,

                                CalculationType =calculationType,

                                CalculatedAmount = calculatedAmount,

                                IsActive = true,

                                CreatedBy = collegeCode,

                                CreatedDate = DateTime.Now
                            });
                    }
                }
            }

            return feeDetails;
        }

        [HttpGet]
        public async Task<IActionResult> Calculate()
        {
            var vm = await LoadSessionCalculationDetailsAsync();
            await CalculateForCurrentSessionAsync(vm);
            return View("Index", vm);
        }

        // POST: /PaymentCalculation/Calculate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(PaymentCalculationViewModel vm)
        {
            var sessionDetails = await LoadSessionCalculationDetailsAsync();
            vm.CollegeCode = sessionDetails.CollegeCode;
            vm.FacultyCode = sessionDetails.FacultyCode;
            vm.AffiliationTypeId = sessionDetails.AffiliationTypeId;
            vm.TypeOfAffiliation = sessionDetails.TypeOfAffiliation;
            vm.CourseLevel = sessionDetails.CourseLevel;
            vm.CourseLevelGroup = sessionDetails.CourseLevelGroup;
            vm.AcademicYear = PaymentAcademicYear;
            vm.AffiliationTypeList = sessionDetails.AffiliationTypeList;

            if (string.IsNullOrWhiteSpace(vm.CollegeCode))
            {
                vm.ErrorMessage = "College code is required.";
                return View("Index", vm);
            }

            if (vm.AffiliationTypeId <= 0)
            {
                vm.ErrorMessage = "Affiliation type is not available in the current session.";
                return View("Index", vm);
            }

            if (string.IsNullOrWhiteSpace(vm.CourseLevelGroup))
            {
                vm.ErrorMessage = "No affiliation type is configured for the current course level.";
                return View("Index", vm);
            }

            await CalculateForCurrentSessionAsync(vm);

            return View("Index", vm);
        }

        private async Task CalculateForCurrentSessionAsync(PaymentCalculationViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.CollegeCode)
                || vm.AffiliationTypeId <= 0
                || string.IsNullOrWhiteSpace(vm.CourseLevelGroup))
            {
                return;
            }

            try
            {
                await RunPaymentCalculationAsync(vm);
                vm.HasResult = true;
            }
            catch (SqlException ex)
            {
                vm.ErrorMessage = "Could not calculate payment: " + ex.Message;
                vm.HasResult = false;
            }
        }

        private async Task<PaymentCalculationViewModel> LoadSessionCalculationDetailsAsync()
        {
            var affiliationTypeId = HttpContext.Session.GetInt32("AffiliationType") ?? 0;
            if (affiliationTypeId <= 0
                && int.TryParse(HttpContext.Session.GetString("AffiliationTypeId"), out var parsedId))
            {
                affiliationTypeId = parsedId;
            }

            var facultyCode = HttpContext.Session.GetString("FacultyCode");
            var courseLevel = HttpContext.Session.GetString("CourseLevel")
                ?? HttpContext.Session.GetString("SelectedCourseLevel");
            var normalizedCourseLevel = courseLevel?.Trim().ToUpperInvariant() ?? string.Empty;

            var sessionAffiliationName = HttpContext.Session.GetString("TypeOfAffiliation");
            var normalizedFacultyCode = NormalizeLabel(facultyCode);
            var affiliationTypes = await _context.MstAffiliationTypes
                .AsNoTracking()
                .Where(x => x.IsActive && x.CourseLevelGroup != null)
                .OrderByDescending(x => x.AcademicYear)
                .ToListAsync();

            var affiliationType = affiliationTypes
                .Where(x => NormalizeLabel(x.FacultyCode) == normalizedFacultyCode
                    && IsAffiliationTypeMatch(x, affiliationTypeId, sessionAffiliationName)
                    && IsCourseLevelMatch(x.CourseLevelGroup, normalizedCourseLevel))
                .OrderByDescending(x => IsAffiliationTypeMatchById(x, affiliationTypeId))
                .ThenByDescending(x => string.Equals(x.AcademicYear, PaymentAcademicYear, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(x => x.AcademicYear)
                .FirstOrDefault();

            return new PaymentCalculationViewModel
            {
                CollegeCode = HttpContext.Session.GetString("CollegeCode"),
                FacultyCode = int.TryParse(facultyCode, out var facultyId)
                    ? facultyId
                    : 0,
                AffiliationTypeId = affiliationType?.AffiliationTypeId ?? affiliationTypeId,
                TypeOfAffiliation = GetAffiliationDisplayName(affiliationType?.AffiliationCategory)
                    ?? HttpContext.Session.GetString("TypeOfAffiliation"),
                CourseLevel = courseLevel,
                CourseLevelGroup = affiliationType?.CourseLevelGroup,
                AcademicYear = PaymentAcademicYear,
                AffiliationTypeList = new List<AffiliationTypeOption>()
            };
        }

        private static bool IsCourseLevelMatch(string? courseLevelGroup, string courseLevel)
        {
            var sessionLevel = NormalizeCourseLevel(courseLevel);
            var databaseLevel = NormalizeCourseLevel(courseLevelGroup);

            return !string.IsNullOrEmpty(sessionLevel)
                && !string.IsNullOrEmpty(databaseLevel)
                && string.Equals(databaseLevel, sessionLevel, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeCourseLevel(string? courseLevel)
        {
            var normalizedLevel = NormalizeLabel(courseLevel);

            return normalizedLevel switch
            {
                "UG" => "UG",
                "PGBROAD" => "PG",
                "PGSS" => "SS",
                "PG" => "PG",
                "SS" => "SS",
                _ => normalizedLevel
            };
        }

        private static bool AreAffiliationNamesEqual(string? databaseName, string? sessionName)
        {
            var databaseValue = NormalizeLabel(databaseName);
            var sessionValue = NormalizeLabel(sessionName);

            return !string.IsNullOrEmpty(databaseValue)
                && !string.IsNullOrEmpty(sessionValue)
                && (databaseValue == sessionValue
                    || databaseValue.Contains(sessionValue, StringComparison.OrdinalIgnoreCase)
                    || sessionValue.Contains(databaseValue, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsAffiliationTypeMatch(
            MstAffiliationType affiliationType,
            int sessionAffiliationTypeId,
            string? sessionAffiliationName)
        {
            var sessionGroup = GetAffiliationGroup(sessionAffiliationName);
            var databaseGroup = GetAffiliationGroup(affiliationType.AffiliationCategory);

            if (!string.IsNullOrEmpty(sessionGroup) && !string.IsNullOrEmpty(databaseGroup))
            {
                return string.Equals(sessionGroup, databaseGroup, StringComparison.OrdinalIgnoreCase);
            }

            return affiliationType.AffiliationTypeId == sessionAffiliationTypeId
                || AreAffiliationNamesEqual(affiliationType.AffiliationCategory, sessionAffiliationName);
        }

        private static bool IsAffiliationTypeMatchById(MstAffiliationType affiliationType, int sessionAffiliationTypeId)
        {
            return affiliationType.AffiliationTypeId == sessionAffiliationTypeId;
        }

        private static string? GetAffiliationGroup(string? affiliationCategory)
        {
            var normalizedCategory = NormalizeLabel(affiliationCategory);

            if (normalizedCategory.Contains("CONTINUATION", StringComparison.OrdinalIgnoreCase))
            {
                return "CONTINUATION";
            }

            if (normalizedCategory.Contains("INCREASE", StringComparison.OrdinalIgnoreCase)
                || normalizedCategory.Contains("ENHANCEMENT", StringComparison.OrdinalIgnoreCase))
            {
                return "ENHANCEMENT";
            }

            if (normalizedCategory.Contains("FRESH", StringComparison.OrdinalIgnoreCase))
            {
                return "FRESH";
            }

            return null;
        }

        private static string? GetAffiliationDisplayName(string? affiliationCategory)
        {
            return GetAffiliationGroup(affiliationCategory) switch
            {
                "FRESH" => "Fresh Affiliation",
                "CONTINUATION" => "Continuation of Affiliation",
                "ENHANCEMENT" => "Enhancement of Seats / Increase in Intake",
                _ => affiliationCategory
            };
        }

        private static string NormalizeLabel(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        }

        private async Task<List<AffiliationTypeOption>> GetAffiliationTypesAsync()
        {
            var list = new List<AffiliationTypeOption>();

            const string sql = @"SELECT AffiliationTypeId, AffiliationCategory, FormNo
                                  FROM MstAffiliationType
                                  WHERE IsActive = 1
                                  ORDER BY AffiliationTypeId";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new AffiliationTypeOption
                        {
                            AffiliationTypeId = reader.GetInt32(reader.GetOrdinal("AffiliationTypeId")),
                            AffiliationCategory = reader.GetString(reader.GetOrdinal("AffiliationCategory")),
                            FormNo = reader.GetString(reader.GetOrdinal("FormNo"))
                        });
                    }
                }
            }

            return list;
        }

        private async Task RunPaymentCalculationAsync(PaymentCalculationViewModel vm)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("usp_CalculateCollegeAffiliationPayment", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CollegeCode", vm.CollegeCode);
                cmd.Parameters.AddWithValue("@FacultyCode", vm.FacultyCode);
                cmd.Parameters.AddWithValue("@AffiliationTypeId", vm.AffiliationTypeId);
                cmd.Parameters.AddWithValue("@AcademicYear", vm.AcademicYear);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    // ---- Result Set 1: matched courses ----
                    while (await reader.ReadAsync())
                    {
                        vm.MatchedCourses.Add(new MatchedCourseVM
                        {
                            SLNO = ReadInt32(reader, "SLNO"),
                            Facultycode = ReadInt32(reader, "Facultycode"),
                            coll_code = reader["coll_code"] as string,
                            collegename = reader["collegename"] as string,
                            course = reader["course"] as string,
                            ug_pg = reader["ug_pg"] as string,
                            Intake_26_27 = ReadNullableInt32(reader, "Intake_26_27"),
                            CourseCode = ReadString(reader, "CourseCode"),
                            CourseName = reader["CourseName"] as string,
                            RawCourseLevel = reader["RawCourseLevel"] as string,
                            MatchNote = reader["MatchNote"] as string
                        });
                    }

                    // ---- Result Set 2: fee lines ----
                    await reader.NextResultAsync();
                    while (await reader.ReadAsync())
                    {
                        vm.FeeLines.Add(new FeeLineVM
                        {
                            FeeHeadName = reader["FeeHeadName"] as string,
                            UnitAmount = ReadDecimal(reader, "UnitAmount"),
                            IsPerCourse = ReadBoolean(reader, "IsPerCourse"),
                            IsPerSeat = ReadBoolean(reader, "IsPerSeat"),
                            SeatRangeFrom = ReadNullableInt32(reader, "SeatRangeFrom"),
                            SeatRangeTo = ReadNullableInt32(reader, "SeatRangeTo"),
                            Multiplier = ReadInt32(reader, "Multiplier"),
                            LineAmount = ReadDecimal(reader, "LineAmount")
                        });
                    }

                    // ---- Result Set 3: summary ----
                    await reader.NextResultAsync();
                    if (await reader.ReadAsync())
                    {
                        vm.Summary = new PaymentSummaryVM
                        {
                            CollegeCode = reader["CollegeCode"] as string,
                            FacultyCode = ReadInt32(reader, "FacultyCode"),
                            AffiliationTypeId = ReadInt32(reader, "AffiliationTypeId"),
                            CourseLevelGroup = reader["CourseLevelGroup"] as string,
                            MatchedCourseCount = ReadInt32(reader, "MatchedCourseCount"),
                            TotalIntakeSeats = ReadInt32(reader, "TotalIntakeSeats"),
                            GrandTotal = reader.IsDBNull(reader.GetOrdinal("GrandTotal"))
                                            ? 0m
                                            : ReadDecimal(reader, "GrandTotal")
                        };
                    }
                }
            }
        }

        private static int ReadInt32(SqlDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName))
                ? 0
                : Convert.ToInt32(reader[columnName]);
        }

        private static int? ReadNullableInt32(SqlDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName))
                ? null
                : Convert.ToInt32(reader[columnName]);
        }

        private static decimal ReadDecimal(SqlDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName))
                ? 0m
                : Convert.ToDecimal(reader[columnName]);
        }

        private static string? ReadString(SqlDataReader reader, string columnName)
        {
            return reader.IsDBNull(reader.GetOrdinal(columnName))
            ? null
            : Convert.ToString(reader[columnName]);
        }

        private static bool ReadBoolean(SqlDataReader reader, string columnName)
        {
            if (reader.IsDBNull(reader.GetOrdinal(columnName)))
            {
                return false;
            }

            var value = reader[columnName];
            return value is bool booleanValue
                ? booleanValue
                : Convert.ToInt32(value) != 0;
        }
    }
}
