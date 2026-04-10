namespace Medical_Affiliation.Models
{
    public class HospitalBlockViewModel
    {
        public int? ParentHospitalId { get; set; }
        public string ParentHospitalName { get; set; }
        public string ParentHospitalAddress { get; set; }
        public string HospitalOwnerName { get; set; }
        public string Kpmebeds { get; set; }
        public string PostBasicBeds { get; set; }
        public string TotalBeds { get; set; }

        public IFormFile ParentHospitalKpmebedsDocFile { get; set; }
        public IFormFile ParentHospitalMoudocFile { get; set; }
        public IFormFile ParentHospitalOwnerNameDocFile { get; set; }
        public IFormFile ParentHospitalPostBasicDocFile { get; set; }

        public List<YearwiseMaterialViewModel> Materials { get; set; } = new();
    }

    public class YearwiseMaterialsFormViewModel
    {
        public List<HospitalBlockViewModel> Hospitals { get; set; } = new();
    }
}
