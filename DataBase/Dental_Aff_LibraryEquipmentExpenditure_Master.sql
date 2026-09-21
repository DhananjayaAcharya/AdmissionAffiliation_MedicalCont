Update CA_MST_Med_CommitteeNames
SET CommitteeName = 'Internal Committee(POSH)'
where CommitteeName like '%POSH%' and FacultyCode = 2

-----------------------------------------------------

INSERT INTO CA_MST_CourseCurriculum
  (CurriculumName, IsActive)
  VALUES
  ('Vacation Period', 1),
  ('University Examination', 1);


-------------------------------------------------

ALTER TABLE [dbo].[Medical_DepartmentOfficesMeu]
ADD
    DEUYearOfStarting NVARCHAR(50) NULL,
    NatureOfActivities NVARCHAR(2000) NULL;

-----------------------------------------------

ALTER TABLE [dbo].[AFF_InstitutionsDetails]
ADD NameOfAdministrativeAuthority VARCHAR(250) NULL,
    AddressOfAdministrativeAuthority VARCHAR (MAX) NULL,
    MembersOfGoverningBodyOrCouncilFilePath VARCHAR (500) NULL;


----------------------------------------------------

    ALTER TABLE [dbo].[AFF_HostelDetails]
ADD
    CommonRoomForMenArea DECIMAL(10,2) NULL,
    CommonRoomForWomenArea DECIMAL(10,2) NULL,
    GamesRecreationFacilities NVARCHAR(MAX) NULL,
    MedicalExaminationHealthServices NVARCHAR(MAX) NULL;
-------------------------------------------------

ALTER TABLE DentalCollegeLandBuildingDetail
ADD 
    PrincipalStaffResidentialQuarter BIT NULL,
    PrincipalStaffResidentialQuarterAreaSqFt DECIMAL(10,2) NULL,

    OtherStaffResidentialQuarter BIT NULL,
    OtherStaffResidentialQuarterAreaSqFt DECIMAL(10,2) NULL,

    TeachingAncillaryStaffResidentialQuarter BIT NULL,
    TeachingAncillaryStaffResidentialQuarterAreaSqFt DECIMAL(10,2) NULL;

------------------------------------------------------------------


ALTER TABLE [dbo].[HospitalDetailsForAffiliation]
ADD
    KPMECertificatePdfPath NVARCHAR(500) NULL,
    PollutionControlBoardCertificatePdfPath NVARCHAR(500) NULL,
    BioMedicalCertificatePdfPath NVARCHAR(500) NULL,
    DrugFreeCampusCertificationPdfPath NVARCHAR(500) NULL,
    ProposedPlansForFutureDevelopmentsPdfPath NVARCHAR(500) NULL;

------------------------------------------------------------------
SELECT * FROM [HospitalDetailsForAffiliation]
WHERE CollegeCode = 'd038'

UPDATE HospitalDetailsForAffiliation
SET CourseLevel = 'UG'
WHERE FacultyCode = 2 
------------------------------------------------------------------
ALTER TABLE [dbo].[HospitalDetailsForAffiliation]
ADD
    HasAnatomyActRegistration BIT NULL,
    AnatomyActRegistrationDetails NVARCHAR(MAX) NULL,
    AnatomyActRegistrationPdfPath NVARCHAR(500) NULL,
    HasHospitalTieUp BIT NULL;

----------------------------------------------------------------

select *  FROM [HospitalTieUpDetails]
WHERE CollegeCode = 'd038'


CREATE TABLE [dbo].[HospitalTieUpDetails]
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_HospitalTieUpDetails PRIMARY KEY,

    HospitalDetailsId INT NOT NULL,

    CollegeCode NVARCHAR(100) NULL,
    CourseLevel VARCHAR(10) NULL,

    FacultyCode INT NOT NULL,

    TieUpType NVARCHAR(200) NOT NULL,

    HospitalName NVARCHAR(300) NULL,

    HospitalAddress NVARCHAR(500) NULL,

    TieUpDetails NVARCHAR(MAX) NULL,

    SupportingDocumentPath NVARCHAR(500) NULL,

    SupportingDocumentName NVARCHAR(255) NULL,

    SupportingDocumentContentType NVARCHAR(100) NULL,

    IsDeleted BIT NOT NULL
        CONSTRAINT DF_HospitalTieUpDetails_IsDeleted DEFAULT (0),

    CreatedOn DATETIME NOT NULL
        CONSTRAINT DF_HospitalTieUpDetails_CreatedOn DEFAULT (GETDATE()),

    ModifiedOn DATETIME NULL,

    CONSTRAINT FK_HospitalTieUpDetails_HospitalDetails
        FOREIGN KEY (HospitalDetailsId)
        REFERENCES [dbo].[HospitalDetailsForAffiliation](HospitalDetailsId),

    CONSTRAINT FK_HospitalTieUpDetails_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES [dbo].[Faculty](FacultyId)
);
-----------------------------------------------------------
--select * from [AFF_HostelDetails]
--where collegecode = 'd038'
-- ============================================================
-- Table Name : Mst_LibraryExpenditure
-- Purpose    : Stores master data for library expenditure items
--              applicable to different faculties and
--              affiliation types.
-- ============================================================

