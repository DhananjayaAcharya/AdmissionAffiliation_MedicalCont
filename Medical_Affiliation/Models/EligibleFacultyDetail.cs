using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class EligibleFacultyDetail
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string Designation { get; set; } = null!;

    public int DesignationSequence { get; set; }

    public int? NumberOfFaculty { get; set; }

    public string? Names { get; set; }

    public int? TotalAdmissionSeats { get; set; }

    public string? IsAdequateForAdmission { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
}
