using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{

    public class SectionViewModel
    {
        public string Name { get; set; }                // e.g., "ClinicalSection"
        public List<string> SubSections { get; set; } = new();
    }
    public class ClinicalHospitalViewModel
    {
        public ClinicalHospitalFormVM Form { get; set; } = new();
        public HospitalFacilitiesViewModel HospitalFacilities { get; set; } = new();

        // Dropdowns only
        public List<DropdownItem> ParentMedicalCollegeExistsOptions { get; set; } = new();
        public List<DropdownItem> HospitalTypes { get; set; } = new();
        public List<DropdownItem> HospitalOwnedByOptions { get; set; } = new();
        public List<DropdownItem> Districts { get; set; } = new();
        public List<TalukItem> Taluks { get; set; } = new();
        public List<string> Locations { get; set; } = new();
        public List<DropdownItem> IsParentHospitalForOtherNursingInstitutionOptions { get; set; } = new();
        public List<DropdownItem> IsOwnerAmemberOfTrustOptions { get; set; } = new();
        // Facilities (for checkboxes, optional if you move to separate partial)


        public List<DropdownItem> AvailableFacilities { get; set; } = new();
        public List<int> SelectedFacilityIds { get; set; } = new();
        public bool IsSupportingDocExists { get; set; }
        public IFormFile? SupportingDoc { get; set; }

    }

    public class DropdownItem
    {
        public string Text { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    public class TalukItem
    {
        public string TalukID { get; set; } = string.Empty;
        public string TalukName { get; set; } = string.Empty;
        public string DistrictID { get; set; } = string.Empty;
    }

    public class AffiliatedHospitalDocumentsViewModel
    {
        [Required]
        public string CollegeCode { get; set; } = string.Empty;

        [Required]
        public string HospitalType { get; set; } = string.Empty;

        [Required]
        public string HospitalName { get; set; } = string.Empty;

        [Range(1, 20000, ErrorMessage = "Total Beds must be within 1 and 20000")]
        public int TotalBeds { get; set; }

        [ValidateNever]
        public IFormFile? DocumentFile { get; set; }
        public bool DocumentExists { get; set; } = false;

        public int? DocumentId { get; set; }

        public string? DocumentName { get; set; }

    }

    public class AffiliatedHospitalDocumentsPostVM
    {
        public string CollegeCode { get; set; } = null!;
        //public int HospitalDetailsId { get; set; }

        [Required]
        public List<AffiliatedHospitalDocumentItemVM> Documents { get; set; } = new();
    }

    public class AffiliatedHospitalDocumentItemVM
    {
        //[Required]
        public int? DocumentId { get; set; }   // null = new row

        [Required]
        public string? HospitalType { get; set; }

        [Required]
        public string? HospitalName { get; set; }

        [Required]
        public int? TotalBeds { get; set; }

        public IFormFile? DocumentFile { get; set; }

        public bool DocumentExists { get; set; }
    }


    public class FieldPracticeAreaViewModel
    {

        public string CollegeCode { get; set; }
        public int FacultyCode { get; set; }
        // 🔹 Dropdown source
        [ValidateNever]
        public List<DropdownItem> FieldTypeOptions { get; set; } = new();
        [ValidateNever]
        public List<DropdownItem> AdopAffTypeOptions { get; set; }
        [ValidateNever]
        public List<DropdownItem> AdministrationTypeOptions { get; set; }
        // ---------------- SELECTED VALUES ----------------

        [Required]
        public int? SelectedFieldTypeId { get; set; }

        [Required]
        public int? SelectedPlanningTypeId { get; set; }

        [Required]
        public int? SelectedAdministrationTypeId { get; set; }

        public string AdminType { get; set; } = string.Empty;
        public string AdopAffType { get; set; } = string.Empty;
        public string FieldType { get; set; } = string.Empty;


        [Required]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name of CHC must be between 2 and 200 characters")]
        public string NameOfCHC { get; set; }

        [Required]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Distance must be numberic")]
        public string DistanceFromNursingInstitution { get; set; }

        [StringLength(500, MinimumLength = 2, ErrorMessage = "Services must be between 2 and 500 characters")]
        public string ServicesRendered { get; set; }
    }

    public class HospitalDocumentsToBeUploadedViewModel
    {
        public string CollegeCode { get; set; }
        public int HospitalDetailsId { get; set; }

        public string HospitalName { get; set; }

        [Required]
        public int? DocumentId { get; set; }
        [Required]
        public string DocumentName { get; set; }

        [Required]
        public IFormFile? DocumentFile { get; set; }
        [Required]
        public bool DocumentExists { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Certificate Number must be between 2 and 500 characters")]
        public string CertificateNumber { get; set; }
    }



    public class HospitalAffiliationCompositeViewModel
    {
        public ClinicalHospitalViewModel ClinicalHospitalDetails { get; set; } = new();

        public ClinicalCapacityViewModel ClinicalCapacity { get; set; }

        public HospitalFacilitiesViewModel HospitalFacilities { get; set; }
        //public List<AffiliatedHospitalDocumentsViewModel> HospitalDocuments { get; set; }
        //public List<FieldPracticeAreaViewModel> FieldPracticeAreas { get; set; }
        public List<HospitalDocumentsToBeUploadedViewModel> HospitalDocumentsToBeUploadedList { get; set; }
        public AffiliatedHospitalDocumentsPostVM AffiliatedDocumentsPostVM { get; set; }

        public FieldPracticeAreasPostViewModel FieldPracticeAreaPostVM { get; set; }

        public IndoorDepartmentRequirementsPostVM IndoorDepartment { get; set; }
        public OTRequirementsPostVM OTRequirements { get; set; }
        public CasualityRequirementsPostVM CasualityRequirements { get; set; }
        public CSSDandLaundryRequirementsPostVM CSSDandLaundryRequirements { get; set; }
        public RadioDiagnosisRequirementsPostVM RadioDiagnosisRequirements { get; set; }
        public AnaesthesiologyRequirementsPostVM AnaesthesiologyRequirements { get; set; }
        public CentralLaboratoryRequirementsPostVM CentralLaboratoryRequirements { get; set; }
        public BloodBankRequirementsPostVM BloodBankRequirements { get; set; }
        public YogaRequirementsPostVM YogaRequirements { get; set; }
        public RadiationOncologyRequirementsPostVM RadiationOncologyRequirements { get; set; }
        public ArtCenterRequirementsPostVM ArtCenterRequirements { get; set; }
        public PharmacyRequirementsPostVM PharmacyRequirements { get; set; }
        public UtilitiesRequirementsPostVM UtilitiesRequirements { get; set; }

        public OutPatientRequirementsPostVM OutPatientRequirements { get; set; }
        public IndoorBedsUnitsRequirementsPostVM IndoorBedsUnitsRequirements { get; set; }

        public IndoorBedsOccupancyPostVM IndoorBedsOccupancy { get; set; }
        public SuperVisionInFieldPracticeAreaPostVm SuperVisionInFieldPracticeArea { get; set; }
        public int FacultyCode { get; set; }
        public string CollegeCode { get; set; }

        public string CourseLevel { get; set; }
        public List<SectionViewModel> Sections { get; set; } = new();

    }
    public class HospitalFacilitiesViewModel
    {
        public int HospitalDetailsId { get; set; }
        public List<DropdownItem> AvailableFacilities { get; set; } = new();
        public List<int> SelectedFacilityIds { get; set; } = new();
    }


    public class ClinicalHospitalDetailsPostVM
    {
        public ClinicalHospitalFormVM Form { get; set; } = new();

        [ValidateNever]
        public IFormFile? SupportingDoc { get; set; }
    }

    public class FieldPracticeAreasPostViewModel
    {
        public List<FieldPracticeAreaViewModel> FieldPracticeAreas { get; set; } = new();
    }

    public class HospitalDocumentsToBeUploadedPostModel
    {
        //public string CollegeCode { get; set; }
        //public int HospitalDetailsId { get; set; }

        [Required]
        public List<HospitalDocumentsToBeUploadedViewModel> Documents { get; set; }
    }
    public class HospitalFacilitiesPostVM
    {
        [Required]
        public int HospitalDetailsId { get; set; }

        [MinLength(1, ErrorMessage = "Select at least one facility.")]
        public List<int> SelectedFacilityIds { get; set; } = new();
    }

    public class ClinicalHospitalFormVM
    {
        public int HospitalDetailsId { get; set; }
        public string CollegeCode { get; set; }
        public string FacultyCode { get; set; }

        public string? CourseLevel { get; set; }
        public int AffiliationTypeId { get; set; }
        public string AffiliationType { get; set; }


        public bool? ParentMedicalCollegeExists { get; set; }

        [Required]
        public string? HospitalType { get; set; }

        [Required]
        public string? HospitalOwnedBy { get; set; }
        [Required]
        public string HospitalName { get; set; }
        [Required]
        public string HospitalOwnerName { get; set; }
        [Required]
        public string? HospitalDistrictId { get; set; }
        [Required]
        public string? HospitalTalukId { get; set; }
        [Required]
        public string Location { get; set; }

        public bool? IsParentHospitalForOtherNursingInstitution { get; set; }
        //public List<SelectListItem> HospitalTypeList { get; set; }
        //public List<SelectListItem> HospitalOwnedByList { get; set; }
        public string? HospitalTypeId { get; set; }
        public string? HospitalOwnedById { get; set; }
    }


    public class ClinicalCapacityViewModel
    {
        public ClinicalCapacityFormVM Form { get; set; } = new();

        // Dropdowns
        public List<DropdownItem> IsParentHospitalForOtherNursingInstitutionOptions { get; set; } = new();
        public List<DropdownItem> IsOwnerAmemberOfTrustOptions { get; set; } = new();
    }

    public class ClinicalCapacityPostVM
    {
        public ClinicalCapacityFormVM Form { get; set; } = new();
    }


    public class ClinicalCapacityFormVM
    {
        public int HospitalDetailsId { get; set; }
        public string CollegeCode { get; set; }
        public string FacultyCode { get; set; }
        public int AffiliationTypeId { get; set; }

        // Capacity & Statistics
        public int? TotalBeds { get; set; }
        public int? OpdperDay { get; set; }
        public decimal? IpdbedOccupancyPercent { get; set; }
        public int? AnnualOpdprevYear { get; set; }
        public int? AnnualIpdprevYear { get; set; }
        public decimal? DistanceBetweenCollegeAndHospitalKm { get; set; }
        public bool? IsOwnerAmemberOfTrust { get; set; }
    }


    public class RequirementItemBaseVM
    {
        [Required]
        public int RequirementId { get; set; }
        public bool IsAvailable { get; set; }

        [Required]
        [StringLength(300, ErrorMessage = "Requirement name cannot exceed 300 characters.")]
        public string RequirementName { get; set; } = string.Empty;

    }
    public abstract class RequirementsBasePostVM<TItem>
    {
        [Required]
        [StringLength(20)]
        public string CollegeCode { get; set; } = string.Empty;

        [Required]
        public int HospitalDetailsId { get; set; }

        [Required]
        public int FacultyCode { get; set; }

        public string CourseLevel;

        [Required]
        [Range(1, int.MaxValue)]
        public int AffiliationTypeId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one requirement is required.")]
        public List<TItem> Requirements { get; set; } = new();
    }


    public class IndoorDepartmentRequirementItemVM : RequirementItemBaseVM { }

    public class IndoorDepartmentRequirementsPostVM
    {
        public string CollegeCode { get; set; } = string.Empty;
        public int HospitalDetailsId { get; set; }
        public int FacultyCode { get; set; }

        [ValidateNever]
        public string CourseLevel { get; set; }
        public int AffiliationTypeId { get; set; }

        public List<IndoorDepartmentRequirementItemVM> Requirements { get; set; }
            = new();
    }


    public class OTRequirementItemVM : RequirementItemBaseVM { }

    public class OTRequirementsPostVM : RequirementsBasePostVM<OTRequirementItemVM> { }

    public class CasualityRequirementItemVM : RequirementItemBaseVM { }


    public class CasualityRequirementsPostVM : RequirementsBasePostVM<CasualityRequirementItemVM> { }

    public class CSSDandLaundryItemVM : RequirementItemBaseVM { }

    public class CSSDandLaundryRequirementsPostVM : RequirementsBasePostVM<CSSDandLaundryItemVM> { }

    public class RadioDiagnosisItemVM : RequirementItemBaseVM { }

    public class RadioDiagnosisRequirementsPostVM : RequirementsBasePostVM<RadioDiagnosisItemVM> { }

    public class AnaesthesiologyItemVM : RequirementItemBaseVM { }

    public class AnaesthesiologyRequirementsPostVM : RequirementsBasePostVM<AnaesthesiologyItemVM> { }


    public class CentralLaboratoryItemVM : RequirementItemBaseVM { }

    public class CentralLaboratoryRequirementsPostVM : RequirementsBasePostVM<CentralLaboratoryItemVM> { }

    public class BloodBankItemVM : RequirementItemBaseVM { }

    public class BloodBankRequirementsPostVM : RequirementsBasePostVM<BloodBankItemVM> { }
    public class YogaItemVM : RequirementItemBaseVM { }

    public class YogaRequirementsPostVM : RequirementsBasePostVM<YogaItemVM> { }
    public class RadiationOncologyItemVM : RequirementItemBaseVM { }

    public class RadiationOncologyRequirementsPostVM : RequirementsBasePostVM<RadiationOncologyItemVM> { }
    public class ArtCenterItemVM : RequirementItemBaseVM { }

    public class ArtCenterRequirementsPostVM : RequirementsBasePostVM<ArtCenterItemVM> { }
    public class PharmacyItemVM : RequirementItemBaseVM { }

    public class PharmacyRequirementsPostVM : RequirementsBasePostVM<PharmacyItemVM> { }
    public class UtilitiesItemVM : RequirementItemBaseVM { }

    public class UtilitiesRequirementsPostVM : RequirementsBasePostVM<UtilitiesItemVM> { }

    public class OutPatientAreaItemVM : RequirementItemBaseVM { }
    public class OutPatientRequirementsPostVM : RequirementsBasePostVM<OutPatientAreaItemVM> { }
    public class IndoorBedsUnitsItemVM : RequirementItemBaseVM { }

    public class IndoorBedsUnitsRequirementsPostVM : RequirementsBasePostVM<IndoorBedsUnitsItemVM> { }

    public class IndoorBedsOccupancyPostVM
    {
        public string CollegeCode { get; set; } = string.Empty;
        public int FacultyCode { get; set; }
        public int AffiliationTypeId { get; set; }
        public int HospitalDetailsId { get; set; }

        public List<IndoorBedsOccupancyItemVM> Items { get; set; } = new();
    }

    public class IndoorBedsOccupancyItemVM
    {
        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public string SeatSlabId { get; set; }

        public int SeatSlab { get; set; }   // 50, 100, etc.

        public int RGUHSintake { get; set; }
        public int CollegeIntake { get; set; }
    }


    public class SuperVisionInFieldPracticeAreaItemVM
    {
        public int? Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Post { get; set; }

        [Required]
        [StringLength(300)]
        public string Name { get; set; }

        [Required]
        [StringLength(300)]
        public string Qualification { get; set; }

        [Required]
        [Range(1900, 2045, ErrorMessage = "Enter a valid year")]
        public int YearOfQualification { get; set; }

        [Required]
        [StringLength(300)]
        public string University { get; set; }

        [Required]
        public DateOnly? UgFromDate { get; set; }

        [Required]
        public DateOnly? UgToDate { get; set; }

        public DateOnly? PgFromDate { get; set; }

        public DateOnly? PgToDate { get; set; }

        [Required]
        public string Responsibilities { get; set; }
    }

    public class SuperVisionInFieldPracticeAreaPostVm
    {
        [Required]
        public string CollegeCode { get; set; }

        [Required]
        public int FacultyCode { get; set; }

        [Required]
        public int AffiliationTypeId { get; set; }

        [Required]
        public int HospitalDetailsId { get; set; }
        public List<SuperVisionInFieldPracticeAreaItemVM> ItemsSuperVision { get; set; } = new();

    }

    //public class HospitalAffiliationCompositeDisplayVM : HospitalAffiliationCompositeViewModel { }

}
