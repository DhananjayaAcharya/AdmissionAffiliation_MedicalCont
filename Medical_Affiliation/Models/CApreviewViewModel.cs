using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CApreviewViewModel
    {
        public string CollegeCode { get; set; }
        public string FacultyCode { get; set; }

        public string CollegeName { get; set; }
        public string FacultyName { get; set; }

        public InstituionBasicDetailsDisplayVM InstitutionBasicVM { get; set; }

        public CA_Aff_AcademicMattersViewModel CAacademicMattersVM { get; set; }
        public HospitalAffiliationCompositeDisplayVM CAHospitalAFfiliationCompVM { get; set; }
        public PhysicalFacilitiesDisplayViewModel PhysicalFacilities { get; set; }

        public MedicalLibraryDisplayViewModel LibraryDisplay { get; set; }

        public FinanceViewModel FinanceVm { get; set; }

        public VehicleDetailListDisplayViewModel VehicleDetailsVM { get; set; }

        public AdminTeachAndHostelDisplayVM AdminTeachAndHostelVM { get; set; }
        public FacultyDesigNonTeachDisplayVM FacultyDesigNonTeachDisplayVM { get; set; }
        public AffiliationPaymentViewModel PaymentVM { get; set; }
        public AffiliationFinalDeclarationViewModel DeclarationVM { get; set; }
        //public

    }

    public class HospitalAffiliationCompositeDisplayVM
    {
        public string CollegeCode { get; set; } = string.Empty;
        public int FacultyCode { get; set; }

        public ClinicalHospitalDisplayViewModel ClinicalHospitalDetails { get; set; } = new();

        public List<AffiliatedHospitalDocumentsDisplayViewModel> AffiliatedHospitalDocuments { get; set; } = new();
        /// <summary>
        /// 🔥 Dynamic department/section-wise requirements
        /// </summary>
        public List<DepartmentRequirementsSectionDisplayVM> Sections { get; set; } = new();

        public List<HospitalDocumentsToBeUploadedDisplayViewModel> HospitalDocumentsToBeUploadedList { get; set; } = new();
        public List<IndoorBedsOccupancyItemVM> IndoorBedsOccupancy { get; set; }
        public List<SuperVisionInFieldPracticeAreaDisplayVM> SuperVisionInFPa { get; set; }

    }


    public class SuperVisionInFieldPracticeAreaItemDisplayVM
    {
        public int Id { get; set; }

        public string Post { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public DateOnly YearOfQualification { get; set; }
        public string University { get; set; } = string.Empty;

        public DateOnly? UgFromDate { get; set; }
        public DateOnly? UgToDate { get; set; }

        public DateOnly? PgFromDate { get; set; }
        public DateOnly? PgToDate { get; set; }

        public string Responsibilities { get; set; } = string.Empty;
    }

    public class SuperVisionInFieldPracticeAreaDisplayVM
    {
        public string CollegeCode { get; set; } = string.Empty;
        public int FacultyCode { get; set; }
        public int AffiliationTypeId { get; set; }
        public int HospitalDetailsId { get; set; }

        public List<SuperVisionInFieldPracticeAreaItemDisplayVM> Items { get; set; } = new();
    }

    public class HospitalDocumentsToBeUploadedDisplayViewModel : HospitalDocumentsToBeUploaded
    {
        public int TotalBeds { get; set; }
        public bool DocumentExists { get; set; }
    }

    public class AffiliatedHospitalDocumentsDisplayViewModel : AffiliatedHospitalDocumentsViewModel { }
    public class IndoorBedsOccupancyDisplayVM
    {
        public string CollegeCode { get; set; } = string.Empty;
        public int FacultyCode { get; set; }
        public int AffiliationTypeId { get; set; }
        public int HospitalDetailsId { get; set; }

        public List<IndoorBedsOccupancyItemVM> Items { get; set; } = new();
    }
    public class DepartmentRequirementBaseDisplayVM
    {
        public int RequirementId { get; set; }
        public string RequirementName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public bool IsCompliant { get; set; } = false;
        public string Remarks { get; set; } = string.Empty;
    }

    public class DepartmentRequirementsSectionDisplayVM
    {
        public string CollegeCode { get; set; } = string.Empty;
        public int FacultyCode { get; set; }
        public int HospitalDetailsId { get; set; }
        public int AffiliationTypeId { get; set; }

        public int SectionCode { get; set; }
        public string SectionName { get; set; } = string.Empty;

        public List<DepartmentRequirementBaseDisplayVM> Items { get; set; } = new();
    }


    public class IndoorBedsUnitsRequirementDisplayVM : IndoorBedsOccupancyDisplayVM { }

    public class ClinicalHospitalDisplayViewModel
    {
        public int HospitalDetailsId { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string HospitalType { get; set; } = string.Empty;
        public string HospitalOwnedBy { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;

        public string DistrictName { get; set; } = string.Empty;
        public string TalukName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        public int TotalBeds { get; set; }
        public int OpdPerDay { get; set; }
        public decimal IpdOccupancyPercent { get; set; }

        public bool ParentMedicalCollegeExists { get; set; }
        public bool IsParentHospitalForOtherNursingInstitution { get; set; }
        public bool IsOwnerAmemberOfTrust { get; set; }

        public decimal DistanceFromCollegeKm { get; set; }

        public bool IsSupportingDocExists { get; set; }
        public int? SupportingDocumentId { get; set; }
        public string? SupportingDocumentName { get; set; }

        public List<string> Facilities { get; set; } = new();
    }

    public class PhysicalFacilitiesDisplayViewModel
    {
        public int FacultyCode { get; set; }
        public string CollegeCode { get; set; }
        public SmallGroupTeachingDisplayViewModel SmallGroupTeaching { get; set; }
        public SmallGroupStudentLabsDisplayViewModel SmallGroupStudentLabs { get; set; }
        public SmallGroupMuseumsDisplayViewModel SmallGroupMuseums { get; set; }
        public SkillsLabDisplayViewModel SkillsLab { get; set; }
        public DepartmentOfficesMeuDisplayViewModel DeptOfficeMeu { get; set; }
        public LaboratoryEquipmentDisplayViewModel LaboratoryEquipment { get; set; }
        public SkillsLabEquipmentViewModel SkillsLabEquipment { get; set; }

    }
    public class SmallGroupTeachingDisplayViewModel
    {
        public int AnnualMbbsIntake { get; set; }

        public int SmallGroupBatchSize { get; set; } = 15;

        public bool? TeachingAreasSharedAllDepts { get; set; }

        public bool? AvInAllTeachingAreas { get; set; }

        public bool? InternetInAllTeachingAreas { get; set; }

        public bool? DigitalLinkAllTeachingAreas { get; set; }

        // ---------------- TEACHING ROOMS ----------------

        public int SmallGroupStudents { get; set; }

        public decimal RequiredAreaSqm { get; set; }

        public decimal AvailableAreaSqm { get; set; }

        public decimal AreaDeficiencySqm { get; set; }

        public bool? RoomsSharedByAllDepts { get; set; }

        public bool? AppropriateAreaEachSpecialty { get; set; }

        public bool? ConnectedToLectureHalls { get; set; }

        public bool? InternetInTeachingRooms { get; set; }

        // ---------------- STUDENT LABS (CHECKBOX GROUPS) ----------------
    }

    public class SmallGroupStudentLabsDisplayViewModel
    {
        public bool HistologyAvailable { get; set; }
        public bool HistologyShared { get; set; }

        public bool ClinicalPhysiologyAvailable { get; set; }
        public bool ClinicalPhysiologyShared { get; set; }

        public bool BiochemistryAvailable { get; set; }
        public bool BiochemistryShared { get; set; }

        public bool HistopathCytopathAvailable { get; set; }
        public bool HistopathCytopathShared { get; set; }

        public bool ClinPathHemeAvailable { get; set; }
        public bool ClinPathHemeShared { get; set; }

        public bool MicrobiologyAvailable { get; set; }
        public bool MicrobiologyShared { get; set; }

        public bool ClinicalPharmAvailable { get; set; }
        public bool ClinicalPharmShared { get; set; }

        public bool CalPharmAvailable { get; set; }
        public bool CalPharmShared { get; set; }

        // ---------------- COMMON FACILITIES ----------------

        public bool AllLabsHaveAV { get; set; }

        public bool AllLabsHaveInternet { get; set; }

        public bool TechnicalStaffFacilitiesEnsured { get; set; }

    }

    public class SmallGroupMuseumsDisplayViewModel
    {
        // ---------------- MUSEUMS ----------------

        public bool? SeparateAnatomyMuseumAvailable { get; set; }

        public bool? PathologyForensicSharedMuseum { get; set; }

        public bool? PharmMicroCommSharedMuseum { get; set; }

        public int SeatingCapacityPerMuseum { get; set; }

        public decimal SeatingAreaAvailableSqm { get; set; }

        public decimal SeatingAreaRequiredSqm { get; set; }
        public decimal SeatingAreaDeficiencySqm { get; set; }

        public bool? MuseumsHaveAV { get; set; }

        public bool? MuseumsHaveInternet { get; set; }

        public bool? MuseumsDigitallyLinked { get; set; }

        public bool? MuseumsHaveRacksShelves { get; set; }

        public bool? MuseumsHaveRadiologyDisplay { get; set; }


        public bool? TeachingTimeSharingProgrammed { get; set; }
    }



    public class SkillsLabDisplayViewModel
    {
        // Basic
        public int AnnualMbbsIntake { get; set; } // 100/150/200/250

        // Area
        public decimal TotalAreaAvailableSqm { get; set; }

        public decimal TotalAreaRequiredSqm { get; set; } // 600 or 800

        
        public decimal TotalAreaDeficiencySqm { get; set; }

       
        public bool? SixWeeksTrainingCompletedBeforeClinical { get; set; }

        // (a) Minimum 4 rooms for exam
        public int NumberOfExaminationRooms { get; set; }

        public bool? HasMinFourExamRooms { get; set; }

        // (b) Demo room for small groups
        public bool? HasDemoRoomSmallGroups { get; set; }

        // (c) Debrief / review area
        public bool? HasDebriefArea { get; set; }

        // (d) Rooms for coordinator and staff
        public bool? HasFacultyCoordinatorRoom { get; set; }

        public bool? HasSupportStaffRoom { get; set; }

        // (e) Storage for mannequins/equipmen
        public bool? HasStorageForMannequins { get; set; }

        // (f) Video recording & review facility
        public bool? HasVideoRecordingFacility { get; set; }

        // (g) Stations for practicing skills
        public int NumberOfSkillStations { get; set; }

        public bool? HasGroupAndIndividualStations { get; set; }

        // (h) Trainers / mannequins as per CBME
        public bool? HasRequiredTrainersAndMannequins { get; set; }

        // (i) Technical officer & support staff
        public bool? HasDedicatedTechnicalOfficer { get; set; }

        public bool? HasAdequateSupportStaff { get; set; }

        // (j) AV / Internet / e‑learning
        public bool? TeachingAreasHaveAV { get; set; }

        public bool? TeachingAreasHaveInternet { get; set; }

        public bool? SkillsLabEnabledForELearning { get; set; }
    }

    public class DepartmentOfficesMeuDisplayViewModel
    {
        // 1.8 Department Offices, Rooms For Staff

        public bool? HasHodRoomWithOfficeAndRecords { get; set; }

        public bool? HasRoomsForFacultyAndResidents { get; set; }

        public bool? FacultyRoomsHaveCommunicationComputerInternet { get; set; }

        public bool? HasRoomsForNonTeachingStaff { get; set; }

        // 1.9 Medical Education Unit

        public bool? HasMedicalEducationUnit { get; set; }

        public decimal? MedicalEducationUnitAreaSqm { get; set; }

        public bool? MedicalEducationUnitHasAudioVisual { get; set; }

        public bool? MedicalEducationUnitHasInternet { get; set; }


        // 3. Medical Education Unit – Coordinator details
        public string MeuCoordinatorName { get; set; }

        public string MeuCoordinatorDesignationDepartment { get; set; }

        public string MeuCoordinatorPhone { get; set; }

        public string MeuCoordinatorEmail { get; set; }

        // Members list + activities (free‑text or file-backed)
        public string MeuMembersListDescription { get; set; }

        public string MeuActivitiesLastAcademicYear { get; set; }

        // File upload for “Enclose Copy” of members list
        public IFormFile MeuMembersListFile { get; set; }

        public bool HasMeuMembersListFile { get; set; }

    }


    public class LaboratoryEquipmentDisplayItemVM
    {
        public int EquipmentId { get; set; }

        public string EquipmentName { get; set; }

        public int RequiredAsPerNorm { get; set; }

        public bool IsAvailable { get; set; }

        public int? AvailableQuantity { get; set; }

        // Optional display helpers
        public string Subject { get; set; }

        public string CourseCode { get; set; } = string.Empty;

        public bool IsDeficient =>
            IsAvailable && AvailableQuantity.HasValue
            && AvailableQuantity.Value < RequiredAsPerNorm;
    }
    public class LaboratoryEquipmentSubjectGroupVM
    {
        public string Subject { get; set; }

        public List<LaboratoryEquipmentDisplayItemVM> Equipments { get; set; }
            = new();
    }
    public class LaboratoryEquipmentCourseGroupVM
    {
        public string CourseCode { get; set; }

        public List<LaboratoryEquipmentSubjectGroupVM> Subjects { get; set; }
            = new();

        // Helpers for UI badges
        public int TotalEquipments =>
            Subjects.Sum(s => s.Equipments.Count);

        public int DeficientEquipments =>
            Subjects.Sum(s => s.Equipments.Count(e => e.IsDeficient));
    }


    public class LaboratoryEquipmentDisplayViewModel
    {
        public List<LaboratoryEquipmentCourseGroupVM> Courses { get; set; }
            = new();

        public int TotalEquipments =>
            Courses.Sum(c => c.TotalEquipments);
    }


    //public class MedicalLibraryDisplayViewModel : CA_Aff_MedicalLibraryViewModel { }
    public class MedicalLibraryDisplayViewModel {
        public string CollegeCode { get; set; } = null!;
        public CA_Aff_MedicalLibraryViewModel1 caAffMedicalLibraryvm {  get; set; }
        public CaMedLibCommitteeListDisplayViewModel librarayCommitteeVM { get; set; }
        public CaMedLibraryGeneralDisplayViewModel LibraryGeneralVM { get; set; }
        public CaMedLibraryItemListDisplayViewModel LibraryItemListVM {  get; set; }
        public CaMedLibraryBuildingDisplayViewModel LibraryBuildingVM { get; set; }
        public CaMedLibTechnicalProcessListDisplayViewModel LibraryTechListVM { get; set; }
        public CaMedLibraryFinanceDisplayViewModel LibraryFinancVM { get; set; }
        public CaMedLibraryEquipmentListDisplayViewModel LibraryEquipmentListVM { get; set; }
        public CaMedResearchPublicationsDisplayViewModel ResearchPublicationsDisplayViewModel { get; set; }

    }

    public class FinanceViewModel
    {
        public string CollegeCode { get; set; } = null!;

        public MedCaAccountAndFeeDetailDisplayViewModel medCaAccountAndFee { get; set; }
        public MedCaStaffParticularListDisplayViewModel staffParticularsVM { get; set; }
        public CaMedStaffParticularsOtherDisplayViewModel otherStaffParticularsVM { get; set; }

    }
    public class MedCaAccountAndFeeDetailDisplayViewModel
    {
        public int Id { get; set; }

        public string CollegeCode { get; set; } = null!;

        public string FacultyCode { get; set; } = null!;

        public string? SubFacultyCode { get; set; }

        public string? RegistrationNo { get; set; }

        // Governing Council
        public string GoverningCouncilPdfName { get; set; } = null!;
        public bool HasGoverningCouncilPdf { get; set; }

        // Authority Details
        public string AuthorityNameAddress { get; set; } = null!;
        public string AuthorityContact { get; set; } = null!;

        // Fees
        public decimal RecurrentAnnual { get; set; }
        public decimal NonRecurrentAnnual { get; set; }
        public decimal Deposits { get; set; }
        public decimal TuitionFee { get; set; }
        public decimal SportsFee { get; set; }
        public decimal UnionFee { get; set; }
        public decimal LibraryFee { get; set; }
        public decimal OtherFee { get; set; }
        public decimal TotalFee { get; set; }

        // Accounts
        public string AccountBooksMaintained { get; set; } = null!;
        public string AccountsAudited { get; set; } = null!;

        public string? AccountSummaryPdfName { get; set; }
        public bool HasAccountSummaryPdf { get; set; }

        public string? AuditedStatementPdfName { get; set; }
        public bool HasAuditedStatementPdf { get; set; }
    }



    public class MedCaStaffParticularListDisplayViewModel
    {
        public string CollegeCode { get; set; } = null!;
        public string FacultyCode { get; set; } = null!;

        public List<MedCaStaffParticularDisplayViewModel> StaffParticulars { get; set; }
            = new();
    }

    public class MedCaStaffParticularDisplayViewModel
    {
        public int Id { get; set; }

        public string CollegeCode { get; set; } = null!;
        public string CollegeName { get; set; } = null!;

        public string FacultyCode { get; set; } = null!;

        public string? SubFacultyCode { get; set; }

        public string? RegistrationNo { get; set; }

        // Designation
        public int DesignationSlNo { get; set; }
        public string DesignationName { get; set; } = null!;

        // Pay
        public decimal PayScale { get; set; }
    }

    public class CaMedStaffParticularsOtherDisplayViewModel
    {
        public int Id { get; set; }

        public string CollegeCode { get; set; } = null!;
        public string CollegeName { get; set; }
        public string FacultyCode { get; set; } = null!;

        public string? RegistrationNo { get; set; }
        public string? SubFacultyCode { get; set; }

        // General Status
        public bool TeachersUpdatedInEms { get; set; }
        public bool ExaminerDetailsAttached { get; set; }

        // Examiner Details
        public string? ExaminerDetailsPdfName { get; set; }
        public bool HasExaminerDetailsPdf { get; set; }

        // AEBAS Details
        public string? AebasLastThreeMonthsPdfName { get; set; }
        public bool HasAebasLastThreeMonthsPdf { get; set; }

        public string? AebasInspectionDayPdfName { get; set; }
        public bool HasAebasInspectionDayPdf { get; set; }

        // Registers
        public bool ServiceRegisterMaintained { get; set; }
        public bool AcquittanceRegisterMaintained { get; set; }

        // PF & ESI
        public string? ProvidentFundPdfName { get; set; }
        public bool HasProvidentFundPdf { get; set; }

        public string? EsipdfName { get; set; }
        public bool HasEsipdf { get; set; }
    }


    public class CaMedLibCommitteeDisplayViewModel
    {
        public int Id { get; set; }

        public int CommitteeId { get; set; }

        public string CommitteeName { get; set; } = null!;
        // (to be populated from master table if available)

        public bool IsPresent { get; set; }

        public string? CommitteePdfName { get; set; }

        public bool HasCommitteePdf { get; set; }
    }

    public class CaMedLibCommitteeListDisplayViewModel
    {

        public List<CaMedLibCommitteeDisplayViewModel> Committees { get; set; }
            = new();
    }


    public class CaMedLibraryGeneralDisplayViewModel
    {
        public int SlNo { get; set; }

        public string CollegeCode { get; set; } = null!;
        public string FacultyCode { get; set; } = null!;

        public string? LibraryEmailId { get; set; }

        // Flags (converted to bool for UI)
        public bool HasDigitalLibrary { get; set; }
        public bool HasHelinetServices { get; set; }
        public bool HasDepartmentWiseLibrary { get; set; }

        // Optional: original values if needed
        public string? DigitalLibrary { get; set; }
        public string? HelinetServices { get; set; }
        public string? DepartmentWiseLibrary { get; set; }
    }

    public class CaMedLibraryItemDisplayViewModel
    {
        public int SlNo { get; set; }

        public string ItemName { get; set; } = null!;

        public int CurrentForeign { get; set; }
        public int CurrentIndian { get; set; }

        public int PreviousForeign { get; set; }
        public int PreviousIndian { get; set; }
        public bool HasIndianForeignSplit { get; set; }
    }

    public class CaMedLibraryItemListDisplayViewModel
    {
        public string CollegeCode { get; set; } = null!;
        public string FacultyCode { get; set; } = null!;

        public List<CaMedLibraryItemDisplayViewModel> Items { get; set; }
            = new();
    }

    public class CaMedLibraryBuildingDisplayViewModel
    {
        public int SlNo { get; set; }

        public string CollegeCode { get; set; } = null!;
        public string FacultyCode { get; set; } = null!;

        // Independent building flag
        public bool IsIndependent { get; set; }
        public string? IsIndependentText { get; set; }

        // Area
        public decimal? AreaSqMtrs { get; set; }
    }


    public class CaMedLibTechnicalProcessDisplayViewModel
    {
        public int SlNo { get; set; }

        public string ProcessName { get; set; } = null!;

        public string? Value { get; set; }

        // Helpful flag for UI
        public bool HasValue => !string.IsNullOrWhiteSpace(Value);
    }
    public class CaMedLibTechnicalProcessListDisplayViewModel
    {

        public List<CaMedLibTechnicalProcessDisplayViewModel> Processes { get; set; }
            = new();
    }



    public class CaMedLibraryFinanceDisplayViewModel
    {
        public string CollegeCode { get; set; } = null!;
        public string FacultyCode { get; set; } = null!;
        public string? SubFacultyCode { get; set; }
        public string? RegistrationNo { get; set; }

        // Finance Details
        public decimal? TotalBudgetLakhs { get; set; }
        public decimal? ExpenditureBooksLakhs { get; set; }
    }

    public class CaMedResearchPublicationsDisplayViewModel
    {
        public int? PublicationsNo { get; set; }
        public string? Pi { get; set; }

        public int? RguhsFunded { get; set; }
        public int? ExternalBodyFunding { get; set; }

        public bool HasPublicationsPdf { get; set; }
        public bool HasProjectsPdf { get; set; }
        public bool HasClinicalTrialsPdf { get; set; }

        public int? StudentsRguhsFunded { get; set; }
        public int? StudentsExternalFunding { get; set; }
        public bool HasStudentsProjectsPdf { get; set; }

        public int? FacultyRguhsFunded { get; set; }
        public int? FacultyExternalFunding { get; set; }
        public bool HasFacultyProjectsPdf { get; set; }
    }

    public class CaMedLibraryEquipmentDisplayViewModel
    {
        public int SlNo { get; set; }

        public string CollegeCode { get; set; } = null!;

        public string FacultyCode { get; set; } = null!;

        public string? SubFacultyCode { get; set; }

        public string? RegistrationNo { get; set; }

        public string EquipmentName { get; set; } = null!;

        // UI-friendly (Yes / No)
        public bool HasEquipment { get; set; }
    }

    public class CaMedLibraryEquipmentListDisplayViewModel
    {
        public string CollegeCode { get; set; } = null!;

        public List<CaMedLibraryEquipmentDisplayViewModel> Items { get; set; }
            = new List<CaMedLibraryEquipmentDisplayViewModel>();
    }

    public class CaVehicleDetailDisplayViewModel
    {
        public int Id { get; set; }

        public string? CollegeCode { get; set; }

        public string? FacultyCode { get; set; }

        public string? RegistrationNo { get; set; }

        public string VehicleRegNo { get; set; } = null!;

        public string VehicleForCode { get; set; } = null!;

        public int? SeatingCapacity { get; set; }

        public DateOnly? ValidityDate { get; set; }

        public string? RcBookStatus { get; set; }

        public string? InsuranceStatus { get; set; }

        public string? DrivingLicenseStatus { get; set; }

        // UI helpers
        public bool HasValidRc => RcBookStatus?.Equals("Yes", StringComparison.OrdinalIgnoreCase) == true;
        public bool HasValidInsurance => InsuranceStatus?.Equals("Yes", StringComparison.OrdinalIgnoreCase) == true;
        public bool HasValidLicense => DrivingLicenseStatus?.Equals("Yes", StringComparison.OrdinalIgnoreCase) == true;
    }

    public class VehicleDetailListDisplayViewModel
    {
        public string? CollegeCode { get; set; }

        public List<CaVehicleDetailDisplayViewModel> Items { get; set; }
            = new List<CaVehicleDetailDisplayViewModel>();
    }

    public class InstituionBasicDetailsDisplayVM
    {
        public string? CollegeCode { get; set; }
        public ContinuationTrustMemberListDisplayViewModel TrustMemberVM {get; set;}
        public AffSanctionedIntakeForCourseListDisplayViewModel IntakeForCourseVM { get; set; }
        public AffCourseDisplayVM AffCoursesVM { get; set; }

        public AffiliationCourseDetailDisplayVM AffiliationCourseDetailVM { get; set; }

        public AffDeanOrDirectorDetailDisplayVM DeanOrDirectorDetailDisplayVM { get; set; }
        public AffPrincipalDetailDisplayVM PrincipalDetailDisplayVM { get; set; }
    }

    public class ContinuationTrustMemberListDisplayViewModel
    {

        public List<ContinuationTrustMemberDisplayViewModel> Items { get; set; }
            = new List<ContinuationTrustMemberDisplayViewModel>();
    }

    public class ContinuationTrustMemberDisplayViewModel
    {
        public int SlNo { get; set; }
        public string Faculty { get; set; }
        public string CollegeCode { get; set; }
        public string TrustMemberName { get; set; } = null!;

        public string? Designation { get; set; }

        public string? Qualification { get; set; }

        public string? MobileNumber { get; set; }

        public int? Age { get; set; }

        public DateOnly? JoiningDate { get; set; }

        public string? DesignationId { get; set; }

        // UI helper for mobile number display
        public string MobileDisplay => string.IsNullOrEmpty(MobileNumber) ? "—" : MobileNumber;

        // UI helper for JoiningDate display
        public string JoiningDateDisplay => JoiningDate?.ToString("dd-MM-yyyy") ?? "—";
    }

    public class AffSanctionedIntakeForCourseDisplayViewModel
    {
        public string CourseName { get; set; } = null!;
        public string SanctionedIntake { get; set; } = null!;
        public string? EligibleSeatSlab { get; set; }
        public bool HasDocument { get; set; }
    }

    public class AffSanctionedIntakeForCourseListDisplayViewModel
    {
        public List<AffSanctionedIntakeForCourseDisplayViewModel> Items { get; set; }
            = new List<AffSanctionedIntakeForCourseDisplayViewModel>();
    }

    public class AffCourseDisplayItemVM
    {
        public string CourseName { get; set; }
        public bool IsRecognized { get; set; }
        public string? RguhsNotificationNo { get; set; }
        public bool HasDocument { get; set; }
    }
    public class AffCourseDisplayVM
    {
        public List<AffCourseDisplayItemVM> Items { get; set; } = new();
    }

    public class AffiliationCourseDetailDisplayVM
    {
        public string? CourseName { get; set; }
        public string? IntakeDuring202526 { get; set; }
        public string? IntakeSlab { get; set; }
        public string? TypeofPermission { get; set; }
        public string? YearOfLop { get; set; }
        public string? DateOfRecognition { get; set; }
        public string? YearOfObtainingEcAndFc { get; set; }
        public string? SanctionedIntakeEcFc { get; set; }
        public bool HasGokOrder { get; set; }
    }

    public class AffDeanOrDirectorDetailDisplayVM
    {
        public string? DeanOrDirectorName { get; set; }
        public string? DeanQualification { get; set; }
        public string? DeanQualificationDate { get; set; }
        public string? DeanUniversity { get; set; }
        public string? DeanStateCouncilNumber { get; set; }
        public string RecognizedByMci { get; set; } = "—";
    }
    public class AffPrincipalDetailDisplayVM
    {
        public string? PrincipalName { get; set; }
        public string? PrincipalQualification { get; set; }
        public string? PrincipalQualificationDate { get; set; }
        public string? PrincipalUniversity { get; set; }
        public string? PrincipalStateCouncilNumber { get; set; }
        public string RecognizedByMci { get; set; } = "—";
    }


    public class AdminTeachAndHostelDisplayVM
    {
        public string CollegeCode { get; set; }
        public List<AffAdminTeachingBlockDisplayVM> AdminTeachingBlockDisplayVM { get; set; }
        public List<HostelDetailDisplayVM> HostelDetailsVM { get; set; }
        public List<AffHostelFacilityDisplayVM> AffHostelFacilitiesVM { get; set; }

    }

    public class AffAdminTeachingBlockDisplayVM
    {
        public string Facilities { get; set; } = null!;
        public string SizeSqFtAsPerNorms { get; set; } = null!;
        public string IsAvailable { get; set; } = null!;
        public string NoOfRooms { get; set; } = null!;
        public string SizeSqFtAvailablePerRoom { get; set; } = null!;
    }

    public class HostelDetailDisplayVM
    {
        public string HostelType { get; set; }
        public string BuiltUpAreaSqFt { get; set; }

        public bool HasSeparateHostel { get; set; }
        public bool SeparateProvisionMaleFemale { get; set; }

        public string TotalFemaleStudents { get; set; }
        public string TotalFemaleRooms { get; set; }

        public string TotalMaleStudents { get; set; }
        public string TotalMaleRooms { get; set; }

        public bool HasPossessionProof { get; set; }
    }

    public class AffHostelFacilityDisplayVM
    {
        public string FacilityName { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }

    public class FacultyDesigNonTeachDisplayVM
    {
        public string collegeCode {  get; set; } = string.Empty;
        public List<FacultyDetailDisplayVM> FacultyDetailDisplayVM { get; set; } = new();
        public List<CollegeDesignationDepartmentGroupVM> CollegeDesignationDisplayVM { get; set; } = new();
        public NonTeachingStaffSectionVM NonTeachingStaffSectionVM { get; set; } = new();
    }

    public class CollegeDesignationDepartmentGroupVM
    {
        public string? Department { get; set; }
        public List<CollegeDesignationDisplayVM> Designations { get; set; } = new();
    }

    public class FacultyDetailDisplayVM
    {
        public string NameOfFaculty { get; set; } = string.Empty;
        public string? Subject { get; set; }
        public string Designation { get; set; } = string.Empty;
        public string Course { get; set; }

        public string? RecognizedPgTeacher { get; set; }
        public string? RecognizedPhDteacher { get; set; }
        public string? LitigationPending { get; set; }

        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string? DepartmentDetails { get; set; }

        public bool HasGuideRecognitionDoc { get; set; }
        public bool HasPhDRecognitionDoc { get; set; }
        public bool HasLitigationDoc { get; set; }
    }

    public class CollegeDesignationDisplayVM
    {
        public string Designation { get; set; } = string.Empty;
        public string? DesignationCode { get; set; }
        public string? Department { get; set; }
        public string? DepartmentCode { get; set; }
        public string? SeatSlabId { get; set; }
        public int SeatSlab { get; set; }
        public string RequiredIntake { get; set; } = string.Empty;
        public string AvailableIntake { get; set; } = string.Empty;
    }

    public class NonTeachingStaffDisplayVM
    {
        public int StaffId { get; set; }

        public string StaffName { get; set; } = string.Empty;

        // 👇 Resolved designation name from DesignationMaster
        public string Designation { get; set; } = string.Empty;

        public string? MobileNumber { get; set; }

        public decimal SalaryPaid { get; set; }

        public bool PfProvided { get; set; }

        public bool EsiProvided { get; set; }

        public bool ServiceRegisterMaintained { get; set; }

        public bool SalaryAcquaintanceRegister { get; set; }
    }
    public class NonTeachingStaffSectionVM
    {
        public string CollegeCode { get; set; } = string.Empty;

        public List<NonTeachingStaffDisplayVM> Staffs { get; set; } = new();
    }

}
