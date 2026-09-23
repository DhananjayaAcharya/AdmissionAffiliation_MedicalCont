
--1
/* ============================================================
   TABLE: MstDentalAffiliationType

   PURPOSE:
   Stores affiliation types specifically applicable to the
   Dental faculty.

   EXAMPLES:
   - Fresh Affiliation
   - Continuation of Affiliation
   - Enhancement / Increase in Intake

   RELATIONSHIPS:
   - FacultyCode → Faculty(FacultyId)

   ============================================================ */

CREATE TABLE MstDentalAffiliationType
(
    DentalAffiliationTypeId INT IDENTITY(1,1) PRIMARY KEY,

    FacultyCode INT NOT NULL,

    AffiliationCategory NVARCHAR(200) NOT NULL,

    TypeId INT null,

    AcademicYear NVARCHAR(20) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CourseLevelGroup VARCHAR(50) NULL,
    -- Examples: UG, PG

    CreatedBy VARCHAR(100) NULL,

    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),

    ModifiedBy VARCHAR(100) NULL,

    ModifiedDate DATETIME NULL,

    CONSTRAINT FK_MstDentalAffiliationType_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT UQ_MstDentalAffiliationType
        UNIQUE
        (
            FacultyCode,
            AffiliationCategory,
            AcademicYear,
            CourseLevelGroup
        )
);

--2
/* ============================================================
   TABLE: MstDentalFeeTypes

   PURPOSE:
   This master table stores the different types of fees applicable
   to Dental courses.

   EXAMPLES:
   - Application Fee
   - Annual Fee
   - Continuation / Renewal Fee
   - Administrative Fee & Service Charges
   - Institutional Helinet Fee
   - Course Identification Fee

   RELATIONSHIPS:
   - FacultyCode → Faculty(FacultyId)
   - AffiliationTypeId → MstAffiliationType(AffiliationTypeId)

   ============================================================ */

CREATE TABLE MstDentalFeeTypes
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    FacultyCode INT NOT NULL,

    FeeType NVARCHAR(300) NOT NULL,
    CourseLevel NVARCHAR(50) NOT NULL,

    DisplayOrder INT NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    AffiliationTypeId INT NOT NULL,

    CreatedBy VARCHAR(100) NULL,

    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),

    ModifiedBy VARCHAR(100) NULL,

    ModifiedDate DATETIME NULL,

    CONSTRAINT FK_MstDentalFeeTypes_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_MstDentalFeeTypes_DentalAffiliationType
        FOREIGN KEY (AffiliationTypeId)
        REFERENCES MstDentalAffiliationType(DentalAffiliationTypeId),

    CONSTRAINT UQ_MstDentalFeeTypes
        UNIQUE
        (
            FacultyCode,
            FeeType,
            AffiliationTypeId,
            CourseLevel
        )
);

--ALTER TABLE MstDentalFeeTypes
--DROP CONSTRAINT FK_MstDentalFeeTypes_AffiliationType;

--ALTER TABLE MstDentalFeeTypes
--ADD CONSTRAINT FK_MstDentalFeeTypes_DentalAffiliationType
--    FOREIGN KEY (AffiliationTypeId)
--    REFERENCES MstDentalAffiliationType(DentalAffiliationTypeId);

--ALTER TABLE MstDentalFeeTypes
--ADD CourseLevel NVARCHAR(50) NULL;

--UPDATE MstDentalFeeTypes
--SET CourseLevel = 'UG'
--WHERE CourseLevel IS NULL;

/* ============================================================
   TABLE: MstDentalFeeStructure

   PURPOSE:
   This table stores the actual fee amount applicable for a
   particular Dental course and fee type.

   EXAMPLE:
   
   Fee Type                 Course     Amount       Calculation
   -------------------------------------------------------------
   Application Fee          BDS        3000         Fixed
   Application Fee          MDS        3000         Fixed
   Annual Fee               BDS        80000        Fixed
   Renewal Fee              BDS        250000       Per Course
   Renewal Fee              MDS        4500         Per Seat

   RELATIONSHIPS:
   - FacultyCode → Faculty(FacultyId)
   - FeeTypeId → MstDentalFeeTypes(Id)
   - AffiliationTypeId → MstAffiliationType(AffiliationTypeId)

   ============================================================ */

