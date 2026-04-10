using System;
using System.Collections.Generic;
using Medical_Affiliation.Models;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.DATA;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcademicIntake> AcademicIntakes { get; set; }

    public virtual DbSet<AcademicYear> AcademicYears { get; set; }

    public virtual DbSet<AdministrativeFacilityType> AdministrativeFacilityTypes { get; set; }

    public virtual DbSet<AffAdminTeachingBlock> AffAdminTeachingBlocks { get; set; }

    public virtual DbSet<AffCourseDetail> AffCourseDetails { get; set; }

    public virtual DbSet<AffDeanAdministrativeExperience> AffDeanAdministrativeExperiences { get; set; }

    public virtual DbSet<AffDeanOrDirectorDetail> AffDeanOrDirectorDetails { get; set; }

    public virtual DbSet<AffDeanTeachingExperience> AffDeanTeachingExperiences { get; set; }

    public virtual DbSet<AffHostelDetail> AffHostelDetails { get; set; }

    public virtual DbSet<AffHostelFacilityDetail> AffHostelFacilityDetails { get; set; }

    public virtual DbSet<AffInstitutionStatusMaster> AffInstitutionStatusMasters { get; set; }

    public virtual DbSet<AffInstitutionsDetail> AffInstitutionsDetails { get; set; }

    public virtual DbSet<AffNonTeachingStaff> AffNonTeachingStaffs { get; set; }

    public virtual DbSet<AffPrincipalAdministrativeExperience> AffPrincipalAdministrativeExperiences { get; set; }

    public virtual DbSet<AffPrincipalDetail> AffPrincipalDetails { get; set; }

    public virtual DbSet<AffPrincipalTeachingExperience> AffPrincipalTeachingExperiences { get; set; }

    public virtual DbSet<AffSanctionedIntakeForCourse> AffSanctionedIntakeForCourses { get; set; }

    public virtual DbSet<AffTeachingFacultyAllDetail> AffTeachingFacultyAllDetails { get; set; }

    public virtual DbSet<AffiliatedHospitalDocument> AffiliatedHospitalDocuments { get; set; }

    public virtual DbSet<AffiliatedYearwiseMaterialsDatum> AffiliatedYearwiseMaterialsData { get; set; }

    public virtual DbSet<AffiliationCollege> AffiliationColleges { get; set; }

    public virtual DbSet<AffiliationCollegeMaster> AffiliationCollegeMasters { get; set; }

    public virtual DbSet<AffiliationCourseDetail> AffiliationCourseDetails { get; set; }

    public virtual DbSet<AffiliationLicinpsection> AffiliationLicinpsections { get; set; }

    public virtual DbSet<AffiliationOtherCoursesPermittedByNmc> AffiliationOtherCoursesPermittedByNmcs { get; set; }

    public virtual DbSet<AffiliationPgSsCourseDetail> AffiliationPgSsCourseDetails { get; set; }

    public virtual DbSet<AffiliationPgSsCourseDetailsForGok> AffiliationPgSsCourseDetailsForGoks { get; set; }

    public virtual DbSet<AffiliationPgSsCourseDetailsRguh> AffiliationPgSsCourseDetailsRguhs { get; set; }

    public virtual DbSet<AhsAffiliatedYearwiseMaterialsDatum> AhsAffiliatedYearwiseMaterialsData { get; set; }

    public virtual DbSet<AhsExpectedIntakeMaster> AhsExpectedIntakeMasters { get; set; }

    public virtual DbSet<AssociatedInstitution> AssociatedInstitutions { get; set; }

    public virtual DbSet<BasicDetail> BasicDetails { get; set; }

    public virtual DbSet<BuildingTypeMaster> BuildingTypeMasters { get; set; }

    public virtual DbSet<CaAcademicPerformance> CaAcademicPerformances { get; set; }

    public virtual DbSet<CaCourseCurriculum> CaCourseCurricula { get; set; }

    public virtual DbSet<CaCourseDetailsInFinancialDetail> CaCourseDetailsInFinancialDetails { get; set; }

    public virtual DbSet<CaDepartmentLibraryDetail> CaDepartmentLibraryDetails { get; set; }

    public virtual DbSet<CaExaminationScheme> CaExaminationSchemes { get; set; }

    public virtual DbSet<CaFinancialDetail> CaFinancialDetails { get; set; }

    public virtual DbSet<CaLibraryDetail> CaLibraryDetails { get; set; }

    public virtual DbSet<CaLibraryService> CaLibraryServices { get; set; }

    public virtual DbSet<CaLibraryStaffDetail> CaLibraryStaffDetails { get; set; }

    public virtual DbSet<CaMedLibCommittee> CaMedLibCommittees { get; set; }

    public virtual DbSet<CaMedLibOtherAcademicActivity> CaMedLibOtherAcademicActivities { get; set; }

    public virtual DbSet<CaMedLibTechnicalProcess> CaMedLibTechnicalProcesses { get; set; }

    public virtual DbSet<CaMedLibraryBuilding> CaMedLibraryBuildings { get; set; }

    public virtual DbSet<CaMedLibraryEquipment> CaMedLibraryEquipments { get; set; }

    public virtual DbSet<CaMedLibraryFinance> CaMedLibraryFinances { get; set; }

    public virtual DbSet<CaMedLibraryGeneral> CaMedLibraryGenerals { get; set; }

    public virtual DbSet<CaMedLibraryItem> CaMedLibraryItems { get; set; }

    public virtual DbSet<CaMedResearchPublicationsDetail> CaMedResearchPublicationsDetails { get; set; }

    public virtual DbSet<CaMedStaffParticularsOther> CaMedStaffParticularsOthers { get; set; }

    public virtual DbSet<CaMedStaffParticularsOtherTemp> CaMedStaffParticularsOtherTemps { get; set; }

    public virtual DbSet<CaMedicalDepartmentLibrary> CaMedicalDepartmentLibraries { get; set; }

    public virtual DbSet<CaMedicalLibraryOtherDetail> CaMedicalLibraryOtherDetails { get; set; }

    public virtual DbSet<CaMedicalLibraryService> CaMedicalLibraryServices { get; set; }

    public virtual DbSet<CaMedicalLibraryStaff> CaMedicalLibraryStaffs { get; set; }

    public virtual DbSet<CaMedicalLibraryUsageReport> CaMedicalLibraryUsageReports { get; set; }

    public virtual DbSet<CaMstCourseCurriculum> CaMstCourseCurricula { get; set; }

    public virtual DbSet<CaMstExaminationScheme> CaMstExaminationSchemes { get; set; }

    public virtual DbSet<CaMstLibraryEquipmentsType> CaMstLibraryEquipmentsTypes { get; set; }

    public virtual DbSet<CaMstLibraryServicesList> CaMstLibraryServicesLists { get; set; }

    public virtual DbSet<CaMstMedCommitteeName> CaMstMedCommitteeNames { get; set; }

    public virtual DbSet<CaMstMedLibTechnicalProcess> CaMstMedLibTechnicalProcesses { get; set; }

    public virtual DbSet<CaMstMedLibraryEquipment> CaMstMedLibraryEquipments { get; set; }

    public virtual DbSet<CaMstMedLibraryItem> CaMstMedLibraryItems { get; set; }

    public virtual DbSet<CaMstMedOtherAcademicActivity> CaMstMedOtherAcademicActivities { get; set; }

    public virtual DbSet<CaMstMediLibraryService> CaMstMediLibraryServices { get; set; }

    public virtual DbSet<CaMstRegisterRecord> CaMstRegisterRecords { get; set; }

    public virtual DbSet<CaMstUserDetail> CaMstUserDetails { get; set; }

    public virtual DbSet<CaMstVdVehicleFor> CaMstVdVehicleFors { get; set; }

    public virtual DbSet<CaMstYearOfStudy> CaMstYearOfStudies { get; set; }

    public virtual DbSet<CaSsAffiliationGrantedYear> CaSsAffiliationGrantedYears { get; set; }

    public virtual DbSet<CaSsLicpreviousInspection> CaSsLicpreviousInspections { get; set; }

    public virtual DbSet<CaSsLopsavedDate> CaSsLopsavedDates { get; set; }

    public virtual DbSet<CaSsOtherCoursesConducted> CaSsOtherCoursesConducteds { get; set; }

    public virtual DbSet<CaSsPermission> CaSsPermissions { get; set; }

    public virtual DbSet<CaStudentRegisterRecord> CaStudentRegisterRecords { get; set; }

    public virtual DbSet<CaVehicleDetail> CaVehicleDetails { get; set; }

    public virtual DbSet<ClinicalDatum> ClinicalData { get; set; }

    public virtual DbSet<ClinicalFacilityDocMaster> ClinicalFacilityDocMasters { get; set; }

    public virtual DbSet<ClinicalMaterialDatum> ClinicalMaterialData { get; set; }

    public virtual DbSet<CollegeCourseIntakeDetail> CollegeCourseIntakeDetails { get; set; }

    public virtual DbSet<CollegeDesignationDetail> CollegeDesignationDetails { get; set; }

    public virtual DbSet<CollegeIntakeDetail> CollegeIntakeDetails { get; set; }

    public virtual DbSet<ContinuationTrustMemberDetail> ContinuationTrustMemberDetails { get; set; }

    public virtual DbSet<CourseIntakeDetail> CourseIntakeDetails { get; set; }

    public virtual DbSet<CourseMaster> CourseMasters { get; set; }

    public virtual DbSet<CoursesOffered> CoursesOffereds { get; set; }

    public virtual DbSet<DepartmentMaster> DepartmentMasters { get; set; }

    public virtual DbSet<DepartmentWiseFacultyMaster> DepartmentWiseFacultyMasters { get; set; }

    public virtual DbSet<DesignationMaster> DesignationMasters { get; set; }

    public virtual DbSet<DistrictMaster> DistrictMasters { get; set; }

    public virtual DbSet<Faculty> Faculties { get; set; }

    public virtual DbSet<FacultyDetail> FacultyDetails { get; set; }

    public virtual DbSet<FacultyExamResult> FacultyExamResults { get; set; }

    public virtual DbSet<FeesMaster> FeesMasters { get; set; }

    public virtual DbSet<FeesType> FeesTypes { get; set; }

    public virtual DbSet<FellowShipMedical> FellowShipMedicals { get; set; }

    public virtual DbSet<FreshOrIncreaseMaster> FreshOrIncreaseMasters { get; set; }

    public virtual DbSet<HealthCenterChp> HealthCenterChps { get; set; }

    public virtual DbSet<HospitalDetailsForAffiliation> HospitalDetailsForAffiliations { get; set; }

    public virtual DbSet<HospitalDocumentDetail> HospitalDocumentDetails { get; set; }

    public virtual DbSet<HospitalDocumentsToBeUploaded> HospitalDocumentsToBeUploadeds { get; set; }

    public virtual DbSet<HospitalFacilitiesMaster> HospitalFacilitiesMasters { get; set; }

    public virtual DbSet<HospitalFacility> HospitalFacilities { get; set; }

    public virtual DbSet<IndoorBedsOccupancy> IndoorBedsOccupancies { get; set; }

    public virtual DbSet<IndoorInfrastructureRequirementsCompliance> IndoorInfrastructureRequirementsCompliances { get; set; }

    public virtual DbSet<InitiativeMaster> InitiativeMasters { get; set; }

    public virtual DbSet<InstitutionBasicDetail> InstitutionBasicDetails { get; set; }

    public virtual DbSet<InstitutionDetail> InstitutionDetails { get; set; }

    public virtual DbSet<InstitutionType> InstitutionTypes { get; set; }

    public virtual DbSet<IntakeDetail> IntakeDetails { get; set; }

    public virtual DbSet<IntakeDetailsLatest> IntakeDetailsLatests { get; set; }

    public virtual DbSet<IntakeMaster> IntakeMasters { get; set; }

    public virtual DbSet<LandBuildingDetail> LandBuildingDetails { get; set; }

    public virtual DbSet<LatestExcelAff> LatestExcelAffs { get; set; }

    public virtual DbSet<LicInspection> LicInspections { get; set; }

    public virtual DbSet<LicInspectionCollegeDetail> LicInspectionCollegeDetails { get; set; }

    public virtual DbSet<LicInspectionOtherDetail> LicInspectionOtherDetails { get; set; }

    public virtual DbSet<LicclaimDetail> LicclaimDetails { get; set; }

    public virtual DbSet<LiccollegeApproval> LiccollegeApprovals { get; set; }

    public virtual DbSet<LicinspectionDetail> LicinspectionDetails { get; set; }

    public virtual DbSet<MedCaAccountAndFeeDetail> MedCaAccountAndFeeDetails { get; set; }

    public virtual DbSet<MedCaMstStaffDesignation> MedCaMstStaffDesignations { get; set; }

    public virtual DbSet<MedCaStaffParticular> MedCaStaffParticulars { get; set; }

    public virtual DbSet<MedicalAdministrativePhysicalFacility> MedicalAdministrativePhysicalFacilities { get; set; }

    public virtual DbSet<MedicalCourseDetail> MedicalCourseDetails { get; set; }

    public virtual DbSet<MedicalDepartmentOfficesMeu> MedicalDepartmentOfficesMeus { get; set; }

    public virtual DbSet<MedicalInstituteDetail> MedicalInstituteDetails { get; set; }

    public virtual DbSet<MedicalMuseum> MedicalMuseums { get; set; }

    public virtual DbSet<MedicalSkillsLaboratory> MedicalSkillsLaboratories { get; set; }

    public virtual DbSet<MedicalStudentPracticalLab> MedicalStudentPracticalLabs { get; set; }

    public virtual DbSet<MedicalUgbedDistribution> MedicalUgbedDistributions { get; set; }

    public virtual DbSet<MstAdministration> MstAdministrations { get; set; }

    public virtual DbSet<MstAdministrativeFacility> MstAdministrativeFacilities { get; set; }

    public virtual DbSet<MstAffiliatedMaterialDatum> MstAffiliatedMaterialData { get; set; }

    public virtual DbSet<MstBuildingDetailRequired> MstBuildingDetailRequireds { get; set; }

    public virtual DbSet<MstClassroomDetail> MstClassroomDetails { get; set; }

    public virtual DbSet<MstCourse> MstCourses { get; set; }

    public virtual DbSet<MstDesignation> MstDesignations { get; set; }

    public virtual DbSet<MstFeesType> MstFeesTypes { get; set; }

    public virtual DbSet<MstFieldTypeChp> MstFieldTypeChps { get; set; }

    public virtual DbSet<MstFpaAdopAffType> MstFpaAdopAffTypes { get; set; }

    public virtual DbSet<MstGeoLocation> MstGeoLocations { get; set; }

    public virtual DbSet<MstHospitalDocument> MstHospitalDocuments { get; set; }

    public virtual DbSet<MstHospitalLocation> MstHospitalLocations { get; set; }

    public virtual DbSet<MstHospitalOwnedBy> MstHospitalOwnedBies { get; set; }

    public virtual DbSet<MstHospitalType> MstHospitalTypes { get; set; }

    public virtual DbSet<MstHostelFacility> MstHostelFacilities { get; set; }

    public virtual DbSet<MstHosteltype> MstHosteltypes { get; set; }

    public virtual DbSet<MstIndoorBedsDepartmentMaster> MstIndoorBedsDepartmentMasters { get; set; }

    public virtual DbSet<MstIndoorBedsOccupancyMaster> MstIndoorBedsOccupancyMasters { get; set; }

    public virtual DbSet<MstIndoorInfrastructureRequirementsMaster> MstIndoorInfrastructureRequirementsMasters { get; set; }

    public virtual DbSet<MstInstitutionType> MstInstitutionTypes { get; set; }

    public virtual DbSet<MstLaboratory> MstLaboratories { get; set; }

    public virtual DbSet<MstLaboratoryEquipmentDetail> MstLaboratoryEquipmentDetails { get; set; }

    public virtual DbSet<MstLaboratoryEquipmentSubject> MstLaboratoryEquipmentSubjects { get; set; }

    public virtual DbSet<MstLibraryEquipmentMaster> MstLibraryEquipmentMasters { get; set; }

    public virtual DbSet<MstLibraryFinanceItem> MstLibraryFinanceItems { get; set; }

    public virtual DbSet<MstLibraryServicesMaster> MstLibraryServicesMasters { get; set; }

    public virtual DbSet<MstLicAcademicCouncilMember> MstLicAcademicCouncilMembers { get; set; }

    public virtual DbSet<MstLicInspectionAllotedMembersDetail> MstLicInspectionAllotedMembersDetails { get; set; }

    public virtual DbSet<MstLicInspectionMember> MstLicInspectionMembers { get; set; }

    public virtual DbSet<MstLicSenateMember> MstLicSenateMembers { get; set; }

    public virtual DbSet<MstLicSubjectExpertiseMember> MstLicSubjectExpertiseMembers { get; set; }

    public virtual DbSet<MstMedicalCourseType> MstMedicalCourseTypes { get; set; }

    public virtual DbSet<MstNursingAffiliatedMaterialDatum> MstNursingAffiliatedMaterialData { get; set; }

    public virtual DbSet<NodalOfficerDetail> NodalOfficerDetails { get; set; }

    public virtual DbSet<NodalOfficerInitiative> NodalOfficerInitiatives { get; set; }

    public virtual DbSet<NonTeachingStaffDetail> NonTeachingStaffDetails { get; set; }

    public virtual DbSet<NursingAffiliatedYearwiseMaterialsDatum> NursingAffiliatedYearwiseMaterialsData { get; set; }

    public virtual DbSet<NursingCollegeRegistration> NursingCollegeRegistrations { get; set; }

    public virtual DbSet<NursingCourse> NursingCourses { get; set; }

    public virtual DbSet<NursingFacultyDetail> NursingFacultyDetails { get; set; }

    public virtual DbSet<NursingFacultyDetail1> NursingFacultyDetails1 { get; set; }

    public virtual DbSet<NursingFacultyWithCollege> NursingFacultyWithColleges { get; set; }

    public virtual DbSet<NursingInstituteDetail> NursingInstituteDetails { get; set; }

    public virtual DbSet<NursingUgpgdetail> NursingUgpgdetails { get; set; }

    public virtual DbSet<RguhsIntakeChangeAndApproval> RguhsIntakeChangeAndApprovals { get; set; }

    public virtual DbSet<SeatSlabMaster> SeatSlabMasters { get; set; }

    public virtual DbSet<SmallGroupTeaching> SmallGroupTeachings { get; set; }

    public virtual DbSet<StateMaster> StateMasters { get; set; }

    public virtual DbSet<SuperVisionInFieldPracticeArea> SuperVisionInFieldPracticeAreas { get; set; }

    public virtual DbSet<TalukMaster> TalukMasters { get; set; }

    public virtual DbSet<TblClassroomAvailability> TblClassroomAvailabilities { get; set; }

    public virtual DbSet<TblEquipmentDetail> TblEquipmentDetails { get; set; }

    public virtual DbSet<TblLaboratoryAvailability> TblLaboratoryAvailabilities { get; set; }

    public virtual DbSet<TblLaboratoryAvailability1> TblLaboratoryAvailabilities1 { get; set; }

    public virtual DbSet<TblMedicalEquipmentAvailability> TblMedicalEquipmentAvailabilities { get; set; }

    public virtual DbSet<TblMedicalSkillsLabEquipment> TblMedicalSkillsLabEquipments { get; set; }

    public virtual DbSet<TblRguhsFacultyUser> TblRguhsFacultyUsers { get; set; }

    public virtual DbSet<TeachingStaffDepartmentWiseDetail> TeachingStaffDepartmentWiseDetails { get; set; }

    public virtual DbSet<TrustDocumentDetail> TrustDocumentDetails { get; set; }

    public virtual DbSet<TrustDocumentMaster> TrustDocumentMasters { get; set; }

    public virtual DbSet<TrustMemberDetail> TrustMemberDetails { get; set; }

    public virtual DbSet<TypeOfAffiliation> TypeOfAffiliations { get; set; }

    public virtual DbSet<TypeOfMinorityMaster> TypeOfMinorityMasters { get; set; }

    public virtual DbSet<TypeOfOrganizationMaster> TypeOfOrganizationMasters { get; set; }

    public virtual DbSet<UgandPgrepository> UgandPgrepositories { get; set; }

    public virtual DbSet<Ugdetail> Ugdetails { get; set; }

    public virtual DbSet<UniversityImage> UniversityImages { get; set; }

    public virtual DbSet<YearwiseMaterialsDatum> YearwiseMaterialsData { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=.;Database=Admission_Affiliation;TrustServerCertificate=True;Trusted_Connection=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademicIntake>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Academic__3214EC07BB2858CE")
                .HasFillFactor(80);

            entity.ToTable("AcademicIntake");

            entity.Property(e => e.Ay2024ExistingIntake).HasColumnName("AY2024_ExistingIntake");
            entity.Property(e => e.Ay2024IncreaseIntake).HasColumnName("AY2024_IncreaseIntake");
            entity.Property(e => e.Ay2024TotalIntake).HasColumnName("AY2024_TotalIntake");
            entity.Property(e => e.Ay2025ExistingIntake).HasColumnName("AY2025_ExistingIntake");
            entity.Property(e => e.Ay2025LopDate).HasColumnName("AY2025_LopDate");
            entity.Property(e => e.Ay2025LopDocument).HasColumnName("AY2025_LopDocument");
            entity.Property(e => e.Ay2025LopNmcIntake).HasColumnName("AY2025_LopNmcIntake");
            entity.Property(e => e.Ay2025NmcDocument).HasColumnName("AY2025_NmcDocument");
            entity.Property(e => e.Ay2025TotalIntake).HasColumnName("AY2025_TotalIntake");
            entity.Property(e => e.Ay2026AddRequestedIntake).HasColumnName("AY2026_AddRequestedIntake");
            entity.Property(e => e.Ay2026ExistingIntake).HasColumnName("AY2026_ExistingIntake");
            entity.Property(e => e.Ay2026TotalIntake).HasColumnName("AY2026_TotalIntake");
            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.Courses).HasMaxLength(250);
            entity.Property(e => e.FacultyCode).HasMaxLength(20);
        });

        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.HasKey(e => e.AcademicYearId)
                .HasName("PK__Academic__11CFB974FEE9D078")
                .HasFillFactor(80);

            entity.HasIndex(e => e.YearLabel, "UQ__Academic__588C243A4B61DABB")
                .IsUnique()
                .HasFillFactor(80);

            entity.Property(e => e.AcademicYearId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("academic_year_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("active")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.YearLabel)
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("year_label");
        });

        modelBuilder.Entity<AdministrativeFacilityType>(entity =>
        {
            entity.HasKey(e => e.FacilityId).HasName("PK__Administ__5FB08A7465C6A277");

            entity.HasIndex(e => e.FacilityName, "UQ__Administ__16622C88303D4B1E").IsUnique();

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FacilityName).HasMaxLength(100);
            entity.Property(e => e.IsMandatory).HasDefaultValue(true);
            entity.Property(e => e.MinAreaSqM).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UpdatedDate).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<AffAdminTeachingBlock>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Aff_Admi__3214EC07CB42AB9F")
                .HasFillFactor(80);

            entity.ToTable("Aff_AdminTeachingBlock");

            entity.Property(e => e.CollegeCode).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(500);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Facilities).HasMaxLength(500);
            entity.Property(e => e.FacilityId).HasMaxLength(500);
            entity.Property(e => e.FacultyCode).HasMaxLength(500);
            entity.Property(e => e.IsAvailable).HasMaxLength(500);
            entity.Property(e => e.NoOfRooms).HasMaxLength(500);
            entity.Property(e => e.SizeSqFtAsPerNorms).HasMaxLength(500);
            entity.Property(e => e.SizeSqFtAvailablePerRoom).HasMaxLength(500);
        });

        modelBuilder.Entity<AffCourseDetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__AFF_Cour__3214EC07AF877DBB")
                .HasFillFactor(80);

            entity.ToTable("AFF_CourseDetails");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseId).HasMaxLength(50);
            entity.Property(e => e.CourseName).HasMaxLength(200);
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.RguhsNotificationNo).HasMaxLength(100);
        });

        modelBuilder.Entity<AffDeanAdministrativeExperience>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Aff_Dean__3214EC07E0B657EC");

            entity.ToTable("Aff_DeanAdministrativeExperience");

            entity.Property(e => e.Collegecode).HasMaxLength(100);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Facultycode).HasMaxLength(100);
            entity.Property(e => e.PostHeld).HasMaxLength(250);
            entity.Property(e => e.TotalExperienceYears).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Dean).WithMany(p => p.AffDeanAdministrativeExperiences)
                .HasForeignKey(d => d.DeanId)
                .HasConstraintName("FK__Aff_DeanA__DeanI__0CFADF99");
        });

        modelBuilder.Entity<AffDeanOrDirectorDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Aff_Dean__3214EC0772362609");

            entity.ToTable("Aff_DeanOrDirectorDetails");

            entity.Property(e => e.CollegeCode).HasMaxLength(100);
            entity.Property(e => e.CourseLevel).HasMaxLength(20);
            entity.Property(e => e.DeanOrDirectorName).HasMaxLength(250);
            entity.Property(e => e.DeanQualification).HasMaxLength(250);
            entity.Property(e => e.DeanStateCouncilNumber).HasMaxLength(250);
            entity.Property(e => e.DeanUniversity).HasMaxLength(225);
            entity.Property(e => e.FacultyCode).HasMaxLength(100);
            entity.Property(e => e.RecognizedByMci).HasColumnName("RecognizedByMCI");
        });

        modelBuilder.Entity<AffDeanTeachingExperience>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Aff_Dean__3214EC0783065F7B");

            entity.ToTable("Aff_DeanTeachingExperience");

            entity.Property(e => e.Collegecode).HasMaxLength(100);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.Facultycode).HasMaxLength(100);
            entity.Property(e => e.Pgfrom).HasColumnName("PGFrom");
            entity.Property(e => e.Pgto).HasColumnName("PGTo");
            entity.Property(e => e.TotalExperienceYears).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Ugfrom).HasColumnName("UGFrom");
            entity.Property(e => e.Ugto).HasColumnName("UGTo");

            entity.HasOne(d => d.Dean).WithMany(p => p.AffDeanTeachingExperiences)
                .HasForeignKey(d => d.DeanId)
                .HasConstraintName("FK__Aff_DeanT__DeanI__11BF94B6");
        });

        modelBuilder.Entity<AffHostelDetail>(entity =>
        {
            entity.HasKey(e => e.HostelDetailsId).HasName("PK__AFF_Host__E51556F824051D42");

            entity.ToTable("AFF_HostelDetails");

            entity.Property(e => e.AnyOtherFacility).HasMaxLength(500);
            entity.Property(e => e.BuiltUpAreaSqFt).HasMaxLength(200);
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.HostelFacilityDetails).HasMaxLength(1000);
            entity.Property(e => e.HostelType).HasMaxLength(50);
            entity.Property(e => e.OwnOrRented).HasMaxLength(50);
            entity.Property(e => e.SpacePerStudent).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalFemaleRooms).HasMaxLength(200);
            entity.Property(e => e.TotalFemaleStudents).HasMaxLength(200);
            entity.Property(e => e.TotalMaleRooms).HasMaxLength(200);
            entity.Property(e => e.TotalMaleStudents).HasMaxLength(200);
        });

        modelBuilder.Entity<AffHostelFacilityDetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Aff_Host__3214EC07F0A12BF7")
                .HasFillFactor(80);

            entity.ToTable("Aff_HostelFacilityDetails");

            entity.Property(e => e.CollegeCode).HasMaxLength(10);
            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.FacilityName).HasMaxLength(200);
            entity.Property(e => e.FacultyCode).HasMaxLength(10);
        });

        modelBuilder.Entity<AffInstitutionStatusMaster>(entity =>
        {
            entity.HasKey(e => e.InstitutionStatusId)
                .HasName("PK__Aff_Inst__F83AD2F1707F8015")
                .HasFillFactor(80);

            entity.ToTable("Aff_InstitutionStatusMaster");

            entity.HasIndex(e => e.StatusName, "UQ__Aff_Inst__05E7698A9D9A5200")
                .IsUnique()
                .HasFillFactor(80);

            entity.HasIndex(e => e.StatusName, "UQ__Aff_Inst__05E7698AAD90ADEB")
                .IsUnique()
                .HasFillFactor(80);

            entity.HasIndex(e => e.StatusName, "UQ__Aff_Inst__05E7698AB7674C5D")
                .IsUnique()
                .HasFillFactor(80);

            entity.HasIndex(e => e.StatusName, "UQ__Aff_Inst__05E7698AD7CEE27D")
                .IsUnique()
                .HasFillFactor(80);

            entity.HasIndex(e => e.StatusCode, "UQ__Aff_Inst__6A7B44FC063E7D1C")
                .IsUnique()
                .HasFillFactor(80);

            entity.HasIndex(e => e.StatusCode, "UQ__Aff_Inst__6A7B44FC69C0146F")
                .IsUnique()
                .HasFillFactor(80);

            entity.HasIndex(e => e.StatusCode, "UQ__Aff_Inst__6A7B44FC6BEC0888")
                .IsUnique()
                .HasFillFactor(80);

            entity.HasIndex(e => e.StatusCode, "UQ__Aff_Inst__6A7B44FCB5465083")
                .IsUnique()
                .HasFillFactor(80);

            entity.Property(e => e.InstitutionStatusId).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModifiedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.StatusCode).HasMaxLength(10);
            entity.Property(e => e.StatusName).HasMaxLength(100);
        });

        modelBuilder.Entity<AffInstitutionsDetail>(entity =>
        {
            entity.HasKey(e => e.InstitutionId)
                .HasName("PK__AFF_Inst__8DF6B6ADAA9E6BF3")
                .HasFillFactor(80);

            entity.ToTable("AFF_InstitutionsDetails");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.AltEmailId).HasMaxLength(500);
            entity.Property(e => e.AltLandlineMobile).HasMaxLength(500);
            entity.Property(e => e.CollegeCode).HasMaxLength(500);
            entity.Property(e => e.CollegeUrl)
                .HasMaxLength(250)
                .HasColumnName("College_URL");
            entity.Property(e => e.CourseApplied).HasMaxLength(500);
            entity.Property(e => e.CourseLevel).HasMaxLength(20);
            entity.Property(e => e.DeanEmailId).HasMaxLength(150);
            entity.Property(e => e.DeanMobileNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DeanName).HasMaxLength(150);
            entity.Property(e => e.District).HasMaxLength(500);
            entity.Property(e => e.DocumentContentType).HasMaxLength(500);
            entity.Property(e => e.DocumentName).HasMaxLength(500);
            entity.Property(e => e.EmailId).HasMaxLength(500);
            entity.Property(e => e.FacultyCode).HasMaxLength(500);
            entity.Property(e => e.Fax).HasMaxLength(500);
            entity.Property(e => e.FinancingAuthority).HasMaxLength(500);
            entity.Property(e => e.HeadAddress).HasMaxLength(500);
            entity.Property(e => e.HeadOfInstitution).HasMaxLength(500);
            entity.Property(e => e.HeadOfInstitutionEmail)
                .HasMaxLength(250)
                .HasColumnName("HeadOfInstitution_Email");
            entity.Property(e => e.HeadOfInstitutionMobNo)
                .HasMaxLength(250)
                .HasColumnName("HeadOfInstitution_Mob_NO");
            entity.Property(e => e.MinorityCategory).HasMaxLength(100);
            entity.Property(e => e.MobileNumber).HasMaxLength(500);
            entity.Property(e => e.NameOfInstitution).HasMaxLength(500);
            entity.Property(e => e.NodalOfficerEmail)
                .HasMaxLength(250)
                .HasColumnName("NodalOfficer_Email");
            entity.Property(e => e.NodalOfficerMobNumber)
                .HasMaxLength(250)
                .HasColumnName("NodalOfficer_Mob_Number");
            entity.Property(e => e.NodalOfficerName)
                .HasMaxLength(250)
                .HasColumnName("NodalOfficer_Name");
            entity.Property(e => e.PinCode).HasMaxLength(500);
            entity.Property(e => e.PrincipalEmail)
                .HasMaxLength(250)
                .HasColumnName("Principal_Email");
            entity.Property(e => e.PrincipalEmailId).HasMaxLength(150);
            entity.Property(e => e.PrincipalMobNo)
                .HasMaxLength(250)
                .HasColumnName("Principal_Mob_No");
            entity.Property(e => e.PrincipalMobileNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PrincipalName)
                .HasMaxLength(250)
                .HasColumnName("Principal_Name");
            entity.Property(e => e.RunningCourse).HasMaxLength(200);
            entity.Property(e => e.StatusOfCollege).HasMaxLength(500);
            entity.Property(e => e.StdCode).HasMaxLength(500);
            entity.Property(e => e.SurveyNoPidNo).HasMaxLength(500);
            entity.Property(e => e.Taluk).HasMaxLength(500);
            entity.Property(e => e.TrustAddress).HasMaxLength(500);
            entity.Property(e => e.TrustName).HasMaxLength(200);
            entity.Property(e => e.TrustPresidentContactNo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrustPresidentName).HasMaxLength(150);
            entity.Property(e => e.TypeOfInstitution).HasMaxLength(500);
            entity.Property(e => e.VillageTownCity).HasMaxLength(500);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.YearOfEstablishment).HasMaxLength(500);
        });

        modelBuilder.Entity<AffNonTeachingStaff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__Aff_NonT__96D4AB17FF6F1195");

            entity.ToTable("Aff_NonTeachingStaff");

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Designation)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MobileNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SalaryPaid).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StaffName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AffPrincipalAdministrativeExperience>(entity =>
        {
            entity.ToTable("Aff_PrincipalAdministrativeExperience");

            entity.Property(e => e.Collegecode).HasMaxLength(100);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Facultycode).HasMaxLength(100);
            entity.Property(e => e.PostHeld).HasMaxLength(250);
            entity.Property(e => e.TotalExperienceYears).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<AffPrincipalDetail>(entity =>
        {
            entity.ToTable("Aff_PrincipalDetails");

            entity.Property(e => e.CollegeCode).HasMaxLength(100);
            entity.Property(e => e.CourseLevel).HasMaxLength(20);
            entity.Property(e => e.DeanOrDirectorName).HasMaxLength(250);
            entity.Property(e => e.DeanQualification).HasMaxLength(250);
            entity.Property(e => e.DeanStateCouncilNumber).HasMaxLength(250);
            entity.Property(e => e.DeanUniversity).HasMaxLength(225);
            entity.Property(e => e.FacultyCode).HasMaxLength(100);
            entity.Property(e => e.RecognizedByMci).HasColumnName("RecognizedByMCI");
        });

        modelBuilder.Entity<AffPrincipalTeachingExperience>(entity =>
        {
            entity.ToTable("Aff_PrincipalTeachingExperience");

            entity.Property(e => e.Collegecode).HasMaxLength(100);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.Facultycode).HasMaxLength(100);
            entity.Property(e => e.Pgfrom).HasColumnName("PGFrom");
            entity.Property(e => e.Pgto).HasColumnName("PGTo");
            entity.Property(e => e.TotalExperienceYears).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Ugfrom).HasColumnName("UGFrom");
            entity.Property(e => e.Ugto).HasColumnName("UGTo");
        });

        modelBuilder.Entity<AffSanctionedIntakeForCourse>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Aff_Sanc__3214EC0745199E23")
                .HasFillFactor(80);

            entity.ToTable("Aff_SanctionedIntakeForCourse");

            entity.Property(e => e.CollegeCode).HasMaxLength(500);
            entity.Property(e => e.CourseName).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(500);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.EligibleSeatSlab).HasMaxLength(500);
            entity.Property(e => e.FacultyCode).HasMaxLength(500);
            entity.Property(e => e.SanctionedIntake).HasMaxLength(500);
        });

        modelBuilder.Entity<AffTeachingFacultyAllDetail>(entity =>
        {
            entity.HasKey(e => e.TeachingFacultyId)
                .HasName("PK__Aff_Teac__57305EAD4650C788")
                .HasFillFactor(80);

            entity.ToTable("Aff_TeachingFacultyAllDetails");

            entity.Property(e => e.AadhaarNumber).HasMaxLength(20);
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.Department).HasMaxLength(150);
            entity.Property(e => e.DepartmentDetails).HasMaxLength(400);
            entity.Property(e => e.Designation).HasMaxLength(150);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.ExaminerFor).HasMaxLength(400);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.IsRecognizedPgguide).HasColumnName("IsRecognizedPGGuide");
            entity.Property(e => e.Madetorecruit).HasColumnName("madetorecruit");
            entity.Property(e => e.Mobile).HasMaxLength(20);
            entity.Property(e => e.Nrtsnumber)
                .HasMaxLength(50)
                .HasColumnName("NRTSNumber");
            entity.Property(e => e.Pandocument).HasColumnName("PANDocument");
            entity.Property(e => e.Pannumber)
                .HasMaxLength(20)
                .HasColumnName("PANNumber");
            entity.Property(e => e.PginstituteName)
                .HasMaxLength(250)
                .HasColumnName("PGInstituteName");
            entity.Property(e => e.PgpassingSpecialization)
                .HasMaxLength(200)
                .HasColumnName("PGPassingSpecialization");
            entity.Property(e => e.PgyearOfPassing).HasColumnName("PGYearOfPassing");
            entity.Property(e => e.PhDrecognitionDoc).HasColumnName("PhDRecognitionDoc");
            entity.Property(e => e.Qualification).HasMaxLength(200);
            entity.Property(e => e.RecognizedPhDteacher).HasColumnName("RecognizedPhDTeacher");
            entity.Property(e => e.RemoveRemarks).HasMaxLength(400);
            entity.Property(e => e.Rguhstin)
                .HasMaxLength(50)
                .HasColumnName("RGUHSTIN");
            entity.Property(e => e.RnRmdocument).HasColumnName("RN_RMDocument");
            entity.Property(e => e.RnRmnumber)
                .HasMaxLength(50)
                .HasColumnName("RN_RMNumber");
            entity.Property(e => e.Subject).HasMaxLength(200);
            entity.Property(e => e.TeachingExperienceAfterPgyears)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("TeachingExperienceAfterPGYears");
            entity.Property(e => e.TeachingExperienceAfterUgyears)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("TeachingExperienceAfterUGYears");
            entity.Property(e => e.TeachingFacultyName).HasMaxLength(200);
            entity.Property(e => e.UginstituteName)
                .HasMaxLength(250)
                .HasColumnName("UGInstituteName");
            entity.Property(e => e.UgyearOfPassing).HasColumnName("UGYearOfPassing");
        });

        modelBuilder.Entity<AffiliatedHospitalDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId)
                .HasName("PK__Affiliat__1ABEEF0F2C130B6A")
                .HasFillFactor(80);

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel).HasMaxLength(20);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.DocumentName)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.HospitalName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.HospitalType)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AffiliatedYearwiseMaterialsDatum>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Affiliat__3214EC07E0D52D76")
                .HasFillFactor(80);

            entity.ToTable("Affiliated_Yearwise_MaterialsData");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.HospitalOwnerName).HasMaxLength(150);
            entity.Property(e => e.Kpmebeds)
                .HasMaxLength(10)
                .HasColumnName("KPMEBeds");
            entity.Property(e => e.ParametersName).HasMaxLength(200);
            entity.Property(e => e.ParentHospitalAddress)
                .HasMaxLength(250)
                .HasColumnName("parentHospitalAddress");
            entity.Property(e => e.ParentHospitalKpmebedsDoc).HasColumnName("parentHospitalKPMEbedsDoc");
            entity.Property(e => e.ParentHospitalMoudoc).HasColumnName("parentHospitalMOUdoc");
            entity.Property(e => e.ParentHospitalName)
                .HasMaxLength(150)
                .HasColumnName("parentHospitalName");
            entity.Property(e => e.ParentHospitalOwnerNameDoc).HasColumnName("parentHospitalOwnerNameDoc");
            entity.Property(e => e.ParentHospitalPostBasicDoc).HasColumnName("parentHospitalPostBasicDoc");
            entity.Property(e => e.PostBasicBeds).HasMaxLength(10);
            entity.Property(e => e.TotalBeds).HasMaxLength(10);
            entity.Property(e => e.Year1).HasMaxLength(50);
            entity.Property(e => e.Year2).HasMaxLength(50);
            entity.Property(e => e.Year3).HasMaxLength(50);
        });

        modelBuilder.Entity<AffiliationCollege>(entity =>
        {
            entity.HasKey(e => e.SlNo)
                .HasName("PK_Affiliation_College_Master")
                .HasFillFactor(80);

            entity.ToTable("Affiliation_Colleges");

            entity.Property(e => e.SlNo)
                .ValueGeneratedNever()
                .HasColumnName("SL_NO");
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(100)
                .HasColumnName("College_Code");
            entity.Property(e => e.CollegeName)
                .HasMaxLength(200)
                .HasColumnName("College_Name");
            entity.Property(e => e.CollegeTown)
                .HasMaxLength(200)
                .HasColumnName("College_Town");
        });

        modelBuilder.Entity<AffiliationCollegeMaster>(entity =>
        {
            entity.HasKey(e => e.CollegeCode)
                .HasName("PK_Affiliation_College_Master_1")
                .HasFillFactor(80);

            entity.ToTable("Affiliation_College_Master");

            entity.Property(e => e.CollegeCode).HasMaxLength(100);
            entity.Property(e => e.ChangedPassword).HasMaxLength(50);
            entity.Property(e => e.CollegeName).HasMaxLength(200);
            entity.Property(e => e.CollegeTown).HasMaxLength(200);
            entity.Property(e => e.DistrictId).HasMaxLength(150);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.HashedPassword).HasMaxLength(100);
            entity.Property(e => e.IsDeclared)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.PrincipalMobileNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PrincipalNameDeclared)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ShowIntakeDetails).HasColumnName("showIntakeDetails");
            entity.Property(e => e.ShowNodalOfficerDetails)
                .HasDefaultValue(true)
                .HasColumnName("showNodalOfficerDetails");
            entity.Property(e => e.ShowRepositoryDetails).HasColumnName("showRepositoryDetails");
            entity.Property(e => e.TalukId).HasMaxLength(150);
        });

        modelBuilder.Entity<AffiliationCourseDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Affiliat__3214EC07EEFCFB75");

            entity.ToTable("Affiliation_CourseDetails");

            entity.HasIndex(e => new { e.Facultycode, e.Collegecode, e.CourseId }, "UQ_MBBS_Per_College").IsUnique();

            entity.Property(e => e.ActionTakenOnDeficiencies).HasMaxLength(500);
            entity.Property(e => e.Collegecode).HasMaxLength(200);
            entity.Property(e => e.CourseId).HasMaxLength(250);
            entity.Property(e => e.CourseName).HasMaxLength(250);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getutcdate())");
            entity.Property(e => e.DateOfLoprenewalGoimci)
                .HasMaxLength(250)
                .HasColumnName("DateOfLOPRenewalGOIMCI");
            entity.Property(e => e.DateOfPreviousLicinspection).HasColumnName("DateOfPreviousLICInspection");
            entity.Property(e => e.Dateofrecognition).HasMaxLength(250);
            entity.Property(e => e.Facultycode).HasMaxLength(200);
            entity.Property(e => e.IntakeDuring202526).HasMaxLength(250);
            entity.Property(e => e.IntakeSlab).HasMaxLength(200);
            entity.Property(e => e.LastAffiliationRguhsfile).HasColumnName("LastAffiliationRGUHSFile");
            entity.Property(e => e.SanctionedIntakeLastAffiliation).HasMaxLength(250);
            entity.Property(e => e.SanctionedIntakePermission).HasMaxLength(250);
            entity.Property(e => e.SannctionedIntakeEcFc).HasMaxLength(250);
            entity.Property(e => e.Typeofpermission).HasMaxLength(100);
            entity.Property(e => e.YearOfLastAffiliationRguhs)
                .HasMaxLength(100)
                .HasColumnName("YearOfLastAffiliationRGUHS");
        });

        modelBuilder.Entity<AffiliationLicinpsection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Affiliation_LICinspection");

            entity.ToTable("Affiliation_LICinpsection");

            entity.Property(e => e.ActionTaken)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TypeOfAffiliation)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AffiliationOtherCoursesPermittedByNmc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Affiliat__3214EC07979E1D6C");

            entity.ToTable("Affiliation_OtherCoursesPermittedByNMC");

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NmcsupportingDocument).HasColumnName("NMCsupportingDocument");
            entity.Property(e => e.PermissionByNmc).HasColumnName("PermissionByNMC");
            entity.Property(e => e.TypeOfAffiliation)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AffiliationPgSsCourseDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Affiliat__3214EC07EEA3226E");

            entity.ToTable("Affiliation_PgSsCourseDetails");

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CoursePrefix)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.DateofRecognitionByNmc).HasColumnName("DateofRecognitionByNMC");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Lopdate).HasColumnName("LOPDate");
            entity.Property(e => e.TypeOfAffiliation)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AffiliationPgSsCourseDetailsForGok>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Affiliat__3214EC07DA6B8D65");

            entity.ToTable("Affiliation_PgSsCourseDetailsForGOK");

            entity.Property(e => e.AcademicYear)
                .HasMaxLength(9)
                .IsUnicode(false);
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CoursePrefix)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.DocumentofGok).HasColumnName("DocumentofGOK");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Gokdate).HasColumnName("GOKdate");
            entity.Property(e => e.TypeOfAffiliation)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AffiliationPgSsCourseDetailsRguh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Affiliat__3214EC0770FE851E");

            entity.ToTable("Affiliation_PgSsCourseDetailsRGUHS");

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RguhssupportingDocument).HasColumnName("RGUHSsupportingDocument");
            entity.Property(e => e.TypeOfAffiliation)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<AhsAffiliatedYearwiseMaterialsDatum>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__AHS_Affi__3214EC07F49DF780")
                .HasFillFactor(80);

            entity.ToTable("AHS_Affiliated_Yearwise_MaterialsData");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.HospitalOwnerName).HasMaxLength(150);
            entity.Property(e => e.HospitalType).HasMaxLength(150);
            entity.Property(e => e.Kpmebeds)
                .HasMaxLength(10)
                .HasColumnName("KPMEBeds");
            entity.Property(e => e.ParametersName).HasMaxLength(200);
            entity.Property(e => e.ParentHospitalAddress).HasMaxLength(250);
            entity.Property(e => e.ParentHospitalKpmebedsDoc).HasColumnName("ParentHospitalKPMEbedsDoc");
            entity.Property(e => e.ParentHospitalKspcdoc).HasColumnName("ParentHospitalKSPCDoc");
            entity.Property(e => e.ParentHospitalMoudoc).HasColumnName("ParentHospitalMOUdoc");
            entity.Property(e => e.ParentHospitalNabldoc).HasColumnName("ParentHospitalNABLDoc");
            entity.Property(e => e.ParentHospitalName).HasMaxLength(150);
            entity.Property(e => e.PostBasicBeds).HasMaxLength(10);
            entity.Property(e => e.TotalBeds).HasMaxLength(10);
            entity.Property(e => e.Year1).HasMaxLength(50);
            entity.Property(e => e.Year2).HasMaxLength(50);
            entity.Property(e => e.Year3).HasMaxLength(50);
        });

        modelBuilder.Entity<AhsExpectedIntakeMaster>(entity =>
        {
            entity.HasKey(e => new { e.IntakeId, e.CourseCode }).HasFillFactor(80);

            entity.ToTable("AHS_ExpectedIntakeMaster");

            entity.Property(e => e.IntakeId).HasColumnName("IntakeID");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ExpectedIntake)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsMedicalCollege)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MedcolexpectedIntake)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MEDCOLExpectedIntake");
            entity.Property(e => e.MedcolmaxSeats).HasColumnName("MEDCOLMaxSeats");
        });

        modelBuilder.Entity<AssociatedInstitution>(entity =>
        {
            entity.Property(e => e.AssociatedCollegeCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.AssociatedFacultyCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BasicDetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__BasicDet__3214EC0743CB9E85")
                .HasFillFactor(80);

            entity.Property(e => e.AadhaarNumber).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CertificateNumber).HasMaxLength(100);
            entity.Property(e => e.ChairmanName).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.ExistingTrustName).HasMaxLength(200);
            entity.Property(e => e.Fax).HasMaxLength(15);
            entity.Property(e => e.GoktrustName)
                .HasMaxLength(200)
                .HasColumnName("GOKTrustName");
            entity.Property(e => e.HasAmendments).HasMaxLength(10);
            entity.Property(e => e.HasOtherNursingCollege).HasMaxLength(10);
            entity.Property(e => e.LandLine).HasMaxLength(15);
            entity.Property(e => e.MobileNumber).HasMaxLength(15);
            entity.Property(e => e.OrganizationType).HasMaxLength(100);
            entity.Property(e => e.Panfile).HasColumnName("PANFile");
            entity.Property(e => e.Pannumber)
                .HasMaxLength(20)
                .HasColumnName("PANNumber");
            entity.Property(e => e.Pincode).HasMaxLength(10);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Stdcode)
                .HasMaxLength(10)
                .HasColumnName("STDCode");
            entity.Property(e => e.Taluk).HasMaxLength(100);
            entity.Property(e => e.TrustName).HasMaxLength(200);
            entity.Property(e => e.TrustNameChanged).HasMaxLength(10);
        });

        modelBuilder.Entity<BuildingTypeMaster>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("BuildingTypeMaster");

            entity.Property(e => e.BuildingName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FacultyId).HasColumnName("FacultyID");
        });

        modelBuilder.Entity<CaAcademicPerformance>(entity =>
        {
            entity.HasKey(e => e.AcademicPerformanceId).HasName("PK__CA_Acade__B11DC2CC07F2D5C2");

            entity.ToTable("CA_AcademicPerformance");

            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PassPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Remarks).HasMaxLength(500);
        });

        modelBuilder.Entity<CaCourseCurriculum>(entity =>
        {
            entity.HasKey(e => e.CourseCurriculumId).HasName("PK__CA_Cours__8DF27A2C01D5FA06");

            entity.ToTable("CA_CourseCurriculum");

            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CurriculumPdfPath).HasMaxLength(500);
            entity.Property(e => e.PdfFileName).HasMaxLength(200);
        });

        modelBuilder.Entity<CaCourseDetailsInFinancialDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_Cours__3214EC07C1EE4940");

            entity.ToTable("CA_CourseDetailsInFinancialDetails");

            entity.Property(e => e.ApexBodyPermissionAndIntakeFileName).HasMaxLength(250);
            entity.Property(e => e.CollegeCode).HasMaxLength(10);
            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FacultyCode).HasMaxLength(10);
            entity.Property(e => e.GoipermissionFile).HasColumnName("GOIPermissionFile");
            entity.Property(e => e.GoipermissionFileName)
                .HasMaxLength(250)
                .HasColumnName("GOIPermissionFileName");
            entity.Property(e => e.GoksanctionIntakeFile).HasColumnName("GOKSanctionIntakeFile");
            entity.Property(e => e.GoksanctionIntakeFileName)
                .HasMaxLength(250)
                .HasColumnName("GOKSanctionIntakeFileName");
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.RguhssanctionIntakeFile).HasColumnName("RGUHSSanctionIntakeFile");
            entity.Property(e => e.RguhssanctionIntakeFileName)
                .HasMaxLength(250)
                .HasColumnName("RGUHSSanctionIntakeFileName");
            entity.Property(e => e.YearOfStarting).HasMaxLength(50);
        });

        modelBuilder.Entity<CaDepartmentLibraryDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_Depar__3214EC07A7E48177");

            entity.ToTable("CA_DepartmentLibraryDetails");

            entity.Property(e => e.CollegeCode).HasMaxLength(10);
            entity.Property(e => e.DepartmentCode).HasMaxLength(10);
            entity.Property(e => e.FacultyCode).HasMaxLength(10);
            entity.Property(e => e.RegistrationNo).HasMaxLength(20);
        });

        modelBuilder.Entity<CaExaminationScheme>(entity =>
        {
            entity.HasKey(e => e.ExaminationSchemeId).HasName("PK__CA_Exami__EC2E8707EA14ED0D");

            entity.ToTable("CA_ExaminationScheme");

            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<CaFinancialDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_Finan__3214EC07BAF83EF2");

            entity.ToTable("CA_FinancialDetails");

            entity.Property(e => e.AccountBooksMaintained)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.AccountsDulyAudited)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.AnnualBudget).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AuditedExpenditureFileName).HasMaxLength(250);
            entity.Property(e => e.CollegeCode).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DepositsHeld).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FacultyCode).HasMaxLength(10);
            entity.Property(e => e.LibraryFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OthersFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.SportsFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TuitionFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnionFee).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<CaLibraryDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_Libra__3214EC07DB6C8CE5");

            entity.ToTable("CA_LibraryDetails");

            entity.Property(e => e.CollegeCode).HasMaxLength(10);
            entity.Property(e => e.FacultyCode).HasMaxLength(10);
            entity.Property(e => e.RegistrationNo).HasMaxLength(20);
            entity.Property(e => e.TotalBudget).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalEbooks).HasColumnName("TotalEBooks");
        });

        modelBuilder.Entity<CaLibraryService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_Libra__3214EC0722257043");

            entity.ToTable("CA_LibraryServices");

            entity.Property(e => e.CollegeCode).HasMaxLength(10);
            entity.Property(e => e.FacultyCode).HasMaxLength(10);
            entity.Property(e => e.RegistrationNo).HasMaxLength(20);
            entity.Property(e => e.ServiceName).HasMaxLength(200);
            entity.Property(e => e.Specify)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<CaLibraryStaffDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_Libra__3214EC076335630E");

            entity.ToTable("CA_LibraryStaffDetails");

            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.FacultyCode).HasMaxLength(20);
            entity.Property(e => e.Qualification).HasMaxLength(50);
            entity.Property(e => e.RegistrationNo).HasMaxLength(20);
            entity.Property(e => e.Remarks).HasMaxLength(400);
            entity.Property(e => e.StaffName).HasMaxLength(50);
        });

        modelBuilder.Entity<CaMedLibCommittee>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_Med_L__3214EC07F7A2E6CC");

            entity.ToTable("CA_Med_Lib_Committee");

            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CommitteePdfName).HasMaxLength(200);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.FacultyCode).HasMaxLength(20);
            entity.Property(e => e.IsPresent)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.SubFacultyCode).HasMaxLength(20);
        });

        modelBuilder.Entity<CaMedLibOtherAcademicActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_Med_L__3214EC07BC494598");

            entity.ToTable("CA_Med_Lib_OtherAcademicActivities");

            entity.Property(e => e.ActivityPdfName).HasMaxLength(200);
            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.DepartmentCode).HasMaxLength(20);
            entity.Property(e => e.DepartmentWise).HasMaxLength(200);
            entity.Property(e => e.FacultyCode).HasMaxLength(20);
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.SubFacultyCode).HasMaxLength(20);
        });

        modelBuilder.Entity<CaMedLibTechnicalProcess>(entity =>
        {
            entity.HasKey(e => new { e.SlNo, e.FacultyCode, e.CollegeCode, e.CourseLevel });

            entity.ToTable("CA_Med_LibTechnicalProcess");

            entity.Property(e => e.FacultyCode).HasMaxLength(25);
            entity.Property(e => e.CollegeCode).HasMaxLength(25);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.ProcessName).HasMaxLength(255);
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.SubFacultyCode).HasMaxLength(25);
            entity.Property(e => e.Value).HasMaxLength(500);
        });

        modelBuilder.Entity<CaMedLibraryBuilding>(entity =>
        {
            entity.HasKey(e => e.SlNo).HasName("PK__CA_Med_L__BC789CF2FFABC706");

            entity.ToTable("CA_Med_LibraryBuilding");

            entity.Property(e => e.AreaSqMtrs).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CollegeCode).HasMaxLength(25);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.FacultyCode).HasMaxLength(25);
            entity.Property(e => e.IsIndependent)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.SubFacultyCode).HasMaxLength(25);
        });

        modelBuilder.Entity<CaMedLibraryEquipment>(entity =>
        {
            entity.HasKey(e => new { e.SlNo, e.FacultyCode, e.CollegeCode }).HasName("PK__CA_Med_L__BF2295F6FD26E713");

            entity.ToTable("CA_Med_LibraryEquipments");

            entity.Property(e => e.FacultyCode).HasMaxLength(25);
            entity.Property(e => e.CollegeCode).HasMaxLength(25);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.EquipmentName).HasMaxLength(255);
            entity.Property(e => e.HasEquipment)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.SubFacultyCode).HasMaxLength(25);
        });

        modelBuilder.Entity<CaMedLibraryFinance>(entity =>
        {
            entity.HasKey(e => e.SlNo).HasName("PK__CA_Med_L__BC789CF2C8B9A020");

            entity.ToTable("CA_Med_LibraryFinance");

            entity.Property(e => e.CollegeCode).HasMaxLength(25);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.ExpenditureBooksLakhs).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FacultyCode).HasMaxLength(25);
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.SubFacultyCode).HasMaxLength(25);
            entity.Property(e => e.TotalBudgetLakhs).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<CaMedLibraryGeneral>(entity =>
        {
            entity.HasKey(e => new { e.FacultyCode, e.CollegeCode, e.CourseLevel }).HasName("PK_CA_MedLibraryGenerals");

            entity.ToTable("CA_Med_LibraryGeneral");

            entity.Property(e => e.FacultyCode).HasMaxLength(25);
            entity.Property(e => e.CollegeCode).HasMaxLength(25);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.DepartmentWiseLibrary)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DigitalLibrary)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.HelinetServices)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.LibraryEmailId)
                .HasMaxLength(255)
                .HasColumnName("LibraryEmailID");
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.SlNo).ValueGeneratedOnAdd();
            entity.Property(e => e.SubFacultyCode).HasMaxLength(25);
        });

        modelBuilder.Entity<CaMedLibraryItem>(entity =>
        {
            entity.HasKey(e => new { e.SlNo, e.FacultyCode, e.CollegeCode, e.CourseLevel });

            entity.ToTable("CA_Med_LibraryItems");

            entity.Property(e => e.FacultyCode).HasMaxLength(25);
            entity.Property(e => e.CollegeCode).HasMaxLength(25);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.ItemName).HasMaxLength(255);
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.SubFacultyCode).HasMaxLength(25);
        });

        modelBuilder.Entity<CaMedResearchPublicationsDetail>(entity =>
        {
            entity.HasKey(e => e.SlNo).HasName("PK__CA_Med_R__BC789CF26A40CA87");

            entity.ToTable("CA_Med_ResearchPublicationsDetails");

            entity.Property(e => e.ClinicalTrialsPdfName).HasMaxLength(200);
            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.FacultyCode).HasMaxLength(20);
            entity.Property(e => e.FacultyProjectsPdfName).HasMaxLength(200);
            entity.Property(e => e.FacultyRguhsfunded).HasColumnName("FacultyRGUHSFunded");
            entity.Property(e => e.Pi)
                .HasMaxLength(50)
                .HasColumnName("PI");
            entity.Property(e => e.ProjectsPdfName).HasMaxLength(200);
            entity.Property(e => e.PublicationsPdfName).HasMaxLength(200);
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.Rguhsfunded).HasColumnName("RGUHSFunded");
            entity.Property(e => e.StudentsProjectsPdfName).HasMaxLength(200);
            entity.Property(e => e.StudentsRguhsfunded).HasColumnName("StudentsRGUHSFunded");
            entity.Property(e => e.SubFacultyCode).HasMaxLength(20);
        });

        modelBuilder.Entity<CaMedStaffParticularsOther>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_Med_S__3214EC0749611D37");

            entity.ToTable("CA_Med_StaffParticularsOther");

            entity.Property(e => e.AcquittanceRegisterMaintained)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.AebasinspectionDayPdf).HasColumnName("AEBASInspectionDayPdf");
            entity.Property(e => e.AebasinspectionDayPdfName)
                .HasMaxLength(255)
                .HasColumnName("AEBASInspectionDayPdfName");
            entity.Property(e => e.AebaslastThreeMonthsPdf).HasColumnName("AEBASLastThreeMonthsPdf");
            entity.Property(e => e.AebaslastThreeMonthsPdfName)
                .HasMaxLength(255)
                .HasColumnName("AEBASLastThreeMonthsPdfName");
            entity.Property(e => e.CollegeCode).HasMaxLength(25);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.Esipdf).HasColumnName("ESIPdf");
            entity.Property(e => e.EsipdfName)
                .HasMaxLength(255)
                .HasColumnName("ESIPdfName");
            entity.Property(e => e.ExaminerDetailsAttached)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ExaminerDetailsPdfName).HasMaxLength(255);
            entity.Property(e => e.FacultyCode).HasMaxLength(25);
            entity.Property(e => e.ProvidentFundPdfName).HasMaxLength(255);
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.ServiceRegisterMaintained)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SubFacultyCode).HasMaxLength(25);
            entity.Property(e => e.TeachersUpdatedInEms)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("TeachersUpdatedInEMS");
            entity.Property(e => e.TeachersUpdatedPdfName).HasMaxLength(255);
        });

        modelBuilder.Entity<CaMedStaffParticularsOtherTemp>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_Med_S__3214EC07F92ACB65");

            entity.ToTable("CA_Med_StaffPArticularsOther_Temp");

            entity.Property(e => e.AcquittanceRegisterMaintained).HasMaxLength(10);
            entity.Property(e => e.AebasinspectionDayPdfName).HasMaxLength(255);
            entity.Property(e => e.AebaslastThreeMonthsPdfName).HasMaxLength(255);
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseLevel).HasMaxLength(20);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EsipdfName).HasMaxLength(255);
            entity.Property(e => e.ExaminerDetailsAttached).HasMaxLength(10);
            entity.Property(e => e.ExaminerDetailsPdfName).HasMaxLength(255);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.ProvidentFundPdfName).HasMaxLength(255);
            entity.Property(e => e.ServiceRegisterMaintained).HasMaxLength(10);
            entity.Property(e => e.TeachersUpdatedInEms).HasMaxLength(10);
            entity.Property(e => e.TeachersUpdatedPdfName).HasMaxLength(255);
        });

        modelBuilder.Entity<CaMedicalDepartmentLibrary>(entity =>
        {
            entity.HasKey(e => e.DepartmentalLibraryId).HasName("PK__CA_Medic__E8CE73C37B8194A4");

            entity.ToTable("CA_MedicalDepartmentLibrary");

            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DepartmentCode).HasMaxLength(20);
            entity.Property(e => e.LibraryStaff).HasMaxLength(500);
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
        });

        modelBuilder.Entity<CaMedicalLibraryOtherDetail>(entity =>
        {
            entity.HasKey(e => e.DigitalValuationId).HasName("PK__CA_Medic__9BA4BEF63ADF996D");

            entity.ToTable("CA_MedicalLibraryOtherDetails");

            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.HasCccameraSystem)
                .HasMaxLength(3)
                .HasColumnName("HasCCCameraSystem");
            entity.Property(e => e.HasDigitalValuationCentre).HasMaxLength(3);
            entity.Property(e => e.HasStableInternet).HasMaxLength(3);
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.SpecialFeaturesQuestion)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.UploadedFileName).HasMaxLength(255);
        });

        modelBuilder.Entity<CaMedicalLibraryService>(entity =>
        {
            entity.HasKey(e => e.LibraryServiceId).HasName("PK__CA_Medic__6311BE381BF76392");

            entity.ToTable("CA_MedicalLibraryServices");

            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsAvailable).HasMaxLength(3);
            entity.Property(e => e.RegistrationNo).HasMaxLength(15);
            entity.Property(e => e.UploadedFileName).HasMaxLength(255);
        });

        modelBuilder.Entity<CaMedicalLibraryStaff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_Medic__3214EC07D4C48BD2");

            entity.ToTable("CA_MedicalLibraryStaff");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.Qualification).HasMaxLength(100);
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.StaffName).HasMaxLength(100);
        });

        modelBuilder.Entity<CaMedicalLibraryUsageReport>(entity =>
        {
            entity.HasKey(e => e.UsageReportId).HasName("PK__CA_Medic__0DD1EF5F271CE46C");

            entity.ToTable("CA_MedicalLibraryUsageReport");

            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.UploadedFileName).HasMaxLength(255);
        });

        modelBuilder.Entity<CaMstCourseCurriculum>(entity =>
        {
            entity.HasKey(e => e.CurriculumId).HasName("PK__CA_MST_C__06C9FA1CACA9C671");

            entity.ToTable("CA_MST_CourseCurriculum");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CurriculumName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<CaMstExaminationScheme>(entity =>
        {
            entity.HasKey(e => e.SchemeId).HasName("PK__CA_MST_E__DB7E1A6244230B61");

            entity.ToTable("CA_MST_ExaminationScheme");

            entity.HasIndex(e => e.SchemeCode, "UQ__CA_MST_E__8B17EDD570E4580C").IsUnique();

            entity.HasIndex(e => e.SchemeCode, "UQ__CA_MST_E__8B17EDD5C08EAB90").IsUnique();

            entity.Property(e => e.SchemeCode).HasMaxLength(10);
        });

        modelBuilder.Entity<CaMstLibraryEquipmentsType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_MST_L__3214EC07D644D800");

            entity.ToTable("CA_MST_LibraryEquipmentsType");

            entity.Property(e => e.TypeOfEquipment).HasMaxLength(200);
        });

        modelBuilder.Entity<CaMstLibraryServicesList>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_MST_L__3214EC07A27A7775");

            entity.ToTable("CA_MST_LibraryServicesList");

            entity.Property(e => e.ServiceName).HasMaxLength(200);
        });

        modelBuilder.Entity<CaMstMedCommitteeName>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_MST_M__3214EC07DEF43FE1");

            entity.ToTable("CA_MST_Med_CommitteeNames");

            entity.Property(e => e.CommitteeName).HasMaxLength(200);
            entity.Property(e => e.FacultyCode).HasMaxLength(20);
            entity.Property(e => e.SubFacultyCode).HasMaxLength(20);
        });

        modelBuilder.Entity<CaMstMedLibTechnicalProcess>(entity =>
        {
            entity.HasKey(e => e.SlNo).HasName("PK__CA_MST_M__BC789CF235DB039D");

            entity.ToTable("CA_MST_Med_LibTechnicalProcess");

            entity.Property(e => e.FacultyCode)
                .HasMaxLength(25)
                .HasDefaultValue("1");
            entity.Property(e => e.ProcessName).HasMaxLength(255);
        });

        modelBuilder.Entity<CaMstMedLibraryEquipment>(entity =>
        {
            entity.HasKey(e => e.SlNo).HasName("PK__CA_MST_M__BC789CF29F44F8FD");

            entity.ToTable("CA_MST_Med_LibraryEquipments");

            entity.Property(e => e.EquipmentName).HasMaxLength(255);
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(25)
                .HasDefaultValue("1");
        });

        modelBuilder.Entity<CaMstMedLibraryItem>(entity =>
        {
            entity.HasKey(e => e.SlNo).HasName("PK__CA_MST_M__BC789CF2B732EE64");

            entity.ToTable("CA_MST_Med_LibraryItems");

            entity.Property(e => e.FacultyCode)
                .HasMaxLength(25)
                .HasDefaultValue("1");
            entity.Property(e => e.ItemName).HasMaxLength(255);
        });

        modelBuilder.Entity<CaMstMedOtherAcademicActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_MST_M__3214EC07D6678A33");

            entity.ToTable("CA_MST_Med_OtherAcademicActivities");

            entity.Property(e => e.ActivityName).HasMaxLength(200);
            entity.Property(e => e.DepartmentCode).HasMaxLength(20);
            entity.Property(e => e.FacultyCode).HasMaxLength(20);
        });

        modelBuilder.Entity<CaMstMediLibraryService>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__CA_MST_M__C51BB00AC6BCA14C");

            entity.ToTable("CA_MST_MediLibraryServices");

            entity.HasIndex(e => e.ServiceName, "UQ__CA_MST_M__A42B5F99A1C924BC").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ServiceName).HasMaxLength(255);
        });

        modelBuilder.Entity<CaMstRegisterRecord>(entity =>
        {
            entity.HasKey(e => e.RegisterRecordId).HasName("PK__CA_MST_R__03F933515B81F988");

            entity.ToTable("CA_MST_RegisterRecord");

            entity.Property(e => e.CourseLevel)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.RegisterName).HasMaxLength(300);
        });

        modelBuilder.Entity<CaMstUserDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_MST_U__3214EC070FC66CF6");

            entity.ToTable("CA_MST_UserDetails");

            entity.Property(e => e.CategoryName).HasMaxLength(200);
            entity.Property(e => e.FacultyCode).HasMaxLength(10);
        });

        modelBuilder.Entity<CaMstVdVehicleFor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_MST_V__3214EC078F22F43F");

            entity.ToTable("CA_MST_VD_VehicleFor");

            entity.Property(e => e.VehicleForCode).HasMaxLength(10);
            entity.Property(e => e.VehicleForName).HasMaxLength(100);
        });

        modelBuilder.Entity<CaMstYearOfStudy>(entity =>
        {
            entity.HasKey(e => e.YearOfStudyId).HasName("PK__CA_MST_Y__F043CBDDB6F7A931");

            entity.ToTable("CA_MST_YearOfStudy");

            entity.Property(e => e.YearName).HasMaxLength(50);
        });

        modelBuilder.Entity<CaSsAffiliationGrantedYear>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_SS_Af__3214EC07126C90C3");

            entity.ToTable("CA_SS_AffiliationGrantedYear");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseName).HasMaxLength(200);
            entity.Property(e => e.CoursesApplied).HasMaxLength(10);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FileName).HasMaxLength(300);
        });

        modelBuilder.Entity<CaSsLicpreviousInspection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_SS_LI__3214EC07E1946047");

            entity.ToTable("CA_SS_LICPreviousInspection");

            entity.Property(e => e.ActionTaken).HasMaxLength(500);
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseName).HasMaxLength(200);
            entity.Property(e => e.CoursesApplied).HasMaxLength(10);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<CaSsLopsavedDate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_SS_LO__3214EC07A1506C29");

            entity.ToTable("CA_SS_LOPSavedDate");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseName).HasMaxLength(200);
            entity.Property(e => e.CoursesApplied).HasMaxLength(10);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<CaSsOtherCoursesConducted>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_SS_Ot__3214EC0755F5403E");

            entity.ToTable("CA_SS_OtherCoursesConducted");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.CourseName).HasMaxLength(200);
            entity.Property(e => e.CoursesApplied).HasMaxLength(10);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.SanctionedIntake).HasMaxLength(100);
        });

        modelBuilder.Entity<CaSsPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_SS_Pe__3214EC07B43E0DA3");

            entity.ToTable("CA_SS_Permission");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseName).HasMaxLength(200);
            entity.Property(e => e.CoursesApplied).HasMaxLength(10);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FileName).HasMaxLength(300);
            entity.Property(e => e.PermissionStatus).HasMaxLength(20);
        });

        modelBuilder.Entity<CaStudentRegisterRecord>(entity =>
        {
            entity.HasKey(e => e.StudentRegisterRecordId).HasName("PK__CA_Stude__105ECE022B11D2A2");

            entity.ToTable("CA_StudentRegisterRecords");

            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsMaintained).HasMaxLength(10);
            entity.Property(e => e.RegisterRecord).HasMaxLength(20);
        });

        modelBuilder.Entity<CaVehicleDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CA_Vehic__3214EC07FBD299C1");

            entity.ToTable("CA_VehicleDetails");

            entity.Property(e => e.CollegeCode).HasMaxLength(10);
            entity.Property(e => e.CourseLevel).HasMaxLength(20);
            entity.Property(e => e.DrivingLicenseStatus)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FacultyCode).HasMaxLength(10);
            entity.Property(e => e.InsuranceStatus)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.RcBookStatus)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.RegistrationNo).HasMaxLength(20);
            entity.Property(e => e.VehicleForCode).HasMaxLength(10);
            entity.Property(e => e.VehicleRegNo).HasMaxLength(50);
        });

        modelBuilder.Entity<ClinicalDatum>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Clinical__3214EC071B32A9E7")
                .HasFillFactor(80);

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.ParametersName).HasMaxLength(200);
        });

        modelBuilder.Entity<ClinicalFacilityDocMaster>(entity =>
        {
            entity.HasKey(e => new { e.DocId, e.FacultyId }).HasFillFactor(80);

            entity.ToTable("ClinicalFacilityDocMaster");

            entity.Property(e => e.FacultyId).HasColumnName("FacultyID");
            entity.Property(e => e.DocumentName)
                .HasMaxLength(300)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ClinicalMaterialDatum>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Clinical__3214EC073DCC4AFD")
                .HasFillFactor(80);

            entity.Property(e => e.FacultyCode).HasMaxLength(150);
            entity.Property(e => e.ParametersName).HasMaxLength(200);
        });

        modelBuilder.Entity<CollegeCourseIntakeDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.Property(e => e.CollegeAddress).HasMaxLength(200);
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CollegeName).HasMaxLength(200);
            entity.Property(e => e.CourseCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CourseName).HasMaxLength(200);
            entity.Property(e => e.DocumentAffiliation).HasColumnName("Document_Affiliation");
            entity.Property(e => e.DocumentLop).HasColumnName("Document_LOP");
        });

        modelBuilder.Entity<CollegeDesignationDetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__CollegeD__3214EC075C27DE3B")
                .HasFillFactor(80);

            entity.Property(e => e.AvailableIntake).HasMaxLength(100);
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.DepartmentCode).HasMaxLength(50);
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.DesignationCode).HasMaxLength(50);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.Goksanctioned)
                .HasMaxLength(150)
                .HasColumnName("GOKsanctioned");
            entity.Property(e => e.PgPresentintake).HasMaxLength(150);
            entity.Property(e => e.PgRguhsintake)
                .HasMaxLength(150)
                .HasColumnName("pgRGUHSintake");
            entity.Property(e => e.Pggoksanctioned)
                .HasMaxLength(150)
                .HasColumnName("PGGOKSanctioned");
            entity.Property(e => e.RequiredIntake).HasMaxLength(100);
            entity.Property(e => e.SeatSlabId).HasMaxLength(50);
            entity.Property(e => e.UgPresentintake)
                .HasMaxLength(150)
                .HasColumnName("ugPresentintake");
            entity.Property(e => e.UgRguhsintake)
                .HasMaxLength(150)
                .HasColumnName("ugRGUHSintake");
        });

        modelBuilder.Entity<CollegeIntakeDetail>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.CollegeCode }).HasFillFactor(80);

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CollegeName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CourseCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CourseName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NoOfSeatsIntake).HasMaxLength(50);
            entity.Property(e => e.Remarks)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ContinuationTrustMemberDetail>(entity =>
        {
            entity.HasKey(e => e.SlNo)
                .HasName("PK__CONTINUA__BC789CF2B9955161")
                .HasFillFactor(80);

            entity.ToTable("CONTINUATION_TrustMemberDetails");

            entity.Property(e => e.CollegeCode).HasMaxLength(100);
            entity.Property(e => e.Designation)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.DesignationId)
                .HasMaxLength(250)
                .HasColumnName("Designation_Id");
            entity.Property(e => e.FacultyCode).HasMaxLength(100);
            entity.Property(e => e.MobileNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Qualification)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.TrustMemberName)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CourseIntakeDetail>(entity =>
        {
            entity.HasKey(e => e.IntakeId).HasFillFactor(80);

            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.CourseName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CourseMaster>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("CourseMaster");

            entity.Property(e => e.CourseCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(6)
                .IsUnicode(false);
            entity.Property(e => e.CourseName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FacultyId).HasColumnName("FacultyID");
        });

        modelBuilder.Entity<CoursesOffered>(entity =>
        {
            entity.ToTable("CoursesOffered");

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Remarks)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.YearOfStarting)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DepartmentMaster>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Departme__3214EC07CC6D57E5")
                .HasFillFactor(80);

            entity.ToTable("DepartmentMaster");

            entity.Property(e => e.DepartmentCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DepartmentWiseFacultyMaster>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Departme__3214EC077B87CEF8")
                .HasFillFactor(80);

            entity.ToTable("DepartmentWiseFacultyMaster");

            entity.Property(e => e.DepartmentCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DesignationCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SeatSlabId)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DesignationMaster>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Designat__3214EC079DAEB187")
                .HasFillFactor(80);

            entity.ToTable("DesignationMaster");

            entity.Property(e => e.DesignationCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DesignationName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<DistrictMaster>(entity =>
        {
            entity.HasKey(e => e.DistrictId)
                .HasName("PK__District__85FDA4A653B05610")
                .HasFillFactor(80);

            entity.ToTable("DistrictMaster");

            entity.HasIndex(e => e.DistrictName, "UQ__District__F4708CA452576EBF")
                .IsUnique()
                .HasFillFactor(80);

            entity.Property(e => e.DistrictId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("DistrictID");
            entity.Property(e => e.DistrictName).HasMaxLength(100);
            entity.Property(e => e.FromPostalCode)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.StateId)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.ToPostalCode)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasKey(e => e.FacultyId)
                .HasName("PK_MST_FACULTY")
                .HasFillFactor(80);

            entity.ToTable("Faculty");

            entity.Property(e => e.FacultyId).ValueGeneratedNever();
            entity.Property(e => e.EmsFacultyId).HasColumnName("EMS_FacultyId");
            entity.Property(e => e.FacultyAbbre)
                .HasMaxLength(2)
                .IsUnicode(false)
                .HasColumnName("Faculty_Abbre");
            entity.Property(e => e.FacultyName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<FacultyDetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__FacultyD__3214EC074E091831")
                .HasFillFactor(80);

            entity.Property(e => e.Aadhaar).HasMaxLength(20);
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.DepartmentDetails).HasMaxLength(100);
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.ExaminerFor).HasMaxLength(150);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.IsExaminer).HasMaxLength(150);
            entity.Property(e => e.LitigationPending).HasMaxLength(150);
            entity.Property(e => e.Mobile).HasMaxLength(15);
            entity.Property(e => e.NameOfFaculty).HasMaxLength(200);
            entity.Property(e => e.Pan).HasMaxLength(20);
            entity.Property(e => e.PhDrecognitionDoc).HasColumnName("PhDRecognitionDoc");
            entity.Property(e => e.RecognizedPgTeacher).HasMaxLength(50);
            entity.Property(e => e.RecognizedPhDteacher)
                .HasMaxLength(150)
                .HasColumnName("RecognizedPhDTeacher");
            entity.Property(e => e.RemoveRemarks).HasMaxLength(150);
            entity.Property(e => e.Subject).HasMaxLength(200);
        });

        modelBuilder.Entity<FacultyExamResult>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__FacultyE__3213E83FA99E200C")
                .HasFillFactor(80);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Collegecode)
                .HasMaxLength(50)
                .HasColumnName("collegecode");
            entity.Property(e => e.Course).HasMaxLength(100);
            entity.Property(e => e.ExamappearedCount).HasColumnName("examappearedCount");
            entity.Property(e => e.Facultycode)
                .HasMaxLength(50)
                .HasColumnName("facultycode");
            entity.Property(e => e.Passedoutcount).HasColumnName("passedoutcount");
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Yearofpercentage)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("yearofpercentage");
        });

        modelBuilder.Entity<FeesMaster>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("FeesMaster");

            entity.Property(e => e.CourseCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Fees).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Scstfees)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("SCSTFees");
            entity.Property(e => e.SeatSlabId).HasColumnName("SeatSlabID");
        });

        modelBuilder.Entity<FeesType>(entity =>
        {
            entity.HasKey(e => e.FeesCode).HasFillFactor(80);

            entity.ToTable("FeesType");

            entity.Property(e => e.FeesCode).ValueGeneratedNever();
            entity.Property(e => e.Fees).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FeesType1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("FeesType");
        });

        modelBuilder.Entity<FellowShipMedical>(entity =>
        {
            entity.ToTable("FellowShip_Medical");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdmissionOpeningDate).HasColumnName("Admission_openingDate");
            entity.Property(e => e.AppointmentLetterDoc).HasColumnName("AppointmentLetter_Doc");
            entity.Property(e => e.ApprovalRemark).HasMaxLength(500);
            entity.Property(e => e.ApprovalStatus)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Collegecode).HasMaxLength(50);
            entity.Property(e => e.Course).HasMaxLength(150);
            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.DrApprovalRemark)
                .HasMaxLength(500)
                .HasColumnName("DR_ApprovalRemark");
            entity.Property(e => e.DrApprovalStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DR_ApprovalStatus");
            entity.Property(e => e.ExperienceLetterDoc).HasColumnName("Experience_Letter_Doc");
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.FellowshipCode).HasMaxLength(150);
            entity.Property(e => e.KmcCertificateNumber)
                .HasMaxLength(150)
                .HasColumnName("KMC_CertificateNumber");
            entity.Property(e => e.KmcDoc).HasColumnName("KMC_Doc");
            entity.Property(e => e.PrincipalDeclaration)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("principal_Declaration");
            entity.Property(e => e.PrincipalName)
                .HasMaxLength(200)
                .HasColumnName("principal_name");
            entity.Property(e => e.SslcDoc).HasColumnName("SSLC_Doc");
            entity.Property(e => e.StudentName).HasMaxLength(150);
        });

        modelBuilder.Entity<FreshOrIncreaseMaster>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK_FreshOrIncrease")
                .HasFillFactor(80);

            entity.ToTable("FreshOrIncreaseMaster");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<HealthCenterChp>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK_MST_HealthCenter_CHP")
                .HasFillFactor(80);

            entity.ToTable("HealthCenter_CHP");

            entity.Property(e => e.AdministrationType)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FieldType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NameofHealthCenter)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.PlanningType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Planning_Type");
            entity.Property(e => e.ServicesRendered)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.HealthCenterChps)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MST_HealthCenter_CHP_FacultyCode");
        });

        modelBuilder.Entity<HospitalDetailsForAffiliation>(entity =>
        {
            entity.HasKey(e => e.HospitalDetailsId)
                .HasName("PK__Hospital__CC90A013776B1379")
                .HasFillFactor(80);

            entity.ToTable("HospitalDetailsForAffiliation");

            entity.Property(e => e.HospitalDetailsId).HasColumnName("HospitalDetailsID");
            entity.Property(e => e.AnnualIpdprevYear).HasColumnName("AnnualIPDPrevYear");
            entity.Property(e => e.AnnualOpdprevYear).HasColumnName("AnnualOPDPrevYear");
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel).HasMaxLength(20);
            entity.Property(e => e.DistanceBetweenCollegeAndHospitalKm)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("DistanceBetweenCollegeAndHospitalKM");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.HospitalDistrictId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("HospitalDistrictID");
            entity.Property(e => e.HospitalName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.HospitalOwnedBy)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.HospitalOwnerName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.HospitalTalukId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("HospitalTalukID");
            entity.Property(e => e.HospitalType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.IpdbedOccupancyPercent)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("IPDBedOccupancyPercent");
            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.OpdperDay).HasColumnName("OPDPerDay");

            entity.HasOne(d => d.AffiliationType).WithMany(p => p.HospitalDetailsForAffiliations)
                .HasForeignKey(d => d.AffiliationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HD_AffiliationType");

            entity.HasOne(d => d.HospitalDistrict).WithMany(p => p.HospitalDetailsForAffiliations)
                .HasForeignKey(d => d.HospitalDistrictId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HD_District");
        });

        modelBuilder.Entity<HospitalDocumentDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.ToTable("Hospital_Document_Details");

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.HospitalDocumentDetails)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Hospital_Document_Details_FacultyCode");
        });

        modelBuilder.Entity<HospitalDocumentsToBeUploaded>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.ToTable("HospitalDocumentsToBeUploaded");

            entity.Property(e => e.CertificateNumber)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.DocumentName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.HospitalName)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.HospitalDetails).WithMany(p => p.HospitalDocumentsToBeUploadeds)
                .HasForeignKey(d => d.HospitalDetailsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HospitalDocumentsToBeUploaded_HospitalDetailsId");
        });

        modelBuilder.Entity<HospitalFacilitiesMaster>(entity =>
        {
            entity.HasKey(e => e.FacilityId)
                .HasName("PK__Hospital__5FB08B9490164BF0")
                .HasFillFactor(80);

            entity.ToTable("HospitalFacilitiesMaster");

            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.FacilityName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<HospitalFacility>(entity =>
        {
            entity.HasKey(e => new { e.HospitalDetailsId, e.FacilityId }).HasFillFactor(80);

            entity.Property(e => e.HospitalDetailsId).HasColumnName("HospitalDetailsID");
            entity.Property(e => e.FacilityId).HasColumnName("FacilityID");
            entity.Property(e => e.CourseLevel).HasMaxLength(20);

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.HospitalFacilities)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HospitalFacilities_Faculty");

            entity.HasOne(d => d.HospitalDetails).WithMany(p => p.HospitalFacilities)
                .HasForeignKey(d => d.HospitalDetailsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HF_Hospital");
        });

        modelBuilder.Entity<IndoorBedsOccupancy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_IndoorBedsOccupancy_Id");

            entity.ToTable("IndoorBedsOccupancy");

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Rguhsintake).HasColumnName("RGUHSintake");
            entity.Property(e => e.SeatSlabId)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.AffiliationType).WithMany(p => p.IndoorBedsOccupancies)
                .HasForeignKey(d => d.AffiliationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndoorBedsOccupancy_AffiliationTypeId");

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.IndoorBedsOccupancies)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndoorBedsOccupancy_FacultyCode");
        });

        modelBuilder.Entity<IndoorInfrastructureRequirementsCompliance>(entity =>
        {
            entity.ToTable("IndoorInfrastructureRequirementsCompliance");

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.InspectedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Remarks)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.SectionCode)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.AffiliationType).WithMany(p => p.IndoorInfrastructureRequirementsCompliances)
                .HasForeignKey(d => d.AffiliationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InfraComp_Affiliation");

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.IndoorInfrastructureRequirementsCompliances)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InfraComp_Faculty");

            entity.HasOne(d => d.HospitalDetails).WithMany(p => p.IndoorInfrastructureRequirementsCompliances)
                .HasForeignKey(d => d.HospitalDetailsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InfraComp_Hospital");
        });

        modelBuilder.Entity<InitiativeMaster>(entity =>
        {
            entity.HasKey(e => e.InitiativeId).HasFillFactor(80);

            entity.ToTable("InitiativeMaster");

            entity.Property(e => e.InitiativeId)
                .HasMaxLength(4)
                .IsUnicode(false)
                .HasColumnName("InitiativeID");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.InitiativeName).HasMaxLength(500);
        });

        modelBuilder.Entity<InstitutionBasicDetail>(entity =>
        {
            entity.HasKey(e => e.InstitutionId).HasFillFactor(80);

            entity.Property(e => e.AadhaarNumber).HasMaxLength(20);
            entity.Property(e => e.AcademicYearStarted).HasMaxLength(20);
            entity.Property(e => e.AddressOfInstitution).HasMaxLength(300);
            entity.Property(e => e.AltEmailId).HasMaxLength(150);
            entity.Property(e => e.AltLandlineOrMobile).HasMaxLength(20);
            entity.Property(e => e.CategoryOfOrganisation).HasMaxLength(100);
            entity.Property(e => e.CollegeCode).HasMaxLength(10);
            entity.Property(e => e.CollegeStatus).HasMaxLength(100);
            entity.Property(e => e.ContactPersonMobile).HasMaxLength(20);
            entity.Property(e => e.ContactPersonName).HasMaxLength(200);
            entity.Property(e => e.ContactPersonRelation).HasMaxLength(200);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.EmailId).HasMaxLength(150);
            entity.Property(e => e.ExistingTrustName).HasMaxLength(200);
            entity.Property(e => e.FacultyCode).HasMaxLength(20);
            entity.Property(e => e.Fax).HasMaxLength(20);
            entity.Property(e => e.FinancingAuthorityName).HasMaxLength(200);
            entity.Property(e => e.GokobtainedTrustName)
                .HasMaxLength(200)
                .HasColumnName("GOKObtainedTrustName");
            entity.Property(e => e.GovAutonomousCertNumber).HasMaxLength(100);
            entity.Property(e => e.HeadOfInstitutionAddress).HasMaxLength(300);
            entity.Property(e => e.HeadOfInstitutionName).HasMaxLength(150);
            entity.Property(e => e.KncCertificateNumber).HasMaxLength(100);
            entity.Property(e => e.MobileNumber).HasMaxLength(20);
            entity.Property(e => e.NameOfInstitution).HasMaxLength(200);
            entity.Property(e => e.Panfile).HasColumnName("PANFile");
            entity.Property(e => e.Pannumber)
                .HasMaxLength(20)
                .HasColumnName("PANNumber");
            entity.Property(e => e.PinCode).HasMaxLength(10);
            entity.Property(e => e.PresidentName).HasMaxLength(100);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.StdCode).HasMaxLength(10);
            entity.Property(e => e.Taluk).HasMaxLength(100);
            entity.Property(e => e.TrustName).HasMaxLength(200);
            entity.Property(e => e.TypeOfInstitution).HasMaxLength(100);
            entity.Property(e => e.TypeOfOrganization).HasMaxLength(100);
            entity.Property(e => e.VillageTownCity).HasMaxLength(100);
            entity.Property(e => e.Website).HasMaxLength(200);
        });

        modelBuilder.Entity<InstitutionDetail>(entity =>
        {
            entity.HasKey(e => e.InstitutionId)
                .HasName("PK_InstitutionDetails_1")
                .HasFillFactor(80);

            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.AlternateContactNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.AlternateEmailId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DistrictCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.EmailId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Fax)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MobileNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.NameOfInstitution)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PinCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StateCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Stdcode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("STDcode");
            entity.Property(e => e.TalukCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Village)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Website)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<InstitutionType>(entity =>
        {
            entity.HasKey(e => new { e.InstitutionTypeId, e.FacultyId })
                .HasName("PK_InstitutionDetails")
                .HasFillFactor(80);

            entity.ToTable("InstitutionType");

            entity.Property(e => e.InstitutionTypeId).ValueGeneratedOnAdd();
            entity.Property(e => e.InstitutionType1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("InstitutionType");
        });

        modelBuilder.Entity<IntakeDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.Property(e => e.Course)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Degree)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FreshOrContinuation)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Lopdate).HasColumnName("LOPdate");

            entity.HasOne(d => d.Institution).WithMany(p => p.IntakeDetails)
                .HasForeignKey(d => d.InstitutionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_IntakeDetails_InstitutionDetails");
        });

        modelBuilder.Entity<IntakeDetailsLatest>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK_IntakeDetailsLatest_ID")
                .HasFillFactor(80);

            entity.ToTable("IntakeDetailsLatest");

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CourseCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseRequestingYear).HasColumnName("course_requesting_year");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExistingIntakeCa).HasColumnName("ExistingIntakeCA");
            entity.Property(e => e.ExsistingIntake2425)
                .HasMaxLength(150)
                .HasColumnName("exsisting_Intake2425");
            entity.Property(e => e.ExsistingIntake2526)
                .HasMaxLength(150)
                .HasColumnName("exsisting_Intake2526");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.IsDeclared).HasDefaultValue(0);
            entity.Property(e => e.Nmcdata)
                .HasMaxLength(150)
                .HasColumnName("NMCDATA");
            entity.Property(e => e.Nmcdoc).HasColumnName("NMCDOC");
            entity.Property(e => e.Nmcfor202526).HasColumnName("NMCfor202526");
            entity.Property(e => e.PrincipalName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.RequestingIntake26)
                .HasMaxLength(150)
                .HasColumnName("requestingIntake26");
            entity.Property(e => e.UpdatedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<IntakeMaster>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("IntakeMaster");

            entity.Property(e => e.IntakeCount)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<LandBuildingDetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__LandBuil__3214EC071CC750F5")
                .HasFillFactor(80);

            entity.Property(e => e.ApprovalCertNo).HasMaxLength(255);
            entity.Property(e => e.Auditorium).HasMaxLength(20);
            entity.Property(e => e.BlueprintCertNo).HasMaxLength(255);
            entity.Property(e => e.BuildingType).HasMaxLength(50);
            entity.Property(e => e.CoursesInBuilding).HasMaxLength(10);
            entity.Property(e => e.CourtCase).HasMaxLength(10);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Electricity).HasMaxLength(20);
            entity.Property(e => e.LandAcres).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OccupancyCertNo).HasMaxLength(255);
            entity.Property(e => e.OfficeFacilities).HasMaxLength(20);
            entity.Property(e => e.OwnerName).HasMaxLength(255);
            entity.Property(e => e.Rr)
                .HasMaxLength(255)
                .HasColumnName("RR");
            entity.Property(e => e.Rtc).HasColumnName("RTC");
            entity.Property(e => e.RtccertNo)
                .HasMaxLength(255)
                .HasColumnName("RTCCertNo");
            entity.Property(e => e.SaleDeedCertNo).HasMaxLength(255);
            entity.Property(e => e.Seating).HasMaxLength(20);
            entity.Property(e => e.Survey).HasMaxLength(255);
            entity.Property(e => e.TaxCertNo).HasMaxLength(255);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.WaterSupply).HasMaxLength(20);
        });

        modelBuilder.Entity<LatestExcelAff>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.ToTable("LatestExcelAff");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CollegeName).HasMaxLength(300);
            entity.Property(e => e.CourseName).HasMaxLength(200);
        });

        modelBuilder.Entity<LicInspection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LIC_Insp__3214EC0799265EB8");

            entity.ToTable("LIC_Inspection");

            entity.Property(e => e.AadhaarNumber).HasMaxLength(12);
            entity.Property(e => e.AccountHolderName).HasMaxLength(100);
            entity.Property(e => e.AccountNumber).HasMaxLength(40);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.AttendanceFilePath).HasMaxLength(500);
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.BranchName).HasMaxLength(150);
            entity.Property(e => e.CollegeName).HasMaxLength(200);
            entity.Property(e => e.CreatedPassword).HasMaxLength(100);
            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FromPlace).HasMaxLength(200);
            entity.Property(e => e.Ifsccode)
                .HasMaxLength(11)
                .HasColumnName("IFSCCode");
            entity.Property(e => e.IsCompleted).HasDefaultValue(false);
            entity.Property(e => e.MemberCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModeOfTravel).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Pannumber)
                .HasMaxLength(10)
                .HasColumnName("PANNumber");
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ReturnFromPlace)
                .HasMaxLength(50)
                .HasColumnName("Return_fromPlace");
            entity.Property(e => e.ReturnKilometers).HasMaxLength(50);
            entity.Property(e => e.ReturnToPlace)
                .HasMaxLength(50)
                .HasColumnName("Return_ToPlace");
            entity.Property(e => e.ToPlace).HasMaxLength(200);
            entity.Property(e => e.TotalCost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TypeofMember).HasMaxLength(50);
        });

        modelBuilder.Entity<LicInspectionCollegeDetail>(entity =>
        {
            entity.ToTable("LIC_InspectionCollege_Details");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AcMemberPhno).HasColumnName("AcMember_Phno");
            entity.Property(e => e.AcademicYear)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Acmember)
                .HasMaxLength(150)
                .HasColumnName("ACMember");
            entity.Property(e => e.CollegePlace).HasMaxLength(150);
            entity.Property(e => e.Collegecode).HasMaxLength(50);
            entity.Property(e => e.Collegename).HasMaxLength(100);
            entity.Property(e => e.Facultycode).HasColumnName("facultycode");
            entity.Property(e => e.SeRevisedOrder).HasColumnName("SE_RevisedOrder");
            entity.Property(e => e.SenetMember).HasMaxLength(150);
            entity.Property(e => e.SenetMemberPhNo).HasColumnName("SenetMember_PhNo");
            entity.Property(e => e.SubjectExpertise).HasMaxLength(150);
            entity.Property(e => e.SubjectExpertisePhNo).HasColumnName("SubjectExpertise_PhNo");
        });

        modelBuilder.Entity<LicInspectionOtherDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LicInspe__3214EC0709FD3DBC");

            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CollegeName).HasMaxLength(200);
            entity.Property(e => e.MemberCode).HasMaxLength(50);
            entity.Property(e => e.MemberName).HasMaxLength(150);
            entity.Property(e => e.Phonenumber).HasMaxLength(20);
            entity.Property(e => e.SenateCode).HasMaxLength(50);
        });

        modelBuilder.Entity<LicclaimDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LICClaim__3214EC07A54FC362");

            entity.ToTable("LICClaimDetails");

            entity.Property(e => e.AirFare).HasMaxLength(150);
            entity.Property(e => e.AirFareCost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AirRoadCost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CollegeCost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Dacost)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DACost");
            entity.Property(e => e.Division).HasMaxLength(50);
            entity.Property(e => e.Faculty).HasMaxLength(50);
            entity.Property(e => e.FromPlace).HasMaxLength(150);
            entity.Property(e => e.InspectionDate).HasColumnName("inspectionDate");
            entity.Property(e => e.IsLca).HasColumnName("IsLCA");
            entity.Property(e => e.Kilometers).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Lcacost)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("LCACost");
            entity.Property(e => e.MemberName).HasMaxLength(150);
            entity.Property(e => e.ModeOfTravel).HasMaxLength(50);
            entity.Property(e => e.NoofDays).HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ReturnFromPlace).HasMaxLength(150);
            entity.Property(e => e.ReturnKilometers).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ReturnToPlace).HasMaxLength(150);
            entity.Property(e => e.ToPlace).HasMaxLength(150);
            entity.Property(e => e.TotalCost).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TravelCost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TypeofMember).HasMaxLength(100);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<LiccollegeApproval>(entity =>
        {
            entity.ToTable("LICCollegeApproval");

            entity.Property(e => e.AcademicYear)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.AirFair).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.AirRoadCost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CashierApprovedDate).HasColumnType("datetime");
            entity.Property(e => e.CashierUpdate)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("Cashier_Update");
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CollegeCost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Dacost)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("DACost");
            entity.Property(e => e.Division).HasMaxLength(50);
            entity.Property(e => e.DrApprovalStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("DR_ApprovalStatus");
            entity.Property(e => e.DrRemarks)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("DR_Remarks");
            entity.Property(e => e.DrapprovedBy)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("DRApprovedBy");
            entity.Property(e => e.DrapprovedDate)
                .HasColumnType("datetime")
                .HasColumnName("DRApprovedDate");
            entity.Property(e => e.FAoSpApprovedDate)
                .HasColumnType("datetime")
                .HasColumnName("F_AO_SP_Approved_Date");
            entity.Property(e => e.FAoSpApprovedRemarks)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("F_AO_SP_Approved_Remarks");
            entity.Property(e => e.FAoSpApprovedStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("F_AO_SP_Approved_Status");
            entity.Property(e => e.FCaseWorkerApproveRemarks)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("F_CaseWorker_Approve_Remarks");
            entity.Property(e => e.FCaseWorkerApproveStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("F_CaseWorker_Approve_Status");
            entity.Property(e => e.FCaseWorkerApprovedDate)
                .HasColumnType("datetime")
                .HasColumnName("F_CaseWorker_Approved_Date");
            entity.Property(e => e.FoLevel1ApprovedStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("FO_Level1_ApprovedStatus");
            entity.Property(e => e.FoLevel1ForwardedDate)
                .HasColumnType("datetime")
                .HasColumnName("FO_Level1_ForwardedDate");
            entity.Property(e => e.FoLevel1Remarks)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("FO_Level1_Remarks");
            entity.Property(e => e.FoLevel2ApprovedDate)
                .HasColumnType("datetime")
                .HasColumnName("FO_Level2_ApprovedDate");
            entity.Property(e => e.FoLevel2ApprovedRemarks)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("FO_Level2_ApprovedRemarks");
            entity.Property(e => e.FoLevel2ApprovedStatus)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("FO_Level2_ApprovedStatus");
            entity.Property(e => e.FromPlace).HasMaxLength(200);
            entity.Property(e => e.IsLca).HasColumnName("IsLCA");
            entity.Property(e => e.Kilometers).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Lcacost)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("LCACost");
            entity.Property(e => e.LicApprovalFileName).HasMaxLength(200);
            entity.Property(e => e.LicApprovalUploadedOn).HasColumnType("datetime");
            entity.Property(e => e.MemberName)
                .HasMaxLength(50)
                .HasColumnName("memberName");
            entity.Property(e => e.MobileNo).HasMaxLength(12);
            entity.Property(e => e.NoOfDays).HasMaxLength(150);
            entity.Property(e => e.ReturnFromPlace).HasMaxLength(200);
            entity.Property(e => e.ReturnKilometers).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ReturnToPlace).HasMaxLength(200);
            entity.Property(e => e.ToPlace).HasMaxLength(200);
            entity.Property(e => e.TotalClaimAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TravelCost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TypeOfMembers).HasMaxLength(50);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<LicinspectionDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LICInspe__3214EC07CF2D6736");

            entity.ToTable("LICInspectionDetails");

            entity.Property(e => e.CollegeName).HasMaxLength(200);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Faculty)
                .HasMaxLength(10)
                .HasColumnName("faculty");
            entity.Property(e => e.FacultyId).HasColumnName("facultyId");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.SelectedCollegeCode).HasMaxLength(50);
            entity.Property(e => e.TypeofMember).HasMaxLength(100);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<MedCaAccountAndFeeDetail>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.CollegeCode, e.FacultyCode, e.CourseLevel });

            entity.ToTable("Med_CA_AccountAndFeeDetails");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CollegeCode).HasMaxLength(25);
            entity.Property(e => e.FacultyCode).HasMaxLength(25);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.AccountBooksMaintained)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.AccountSummaryPdfName).HasMaxLength(200);
            entity.Property(e => e.AccountsAudited)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.AuditedStatementPdfName).HasMaxLength(200);
            entity.Property(e => e.AuthorityContact).HasMaxLength(20);
            entity.Property(e => e.Deposits).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DonationLevied)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DonationPdfName).HasMaxLength(255);
            entity.Property(e => e.GoverningCouncilPdfName).HasMaxLength(255);
            entity.Property(e => e.LibraryFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.NonRecurrentAnnual).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OtherFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RecurrentAnnual).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.SportsFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SubFacultyCode).HasMaxLength(20);
            entity.Property(e => e.TotalFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TuitionFee).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnionFee).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<MedCaMstStaffDesignation>(entity =>
        {
            entity.HasKey(e => e.SlNo).HasName("PK__Med_CA_M__BC789CF28BB59A34");

            entity.ToTable("Med_CA_MST_StaffDesignation");

            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.FacultyCode).HasMaxLength(20);
            entity.Property(e => e.SubFacultyCode).HasMaxLength(20);
        });

        modelBuilder.Entity<MedCaStaffParticular>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.CollegeCode, e.FacultyCode, e.DesignationSlNo, e.CourseLevel });

            entity.ToTable("Med_CA_StaffParticulars");

            entity.HasIndex(e => new { e.CollegeCode, e.FacultyCode, e.DesignationSlNo, e.CourseLevel }, "UQ_StaffParticulars_CollegeFacultyDesignationLevel").IsUnique();

            entity.HasIndex(e => new { e.CollegeCode, e.FacultyCode, e.DesignationSlNo, e.CourseLevel }, "UQ_StaffParticulars_CollegeFacultyDesignation_Level").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.FacultyCode).HasMaxLength(20);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.PayScale).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RegistrationNo).HasMaxLength(50);
            entity.Property(e => e.SubFacultyCode).HasMaxLength(20);
        });

        modelBuilder.Entity<MedicalAdministrativePhysicalFacility>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Medical___3214EC078089FFAF");

            entity.ToTable("Medical_AdministrativePhysicalFacilities");

            entity.Property(e => e.AnimalHouseAreaSqFt).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.AnimalTypes).HasMaxLength(300);
            entity.Property(e => e.AuditoriumAreaSqFt).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CommitteeRoomsAreaSqFt).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CourseLevel).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.LaboratoriesAreaSqFt).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.LectureHallsAreaSqFt).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.MuseumAreaSqFt).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.OfficeRoomAreaSqFt).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.PrincipalChamberAreaSqFt).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.SeminarHallAreaSqFt).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.StaffRoomsAreaSqFt).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.WorkshopEquipmentDetails).HasMaxLength(500);
            entity.Property(e => e.WorkshopScopeOfWork).HasMaxLength(500);
        });

        modelBuilder.Entity<MedicalCourseDetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__MedicalC__3214EC073646CF08")
                .HasFillFactor(80);

            entity.ToTable("MedicalCourseDetail");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.FreshOrIncrease).HasMaxLength(50);
            entity.Property(e => e.UgIntake).HasMaxLength(50);
        });

        modelBuilder.Entity<MedicalDepartmentOfficesMeu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Medical___3214EC070323BF7C");

            entity.ToTable("Medical_DepartmentOfficesMeu");

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MedicalEducationUnitAreaSqm).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MedicalEducationUnitHasAudioVisual).HasDefaultValue(false);
            entity.Property(e => e.MedicalEducationUnitHasInternet).HasDefaultValue(false);
            entity.Property(e => e.MeuCoordinatorDesignationDepartment).HasMaxLength(300);
            entity.Property(e => e.MeuCoordinatorEmail).HasMaxLength(150);
            entity.Property(e => e.MeuCoordinatorName).HasMaxLength(200);
            entity.Property(e => e.MeuCoordinatorPhone).HasMaxLength(50);
        });

        modelBuilder.Entity<MedicalInstituteDetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__MedicalI__3214EC07FD67D1BD")
                .HasFillFactor(80);

            entity.ToTable("MedicalInstituteDetail");

            entity.Property(e => e.Age).HasMaxLength(10);
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.Course).HasMaxLength(10);
            entity.Property(e => e.District).HasMaxLength(50);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.HodofInstitution).HasMaxLength(200);
            entity.Property(e => e.InstituteAddress).HasMaxLength(1000);
            entity.Property(e => e.InstituteName).HasMaxLength(200);
            entity.Property(e => e.InstitutionType).HasMaxLength(100);
            entity.Property(e => e.OtherDegree).HasMaxLength(150);
            entity.Property(e => e.PgDegree).HasMaxLength(100);
            entity.Property(e => e.SelectedSpecialities).HasMaxLength(200);
            entity.Property(e => e.Specialisation).HasMaxLength(200);
            entity.Property(e => e.Taluk).HasMaxLength(50);
            entity.Property(e => e.TeachingExperience).HasMaxLength(100);
            entity.Property(e => e.TrustSocietyName).HasMaxLength(200);
            entity.Property(e => e.YearOfEstablishmentOfCollege).HasMaxLength(10);
            entity.Property(e => e.YearOfEstablishmentOfTrust).HasMaxLength(10);
        });

        modelBuilder.Entity<MedicalMuseum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Medical___3214EC07B6122AA4");

            entity.ToTable("Medical_Museums");

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.MuseumsHaveAv).HasColumnName("MuseumsHaveAV");
            entity.Property(e => e.SeatingAreaAvailableSqm).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SeatingAreaDeficiencySqm).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SeatingAreaRequiredSqm).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<MedicalSkillsLaboratory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Medical___3214EC072BF21B5C");

            entity.ToTable("Medical_SkillsLaboratory");

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SkillsLabEnabledForElearning).HasColumnName("SkillsLabEnabledForELearning");
            entity.Property(e => e.TeachingAreasHaveAv).HasColumnName("TeachingAreasHaveAV");
            entity.Property(e => e.TotalAreaAvailableSqm).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAreaDeficiencySqm).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAreaRequiredSqm).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<MedicalStudentPracticalLab>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Medical___3214EC07AD980F64");

            entity.ToTable("Medical_StudentPracticalLabs");

            entity.Property(e => e.AllLabsHaveAv).HasColumnName("AllLabsHaveAV");
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel).HasMaxLength(20);
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MedicalUgbedDistribution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Medical___3214EC07F7CDEC4F");

            entity.ToTable("Medical_UGBedDistribution");

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Ent).HasColumnName("ENT");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Iccu).HasColumnName("ICCU");
            entity.Property(e => e.Icu).HasColumnName("ICU");
            entity.Property(e => e.MajorOt).HasColumnName("MajorOT");
            entity.Property(e => e.MinorOt).HasColumnName("MinorOT");
            entity.Property(e => e.ObstetricsAnc).HasColumnName("ObstetricsANC");
            entity.Property(e => e.PicuNicu).HasColumnName("PICU_NICU");
            entity.Property(e => e.Sicu).HasColumnName("SICU");
            entity.Property(e => e.SkinVd).HasColumnName("SkinVD");
            entity.Property(e => e.TotalIcubeds).HasColumnName("TotalICUBeds");
        });

        modelBuilder.Entity<MstAdministration>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.ToTable("MST_Administration");

            entity.Property(e => e.AdministrationType)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.MstAdministrations)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MST_Administration");
        });

        modelBuilder.Entity<MstAdministrativeFacility>(entity =>
        {
            entity.HasKey(e => new { e.FacilityId, e.FacultyId, e.CourseCode }).HasFillFactor(80);

            entity.ToTable("Mst_AdministrativeFacilities");

            entity.Property(e => e.CourseCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Facilities)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SizeofFacilities)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstAffiliatedMaterialDatum>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__MST_Affi__3214EC076046D914")
                .HasFillFactor(80);

            entity.ToTable("MST_AffiliatedMaterialData");

            entity.Property(e => e.ParametersName).HasMaxLength(200);
        });

        modelBuilder.Entity<MstBuildingDetailRequired>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Mst_Buil__3214EC0728CAA792")
                .HasFillFactor(80);

            entity.ToTable("Mst_BuildingDetailRequired");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<MstClassroomDetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Mst_Clas__3214EC2754F8C96F")
                .HasFillFactor(80);

            entity.ToTable("Mst_ClassroomDetail");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IntakeId).HasColumnName("IntakeID");
            entity.Property(e => e.SizeOfClassrooms)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstCourse>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.ToTable("Mst_Course");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.CourseLevel).HasMaxLength(100);
            entity.Property(e => e.CourseName).HasMaxLength(100);
            entity.Property(e => e.CoursePrefix).HasMaxLength(100);
            entity.Property(e => e.SubjectName).HasMaxLength(100);
        });

        modelBuilder.Entity<MstDesignation>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Mst_Designation");

            entity.Property(e => e.Constituency)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.DesignationId).HasColumnName("Designation_Id");
            entity.Property(e => e.DesignationName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Designation_Name");
            entity.Property(e => e.DesignationType)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("Designation_Type");
            entity.Property(e => e.FacultyId).HasColumnName("FacultyID");
        });

        modelBuilder.Entity<MstFeesType>(entity =>
        {
            entity.HasKey(e => new { e.FeesCode, e.FacultyId }).HasFillFactor(80);

            entity.ToTable("Mst_FeesType");

            entity.Property(e => e.FeesType)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstFieldTypeChp>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.ToTable("MST_FieldType_CHP");

            entity.Property(e => e.FieldType)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.MstFieldTypeChps)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MST_FieldType_CHP_FacultyCode");
        });

        modelBuilder.Entity<MstFpaAdopAffType>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK_MST_FPA_CommunityHealthPlanning_Type")
                .HasFillFactor(80);

            entity.ToTable("MST_FPA_AdopAff_Type");

            entity.Property(e => e.FpaType)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.MstFpaAdopAffTypes)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MST_FPA_CommunityHealthPlanning_Type_FacultyCode");
        });

        modelBuilder.Entity<MstGeoLocation>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Mst_GeoLocation");

            entity.Property(e => e.BuildingName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Type)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<MstHospitalDocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.ToTable("MST_Hospital_Documents");

            entity.Property(e => e.CertificateNo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.DocumentName)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.MstHospitalDocuments)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MST_Hospital_Documents_FacultyCode");
        });

        modelBuilder.Entity<MstHospitalLocation>(entity =>
        {
            entity.HasKey(e => new { e.FacultyId, e.LocationId }).HasFillFactor(80);

            entity.ToTable("Mst_HospitalLocation");

            entity.Property(e => e.LocationDescription)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstHospitalOwnedBy>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.ToTable("MST_HospitalOwnedBy");

            entity.Property(e => e.OwnedBy)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.MstHospitalOwnedBies)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HospitalOwnedBy_Faculty");
        });

        modelBuilder.Entity<MstHospitalType>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK_MST_Hospital_Type_Id")
                .HasFillFactor(80);

            entity.ToTable("MST_Hospital_Type");

            entity.Property(e => e.HospitalType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.MstHospitalTypes)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MST_Hospital_Type_FacultyCode");
        });

        modelBuilder.Entity<MstHostelFacility>(entity =>
        {
            entity.HasKey(e => new { e.HostelFacilityId, e.FacultyId }).HasFillFactor(80);

            entity.ToTable("Mst_HostelFacilities");

            entity.Property(e => e.HostelFacilityName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstHosteltype>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__mst_host__3213E83F6E786D7B")
                .HasFillFactor(80);

            entity.ToTable("mst_hosteltype");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HospitalType)
                .HasMaxLength(200)
                .HasColumnName("hospital_type");
        });

        modelBuilder.Entity<MstIndoorBedsDepartmentMaster>(entity =>
        {
            entity.HasKey(e => e.DeptId);

            entity.ToTable("MST_IndoorBedsDepartmentMaster");

            entity.Property(e => e.DepartmentName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.AffiliationType).WithMany(p => p.MstIndoorBedsDepartmentMasters)
                .HasForeignKey(d => d.AffiliationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MST_IndoorBedsDepartmentMaster_AffiliationTypeId");

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.MstIndoorBedsDepartmentMasters)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MST_IndoorBedsDepartmentMaster_FacultyCode");
        });

        modelBuilder.Entity<MstIndoorBedsOccupancyMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MST_Indo__3214EC07E72626F0");

            entity.ToTable("MST_IndoorBedsOccupancyMaster");

            entity.Property(e => e.SeatSlabId)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.HasOne(d => d.AffiliationType).WithMany(p => p.MstIndoorBedsOccupancyMasters)
                .HasForeignKey(d => d.AffiliationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndoorBeds_Affiliation");

            entity.HasOne(d => d.DepartmentCodeNavigation).WithMany(p => p.MstIndoorBedsOccupancyMasters)
                .HasForeignKey(d => d.DepartmentCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndoorBeds_Department");

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.MstIndoorBedsOccupancyMasters)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndoorBeds_Faculty");
        });

        modelBuilder.Entity<MstIndoorInfrastructureRequirementsMaster>(entity =>
        {
            entity.ToTable("MST_IndoorInfrastructureRequirementsMaster");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RequirementName)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.SectionCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SectionName)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.HasOne(d => d.AffiliationType).WithMany(p => p.MstIndoorInfrastructureRequirementsMasters)
                .HasForeignKey(d => d.AffiliationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndoorInfraReq_Affiliation");

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.MstIndoorInfrastructureRequirementsMasters)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndoorInfraReq_Faculty");
        });

        modelBuilder.Entity<MstInstitutionType>(entity =>
        {
            entity.HasKey(e => e.InstitutionTypeId)
                .HasName("PK_Mst_InstitutionType_1")
                .HasFillFactor(80);

            entity.ToTable("Mst_InstitutionType");

            entity.Property(e => e.InstitutionType)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.OrganizationCategory)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
        });

        modelBuilder.Entity<MstLaboratory>(entity =>
        {
            entity.HasKey(e => new { e.LabId, e.Facultyid }).HasFillFactor(80);

            entity.ToTable("Mst_Laboratories");

            entity.Property(e => e.LabId).HasColumnName("LabID");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Laboratories)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstLaboratoryEquipmentDetail>(entity =>
        {
            entity.HasKey(e => new { e.EquipmentId, e.FacultyId }).HasFillFactor(80);

            entity.ToTable("Mst_LaboratoryEquipmentDetails");

            entity.Property(e => e.EquipmentId).HasColumnName("EquipmentID");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.EquipmentName)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.SubId).HasColumnName("SubID");
            entity.Property(e => e.Subjects)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstLaboratoryEquipmentSubject>(entity =>
        {
            entity.HasKey(e => new { e.SubjectId, e.FacultyId }).HasFillFactor(80);

            entity.ToTable("Mst_LaboratoryEquipmentSubjects");

            entity.Property(e => e.SubjectId).HasColumnName("SubjectID");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SubjectName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstLibraryEquipmentMaster>(entity =>
        {
            entity.HasKey(e => new { e.EquipmentId, e.FacultyId }).HasFillFactor(80);

            entity.ToTable("Mst_LibraryEquipmentMaster");

            entity.Property(e => e.EquipmentName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstLibraryFinanceItem>(entity =>
        {
            entity.HasKey(e => new { e.LibFinItemId, e.FacultyId }).HasFillFactor(80);

            entity.ToTable("Mst_LibraryFinanceItems");

            entity.Property(e => e.ItemsName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstLibraryServicesMaster>(entity =>
        {
            entity.HasKey(e => new { e.LibraryServiceId, e.FacultyId }).HasFillFactor(80);

            entity.ToTable("Mst_LibraryServicesMaster");

            entity.Property(e => e.LibraryServiceName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstLicAcademicCouncilMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_AcMemberList");

            entity.ToTable("MST_Lic_AcademicCouncilMembers");

            entity.Property(e => e.AcCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.AcmemberName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ACMemberName");
            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CollegeName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.EmailId)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.MobileNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstLicInspectionAllotedMembersDetail>(entity =>
        {
            entity.HasKey(e => e.SlNo);

            entity.ToTable("MST_LIC_Inspection_AllotedMembersDetails");

            entity.Property(e => e.SlNo)
                .ValueGeneratedNever()
                .HasColumnName("SL_No");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.CollegeName)
                .HasMaxLength(200)
                .HasColumnName("College_Name");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.MemberName).HasMaxLength(150);
            entity.Property(e => e.PhoneNo).HasColumnName("Phone_No");
            entity.Property(e => e.TypeofMemebers).HasMaxLength(150);
        });

        modelBuilder.Entity<MstLicInspectionMember>(entity =>
        {
            entity.ToTable("MST_LIC_Inspection_Members");

            entity.Property(e => e.Licid)
                .HasMaxLength(50)
                .HasColumnName("LICId");
            entity.Property(e => e.TypeofMemebers).HasMaxLength(150);
        });

        modelBuilder.Entity<MstLicSenateMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__MST_Lic___3214EC07F8095D39");

            entity.ToTable("MST_Lic_SenateMembers");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CollegeName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.EmailId).HasMaxLength(150);
            entity.Property(e => e.MobileNumber).HasMaxLength(15);
            entity.Property(e => e.SeCode).HasMaxLength(10);
            entity.Property(e => e.SenateMemberName).HasMaxLength(150);
        });

        modelBuilder.Entity<MstLicSubjectExpertiseMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ExpMemberList");

            entity.ToTable("MST_Lic_SubjectExpertiseMembers");

            entity.Property(e => e.Address)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CollegeName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.EmailId)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ExpCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.ExpMemberName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MobileNumber)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<MstMedicalCourseType>(entity =>
        {
            entity.HasKey(e => e.CourseTypeId)
                .HasName("PK__MST_Medi__81736972198B375B")
                .HasFillFactor(80);

            entity.ToTable("MST_MedicalCourseType");

            entity.HasIndex(e => e.CourseTypeName, "UQ__MST_Medi__3CFBF7720ABDF5B5")
                .IsUnique()
                .HasFillFactor(80);

            entity.HasIndex(e => e.CourseTypeName, "UQ__MST_Medi__3CFBF77241DB9EE7")
                .IsUnique()
                .HasFillFactor(80);

            entity.HasIndex(e => e.CourseTypeName, "UQ__MST_Medi__3CFBF7726D5766A0")
                .IsUnique()
                .HasFillFactor(80);

            entity.HasIndex(e => e.CourseTypeName, "UQ__MST_Medi__3CFBF772D6C791D9")
                .IsUnique()
                .HasFillFactor(80);

            entity.Property(e => e.CourseTypeName).HasMaxLength(50);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsPg).HasColumnName("IsPG");
            entity.Property(e => e.IsSs).HasColumnName("IsSS");
            entity.Property(e => e.IsUg).HasColumnName("IsUG");
            entity.Property(e => e.ModifiedDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<MstNursingAffiliatedMaterialDatum>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__MST_Nurs__3214EC079292C0BD")
                .HasFillFactor(80);

            entity.ToTable("MST_NursingAffiliatedMaterialData");

            entity.Property(e => e.FacultyCode).HasMaxLength(25);
            entity.Property(e => e.ParametersName).HasMaxLength(200);
        });

        modelBuilder.Entity<NodalOfficerDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CollegeName).HasMaxLength(200);
            entity.Property(e => e.EmailId).HasMaxLength(50);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.FacultyName).HasMaxLength(50);
            entity.Property(e => e.NodalOfficerName).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
        });

        modelBuilder.Entity<NodalOfficerInitiative>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__NodalOff__3214EC0722655C76")
                .HasFillFactor(80);

            entity.Property(e => e.InitiativeId)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NodalOfficerName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<NonTeachingStaffDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__NonTeach__3214EC0796E2CA6C");

            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.FacultyCode).HasMaxLength(20);
            entity.Property(e => e.MobileNumber).HasMaxLength(20);
            entity.Property(e => e.SalaryPaid).HasMaxLength(100);
            entity.Property(e => e.StaffName).HasMaxLength(150);
        });

        modelBuilder.Entity<NursingAffiliatedYearwiseMaterialsDatum>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Nursing___3214EC0755F69B78")
                .HasFillFactor(80);

            entity.ToTable("Nursing_Affiliated_Yearwise_MaterialsData");

            entity.Property(e => e.BiomedicalWasteManagemenDoc).HasColumnName("Biomedical_Waste_Managemen_Doc");
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.FireSafetyDoc).HasColumnName("Fire_Safety_Doc");
            entity.Property(e => e.HospitalOwnerName).HasMaxLength(150);
            entity.Property(e => e.HospitalType).HasMaxLength(150);
            entity.Property(e => e.IpdDocument).HasColumnName("IPD_Document");
            entity.Property(e => e.Kpmebeds)
                .HasMaxLength(10)
                .HasColumnName("KPMEBeds");
            entity.Property(e => e.MajorOperationsSurgeries).HasColumnName("Major_Operations_Surgeries");
            entity.Property(e => e.MinorOperationsSurgeries).HasColumnName("Minor_Operations_Surgeries");
            entity.Property(e => e.OpdDocument).HasColumnName("OPD_Document");
            entity.Property(e => e.ParametersName).HasMaxLength(200);
            entity.Property(e => e.ParentHospitalAddress)
                .HasMaxLength(250)
                .HasColumnName("parentHospitalAddress");
            entity.Property(e => e.ParentHospitalKpmebedsDoc).HasColumnName("parentHospitalKPMEbedsDoc");
            entity.Property(e => e.ParentHospitalMoudoc).HasColumnName("parentHospitalMOUdoc");
            entity.Property(e => e.ParentHospitalName)
                .HasMaxLength(150)
                .HasColumnName("parentHospitalName");
            entity.Property(e => e.ParentHospitalOwnerNameDoc).HasColumnName("parentHospitalOwnerNameDoc");
            entity.Property(e => e.ParentHospitalPostBasicDoc).HasColumnName("parentHospitalPostBasicDoc");
            entity.Property(e => e.PolutionControlDoc).HasColumnName("Polution_Control_Doc");
            entity.Property(e => e.PostBasicBeds).HasMaxLength(10);
            entity.Property(e => e.TotalBeds).HasMaxLength(10);
            entity.Property(e => e.Year1).HasMaxLength(50);
            entity.Property(e => e.Year2).HasMaxLength(50);
            entity.Property(e => e.Year3).HasMaxLength(50);
        });

        modelBuilder.Entity<NursingCollegeRegistration>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Nursing___3214EC07266CF326")
                .HasFillFactor(80);

            entity.ToTable("Nursing_CollegeRegistration");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.CourseName).HasMaxLength(200);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.HodOfInstitution).HasMaxLength(150);
            entity.Property(e => e.InstituteAddress).HasMaxLength(500);
            entity.Property(e => e.InstituteName).HasMaxLength(200);
            entity.Property(e => e.InstitutionType).HasMaxLength(100);
            entity.Property(e => e.RegistrationNumber).HasMaxLength(50);
            entity.Property(e => e.TrustSocietyName).HasMaxLength(200);
        });

        modelBuilder.Entity<NursingCourse>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("nursing_Courses");

            entity.Property(e => e.CourseLevel).HasMaxLength(100);
            entity.Property(e => e.CourseName).HasMaxLength(100);
            entity.Property(e => e.CoursePrefix).HasMaxLength(100);
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.SubjectName).HasMaxLength(100);
        });

        modelBuilder.Entity<NursingFacultyDetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Nursing___3214EC0749312225")
                .HasFillFactor(80);

            entity.ToTable("Nursing_FacultyDetails");

            entity.Property(e => e.Aadhaar).HasMaxLength(20);
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.DepartmentDetails).HasMaxLength(100);
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.Mobile).HasMaxLength(15);
            entity.Property(e => e.NameOfFaculty).HasMaxLength(200);
            entity.Property(e => e.Pan).HasMaxLength(20);
            entity.Property(e => e.RecognizedPgTeacher).HasMaxLength(50);
            entity.Property(e => e.Subject).HasMaxLength(100);
        });

        modelBuilder.Entity<NursingFacultyDetail1>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__NursingF__3214EC077FED1CFC")
                .HasFillFactor(80);

            entity.ToTable("NursingFacultyDetails");

            entity.Property(e => e.AadhaarNumber).HasMaxLength(50);
            entity.Property(e => e.CollegeName).HasMaxLength(200);
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.EmscollegeCode)
                .HasMaxLength(50)
                .HasColumnName("EMSCollegeCode");
            entity.Property(e => e.Pannumber)
                .HasMaxLength(50)
                .HasColumnName("PANNumber");
            entity.Property(e => e.RegistrationNumber).HasMaxLength(100);
            entity.Property(e => e.TeachingFacultyName).HasMaxLength(200);
        });

        modelBuilder.Entity<NursingFacultyWithCollege>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("NursingFacultyWithCollege");

            entity.Property(e => e.AadhaarNumber).HasMaxLength(50);
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CollegeName).HasMaxLength(200);
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.Pannumber)
                .HasMaxLength(50)
                .HasColumnName("PANNumber");
            entity.Property(e => e.TeachingFacultyName).HasMaxLength(200);
        });

        modelBuilder.Entity<NursingInstituteDetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__NursingI__3214EC07E4A961C0")
                .HasFillFactor(80);

            entity.ToTable("NursingInstituteDetail");

            entity.Property(e => e.Age).HasMaxLength(10);
            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseSelectedSpecialities).HasMaxLength(200);
            entity.Property(e => e.Degree).HasMaxLength(100);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.HighestQualification).HasMaxLength(200);
            entity.Property(e => e.HodofInstitution).HasMaxLength(200);
            entity.Property(e => e.InstituteAddress).HasMaxLength(200);
            entity.Property(e => e.InstituteName).HasMaxLength(200);
            entity.Property(e => e.InstitutionType).HasMaxLength(100);
            entity.Property(e => e.Qualifications).HasMaxLength(200);
            entity.Property(e => e.TeachingExperience).HasMaxLength(100);
            entity.Property(e => e.TrustSocietyName).HasMaxLength(200);
            entity.Property(e => e.YearOfEstablishmentOfCollege).HasMaxLength(10);
            entity.Property(e => e.YearOfEstablishmentOfTrust).HasMaxLength(10);
        });

        modelBuilder.Entity<NursingUgpgdetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__NursingU__3214EC0744384C85")
                .HasFillFactor(80);

            entity.ToTable("NursingUGPGDetails");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.Course).HasMaxLength(100);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.FreshOrIncrease).HasMaxLength(50);
            entity.Property(e => e.Gok).HasColumnName("GOK");
            entity.Property(e => e.Inc).HasColumnName("INC");
            entity.Property(e => e.IntakeDetails).HasMaxLength(50);
            entity.Property(e => e.Knmc).HasColumnName("KNMC");
            entity.Property(e => e.NumberOfSeats).HasMaxLength(20);
            entity.Property(e => e.PermittedYear).HasMaxLength(10);
            entity.Property(e => e.RecognizedYear).HasMaxLength(10);
        });

        modelBuilder.Entity<RguhsIntakeChangeAndApproval>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.ToTable("RguhsIntakeChangeAndApproval");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.RemarksForIntakeChange).IsUnicode(false);
        });

        modelBuilder.Entity<SeatSlabMaster>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__SeatSlab__3214EC0713AF5B58")
                .HasFillFactor(80);

            entity.ToTable("SeatSlabMaster");

            entity.Property(e => e.SeatSlabId)
                .HasMaxLength(10)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SmallGroupTeaching>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SmallGro__3214EC07193B4222");

            entity.Property(e => e.ApprovedBuildingPlanPath).HasMaxLength(500);
            entity.Property(e => e.AreaDeficiencySqm).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.AvailableAreaSqm).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.BuildingOwnershipType).HasMaxLength(50);
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.FloorAreaSqFt).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.LandDetailsIfYes).HasMaxLength(1000);
            entity.Property(e => e.LandOwnershipType).HasMaxLength(50);
            entity.Property(e => e.LandRecordsPath).HasMaxLength(500);
            entity.Property(e => e.RequiredAreaSqm).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<StateMaster>(entity =>
        {
            entity.HasKey(e => e.StateId)
                .HasName("PK__StateMas__C3BA3B3A69E124E1")
                .HasFillFactor(80);

            entity.ToTable("StateMaster");

            entity.HasIndex(e => e.StateName, "UQ__StateMas__554763154DC5877D")
                .IsUnique()
                .HasFillFactor(80);

            entity.Property(e => e.StateId)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.StateName).HasMaxLength(100);
        });

        modelBuilder.Entity<SuperVisionInFieldPracticeArea>(entity =>
        {
            entity.ToTable("SuperVisionInFieldPracticeArea");

            entity.Property(e => e.CollegeCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CourseLevel)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Post)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Qualification)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.Responsibilities).IsUnicode(false);
            entity.Property(e => e.University)
                .HasMaxLength(300)
                .IsUnicode(false);

            entity.HasOne(d => d.AffiliationType).WithMany(p => p.SuperVisionInFieldPracticeAreas)
                .HasForeignKey(d => d.AffiliationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SuperVisionInFieldPracticeArea_Affiliation");

            entity.HasOne(d => d.FacultyCodeNavigation).WithMany(p => p.SuperVisionInFieldPracticeAreas)
                .HasForeignKey(d => d.FacultyCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SuperVisionInFieldPracticeArea_Faculty");

            entity.HasOne(d => d.HospitalDetails).WithMany(p => p.SuperVisionInFieldPracticeAreas)
                .HasForeignKey(d => d.HospitalDetailsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SuperVisionInFieldPracticeArea_HospitalDetails");
        });

        modelBuilder.Entity<TalukMaster>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__TalukMas__54E8482A915BA554")
                .HasFillFactor(80);

            entity.ToTable("TalukMaster");

            entity.Property(e => e.DistrictId)
                .HasMaxLength(6)
                .IsUnicode(false)
                .HasColumnName("DistrictID");
            entity.Property(e => e.TalukId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("TalukID");
            entity.Property(e => e.TalukName).HasMaxLength(100);
        });

        modelBuilder.Entity<TblClassroomAvailability>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Tbl_Clas__3214EC275EED1A15")
                .HasFillFactor(80);

            entity.ToTable("Tbl_ClassroomAvailability");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CourseCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblEquipmentDetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Tbl_Equi__3214EC279D4D2433")
                .HasFillFactor(80);

            entity.ToTable("Tbl_EquipmentDetail");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EquipmentName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Make)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Model)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblLaboratoryAvailability>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Tbl_Labo__3214EC2724A8F7C9")
                .HasFillFactor(80);

            entity.ToTable("Tbl_LaboratoryAvailability");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LabId).HasColumnName("LabID");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblLaboratoryAvailability1>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__TblLabor__3214EC276C70908D")
                .HasFillFactor(80);

            entity.ToTable("TblLaboratoryAvailability");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FacultyCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LabId).HasColumnName("LabID");
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblMedicalEquipmentAvailability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Medi__3214EC07D2B24EBD");

            entity.ToTable("Tbl_MedicalEquipmentAvailability");

            entity.Property(e => e.AcademicYear)
                .HasMaxLength(9)
                .IsUnicode(false);
            entity.Property(e => e.CollegeCode)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.CourseCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<TblMedicalSkillsLabEquipment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tbl_Medi__3214EC077027A9CB");

            entity.ToTable("Tbl_MedicalSkillsLabEquipments");

            entity.Property(e => e.IsRequired).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(300);
        });

        modelBuilder.Entity<TblRguhsFacultyUser>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("TblRguhsFacultyUser");

            entity.Property(e => e.DesignationDescription)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FinanceDesignation)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Password).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        modelBuilder.Entity<TeachingStaffDepartmentWiseDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Teaching__3214EC0724B5F7D3");

            entity.Property(e => e.CollegeCode).HasMaxLength(20);
            entity.Property(e => e.CourseLevel).HasMaxLength(10);
            entity.Property(e => e.DepartmentCode).HasMaxLength(20);
            entity.Property(e => e.DesignationCode).HasMaxLength(20);
            entity.Property(e => e.DesignationName).HasMaxLength(100);
            entity.Property(e => e.FacultyCode).HasMaxLength(20);
            entity.Property(e => e.Pgfrom).HasColumnName("PGFrom");
            entity.Property(e => e.Pgto).HasColumnName("PGTo");
            entity.Property(e => e.TotalExperience).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Ugfrom).HasColumnName("UGFrom");
            entity.Property(e => e.Ugto).HasColumnName("UGTo");
        });

        modelBuilder.Entity<TrustDocumentDetail>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasFillFactor(80);

            entity.Property(e => e.DocumentId).ValueGeneratedNever();
            entity.Property(e => e.DocumentName).HasMaxLength(50);
            entity.Property(e => e.FacultyCode).HasColumnName("facultyCode");
            entity.Property(e => e.FileName).HasMaxLength(50);
            entity.Property(e => e.UploadedOn).HasColumnType("datetime");
        });

        modelBuilder.Entity<TrustDocumentMaster>(entity =>
        {
            entity.HasKey(e => e.DocumentId)
                .HasName("PK__TrustDoc__1ABEEF0F1BB8FA8F")
                .HasFillFactor(80);

            entity.ToTable("TrustDocumentMaster");

            entity.HasIndex(e => e.DocumentName, "UQ__TrustDoc__7DEDE07E4DD3E0DC")
                .IsUnique()
                .HasFillFactor(80);

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DocumentName).HasMaxLength(200);
        });

        modelBuilder.Entity<TrustMemberDetail>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__TrustMem__3214EC279255BC10")
                .HasFillFactor(80);

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.ExistingMember).HasDefaultValue(false);
            entity.Property(e => e.NewMember).HasDefaultValue(false);
            entity.Property(e => e.Qualification).HasMaxLength(100);
            entity.Property(e => e.TrustMemberName).HasMaxLength(100);
        });

        modelBuilder.Entity<TypeOfAffiliation>(entity =>
        {
            entity.HasKey(e => e.TypeId)
                .HasName("PK__TypeOfAf__516F03B5CEED9DAB")
                .HasFillFactor(80);

            entity.ToTable("TypeOfAffiliation");

            entity.HasIndex(e => e.TypeDescription, "UQ__TypeOfAf__B0BEAA3891706C09")
                .IsUnique()
                .HasFillFactor(80);

            entity.Property(e => e.TypeDescription)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TypeOfMinorityMaster>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.ToTable("TypeOfMinorityMaster");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Minority)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TypeOfOrganizationMaster>(entity =>
        {
            entity.HasKey(e => e.TypeId)
                .HasName("PK__TypeOfOr__516F03B5FBFF1B7E")
                .HasFillFactor(80);

            entity.ToTable("TypeOfOrganizationMaster");

            entity.HasIndex(e => e.TypeName, "UQ__TypeOfOr__D4E7DFA8EDD9A031")
                .IsUnique()
                .HasFillFactor(80);

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<UgandPgrepository>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__UGAndPGR__3214EC0747E94C28")
                .HasFillFactor(80);

            entity.ToTable("UGAndPGRepository");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.Course).HasMaxLength(100);
            entity.Property(e => e.CourseName).HasMaxLength(100);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.FreshOrIncrease).HasMaxLength(50);
            entity.Property(e => e.Gok).HasColumnName("GOK");
            entity.Property(e => e.Inc).HasColumnName("INC");
            entity.Property(e => e.IntakeDetails).HasMaxLength(50);
            entity.Property(e => e.Knmc).HasColumnName("KNMC");
            entity.Property(e => e.Ncisc).HasColumnName("NCISC");
            entity.Property(e => e.NumberOfSeats).HasMaxLength(50);
            entity.Property(e => e.PermittedYear).HasMaxLength(50);
            entity.Property(e => e.RecognizedYear).HasMaxLength(50);
        });

        modelBuilder.Entity<Ugdetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.ToTable("Ugdetail");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.Course).HasMaxLength(100);
            entity.Property(e => e.CourseCode).HasMaxLength(150);
            entity.Property(e => e.CourseLevel).HasMaxLength(500);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.FreshOrIncrease).HasMaxLength(50);
            entity.Property(e => e.Ksnc).HasColumnName("KSNC");
            entity.Property(e => e.NumberOfSeats).HasMaxLength(20);
            entity.Property(e => e.PermittedYear).HasMaxLength(10);
            entity.Property(e => e.RecognizedYear).HasMaxLength(10);
            entity.Property(e => e.SeatSlab).HasMaxLength(100);
            entity.Property(e => e.Ugintake).HasMaxLength(50);
        });

        modelBuilder.Entity<UniversityImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasFillFactor(80);

            entity.Property(e => e.FileName)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        modelBuilder.Entity<YearwiseMaterialsDatum>(entity =>
        {
            entity.HasKey(e => e.Id)
                .HasName("PK__Yearwise__3214EC07702AC217")
                .HasFillFactor(80);

            entity.ToTable("Yearwise_MaterialsData");

            entity.Property(e => e.CollegeCode).HasMaxLength(50);
            entity.Property(e => e.FacultyCode).HasMaxLength(50);
            entity.Property(e => e.HospitalOwnerName).HasMaxLength(150);
            entity.Property(e => e.Kpmebeds)
                .HasMaxLength(10)
                .HasColumnName("KPMEBeds");
            entity.Property(e => e.ParametersName).HasMaxLength(200);
            entity.Property(e => e.ParentHospitalAddress)
                .HasMaxLength(250)
                .HasColumnName("parentHospitalAddress");
            entity.Property(e => e.ParentHospitalKpmebedsDoc).HasColumnName("parentHospitalKPMEbedsDoc");
            entity.Property(e => e.ParentHospitalMoudoc).HasColumnName("parentHospitalMOUdoc");
            entity.Property(e => e.ParentHospitalName)
                .HasMaxLength(150)
                .HasColumnName("parentHospitalName");
            entity.Property(e => e.ParentHospitalOwnerNameDoc).HasColumnName("parentHospitalOwnerNameDoc");
            entity.Property(e => e.ParentHospitalPostBasicDoc).HasColumnName("parentHospitalPostBasicDoc");
            entity.Property(e => e.PostBasicBeds).HasMaxLength(10);
            entity.Property(e => e.TotalBeds).HasMaxLength(10);
            entity.Property(e => e.Year1).HasMaxLength(50);
            entity.Property(e => e.Year2).HasMaxLength(50);
            entity.Property(e => e.Year3).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
