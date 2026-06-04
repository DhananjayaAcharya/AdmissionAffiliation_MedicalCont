namespace Medical_Affiliation.Models
{

    public class TalukMappingViewModel
    {
        public string? DistrictId { get; set; }

        public List<DistrictMaster> Districts { get; set; } = new();
    }
    public class DeleteTalukViewModel
    {
        public string TalukId { get; set; } = string.Empty;
    }

    public class SaveTalukMappingViewModel
    {
        public string DistrictId { get; set; } = string.Empty;

        public List<string> SelectedTalukIds { get; set; } = new();
        public List<string> NewTaluks { get; set; } = new();
    }

    public class UpdateTalukViewModel
    {
        public string TalukId { get; set; } = string.Empty;

        public string NewTalukId { get; set; } = string.Empty;

        public string TalukName { get; set; } = string.Empty;
    }
}