--3
CREATE TABLE MstDentalFeeStructure
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    FacultyCode INT NOT NULL,

    FeeTypeId INT NOT NULL,

    CourseName VARCHAR (50) NOT NULL,

    CourseCode INT NULL,

    CourseLevel VARCHAR(50) NULL,

    AmountToBePaid DECIMAL(18,2) NOT NULL,

    CalculationType VARCHAR(50) NULL,
    -- Examples: Fixed, Per Course, Per Seat

    AffiliationTypeId INT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy VARCHAR(100) NULL,

    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),

    ModifiedBy VARCHAR(100) NULL,

    ModifiedDate DATETIME NULL,

    CONSTRAINT FK_MstDentalFeeStructure_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_MstDentalFeeStructure_FeeType
        FOREIGN KEY (FeeTypeId)
        REFERENCES MstDentalFeeTypes(Id),

    CONSTRAINT FK_MstDentalFeeStructure_AffiliationType
        FOREIGN KEY (AffiliationTypeId)
        REFERENCES MstAffiliationType(AffiliationTypeId),

    CONSTRAINT UQ_MstDentalFeeStructure
        UNIQUE
        (
            FacultyCode,
            FeeTypeId,
            CourseName,
            CourseLevel,
            AffiliationTypeId
        )
);


/* ============================================================
   TABLE: MstDentalOtherFeeStructure

   PURPOSE:
   This table stores additional Dental affiliation-related fees
   that are not specifically dependent on BDS or MDS courses.

   EXAMPLES:
   - Application Fee
   - Fee for Change of Name of the Institution
   - Fee for Change of Address of the Institution
   - Re-Inspection Fee

   RELATIONSHIPS:
   - FacultyCode → Faculty(FacultyId)
   - AffiliationTypeId → MstAffiliationType(AffiliationTypeId)

   ============================================================ */
--4
CREATE TABLE MstDentalOtherFeeStructure
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    FacultyCode INT NOT NULL,

    FeeName NVARCHAR(300) NOT NULL,

    AmountToBePaid DECIMAL(18,2) NOT NULL,

    AffiliationTypeId INT NULL,

    DisplayOrder INT NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy VARCHAR(100) NULL,

    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),

    ModifiedBy VARCHAR(100) NULL,

    ModifiedDate DATETIME NULL,

    CONSTRAINT FK_MstDentalOtherFeeStructure_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_MstDentalOtherFeeStructure_AffiliationType
        FOREIGN KEY (AffiliationTypeId)
        REFERENCES MstAffiliationType(AffiliationTypeId)
);



CREATE INDEX IX_MstDentalFeeTypes_Search
ON MstDentalFeeTypes
(
    FacultyCode,
    AffiliationTypeId,
    IsActive
);


CREATE INDEX IX_MstDentalFeeStructure_Search
ON MstDentalFeeStructure
(
    FacultyCode,
    AffiliationTypeId,
    FeeTypeId,
    IsActive
);


CREATE INDEX IX_MstDentalOtherFeeStructure_Search
ON MstDentalOtherFeeStructure
(
    FacultyCode,
    AffiliationTypeId,
    IsActive
);



/* ============================================================
   TABLE: TxnDentalFeeStructure

   PURPOSE:
   Stores the Dental affiliation fee details applicable to a
   specific college/application.

   Fee details are copied from the Dental Fee Master tables
   and stored here as transaction data.

   RELATIONSHIPS:
   - FacultyCode → Faculty(FacultyId)
   - AffiliationTypeId → MstAffiliationType(AffiliationTypeId)
   - FeeTypeId → MstDentalFeeTypes(Id)
   - DentalFeeStructureId → MstDentalFeeStructure(Id)

   ============================================================ */

