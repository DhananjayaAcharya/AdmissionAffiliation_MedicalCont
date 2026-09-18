using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services;

namespace Medical_Affiliation.Controllers
{
    public class PaymentCalculationController : Controller
    {
        private const string PaymentAcademicYear = "2025-26";
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;
        private readonly IPaymentReceiptPdfService _paymentReceiptPdfService;

        public PaymentCalculationController(
            ApplicationDbContext context,
            IPaymentReceiptPdfService paymentReceiptPdfService)
        {
            _context = context;
            _paymentReceiptPdfService = paymentReceiptPdfService;
            _connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("The application's database connection string is not configured.");
        }

        // GET: /PaymentCalculation
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = await LoadSessionCalculationDetailsAsync();
            await CalculateForCurrentSessionAsync(vm);
            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Calculate()
        {
            var vm = await LoadSessionCalculationDetailsAsync();
            await CalculateForCurrentSessionAsync(vm);
            return View("Index", vm);
        }

        public async Task<PaymentCalculationViewModel> GetCurrentCalculationAsync()
        {
            var vm = await LoadSessionCalculationDetailsAsync();
            await CalculateForCurrentSessionAsync(vm);
            return vm;
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

        [HttpGet]
        public async Task<IActionResult> DownloadPdf()
        {
            var vm = await LoadSessionCalculationDetailsAsync();
            await CalculateForCurrentSessionAsync(vm);

            if (!vm.HasResult)
            {
                return BadRequest(vm.ErrorMessage ?? "Payment calculation is not available.");
            }
            
            var collegeName = await _context.AffiliationCollegeMasters
                .AsNoTracking()
                .Where(x => x.CollegeCode == vm.CollegeCode)
                .Select(x => x.CollegeName)
                .FirstOrDefaultAsync();

            var data = new PaymentReceiptData
            {
                CollegeCode = vm.CollegeCode ?? string.Empty,
                CollegeName = collegeName,
                CourseLevel = vm.CourseLevelGroup ?? vm.CourseLevel ?? string.Empty,
                AffiliationTypeName = vm.TypeOfAffiliation,
                MatchedCourses = vm.MatchedCourses.Select(course => new PaymentReceiptCourseLine
                {
                    CourseName = string.IsNullOrWhiteSpace(course.CourseName) ? course.course : course.CourseName,
                    CourseLevel = course.RawCourseLevel ?? course.ug_pg ?? string.Empty,
                    Intake = GetEffectiveSeatCount(course, IsEnhancementAffiliation(vm))
                }).ToList(),
                FeeLines = vm.FeeLines.Select(fee => new PaymentReceiptFeeLine
                {
                    FeeHead = fee.FeeHeadName ?? string.Empty,
                    UnitAmount = fee.UnitAmount,
                    Basis = fee.IsPerCourse ? "Per course" : fee.IsPerSeat ? "Per seat" : "Fixed",
                    Multiplier = fee.Multiplier,
                    LineAmount = fee.LineAmount
                }).ToList(),
                GrandTotal = vm.Summary?.GrandTotal ?? 0
            };

            var outputFolder = Path.Combine(Path.GetTempPath(), "MedicalAffiliationPaymentReports");
            var filePath = _paymentReceiptPdfService.GenerateReceipt(data, outputFolder);
            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);

            return File(bytes, "application/pdf", $"PaymentCalculation-{vm.CollegeCode}.pdf");
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
            var queryAffiliationTypeId = Request.Query["affiliationTypeId"].ToString();
            var queryTypeOfAffiliation = Request.Query["typeOfAffiliation"].ToString();
            var queryCourseLevel = Request.Query["courseLevel"].ToString();
            var queryCollegeCode = Request.Query["collegeCode"].ToString();
            var queryFacultyCode = Request.Query["facultyCode"].ToString();
            var queryCourseCode = Request.Query["courseCode"].ToString();
            if (string.IsNullOrWhiteSpace(queryCourseLevel))
            {
                queryCourseLevel = Request.Query["level"].ToString();
            }

            if (!string.IsNullOrWhiteSpace(queryTypeOfAffiliation))
            {
                HttpContext.Session.SetString("TypeOfAffiliation", queryTypeOfAffiliation.Trim());
            }

            if (!string.IsNullOrWhiteSpace(queryCourseLevel))
            {
                HttpContext.Session.SetString("CourseLevel", queryCourseLevel.Trim());
                HttpContext.Session.SetString("SelectedCourseLevel", queryCourseLevel.Trim());
            }

            if (!string.IsNullOrWhiteSpace(queryCollegeCode))
            {
                HttpContext.Session.SetString("CollegeCode", queryCollegeCode.Trim());
            }

            if (!string.IsNullOrWhiteSpace(queryFacultyCode))
            {
                HttpContext.Session.SetString("FacultyCode", queryFacultyCode.Trim());
            }

            // Persist a specific course code (e.g. selected while requesting an
            // additional course) so the Additional-Courses fee calculation can
            // scope itself to just that course when one has been chosen.
            if (!string.IsNullOrWhiteSpace(queryCourseCode))
            {
                HttpContext.Session.SetString("CourseCode", queryCourseCode.Trim());
            }

            if (!string.IsNullOrWhiteSpace(queryAffiliationTypeId)
                && int.TryParse(queryAffiliationTypeId, out var parsedQueryAffiliationTypeId))
            {
                HttpContext.Session.SetInt32("AffiliationType", parsedQueryAffiliationTypeId);
                HttpContext.Session.SetString("AffiliationTypeId", parsedQueryAffiliationTypeId.ToString());
            }

            var affiliationTypeId = HttpContext.Session.GetInt32("AffiliationType") ?? 0;
            if (affiliationTypeId <= 0
                && int.TryParse(HttpContext.Session.GetString("AffiliationTypeId"), out var parsedId))
            {
                affiliationTypeId = parsedId;
            }

            var facultyCode = HttpContext.Session.GetString("FacultyCode")
                ?? User?.FindFirst("FacultyCode")?.Value;
            var courseLevel = HttpContext.Session.GetString("CourseLevel")
                ?? HttpContext.Session.GetString("SelectedCourseLevel")
                ?? User?.FindFirst("CourseLevel")?.Value
                ?? User?.FindFirst("SelectedCourseLevel")?.Value;
            if (string.IsNullOrWhiteSpace(courseLevel) && !string.IsNullOrWhiteSpace(queryCourseLevel))
            {
                courseLevel = queryCourseLevel.Trim();
            }

            var normalizedCourseLevel = courseLevel?.Trim().ToUpperInvariant() ?? string.Empty;

            var sessionAffiliationName = HttpContext.Session.GetString("TypeOfAffiliation")
                ?? User?.FindFirst("TypeOfAffiliation")?.Value;
            if (string.IsNullOrWhiteSpace(sessionAffiliationName) && !string.IsNullOrWhiteSpace(queryTypeOfAffiliation))
            {
                sessionAffiliationName = queryTypeOfAffiliation.Trim();
            }

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

            var resolvedAffiliationTypeId = affiliationType?.AffiliationTypeId ?? affiliationTypeId;
            if (resolvedAffiliationTypeId <= 0 && !string.IsNullOrWhiteSpace(sessionAffiliationName))
            {
                var normalizedAffiliationName = NormalizeLabel(sessionAffiliationName);
                if (normalizedAffiliationName.Contains("ADDITIONAL", StringComparison.OrdinalIgnoreCase)
                    || normalizedAffiliationName.Contains("ADDCOURSE", StringComparison.OrdinalIgnoreCase))
                {
                    resolvedAffiliationTypeId = 4;
                }
                else if (normalizedAffiliationName.Contains("ENHANCEMENT", StringComparison.OrdinalIgnoreCase)
                    || normalizedAffiliationName.Contains("INCREASE", StringComparison.OrdinalIgnoreCase))
                {
                    resolvedAffiliationTypeId = 3;
                }
                else if (normalizedAffiliationName.Contains("CONTINUATION", StringComparison.OrdinalIgnoreCase))
                {
                    resolvedAffiliationTypeId = 1;
                }
            }

            if (resolvedAffiliationTypeId > 0)
            {
                HttpContext.Session.SetString("AffiliationTypeId", resolvedAffiliationTypeId.ToString());
                HttpContext.Session.SetInt32("AffiliationType", resolvedAffiliationTypeId);
            }

            if (!string.IsNullOrWhiteSpace(sessionAffiliationName))
            {
                HttpContext.Session.SetString("TypeOfAffiliation", sessionAffiliationName);
            }

            var collegeCode = HttpContext.Session.GetString("CollegeCode")
                ?? User?.FindFirst("CollegeCode")?.Value
                ?? (!string.IsNullOrWhiteSpace(queryCollegeCode) ? queryCollegeCode.Trim() : null);
            var currentFacultyCode = int.TryParse(
                facultyCode ?? User?.FindFirst("FacultyCode")?.Value ?? (!string.IsNullOrWhiteSpace(queryFacultyCode) ? queryFacultyCode.Trim() : null),
                out var parsedFacultyCode)
                ? parsedFacultyCode
                : 0;
            var paymentSaved = !string.IsNullOrWhiteSpace(collegeCode)
                && resolvedAffiliationTypeId > 0
                && !string.IsNullOrWhiteSpace(courseLevel)
                && await _context.PaymentAffiliationDocuments.AnyAsync(p =>
                    p.CollegeCode == collegeCode
                    && p.FacultyCode == currentFacultyCode
                    && p.CourseLevel == courseLevel
                    && p.AffiliationTypeId == resolvedAffiliationTypeId);

            var resolvedCourseLevelGroup = affiliationType?.CourseLevelGroup
                ?? (!string.IsNullOrWhiteSpace(courseLevel) ? NormalizeCourseLevel(courseLevel) : null);

            return new PaymentCalculationViewModel
            {
                CollegeCode = collegeCode,
                FacultyCode = currentFacultyCode,
                AffiliationTypeId = resolvedAffiliationTypeId,
                TypeOfAffiliation = GetAffiliationDisplayName(affiliationType?.AffiliationCategory)
                    ?? HttpContext.Session.GetString("TypeOfAffiliation"),
                CourseLevel = courseLevel,
                CourseLevelGroup = resolvedCourseLevelGroup,
                AcademicYear = PaymentAcademicYear,
                PaymentSaved = paymentSaved,
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

            if (string.IsNullOrEmpty(normalizedLevel))
            {
                return string.Empty;
            }

            if (normalizedLevel.Contains("UG", StringComparison.OrdinalIgnoreCase)
                || normalizedLevel.Contains("UNDERGRADUATE", StringComparison.OrdinalIgnoreCase))
            {
                return "UG";
            }

            if (normalizedLevel.Contains("SS", StringComparison.OrdinalIgnoreCase)
                || normalizedLevel.Contains("SUPERSPECIALTY", StringComparison.OrdinalIgnoreCase)
                || normalizedLevel.Contains("SUPERSPECIALITY", StringComparison.OrdinalIgnoreCase))
            {
                return "SS";
            }

            if (normalizedLevel.Contains("PG", StringComparison.OrdinalIgnoreCase)
                || normalizedLevel.Contains("POSTGRADUATE", StringComparison.OrdinalIgnoreCase)
                || normalizedLevel.Contains("PGBROAD", StringComparison.OrdinalIgnoreCase))
            {
                return "PG";
            }

            return normalizedLevel;
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

        /// <summary>
        /// Buckets a free-text affiliation category into one of the three
        /// affiliation groups the 2025-26 circular defines fee schedules for:
        /// CONTINUATION, ADDITIONAL (additional/fresh courses for an already
        /// affiliated college) or ENHANCEMENT (increase in intake). "FRESH"
        /// (brand-new UG affiliation) is recognised too, but has no per-course
        /// or per-seat schedule of its own in <see cref="BuildCircularFeeLines"/>.
        /// </summary>
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

            if (normalizedCategory.Contains("ADDITIONAL", StringComparison.OrdinalIgnoreCase)
                || normalizedCategory.Contains("ADDCOURSE", StringComparison.OrdinalIgnoreCase)
                || normalizedCategory.Contains("ADDCOURSES", StringComparison.OrdinalIgnoreCase))
            {
                return "ADDITIONAL";
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
                "ADDITIONAL" => "Additional Courses for college",
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

                    // ---- Result Set 2: legacy fee lines (kept for reader compatibility;
                    // the amounts are superseded below by the circular fee schedule) ----
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

            await EnrichAdditionalCourseSelectionsAsync(vm);
            await PopulateIncreasedIntakeDataAsync(vm);
            await PopulateTotalSeatsAsync(vm);
            ApplyCircularFeeSchedule(vm);
        }

        private async Task PopulateTotalSeatsAsync(PaymentCalculationViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.CollegeCode) || vm.MatchedCourses.Count == 0)
            {
                return;
            }

            var intakeRows = await _context.MstMedicalCollegeCourseIntakes
                .AsNoTracking()
                .Where(x => x.CollCode == vm.CollegeCode)
                .Select(x => new { x.UgPg, x.Intake2627 })
                .ToListAsync();

            var totalsByLevel = intakeRows
                .GroupBy(x => NormalizeCourseLevel(x.UgPg))
                .ToDictionary(
                    group => group.Key,
                    group => group.Sum(x => x.Intake2627 ?? 0),
                    StringComparer.OrdinalIgnoreCase);

            foreach (var course in vm.MatchedCourses)
            {
                var courseLevel = NormalizeCourseLevel(course.RawCourseLevel ?? course.ug_pg);
                course.TotalSeats = totalsByLevel.TryGetValue(courseLevel, out var totalSeats)
                    ? totalSeats
                    : 0;
            }
        }