CREATE TABLE Mst_LibraryExpenditure
(
    LibraryExpenditureId INT IDENTITY(1,1) NOT NULL,

    FacultyId INT NOT NULL,
    TypeId INT NOT NULL,

    ItemName NVARCHAR(250) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy NVARCHAR(100) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2 NULL,

    CONSTRAINT PK_Mst_LibraryExpenditure
        PRIMARY KEY (LibraryExpenditureId),

    CONSTRAINT FK_Mst_LibraryExpenditure_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_Mst_LibraryExpenditure_AffiliationType
        FOREIGN KEY (TypeId)
        REFERENCES TypeOfAffiliation(TypeId)
);

-- ============================================================
-- Insert Library Expenditure Master Items
-- FacultyId : 2
-- TypeId    : 2
-- ============================================================

INSERT INTO Mst_LibraryExpenditure
(
    FacultyId,
    TypeId,
    ItemName
)
VALUES
(2, 2, 'BOOKS'),
(2, 2, 'CD-ROM DATABASE'),
(2, 2, 'MICROFILMS'),
(2, 2, 'MICRO FICHES'),
(2, 2, 'AUDIO � CASSETTES'),
(2, 2, 'VIDEO � CASSETTES'),
(2, 2, 'BINDING WORKS');

-- ============================================================
-- Table Name : LibraryExpenditure
-- Purpose    : Stores college-wise proposed library expenditure
--              against the Library Expenditure Master items.
-- ============================================================

CREATE TABLE LibraryExpenditure
(
    LibraryExpenditureId INT IDENTITY(1,1) NOT NULL,

    CollegeCode NVARCHAR(100) NOT NULL,
    ItemId INT NOT NULL,

    ExpenditureProposed DECIMAL(18,2) NOT NULL,
    CourseLevel VARCHAR(10) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy NVARCHAR(100) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2 NULL,

    CONSTRAINT PK_LibraryExpenditure
        PRIMARY KEY (LibraryExpenditureId),

    CONSTRAINT FK_LibraryExpenditure_College
        FOREIGN KEY (CollegeCode)
        REFERENCES [dbo].[Affiliation_College_Master](CollegeCode),

    CONSTRAINT FK_LibraryExpenditure_Item
        FOREIGN KEY (ItemId)
        REFERENCES Mst_LibraryExpenditure(LibraryExpenditureId)
);

-----------------------------------------------


-- ============================================================
-- Table Name : MstDentalLibraryServices
-- Purpose    : Stores master data for dental library services
--              based on faculty and affiliation type.
-- ============================================================

CREATE TABLE MstDentalLibraryServices
(
    DentalLibraryServiceId INT IDENTITY(1,1) NOT NULL,

    FacultyId INT NOT NULL,
    TypeId INT NOT NULL,

    ServiceName NVARCHAR(250) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy NVARCHAR(100) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2 NULL,

    CONSTRAINT PK_MstDentalLibraryServices
        PRIMARY KEY (DentalLibraryServiceId),

    CONSTRAINT FK_MstDentalLibraryServices_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_MstDentalLibraryServices_AffiliationType
        FOREIGN KEY (TypeId)
        REFERENCES TypeOfAffiliation(TypeId)
);

-------------------------------------------


-- ============================================================
-- Insert Dental Library Services Master Items
-- FacultyId : 2
-- TypeId    : 2
-- ============================================================