--5
CREATE TABLE TxnDentalFeeStructure
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    CollegeCode VARCHAR(50) NOT NULL,

    FacultyCode INT NOT NULL,

    AffiliationTypeId INT NOT NULL,

    FeeTypeId INT NOT NULL,

    DentalFeeStructureId INT NULL,

    CourseName VARCHAR(50) NULL,

    CourseCode INT NULL,

    CourseLevel VARCHAR(50) NULL,

    AmountToBePaid DECIMAL(18,2) NOT NULL,

    CalculationType VARCHAR(50) NULL,

    AcademicIntake2026 INT NULL,
    -- Example:
    -- Per Seat  → Number of Seats
    -- Per Course → Number of Courses
    -- Fixed → 1

    CalculatedAmount DECIMAL(18,2) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy VARCHAR(100) NULL,

    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),

    ModifiedBy VARCHAR(100) NULL,

    ModifiedDate DATETIME NULL,

    CONSTRAINT FK_TxnDentalFeeStructure_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_TxnDentalFeeStructure_AffiliationType
        FOREIGN KEY (AffiliationTypeId)
        REFERENCES MstAffiliationType(AffiliationTypeId),

    CONSTRAINT FK_TxnDentalFeeStructure_FeeType
        FOREIGN KEY (FeeTypeId)
        REFERENCES MstDentalFeeTypes(Id),

    CONSTRAINT FK_TxnDentalFeeStructure_DentalFeeStructure
        FOREIGN KEY (DentalFeeStructureId)
        REFERENCES MstDentalFeeStructure(Id)
);


/* ============================================================
   TABLE: TxnDentalPayment

   PURPOSE:
   This table stores the overall payment transaction details
   for a Dental Affiliation application.

   One record represents one payment transaction and stores:
   - College and Faculty details
   - Affiliation Type
   - Transaction ID
   - Transaction Receipt file path
   - Total Amount Paid

   Individual fee calculation rows remain in:
   dbo.TxnDentalFeeStructures

   This table can later be linked with TxnDentalFeeStructures
   using DentalPaymentId for a one-to-many relationship.
   ============================================================ */

--6
CREATE TABLE dbo.TxnDentalPayment
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_TxnDentalPayment PRIMARY KEY,

    CollegeCode NVARCHAR(50) NOT NULL,

    CourseLevel VARCHAR(50) NULL,
    FacultyCode INT NOT NULL,

    AffiliationTypeId INT NOT NULL,

    TransactionId NVARCHAR(100) NOT NULL,

    TransactionReceiptPath NVARCHAR(500) NOT NULL,

    AmountPaid DECIMAL(18,2) NOT NULL,

    IsActive BIT NOT NULL
        CONSTRAINT DF_TxnDentalPayment_IsActive DEFAULT 1,

    CreatedBy NVARCHAR(100) NULL,

    CreatedDate DATETIME NOT NULL
        CONSTRAINT DF_TxnDentalPayment_CreatedDate DEFAULT GETDATE(),

    ModifiedBy NVARCHAR(100) NULL,

    ModifiedDate DATETIME NULL
);


/* ============================================================
   FOREIGN KEY RELATIONSHIP

   Links TxnDentalFeeStructures.DentalPaymentId with
   TxnDentalPayment.Id.
   ============================================================ */
--7
ALTER TABLE [dbo].[TxnDentalFeeStructure]
ADD DentalPaymentId INT NULL;

ALTER TABLE [dbo].[TxnDentalFeeStructure]
ADD CONSTRAINT FK_TxnDentalFeeStructures_TxnDentalPayment
FOREIGN KEY (DentalPaymentId)
REFERENCES dbo.TxnDentalPayment(Id);


