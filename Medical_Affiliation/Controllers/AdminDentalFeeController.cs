
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Controllers;

[Authorize]
public class AdminDentalFeeController : Controller
{
    private readonly ApplicationDbContext _context;
    private const int FacultyCode = 2;

    public AdminDentalFeeController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index( string? collegeCode, int? affiliationTypeId, string? courseLevel)
    {
        var currentUtc = DateTime.UtcNow;

        var model = new AdminDentalFeeViewModel
        {
            CollegeCode = collegeCode,
            AffiliationTypeId = affiliationTypeId,
            CourseLevel = courseLevel,
            FacultyCode = FacultyCode,
            CurrentUtc = currentUtc
        };

        // 1. Load colleges
        model.Colleges = await GetDentalCollegesAsync();

        // 2. Load active dental affiliation types.
        // Use MstDentalAffiliationTypes because it contains
        // DentalAffiliationTypeId and CourseLevelGroup.
        var dentalAffiliationTypes = await _context.MstDentalAffiliationTypes
            .AsNoTracking()
            .Where(x =>
                x.FacultyCode == FacultyCode &&
                x.IsActive)
            .OrderBy(x => x.AffiliationCategory)
            .ToListAsync();

        model.AffiliationTypes = dentalAffiliationTypes
            .Select(x => new SelectListItem
            {
                Value = x.TypeId.ToString(),
                Text = $"{x.AffiliationCategory} - {x.CourseLevelGroup}",
                Selected = x.TypeId == affiliationTypeId
            })
            .ToList();

        // 3. Course-level dropdown
        model.CourseLevels = new List<SelectListItem>
        {
            new() { Value = "UG", Text = "Undergraduate (UG)" },
            new() { Value = "PG", Text = "Postgraduate (PG)" }
        };

        // 4. Do not load financial records until filters are selected.
        if (string.IsNullOrWhiteSpace(collegeCode) ||
            !affiliationTypeId.HasValue)
        {
            return View(model);
        }

        // 5. Validate college
        var institution = await _context.AffInstitutionsDetails
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.FacultyCode == FacultyCode.ToString() &&
                x.CollegeCode == collegeCode);

        if (institution == null)
        {
            return NotFound("College not found.");
        }

        // 6. Validate the selected dental affiliation type.
        var affiliationType = dentalAffiliationTypes
            .FirstOrDefault(x =>
                x.TypeId == affiliationTypeId.Value && x.CourseLevelGroup == courseLevel);

        if (affiliationType == null)
        {
            ModelState.AddModelError(
                nameof(model.AffiliationTypeId),
                "Invalid or inactive dental affiliation type.");

            return View(model);
        }

        // 7. Resolve UG/PG from the selected dental affiliation type.
        var resolvedCourseLevel =
            affiliationType.CourseLevelGroup?.Trim().ToUpperInvariant();

        if (resolvedCourseLevel is not ("UG" or "PG"))
        {
            ModelState.AddModelError(
                nameof(model.CourseLevel),
                "Invalid course level configured for this affiliation type.");

            return View(model);
        }

