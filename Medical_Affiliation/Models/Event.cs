using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class Event
{
    public int EventId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? FacultyId { get; set; }

    public int EventCategoryId { get; set; }

    public DateOnly EventDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public DateTime? ApplicationStartDate { get; set; }

    public DateTime? ApplicationEndDate { get; set; }

    public string Status { get; set; } = null!;

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual AppUser CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<EventAssignment> EventAssignments { get; set; } = new List<EventAssignment>();

    public virtual EventCategory EventCategory { get; set; } = null!;

    public virtual Faculty? Faculty { get; set; }

    public virtual AppUser? UpdatedByNavigation { get; set; }
}