        private async Task EnrichAdditionalCourseSelectionsAsync(PaymentCalculationViewModel vm)
        {
            var sessionCollegeCode = HttpContext.Session.GetString("CollegeCode") ?? vm.CollegeCode;
            var sessionFacultyCode = HttpContext.Session.GetString("FacultyCode") ?? vm.FacultyCode.ToString();
            var sessionCourseLevel = HttpContext.Session.GetString("CourseLevel")
                ?? HttpContext.Session.GetString("SelectedCourseLevel")
                ?? vm.CourseLevel;
            var sessionAffiliationName = HttpContext.Session.GetString("TypeOfAffiliation") ?? vm.TypeOfAffiliation ?? string.Empty;

            if (!IsAdditionalCourseAffiliation(vm)
                || string.IsNullOrWhiteSpace(sessionCollegeCode)
                || string.IsNullOrWhiteSpace(sessionFacultyCode)
                || !int.TryParse(sessionFacultyCode, out var sessionFacultyId)
                || sessionFacultyId <= 0)
            {
                return;
            }

            var requestedCoursesQuery = _context.AddCoursedetails
                .AsNoTracking()
                .Where(x => x.CollegeCode == sessionCollegeCode
                    && x.FacultyCode == sessionFacultyCode
                    && x.AddCourseRequested);

            if (!string.IsNullOrWhiteSpace(sessionCourseLevel))
            {
                requestedCoursesQuery = requestedCoursesQuery.Where(x =>
                    string.IsNullOrWhiteSpace(x.CourseLevel)
                    || x.CourseLevel == sessionCourseLevel
                    || x.CourseLevel.Trim() == sessionCourseLevel.Trim());
            }

            if (!string.IsNullOrWhiteSpace(sessionAffiliationName))
            {
                requestedCoursesQuery = requestedCoursesQuery.Where(x =>
                    string.IsNullOrWhiteSpace(x.TypeOfAffiliation)
                    || x.TypeOfAffiliation == sessionAffiliationName
                    || x.TypeOfAffiliation.Contains("Additional")
                    || x.TypeOfAffiliation.Contains("Add Course")
                    || x.TypeOfAffiliation.Contains("Addl"));
            }

            // Optional: scope to one specific course code carried in session
            // (e.g. the course the user picked in the additional-course wizard).
            var sessionCourseCode = HttpContext.Session.GetString("CourseCode");
            if (!string.IsNullOrWhiteSpace(sessionCourseCode))
            {
                var requestedCourseCodes = sessionCourseCode
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();

                if (requestedCourseCodes.Count > 0)
                {
                    requestedCoursesQuery = requestedCoursesQuery.Where(x =>
                        requestedCourseCodes.Contains(x.CourseCode));
                }
            }

            var requestedCourses = await requestedCoursesQuery.ToListAsync();

            if (requestedCourses.Count == 0)
            {
                return;
            }

            var masterCourseMap = await _context.MstCourses
                .AsNoTracking()
                .Where(c => c.FacultyCode == vm.FacultyCode)
                .ToDictionaryAsync(c => c.CourseCode.ToString(), c => c, StringComparer.OrdinalIgnoreCase);

            foreach (var requested in requestedCourses)
            {
                if (string.IsNullOrWhiteSpace(requested.CourseCode))
                {
                    continue;
                }

                var existing = vm.MatchedCourses.FirstOrDefault(c =>
                    string.Equals(c.CourseCode, requested.CourseCode, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    existing.Intake_26_27 = requested.RequestedCourseIntake ?? existing.Intake_26_27 ?? 0;
                    existing.CourseName = masterCourseMap.TryGetValue(requested.CourseCode, out var course)
                        ? course.CourseName
                        : existing.CourseName;
                    existing.RawCourseLevel = string.IsNullOrWhiteSpace(requested.CourseLevel)
                        ? masterCourseMap.TryGetValue(requested.CourseCode, out var matchedCourse) ? matchedCourse.CourseLevel : existing.RawCourseLevel
                        : requested.CourseLevel;
                    existing.MatchNote = "Requested via Additional Courses";
                    continue;
                }

                var info = masterCourseMap.TryGetValue(requested.CourseCode, out var masterCourse)
                    ? masterCourse
                    : null;

                vm.MatchedCourses.Add(new MatchedCourseVM
                {
                    SLNO = 0,
                    Facultycode = vm.FacultyCode,
                    coll_code = vm.CollegeCode,
                    collegename = string.Empty,
                    course = info?.CourseName ?? requested.CourseCode,
                    ug_pg = info?.CourseLevel ?? requested.CourseLevel ?? vm.CourseLevel ?? string.Empty,
                    Intake_26_27 = requested.RequestedCourseIntake ?? 0,
                    CourseCode = requested.CourseCode,
                    CourseName = info?.CourseName ?? requested.CourseCode,
                    RawCourseLevel = info?.CourseLevel ?? requested.CourseLevel ?? vm.CourseLevel ?? string.Empty,
                    MatchNote = "Requested via Additional Courses"
                });
            }

            if (vm.Summary != null)
            {
                vm.Summary.MatchedCourseCount = vm.MatchedCourses.Count;
                vm.Summary.TotalIntakeSeats = vm.MatchedCourses.Sum(x => x.Intake_26_27 ?? 0);
            }
        }

        /// <summary>
        /// Populates each matched course's increased-intake figure from the
        /// <c>Increased_Intake</c> column of
        /// <c>[Admission_Affiliation].[dbo].[Mst_MedicalCollegeCourseIntake]</c>
        /// (mapped via EF as <c>MstMedicalCollegeCourseIntakes</c>, property
        /// <c>IncreasedIntake</c>). This is the sole source the Enhancement /
        /// Increase-in-Intake fee calculation reads from — it does NOT fall
        /// back to <c>Intake_26_27</c> (that column is the currently
        /// sanctioned/base intake, not the increase being paid for). This
        /// method only enriches course data — fee amounts are computed
        /// separately in <see cref="ApplyCircularFeeSchedule"/>.
        /// </summary>
        private async Task PopulateIncreasedIntakeDataAsync(PaymentCalculationViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.CollegeCode) || vm.FacultyCode <= 0)
            {
                return;
            }

