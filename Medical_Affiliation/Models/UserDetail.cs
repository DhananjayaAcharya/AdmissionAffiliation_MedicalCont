using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class UserDetail
{
    public int UserDetailsId { get; set; }

    public string CollegeCode { get; set; } = null!;

    public int FacultyId { get; set; }

    public int TypeId { get; set; }

    public string? CourseLevel { get; set; }

    public int? NoOfTeachingStaff { get; set; }

    public int? NoOfResearchScholarsAssistants { get; set; }

    public int? NoOfPostGraduateStudents { get; set; }

    public int? NoOfUnderGraduateStudents { get; set; }

    public int? NoOfAdministrativeStaff { get; set; }

    public int? NoOfParaMedicalStaff { get; set; }

    public int? NoOfOutsiders { get; set; }

    public bool? ProvideUserEducationProgrammes { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual AffiliationCollegeMaster CollegeCodeNavigation { get; set; } = null!;

    public virtual Faculty Faculty { get; set; } = null!;

    public virtual TypeOfAffiliation Type { get; set; } = null!;
}