/* ============================================================
   TABLE: TxnDentalOtherFeeStructure

   PURPOSE:
   Stores additional Dental affiliation-related fees selected
   or applicable to a specific college/application.

   EXAMPLES:
   - Change of Name of Institution
   - Change of Address of Institution
   - Re-Inspection Fee

   RELATIONSHIPS:
   - FacultyCode → Faculty(FacultyId)
   - AffiliationTypeId → MstAffiliationType(AffiliationTypeId)
   - DentalOtherFeeStructureId
       → MstDentalOtherFeeStructure(Id)

   ============================================================ */

--8
CREATE TABLE TxnDentalOtherFeeStructure
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    CollegeCode VARCHAR(50) NOT NULL,

    FacultyCode INT NOT NULL,

    AffiliationTypeId INT NOT NULL,

    DentalOtherFeeStructureId INT NOT NULL,

    FeeName NVARCHAR(300) NOT NULL,

    AmountToBePaid DECIMAL(18,2) NOT NULL,

    IsApplicable BIT NOT NULL DEFAULT 1,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy VARCHAR(100) NULL,

    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),

    ModifiedBy VARCHAR(100) NULL,

    ModifiedDate DATETIME NULL,

    CONSTRAINT FK_TxnDentalOtherFeeStructure_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_TxnDentalOtherFeeStructure_AffiliationType
        FOREIGN KEY (AffiliationTypeId)
        REFERENCES MstAffiliationType(AffiliationTypeId),

    CONSTRAINT FK_TxnDentalOtherFeeStructure_Master
        FOREIGN KEY (DentalOtherFeeStructureId)
        REFERENCES MstDentalOtherFeeStructure(Id)
);

--select * from MstAffiliationType


--SELECT 
--    AffiliationTypeId,
--    FacultyCode,
--    AffiliationCategory,
--    AcademicYear
--FROM MstAffiliationType
--WHERE FacultyCode = '2'
--  AND IsActive = 1;

-- Drop existing unique constraint
--ALTER TABLE MstDentalFeeTypes
--DROP CONSTRAINT UQ_MstDentalFeeTypes;

--ALTER TABLE MstDentalFeeTypes
--ADD CONSTRAINT UQ_MstDentalFeeTypes
--UNIQUE
--(
--    FacultyCode,
--    FeeType,
--    AffiliationTypeId,
--    CourseLevel
--);

--9
INSERT INTO MstDentalAffiliationType
(
    TypeId,
    FacultyCode,
    AffiliationCategory,
    AcademicYear,
    CourseLevelGroup,
    IsActive,
    CreatedDate
)
VALUES
    -- UG
    (1, 2, 'Fresh Affiliation', '2025-26', 'UG', 1, GETDATE()),
    (2, 2, 'Continuation of Affiliation', '2025-26', 'UG', 1, GETDATE()),
    (3, 2, 'Enhancement of Seats / Increase in Intake', '2025-26', 'UG', 1, GETDATE()),
    (4, 2, 'Additional Courses for college', '2025-26', 'UG', 1, GETDATE()),
    (5, 2, 'Renewal of Consent of Affiliation', '2025-26', 'UG', 1, GETDATE()),

    -- PG
    (1, 2, 'Fresh Affiliation', '2025-26', 'PG', 1, GETDATE()),
    (2, 2, 'Continuation of Affiliation', '2025-26', 'PG', 1, GETDATE()),
    (3, 2, 'Enhancement of Seats / Increase in Intake', '2025-26', 'PG', 1, GETDATE()),
    (4, 2, 'Additional Courses for college', '2025-26', 'PG', 1, GETDATE()),
    (5, 2, 'Renewal of Consent of Affiliation', '2025-26', 'PG', 1, GETDATE()),

    -- COMMON / INSTITUTION LEVEL
    (6, 2, 'Change of Name of the College', '2025-26', NULL, 1, GETDATE()),
    (7, 2, 'Change of Address of the College', '2025-26', NULL, 1, GETDATE()),
    (8, 2, 'Change of Name And Address of the College', '2025-26', NULL, 1, GETDATE());