INSERT INTO MstDentalLibraryServices
(
    FacultyId,
    TypeId,
    ServiceName
)
VALUES
(2, 2, 'Literature Search'),
(2, 2, 'Compiling Bibliography on Request'),
(2, 2, 'Compiling Bibliography in Anticipation'),
(2, 2, 'Selective Dissemination of Information'),
(2, 2, 'Abstracting Services'),
(2, 2, 'Indexing Services'),
(2, 2, 'Translating Material for Users'),
(2, 2, 'Current Awareness'),
(2, 2, 'MEDLARS / MEDLINE'),
(2, 2, 'E-Mail'),
(2, 2, 'Internet'),
(2, 2, 'Consultancy'),
(2, 2, 'Photocopying Facility');

----------------------------------------

-- ============================================================
-- Table Name : DentalLibraryServices
-- Purpose    : Stores college-wise availability of dental
--              library services.
-- ============================================================


CREATE TABLE DentalLibraryServices
(
    DentalLibraryServiceId INT IDENTITY(1,1) NOT NULL,

    CollegeCode NVARCHAR(100) NOT NULL,
    ServiceId INT NOT NULL,
    CourseLevel VARCHAR(10) NULL,

    IsAvailable BIT NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy NVARCHAR(100) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2 NULL,

    CONSTRAINT PK_DentalLibraryServices
        PRIMARY KEY (DentalLibraryServiceId),

    CONSTRAINT FK_DentalLibraryServices_College
        FOREIGN KEY (CollegeCode)
        REFERENCES [dbo].[Affiliation_College_Master](CollegeCode),

    CONSTRAINT FK_DentalLibraryServices_Service
        FOREIGN KEY (ServiceId)
        REFERENCES MstDentalLibraryServices(DentalLibraryServiceId)
);

-----------------------------------------------


-- ============================================================
-- Table Name : LibraryStaffDetails
-- Purpose    : Stores college-wise library staff details
--              including designation, qualification, experience,
--              pay scale and category.
-- ============================================================

CREATE TABLE LibraryStaffDetails
(
    LibraryStaffId INT IDENTITY(1,1) NOT NULL,

    CollegeCode NVARCHAR(100) NOT NULL,
    FacultyId INT NOT NULL,
    TypeId INT NOT NULL,

    Name NVARCHAR(200) NOT NULL,
    Designation NVARCHAR(150) NOT NULL,
    Qualification NVARCHAR(250) NULL,
    CourseLevel VARCHAR(10) NULL,

    ExperienceFrom DATE NULL,
    ExperienceTo DATE NULL,

    PayScale NVARCHAR(150) NULL,
    Category NVARCHAR(100) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy NVARCHAR(100) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2 NULL,

    CONSTRAINT PK_LibraryStaffDetails
        PRIMARY KEY (LibraryStaffId),

    CONSTRAINT FK_LibraryStaffDetails_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    CONSTRAINT FK_LibraryStaffDetails_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_LibraryStaffDetails_AffiliationType
        FOREIGN KEY (TypeId)
        REFERENCES TypeOfAffiliation(TypeId)
);

---------------------------------------------

-- ============================================================
-- Table Name : UserDetails
-- Purpose    : Stores college-wise library user details,
--              including staff/student counts and
--              user education programme availability.
-- ============================================================

CREATE TABLE UserDetails
(
    UserDetailsId INT IDENTITY(1,1) NOT NULL,

    CollegeCode NVARCHAR(100) NOT NULL,
    FacultyId INT NOT NULL,
    TypeId INT NOT NULL,
    CourseLevel VARCHAR(10) NULL,

    NoOfTeachingStaff INT NULL,
    NoOfResearchScholarsAssistants INT NULL,
    NoOfPostGraduateStudents INT NULL,
    NoOfUnderGraduateStudents INT NULL,
    NoOfAdministrativeStaff INT NULL,
    NoOfParaMedicalStaff INT NULL,
    NoOfOutsiders INT NULL,

    ProvideUserEducationProgrammes BIT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy NVARCHAR(100) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2 NULL,

    CONSTRAINT PK_UserDetails
        PRIMARY KEY (UserDetailsId),

    CONSTRAINT FK_UserDetails_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    CONSTRAINT FK_UserDetails_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_UserDetails_AffiliationType
        FOREIGN KEY (TypeId)
        REFERENCES TypeOfAffiliation(TypeId)
);

--------------------------------------

-- ============================================================
-- Table Name : WorkShopDetails
-- Purpose    : Stores course-level workshop details for a
--              college, faculty and affiliation type, including
--              staff, equipment and scope of work.
-- ============================================================

