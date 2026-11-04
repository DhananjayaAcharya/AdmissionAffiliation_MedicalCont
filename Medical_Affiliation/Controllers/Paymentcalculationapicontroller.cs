using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;

namespace Medical_Affiliation.Controllers.Api
{
    /// <summary>
    /// Read-only API that returns the "Payment Calculation" (matched courses)
    /// and the "Fee Breakdown" for a college, using the same rules as
    /// PaymentCalculationController (RGUHS fee circular 2025-26, government flat fee,
    /// late fine and UG payment window).
    ///
    /// Every endpoint is protected with HTTP Basic authentication (see BasicAuthAttribute below).
    ///
    /// Endpoints:
    ///   GET api/payment-calculation                      -> one application type (default Continuation)
    ///   GET api/payment-calculation/by-application-type  -> full breakdown for ALL application types, grouped
    ///   GET api/payment-calculation/summary              -> compact totals for ALL application types, grouped
    ///
    /// This controller is self-contained: it does NOT use session, claims or any code from
    /// PaymentCalculationController, so the existing controller is untouched.
    /// If the fee rules change there, mirror the change here.
    /// </summary>
    [ApiController]
    [AllowAnonymous]   // bypasses the app's cookie/login policy; BasicAuth below still enforces credentials
    [BasicAuth]        // Basic auth required on every endpoint in this controller
    [Route("api/payment-calculation")]
    [Produces("application/json")]
    public class PaymentCalculationApiController : ControllerBase
    {
        // ---- keep identical to PaymentCalculationController ----------------------------
        private const decimal GovernmentCollegeFee = 3010m;
        private const string AdditionalNote = "Requested via Additional Courses";

        // NOTE: last dates (late fine / UG payment window) are intentionally NOT considered in this API.
        // It only displays the fee breakdown.

        private static readonly string[] LevelOrder = { "UG", "PG", "SS" };

        // Order in which application types are returned by the grouped endpoints.
        private static readonly string[] GroupOrder = { "CONTINUATION", "ENHANCEMENT", "ADDITIONAL", "FRESH" };

        private readonly ApplicationDbContext _context;

