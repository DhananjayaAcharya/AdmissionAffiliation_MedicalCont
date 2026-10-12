
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

        // 1. Load colleges from institution details.
        // Display CollegeCode until the actual college-name
        // property is confirmed.

        var collegeList = await (
                from inst in _context.AffInstitutionsDetails.AsNoTracking()
                join college in _context.AffiliationCollegeMasters.AsNoTracking()
                    on inst.CollegeCode equals college.CollegeCode
                where inst.CollegeCode != null && inst.FacultyCode == "2"
                select new SelectListItem
                {
                    Value = inst.CollegeCode,
                    Text = college.CollegeName,
                })
                .Distinct()
                .OrderBy(e => e.Text)
                .ToListAsync();

        model.Colleges = collegeList;

        // 2. Load dental affiliation types.
        var affiliationTypes = await _context.MstDentalAffiliationTypes
            .AsNoTracking()
            .Where(e => e.FacultyCode == FacultyCode && e.IsActive)
            .OrderBy(e => e.AffiliationCategory)
            .ToListAsync();

        model.AffiliationTypes = affiliationTypes
            .Select(e => new SelectListItem
            {
                Value = e.DentalAffiliationTypeId.ToString(),
                Text = $"{e.AffiliationCategory} - {e.CourseLevelGroup}",
                Selected = e.DentalAffiliationTypeId == affiliationTypeId
            })
            .ToList();

        // 3. Course-level filter.
        model.CourseLevels = new List<SelectListItem>
        {
            new() { Value = "UG", Text = "Undergraduate (UG)" },
            new() { Value = "PG", Text = "Postgraduate (PG)" }
        };

        // Do not load financial records until a college
        // and affiliation type have been selected.
        if (string.IsNullOrWhiteSpace(collegeCode) ||
            !affiliationTypeId.HasValue)
        {
            return View(model);
        }

        // 4. Verify the selected college exists.
        var institution = await _context.AffInstitutionsDetails
            .AsNoTracking()
            .Where(e=> e.FacultyCode == FacultyCode.ToString())
            .FirstOrDefaultAsync(e => e.CollegeCode == collegeCode);

        if (institution == null)
        {
            return NotFound("College not found.");
        }

        var isGovernment = string.Equals(
            institution.TypeOfInstitution?.Trim(),
            "11",
            StringComparison.OrdinalIgnoreCase);

        model.TypeOfInstitution = institution.TypeOfInstitution;
        model.IsGovernment = isGovernment;

        // 5. Verify affiliation type.
        var affiliationType = affiliationTypes.FirstOrDefault(
            e => e.DentalAffiliationTypeId == affiliationTypeId.Value);

        if (affiliationType == null)
        {
            ModelState.AddModelError( nameof(model.AffiliationTypeId), "Invalid or inactive dental affiliation type.");

            return View(model);
        }

        // Prevent mismatched UG/PG selections.
        if (!string.IsNullOrWhiteSpace(courseLevel) &&
            !string.Equals(
                courseLevel,
                affiliationType.CourseLevelGroup?.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError( nameof(model.CourseLevel), "Course level does not match the selected affiliation type.");

            return View(model);
        }

        model.CourseLevel = affiliationType.CourseLevelGroup;

        // 6. Load fee types using the existing eligibility rules.
        var feeTypes = await _context.MstDentalFeeTypes
            .AsNoTracking()
            .Where(e =>
                e.FacultyCode == FacultyCode &&
                e.AffiliationTypeId == affiliationTypeId.Value &&
                e.IsActive &&
                (
                    (
                        isGovernment &&
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
                        !isGovernment &&
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

        // 7. Load existing fee transaction rows.
        // Include inactive rows so administrators can inspect
        // historical records too.
        var feeQuery = _context.TxnDentalFeeStructures
            .AsNoTracking()
            .Where(e =>
                e.CollegeCode == collegeCode &&
                e.FacultyCode == FacultyCode &&
                e.AffiliationTypeId == affiliationTypeId.Value);

        if (!string.IsNullOrWhiteSpace(courseLevel))
        {
            feeQuery = feeQuery.Where(e => e.CourseLevel == courseLevel);
        }

        var savedFees = await feeQuery
            .OrderBy(e => e.FeeTypeId)
            .ThenBy(e => e.CourseName)
            .ThenBy(e => e.Id)
            .ToListAsync();

        // Load all master fee-type names so historical or
        // currently inapplicable records remain identifiable.
        var allFeeTypes = await _context.MstDentalFeeTypes
            .AsNoTracking()
            .Where(e =>
                e.FacultyCode == FacultyCode &&
                e.AffiliationTypeId == affiliationTypeId.Value)
            .ToDictionaryAsync(e => e.Id);

        model.FeeItems = savedFees.Select(fee =>
        {
            allFeeTypes.TryGetValue(fee.FeeTypeId, out var masterFee);

            var isApplicable = applicableFeeTypes.ContainsKey( fee.FeeTypeId);

            return new AdminDentalFeeItemViewModel
            {
                Id = fee.Id,
                FeeTypeId = fee.FeeTypeId,
                FeeTypeName = masterFee?.FeeType ?? $"Fee Type #{fee.FeeTypeId}",
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

        // 8. Load payment records for this college and affiliation.
        var paymentQuery = _context.TxnDentalPayments
            .AsNoTracking()
            .Where(e =>
                e.CollegeCode == collegeCode &&
                e.FacultyCode == FacultyCode &&
                e.AffiliationTypeId == affiliationTypeId.Value);

        if (!string.IsNullOrWhiteSpace(courseLevel))
        {
            paymentQuery = paymentQuery.Where(e =>
                e.CourseLevel == courseLevel);
        }

        var payments = await paymentQuery
            .OrderByDescending(e => e.CreatedDate)
            .ThenByDescending(e => e.Id)
            .ToListAsync();

        model.Payments = payments.Select(payment =>
            new AdminDentalPaymentViewModel
            {
                Id = payment.Id,
                CollegeCode = payment.CollegeCode,
                FacultyCode = payment.FacultyCode,
                AffiliationTypeId = payment.AffiliationTypeId,
                CourseLevel = payment.CourseLevel,
                TransactionId = payment.TransactionId,
                TransactionReceiptPath =
                    payment.TransactionReceiptPath,
                AmountPaid = payment.AmountPaid,
                IsActive = payment.IsActive,
                CreatedDate = payment.CreatedDate,
                ModifiedDate = payment.ModifiedDate,
                CreatedBy = payment.CreatedBy,
                ModifiedBy = payment.ModifiedBy
            }).ToList();

        // 9. Calculate totals from the currently applicable,
        // active fee transaction rows only.
        model.TotalApplicableFee = model.FeeItems
            .Where(e => e.IsApplicable && e.IsActive)
            .Sum(e => e.CalculatedAmount);

        // Sum active payments only. This is a recorded-payment
        // total, not a guarantee that every record is unique.
        model.TotalAmountPaid = model.Payments
            .Where(e => e.IsActive)
            .Sum(e => e.AmountPaid);

        return View(model);
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

        if (!fee.IsActive)
        {
            TempData["ErrorMessage"] = "This fee record is already inactive.";
            return RedirectToAction(nameof(Index), new
            {
                collegeCode,
                affiliationTypeId,
                courseLevel
            });
        }

        fee.IsActive = false;
        fee.ModifiedDate = DateTime.UtcNow;
        fee.ModifiedBy = User.Identity?.Name ?? "Admin";

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Fee record deleted successfully.";

        return RedirectToAction(nameof(Index), new
        {
            collegeCode = fee.CollegeCode,
            affiliationTypeId = fee.AffiliationTypeId,
            courseLevel = fee.CourseLevel
        });
    }

}
