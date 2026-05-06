namespace Medical_Affiliation.Models
{
    public class CA_SS_SSCourseParticularsVM
    {
        public List<CA_SS_LOPSavedDateVM> LopList { get; set; } = new();

        public List<CA_SS_PermissionVM> PermissionList { get; set; } = new();

        public List<CA_SS_LICPreviousInspectionVM> LICInspections { get; set; } = new();

        public List<CA_SS_AffiliationGrantedYearVM> AffiliationList { get; set; } = new();

        public List<CA_SS_OtherCoursesConductedVM> OtherCourses { get; set; } = new();


    }

    public class CA_SS_LOPSavedDateVM
    {
        public int CourseCode { get; set; }
        public string CourseName { get; set; }

        public int SanctionedIntake { get; set; }

        public DateTime? LopDate { get; set; }
        public DateTime? RecognitionDate { get; set; }
    }

    public class CA_SS_PermissionVM
    {
        public int Id { get; set; }
        public int CourseCode { get; set; }
        public string CourseName { get; set; }

        public string? PermissionStatus { get; set; } // Yes / No

        public IFormFile? PermissionFile { get; set; }

        public string? ExistingFileName { get; set; }
        public bool HasFile { get; set; }
    }

    public class CA_SS_AffiliationGrantedYearVM
    {
        public int CourseCode { get; set; }
        public string CourseName { get; set; }

        public DateTime? AffiliationDate { get; set; }

        public int SanctionedIntake { get; set; }

        public IFormFile? SupportingDoc { get; set; }

        public string? ExistingFileName { get; set; }

        public int Id { get; set; }              // ✅ ADD THIS

        public string? FileName { get; set; }    // ✅ ADD THIS
        public string? FilePath { get; set; }    // ✅ ADD THIS

        public bool HasFile { get; set; }        // ✅ ADD THIS
    }

    public class CA_SS_OtherCoursesConductedVM
    {
        public int Id { get; set; }           // ✅ Add this
        public string CourseName { get; set; }

        public string SanctionedIntake { get; set; }

        public IFormFile? SupportingDoc { get; set; }

        public string? ExistingFileName { get; set; }

        public List<string> CourseList { get; set; } = new();

    }

    public class CA_SS_LICPreviousInspectionVM
    {
        public int Id { get; set; }

        public string CollegeCode { get; set; }

        public string CourseName { get; set; }

        public DateTime? InspectionDate { get; set; }

        public string? ActionTaken { get; set; }
    }


}

