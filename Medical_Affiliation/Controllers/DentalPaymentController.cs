using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Medical_Affiliation.Controllers
{
    public class DentalPaymentController : Controller
    {
        private const string PaymentAcademicYear = "2025-26";
        private readonly ApplicationDbContext _context;
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


        public DentalPaymentController(ApplicationDbContext context)
        {
            _context = context;
            _connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("The application's database connection string is not configured.");
        }

        // GET: /PaymentCalculation
        [HttpGet]
        public async Task<IActionResult> Index(string AffiliationTypeId)
        {

            if (string.IsNullOrWhiteSpace(AffiliationTypeId) || AffTypeId > 0)
            {
                return BadRequest("Invalid Affiliation Type.");
            }

            var affilitionType = await _context.MstAffiliationTypes
                .FirstOrDefaultAsync(e =>
                    e.AffiliationTypeId == AffTypeId &&
                    e.FacultyCode == _facultyCode.ToString()
                );

            if (affilitionType == null) return NotFound();

            var feeTypes = await _context.MstDentalFeeTypes
                .Where(e =>
                    e.FacultyCode == _facultyCode &&
                    e.AffiliationTypeId == AffTypeId &&
                    e.IsActive
                )
                .OrderBy(e=> e.DisplayOrder)
                .ToListAsync();

            var feeStructures = await _context.MstDentalFeeStructures
                .Where(e =>
                    e.FacultyCode == _facultyCode &&
                    e.AffiliationTypeId.Equals(AffTypeId) &&
                    e.IsActive
                )
                .ToListAsync();

            var otherFees = await _context.MstDentalOtherFeeStructures
                .Where(e => 
                    e.FacultyCode == _facultyCode &&
                    e.AffiliationTypeId == AffTypeId &&
                    e.IsActive
                )
                .OrderBy(e=>e.DisplayOrder)
                .ToListAsync();

            var model = new DentalFeeStructureViewModel
            {
                FacultyCode = _facultyCode,
                AffiliationTypeId = AffTypeId,
                AffiliationCategory = affilitionType.AffiliationCategory,

                FeeTypes = feeTypes.Select(feeType =>
                {
                    var bdsFee = feeStructures.FirstOrDefault(e => e.FeeTypeId == feeType.Id && (e.CourseName == "BDS" || e.CourseLevel.ToLower() == "ug"));
                    var MdsFee = feeStructures.FirstOrDefault(e => e.FeeTypeId == feeType.Id && (e.CourseName == "MDS" || e.CourseLevel.ToLower() == "pg"));

                    return new DentalFeeTypeRowViewModel
                    {
                        FeeTypeId = feeType.Id,
                        FeeType = feeType.FeeType,
                        DisplayOrder = feeType.DisplayOrder,

                        BDS = bdsFee == null ? null : new DentalCourseFeeViewModel
                        {
                            Id = bdsFee.Id,
                            CourseName = bdsFee.CourseName,
                            CourseLevel = bdsFee.CourseLevel,
                            AmountToBePaid = bdsFee.AmountToBePaid,
                            CalculationType = bdsFee.CalculationType
                        },

                        MDS = MdsFee == null ? null : new DentalCourseFeeViewModel
                        {
                            Id = MdsFee.Id,
                            CourseName = MdsFee.CourseName,
                            CourseLevel = MdsFee.CourseLevel,
                            AmountToBePaid = MdsFee.AmountToBePaid,
                            CalculationType = MdsFee.CalculationType
                        }
                    };
                }).ToList(),

                OtherFees = otherFees.Select( e => new DentalOtherFeeViewModel
                {
                    Id = e.Id,
                    FeeName = e.FeeName,
                    AmountToBePaid = e.AmountToBePaid,
                    DisplayOrder = e.DisplayOrder,
                }).ToList(),
            };

            return View(model);
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
