using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class AHShospitalDetailsViewModel
    {
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }
        public bool IsParentMedicalCollegeHospitalExists { get; set; }
        public bool IsParentHospitalExists { get; set; }
        public string HospitalName { get; set; }

        public string SelectedDistrictId { get; set; }
        public string SelectedTalukId { get; set; }
        public List<SelectListItem> HospitalDistrict {  get; set; }
        public List<SelectListItem> HospitalTaluk {  get; set; }
        public string HospitalLocation { get; set; }
        public string HospitalStateId { get; set; }
        public int? OpdPerDay { get; set; }
        public int? IpdPerDay { get; set; }
        public int? TotalBedsAvailable { get; set; }
        public bool IsParentOraffHospitalforotherAHsInstitution { get; set; }
        public int CollegeToHospitalDistance { get; set; }
        public bool IsManagedbyTrustMember { get; set; }
        public bool IsOwnerATrustMember { get; set; }
        public AffiliatedHospitalDetailsViewModel AffHospitalViewModel { get; set; } = new ();
    }

    public class AffiliatedHospitalDetailsViewModel
    {
        public int HospitalId { get; set; }
        public string AffiliatedHospitalName { get; set; }
        public int? TotalNoOfBeds { get; set; }
        public byte[] PermissionLetter { get; set; }
        public IFormFile PermissionLetterFile { get; set; }

    }

    public class AhsTeachingFactultyDetails
    {
        public int facId { get; set; }
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
        public string TeachingFacultyName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string AadharNumber { get; set; }
        public string PanNumber { get; set; }
        public IFormFile AadharFile { get; set; }
        public IFormFile PanFile { get; set; }
        public IFormFile Form16_bankStatement { get; set; }
        public bool HasAdharFile { get; set; }
        public bool HasPanFile { get; set; }
        public bool HasForm16File { get; set; }

        public byte[] AdharFileBytes { get; set; }
        public byte[] PanFileBytes { get; set; }
        public byte[] Form16_bankStatementBytes { get; set; }
        public bool RecognizedPgGuide { get; set; }
        public List<SelectListItem> FacultyDepartment {  get; set; }
        public List<SelectListItem> FacultyDesignation { get; set; }
        public string selectedFacultyDepartment { get; set; }
        public string selectedFacultyDesignation { get; set; }
        public string Qualification {  get; set; }
        public string UgUniversityName { get; set; }
        public string PgUniverstityName { get; set; }
        public List<SelectListItem> UGPassingYearList { get; set; }
        public List<SelectListItem> PGPassingYearList { get; set; }
        public string selectedUgYear { get; set; }
        public string selectedPgYear { get; set; }
        public string TeachingExpAfterUg { get; set; }
        public string TeachingExpAfterPg { get; set; }
        public DateOnly? DateOfJoining { get; set; }

    }
}