CREATE TABLE WorkShopDetails
(
    WorkShopDetailsId INT IDENTITY(1,1) NOT NULL,

    FacultyId INT NOT NULL,
    CollegeCode NVARCHAR(100) NOT NULL,
    TypeId INT NOT NULL,
    CourseLevel NVARCHAR(50) NOT NULL,

    Staff NVARCHAR(MAX) NULL,
    Equipment NVARCHAR(MAX) NULL,
    ScopeOfWork NVARCHAR(MAX) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy NVARCHAR(100) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME2 NULL,

    CONSTRAINT PK_WorkShopDetails
        PRIMARY KEY (WorkShopDetailsId),

    CONSTRAINT FK_WorkShopDetails_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_WorkShopDetails_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    CONSTRAINT FK_WorkShopDetails_AffiliationType
        FOREIGN KEY (TypeId)
        REFERENCES TypeOfAffiliation(TypeId)
);


--------------------------------------------

-- ============================================================
-- Table Name : AnimalHouseDetails
-- Purpose    : Stores college-wise Animal House particulars
--              for the current faculty, affiliation type
--              and course level.
-- ============================================================

CREATE TABLE AnimalHouseDetails
(
    AnimalHouseDetailsId INT IDENTITY(1,1) NOT NULL,

    FacultyId INT NOT NULL,

    CollegeCode NVARCHAR(100) NOT NULL,

    TypeId INT NOT NULL,

    CourseLevel NVARCHAR(50) NOT NULL,

    Area DECIMAL(18,2) NULL,

    Staff NVARCHAR(MAX) NULL,

    TypeOfAnimals NVARCHAR(MAX) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy NVARCHAR(100) NULL,

    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),

    ModifiedBy NVARCHAR(100) NULL,

    ModifiedDate DATETIME2 NULL,

    CONSTRAINT PK_AnimalHouseDetails
        PRIMARY KEY (AnimalHouseDetailsId),

    CONSTRAINT FK_AnimalHouseDetails_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_AnimalHouseDetails_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    CONSTRAINT FK_AnimalHouseDetails_AffiliationType
        FOREIGN KEY (TypeId)
        REFERENCES TypeOfAffiliation(TypeId)
);

---------------------------------------------------

INSERT INTO [dbo].[MST_FieldType_CHP]
(
    [FacultyCode],
    [FieldType]
)
VALUES
(2, 'Rural Field'),
(2, 'Urban Field');

----------------------------------------------------------------------

-- ============================================================
-- Table Name : DentalFieldPracticeArea
-- Purpose    : Stores dental field practice area details
--              for a college, faculty, affiliation type
--              and course level.
-- ============================================================

CREATE TABLE DentalFieldPracticeArea
(
    DentalFieldPracticeAreaId INT IDENTITY(1,1) NOT NULL,

    FacultyId INT NOT NULL,

    CollegeCode NVARCHAR(100) NOT NULL,

    TypeId INT NOT NULL,

    CourseLevel NVARCHAR(50) NOT NULL,

    -- Rural / Urban
    FieldTypeId INT NOT NULL,

    -- a. Location and address
    Location NVARCHAR(250) NOT NULL,
    Address NVARCHAR(500) NOT NULL,

    -- b. Managed by
    ManagedBy NVARCHAR(250) NOT NULL,

    -- c. Staff
    StaffList NVARCHAR(1000) NULL,

    -- d. Population served
    PopulationServed INT NULL,

    -- e. Activities and services provided
    ActivitiesAndServices NVARCHAR(2000) NULL,

    -- f. Records maintained
    RecordsMaintained NVARCHAR(2000) NULL,

    -- g. Equipments available
    EquipmentsAvailable NVARCHAR(2000) NULL,

    -- h(i). Residential / Non-Residential training activities
    TrainingActivities NVARCHAR(2000) NULL,

    -- h(ii). How supervision is done
    SupervisionMethod NVARCHAR(2000) NULL,

    -- h(iii). Accommodation for trainees and supervisors
    TraineeSupervisorAccommodation NVARCHAR(2000) NULL,

    -- Audit fields
    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy NVARCHAR(100) NULL,

    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),

    ModifiedBy NVARCHAR(100) NULL,

    ModifiedDate DATETIME2 NULL,

    CONSTRAINT PK_DentalFieldPracticeArea
        PRIMARY KEY (DentalFieldPracticeAreaId),

    CONSTRAINT FK_DentalFieldPracticeArea_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_DentalFieldPracticeArea_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    CONSTRAINT FK_DentalFieldPracticeArea_AffiliationType
        FOREIGN KEY (TypeId)
        REFERENCES TypeOfAffiliation(TypeId),

    CONSTRAINT FK_DentalFieldPracticeArea_FieldType
        FOREIGN KEY (FieldTypeId)
        REFERENCES MST_FieldType_CHP(Id)
);
--------------------------------

