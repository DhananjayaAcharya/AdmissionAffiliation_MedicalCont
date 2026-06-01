namespace Medical_Affiliation.Models
{

    public class TalukMappingViewModel
    {
        public string? DistrictId { get; set; }

        public List<DistrictMaster> Districts { get; set; } = new();
    }

    public class SaveTalukMappingViewModel
    {
        public string DistrictId { get; set; } = string.Empty;

        public List<string> SelectedTalukIds { get; set; } = new();
        public List<string> NewTaluks { get; set; } = new();
    }
}
