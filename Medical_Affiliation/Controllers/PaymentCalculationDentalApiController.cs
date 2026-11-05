using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Security.Cryptography;
using System.Text;

namespace Medical_Affiliation.Controllers.Api;

/// <summary>
/// Returns saved dental affiliation fee calculations and related payment details.
/// </summary>
[ApiController]
[AllowAnonymous]
[DentalBasicAuth]
[Route("api/dental-payment-calculation")]
[Produces("application/json")]
public sealed class PaymentCalculationDentalApiController : ControllerBase
{
    private const int DentalFacultyCode = 2;
    private const string MaximumCollegeCode = "D037";
    private static readonly string[] LevelOrder = ["UG", "PG"];

    private readonly ApplicationDbContext _context;

    public PaymentCalculationDentalApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets the saved dental fee calculation and payment details for a college.
    /// </summary>
    /// <param name="collegeCode">The college code (required).</param>
    /// <param name="affiliationTypeId">Optional dental affiliation type identifier.</param>
    /// <param name="courseLevel">Optional course level: UG, PG, or SS.</param>
    [HttpGet]
    [ProducesResponseType(typeof(DentalPaymentCalculationApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        [FromQuery] string? collegeCode,
        [FromQuery] int? affiliationTypeId = null,
        [FromQuery] string? courseLevel = null,
        CancellationToken ct = default)
    {
        var (data, error) = await LoadPaymentDataAsync(collegeCode, affiliationTypeId, courseLevel, ct);
        if (error is not null) return error;

        var paymentData = data!;
        var response = new DentalPaymentCalculationApiResponse(
            paymentData.CollegeCode,
            paymentData.CollegeName,
            paymentData.IsGovernmentCollege,
            BuildResults(paymentData));

        return Ok(response);
    }

    /// <summary>
    /// Gets aggregate fee totals and payment status for a college's dental affiliations.
    /// </summary>
    /// <param name="collegeCode">The college code (required).</param>
    /// <param name="affiliationTypeId">Optional dental affiliation type identifier.</param>
    /// <param name="courseLevel">Optional course level: UG, PG, or SS.</param>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DentalPaymentCalculationApiSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] string? collegeCode,
        [FromQuery] int? affiliationTypeId = null,
        [FromQuery] string? courseLevel = null,
        CancellationToken ct = default)
    {
        var (data, error) = await LoadPaymentDataAsync(collegeCode, affiliationTypeId, courseLevel, ct);
        if (error is not null) return error;

        var paymentData = data!;
        var results = BuildResults(paymentData);
        var response = new DentalPaymentCalculationApiSummaryResponse(
            paymentData.CollegeCode,
            paymentData.CollegeName,
            paymentData.IsGovernmentCollege,
            results.Select(result => new DentalPaymentCalculationApiSummaryResult(
                result.AffiliationTypeId,
                result.AffiliationCategory,
                result.CourseLevel,
                result.MatchedCourseCount,
                result.TotalIntakeSeats,
                result.GrandTotal,
                result.Payment?.TransactionId,
                result.Payment?.AmountPaid,
                result.Payment is not null,
                result.Payment?.HasReceipt ?? false)).ToList());

        return Ok(response);
    }

    private async Task<(DentalPaymentData? Data, IActionResult? Error)> LoadPaymentDataAsync(
        string? collegeCode,
        int? affiliationTypeId,
        string? courseLevel,
        CancellationToken ct)
    {
        var code = collegeCode?.Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            return (null, BadRequest(new { message = "collegeCode is required." }));
        }

        if (string.Compare(code, MaximumCollegeCode, StringComparison.OrdinalIgnoreCase) > 0)
        {
            return (null, BadRequest(new { message = $"collegeCode must not be greater than {MaximumCollegeCode}." }));
        }

        if (affiliationTypeId is <= 0)
        {
            return (null, BadRequest(new { message = "affiliationTypeId must be greater than zero." }));
        }

        string? requestedLevel = null;
        if (!string.IsNullOrWhiteSpace(courseLevel))
        {
            requestedLevel = NormalizeCourseLevel(courseLevel);
            if (requestedLevel is not ("UG" or "PG" or "SS"))
            {
                return (null, BadRequest(new { message = "courseLevel must be UG, PG or SS." }));
            }
        }

        var feeRows = await _context.TxnDentalFeeStructures
            .AsNoTracking()
            .Include(row => row.FeeType)
            .Where(row =>
                row.CollegeCode == code &&
                row.FacultyCode == DentalFacultyCode &&
                row.IsActive &&
                (!affiliationTypeId.HasValue || row.AffiliationTypeId == affiliationTypeId.Value))
            .ToListAsync(ct);

        if (requestedLevel is not null)
        {
            feeRows = feeRows
                .Where(row => NormalizeCourseLevel(row.CourseLevel) == requestedLevel)
                .ToList();
        }

        if (feeRows.Count == 0)
        {
            return (null, NotFound(new
            {
                message = $"No active dental fee calculation rows found for college '{code}' with the requested filters."
            }));
        }

        var affiliationIds = feeRows
            .Select(row => row.AffiliationTypeId)
            .Distinct()
            .ToList();

        var payments = await _context.TxnDentalPayments
            .AsNoTracking()
            .Where(payment =>
                payment.CollegeCode == code &&
                payment.FacultyCode == DentalFacultyCode &&
                payment.IsActive &&
                affiliationIds.Contains(payment.AffiliationTypeId))
            .ToListAsync(ct);

        if (requestedLevel is not null)
        {
            payments = payments
                .Where(payment => NormalizeCourseLevel(payment.CourseLevel) == requestedLevel)
                .ToList();
        }

        var affiliationCategories = await _context.MstDentalAffiliationTypes
            .AsNoTracking()
            .Where(type =>
                type.FacultyCode == DentalFacultyCode &&
                affiliationIds.Contains(type.DentalAffiliationTypeId))
            .ToDictionaryAsync(type => type.DentalAffiliationTypeId, type => type.AffiliationCategory, ct);

        var collegeName = await _context.AffiliationCollegeMasters
            .AsNoTracking()
            .Where(college => college.CollegeCode == code)
            .Select(college => college.CollegeName)
            .FirstOrDefaultAsync(ct);

        var institution = await _context.AffInstitutionsDetails
            .AsNoTracking()
            .Where(institution => institution.CollegeCode == code && institution.FacultyCode == DentalFacultyCode.ToString())
            .Select(institution => new
            {
                institution.NameOfInstitution,
                institution.TypeOfInstitution
            })
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrWhiteSpace(collegeName))
        {
            collegeName = institution?.NameOfInstitution;
        }