            var slnos = vm.MatchedCourses
                .Where(x => x.SLNO > 0)
                .Select(x => x.SLNO)
                .Distinct()
                .ToList();

            var courseCodes = vm.MatchedCourses
                .Where(x => x.SLNO <= 0 && !string.IsNullOrWhiteSpace(x.CourseCode))
                .Select(x => x.CourseCode!)
                .Distinct()
                .ToList();

            // Mst_MedicalCollegeCourseIntake.CourseCode is an int column, while
            // MatchedCourseVM.CourseCode is a string - convert for the lookup.
            var courseCodeInts = courseCodes
                .Select(c => int.TryParse(c, out var parsed) ? (int?)parsed : null)
                .Where(c => c.HasValue)
                .Select(c => c!.Value)
                .Distinct()
                .ToList();

            if (slnos.Count == 0 && courseCodeInts.Count == 0)
            {
                return;
            }

            var intakeRows = await _context.MstMedicalCollegeCourseIntakes
                .AsNoTracking()
                .Where(x => x.CollCode == vm.CollegeCode
                    && x.Facultycode == vm.FacultyCode
                    && (slnos.Contains(x.Slno)
                        || (x.CourseCode.HasValue && courseCodeInts.Contains(x.CourseCode.Value))))
                .ToListAsync();

