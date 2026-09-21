using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class HospitalTieUpDetail
{
    public int Id { get; set; }

    public int HospitalDetailsId { get; set; }

    public string? CollegeCode { get; set; }

    public int FacultyCode { get; set; }

    public string TieUpType { get; set; } = null!;

    public string? HospitalName { get; set; }

    public string? HospitalAddress { get; set; }

    public string? TieUpDetails { get; set; }

    public string? SupportingDocumentPath { get; set; }

    public string? SupportingDocumentName { get; set; }

    public string? SupportingDocumentContentType { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public string? CourseLevel { get; set; }

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual HospitalDetailsForAffiliation HospitalDetails { get; set; } = null!;
}