        return (new DentalPaymentData(
            code,
            collegeName,
            string.Equals(institution?.TypeOfInstitution?.Trim(), "11", StringComparison.OrdinalIgnoreCase),
            feeRows,
            payments,
            affiliationCategories), null);
    }

    private static List<DentalPaymentCalculationApiResult> BuildResults(DentalPaymentData data)
    {
        return data.FeeRows
            .GroupBy(row => new
            {
                row.AffiliationTypeId,
                CourseLevel = NormalizeCourseLevel(row.CourseLevel)
            })
            .OrderBy(group => group.Key.AffiliationTypeId)
            .ThenBy(group => GetLevelOrder(group.Key.CourseLevel))
            .Select(group =>
            {
                var rows = group.ToList();
                var payment = data.Payments
                    .Where(candidate =>
                        candidate.AffiliationTypeId == group.Key.AffiliationTypeId &&
                        NormalizeCourseLevel(candidate.CourseLevel) == group.Key.CourseLevel)
                    .OrderByDescending(candidate => candidate.ModifiedDate ?? candidate.CreatedDate)
                    .FirstOrDefault();

                var feeLines = rows
                    .GroupBy(row => new
                    {
                        row.FeeTypeId,
                        row.FeeType.FeeType,
                        row.FeeType.DisplayOrder
                    })
                    .OrderBy(feeGroup => feeGroup.Key.DisplayOrder)
                    .ThenBy(feeGroup => feeGroup.Key.FeeTypeId)
                    .Select(feeGroup => new DentalPaymentCalculationApiFeeLine(
                        feeGroup.Key.FeeTypeId,
                        feeGroup.Key.FeeType,
                        feeGroup.Sum(row => row.CalculatedAmount),
                        feeGroup
                            .OrderBy(row => row.CourseName)
                            .ThenBy(row => row.Id)
                            .Select(row => new DentalPaymentCalculationApiFeeItem(
                                row.Id,
                                row.CourseCode,
                                row.CourseName,
                                row.CourseLevel,
                                row.AmountToBePaid,
                                row.AcademicIntake2026,
                                row.CalculationType,
                                row.CalculatedAmount))
                            .ToList()))
                    .ToList();

                var courses = rows
                    .Where(row => row.CourseCode.HasValue)
                    .GroupBy(row => row.CourseCode!.Value)
                    .ToList();
                var totalIntake = courses.Sum(course =>
                    course.Max(row => row.AcademicIntake2026 ?? 0));

                var affiliationCategory = data.AffiliationCategories.TryGetValue(
                    group.Key.AffiliationTypeId,
                    out var category)
                    ? category
                    : $"Affiliation type {group.Key.AffiliationTypeId}";

                var paymentResponse = payment is null
                    ? null
                    : new DentalPaymentCalculationApiPayment(
                        payment.Id,
                        payment.TransactionId,
                        payment.AmountPaid,
                        !string.IsNullOrWhiteSpace(payment.TransactionReceiptPath));

                return new DentalPaymentCalculationApiResult(
                    group.Key.AffiliationTypeId,
                    affiliationCategory,
                    group.Key.CourseLevel,
                    courses.Count,
                    totalIntake,
                    feeLines.Sum(line => line.TotalAmount),
                    paymentResponse,
                    feeLines);
            })
            .ToList();
    }

    private static int GetLevelOrder(string level)
    {
        var index = Array.IndexOf(LevelOrder, level);
        return index < 0 ? LevelOrder.Length : index;
    }

    private static string NormalizeCourseLevel(string? courseLevel)
    {
        var normalized = string.IsNullOrWhiteSpace(courseLevel)
            ? string.Empty
            : new string(courseLevel.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        if (normalized.Contains("UG", StringComparison.Ordinal) ||
            normalized.Contains("UNDERGRADUATE", StringComparison.Ordinal))
        {
            return "UG";
        }

        if (normalized.Contains("SS", StringComparison.Ordinal) ||
            normalized.Contains("SUPERSPECIALTY", StringComparison.Ordinal) ||
            normalized.Contains("SUPERSPECIALITY", StringComparison.Ordinal))
        {
            return "SS";
        }

        if (normalized.Contains("PG", StringComparison.Ordinal) ||
            normalized.Contains("POSTGRADUATE", StringComparison.Ordinal) ||
            normalized.Contains("PGBROAD", StringComparison.Ordinal))
        {
            return "PG";
        }

        return normalized;
    }

    private sealed record DentalPaymentData(
        string CollegeCode,
        string? CollegeName,
        bool IsGovernmentCollege,
        List<TxnDentalFeeStructure> FeeRows,
        List<TxnDentalPayment> Payments,
        Dictionary<int, string> AffiliationCategories);
}

