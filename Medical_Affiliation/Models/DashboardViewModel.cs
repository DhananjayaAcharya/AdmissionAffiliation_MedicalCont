namespace Medical_Affiliation.Models
{
    public class DashboardViewModel
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }

        public string? DistrictId { get; set; }
        public string? TalukId { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public List<DistrictMaster> Districts { get; set; } = new();
    }
}