        public PaymentCalculationApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Payment Calculation + Fee Breakdown for a college.
        /// </summary>
        /// <param name="collegeCode">College code, e.g. M001 (required).</param>
        /// <param name="applicationType">
        /// Optional. Continuation (default) | Enhancement (Increase in Intake) | Additional | Fresh.
        /// </param>
        /// <param name="courseLevel">
        /// Optional. UG | PG | SS. When omitted, every level the college has is returned.
        /// </param>
        [HttpGet]
        [ProducesResponseType(typeof(PaymentApiResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(
            [FromQuery] string collegeCode,
            [FromQuery] string? applicationType = null,
            [FromQuery] string? courseLevel = null,
            CancellationToken ct = default)
        {
            var code = collegeCode?.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(new { message = "collegeCode is required." });
            }

            var group = string.IsNullOrWhiteSpace(applicationType)
                ? "CONTINUATION"
                : GetAffiliationGroup(applicationType);
            if (group == null)
            {
                return BadRequest(new
                {
                    message = "applicationType must be one of: Continuation, Enhancement (Increase in Intake), Additional, Fresh."
                });
            }

            string? requestedLevel = null;
            if (!string.IsNullOrWhiteSpace(courseLevel))
            {
                requestedLevel = NormalizeCourseLevel(courseLevel);
                if (requestedLevel is not ("UG" or "PG" or "SS"))
                {
                    return BadRequest(new { message = "courseLevel must be UG, PG or SS." });
                }
            }

            // Same source as the existing page: dbo.Mst_MedicalCollegeCourseIntake.
            var rows = await _context.MstMedicalCollegeCourseIntakes
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM dbo.Mst_MedicalCollegeCourseIntake
                    WHERE UPPER(LTRIM(RTRIM(coll_code))) = UPPER({code})")
                .AsNoTracking()
                .ToListAsync(ct);

            if (rows.Count == 0)
            {
                return NotFound(new { message = $"No course intake rows found for college code '{code}'." });
            }

            var canonicalCode = rows[0].CollCode?.Trim() ?? code;

            var collegeName = await _context.AffiliationCollegeMasters
                .AsNoTracking()
                .Where(x => x.CollegeCode == canonicalCode)
                .Select(x => x.CollegeName)
                .FirstOrDefaultAsync(ct);
            if (string.IsNullOrWhiteSpace(collegeName))
            {
                collegeName = rows.Select(r => r.Collegename).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));
            }

            // Government colleges pay a flat fee (PVT_GOVT starts with "GOV"; blank = private).
            var pvtGovt = rows.Select(r => r.PvtGovt).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
            var isGovernment = NormalizeLabel(pvtGovt).StartsWith("GOV", StringComparison.Ordinal);

            // Course master: course name / level fallback (safe if a CourseCode exists in several faculties).
            var masterCourses = await _context.MstCourses.AsNoTracking().ToListAsync(ct);
            var masterByCode = masterCourses
                .GroupBy(c => c.CourseCode.ToString(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            (string? Name, string? Level) Master(string? courseCode, int? faculty)
            {
                if (string.IsNullOrWhiteSpace(courseCode) || !masterByCode.TryGetValue(courseCode, out var list) || list.Count == 0)
                {
                    return (null, null);
                }

                var m = list.FirstOrDefault(c => c.FacultyCode == faculty) ?? list[0];
                return (m.CourseName, m.CourseLevel);
            }

            // Level comes from ug_pg; if blank, fall back to the course master's level.
            string RowLevel(MstMedicalCollegeCourseIntake r)
            {
                var level = NormalizeCourseLevel(r.UgPg);
                if (!string.IsNullOrEmpty(level)) return level;

                var (_, masterLevel) = Master(r.CourseCode?.ToString(), r.Facultycode);
                return NormalizeCourseLevel(masterLevel);
            }

            var levels = requestedLevel != null
                ? new List<string> { requestedLevel }
                : rows.Select(RowLevel)
                      .Where(l => l is "UG" or "PG" or "SS")
                      .Distinct()
                      .OrderBy(l => Array.IndexOf(LevelOrder, l))
                      .ToList();

            if (levels.Count == 0)
            {
                return NotFound(new { message = $"College '{canonicalCode}' has no UG / PG / SS course rows." });
            }

            var ctx = new CalcContext
            {
                CollegeCode = canonicalCode,
                IsGovernment = isGovernment,
                Group = group,
                Rows = rows,
                RowLevel = RowLevel,
                Master = Master
            };

            var response = new PaymentApiResponse
            {
                CollegeCode = canonicalCode,
                CollegeName = collegeName,
                IsGovernmentCollege = isGovernment,
                ApplicationType = DisplayName(group)
            };

            foreach (var level in levels)
            {
                response.Results.Add(await BuildLevelResultAsync(ctx, level, ct));
            }

            if (requestedLevel != null && response.Results[0].PaymentCalculation.MatchedCourseCount == 0)
            {
                return NotFound(new { message = $"College '{canonicalCode}' has no {requestedLevel} courses." });
            }

            return Ok(response);
        }

        // =====================================================================
        //  NEW 1: full Payment Calculation + Fee Breakdown, grouped by application type
        // =====================================================================
        /// <summary>
        /// Full payment breakdown for a college across ALL application types
        /// (Continuation, Enhancement, Additional, Fresh), grouped by application type.
        /// Each group contains the matched courses and fee breakdown for every level (UG / PG / SS).
        /// </summary>
        /// <param name="collegeCode">College code, e.g. M001 (required).</param>
        /// <param name="courseLevel">Optional. UG | PG | SS. When omitted, every level the college has is returned.</param>
        [HttpGet("by-application-type")]
        [ProducesResponseType(typeof(PaymentApiAllResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByApplicationType(
            [FromQuery] string collegeCode,
            [FromQuery] string? courseLevel = null,
            CancellationToken ct = default)
        {
            var (data, error) = await PrepareAllGroupsAsync(collegeCode, courseLevel, ct);
            if (error != null) return error;

            var response = new PaymentApiAllResponse
            {
                CollegeCode = data!.CollegeCode,
                CollegeName = data.CollegeName,
                IsGovernmentCollege = data.IsGovernment
            };

            foreach (var group in GroupOrder)
            {
                var results = await BuildGroupResultsAsync(data, group, data.Levels, ct);

                response.ApplicationTypes.Add(new PaymentApiApplicationTypeResult
                {
                    ApplicationType = DisplayName(group),
                    ApplicationTypeCode = group,
                    Results = results,
                    TotalPayable = results.Sum(r => r.FeeBreakdown.GrandTotal)
                });
            }

            return Ok(response);
        }

        // =====================================================================
        //  NEW 2: compact totals, grouped by application type
        // =====================================================================
        /// <summary>
        /// Compact summary for a college across ALL application types, grouped by application type:
        /// per level it returns only the counts and the grand total (no course list / fee lines).
        /// </summary>
        /// <param name="collegeCode">College code, e.g. M001 (required).</param>
        /// <param name="courseLevel">Optional. UG | PG | SS. When omitted, every level the college has is returned.</param>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(PaymentApiSummaryResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSummary(
            [FromQuery] string collegeCode,
            [FromQuery] string? courseLevel = null,
            CancellationToken ct = default)
        {
            var (data, error) = await PrepareAllGroupsAsync(collegeCode, courseLevel, ct);
            if (error != null) return error;

            var response = new PaymentApiSummaryResponse
            {
                CollegeCode = data!.CollegeCode,
                CollegeName = data.CollegeName,
                IsGovernmentCollege = data.IsGovernment
            };

            foreach (var group in GroupOrder)
            {
                var results = await BuildGroupResultsAsync(data, group, data.Levels, ct);

                var summaryGroup = new PaymentApiSummaryGroup
                {
                    ApplicationType = DisplayName(group),
                    ApplicationTypeCode = group
                };

                foreach (var r in results)
                {
                    summaryGroup.Levels.Add(new PaymentApiSummaryLevel
                    {
                        CourseLevel = r.CourseLevel,
                        MatchedCourseCount = r.PaymentCalculation.MatchedCourseCount,
                        CoursesUsedForFee = r.PaymentCalculation.CoursesUsedForFee,
                        SeatsUsedForFee = r.PaymentCalculation.SeatsUsedForFee,
                        GrandTotal = r.FeeBreakdown.GrandTotal,
                        Message = r.Message
                    });
                }

                summaryGroup.TotalPayable = summaryGroup.Levels.Sum(l => l.GrandTotal);

                response.ApplicationTypes.Add(summaryGroup);
            }

            return Ok(response);
        }

        // =====================================================================
        //  Shared loading / validation for the grouped endpoints
        // =====================================================================
        private async Task<(CollegeData? Data, IActionResult? Error)> PrepareAllGroupsAsync(
            string? collegeCode, string? courseLevel, CancellationToken ct)
        {
            var code = collegeCode?.Trim();
            if (string.IsNullOrWhiteSpace(code))
            {
                return (null, BadRequest(new { message = "collegeCode is required." }));
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

            var data = await LoadCollegeAsync(code, ct);
            if (data == null)
            {
                return (null, NotFound(new { message = $"No course intake rows found for college code '{code}'." }));
            }

            var availableLevels = data.Rows
                .Select(data.RowLevel)
                .Where(l => l is "UG" or "PG" or "SS")
                .Distinct()
                .OrderBy(l => Array.IndexOf(LevelOrder, l))
                .ToList();

            if (availableLevels.Count == 0)
            {
                return (null, NotFound(new { message = $"College '{data.CollegeCode}' has no UG / PG / SS course rows." }));
            }

            if (requestedLevel != null)
            {
                if (!availableLevels.Contains(requestedLevel))
                {
                    return (null, NotFound(new { message = $"College '{data.CollegeCode}' has no {requestedLevel} courses." }));
                }

                data.Levels = new List<string> { requestedLevel };
            }
            else
            {
                data.Levels = availableLevels;
            }

            return (data, null);
        }

        private async Task<CollegeData?> LoadCollegeAsync(string code, CancellationToken ct)
        {
            // Same source as the existing page: dbo.Mst_MedicalCollegeCourseIntake.
            var rows = await _context.MstMedicalCollegeCourseIntakes
                .FromSqlInterpolated($@"
                    SELECT *
                    FROM dbo.Mst_MedicalCollegeCourseIntake
                    WHERE UPPER(LTRIM(RTRIM(coll_code))) = UPPER({code})")
                .AsNoTracking()
                .ToListAsync(ct);

            if (rows.Count == 0) return null;

            var canonicalCode = rows[0].CollCode?.Trim() ?? code;

            var collegeName = await _context.AffiliationCollegeMasters
                .AsNoTracking()
                .Where(x => x.CollegeCode == canonicalCode)
                .Select(x => x.CollegeName)
                .FirstOrDefaultAsync(ct);
            if (string.IsNullOrWhiteSpace(collegeName))
            {
                collegeName = rows.Select(r => r.Collegename).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));
            }

            var pvtGovt = rows.Select(r => r.PvtGovt).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
            var isGovernment = NormalizeLabel(pvtGovt).StartsWith("GOV", StringComparison.Ordinal);

            var masterCourses = await _context.MstCourses.AsNoTracking().ToListAsync(ct);
            var masterByCode = masterCourses
                .GroupBy(c => c.CourseCode.ToString(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            (string? Name, string? Level) Master(string? courseCode, int? faculty)
            {
                if (string.IsNullOrWhiteSpace(courseCode) || !masterByCode.TryGetValue(courseCode, out var list) || list.Count == 0)
                {
                    return (null, null);
                }

                var m = list.FirstOrDefault(c => c.FacultyCode == faculty) ?? list[0];
                return (m.CourseName, m.CourseLevel);
            }

            string RowLevel(MstMedicalCollegeCourseIntake r)
            {
                var level = NormalizeCourseLevel(r.UgPg);
                if (!string.IsNullOrEmpty(level)) return level;

                var (_, masterLevel) = Master(r.CourseCode?.ToString(), r.Facultycode);
                return NormalizeCourseLevel(masterLevel);
            }

            return new CollegeData
            {
                CollegeCode = canonicalCode,
                CollegeName = collegeName,
                IsGovernment = isGovernment,
                Rows = rows,
                RowLevel = RowLevel,
                Master = Master
            };
        }

        /// <summary>
        /// Runs the per-level calculation for one application type. Calls are sequential
        /// because a DbContext instance must not be used concurrently.
        /// </summary>
        private async Task<List<PaymentApiLevelResult>> BuildGroupResultsAsync(
            CollegeData data, string group, List<string> levels, CancellationToken ct)
        {
            var ctx = new CalcContext
            {
                CollegeCode = data.CollegeCode,
                IsGovernment = data.IsGovernment,
                Group = group,
                Rows = data.Rows,
                RowLevel = data.RowLevel,
                Master = data.Master
            };

            var results = new List<PaymentApiLevelResult>();
            foreach (var level in levels)
            {
                results.Add(await BuildLevelResultAsync(ctx, level, ct));
            }

            return results;
        }

        // =====================================================================
        //  Per-level calculation
        // =====================================================================
        private async Task<PaymentApiLevelResult> BuildLevelResultAsync(CalcContext ctx, string level, CancellationToken ct)
        {
            var levelRows = ctx.Rows
                .Where(r => ctx.RowLevel(r) == level && !string.IsNullOrWhiteSpace(r.Course))
                .OrderBy(r => r.Course)
                .ToList();

            var levelTotalSeats = ctx.Rows
                .Where(r => ctx.RowLevel(r) == level)
                .Sum(r => r.Intake2627 ?? 0);

            // ---- Payment Calculation: matched courses --------------------------------
            var courses = new List<PaymentApiCourse>();
            foreach (var r in levelRows)
            {
                var codeStr = r.CourseCode?.ToString();
                var (masterName, _) = ctx.Master(codeStr, r.Facultycode);
                var intakeName = r.Course?.Trim();

                var name = !string.IsNullOrWhiteSpace(masterName) ? masterName!
                    : !string.IsNullOrWhiteSpace(intakeName) ? intakeName!
                    : codeStr ?? string.Empty;

                courses.Add(new PaymentApiCourse
                {
                    CourseName = name,
                    CourseCode = codeStr,
                    CourseLevel = string.IsNullOrWhiteSpace(r.UgPg) ? level : r.UgPg.Trim(),
                    Intake2627 = r.Intake2627,
                    IncreasedIntake = r.IncreasedIntake,
                    AcademicYear = r.AcademicYear
                });
            }

            // "Additional Courses": merge the courses the college actually requested.
            if (ctx.Group == "ADDITIONAL")
            {
                // Project only the columns we need: selecting the whole entity also selects
                // "CourseName", which does not exist in the AddCoursedetails table (SQL error 207).
                var requested = (await _context.AddCoursedetails
                        .AsNoTracking()
                        .Where(x => x.CollegeCode == ctx.CollegeCode && x.AddCourseRequested)
                        .Select(x => new { x.CourseCode, x.CourseLevel, x.RequestedCourseIntake })
                        .ToListAsync(ct))
                    .Where(x => string.IsNullOrWhiteSpace(x.CourseLevel) || NormalizeCourseLevel(x.CourseLevel) == level)
                    .ToList();

                foreach (var req in requested)
                {
                    if (string.IsNullOrWhiteSpace(req.CourseCode)) continue;

                    var (masterName, masterLevel) = ctx.Master(req.CourseCode, null);
                    var existing = courses.FirstOrDefault(c =>
                        string.Equals(c.CourseCode, req.CourseCode, StringComparison.OrdinalIgnoreCase));

                    if (existing != null)
                    {
                        existing.Intake2627 = req.RequestedCourseIntake ?? existing.Intake2627 ?? 0;
                        if (!string.IsNullOrWhiteSpace(masterName)) existing.CourseName = masterName!;
                        existing.Note = AdditionalNote;
                        continue;
                    }

                    courses.Add(new PaymentApiCourse
                    {
                        CourseName = !string.IsNullOrWhiteSpace(masterName) ? masterName! : req.CourseCode,
                        CourseCode = req.CourseCode,
                        CourseLevel = masterLevel ?? req.CourseLevel ?? level,
                        Intake2627 = req.RequestedCourseIntake ?? 0,
                        Note = AdditionalNote
                    });
                }
            }

            // ---- Seat / course counts that drive the fee ------------------------------
            int courseCount;
            int seatCount;

            if (ctx.Group == "ADDITIONAL")
            {
                var requestedCount = courses.Count(c => c.Note == AdditionalNote);
                courseCount = requestedCount > 0 ? requestedCount : courses.Count;
                seatCount = 0;
            }
            else if (ctx.Group == "ENHANCEMENT")
            {
                courseCount = courses.Count;
                seatCount = level == "UG"
                    ? courses.Sum(c => (c.Intake2627 ?? 0) + ParseInt(c.IncreasedIntake))   // UG: intake + increase
                    : courses.Sum(c => ParseInt(c.IncreasedIntake));                       // PG / SS: increase only
            }
            else
            {
                // Continuation / Fresh: sanctioned intake only.
                courseCount = courses.Count;
                seatCount = courses.Sum(c => c.Intake2627 ?? 0);
            }

            var calculation = new PaymentApiCalculation
            {
                MatchedCourseCount = courses.Count,
                TotalIntakeSeats = courses.Sum(c => c.Intake2627 ?? 0),
                LevelTotalSeats = levelTotalSeats,
                SeatsUsedForFee = seatCount,
                CoursesUsedForFee = courseCount,
                MatchedCourses = courses
            };

            var result = new PaymentApiLevelResult
            {
                CourseLevel = level,
                PaymentCalculation = calculation
            };

            // ---- Fee Breakdown -------------------------------------------------------
            var fees = ctx.IsGovernment
                ? new List<PaymentApiFeeLine> { FixedFee("Affiliation fee (Government college)", GovernmentCollegeFee) }
                : BuildCircularFeeLines(ctx.Group, level, courseCount, seatCount);

            // Only Application fee + Course identification fee matched: no schedule for this combination.
            if (!ctx.IsGovernment && fees.Count == 2)
            {
                result.Message = $"No {level}-level fee schedule is defined in the circular for '{DisplayName(ctx.Group)}'. Only the application and course identification fees are shown.";
            }

            result.FeeBreakdown = new PaymentApiFeeBreakdown
            {
                FeeLines = fees,
                GrandTotal = fees.Sum(f => f.LineAmount)
            };
            return result;
        }

        // =====================================================================
        //  Fee schedule (RGUHS/MEDICAL/AFF-FEE/2025-26) - private colleges
        // =====================================================================
        private static List<PaymentApiFeeLine> BuildCircularFeeLines(string? group, string level, int courseCount, int seatCount)
        {
            var lines = new List<PaymentApiFeeLine>
            {
                FixedFee("Application fee", 3000m),
                FixedFee("Course identification fee", 10m)
            };

            switch (group)
            {
                case "CONTINUATION":
                    switch (level)
                    {
                        case "UG":
                            lines.Add(FixedFee("Annual fee", 200000m));
                            lines.Add(FixedFee("Renewal of affiliation fee", 500000m));
                            lines.Add(FixedFee($"Administration and Service charges ({DescribeSeatBand(seatCount)})", GetUgSeatBandAmount(seatCount)));
                            lines.Add(FixedFee("HELINET fee", 100000m));
                            break;
                        case "PG":
                            lines.Add(PerSeatFee("Renewal of PG broad specialty", 4500m, seatCount));
                            lines.Add(FixedFee("HELINET fee", 30000m));
                            break;
                        case "SS":
                            lines.Add(PerSeatFee("Renewal of PG Super Specialty", 7500m, seatCount));
                            lines.Add(FixedFee("HELINET fee", 30000m));
                            break;
                    }
                    break;

                case "ADDITIONAL":
                    switch (level)
                    {
                        case "PG":
                            lines.Add(PerCourseFee("PG fresh course affiliation fee", 400000m, courseCount));
                            break;
                        case "SS":
                            lines.Add(PerCourseFee("Fresh PG Super Specialty course", 400000m, courseCount));
                            break;
                            // UG: the circular has no "additional course" fee head.
                    }
                    break;

                case "ENHANCEMENT":
                    switch (level)
                    {
                        case "UG":
                            lines.Add(FixedFee($"Administration and Service charges ({DescribeSeatBand(seatCount)})", GetUgSeatBandAmount(seatCount)));
                            break;
                        case "PG":
                            lines.Add(PerSeatFee("Increase in intake fee", 50000m, seatCount));
                            break;
                        case "SS":
                            lines.Add(PerSeatFee("PG Super Specialty course - increase in intake fee", 50000m, seatCount));
                            break;
                    }
                    break;
            }

            return lines;
        }

        private static decimal GetUgSeatBandAmount(int seats)
        {
            if (seats <= 100) return 300000m;
            if (seats <= 150) return 450000m;
            if (seats <= 200) return 600000m;
            return 750000m;
        }

        private static string DescribeSeatBand(int seats)
        {
            if (seats <= 100) return "1 to 100 seats";
            if (seats <= 150) return "101 to 150 seats";
            if (seats <= 200) return "151 to 200 seats";
            return "201 to 250 seats";
        }

        private static PaymentApiFeeLine FixedFee(string head, decimal amount) => new PaymentApiFeeLine
        {
            FeeHead = head,
            UnitAmount = amount,
            Basis = "Fixed",
            Multiplier = 1,
            Calculation = $"{amount:N2} x 1",
            LineAmount = amount
        };

        private static PaymentApiFeeLine PerSeatFee(string head, decimal unit, int seats)
        {
            var n = Math.Max(seats, 0);
            return new PaymentApiFeeLine
            {
                FeeHead = head,
                UnitAmount = unit,
                Basis = "Per seat",
                Multiplier = n,
                Calculation = $"{unit:N2} x {n} seat(s)",
                LineAmount = unit * n
            };
        }

        private static PaymentApiFeeLine PerCourseFee(string head, decimal unit, int courses)
        {
            var n = Math.Max(courses, 0);
            return new PaymentApiFeeLine
            {
                FeeHead = head,
                UnitAmount = unit,
                Basis = "Per course",
                Multiplier = n,
                Calculation = $"{unit:N2} x {n} course(s)",
                LineAmount = unit * n
            };
        }

        // =====================================================================
        //  Helpers (same normalisation rules as PaymentCalculationController)
        // =====================================================================
        private static int ParseInt(string? value) => int.TryParse(value, out var v) ? v : 0;

        private static string NormalizeLabel(string? value) =>
            string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : new string(value.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        private static string NormalizeCourseLevel(string? courseLevel)
        {
            var n = NormalizeLabel(courseLevel);
            if (string.IsNullOrEmpty(n)) return string.Empty;

            if (n.Contains("UG", StringComparison.OrdinalIgnoreCase)
                || n.Contains("UNDERGRADUATE", StringComparison.OrdinalIgnoreCase))
                return "UG";

            if (n.Contains("SS", StringComparison.OrdinalIgnoreCase)
                || n.Contains("SUPERSPECIALTY", StringComparison.OrdinalIgnoreCase)
                || n.Contains("SUPERSPECIALITY", StringComparison.OrdinalIgnoreCase))
                return "SS";

            if (n.Contains("PG", StringComparison.OrdinalIgnoreCase)
                || n.Contains("POSTGRADUATE", StringComparison.OrdinalIgnoreCase)
                || n.Contains("PGBROAD", StringComparison.OrdinalIgnoreCase))
                return "PG";

            return n;
        }

        private static string? GetAffiliationGroup(string? category)
        {
            var n = NormalizeLabel(category);

            if (n.Contains("CONTINUATION", StringComparison.OrdinalIgnoreCase)) return "CONTINUATION";
            if (n.Contains("INCREASE", StringComparison.OrdinalIgnoreCase)
                || n.Contains("ENHANCEMENT", StringComparison.OrdinalIgnoreCase)) return "ENHANCEMENT";
            if (n.Contains("ADDITIONAL", StringComparison.OrdinalIgnoreCase)
                || n.Contains("ADDCOURSE", StringComparison.OrdinalIgnoreCase)
                || n.Contains("ADDCOURSES", StringComparison.OrdinalIgnoreCase)) return "ADDITIONAL";
            if (n.Contains("FRESH", StringComparison.OrdinalIgnoreCase)) return "FRESH";

            return null;
        }

        private static string DisplayName(string? group) => group switch
        {
            "FRESH" => "Fresh Affiliation",
            "CONTINUATION" => "Continuation of Affiliation",
            "ENHANCEMENT" => "Enhancement of Seats / Increase in Intake",
            "ADDITIONAL" => "Additional Courses for college",
            _ => group ?? string.Empty
        };

        private sealed class CalcContext
        {
            public string CollegeCode { get; set; } = string.Empty;
            public bool IsGovernment { get; set; }
            public string Group { get; set; } = "CONTINUATION";
            public List<MstMedicalCollegeCourseIntake> Rows { get; set; } = new();
            public Func<MstMedicalCollegeCourseIntake, string> RowLevel { get; set; } = _ => string.Empty;
            public Func<string?, int?, (string? Name, string? Level)> Master { get; set; } = (_, _) => (null, null);
        }

        /// <summary>College data loaded once and reused for every application type.</summary>
        private sealed class CollegeData
        {
            public string CollegeCode { get; set; } = string.Empty;
            public string? CollegeName { get; set; }
            public bool IsGovernment { get; set; }
            public List<MstMedicalCollegeCourseIntake> Rows { get; set; } = new();
            public Func<MstMedicalCollegeCourseIntake, string> RowLevel { get; set; } = _ => string.Empty;
            public Func<string?, int?, (string? Name, string? Level)> Master { get; set; } = (_, _) => (null, null);
            public List<string> Levels { get; set; } = new();
        }
    }

    // =========================================================================
    //  Basic authentication filter (applies to every endpoint it decorates)
    // =========================================================================
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class BasicAuthAttribute : Attribute, IAuthorizationFilter
    {
        private const string Username = "Medical_01";
        private const string Password = "Rguhs@1996";

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var header = context.HttpContext.Request.Headers["Authorization"].ToString();

            if (header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header[6..].Trim()));
                    var idx = decoded.IndexOf(':');
                    if (idx > 0)
                    {
                        var user = decoded[..idx];
                        var pass = decoded[(idx + 1)..];

                        // Hash first so both comparisons are constant-time and equal length
                        var userOk = CryptographicOperations.FixedTimeEquals(
                            SHA256.HashData(Encoding.UTF8.GetBytes(user)),
                            SHA256.HashData(Encoding.UTF8.GetBytes(Username)));
                        var passOk = CryptographicOperations.FixedTimeEquals(
                            SHA256.HashData(Encoding.UTF8.GetBytes(pass)),
                            SHA256.HashData(Encoding.UTF8.GetBytes(Password)));

                        if (userOk && passOk) return;
                    }
                }
                catch (FormatException) { /* bad base64 -> fall through to 401 */ }
            }

            // No WWW-Authenticate header on purpose: it would trigger the browser's own login popup.
            // Credentials are entered through the Swagger "Authorize" button instead.
            context.Result = new UnauthorizedResult();
        }
    }

    // =========================================================================
    //  Swagger integration: "Authorize" button + padlock on every [BasicAuth] endpoint.
    //  Program.cs only needs:  builder.Services.AddSwaggerGen(c => { c.AddBasicAuthSecurity(); });
    // =========================================================================
    public static class SwaggerBasicAuthExtensions
    {
        public static void AddBasicAuthSecurity(this SwaggerGenOptions options)
        {
            options.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "basic",
                Description = "Enter your API username and password."
            });

            options.OperationFilter<BasicAuthSwaggerFilter>();
        }
    }