select * from CA_MST_Med_CommitteeNames
where FacultyCode=2



----------------------------------------------------------------


CREATE TABLE OtherHealthScienceColleges
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_OtherHealthScienceColleges PRIMARY KEY,

    FacultyId INT NOT NULL,

    CollegeCode NVARCHAR(100) NOT NULL,

    OtherCollegeCode NVARCHAR(100) NOT NULL,

    CourseCode INT NOT NULL,

    CreatedOn DATETIME2 NOT NULL
        CONSTRAINT DF_OtherHealthScienceColleges_CreatedOn
        DEFAULT GETDATE(),

    ModifiedOn DATETIME2 NULL,

    CONSTRAINT FK_OtherHealthScienceColleges_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_OtherHealthScienceColleges_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    CONSTRAINT FK_OtherHealthScienceColleges_OtherCollege
        FOREIGN KEY (OtherCollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

);

------------------------------------------------------------

----If a college should have only one Dental Field Practice Area record 
----for the current affiliation/course-level selection:

--ALTER TABLE DentalFieldPracticeArea
--ADD CONSTRAINT UQ_DentalFieldPracticeArea_College_Faculty_Type_Level
--UNIQUE
--(
--    CollegeCode,
--    FacultyId,
--    TypeId,
--    CourseLevel
--);

---------------------------------------------

-- ============================================================
-- Table Name : ActionTakenDeficiencyReport
-- Purpose    : Stores action taken details against deficiencies
--              pointed out during the previous inspection.
-- ============================================================

CREATE TABLE ActionTakenDeficiencyReport
(
    ActionTakenDeficiencyReportId INT IDENTITY(1,1) NOT NULL,

    FacultyId INT NOT NULL,

    CollegeCode NVARCHAR(100) NOT NULL,

    TypeId INT NOT NULL,

    CourseLevel NVARCHAR(50) NOT NULL,

    DeficiencyPointedOut NVARCHAR(MAX) NOT NULL,

    ExtentRemedied NVARCHAR(MAX) NOT NULL,

    RelevantReportPath NVARCHAR(500) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy NVARCHAR(100) NULL,

    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),

    ModifiedBy NVARCHAR(100) NULL,

    ModifiedDate DATETIME2 NULL,

    CONSTRAINT PK_ActionTakenDeficiencyReport
        PRIMARY KEY (ActionTakenDeficiencyReportId),

    CONSTRAINT FK_ActionTakenDeficiencyReport_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_ActionTakenDeficiencyReport_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    CONSTRAINT FK_ActionTakenDeficiencyReport_AffiliationType
        FOREIGN KEY (TypeId)
        REFERENCES TypeOfAffiliation(TypeId)
);
--------------------------------------------------

--select * from CA_MST_RegisterRecord


INSERT INTO [CA_MST_RegisterRecord]
(
    RegisterName,
    FacultyId,
    CourseLevel,
    AffiliationType
)
VALUES
(
    'Do you maintain the counterfoil of the receipt book?',
    1,
    'UG',
    2
),
(
    'Do you maintain the counterfoil of transfer certificates?',
    1,
    'UG',
    2
),
(
    'Do you maintain a register of address of students?',
    1,
    'UG',
    2
);

-----------------------------------------

--select * from CA_MST_Med_LibraryItems

INSERT INTO [Admission_Affiliation].[dbo].[CA_MST_Med_LibraryItems]
(
    FacultyCode,
    ItemName
)
VALUES
(2, 'Reports / Pamphlets'),
(2, 'Microfilms / Microfiche'),
(2, 'Slides'),
(2, 'Audio Cassettes'),
(2, 'Video Cassettes');

-----------------------------------------

--select * from [dbo].[CA_MST_Med_LibraryEquipments]

INSERT INTO [dbo].[CA_MST_Med_LibraryEquipments]
    ([FacultyCode], [EquipmentName])
VALUES
    (2, 'Connected to any network'),
    (2, 'Microfilm reader'),
    (2, 'Telephone'),
    (2, 'Telex'),
    (2, 'Fax');

