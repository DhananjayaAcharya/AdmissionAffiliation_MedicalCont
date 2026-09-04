using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public class DentalFeeStructureViewModel
{
    public int FacultyCode { get; set; }

    public int AffiliationTypeId { get; set; }

    public string? AffiliationCategory { get; set; }

    public List<DentalFeeTypeRowViewModel> FeeTypes { get; set; }
        = new();

    public List<DentalOtherFeeViewModel> OtherFees { get; set; }
        = new();
}

public class DentalFeeTypeRowViewModel
{
    public int FeeTypeId { get; set; }

    public string FeeType { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public DentalCourseFeeViewModel? BDS { get; set; }

    public DentalCourseFeeViewModel? MDS { get; set; }
}

public class DentalCourseFeeViewModel
{
    public int? Id { get; set; }

    public string CourseName { get; set; } = null!;

    public int? CourseCode { get; set; }

    public string? CourseLevel { get; set; }

    public decimal? AmountToBePaid { get; set; }

    public string? CalculationType { get; set; }
}

public class DentalOtherFeeViewModel
{
    public int? Id { get; set; }

    public string FeeName { get; set; } = null!;

    public decimal AmountToBePaid { get; set; }

    public int DisplayOrder { get; set; }
}