    public sealed class BasicAuthSwaggerFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasBasicAuth =
                (context.MethodInfo.DeclaringType?.GetCustomAttributes(true).OfType<BasicAuthAttribute>().Any() ?? false)
                || context.MethodInfo.GetCustomAttributes(true).OfType<BasicAuthAttribute>().Any();

            if (!hasBasicAuth) return;

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Basic" }
                    }] = new List<string>()
                }
            };
        }
    }

    // =========================================================================
    //  Response models (Swagger shows these)
    // =========================================================================
    public class PaymentApiResponse
    {
        public string CollegeCode { get; set; } = string.Empty;
        public string? CollegeName { get; set; }
        public bool IsGovernmentCollege { get; set; }
        public string ApplicationType { get; set; } = string.Empty;
        public List<PaymentApiLevelResult> Results { get; set; } = new();
    }

    public class PaymentApiLevelResult
    {
        public string CourseLevel { get; set; } = string.Empty;
        public PaymentApiCalculation PaymentCalculation { get; set; } = new();
        public PaymentApiFeeBreakdown FeeBreakdown { get; set; } = new();
        public string? Message { get; set; }
    }

    public class PaymentApiCalculation
    {
        public int MatchedCourseCount { get; set; }
        public int TotalIntakeSeats { get; set; }
        public int LevelTotalSeats { get; set; }
        public int CoursesUsedForFee { get; set; }
        public int SeatsUsedForFee { get; set; }
        public List<PaymentApiCourse> MatchedCourses { get; set; } = new();
    }

    public class PaymentApiCourse
    {
        public string CourseName { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public string? CourseLevel { get; set; }
        public int? Intake2627 { get; set; }
        public string? IncreasedIntake { get; set; }
        public string? AcademicYear { get; set; }
        public string? Note { get; set; }
    }

    public class PaymentApiFeeBreakdown
    {
        public List<PaymentApiFeeLine> FeeLines { get; set; } = new();
        public decimal GrandTotal { get; set; }
    }

    public class PaymentApiFeeLine
    {
        public string FeeHead { get; set; } = string.Empty;
        public decimal UnitAmount { get; set; }
        public string Basis { get; set; } = string.Empty;
        public int Multiplier { get; set; }
        public string Calculation { get; set; } = string.Empty;
        public decimal LineAmount { get; set; }
    }

    // ---- by-application-type (full detail, grouped) ---------------------------
    public class PaymentApiAllResponse
    {
        public string CollegeCode { get; set; } = string.Empty;
        public string? CollegeName { get; set; }
        public bool IsGovernmentCollege { get; set; }
        public List<PaymentApiApplicationTypeResult> ApplicationTypes { get; set; } = new();
    }

    public class PaymentApiApplicationTypeResult
    {
        public string ApplicationType { get; set; } = string.Empty;
        public string ApplicationTypeCode { get; set; } = string.Empty;
        /// <summary>Sum of the level grand totals for this application type.</summary>
        public decimal TotalPayable { get; set; }
        public List<PaymentApiLevelResult> Results { get; set; } = new();
    }

    // ---- summary (compact, grouped) -------------------------------------------
    public class PaymentApiSummaryResponse
    {
        public string CollegeCode { get; set; } = string.Empty;
        public string? CollegeName { get; set; }
        public bool IsGovernmentCollege { get; set; }
        public List<PaymentApiSummaryGroup> ApplicationTypes { get; set; } = new();
    }

    public class PaymentApiSummaryGroup
    {
        public string ApplicationType { get; set; } = string.Empty;
        public string ApplicationTypeCode { get; set; } = string.Empty;
        public decimal TotalPayable { get; set; }
        public List<PaymentApiSummaryLevel> Levels { get; set; } = new();
    }

    public class PaymentApiSummaryLevel
    {
        public string CourseLevel { get; set; } = string.Empty;
        public int MatchedCourseCount { get; set; }
        public int CoursesUsedForFee { get; set; }
        public int SeatsUsedForFee { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Message { get; set; }
    }
}