-------------------------------------------------

INSERT INTO [Admission_Affiliation].[dbo].[HospitalFacilitiesMaster]
(
    [AffiliationTypeId],
    [FacultyCode],
    [FacilityName],
    [IsActive],
    [CreatedDate]
)
VALUES
(2, 2, 'Radiology', 1, GETDATE()),
(2, 2, 'Ultra Sound', 1, GETDATE()),
(2, 2, 'Clinical Laboratory', 1, GETDATE()),
(2, 2, 'Blood Bank', 1, GETDATE()),
(2, 2, 'Operation Theatre', 1, GETDATE()),
(2, 2, 'Casualty / Emergency Service', 1, GETDATE()),
(2, 2, 'Disposal of Hospital Waste', 1, GETDATE()),
(2, 2, 'Central Sterile Service', 1, GETDATE()),
(2, 2, 'Kitchen', 1, GETDATE()),
(2, 2, 'Laundry', 1, GETDATE()),
(2, 2, 'Canteen', 1, GETDATE()),
(2, 2, 'Pharmacy', 1, GETDATE()),
(2, 2, 'Workshop', 1, GETDATE()),
(2, 2, 'Stores', 1, GETDATE()),
(2, 2, 'Medical Records Keeping', 1, GETDATE()),
(2, 2, 'Mortuary and Central Cold Storage', 1, GETDATE()),
(2, 2, 'Any Other Special Services and Special Clinics', 1, GETDATE());

INSERT INTO [Admission_Affiliation].[dbo].[HospitalFacilitiesMaster]
(
    [AffiliationTypeId],
    [FacultyCode],
    [FacilityName],
    [IsActive],
    [CreatedDate]
)
VALUES
(2, 2, 'Central Photographic cum Audio Visual Unit', 1, GETDATE());

----------------------------------------------------------------------

CREATE TABLE StaffShortageDetails
(
    StaffShortageId INT IDENTITY(1,1) PRIMARY KEY,

    CollegeCode NVARCHAR(100) NOT NULL,
    FacultyId INT NOT NULL,

    PostName NVARCHAR(200) NOT NULL,
    ReasonForShortage NVARCHAR(1000) NULL,
    ArrangementMade NVARCHAR(1000) NULL,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedOn DATETIME NULL,

    CONSTRAINT FK_StaffShortageDetails_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    CONSTRAINT FK_StaffShortageDetails_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES Faculty(FacultyId)
);

--select * from StaffShortageDetails

--delete from CollegeAdditionalFeeDetails
-----------------------------------------------------

/* ============================================================
   Question 6 - Additional Fee / Donation / Capitation Details
   Stores details only when any fee other than tuition fee
   is levied by the college.
   ============================================================ */

--SELECT * FROM CollegeAdditionalFeeDetails

CREATE TABLE CollegeAdditionalFeeDetails
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_CollegeAdditionalFeeDetails PRIMARY KEY,

    CollegeCode NVARCHAR(100) NOT NULL,
    FacultyId INT NOT NULL,
    CourseLevel VARCHAR(10) NULL,

    IsFeeLevied BIT NOT NULL,

    FeeType NVARCHAR(200) NULL,
    FeeAmount DECIMAL(18,2) NULL,

    IsDeleted BIT NOT NULL
        CONSTRAINT DF_CollegeAdditionalFeeDetails_IsDeleted DEFAULT 0,

    CreatedOn DATETIME NOT NULL
        CONSTRAINT DF_CollegeAdditionalFeeDetails_CreatedOn DEFAULT GETDATE(),

    ModifiedOn DATETIME NULL,

    CONSTRAINT FK_CollegeAdditionalFeeDetails_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    CONSTRAINT FK_CollegeAdditionalFeeDetails_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES Faculty(FacultyId)
);

/* ============================================================
   Question 6 - Courses Offered

   Stores UG, PG and Diploma courses offered by the college,
   including course code, year of starting, sanctioned/admitted
   admissions and course-wise permission / affiliation documents.

   One row represents one course.

   Permission / affiliation documents:
   1. Permission of Government of Karnataka
   2. Permission of concerned Council / Apex Body
   3. Last Affiliation granted by RGUHS
   4. Permission of Government of India wherever applicable
   ============================================================ */

--SELECT * FROM CollegeCoursesOffered

