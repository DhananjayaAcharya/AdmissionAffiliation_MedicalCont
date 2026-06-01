namespace Medical_Affiliation.Models
{
    public class CollegeLocationViewModel
    {
        public string CollegeCode { get; set; }

        public string CollegeName { get; set; }

        public string? DistrictId { get; set; }

        public string? TalukId { get; set; }

        public List<DistrictMaster> Districts { get; set; } = new();

        public List<TalukMaster> Taluks { get; set; } = new();
    }
}