        // Reject a course level that conflicts with the affiliation type.
        if (!string.IsNullOrWhiteSpace(courseLevel) &&
            !string.Equals(
                courseLevel.Trim(),
                resolvedCourseLevel,
                StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(
                nameof(model.CourseLevel),
                "Course level does not match the selected affiliation type.");

            return View(model);
        }

        model.CourseLevel = resolvedCourseLevel;

        // 8. Determine government/private institution.
        model.TypeOfInstitution = institution.TypeOfInstitution;

        model.IsGovernment = string.Equals(
            institution.TypeOfInstitution?.Trim(),
            "11",
            StringComparison.OrdinalIgnoreCase);

        // 9. Load applicable fee types.
        var feeTypes = await _context.MstDentalFeeTypes
            .AsNoTracking()
            .Where(e =>
                e.FacultyCode == FacultyCode &&
                e.AffiliationTypeId == affiliationTypeId.Value &&
                e.IsActive &&
                (
                    (
                        model.IsGovernment &&
                        (
                            e.FeeType == "Application Fee" ||
                            e.FeeType == "Course Identification Fee" ||
                            (
                                e.FeeType == "Late Submission Fee" &&
                                e.ActivationDate.HasValue &&
                                e.ActivationDate.Value <= currentUtc
                            )
                        )
                    )
                    ||
                    (
                        !model.IsGovernment &&
                        (
                            e.FeeType != "Late Submission Fee" ||
                            (
                                e.ActivationDate.HasValue &&
                                e.ActivationDate.Value <= currentUtc
                            )
                        )
                    )
                ))
            .OrderBy(e => e.DisplayOrder)
            .ToListAsync();

        var applicableFeeTypes = feeTypes.ToDictionary(e => e.Id);

        // 10. Load saved fee transactions for the selected UG/PG level.
        var savedFees = await GetDentalFeeTransactionsAsync(
            collegeCode,
            affiliationTypeId.Value,
            resolvedCourseLevel);

        // 11. Load all fee-type names, including inactive fee types.
        var allFeeTypes = await _context.MstDentalFeeTypes
            .AsNoTracking()
            .Where(e =>
                e.FacultyCode == FacultyCode &&
                e.CourseLevel == courseLevel &&
                e.AffiliationTypeId == affiliationTypeId.Value)
            .ToDictionaryAsync(e => e.Id);

        // 12. Map saved fees to the view model.
        model.FeeItems = savedFees.Select(fee =>
        {
            allFeeTypes.TryGetValue(fee.FeeTypeId, out var masterFee);

            var isApplicable =
                applicableFeeTypes.ContainsKey(fee.FeeTypeId);

            return new AdminDentalFeeItemViewModel
            {
                Id = fee.Id,
                FeeTypeId = fee.FeeTypeId,
                FeeTypeName = masterFee?.FeeType
                    ?? $"Fee Type #{fee.FeeTypeId}",

                DisplayOrder = masterFee?.DisplayOrder ?? 0,
                ActivationDate = masterFee?.ActivationDate,

                IsApplicable = isApplicable,
                ApplicabilityReason = isApplicable
                    ? "Currently applicable"
                    : "Inactive or excluded by eligibility rules",

                DentalFeeStructureId = fee.DentalFeeStructureId,
                CourseName = fee.CourseName,
                CourseCode = fee.CourseCode,
                CourseLevel = fee.CourseLevel,
                CalculationType = fee.CalculationType,
                AcademicIntake2026 = fee.AcademicIntake2026,
                AmountToBePaid = fee.AmountToBePaid,
                CalculatedAmount = fee.CalculatedAmount,
                IsActive = fee.IsActive
            };
        }).ToList();

        // 13. Load payment records for the selected UG/PG level.
        var payments = await GetDentalPaymentsAsync(
            collegeCode,
            affiliationTypeId.Value,
            resolvedCourseLevel);

        model.Payments = payments.Select(payment =>
            new AdminDentalPaymentViewModel
            {
                Id = payment.Id,
                CollegeCode = payment.CollegeCode,
                FacultyCode = payment.FacultyCode,
                AffiliationTypeId = payment.AffiliationTypeId,
                CourseLevel = payment.CourseLevel,
                TransactionId = payment.TransactionId,
                TransactionReceiptPath = payment.TransactionReceiptPath,
                AmountPaid = payment.AmountPaid,
                IsActive = payment.IsActive,
                CreatedDate = payment.CreatedDate,
                ModifiedDate = payment.ModifiedDate,
                CreatedBy = payment.CreatedBy,
                ModifiedBy = payment.ModifiedBy
            }).ToList();

        // 14. Calculate total applicable fees.
        model.TotalApplicableFee = model.FeeItems
            .Where(x => x.IsApplicable && x.IsActive)
            .Sum(x => x.CalculatedAmount);

        // 15. Calculate total active payments.
        model.TotalAmountPaid = model.Payments
            .Where(x => x.IsActive)
            .Sum(x => x.AmountPaid);

        return View(model);
    }
    private async Task<List<SelectListItem>> GetDentalCollegesAsync()
    {
        return await (
            from inst in _context.AffInstitutionsDetails.AsNoTracking()
            join college in _context.AffiliationCollegeMasters.AsNoTracking()
                on inst.CollegeCode equals college.CollegeCode
            where inst.CollegeCode != null
                  && inst.FacultyCode == FacultyCode.ToString()
            select new SelectListItem
            {
                Value = inst.CollegeCode,
                Text = college.CollegeName
            })
            .Distinct()
            .OrderBy(x => x.Text)
            .ToListAsync();
    }

    private async Task<List<TypeOfAffiliation>> GetDentalAffiliationTypesAsync()
    {
        return await _context.TypeOfAffiliations
            .AsNoTracking()
            .OrderBy(x => x.TypeDescription)
            .ToListAsync();
    }


