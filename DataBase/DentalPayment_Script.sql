
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
