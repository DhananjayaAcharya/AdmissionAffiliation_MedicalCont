using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class DeptWisePublication
{
    public int Id { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyCode { get; set; }

    public string DeptCode { get; set; } = null!;

    public string DeptName { get; set; } = null!;

    public int PublicationsCount { get; set; }

    public string? PublicationPath { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;
}