            var rowsBySlno = intakeRows
                .Where(x => x.Slno > 0)
                .GroupBy(x => x.Slno)
                .ToDictionary(g => g.Key, g => g.First());

            var rowsByCourseCode = intakeRows
                .Where(x => x.CourseCode.HasValue)
                .GroupBy(x => x.CourseCode!.Value.ToString(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var matchedCourse in vm.MatchedCourses)
            {
                var row = matchedCourse.SLNO > 0 && rowsBySlno.TryGetValue(matchedCourse.SLNO, out var slnoRow)
                    ? slnoRow
                    : (!string.IsNullOrWhiteSpace(matchedCourse.CourseCode)
                        && rowsByCourseCode.TryGetValue(matchedCourse.CourseCode, out var courseCodeRow)
                        ? courseCodeRow
                        : null);

                if (row == null)
                {
                    continue;
                }

                // Increased_Intake column - the actual seat increase being
                // affiliated/paid for under Enhancement of Seats.
                matchedCourse.IncreasedIntake = row.IncreasedIntake;
                matchedCourse.AcademicYear = row.AcademicYear ?? string.Empty;
            }
        }

        // =====================================================================
        // Circular-driven fee schedule
        // Source: RGUHS/MEDICAL/AFF-FEE/2025-26, dated 01/12/2025.
        // Three affiliation types are supported, each calculated per
        // course level (UG / PG broad specialty / Super Specialty):
        //   1. Continuation of Affiliation      -> Forms 01 (UG), 02 (PG), 03 (SS)
        //   2. Additional Courses for college    -> Forms 06 (PG), 07 (SS)
        //                                           (circular has no UG "additional
        //                                            course" fee head - UG only has
        //                                            a single fresh-affiliation fee)
        //   3. Enhancement of Seats / Increase
        //      in Intake                         -> Forms 05 (UG), 06 (PG), 07 (SS)
        // =====================================================================