--10
INSERT INTO MstDentalFeeTypes
(
    FacultyCode,
    FeeType,
    CourseLevel,
    DisplayOrder,
    AffiliationTypeId,
    IsActive,
    CreatedBy,
    CreatedDate
)
VALUES
(2, 'Application Fee', 'UG', 1, 2, 1, 'Admin', GETDATE()),
(2, 'Annual Fee', 'UG', 2, 2, 1, 'Admin', GETDATE()),
(2, 'Continuation of Affiliation / Renewal Fee of Affiliation', 'UG', 3, 2, 1, 'Admin', GETDATE()),
(2, 'Administrative Fee & Service Charges', 'UG', 4, 2, 1, 'Admin', GETDATE()),
(2, 'Institutional Helinet Fee', 'UG', 5, 2, 1, 'Admin', GETDATE()),
(2, 'Course Identification Fee', 'UG', 6, 2, 1, 'Admin', GETDATE());

--11
INSERT INTO MstDentalFeeTypes
(
    FacultyCode,
    FeeType,
    CourseLevel,
    DisplayOrder,
    AffiliationTypeId,
    IsActive,
    CreatedBy,
    CreatedDate
)
VALUES
(2, 'Application Fee', 'PG', 1, 7, 1, 'Admin', GETDATE()),
(2, 'Annual Fee', 'PG', 2, 7, 1, 'Admin', GETDATE()),
(2, 'Continuation of Affiliation / Renewal Fee of Affiliation', 'PG', 3, 7, 1, 'Admin', GETDATE()),
(2, 'Administrative Fee & Service Charges', 'PG', 4, 7, 1, 'Admin', GETDATE()),
(2, 'Institutional Helinet Fee', 'PG', 5, 7, 1, 'Admin', GETDATE()),
(2, 'Course Identification Fee', 'PG', 6, 7, 1, 'Admin', GETDATE());
-------------------------------------------------------------

--12
INSERT INTO MstDentalFeeStructure (FacultyCode, FeeTypeId, CourseName, CourseCode, CourseLevel, AmountToBePaid, CalculationType, AffiliationTypeId, IsActive, CreatedBy, CreatedDate)
VALUES
(2, 1, 'BDS', NULL, 'UG', 3000, 'Fixed', 2, 1, 'Admin', GETDATE()),
(2, 1, 'MDS', NULL, 'PG', 3000, 'Fixed', 2, 1, 'Admin', GETDATE()),
(2, 2, 'BDS', NULL, 'UG', 80000, 'Fixed', 2, 1, 'Admin', GETDATE()),
(2, 3, 'BDS', NULL, 'UG', 250000, 'Per Course', 2, 1, 'Admin', GETDATE()),
(2, 3, 'MDS', NULL, 'PG', 4500, 'Per Seat', 2, 1, 'Admin', GETDATE()),
(2, 4, 'BDS', NULL, 'UG', 1500, 'Per Seat', 2, 1, 'Admin', GETDATE()),
(2, 5, 'BDS', NULL, 'UG', 100000, 'Fixed', 2, 1, 'Admin', GETDATE()),
(2, 5, 'MDS', NULL, 'PG', 30000, 'Fixed', 2, 1, 'Admin', GETDATE()),
(2, 6, 'BDS', NULL, 'UG', 20, 'Fixed', 2, 1, 'Admin', GETDATE()),
(2, 6, 'MDS', NULL, 'PG', 20, 'Fixed', 2, 1, 'Admin', GETDATE());

--13
-------------------------------------------------------
INSERT INTO MstDentalOtherFeeStructure
(
    FacultyCode,
    FeeName,
    AmountToBePaid,
    AffiliationTypeId,
    DisplayOrder,
    IsActive,
    CreatedBy,
    CreatedDate
)
VALUES

( 2, 'Application Fee', 3000, 2, 1, 1, 'Admin', GETDATE()),

( 2, 'Fee for Change of Name of the Institution', 300000, 2, 2, 1, 'Admin', GETDATE()),

