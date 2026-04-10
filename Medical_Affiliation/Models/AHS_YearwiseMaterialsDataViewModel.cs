using Medical_Affiliation.Models;

namespace Medical_Affiliation.Models
{
    public class AHS_YearwiseMaterialsDataViewModel
    {
        public int Id { get; set; }
        public string CollegeCode { get; set; }
        public string FacultyCode { get; set; }
        public int ParametersId { get; set; }
        public string ParametersName { get; set; }
        public string HospitalType { get; set; }
        public string Year1 { get; set; }
        public string Year2 { get; set; }
        public string Year3 { get; set; }

        public string? ParentHospitalName { get; set; }
        public string? ParentHospitalAddress { get; set; }

        // Display file names or metadata
        public string? ParentHospitalMoudocName { get; set; }
        public string? ParentHospitalOwnerNameDocName { get; set; }
        public string? ParentHospitalKpmebedsDocName { get; set; }
        public string? ParentHospitalPostBasicDocName { get; set; }

        // For handling uploads
        public IFormFile? ParentHospitalMoudocFile { get; set; }
        public IFormFile ParentHospitalOwnerNameDocFile { get; set; }
        public IFormFile ParentHospitalKpmebedsDocFile { get; set; }
        public IFormFile? ParentHospitalPostBasicDocFile { get; set; }

        public string? Kpmebeds { get; set; }
        public string? PostBasicBeds { get; set; }
        public string? TotalBeds { get; set; }
        public string? HospitalOwnerName { get; set; }


        // Optional: a list for dropdowns or selectable options
        public List<string>? ParameterOptions { get; set; }
        public string? HospitalId { get; set; }
        public List<YearwiseMaterialViewModel> YearwiseMaterials { get; set; }
        public byte[] ParentHospitalMoudocAvailable { get; internal set; }
        public byte[] ParentHospitalOwnerNameDocAvailable { get; internal set; }
        public byte[] ParentHospitalKpmebedsDocAvailable { get; internal set; }
        public byte[] ParentHospitalPostBasicDocAvailable { get; internal set; }
    }

  
}