        private static class MedicalAffiliationFeeSchedule
        {
            public const decimal ApplicationFee = 3000m;
            public const decimal CourseIdentificationFee = 10m;

            // Continuation of Affiliation - UG (Form 01)
            public const decimal ContinuationUgAnnualFee = 200000m;
            public const decimal ContinuationUgRenewalFee = 500000m;
            public const decimal ContinuationUgHelinetFee = 100000m;

            // Continuation of Affiliation - PG broad specialty (Form 02)
            public const decimal ContinuationPgPerSeatRenewalFee = 4500m;
            public const decimal ContinuationPgHelinetFee = 30000m;

            // Continuation of Affiliation - Super Specialty (Form 03)
            public const decimal ContinuationSsPerSeatRenewalFee = 7500m;
            public const decimal ContinuationSsHelinetFee = 30000m;

            // Additional Courses for college - PG broad specialty (Form 06)
            public const decimal AdditionalPgPerCourseFee = 400000m;

            // Additional Courses for college - Super Specialty (Form 07)
            public const decimal AdditionalSsPerCourseFee = 400000m;

            // Enhancement of Seats / Increase in Intake - PG broad specialty (Form 06)
            public const decimal EnhancementPgPerSeatFee = 50000m;

            // Enhancement of Seats / Increase in Intake - Super Specialty (Form 07)
            public const decimal EnhancementSsPerSeatFee = 50000m;

