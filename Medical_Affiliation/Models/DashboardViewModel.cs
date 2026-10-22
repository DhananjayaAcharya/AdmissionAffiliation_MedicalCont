namespace Medical_Affiliation.Models
{
    public class DashboardViewModel
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }

        // Profile fields surfaced to the dashboard
        public string? FacultyName { get; set; }
        public string? CollegeLogoPath { get; set; }
        public string? PrincipalName { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Website { get; set; }
        public int? EstablishedYear { get; set; }

        public string? DistrictId { get; set; }
        public string? TalukId { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public List<DistrictMaster> Districts { get; set; } = new();

        public List<DashboardLateSubmissionFeeViewModel> LateSubmissionFees { get; set; } = new();
    }

    public class DashboardLateSubmissionFeeViewModel
    {
        public int Id { get; set; }
        public int AffiliationTypeId { get; set; }
        public string? CourseLevel { get; set; }
        public DateTime? ActivationDate { get; set; }
        public bool IsActive { get; set; }
    }
}