/// <summary>Applies the dental payment API's independently configured Basic Auth credentials.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class DentalBasicAuthAttribute : TypeFilterAttribute
{
    public DentalBasicAuthAttribute() : base(typeof(DentalBasicAuthFilter))
    {
    }
}

public sealed class DentalBasicAuthFilter : IAuthorizationFilter
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DentalBasicAuthFilter> _logger;

    public DentalBasicAuthFilter(
        IConfiguration configuration,
        ILogger<DentalBasicAuthFilter> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var username = "DENTAL";
        var password = "dental@007";
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogError("Dental payment API Basic Auth credentials are not configured.");
            context.Result = new ObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Dental payment API authentication is not configured."
            })
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable
            };
            return;
        }

        var header = context.HttpContext.Request.Headers["Authorization"].ToString();
        if (header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header[6..].Trim()));
                var separator = decoded.IndexOf(':');
                if (separator > 0)
                {
                    var suppliedUsername = decoded[..separator];
                    var suppliedPassword = decoded[(separator + 1)..];
                    var usernameMatches = CryptographicOperations.FixedTimeEquals(
                        SHA256.HashData(Encoding.UTF8.GetBytes(suppliedUsername)),
                        SHA256.HashData(Encoding.UTF8.GetBytes(username)));
                    var passwordMatches = CryptographicOperations.FixedTimeEquals(
                        SHA256.HashData(Encoding.UTF8.GetBytes(suppliedPassword)),
                        SHA256.HashData(Encoding.UTF8.GetBytes(password)));

                    if (usernameMatches && passwordMatches) return;
                }
            }
            catch (FormatException)
            {
                // Invalid Base64 falls through to the unauthorized response.
            }
        }

        context.Result = new UnauthorizedResult();
    }
}

public sealed class DentalBasicAuthSwaggerFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var requiresDentalBasicAuth =
            (context.MethodInfo.DeclaringType?.IsDefined(typeof(DentalBasicAuthAttribute), true) ?? false)
            || context.MethodInfo.IsDefined(typeof(DentalBasicAuthAttribute), true);

        if (!requiresDentalBasicAuth) return;

        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Basic"
                    }
                }] = new List<string>()
            }
        ];
    }
}

/// <summary>Detailed saved dental fee calculation and payment response.</summary>
public sealed record DentalPaymentCalculationApiResponse(
    string CollegeCode,
    string? CollegeName,
    bool IsGovernmentCollege,
    IReadOnlyList<DentalPaymentCalculationApiResult> Results);

/// <summary>A saved dental fee calculation for one affiliation type and course level.</summary>
public sealed record DentalPaymentCalculationApiResult(
    int AffiliationTypeId,
    string AffiliationCategory,
    string CourseLevel,
    int MatchedCourseCount,
    int TotalIntakeSeats,
    decimal GrandTotal,
    DentalPaymentCalculationApiPayment? Payment,
    IReadOnlyList<DentalPaymentCalculationApiFeeLine> FeeLines);

/// <summary>Payment transaction details, excluding the private receipt file path.</summary>
public sealed record DentalPaymentCalculationApiPayment(
    int PaymentId,
    string TransactionId,
    decimal AmountPaid,
    bool HasReceipt);

/// <summary>Saved fee totals for a dental fee type.</summary>
public sealed record DentalPaymentCalculationApiFeeLine(
    int FeeTypeId,
    string FeeType,
    decimal TotalAmount,
    IReadOnlyList<DentalPaymentCalculationApiFeeItem> Items);

/// <summary>A saved fee amount for a course.</summary>
public sealed record DentalPaymentCalculationApiFeeItem(
    int Id,
    int? CourseCode,
    string? CourseName,
    string? CourseLevel,
    decimal AmountToBePaid,
    int? AcademicIntake2026,
    string? CalculationType,
    decimal CalculatedAmount);

/// <summary>Aggregate totals and payment status for dental fee calculations.</summary>
public sealed record DentalPaymentCalculationApiSummaryResponse(
    string CollegeCode,
    string? CollegeName,
    bool IsGovernmentCollege,
    IReadOnlyList<DentalPaymentCalculationApiSummaryResult> Results);

/// <summary>A compact fee total and payment status for one affiliation and course level.</summary>
public sealed record DentalPaymentCalculationApiSummaryResult(
    int AffiliationTypeId,
    string AffiliationCategory,
    string CourseLevel,
    int MatchedCourseCount,
    int TotalIntakeSeats,
    decimal GrandTotal,
    string? TransactionId,
    decimal? AmountPaid,
    bool HasPayment,
    bool HasReceipt);