            /// <summary>
            /// UG Administration &amp; Service charge seat bands. The circular
            /// uses the identical band schedule for both Continuation (Form 01,
            /// item 5) and Enhancement / Increase in Intake (Form 05, item 3):
            /// 1-100 seats -> 3,00,000; 101-150 -> 4,50,000;
            /// 151-200 -> 6,00,000; 201-250 -> 7,50,000.
            /// Seat counts above 250 are billed at the top band pending an
            /// updated circular.
            /// </summary>
            public static decimal GetUgSeatBandAmount(int seats)
            {
                if (seats <= 100) return 300000m;
                if (seats <= 150) return 450000m;
                if (seats <= 200) return 600000m;
                return 750000m;
            }

            public static string DescribeSeatBand(int seats)
            {
                if (seats <= 100) return "1 to 100 seats";
                if (seats <= 150) return "101 to 150 seats";
                if (seats <= 200) return "151 to 200 seats";
                return "201 to 250 seats";
            }
        }

        private static FeeLineVM FixedFee(string feeHeadName, decimal amount)
        {
            return new FeeLineVM
            {
                FeeHeadName = feeHeadName,
                UnitAmount = amount,
                IsPerCourse = false,
                IsPerSeat = false,
                Multiplier = 1,
                LineAmount = amount
            };
        }

        private static FeeLineVM PerSeatFee(string feeHeadName, decimal unitAmount, int seatCount)
        {
            var effectiveSeatCount = Math.Max(seatCount, 0);
            return new FeeLineVM
            {
                FeeHeadName = feeHeadName,
                UnitAmount = unitAmount,
                IsPerCourse = false,
                IsPerSeat = true,
                Multiplier = effectiveSeatCount,
                LineAmount = unitAmount * effectiveSeatCount
            };
        }

        private static FeeLineVM PerCourseFee(string feeHeadName, decimal unitAmount, int courseCount)
        {
            var effectiveCourseCount = Math.Max(courseCount, 0);
            return new FeeLineVM
            {
                FeeHeadName = feeHeadName,
                UnitAmount = unitAmount,
                IsPerCourse = true,
                IsPerSeat = false,
                Multiplier = effectiveCourseCount,
                LineAmount = unitAmount * effectiveCourseCount
            };
        }

