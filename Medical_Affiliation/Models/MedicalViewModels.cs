namespace Medical_Affiliation.Models
{
    public class MedicalViewModels
    {
        public class MedicalDataInstituteDetailViewModel
        {
            public int Id { get; set; }

            public string CollegeCode { get; set; } = null!;

            public string FacultyCode { get; set; } = null!;

            public string InstituteName { get; set; } = null!;

            public string InstituteAddress { get; set; } = null!;

            public string? TrustSocietyName { get; set; }

            public string? YearOfEstablishmentOfTrust { get; set; }

            public string? YearOfEstablishmentOfCollege { get; set; }

            public string? InstitutionType { get; set; }

            public string? HodofInstitution { get; set; }

            public DateOnly? Dob { get; set; }

            public string? Age { get; set; }

            public string? TeachingExperience { get; set; }

            public List<string> Degree { get; set; } = new List<string>();

            public string? CourseSelectedSpecialities { get; set; }

            public string Qualifications { get; set; } = null!;

            public string HighestQualification { get; set; } = null!;
            public string? CourseCode { get; set; }
            public bool IsReadOnly { get; set; }
        }

        // Used by Medical_YearwiseMaterials POST/GET
        public class YearwiseMaterialViewModel
        {
            public int ParametersId { get; set; }
            public string ParametersName { get; set; }
            public string CollegeCode { get; set; }
            public string FacultyCode { get; set; }

            // controller treats years as strings (often "0"), keep as string for safe binding
            public string Year1 { get; set; } = "0";
            public string Year2 { get; set; } = "0";
            public string Year3 { get; set; } = "0";
        }

        // Lightweight DTO used in the GET projection (read-only display)
        public class Nursing_YearwiseMaterialsDataViewModel
        {
            public int ParametersId { get; set; }
            public string ParametersName { get; set; }
            public string CollegeCode { get; set; }
            public string FacultyCode { get; set; }
            public string Year1 { get; set; } = "0";
            public string Year2 { get; set; } = "0";
            public string Year3 { get; set; } = "0";
        }

        // TeachingFaculty view model used by TeachingFacultyDetails GET/POST
        public class TeachingFacultyViewModel
        {
            public string CollegeCode { get; set; }
            public string DepartmentCode { get; set; }
            public string DepartmentName { get; set; }
            public string FacultyCode { get; set; }
            public string Faculty { get; set; }

            public string DesignationCode { get; set; }
            public string DesignationName { get; set; }

            // keep as string because DB's SeatSlabId appears to be stored as string in CollegeDesignationDetails
            public string SeatSlabId { get; set; }

            // numeric/int-like fields are stored as strings in controller/database, keep strings to match
            public string ExistingSeatIntake { get; set; }
            public string PresentSeatIntake { get; set; }
        }

        // Typed view model for the Yearwise page used for downloads and display (GET projection)
        public class YearwiseDocViewModel
        {
            public int Id { get; set; }                       // DB row id in NursingAffiliatedYearwiseMaterialsData
            public int ParametersId { get; set; }
            public string ParametersName { get; set; }
            public string CollegeCode { get; set; }
            public string FacultyCode { get; set; }
            public string Year1 { get; set; } = "0";
            public string Year2 { get; set; } = "0";
            public string Year3 { get; set; } = "0";

            public string ParentHospitalName { get; set; }
            public string ParentHospitalAddress { get; set; }
            public string HospitalOwnerName { get; set; }
            public string HospitalType { get; set; }

            public bool ParentHospitalMoudocPresent { get; set; }
            public bool ParentHospitalKpmebedsDocPresent { get; set; }
            public bool ParentHospitalOwnerNameDocPresent { get; set; }
            public bool ParentHospitalPostBasicDocPresent { get; set; }
        }
    }
}