    private async Task<List<TxnDentalFeeStructure>> GetDentalFeeTransactionsAsync( string collegeCode, int affiliationTypeId, string courseLevel)
    {
        courseLevel = courseLevel.Trim().ToUpperInvariant();

        return await _context.TxnDentalFeeStructures
            .AsNoTracking()
            .Where(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyCode == FacultyCode &&
                x.AffiliationTypeId == affiliationTypeId &&
                x.CourseLevel != null &&
                x.CourseLevel.Trim().ToUpper() == courseLevel)
            .OrderBy(x => x.FeeTypeId)
            .ThenBy(x => x.CourseName)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    private async Task<List<TxnDentalPayment>> GetDentalPaymentsAsync(string collegeCode, int affiliationTypeId, string courseLevel)
    {
        courseLevel = courseLevel.Trim().ToUpperInvariant();

        return await _context.TxnDentalPayments
            .AsNoTracking()
            .Where(x =>
                x.CollegeCode == collegeCode &&
                x.FacultyCode == FacultyCode &&
                x.AffiliationTypeId == affiliationTypeId &&
                x.CourseLevel != null &&
                x.CourseLevel.Trim().ToUpper() == courseLevel)
            .OrderByDescending(x => x.CreatedDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFeeStatus(
        int id,
        string? collegeCode,
        int? affiliationTypeId,
        string? courseLevel)
    {
        var fee = await _context.TxnDentalFeeStructures
            .FirstOrDefaultAsync(x => x.Id == id && x.FacultyCode == 2);

        if (fee == null)
        {
            TempData["ErrorMessage"] = "Fee record not found.";
            return RedirectToAction(nameof(Index),
                new { collegeCode, affiliationTypeId, courseLevel });
        }

        fee.IsActive = !fee.IsActive;
        fee.ModifiedDate = DateTime.UtcNow;
        fee.ModifiedBy = User.Identity?.Name ?? "Admin";

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = fee.IsActive
            ? "Fee record activated successfully."
            : "Fee record deactivated successfully.";

        return RedirectToAction(nameof(Index), new
        {
            collegeCode = fee.CollegeCode,
            affiliationTypeId = fee.AffiliationTypeId,
            courseLevel = fee.CourseLevel
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePaymentAmount(
        int id,
        decimal amountPaid,
        string? collegeCode,
        int? affiliationTypeId,
        string? courseLevel)
    {
        if (amountPaid < 0 || amountPaid > 999999999999m)
        {
            TempData["ErrorMessage"] = "Please enter a valid payment amount.";

            return RedirectToAction(nameof(Index), new
            {
                collegeCode,
                affiliationTypeId,
                courseLevel
            });
        }

        var payment = await _context.TxnDentalPayments
            .FirstOrDefaultAsync(x => x.Id == id && x.FacultyCode == 2);

        if (payment == null)
        {
            TempData["ErrorMessage"] = "Payment record not found.";

            return RedirectToAction(nameof(Index), new
            {
                collegeCode,
                affiliationTypeId,
                courseLevel
            });
        }

        payment.AmountPaid = amountPaid;
        payment.ModifiedDate = DateTime.UtcNow;
        payment.ModifiedBy = User.Identity?.Name ?? "Admin";

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Payment amount updated successfully.";

        return RedirectToAction(nameof(Index), new
        {
            collegeCode = payment.CollegeCode,
            affiliationTypeId = payment.AffiliationTypeId,
            courseLevel = payment.CourseLevel
        });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFee(int id, string? collegeCode, int? affiliationTypeId, string? courseLevel)
    {
        var fee = await _context.TxnDentalFeeStructures.FirstOrDefaultAsync(x => x.Id == id && x.FacultyCode == 2 && x.CollegeCode == collegeCode);

        if (fee == null)
        {
            TempData["ErrorMessage"] = "Fee record not found.";
            return RedirectToAction(nameof(Index), new
            {
                collegeCode,
                affiliationTypeId,
                courseLevel
            });
        }

        _context.TxnDentalFeeStructures.Remove(fee);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Fee record deleted successfully.";

        return RedirectToAction(nameof(Index), new
        {
            collegeCode = fee.CollegeCode,
            affiliationTypeId = fee.AffiliationTypeId,
            courseLevel = fee.CourseLevel
        });
    }

    [HttpGet]
    public async Task<IActionResult> ViewReceipt(int paymentId)
    {
        var payment = await _context.TxnDentalPayments
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.Id == paymentId &&
                e.FacultyCode == 2 &&
                e.IsActive);

        if (payment == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(payment.TransactionReceiptPath))
            return NotFound("Receipt not found.");

        if (!System.IO.File.Exists(payment.TransactionReceiptPath))
            return NotFound("Receipt file not found.");

        var extension = Path.GetExtension(payment.TransactionReceiptPath);

        if (!string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest("The receipt is not a PDF file.");

        return PhysicalFile(
            payment.TransactionReceiptPath,
            "application/pdf",
            enableRangeProcessing: true);
    }


}