        /// <summary>
        /// Builds the fee lines for one of the three supported affiliation
        /// groups (CONTINUATION / ADDITIONAL / ENHANCEMENT) at the given
        /// course level (UG / PG / SS), using the fixed amounts from the
        /// 2025-26 fee circular.
        /// </summary>
        private static List<FeeLineVM> BuildCircularFeeLines(
            string? affiliationGroup,
            string courseLevel,
            int courseCount,
            int seatCount)
        {
            var feeLines = new List<FeeLineVM>
            {
                FixedFee("Application fee", MedicalAffiliationFeeSchedule.ApplicationFee),
                FixedFee("Course identification fee", MedicalAffiliationFeeSchedule.CourseIdentificationFee)
            };

            switch (affiliationGroup)
            {
                case "CONTINUATION":
                    switch (courseLevel)
                    {
                        case "UG":
                            feeLines.Add(FixedFee("Annual fee", MedicalAffiliationFeeSchedule.ContinuationUgAnnualFee));
                            feeLines.Add(FixedFee("Renewal of affiliation fee", MedicalAffiliationFeeSchedule.ContinuationUgRenewalFee));
                            feeLines.Add(FixedFee(
                                $"Administration and Service charges ({MedicalAffiliationFeeSchedule.DescribeSeatBand(seatCount)})",
                                MedicalAffiliationFeeSchedule.GetUgSeatBandAmount(seatCount)));
                            feeLines.Add(FixedFee("HELINET fee", MedicalAffiliationFeeSchedule.ContinuationUgHelinetFee));
                            break;
                        case "PG":
                            feeLines.Add(PerSeatFee("Renewal of PG broad specialty", MedicalAffiliationFeeSchedule.ContinuationPgPerSeatRenewalFee, seatCount));
                            feeLines.Add(FixedFee("HELINET fee", MedicalAffiliationFeeSchedule.ContinuationPgHelinetFee));
                            break;
                        case "SS":
                            feeLines.Add(PerSeatFee("Renewal of PG Super Specialty", MedicalAffiliationFeeSchedule.ContinuationSsPerSeatRenewalFee, seatCount));
                            feeLines.Add(FixedFee("HELINET fee", MedicalAffiliationFeeSchedule.ContinuationSsHelinetFee));
                            break;
                    }
                    break;

                case "ADDITIONAL":
                    switch (courseLevel)
                    {
                        case "PG":
                            feeLines.Add(PerCourseFee("PG fresh course affiliation fee", MedicalAffiliationFeeSchedule.AdditionalPgPerCourseFee, courseCount));
                            break;
                        case "SS":
                            feeLines.Add(PerCourseFee("Fresh PG Super Specialty course", MedicalAffiliationFeeSchedule.AdditionalSsPerCourseFee, courseCount));
                            break;
                            // UG: the circular has no "additional course" fee head for UG -
                            // only Application fee + Course identification fee apply.
                    }
                    break;

                case "ENHANCEMENT":
                    switch (courseLevel)
                    {
                        case "UG":
                            feeLines.Add(FixedFee(
                                $"Administration and Service charges ({MedicalAffiliationFeeSchedule.DescribeSeatBand(seatCount)})",
                                MedicalAffiliationFeeSchedule.GetUgSeatBandAmount(seatCount)));
                            break;
                        case "PG":
                            feeLines.Add(PerSeatFee("Increase in intake fee", MedicalAffiliationFeeSchedule.EnhancementPgPerSeatFee, seatCount));
                            break;
                        case "SS":
                            feeLines.Add(PerSeatFee("PG Super Specialty course - increase in intake fee", MedicalAffiliationFeeSchedule.EnhancementSsPerSeatFee, seatCount));
                            break;
                    }
                    break;
            }

            return feeLines;
        }