( 2, 'Fee for Change of Address of the Institution', 500000, 2,  3, 1, 'Admin', GETDATE()),
( 2, 'Re-Inspection Fee', 100000, 2, 4, 1, 'Admin', GETDATE());


--ALTER TABLE MstDentalAffiliationType
--ADD TypeId INT null;




--select *  from [dbo].TxnDentalFeeStructure
--where CollegeCode = 'd038' and @AffiliationTypeId = 2 and FacultyCode = 2 and CourseLevel

--select * from CA_Progress
--where CollegeCode = 'd038';

--delete from CA_Progress
--where Id in (1395, 1396)
---------------------------------------------------------------
--ALTER TABLE TxnDentalPayment
--ADD CourseLevel VARCHAR(50) NULL;


--select * from mstdentalfeestructure;

--DELETE FROM MstDentalFeeStructure;

--delete from TxnDentalFeeStructure

--select * from mstdentalfeetypes;

--select * from mstdentalotherfeestructure



--14
Update CA_MST_Med_CommitteeNames
SET CommitteeName = 'Internal Committee(POSH)'
where CommitteeName like '%POSH%' and FacultyCode = 2

--------------------------------
--15
ALTER TABLE DentalChairs
ADD AffiliationTypeId INT NULL;

--16
ALTER TABLE DentalChairs
ADD CONSTRAINT FK_DentalChairs_TypeOfAffiliation
FOREIGN KEY (AffiliationTypeId)
REFERENCES TypeOfAffiliation(TypeId);

--17
UPDATE DentalChairs
SET AffiliationTypeId = 2
WHERE AffiliationTypeId IS NULL;

--------------------------------

-----------------------------------------------------

--18
INSERT INTO CA_MST_CourseCurriculum
  (CurriculumName, IsActive)
  VALUES
  ('Vacation Period', 1),
  ('University Examination', 1);


-------------------------------------------------

--19

ALTER TABLE [dbo].[Medical_DepartmentOfficesMeu]
ADD
    DEUYearOfStarting NVARCHAR(50) NULL,
    NatureOfActivities NVARCHAR(2000) NULL;

--20
ALTER TABLE [dbo].[Medical_DepartmentOfficesMeu]
ADD
    TypeId INT NULL,
    CONSTRAINT FK_Medical_DepartmentOfficesMeu_AffiliationType
        FOREIGN KEY (TypeId)
        REFERENCES [dbo].[TypeOfAffiliation](TypeId);

-----------------------------------------------

--21
ALTER TABLE [dbo].[AFF_InstitutionsDetails]
ADD NameOfAdministrativeAuthority VARCHAR(250) NULL,
    AddressOfAdministrativeAuthority VARCHAR (MAX) NULL,
    MembersOfGoverningBodyOrCouncilFilePath VARCHAR (500) NULL;


----------------------------------------------------

--22
    ALTER TABLE [dbo].[AFF_HostelDetails]
ADD
    CommonRoomForMenArea DECIMAL(10,2) NULL,
    CommonRoomForWomenArea DECIMAL(10,2) NULL,
    GamesRecreationFacilities NVARCHAR(MAX) NULL,
    MedicalExaminationHealthServices NVARCHAR(MAX) NULL;
-------------------------------------------------

--23

ALTER TABLE DentalCollegeLandBuildingDetail
ADD 
    PrincipalStaffResidentialQuarter BIT NULL,
    PrincipalStaffResidentialQuarterAreaSqFt DECIMAL(10,2) NULL,

    OtherStaffResidentialQuarter BIT NULL,
    OtherStaffResidentialQuarterAreaSqFt DECIMAL(10,2) NULL,

    TeachingAncillaryStaffResidentialQuarter BIT NULL,
    TeachingAncillaryStaffResidentialQuarterAreaSqFt DECIMAL(10,2) NULL;

------------------------------------------------------------------

--24

