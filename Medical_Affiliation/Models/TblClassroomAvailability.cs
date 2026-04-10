using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class TblClassroomAvailability
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public int? NoOfClassroomsAvailable { get; set; }

    public int? SizeAvailableSqFt { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool? AgreeTerms { get; set; }
}
