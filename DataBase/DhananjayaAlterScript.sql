-- Add new columns
ALTER TABLE [dbo].[AFF_InstitutionsDetails]
ADD DocumentDataPath NVARCHAR(500);

ALTER TABLE [dbo].[CA_Med_StaffParticularsOther]
DROP COLUMN [TeachersUpdatedPdf];

ALTER TABLE [dbo].[CA_Med_StaffParticularsOther]
ADD TeachersUpdatedPdfPath NVARCHAR(500);

ALTER TABLE [dbo].[CA_Med_StaffPArticularsOther_Temp]
DROP COLUMN [TeachersUpdatedPdf];

ALTER TABLE [dbo].[CA_Med_StaffParticularsOther_Temp]
ADD TeachersUpdatedPdfPath NVARCHAR(500);

ALTER TABLE [dbo].[AFF_InstitutionsDetails]
DROP COLUMN DocumentData;

-- Add new path columns
ALTER TABLE [dbo].[FacultyDetails]
ADD GuideRecognitionDocPath NVARCHAR(500),
    PhDrecognitionDocPath NVARCHAR(500),
    LitigationDocPath NVARCHAR(500);

-- (Optional) Drop old columns
ALTER TABLE [dbo].[FacultyDetails]
DROP COLUMN GuideRecognitionDoc,
             PhDrecognitionDoc,
             LitigationDoc;

ALTER TABLE [dbo].[InstitutionBasicDetails]
ADD GovAutonomousCertFilePath NVARCHAR(500),
    GovCouncilMembershipFilePath NVARCHAR(500),
    GokOrderExistingCoursesFilePath NVARCHAR(500),
    FirstAffiliationNotifFilePath NVARCHAR(500),
    ContinuationAffiliationFilePath NVARCHAR(500),
    KncCertificateFilePath NVARCHAR(500),
    AmendedDocPath NVARCHAR(500),
    AadhaarFilePath NVARCHAR(500),
    PanfilePath NVARCHAR(500),
    BankStatementFilePath NVARCHAR(500),
    RegistrationCertificateFilePath NVARCHAR(500),
    RegisteredTrustMemberDetailsPath NVARCHAR(500),
    AuditStatementFilePath NVARCHAR(500);

-- Optional drop old
ALTER TABLE [dbo].[InstitutionBasicDetails]
DROP COLUMN GovAutonomousCertFile,
             GovCouncilMembershipFile,
             GokOrderExistingCoursesFile,
             FirstAffiliationNotifFile,
             ContinuationAffiliationFile,
             KncCertificateFile,
             AmendedDoc,
             AadhaarFile,
             Panfile,
             BankStatementFile,
             RegistrationCertificateFile,
             RegisteredTrustMemberDetails,
             AuditStatementFile;

ALTER TABLE [dbo].[SmallGroupTeachings]
ADD LandRecordsFilePath NVARCHAR(500),
    ApprovedBuildingPlanFilePath NVARCHAR(500);

-- Optional drop old
ALTER TABLE [dbo].[SmallGroupTeachings]
DROP COLUMN LandRecordsFile,
             ApprovedBuildingPlanFile;

ALTER TABLE [dbo].Affiliation_CourseDetails
ADD GokorderPath NVARCHAR(500),
 PreviousNotificationFilesPath NVARCHAR(500),
    LastAffiliationRguhsfilePath NVARCHAR(500);

ALTER TABLE [dbo].Affiliation_CourseDetails
DROP COLUMN Gokorder,
             LastAffiliationRguhsfile;

             ALTER TABLE AffiliatedHospitalDocuments
ADD DocumentFilePth NVARCHAR(500);

ALTER TABLE AffiliatedHospitalDocuments
DROP COLUMN DocumentFile;

ALTER TABLE [dbo].[CA_CourseCurriculum]
Drop Column CurriculumPdf;


