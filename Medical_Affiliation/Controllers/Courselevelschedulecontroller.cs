using Microsoft.AspNetCore.Mvc;
using Rguhs.Portal.Models;
using Rguhs.Portal.Services;

namespace Rguhs.Portal.Controllers;

/// <summary>
/// Single source of truth for UG / PG / SS submission windows.
/// The dashboard (countdown + notification) and the sidebar (level availability)
/// both read from here, so changing a row in dbo.Mst_CourseLevelSchedule updates everything.
/// </summary>
[Route("CourseLevelSchedule")]
public class CourseLevelScheduleController : Controller
{
    private readonly ICourseLevelScheduleService _schedule;
    private readonly ILogger<CourseLevelScheduleController> _logger;
    private readonly IWebHostEnvironment _env;

    public CourseLevelScheduleController(ICourseLevelScheduleService schedule, ILogger<CourseLevelScheduleController> logger, IWebHostEnvironment env)
    {
        _schedule = schedule;
        _logger = logger;
        _env = env;
    }

    /// <summary>GET /CourseLevelSchedule/GetSchedule[?facultyCode=1]  (defaults to the logged-in faculty)</summary>
    [HttpGet("GetSchedule")]
    public async Task<IActionResult> GetSchedule(int? facultyCode, CancellationToken ct)
    {
        var sessionFaculty = HttpContext.Session.GetString("FacultyCode");
        if (string.IsNullOrEmpty(sessionFaculty))
            return Unauthorized(new { success = false, message = "Session expired." });

        var faculty = facultyCode ?? (int.TryParse(sessionFaculty, out var f) ? f : 0);
        if (faculty <= 0)
            return BadRequest(new { success = false, message = "Invalid faculty." });

        try
        {
            var items = await _schedule.GetAsync(faculty, ct);

            // Levels this college offers (Mst_MedicalCollegeCourseIntake.ug_pg). A failure here must not
            // hide the deadline data, so it is reported with a flag instead of a 500.
            IReadOnlyList<string> collegeLevels = Array.Empty<string>();
            var collegeLevelsFailed = false;
            try
            {
                collegeLevels = await _schedule.GetCollegeLevelsAsync(HttpContext.Session.GetString("CollegeCode") ?? "", ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                collegeLevelsFailed = true;
                _logger.LogError(ex, "Failed to load course levels for college {College}", HttpContext.Session.GetString("CollegeCode"));
            }

            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";

            return Json(new
            {
                success = true,
                facultyCode = faculty,
                serverNow = CourseLevelScheduleService.NowIst().ToString("yyyy-MM-dd'T'HH:mm:ssK"),
                today = CourseLevelScheduleService.TodayIst().ToString("yyyy-MM-dd"),
                levels = items.Select(ToJson),
                collegeLevels,
                collegeLevelsFailed
            });
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load course level schedule for faculty {Faculty}", faculty);
            // The real reason is only sent to the browser in Development (see the Network tab > Response)
            return StatusCode(500, new
            {
                success = false,
                message = "Unable to load submission schedule.",
                detail = _env.IsDevelopment() ? ex.Message : null
            });
        }
    }

    private static CourseLevelScheduleJson ToJson(CourseLevelScheduleDto d)
    {
        static string? Ymd(DateOnly? x) => x?.ToString("yyyy-MM-dd");

        DateOnly? phaseEnd = d.Status switch
        {
            ScheduleStatus.Open => d.LastDate ?? d.FineLastDate,
            ScheduleStatus.LateFine => d.FineLastDate,
            _ => null
        };

        return new CourseLevelScheduleJson
        {
            CourseLevel = d.CourseLevel,
            IsEnabled = d.IsEnabled,
            Status = d.Status,
            IsSelectable = d.IsSelectable,
            HasDeadline = d.HasDeadline,
            StartDate = Ymd(d.StartDate),
            LastDate = Ymd(d.LastDate),
            FineStartDate = Ymd(d.FineStartDate),
            FineLastDate = Ymd(d.FineLastDate),
            FineAmount = d.FineAmount,
            DaysLeft = d.DaysLeft,
            PhaseEndsAt = phaseEnd.HasValue ? CourseLevelScheduleService.EndOfDayIst(phaseEnd.Value) : null,
            StartsAt = d.StartDate.HasValue ? $"{d.StartDate:yyyy-MM-dd}T00:00:00+05:30" : null
        };
    }
}