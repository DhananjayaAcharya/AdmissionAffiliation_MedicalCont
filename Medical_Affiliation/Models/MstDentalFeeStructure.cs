using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MstDentalFeeStructure
{
    public int Id { get; set; }

    public int FacultyCode { get; set; }

    public int FeeTypeId { get; set; }

    public string CourseName { get; set; } = null!;

    public int? CourseCode { get; set; }

    public string? CourseLevel { get; set; }

    public decimal AmountToBePaid { get; set; }

    public string? CalculationType { get; set; }

    public int? AffiliationTypeId { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual MstAffiliationType? AffiliationType { get; set; }

    public virtual Faculty FacultyCodeNavigation { get; set; } = null!;

    public virtual MstDentalFeeType FeeType { get; set; } = null!;
}