ALTER TABLE [dbo].[HospitalDetailsForAffiliation]
ADD
    KPMECertificatePdfPath NVARCHAR(500) NULL,
    PollutionControlBoardCertificatePdfPath NVARCHAR(500) NULL,
    BioMedicalCertificatePdfPath NVARCHAR(500) NULL,
    DrugFreeCampusCertificationPdfPath NVARCHAR(500) NULL,
    ProposedPlansForFutureDevelopmentsPdfPath NVARCHAR(500) NULL;

------------------------------------------------------------------
--SELECT * FROM [HospitalDetailsForAffiliation]
--WHERE CollegeCode = 'd038'

--25
UPDATE HospitalDetailsForAffiliation
SET CourseLevel = 'UG'
WHERE FacultyCode = 2 

------------------------------------------------------------------

--26
ALTER TABLE [dbo].[HospitalDetailsForAffiliation]
ADD
    HasAnatomyActRegistration BIT NULL,
    AnatomyActRegistrationDetails NVARCHAR(MAX) NULL,
    AnatomyActRegistrationPdfPath NVARCHAR(500) NULL,
    HasHospitalTieUp BIT NULL;

----------------------------------------------------------------

--select *  FROM [HospitalTieUpDetails]
--WHERE CollegeCode = 'd038'

--27
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

--28
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

--29
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

--30
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

--31
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

--32
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

--33
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

--34
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
--35

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

--36
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

--37
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

--38
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

--39

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

--select * from CA_MST_Med_CommitteeNames
--where FacultyCode=2



----------------------------------------------------------------

--40
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

--41
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

--42
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

--43
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

--44
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

--45
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

--46
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

--47
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

--48
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
--49
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

--select * from DepartmentMaster
--where FacultyCode=2

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
*/

--50
CREATE TABLE DepartmentWiseResearchProjects
(
    Id INT IDENTITY(1,1) NOT NULL,

    FacultyCode INT NOT NULL,

    CollegeCode NVARCHAR(100) NOT NULL,
    CourseLevel VARCHAR(10) NULL,
    TypeId INT NULL,

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

    CONSTRAINT FK_DepartmentWiseResearchProjects_AffiliationType
        FOREIGN KEY (TypeId)
        REFERENCES TypeOfAffiliation(TypeId)

);


--ALTER TABLE DepartmentWiseResearchProjects
--ADD TypeId INT NULL;

--ALTER TABLE DepartmentWiseResearchProjects
--ADD CONSTRAINT FK_DepartmentWiseResearchProjects_AffiliationType
--    FOREIGN KEY (TypeId)
--    REFERENCES TypeOfAffiliation(TypeId);



---------------------------------------------------------

/*
    Table Name : AdditionalInformationInAcademicActivities

    Description:
    This table stores additional information related to academic
    activities of a college, including the availability of a
    Medical Education Unit, TOT programmes conducted and attended,
    and the supporting CME programme PDF document path.

    References:
    - FacultyCode -> Faculty master
    - CollegeCode -> College master
*/

--51
CREATE TABLE AdditionalInformationInAcademicActivities
(
    Id INT IDENTITY(1,1) NOT NULL,

    FacultyCode INT NOT NULL,

    CollegeCode NVARCHAR(100) NOT NULL,

    CourseLevel VARCHAR(50) NULL,
    TypeId INT NULL,

    HasMedicalEducationUnit BIT NOT NULL DEFAULT 0,

    HasTOTProgrammesConducted BIT NOT NULL DEFAULT 0,

    HasTOTProgrammesAttended BIT NOT NULL DEFAULT 0,
    -- Number of TOT programmes conducted
    TOTProgrammesConducted INT NOT NULL DEFAULT 0,

    -- Number of TOT programmes attended
    TOTProgrammesAttended INT NOT NULL DEFAULT 0,
    HasCMEProgrammesConducted BIT NULL,
    NoOfCMEProgrammesConducted INT NULL,
    HasCMEProgrammesAttended BIT NULL,
    NoOfCMEProgrammesAttended INT NULL,

    CMEProgrammePdfPath NVARCHAR(1000) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedBy NVARCHAR(100) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),

    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME NULL,

    CONSTRAINT PK_AdditionalInformationInAcademicActivities
        PRIMARY KEY (Id),

    CONSTRAINT FK_AIAAF_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_AIAAF_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    CONSTRAINT FK_AdditionalInformationInAcademicActivities_AffiliationType
        FOREIGN KEY (TypeId)
        REFERENCES TypeOfAffiliation(TypeId)
);


