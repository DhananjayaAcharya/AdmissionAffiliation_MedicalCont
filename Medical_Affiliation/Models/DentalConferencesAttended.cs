using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DentalConferencesAttended
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public string? CourseLevel { get; set; }

    public int? TypeId { get; set; }

    public string ConferenceName { get; set; } = null!;

    public string ConferencePlace { get; set; } = null!;

    public DateOnly ConferenceDate { get; set; }

    public int StudentParticipants { get; set; }

    public int TeacherParticipants { get; set; }

    public int TotalParticipants { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual TypeOfAffiliation? Type { get; set; }
}
