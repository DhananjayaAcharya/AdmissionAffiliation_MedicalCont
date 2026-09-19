using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class ApplicationSubmission
{
    public int Id { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string CollegeCode { get; set; } = null!;

    public string CourseCode { get; set; } = null!;

    public string TypeOfAffiliation { get; set; } = null!;

    public string CourseLevel { get; set; } = null!;

    public string RegistrationNumber { get; set; } = null!;

    public DateTime CreatedOn { get; set; }
}