        /// <summary>
        /// Reads College Code, Faculty Code, Course Level and Type of
        /// Affiliation from session (falling back to values already resolved
        /// onto the view model), classifies them into one of the three
        /// circular affiliation groups x three course levels, and replaces
        /// vm.FeeLines / vm.Summary with the resulting circular-based totals.
        /// </summary>
        private void ApplyCircularFeeSchedule(PaymentCalculationViewModel vm)
        {
            var sessionCollegeCode = HttpContext.Session.GetString("CollegeCode") ?? vm.CollegeCode ?? string.Empty;
            var sessionCourseLevel = HttpContext.Session.GetString("CourseLevel")
                ?? HttpContext.Session.GetString("SelectedCourseLevel")
                ?? vm.CourseLevelGroup
                ?? vm.CourseLevel
                ?? string.Empty;
            var sessionAffiliationName = HttpContext.Session.GetString("TypeOfAffiliation") ?? vm.TypeOfAffiliation ?? string.Empty;
            var sessionCourseCode = HttpContext.Session.GetString("CourseCode");

            var courseLevel = NormalizeCourseLevel(sessionCourseLevel);
            var affiliationGroup = GetAffiliationGroup(sessionAffiliationName);

            // Fall back to the resolved AffiliationTypeId when the free-text
            // category couldn't be classified (1 = Continuation, 3 = Enhancement,
            // 4 = Additional, mirroring the ids already used elsewhere in this
            // controller for the enhancement / additional groups).
            if (string.IsNullOrEmpty(affiliationGroup))
            {
                affiliationGroup = vm.AffiliationTypeId switch
                {
                    1 => "CONTINUATION",
                    3 => "ENHANCEMENT",
                    4 => "ADDITIONAL",
                    _ => null
                };
            }

            int courseCount;
            int seatCount;

            if (string.Equals(affiliationGroup, "ADDITIONAL", StringComparison.OrdinalIgnoreCase))
            {
                var additionalCourses = vm.MatchedCourses
                    .Where(c => string.Equals(c.MatchNote, "Requested via Additional Courses", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!string.IsNullOrWhiteSpace(sessionCourseCode))
                {
                    var requestedCodes = sessionCourseCode
                        .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    courseCount = additionalCourses.Count > 0
                        ? additionalCourses.Count(c => requestedCodes.Contains(c.CourseCode, StringComparer.OrdinalIgnoreCase))
                        : requestedCodes.Length;
                }
                else
                {
                    courseCount = additionalCourses.Count > 0 ? additionalCourses.Count : vm.MatchedCourses.Count;
                }

                seatCount = 0;
            }
            else if (string.Equals(affiliationGroup, "ENHANCEMENT", StringComparison.OrdinalIgnoreCase))
            {
                // Enhancement of Seats / Increase in Intake is billed strictly
                // on the Increased_Intake column (the seats being added),
                // never on Intake_26_27 (the existing sanctioned intake).
                courseCount = vm.MatchedCourses.Count;
                seatCount = vm.MatchedCourses.Sum(c => c.IncreasedIntake ?? 0);
            }
            else
            {
                // CONTINUATION (and any unclassified group defaults to the
                // currently sanctioned seat/course counts).
                courseCount = vm.MatchedCourses.Count;
                seatCount = vm.MatchedCourses.Sum(c => c.Intake_26_27 ?? 0);
            }

            var feeLines = BuildCircularFeeLines(affiliationGroup, courseLevel, courseCount, seatCount);

            vm.FeeLines.Clear();
            vm.FeeLines.AddRange(feeLines);

            var grandTotal = feeLines.Sum(f => f.LineAmount);

            if (vm.Summary == null)
            {
                vm.Summary = new PaymentSummaryVM
                {
                    CollegeCode = sessionCollegeCode,
                    FacultyCode = vm.FacultyCode,
                    AffiliationTypeId = vm.AffiliationTypeId
                };
            }

            vm.Summary.CourseLevelGroup = string.IsNullOrEmpty(courseLevel) ? vm.Summary.CourseLevelGroup : courseLevel;
            vm.Summary.MatchedCourseCount = vm.MatchedCourses.Count;
            vm.Summary.TotalIntakeSeats = seatCount > 0 ? seatCount : vm.MatchedCourses.Sum(c => c.Intake_26_27 ?? 0);
            vm.Summary.GrandTotal = grandTotal;

            // No schedule head beyond Application/Course-identification fee was
            // matched (e.g. "Additional Courses" requested at UG level, which
            // the circular does not define) - flag it instead of silently
            // billing only the two base fees.
            if (feeLines.Count == 2 && !string.IsNullOrEmpty(affiliationGroup))
            {
                vm.ErrorMessage ??= $"No {courseLevel}-level fee schedule is defined in the circular for '{GetAffiliationDisplayName(sessionAffiliationName) ?? sessionAffiliationName}'.";
            }
        }

        private static bool IsEnhancementAffiliation(PaymentCalculationViewModel vm)
        {
            var affiliationName = vm.TypeOfAffiliation ?? string.Empty;
            return vm.AffiliationTypeId == 3
                || affiliationName.Contains("Enhancement", StringComparison.OrdinalIgnoreCase)
                || affiliationName.Contains("Increase in Intake", StringComparison.OrdinalIgnoreCase)
                || affiliationName.Contains("Increase", StringComparison.OrdinalIgnoreCase) && affiliationName.Contains("Intake", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAdditionalCourseAffiliation(PaymentCalculationViewModel vm)
        {
            var affiliationName = vm.TypeOfAffiliation ?? string.Empty;
            return vm.AffiliationTypeId == 4
                || affiliationName.Contains("Additional", StringComparison.OrdinalIgnoreCase)
                || affiliationName.Contains("Addl", StringComparison.OrdinalIgnoreCase)
                || affiliationName.Contains("Add Course", StringComparison.OrdinalIgnoreCase);
        }

        private static int GetEffectiveSeatCount(MatchedCourseVM course, bool useIncreasedIntakeSeats)
        {
            // For Enhancement receipts, show the Increased_Intake figure that
            // was actually billed - not the pre-existing base intake.
            return useIncreasedIntakeSeats
                ? course.IncreasedIntake ?? 0
                : course.Intake_26_27 ?? 0;
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