--ALTER TABLE AdditionalInformationInAcademicActivities
--ADD TypeId INT NULL;

--ALTER TABLE AdditionalInformationInAcademicActivities
--ADD CONSTRAINT FK_AdditionalInformationInAcademicActivities_AffiliationType
--    FOREIGN KEY (TypeId)
--    REFERENCES TypeOfAffiliation(TypeId);

--update AdditionalInformationInAcademicActivities
--set TypeId = 2
-----------------------------


-- ============================================================
-- Table: DentalConferencesConducted
-- Description:
-- Stores department/institution-level details of conferences
-- conducted by dental colleges, including conference name,
-- place, date, college, faculty, and course level.
-- ============================================================


--52

CREATE TABLE DentalConferencesConducted
(
    Id INT IDENTITY(1,1) NOT NULL,

    -- College reference
    CollegeCode NVARCHAR(100) NOT NULL,

    -- Faculty reference
    FacultyCode INT NOT NULL,

    -- Course level: UG / PG / SS
    CourseLevel VARCHAR(50) NULL,

    TypeId INT NULL,

    -- Conference details
    ConferenceName NVARCHAR(500) NOT NULL,

    ConferencePlace NVARCHAR(300) NOT NULL,

    ConferenceDate DATE NOT NULL,

    -- Status
    IsActive BIT NOT NULL DEFAULT 1,

    -- Audit fields
    CreatedBy NVARCHAR(100) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),

    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME NULL,

    -- Primary Key
    CONSTRAINT PK_DentalConferencesConducted
        PRIMARY KEY (Id),

    -- Faculty Foreign Key
    CONSTRAINT FK_DentalConferencesConducted_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    -- College Foreign Key
    CONSTRAINT FK_DentalConferencesConducted_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    CONSTRAINT FK_DentalConferencesConducted_Affiliated_TypeId
        FOREIGN KEY (TypeId)
        REFERENCES TypeOfAffiliation(TypeId)
);


-----------------------------------------------------------

--53

CREATE TABLE DentalConferencesAttended
(
    Id INT IDENTITY(1,1) NOT NULL,

    -- College
    CollegeCode NVARCHAR(100) NOT NULL,

    -- Faculty
    FacultyCode INT NOT NULL,

    -- Course Level
    CourseLevel VARCHAR(50) NULL,

    -- Affiliation Type
    TypeId INT NULL,

    -- Conference Details
    ConferenceName NVARCHAR(500) NOT NULL,

    ConferencePlace NVARCHAR(300) NOT NULL,

    ConferenceDate DATE NOT NULL,

    -- Participants
    StudentParticipants INT NOT NULL DEFAULT 0,

    TeacherParticipants INT NOT NULL DEFAULT 0,

    TotalParticipants INT NOT NULL DEFAULT 0,

    -- Status
    IsActive BIT NOT NULL DEFAULT 1,

    -- Audit
    CreatedBy NVARCHAR(100) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),

    ModifiedBy NVARCHAR(100) NULL,
    ModifiedDate DATETIME NULL,

    -- Primary Key
    CONSTRAINT PK_DentalConferencesAttended
        PRIMARY KEY (Id),

    -- Faculty FK
    CONSTRAINT FK_DentalConferencesAttended_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    -- College FK
    CONSTRAINT FK_DentalConferencesAttended_College
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    -- Affiliation Type FK
    CONSTRAINT FK_DentalConferencesAttended_Affiliated_TypeId
        FOREIGN KEY (TypeId)
        REFERENCES TypeOfAffiliation(TypeId)
);

---------------------------------------------------