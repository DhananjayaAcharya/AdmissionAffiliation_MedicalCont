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
    }
}
