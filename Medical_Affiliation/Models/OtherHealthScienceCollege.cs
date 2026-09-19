using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class OtherHealthScienceCollege
{
    public int Id { get; set; }

    public int FacultyId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public string OtherCollegeCode { get; set; } = null!;

    public int CourseCode { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty Faculty { get; set; } = null!;

    public virtual AffiliationCollegeMaster OtherCollegeCodeNavigation { get; set; } = null!;
}
