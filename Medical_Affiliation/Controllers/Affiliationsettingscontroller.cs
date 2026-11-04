using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Medical_Affiliation.DATA;

namespace Medical_Affiliation.Controllers
{
    // TODO: apply the same admin authorization attribute / base controller
    // your other admin controllers use. Anyone who can open this can change fines and college status.
    public class AffiliationSettingsController : Controller
    {
        // College master: [Admission_Affiliation].[dbo].[Affiliation_College_Master]
        // The [Status] column is expected to be a bit (1 = true, 0 = false).
        private const string CollegeTable = "dbo.Affiliation_College_Master";

        private readonly string _connectionString;

        public AffiliationSettingsController(ApplicationDbContext context)
        {
            _connectionString = context.Database.GetConnectionString()
                ?? throw new InvalidOperationException("The application's database connection string is not configured.");
        }

        // ---------------------------------------------------------------
        // View models / request DTOs
        // ---------------------------------------------------------------
        public class ScheduleRow
        {
            public int Id { get; set; }
            public int FacultyCode { get; set; }
            public string? CourseLevel { get; set; }
            public bool IsEnabled { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? LastDate { get; set; }
            public DateTime? FineLastDate { get; set; }
            public decimal? FineAmount { get; set; }
            public DateTime? UpdatedOn { get; set; }
        }

        public class CollegeRow
        {
            public string CollegeCode { get; set; } = string.Empty;
            public string? CollegeName { get; set; }
            public string? CollegeTown { get; set; }
            public string? FacultyCode { get; set; }
            public bool Status { get; set; }
        }

        public class SettingsPageVm
        {
            public List<ScheduleRow> Schedules { get; set; } = new();
            public List<CollegeRow> Colleges { get; set; } = new();
        }

        public class CollegeStatusRequest
        {
            public string? CollegeCode { get; set; }
            public bool Status { get; set; }
        }

        public class AllCollegeStatusRequest
        {
            public bool Status { get; set; }
        }

        // Reads bit / numeric / "true" / "1" style values safely.
        private static bool ParseStatus(object? value)
        {
            if (value == null || value == DBNull.Value) return false;
            if (value is bool b) return b;

            var text = Convert.ToString(value)?.Trim() ?? string.Empty;
            return text.Equals("1") || text.Equals("true", StringComparison.OrdinalIgnoreCase)
                || text.Equals("yes", StringComparison.OrdinalIgnoreCase)
                || text.Equals("active", StringComparison.OrdinalIgnoreCase);
        }

        // ---------------------------------------------------------------
        // Page
        // ---------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = new SettingsPageVm();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            const string scheduleSql = @"
                SELECT Id, FacultyCode, CourseLevel, IsEnabled, StartDate, LastDate,
                       FineLastDate, FineAmount, UpdatedOn
                FROM dbo.Mst_CourseLevelSchedule
                ORDER BY FacultyCode, CourseLevel";

            using (var cmd = new SqlCommand(scheduleSql, conn))
            using (var r = await cmd.ExecuteReaderAsync())
            {
                while (await r.ReadAsync())
                {
                    vm.Schedules.Add(new ScheduleRow
                    {
                        Id = r.GetInt32(r.GetOrdinal("Id")),
                        FacultyCode = Convert.ToInt32(r["FacultyCode"]),
                        CourseLevel = r["CourseLevel"] as string,
                        IsEnabled = ParseStatus(r["IsEnabled"]),
                        StartDate = r.IsDBNull(r.GetOrdinal("StartDate")) ? null : r.GetDateTime(r.GetOrdinal("StartDate")),
                        LastDate = r.IsDBNull(r.GetOrdinal("LastDate")) ? null : r.GetDateTime(r.GetOrdinal("LastDate")),
                        FineLastDate = r.IsDBNull(r.GetOrdinal("FineLastDate")) ? null : r.GetDateTime(r.GetOrdinal("FineLastDate")),
                        FineAmount = r.IsDBNull(r.GetOrdinal("FineAmount")) ? null : Convert.ToDecimal(r["FineAmount"]),
                        UpdatedOn = r.IsDBNull(r.GetOrdinal("UpdatedOn")) ? null : r.GetDateTime(r.GetOrdinal("UpdatedOn"))
                    });
                }
            }

            // Passwords and other sensitive columns are intentionally NOT selected.
            var collegeSql = $@"
                SELECT CollegeCode, CollegeName, CollegeTown, FacultyCode, [Status]
                FROM {CollegeTable}
                ORDER BY CollegeName";

            using (var cmd = new SqlCommand(collegeSql, conn))
            using (var r = await cmd.ExecuteReaderAsync())
            {
                while (await r.ReadAsync())
                {
                    vm.Colleges.Add(new CollegeRow
                    {
                        CollegeCode = Convert.ToString(r["CollegeCode"]) ?? string.Empty,
                        CollegeName = r["CollegeName"] as string,
                        CollegeTown = r["CollegeTown"] as string,
                        FacultyCode = r.IsDBNull(r.GetOrdinal("FacultyCode")) ? null : Convert.ToString(r["FacultyCode"]),
                        Status = ParseStatus(r["Status"])
                    });
                }
            }

            return View(vm);
        }

