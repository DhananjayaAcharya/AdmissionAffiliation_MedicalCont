namespace Medical_Affiliation.Models
{
    public class CA_SS_FullViewVM
    {
        // ===== Applied Courses =====
        public string? CollegeCode { get; set; }
        public string? FacultyCode { get; set; }

        public string CautionMessage { get; set; } = "";

        public List<SSCourseRow> DMCourses { get; set; } = new();
        public List<SSCourseRow> MChCourses { get; set; } = new();


        // ===== Course Particulars =====

        public List<CA_SS_LOPSavedDateVM> LopList { get; set; } = new();

        public List<CA_SS_PermissionVM> PermissionList { get; set; } = new();

        public List<CA_SS_LICPreviousInspectionVM> LICInspections { get; set; } = new();

        public List<CA_SS_AffiliationGrantedYearVM> AffiliationList { get; set; } = new();

        public List<CA_SS_OtherCoursesConductedVM> OtherCourses { get; set; } = new();
    }
}