ALTER TABLE [dbo].[Med_CA_AccountAndFeeDetails]
ADD GoverningCouncilPdfPath NVARCHAR(500),
    AccountSummaryPdfPath NVARCHAR(500),
    AuditedStatementPdfPath NVARCHAR(500),
    DonationPdfPath NVARCHAR(500);

    ALTER TABLE [dbo].[Med_CA_AccountAndFeeDetails]
DROP COLUMN GoverningCouncilPdf,
             AccountSummaryPdf,
             AuditedStatementPdf,
             DonationPdf;

ALTER TABLE [dbo].[CA_Med_StaffParticularsOther]
ADD 
    ExaminerDetailsPdfPath NVARCHAR(500),
    AebaslastThreeMonthsPdfPath NVARCHAR(500),
    AebasinspectionDayPdfPath NVARCHAR(500),
    ProvidentFundPdfPath NVARCHAR(500),
    EsipdfPath NVARCHAR(500);

ALTER TABLE [dbo].[CA_Med_StaffParticularsOther]
DROP COLUMN 
    ExaminerDetailsPdf,
    AebaslastThreeMonthsPdf,
    AebasinspectionDayPdf,
    ProvidentFundPdf,
    Esipdf;

ALTER TABLE [dbo].[CA_Med_StaffPArticularsOther_Temp]
ADD 
    ExaminerDetailsPdfPath NVARCHAR(500),
    AebaslastThreeMonthsPdfPath NVARCHAR(500),
    AebasinspectionDayPdfPath NVARCHAR(500),
    ProvidentFundPdfPath NVARCHAR(500),
    EsipdfPath NVARCHAR(500);

ALTER TABLE [dbo].[CA_Med_StaffPArticularsOther_Temp]
DROP COLUMN 
    ExaminerDetailsPdf,
    AebaslastThreeMonthsPdf,
    AebasinspectionDayPdf,
    ProvidentFundPdf,
    Esipdf;

    ALTER TABLE CA_MedicalLibraryServices
DROP COLUMN UploadedPdf;
ALTER TABLE CA_MedicalLibraryServices
ADD UploadedPdfPath NVARCHAR(500);

ALTER TABLE CA_MedicalLibraryUsageReport
DROP COLUMN UploadedFileData;
ALTER TABLE CA_MedicalLibraryServices
ADD UploadedFileDataPath NVARCHAR(500);

ALTER TABLE CA_MedicalLibraryOtherDetails
DROP COLUMN SpecialFeaturesAchievementspdf;
ALTER TABLE CA_MedicalLibraryOtherDetails
ADD SpecialFeaturesAchievementsPdfPath NVARCHAR(500);

ALTER TABLE CA_Med_ResearchPublicationsDetails
DROP COLUMN 
    PublicationsPdf,
    ProjectsPdf,
    ClinicalTrialsPdf,
    StudentsProjectsPdf,
    FacultyProjectsPdf;
ALTER TABLE CA_Med_ResearchPublicationsDetails
ADD 
    PublicationsPdfPath NVARCHAR(500),
    ProjectsPdfPath NVARCHAR(500),
    ClinicalTrialsPdfPath NVARCHAR(500),
    StudentsProjectsPdfPath NVARCHAR(500),
    FacultyProjectsPdfPath NVARCHAR(500);

ALTER TABLE [dbo].[CA_Med_Lib_Committee]
DROP COLUMN [CommitteePdf];

ALTER TABLE [dbo].[CA_Med_Lib_Committee]
ADD CommitteePdfPath NVARCHAR(500);

ALTER TABLE [dbo].[CA_Med_Lib_OtherAcademicActivities]
DROP COLUMN ActivityPdf;

ALTER TABLE [dbo].[CA_Med_Lib_OtherAcademicActivities]
ADD ActivityPdfPath NVARCHAR(500);

ALTER TABLE [dbo].[CA_Med_Lib_OtherAcademicActivities]
ADD ActivityPdfPath NVARCHAR(500);


--ALTER TABLE [dbo].[CA_MedicalLibraryUsageReport]
--DROP COLUMN UploadedFileData;

ALTER TABLE [dbo].[CA_MedicalLibraryUsageReport]
ADD UploadedFileDataPath NVARCHAR(500);