        // ---------------------------------------------------------------
        // Mst_CourseLevelSchedule: update (Id > 0) or add (Id = 0)
        // ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSchedule([FromBody] ScheduleRow model)
        {
            if (model == null)
                return Json(new { success = false, message = "Invalid request." });

            var level = model.CourseLevel?.Trim();
            if (string.IsNullOrWhiteSpace(level))
                return Json(new { success = false, message = "Course level is required." });
            if (model.FacultyCode <= 0)
                return Json(new { success = false, message = "Faculty code is required." });
            if (!model.LastDate.HasValue)
                return Json(new { success = false, message = "Last date is required." });
            if (model.StartDate.HasValue && model.StartDate.Value.Date > model.LastDate.Value.Date)
                return Json(new { success = false, message = "Start date cannot be after the last date." });
            if (model.FineLastDate.HasValue && model.FineLastDate.Value.Date < model.LastDate.Value.Date)
                return Json(new { success = false, message = "Fine last date cannot be before the last date." });
            if (model.FineAmount.HasValue && model.FineAmount.Value < 0)
                return Json(new { success = false, message = "Fine amount cannot be negative." });

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            if (model.Id > 0)
            {
                const string sql = @"
                    UPDATE dbo.Mst_CourseLevelSchedule
                    SET FacultyCode = @FacultyCode, CourseLevel = @CourseLevel, IsEnabled = @IsEnabled,
                        StartDate = @StartDate, LastDate = @LastDate, FineLastDate = @FineLastDate,
                        FineAmount = @FineAmount, UpdatedOn = GETDATE()
                    WHERE Id = @Id";

                using var cmd = new SqlCommand(sql, conn);
                AddScheduleParams(cmd, model, level);
                cmd.Parameters.AddWithValue("@Id", model.Id);
                var rows = await cmd.ExecuteNonQueryAsync();

                return Json(rows > 0
                    ? new { success = true, message = "Schedule updated.", id = model.Id }
                    : new { success = false, message = "Row not found.", id = model.Id });
            }

            // Add: one row per faculty + course level
            const string dupSql = @"SELECT COUNT(1) FROM dbo.Mst_CourseLevelSchedule
                                    WHERE FacultyCode = @FacultyCode AND CourseLevel = @CourseLevel";
            using (var dup = new SqlCommand(dupSql, conn))
            {
                dup.Parameters.AddWithValue("@FacultyCode", model.FacultyCode);
                dup.Parameters.AddWithValue("@CourseLevel", level);
                if (Convert.ToInt32(await dup.ExecuteScalarAsync()) > 0)
                    return Json(new { success = false, message = "A schedule for this faculty and course level already exists. Edit that row instead." });
            }

            const string insertSql = @"
                INSERT INTO dbo.Mst_CourseLevelSchedule
                    (FacultyCode, CourseLevel, IsEnabled, StartDate, LastDate, FineLastDate, FineAmount, UpdatedOn)
                OUTPUT INSERTED.Id
                VALUES
                    (@FacultyCode, @CourseLevel, @IsEnabled, @StartDate, @LastDate, @FineLastDate, @FineAmount, GETDATE())";

            using (var cmd = new SqlCommand(insertSql, conn))
            {
                AddScheduleParams(cmd, model, level);
                var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return Json(new { success = true, message = "Schedule added.", id = newId });
            }
        }

        private static void AddScheduleParams(SqlCommand cmd, ScheduleRow m, string level)
        {
            cmd.Parameters.AddWithValue("@FacultyCode", m.FacultyCode);
            cmd.Parameters.AddWithValue("@CourseLevel", level);
            cmd.Parameters.AddWithValue("@IsEnabled", m.IsEnabled);
            cmd.Parameters.AddWithValue("@StartDate", (object?)m.StartDate?.Date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LastDate", m.LastDate!.Value.Date);
            cmd.Parameters.AddWithValue("@FineLastDate", (object?)m.FineLastDate?.Date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FineAmount", (object?)m.FineAmount ?? DBNull.Value);
        }

        // ---------------------------------------------------------------
        // Affiliation_College_Master.[Status]: one college
        // ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetCollegeStatus([FromBody] CollegeStatusRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.CollegeCode))
                return Json(new { success = false, message = "College code is required." });

            var sql = $"UPDATE {CollegeTable} SET [Status] = @Status WHERE CollegeCode = @Code";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Status", req.Status);
            cmd.Parameters.AddWithValue("@Code", req.CollegeCode.Trim());
            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return Json(rows > 0
                ? new { success = true, message = "Status updated." }
                : new { success = false, message = "College not found." });
        }

        // ---------------------------------------------------------------
        // Affiliation_College_Master.[Status]: ALL colleges (true or false)
        // ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAllCollegeStatus([FromBody] AllCollegeStatusRequest req)
        {
            if (req == null)
                return Json(new { success = false, message = "Invalid request." });

            var sql = $"UPDATE {CollegeTable} SET [Status] = @Status";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Status", req.Status);
            await conn.OpenAsync();
            var rows = await cmd.ExecuteNonQueryAsync();

            return Json(new
            {
                success = true,
                message = $"{rows} college(s) set to {(req.Status ? "true" : "false")}.",
                count = rows
            });
        }
    }
}