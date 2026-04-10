using System;
using System.Collections.Generic;

namespace Medical_Affiliation.Models;

public partial class MedicalStudentPracticalLab
{
    public int Id { get; set; }

    public bool HistologyAvailable { get; set; }

    public bool HistologyShared { get; set; }

    public bool ClinicalPhysiologyAvailable { get; set; }

    public bool ClinicalPhysiologyShared { get; set; }

    public bool BiochemistryAvailable { get; set; }

    public bool BiochemistryShared { get; set; }

    public bool HistopathCytopathAvailable { get; set; }

    public bool HistopathCytopathShared { get; set; }

    public bool ClinPathHemeAvailable { get; set; }

    public bool ClinPathHemeShared { get; set; }

    public bool MicrobiologyAvailable { get; set; }

    public bool MicrobiologyShared { get; set; }

    public bool ClinicalPharmAvailable { get; set; }

    public bool ClinicalPharmShared { get; set; }

    public bool CalPharmAvailable { get; set; }

    public bool CalPharmShared { get; set; }

    public bool AllLabsHaveAv { get; set; }

    public bool AllLabsHaveInternet { get; set; }

    public bool TechnicalStaffFacilitiesEnsured { get; set; }

    public string FacultyCode { get; set; } = null!;

    public string? CollegeCode { get; set; }

    public string? CourseLevel { get; set; }
}
