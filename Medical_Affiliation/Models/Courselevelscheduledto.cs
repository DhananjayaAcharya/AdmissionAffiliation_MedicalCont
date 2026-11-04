namespace Rguhs.Portal.Models;

/// <summary>Computed state of one course level, mirroring the SQL "StatusToday" rule.</summary>
public static class ScheduleStatus
{
    public const string Open = "Open";
    public const string LateFine = "LateFine";
    public const string Closed = "Closed";
    public const string Disabled = "Disabled";
}

/// <summary>One row of dbo.Mst_CourseLevelSchedule plus its computed status.</summary>
public sealed class CourseLevelScheduleDto
{
    public int FacultyCode { get; init; }
    public string CourseLevel { get; init; } = "";
    public bool IsEnabled { get; init; }

    // Raw dates (India calendar dates, inclusive)
    public DateOnly? StartDate { get; init; }
    public DateOnly? LastDate { get; init; }
    public DateOnly? FineLastDate { get; init; }
    public decimal? FineAmount { get; init; }

    // Computed
    public string Status { get; init; } = ScheduleStatus.Open;
    public bool IsSelectable => Status is ScheduleStatus.Open or ScheduleStatus.LateFine;
    public bool HasDeadline => LastDate.HasValue || FineLastDate.HasValue;
    public DateOnly? FineStartDate => LastDate.HasValue && FineLastDate.HasValue ? LastDate.Value.AddDays(1) : null;
    public int DaysLeft { get; init; }
}

/// <summary>JSON shape returned to the browser (dates as yyyy-MM-dd strings, end moments in IST).</summary>
public sealed class CourseLevelScheduleJson
{
    public string CourseLevel { get; init; } = "";
    public bool IsEnabled { get; init; }
    public string Status { get; init; } = "";
    public bool IsSelectable { get; init; }
    public bool HasDeadline { get; init; }
    public string? StartDate { get; init; }
    public string? LastDate { get; init; }
    public string? FineStartDate { get; init; }
    public string? FineLastDate { get; init; }
    public decimal? FineAmount { get; init; }
    public int DaysLeft { get; init; }

    /// <summary>ISO moment the current phase ends (23:59:59 IST), null when there is no deadline.</summary>
    public string? PhaseEndsAt { get; init; }
    public string? StartsAt { get; init; }
}