CREATE TABLE CollegeCoursesOffered
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_CollegeCoursesOffered PRIMARY KEY,

    /* ========================================================
       College / Faculty Reference
       ======================================================== */

    CollegeCode NVARCHAR(100) NOT NULL,

    FacultyId INT NOT NULL,

    /* ========================================================
       Course Details
       ======================================================== */

    CourseCode NVARCHAR(100) NOT NULL,

    CourseLevel NVARCHAR(50) NOT NULL,

    CourseName NVARCHAR(300) NOT NULL,

    YearOfStarting INT NULL,

    /* ========================================================
       Admission Details
       ======================================================== */

    SanctionedAdmissions INT NULL,

    AdmittedAdmissions INT NULL,

    Remarks NVARCHAR(1000) NULL,

    /* ========================================================
       1. Government of Karnataka Permission
       ======================================================== */

    GovtKarnatakaPermissionNumber NVARCHAR(200) NULL,

    GovtKarnatakaDocumentName NVARCHAR(500) NULL,

    GovtKarnatakaDocumentPath NVARCHAR(1000) NULL,

    GovtKarnatakaDocumentContentType NVARCHAR(100) NULL,

    /* ========================================================
       2. Concerned Council / Apex Body Permission

       Example:
       Medical Council
       Dental Council
       AICTE
       etc.
       ======================================================== */

    CouncilPermissionNumber NVARCHAR(200) NULL,

    CouncilDocumentName NVARCHAR(500) NULL,

    CouncilDocumentPath NVARCHAR(1000) NULL,

    CouncilDocumentContentType NVARCHAR(100) NULL,

    /* ========================================================
       3. Last Affiliation Granted by RGUHS
       ======================================================== */

    RGUHSLastAffiliationNumber NVARCHAR(200) NULL,

    RGUHSLastAffiliationDocumentName NVARCHAR(500) NULL,

    RGUHSLastAffiliationDocumentPath NVARCHAR(1000) NULL,

    RGUHSLastAffiliationDocumentContentType NVARCHAR(100) NULL,

    /* ========================================================
       4. Government of India Permission
       Wherever applicable
       ======================================================== */

    GovtIndiaPermissionNumber NVARCHAR(200) NULL,

    GovtIndiaDocumentName NVARCHAR(500) NULL,

    GovtIndiaDocumentPath NVARCHAR(1000) NULL,

    GovtIndiaDocumentContentType NVARCHAR(100) NULL,

    /* ========================================================
       Soft Delete
       ======================================================== */

    IsDeleted BIT NOT NULL
        CONSTRAINT DF_CollegeCoursesOffered_IsDeleted
        DEFAULT 0,

    /* ========================================================
       Audit
       ======================================================== */

    CreatedOn DATETIME NOT NULL
        CONSTRAINT DF_CollegeCoursesOffered_CreatedOn
        DEFAULT GETDATE(),

    ModifiedOn DATETIME NULL,

    /* ========================================================
       Foreign Keys
       ======================================================== */

    CONSTRAINT FK_CollegeCoursesOffered_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    CONSTRAINT FK_CollegeCoursesOffered_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES Faculty(FacultyId)
);

-----------------------------------------------------

select * from DepartmentMaster
where FacultyCode=2

-----------------------------------------------------

/*
    Table Name : DepartmentWiseResearchProjects

    Description:
    This table stores department-wise research project information
    for a college, including the faculty, college, department,
    number of research projects completed/added during the last
    three years, and the supporting PDF document path.

    References:
    - FacultyCode  -> TblFacultyMasters.FacultyCode
    - CollegeCode  -> TblCollegeMasters.CollegeCode
    - DepartmentId -> DepartmentMaster.Id
*/


CREATE TABLE DepartmentWiseResearchProjects
(
    Id INT IDENTITY(1,1) NOT NULL,

    FacultyCode INT NOT NULL,

    CollegeCode NVARCHAR(100) NOT NULL,
    CourseLevel VARCHAR(10) NULL,

    DepartmentCode VARCHAR(50) NULL,

    NoOfResearchProjectsLast3Years INT NOT NULL DEFAULT 0,

    PdfFilePath NVARCHAR(1000) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy NVARCHAR(100) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),

    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME NULL,

    CONSTRAINT PK_DepartmentWiseResearchProjects
        PRIMARY KEY (Id),

    CONSTRAINT FK_DWRP_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_DWRP_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

);

---------------------------------------------------------