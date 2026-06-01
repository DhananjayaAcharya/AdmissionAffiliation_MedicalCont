namespace Medical_Affiliation.Models
{
    // ── Main dashboard page view model ────────────────────────────
    public class MedicalDashboardViewModel
    {
        public List<CollegeListItem> Colleges { get; set; } = new();
        public List<DistrictItem> Districts { get; set; } = new();
        public DashboardStats Stats { get; set; } = new();
        public string SearchTerm { get; set; } = "";
        public string SelectedDistrict { get; set; } = "";
        public string SelectedTaluk { get; set; } = "";
    }

    // ── College row shown in the main table ───────────────────────
    public class CollegeListItem
    {
        public string CollegeCode { get; set; } = "";
        public string CollegeName { get; set; } = "";
        public string CollegeTown { get; set; } = "";
        public string DistrictId { get; set; } = "";
        public string DistrictName { get; set; } = "";
        public string TalukId { get; set; } = "";
        public string TalukName { get; set; } = "";
        public int UGSeats { get; set; }
        public int PGSeats { get; set; }
        public int TotalSeats => UGSeats + PGSeats;
    }

    // ── District dropdown item ─────────────────────────────────────
    public class DistrictItem
    {
        public string DistrictID { get; set; } = "";
        public string DistrictName { get; set; } = "";
    }

    // ── Taluk dropdown item ────────────────────────────────────────
    //public class TalukItem
    //{
    //    public string TalukID { get; set; } = "";
    //    public string TalukName { get; set; } = "";
    //}

    // ── Stats strip numbers ────────────────────────────────────────
    public class DashboardStats
    {
        public int TotalColleges { get; set; }
        public int TotalUGSeats { get; set; }
        public int TotalPGSeats { get; set; }
        public int DistrictsCovered { get; set; }
    }

    // ── AJAX response: taluk list ──────────────────────────────────
    public class TalukResult
    {
        public bool Success { get; set; }
        public List<TalukItem> Taluks { get; set; } = new();
    }

    // ── AJAX response: college detail (modal) ─────────────────────
    public class CollegeDetailResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public CollegeDetailData? Data { get; set; }
    }

    public class CollegeDetailData
    {
        public string CollegeCode { get; set; } = "";
        public string CollegeName { get; set; } = "";
        public string CollegeTown { get; set; } = "";
        public int TotalCourses { get; set; }
        public int TotalIntake { get; set; }
        public int UGIntake { get; set; }
        public int PGIntake { get; set; }
        public List<CourseRow> UGCourses { get; set; } = new();
        public List<CourseRow> PGCourses { get; set; } = new();
    }

    public class CourseRow
    {
        public string CourseName { get; set; } = "";
        public string CourseLevel { get; set; } = "";   // "UG" or "PG"
        public int PresentIntake { get; set; }
        public int ExistingIntake { get; set; }
    }
}
