using Microsoft.Data.SqlClient;
using Rguhs.Portal.Models;

namespace Rguhs.Portal.Services;

public interface ICourseLevelScheduleService
{
    /// <summary>All UG/PG/SS windows of a faculty (levels without a row are returned as open, no deadline).</summary>
    Task<IReadOnlyList<CourseLevelScheduleDto>> GetAsync(int facultyCode, CancellationToken ct = default);

    /// <summary>Server-side guard: true when the level can currently be opened/submitted.</summary>
    Task<bool> IsLevelOpenAsync(int facultyCode, string courseLevel, CancellationToken ct = default);

    /// <summary>
    /// Course levels (UG / PG / SS) this college actually offers, read from the [ug_pg] column of
    /// Mst_MedicalCollegeCourseIntake, always returned in UG, PG, SS order.
    /// </summary>
    Task<IReadOnlyList<string>> GetCollegeLevelsAsync(string collegeCode, CancellationToken ct = default);
}

public sealed class CourseLevelScheduleService : ICourseLevelScheduleService
{
    private static readonly string[] LevelOrder = { "UG", "PG", "SS" };
    private static readonly TimeZoneInfo? IstZone = FindIst();
    private static readonly TimeSpan IstOffset = TimeSpan.FromHours(5.5);

    private readonly string _connectionString;

    /// <param name="connectionString">The already-decrypted connection string (see Program.cs).</param>
    public CourseLevelScheduleService(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is empty.", nameof(connectionString));
        _connectionString = connectionString;
    }

    // ── India time ────────────────────────────────────────────────────────
    private static TimeZoneInfo? FindIst()
    {
        foreach (var id in new[] { "India Standard Time", "Asia/Kolkata" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); } catch { /* try next */ }
        }
        return null;
    }

    public static DateTimeOffset NowIst() =>
        IstZone != null
            ? TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, IstZone)
            : DateTimeOffset.UtcNow.ToOffset(IstOffset);

    public static DateOnly TodayIst() => DateOnly.FromDateTime(NowIst().DateTime);

    /// <summary>A level stays open until 23:59:59 IST of its final date.</summary>
    public static string EndOfDayIst(DateOnly d) => $"{d:yyyy-MM-dd}T23:59:59+05:30";

    // ── Data access ───────────────────────────────────────────────────────
    public async Task<IReadOnlyList<CourseLevelScheduleDto>> GetAsync(int facultyCode, CancellationToken ct = default)
    {
        const string sql = @"
SELECT CourseLevel, IsEnabled, StartDate, LastDate, FineLastDate, FineAmount
FROM   dbo.Mst_CourseLevelSchedule
WHERE  FacultyCode = @FacultyCode;";

        var rows = new Dictionary<string, (bool En, DateOnly? S, DateOnly? L, DateOnly? F, decimal? Amt)>(StringComparer.OrdinalIgnoreCase);

        await using (var con = new SqlConnection(_connectionString))
        await using (var cmd = new SqlCommand(sql, con))
        {
            cmd.Parameters.AddWithValue("@FacultyCode", facultyCode);
            await con.OpenAsync(ct);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
            {
                static DateOnly? D(SqlDataReader x, int i) => x.IsDBNull(i) ? null : DateOnly.FromDateTime(x.GetDateTime(i));
                rows[r.GetString(0).Trim()] = (
                    r.GetBoolean(1), D(r, 2), D(r, 3), D(r, 4),
                    r.IsDBNull(5) ? null : r.GetDecimal(5));
            }
        }

        var today = TodayIst();
        var result = new List<CourseLevelScheduleDto>();

        foreach (var level in LevelOrder)
        {
            rows.TryGetValue(level, out var row);
            var found = rows.ContainsKey(level);
            result.Add(Evaluate(facultyCode, level,
                found ? row.En : true, found ? row.S : null, found ? row.L : null,
                found ? row.F : null, found ? row.Amt : null, today));
        }
        return result;
    }

    public async Task<bool> IsLevelOpenAsync(int facultyCode, string courseLevel, CancellationToken ct = default)
    {
        var level = (courseLevel ?? "").Trim();
        var all = await GetAsync(facultyCode, ct);
        var item = all.FirstOrDefault(x => x.CourseLevel.Equals(level, StringComparison.OrdinalIgnoreCase));
        return item?.IsSelectable ?? false;
    }

    public async Task<IReadOnlyList<string>> GetCollegeLevelsAsync(string collegeCode, CancellationToken ct = default)
    {
        var code = (collegeCode ?? "").Trim();
        if (code.Length == 0) return Array.Empty<string>();

        const string sql = @"
SELECT DISTINCT UPPER(LTRIM(RTRIM(ug_pg)))
FROM   [Admission_Affiliation].[dbo].[Mst_MedicalCollegeCourseIntake]
WHERE  LTRIM(RTRIM(coll_code)) = @CollegeCode
  AND  ug_pg IS NOT NULL;";

        var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var con = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@CollegeCode", code);
        await con.OpenAsync(ct);
        await using var r = await cmd.ExecuteReaderAsync(ct);
        while (await r.ReadAsync(ct))
        {
            var level = NormalizeLevel(r.IsDBNull(0) ? null : r.GetString(0));
            if (level != null) found.Add(level);
        }

        // always UG, PG, SS order
        return LevelOrder.Where(found.Contains).ToList();
    }

    /// <summary>Maps values such as "UG", "Under Graduate", "PG", "Post Graduate", "SS", "Super Speciality" to UG / PG / SS.</summary>
    private static string? NormalizeLevel(string? raw)
    {
        var v = (raw ?? "").Trim().ToUpperInvariant();
        if (v.Length == 0) return null;
        if (v is "UG" or "PG" or "SS") return v;
        if (v.StartsWith("UNDER")) return "UG";
        if (v.StartsWith("POST")) return "PG";
        if (v.StartsWith("SUPER")) return "SS";
        return null;
    }

    // ── Same rule as the SQL status query ─────────────────────────────────
    public static CourseLevelScheduleDto Evaluate(int faculty, string level, bool enabled,
        DateOnly? start, DateOnly? last, DateOnly? fineLast, decimal? fineAmount, DateOnly today)
    {
        var finalEnd = fineLast ?? last;
        string status;
        if (!enabled) status = ScheduleStatus.Disabled;
        else if (finalEnd.HasValue && today > finalEnd.Value) status = ScheduleStatus.Closed;
        else if (fineLast.HasValue && last.HasValue && today > last.Value) status = ScheduleStatus.LateFine;
        else status = ScheduleStatus.Open;

        DateOnly? phaseEnd = status switch
        {
            ScheduleStatus.Open => last ?? fineLast,
            ScheduleStatus.LateFine => fineLast,
            _ => null
        };
        var daysLeft = phaseEnd.HasValue ? Math.Max(0, phaseEnd.Value.DayNumber - today.DayNumber + 1) : 0;

        return new CourseLevelScheduleDto
        {
            FacultyCode = faculty,
            CourseLevel = level,
            IsEnabled = enabled,
            StartDate = start,
            LastDate = last,
            FineLastDate = fineLast,
            FineAmount = fineAmount,
            Status = status,
            DaysLeft = daysLeft
        };
    }
}