using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models;

public class DentalLibraryUserPageViewModel
{
    public string CollegeCode { get; set; } = string.Empty;
    public int FacultyId { get; set; }
    public int AffiliationTypeId { get; set; }
    public string CourseLevel { get; set; } = string.Empty;
    public DentalLibraryUserForm Form { get; set; } = new();
}

public class DentalLibraryUserForm
{
    [Range(0, int.MaxValue)]
    [Display(Name = "Teaching staff")]
    public int NoOfTeachingStaff { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Research scholars / assistants")]
    public int NoOfResearchScholarsAssistants { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Postgraduate students")]
    public int NoOfPostGraduateStudents { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Undergraduate students")]
    public int NoOfUnderGraduateStudents { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Administrative staff")]
    public int NoOfAdministrativeStaff { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Paramedical staff")]
    public int NoOfParaMedicalStaff { get; set; }

    [Range(0, int.MaxValue)]
    [Display(Name = "Outsiders")]
    public int NoOfOutsiders { get; set; }

    [Display(Name = "Provide user education programmes")]
    public bool ProvideUserEducationProgrammes { get; set; }
}
