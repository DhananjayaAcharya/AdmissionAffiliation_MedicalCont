
select * from TblRguhsFacultyUser
where Faculty = 2

------------- PRIMARY KEY FOR FACULTY USER -----------
EXEC sp_rename
    'dbo.TblRguhsFacultyUser',
    'TblRguhsFacultyUser_Old';
GO


CREATE TABLE dbo.TblRguhsFacultyUser
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_TblRguhsFacultyUser PRIMARY KEY,

    UserId INT NOT NULL,
    Password NVARCHAR(256) NULL,
    PasswordHash NVARCHAR(MAX) NULL,
    UserName NVARCHAR(100) NULL,
    IsActive BIT NOT NULL,
    Faculty INT NULL,
    IsFinance BIT NULL,
    FinanceDesignation VARCHAR(10) NULL,
    DesignationDescription VARCHAR(200) NULL,
    IsSection BIT NULL,
    IsAdmin BIT NULL,
    FailedLoginAttempts INT NOT NULL,
    LockoutEndTime DATETIME NULL
);
GO


INSERT INTO dbo.TblRguhsFacultyUser
(
    UserId,
    Password,
    PasswordHash,
    UserName,
    IsActive,
    Faculty,
    IsFinance,
    FinanceDesignation,
    DesignationDescription,
    IsSection,
    IsAdmin,
    FailedLoginAttempts,
    LockoutEndTime
)
SELECT
    UserId,
    Password,
    PasswordHash,
    UserName,
    IsActive,
    Faculty,
    IsFinance,
    FinanceDesignation,
    DesignationDescription,
    IsSection,
    IsAdmin,
    FailedLoginAttempts,
    LockoutEndTime
FROM dbo.TblRguhsFacultyUser_Old
ORDER BY Id;

--------------------------------

--DROP TABLE dbo.TblRguhsFacultyUser_Old;

----------------------------------


ALTER TABLE [Admission_Affiliation].[dbo].[CA_Med_StaffParticularsOther]
ADD
    ExaminerDetailsPdfName2 NVARCHAR(255) NULL,
    ExaminerDetailsPdfName3 NVARCHAR(255) NULL,
    ExaminerDetailsPdfName4 NVARCHAR(255) NULL,
    ExaminerDetailsPdfName5 NVARCHAR(255) NULL,
    ExaminerDetailsPdfPath2 NVARCHAR(500) NULL,
    ExaminerDetailsPdfPath3 NVARCHAR(500) NULL,
    ExaminerDetailsPdfPath4 NVARCHAR(500) NULL,
    ExaminerDetailsPdfPath5 NVARCHAR(500) NULL;

ALTER TABLE IndoorBedsOccupancy
ADD CONSTRAINT CK_IndoorBedsOccupancy_RGUHSIntake
CHECK (RGUHSintake >= 0);

ALTER TABLE DentalCollegeLandBuildingDetail
ADD Latitude  decimal(9,6) NULL,
    Longitude decimal(9,6) NULL;

ALTER TABLE DentalCollegeLandBuildingDetail
ADD CourseLevel VARCHAR(10) NULL;

UPDATE DentalCollegeLandBuildingDetail
SET CourseLevel = 'UG';
------------------------------


ALTER TABLE [dbo].[Affiliation_College_Master]
ADD CollegeEmail NVARCHAR(255) NULL;


UPDATE [Affiliation_College_Master]
SET CollegeEmail = 'den.td038@gmail.com'
where CollegeCode = 'd038';
--------------------------------

ALTER TABLE DentalChairs
ADD AffiliationTypeId INT NULL;

ALTER TABLE DentalChairs
ADD CONSTRAINT FK_DentalChairs_TypeOfAffiliation
FOREIGN KEY (AffiliationTypeId)
REFERENCES TypeOfAffiliation(TypeId);

UPDATE DentalChairs
SET AffiliationTypeId = 2
WHERE AffiliationTypeId IS NULL;

--------------------------------

--------------------------------

ALTER TABLE [dbo].[DentalCollegeLandBuildingDetail]
ADD AffiliationTypeId INT NULL;

ALTER TABLE [dbo].[DentalCollegeLandBuildingDetail]
ADD CONSTRAINT FK_DentalCollegeLandBuildingDetail_TypeOfAffiliation
FOREIGN KEY (AffiliationTypeId)
REFERENCES TypeOfAffiliation(TypeId);

UPDATE DentalCollegeLandBuildingDetail
SET AffiliationTypeId = 2
WHERE AffiliationTypeId IS NULL;

--------------------------------



--------------------------------

ALTER TABLE [dbo].[Medical_SkillsLaboratory]
ADD AffiliationTypeId INT NULL;

ALTER TABLE [Medical_SkillsLaboratory]
ADD CONSTRAINT FK_Medical_SkillsLaboratory_TypeOfAffiliation
FOREIGN KEY (AffiliationTypeId)
REFERENCES TypeOfAffiliation(TypeId);

UPDATE Medical_SkillsLaboratory
SET AffiliationTypeId = 2
WHERE AffiliationTypeId IS NULL;

--------------------------------

---------------------------------

ALTER TABLE Medical_UGBedDistribution
ADD AffiliationTypeId INT NULL;

ALTER TABLE Medical_UGBedDistribution
ADD CONSTRAINT FK_Medical_UGBedDistribution_TypeOfAffiliation
FOREIGN KEY (AffiliationTypeId)
REFERENCES TypeOfAffiliation(TypeId);

UPDATE Medical_UGBedDistribution
SET AffiliationTypeId = 2
WHERE AffiliationTypeId IS NULL;

-----------------------------------------

ALTER TABLE DentalInfrastructure
ADD CourseLevel VARCHAR(10) NULL;

UPDATE DentalInfrastructure
SET CourseLevel = 'UG';


ALTER TABLE DentalInfrastructure
ADD CONSTRAINT UQ_DentalInfrastructure
UNIQUE
(
    CollegeCode,
    FacultyCode,
    AffiliationTypeId,
    CourseLevel,
    RequirementId,
    SeatSlab
);

---------
-- ============================================
-- Insert Faculty Users for DENTAL (Faculty = 2)
-- ============================================
-- Password: "Dental@2026" for all users
-- PasswordHash: Use BCrypt to generate actual hash

BEGIN TRANSACTION;

----- INSERT QUERY FOR COMMITTEE NAMES FOR DENTAL FACULTY ----------

INSERT INTO CA_MST_Med_CommitteeNames
(
    CommitteeName,
    FacultyCode,
    SubFacultyCode,
    CourseLevel
)
VALUES
('Academic council details',        2, NULL, 'ALL'),
('Anti-ragging committee',          2, NULL, 'ALL'),
('Gender harassment committee',     2, NULL, 'ALL'),
('Institutional ethical committee', 2, NULL, 'ALL'),
('POSH Committee',                  2, NULL, 'ALL'),
('Pharmacovigilance Committee',     2, NULL, 'UG'),
('Curriculum Committee',            2, NULL, 'UG'),
('IACE (Institutional Animal Ethics Committee)', 2, NULL, 'ALL');

-- 1. Data Entry Operator (DEO)
INSERT INTO TblRguhsFacultyUser 
    (UserId, UserName, Password, PasswordHash, IsActive, Faculty, IsFinance, 
     FinanceDesignation, DesignationDescription, IsSection, IsAdmin, 
     FailedLoginAttempts, LockoutEndTime)
VALUES 
    (1, 'DENTAL_DEO', 'DentalDEO@2026', '$2y$10$ay08DD3FqFHUyZqw/SE6dOorSppfLzTMZdd4je1kksHf6HxWuKYfW', 
     1, 2, 0, NULL, 'Data Entry Operator', 0, 0, 0, NULL);

-- 2. Junior Assistant (JR)
INSERT INTO TblRguhsFacultyUser 
    (UserId, UserName, Password, PasswordHash, IsActive, Faculty, IsFinance, 
     FinanceDesignation, DesignationDescription, IsSection, IsAdmin, 
     FailedLoginAttempts, LockoutEndTime)
VALUES 
    (2, 'DENTAL_JR', 'DentalJR@2026', '$2y$10$Vb/o0SOyv.X2dWmFZKI7kus/AqBtOOnIfxf5BHCfntsA3NQCvc/.K', 
     1, 2, 0, NULL, 'Junior Assistant', 0, 0, 0, NULL);

-- 3. Section Officer (SO)
INSERT INTO TblRguhsFacultyUser 
    (UserId, UserName, Password, PasswordHash, IsActive, Faculty, IsFinance, 
     FinanceDesignation, DesignationDescription, IsSection, IsAdmin, 
     FailedLoginAttempts, LockoutEndTime)
VALUES 
    (3, 'DENTAL_SO', 'DentalSO@2026', '$2y$10$68SypNpAYeJveDDtR9A.ZefTlq4vp4Nm6Smbh/vfGwG1MV0klIU5y', 
     1, 2, 0, NULL, 'Section Officer', 1, 0, 0, NULL);

-- 4. Assistant Registrar (AR)
INSERT INTO TblRguhsFacultyUser 
    (UserId, UserName, Password, PasswordHash, IsActive, Faculty, IsFinance, 
     FinanceDesignation, DesignationDescription, IsSection, IsAdmin, 
     FailedLoginAttempts, LockoutEndTime)
VALUES 
    (4, 'DENTAL_AR', 'DentalAR@2026', '$2y$10$EM4KacCJx4VFEgD.88.hO.W0cL15tsr.obOEkwYKjtu7.SmzVJLEu', 
     1, 2, 0, NULL, 'Assistant Registrar', 0, 0, 0, NULL);

-- 5. Registrar (RG)
INSERT INTO TblRguhsFacultyUser 
    (UserId, UserName, Password, PasswordHash, IsActive, Faculty, IsFinance, 
     FinanceDesignation, DesignationDescription, IsSection, IsAdmin, 
     FailedLoginAttempts, LockoutEndTime)
VALUES 
    (5, 'DENTAL_RG', 'DentalRG@2026', '$2y$10$Dvyp7iYXiy3hHfyzrghTtOpq5t9sEL/utd6gSqT7AWuAmqSCDq6oy', 
     1, 2, 0, NULL, 'Registrar', 0, 0, 0, NULL);

-- 6. Registrar Evaluation (RE)
INSERT INTO TblRguhsFacultyUser 
    (UserId, UserName, Password, PasswordHash, IsActive, Faculty, IsFinance, 
     FinanceDesignation, DesignationDescription, IsSection, IsAdmin, 
     FailedLoginAttempts, LockoutEndTime)
VALUES 
    (6, 'DENTAL_RE', 'DentalRE@2026', '$2y$10$2QhIaIi/BH6RM.RUmKGC4uebNLQKVN42pWSDlVGjCREz2G7qjiIMW', 
     1, 2, 0, NULL, 'Registrar Evaluation', 0, 0, 0, NULL);

-- 7. Director (DR)
INSERT INTO TblRguhsFacultyUser 
    (UserId, UserName, Password, PasswordHash, IsActive, Faculty, IsFinance, 
     FinanceDesignation, DesignationDescription, IsSection, IsAdmin, 
     FailedLoginAttempts, LockoutEndTime)
VALUES 
    (7, 'DENTAL_DR', 'DentalDR@2026', '$2y$10$QH2iUDvmoniP7TRBry4GluosxVHrewtD4bH255zf1N6IbjS9cj1xK', 
     1, 2, 0, NULL, 'Director', 0, 1, 0, NULL);

-- 8. Vice Chancellor (VC)
INSERT INTO TblRguhsFacultyUser 
    (UserId, UserName, Password, PasswordHash, IsActive, Faculty, IsFinance, 
     FinanceDesignation, DesignationDescription, IsSection, IsAdmin, 
     FailedLoginAttempts, LockoutEndTime)
VALUES 
    (8, 'DENTAL_VC', 'DentalVC@2026', '$2y$10$r2LCi8ut3oHNUYLF7XfDcuJtN1ANhhfCp740xOlhKqDmoZzJLoqVe', 
     1, 2, 0, NULL, 'Vice Chancellor', 0, 1, 0, NULL);



-- ============================================
-- Insert ADMIN User
-- ============================================
-- This admin user has NO faculty assignment
-- and has full system access (IsAdmin = 1)

INSERT INTO TblRguhsFacultyUser 
    (UserId, UserName, Password, PasswordHash, IsActive, Faculty, IsFinance, 
     FinanceDesignation, DesignationDescription, IsSection, IsAdmin, 
     FailedLoginAttempts, LockoutEndTime)
VALUES 
    (0, 'ADMIN', 'Admin@2026', '$2y$10$l6rdWrtRY4G654O3TPIRROQ9M6Y.NsaMbszy1axC8iMWhkkGiBPUe', 
     1, NULL, 0, NULL, 'System Administrator', 1, 1, 0, NULL);


COMMIT;

-- ============================================
-- Verify Inserted Data
-- ============================================
SELECT 
    Id, UserId, UserName, IsActive, Faculty, 
    DesignationDescription, IsSection, IsAdmin
FROM TblRguhsFacultyUser 
WHERE Faculty = 2 
ORDER BY UserId;

SELECT * FROM [dbo].[TblRguhsFacultyUser]
  where Faculty=2


------------------------------------------------

-- ============================================
-- Create College Mapping Table
-- ============================================
USE [Admission_Affiliation];
GO

CREATE TABLE [dbo].[Tbl_CollegeMapping](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [UserName] [varchar](50) NOT NULL,
    [FacultyCode] int NOT NULL,
    [CollegeFrom] [varchar](5) NOT NULL,   -- Starting letter (e.g., 'A')
    [CollegeTo] [varchar](5) NOT NULL,     -- Ending letter (e.g., 'M')
    [CreatedDate] [datetime] NULL,
    [CreatedBy] [varchar](50) NULL,
    [IsActive] [bit] NULL,
    CONSTRAINT [PK_Tbl_CollegeMapping] PRIMARY KEY CLUSTERED ([Id] ASC)
) ON [PRIMARY];
GO

-- Add foreign key relationship
ALTER TABLE [dbo].[Tbl_CollegeMapping] WITH CHECK 
ADD CONSTRAINT [FK_Tbl_CollegeMapping_Faculty] 
FOREIGN KEY([FacultyCode])
REFERENCES [dbo].[Faculty] ([FacultyId]);
GO

-- Add index for faster lookups
CREATE NONCLUSTERED INDEX [IX_Tbl_CollegeMapping_UserId] 
ON [dbo].[Tbl_CollegeMapping] ([UserId] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Tbl_CollegeMapping_FacultyCode] 
ON [dbo].[Tbl_CollegeMapping] ([FacultyCode] ASC);
GO

-- Verify table created
SELECT * FROM [dbo].[Tbl_CollegeMapping];

ALTER TABLE Tbl_CollegeMapping
ADD FromLetter NVARCHAR(1) NOT NULL DEFAULT('A'),
    ToLetter NVARCHAR(1) NOT NULL DEFAULT('Z');

-- Update existing NULL values first
UPDATE Tbl_CollegeMapping
SET IsActive = 1
WHERE IsActive IS NULL;

-- Make the column NOT NULL
ALTER TABLE Tbl_CollegeMapping
ALTER COLUMN IsActive BIT NOT NULL;

ALTER TABLE Tbl_CollegeMapping
ADD CONSTRAINT DF_TblCollegeMapping_IsActive
DEFAULT (1) FOR IsActive;


SELECT * FROM Affiliation_College_Master
WHERE FacultyCode=1 order by CollegeName desc;

SELECT * FROM CollegeCourseIntakeDetails
WHERE FacultyCode = 2

SELECT * FROM AcademicIntake
WHERE FacultyCode = 2 and CollegeCode = 'd038';

select * from CA_Med_LibraryEquipments
where FacultyCode = 2

select * from [dbo].[AFF_InstitutionsDetails]
where facultycode = 2 and CollegeCode='d038'

ALTER TABLE AFF_InstitutionsDetails
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG (Registrar)
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE (Registrar Evaluation / Relevant Role)
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR (Director)
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC (Vice Chancellor)
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL;


ALTER TABLE InstitutionBasicDetails
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;




ALTER TABLE CONTINUATION_TrustMemberDetails
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;


ALTER TABLE [Aff_DeanOrDirectorDetails]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;


ALTER TABLE Aff_PrincipalDetails
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;


ALTER TABLE [dbo].[Affiliation_CourseDetails]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;


ALTER TABLE [dbo].[Affiliation_PgSsCourseDetails]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;


ALTER TABLE [dbo].[DentalInfrastructure]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;


ALTER TABLE [dbo].[DentalCollegeLandBuildingDetail]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;


ALTER TABLE [dbo].[DentalChairs]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;



ALTER TABLE [dbo].[Medical_UGBedDistribution]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;



ALTER TABLE [dbo].[AFF_HostelDetails]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;



ALTER TABLE [dbo].[Medical_DepartmentOfficesMeu]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;



ALTER TABLE [dbo].[DentalCollegeEquipmentDetails]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE [dbo].[CA_VehicleDetails]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;


ALTER TABLE [dbo].[CA_AcademicPerformance]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE [dbo].[Med_CA_AccountAndFeeDetails]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE [dbo].[Med_CA_StaffParticulars]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE [dbo].[CA_Med_StaffParticularsOther]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE [dbo].[CA_MedicalLibraryServices]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;



ALTER TABLE [dbo].[CA_MedicalDepartmentLibrary]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE CA_DentalLibraryRecords
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE CA_Med_ResearchPublicationsDetails
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE DeptWisePublications
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE CA_Med_LibraryGeneral
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE CA_Med_LibraryItems
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE CA_Med_LibraryBuilding
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE CA_Med_LibTechnicalProcess
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE CA_Med_LibraryEquipments
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE CA_Med_LibraryFinance
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE [dbo].[FacultyDetails]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE [dbo].[TeachingStaffDepartmentWiseDetails]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE [dbo].[HospitalDetailsForAffiliation]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;


ALTER TABLE [dbo].[DentalServices]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;

ALTER TABLE [dbo].[DentalWardBedDistribution]
ADD
    -- DEO
    IsDeoVerified BIT NULL,
    DeoRemarks NVARCHAR(1000) NULL,
    DeoVerifiedDate DATETIME NULL,
    DeoName NVARCHAR(200) NULL,

    -- JR
    IsJrVerified BIT NULL,
    JrRemarks NVARCHAR(1000) NULL,
    JrVerifiedDate DATETIME NULL,
    JrName NVARCHAR(200) NULL,

    -- SO
    IsSoVerified BIT NULL,
    SoRemarks NVARCHAR(1000) NULL,
    SoVerifiedDate DATETIME NULL,
    SoName NVARCHAR(200) NULL,

    -- AR
    IsArVerified BIT NULL,
    ArRemarks NVARCHAR(1000) NULL,
    ArVerifiedDate DATETIME NULL,
    ArName NVARCHAR(200) NULL,

    -- RG
    IsRgVerified BIT NULL,
    RgRemarks NVARCHAR(1000) NULL,
    RgVerifiedDate DATETIME NULL,
    RgName NVARCHAR(200) NULL,

    -- RE
    IsReVerified BIT NULL,
    ReRemarks NVARCHAR(1000) NULL,
    ReVerifiedDate DATETIME NULL,
    ReName NVARCHAR(200) NULL,

    -- DR
    IsDrVerified BIT NULL,
    DrRemarks NVARCHAR(1000) NULL,
    DrVerifiedDate DATETIME NULL,
    DrName NVARCHAR(200) NULL,

    -- VC
    IsVcVerified BIT NULL,
    VcRemarks NVARCHAR(1000) NULL,
    VcVerifiedDate DATETIME NULL,
    VcName NVARCHAR(200) NULL,

    -- Overall Workflow
    CurrentVerificationLevel NVARCHAR(50) NULL,
    OverallStatus NVARCHAR(30) NULL,
    LastUpdatedDate DATETIME NULL;


select * from TblRguhsFacultyUser;

SELECT * FROM [dbo].[AFF_InstitutionsDetails] WHERE CollegeCode='D008';

SELECT * FROM Affiliation_College_Master
WHERE CollegeCode = 'D008';

SELECT * FROM CollegeCourseIntakeDetails
WHERE FacultyCode=2;

SELECT * FROM AcademicIntake
WHERE FacultyCode=2 and CollegeCode = 'D008';

SELECT * FROM InstitutionBasicDetails
WHERE CollegeCode = 'DO38';


select * from Aff_DeanOrDirectorDetails
where CollegeCode = 'd038';

SELECT * FROM Aff_PrincipalDetails
WHERE CollegeCode = 'D038';

select * from [dbo].[Affiliation_CourseDetails]
where facultycode = 2 and Collegecode = 'd038';

SELECT * FROM [dbo].[CA_Progress]
WHERE CollegeCode = 'D007' and courselevel like '%pG%'
and StepKey like '%veh%' 
SELECT * FROM [dbo].[CA_Med_LibraryEquipments]
WHERE FacultyCode = 2 AND CollegeCode = 'D008'

SELECT * FROM CA_MST_Med_LibraryEquipments;

SELECT * FROM [dbo].[CA_MST_Med_CommitteeNames]

SELECT * FROM CA_ExaminationScheme

SELECT * FROM HospitalDetailsForAffiliation
where FacultyCode= 2 and CollegeCode = 'd037';


SELECT* FROM DentalWardBedDistribution
WHERE CollegeCode = 'D038'

select * from Medical_UGBedDistribution
where collegecode = 'd038'

SELECT * FROM MstDentalBedDistribution
where SeatSlab = 50;

SELECT * FROM DentalInfrastructure;

select * from MstDentalInfrastructure;

SELECT * FROM Medical_DepartmentOfficesMeu
WHERE FacultyCode=2 AND CollegeCode='D038';


SELECT * FROM DesignationMaster
WHERE FacultyCode = 2

SELECT * FROM [dbo].[FacultyDetails]
where FacultyCode = 2 and CollegeCode='d038'

SELECT * FROM DepartmentMaster
WHERE FacultyCode=2;

SELECT * FROM [Med_CA_AccountAndFeeDetails]
WHERE FacultyCode= 2 and CollegeCode='d038'

----part 1

SELECT * FROM CA_MedicalDepartmentLibrary
where FacultyCode=2 and CollegeCode='d038'

select * from DepartmentMaster
where FacultyCode = 2

----- part 2

SELECT * FROM CA_MST_DentalLibraryRecords;

SELECT * FROM CA_DentalLibraryRecords
WHERE CollegeCode = 'D038';

----- RESEARCH AND PUBLICATIONS -------

--- PART 1
SELECT * FROM CA_Med_ResearchPublicationsDetails
WHERE CollegeCode='D038';

SELECT * FROM DeptWisePublications
WHERE CollegeCode = 'D038'

-- LIBRARY DETAILS 
-- PART 1

SELECT * FROM CA_Med_LibraryGeneral
WHERE CollegeCode = 'D038';

SELECT * FROM CA_Med_LibraryItems
WHERE CollegeCode = 'D038'

SELECT * FROM CA_Med_LibraryBuilding
WHERE CollegeCode = 'D038'

SELECT * FROM CA_MST_Med_LibTechnicalProcess
WHERE FacultyCode=2;

SELECT * FROM CA_Med_LibTechnicalProcess
WHERE CollegeCode = 'D038'

SELECT * FROM CA_MST_Med_LibraryEquipments
WHERE FacultyCode=2

SELECT * FROM CA_Med_LibraryEquipments
WHERE CollegeCode = 'D038';

SELECT * FROM CA_Med_LibraryFinance
WHERE CollegeCode = 'D038';

SELECT * FROM TeachingStaffDepartmentWiseDetails
WHERE CollegeCode = 'd038';


SELECT * FROM MedicalAlliedDisciplineDetail
WHERE CollegeCode='D038' ORDER BY DisciplineName;

SELECT * FROM DentalServices
WHERE CollegeCode = 'd038' and SectionCode = 1;

SELECT * FROM MstDentalServices;

SELECT * FROM DentalWardBedDistribution
WHERE CollegeCode = 'D038'


/* ============================================================
   1. MstTabs
   ============================================================ */

CREATE TABLE dbo.MstTabs
(
    TabId INT IDENTITY(1,1) NOT NULL,
    FacultyId INT NOT NULL,
    TabName NVARCHAR(200) NOT NULL,

    CONSTRAINT PK_MstTabs
        PRIMARY KEY (TabId),

    CONSTRAINT FK_MstTabs_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES dbo.Faculty(FacultyId)
);
GO

INSERT INTO dbo.MstTabs (FacultyId, TabName)
VALUES
    (2, N'Institution Details'),
    (2, N'Trust Details'),
    (2, N'Trust Member Details'),
    (2, N'Dean / Director Details'),
    (2, N'Principal Details'),
    (2, N'UG Course Details'),
    (2, N'PG Course Details'),
    (2, N'Courses & Intake'),
    (2, N'Land & Building Details'),
    (2, N'Classroom & Laboratory'),
    (2, N'Chair Distribution'),
    (2, N'Bed Distribution'),
    (2, N'Hostel Details'),
    (2, N'Department Offices And Educational Unit'),
    (2, N'Equipment List'),
    (2, N'Vehicle Details'),
    (2, N'UG Academic Matters'),
    (2, N'PG Academic Matters'),
    (2, N'Staff Pay Scale'),
    (2, N'Staff Other Details'),
    (2, N'Finance Details'),
    (2, N'Library Services'),
    (2, N'Research & Publications'),
    (2, N'Library Details'),
    (2, N'Faculty Details'),
    (2, N'Teaching Experience'),
    (2, N'Clinical Facilities');
GO


/* ============================================================
   2. MstSections
   ============================================================ */

CREATE TABLE dbo.MstSections
(
    SectionId INT IDENTITY(1,1) NOT NULL,
    FacultyId INT NOT NULL,
    TabId INT NOT NULL,
    SectionName NVARCHAR(200) NOT NULL,

    CONSTRAINT PK_MstSections
        PRIMARY KEY (SectionId),

    CONSTRAINT FK_MstSections_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES dbo.Faculty(FacultyId),

    CONSTRAINT FK_MstSections_MstTabs
        FOREIGN KEY (TabId)
        REFERENCES dbo.MstTabs(TabId)
);
GO



/* ============================================================
   Institution Details
   ============================================================ */
INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Basic Information'
FROM dbo.MstTabs
WHERE TabName = N'Institution Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Address Details'
FROM dbo.MstTabs
WHERE TabName = N'Institution Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Contact Information'
FROM dbo.MstTabs
WHERE TabName = N'Institution Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Institutional Details'
FROM dbo.MstTabs
WHERE TabName = N'Institution Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Head of Institution'
FROM dbo.MstTabs
WHERE TabName = N'Institution Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Nodal Officer Details'
FROM dbo.MstTabs
WHERE TabName = N'Institution Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Principal Details'
FROM dbo.MstTabs
WHERE TabName = N'Institution Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Dean / Director Details'
FROM dbo.MstTabs
WHERE TabName = N'Institution Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Trust Details'
FROM dbo.MstTabs
WHERE TabName = N'Institution Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Trust Information'
FROM dbo.MstTabs
WHERE TabName = N'Trust Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Trust Members'
FROM dbo.MstTabs
WHERE TabName = N'Trust Member Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Dean / Director Information'
FROM dbo.MstTabs
WHERE TabName = N'Dean / Director Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Teaching Experience'
FROM dbo.MstTabs
WHERE TabName = N'Dean / Director Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Administrative Experience'
FROM dbo.MstTabs
WHERE TabName = N'Dean / Director Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Principal Information'
FROM dbo.MstTabs
WHERE TabName = N'Principal Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Teaching Experience'
FROM dbo.MstTabs
WHERE TabName = N'Principal Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Administrative Experience'
FROM dbo.MstTabs
WHERE TabName = N'Principal Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Present Intake'
FROM dbo.MstTabs
WHERE TabName = N'UG Course Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Previous Details'
FROM dbo.MstTabs
WHERE TabName = N'UG Course Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Particulars of Permission'
FROM dbo.MstTabs
WHERE TabName = N'UG Course Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Year of Obtaining EC & FC from Government of Karnataka'
FROM dbo.MstTabs
WHERE TabName = N'UG Course Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Year of Last Affiliation Granted by RGUHS'
FROM dbo.MstTabs
WHERE TabName = N'UG Course Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Previous Inspection by LIC'
FROM dbo.MstTabs
WHERE TabName = N'UG Course Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'PG Course Details'
FROM dbo.MstTabs
WHERE TabName = N'PG Course Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Land Details'
FROM dbo.MstTabs
WHERE TabName = N'Land & Building Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Building Details'
FROM dbo.MstTabs
WHERE TabName = N'Land & Building Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Facility Requirements'
FROM dbo.MstTabs
WHERE TabName = N'Classroom & Laboratory';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Chair Distribution Details'
FROM dbo.MstTabs
WHERE TabName = N'Chair Distribution';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Beds Distribution'
FROM dbo.MstTabs
WHERE TabName = N'Bed Distribution';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Hostel Information'
FROM dbo.MstTabs
WHERE TabName = N'Hostel Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Student Accommodation'
FROM dbo.MstTabs
WHERE TabName = N'Hostel Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Common Room Facilities'
FROM dbo.MstTabs
WHERE TabName = N'Hostel Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Hostel Facilities'
FROM dbo.MstTabs
WHERE TabName = N'Hostel Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Department Office Infrastructure'
FROM dbo.MstTabs
WHERE TabName = N'Department Offices And Educational Unit';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Dental Education Unit'
FROM dbo.MstTabs
WHERE TabName = N'Department Offices And Educational Unit';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Equipment Details'
FROM dbo.MstTabs
WHERE TabName = N'Equipment List';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Vehicle Information'
FROM dbo.MstTabs
WHERE TabName = N'Vehicle Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'UG Academic Performance'
FROM dbo.MstTabs
WHERE TabName = N'UG Academic Matters';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'PG Academic Performance'
FROM dbo.MstTabs
WHERE TabName = N'PG Academic Matters';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Staff Pay Scale'
FROM dbo.MstTabs
WHERE TabName = N'Staff Pay Scale';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Staff Other Details'
FROM dbo.MstTabs
WHERE TabName = N'Staff Other Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Authority & Account Details - UG'
FROM dbo.MstTabs
WHERE TabName = N'Finance Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Annual Financial Details - UG'
FROM dbo.MstTabs
WHERE TabName = N'Finance Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Account & Audit Details - UG'
FROM dbo.MstTabs
WHERE TabName = N'Finance Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Authority & Account Details - PG'
FROM dbo.MstTabs
WHERE TabName = N'Finance Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Annual Financial Details - PG'
FROM dbo.MstTabs
WHERE TabName = N'Finance Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Account & Audit Details - PG'
FROM dbo.MstTabs
WHERE TabName = N'Finance Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Department Library Details'
FROM dbo.MstTabs
WHERE TabName = N'Library Services';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Research & Publication Details'
FROM dbo.MstTabs
WHERE TabName = N'Research & Publications';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'General Library Details'
FROM dbo.MstTabs
WHERE TabName = N'Library Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Library Items'
FROM dbo.MstTabs
WHERE TabName = N'Library Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Library Building'
FROM dbo.MstTabs
WHERE TabName = N'Library Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Technical Processes'
FROM dbo.MstTabs
WHERE TabName = N'Library Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Library Equipment'
FROM dbo.MstTabs
WHERE TabName = N'Library Details';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Library Finance'
FROM dbo.MstTabs
WHERE TabName = N'Library Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Faculty Details'
FROM dbo.MstTabs
WHERE TabName = N'Faculty Details';


INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Clinical Hospital Details'
FROM dbo.MstTabs
WHERE TabName = N'Clinical Facilities';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Clinical Statistics'
FROM dbo.MstTabs
WHERE TabName = N'Clinical Facilities';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Discipline Details'
FROM dbo.MstTabs
WHERE TabName = N'Clinical Facilities';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Nursing, Paramedical, Technical & Allied Services'
FROM dbo.MstTabs
WHERE TabName = N'Clinical Facilities';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Engineering & Allied Services'
FROM dbo.MstTabs
WHERE TabName = N'Clinical Facilities';

INSERT INTO dbo.MstSections (FacultyId, TabId, SectionName)
SELECT FacultyId, TabId, N'Ward-wise Bed Distribution in Attached Hospital'
FROM dbo.MstTabs
WHERE TabName = N'Clinical Facilities';

/* ============================================================
   3. SectionWiseFeedback
   ============================================================ */

CREATE TABLE dbo.SectionWiseFeedback
(
    SectionWiseFeedbackId INT IDENTITY(1,1) NOT NULL,

    FacultyId INT NOT NULL,
    CollegeCode NVARCHAR(100) NOT NULL,
    TabId INT NOT NULL,
    SectionId INT NOT NULL,
    VerificationStatus NVARCHAR(50) NULL,
    Remarks NVARCHAR(max) NULL,

    VerifiedBy NVARCHAR(200) NULL,
    VerifiedOn DATETIME2 NULL,

    CONSTRAINT PK_SectionWiseFeedback
        PRIMARY KEY (SectionWiseFeedbackId),

    CONSTRAINT FK_SectionWiseFeedback_Faculty
        FOREIGN KEY (FacultyId)
        REFERENCES dbo.Faculty(FacultyId),

    CONSTRAINT FK_SectionWiseFeedback_College
        FOREIGN KEY (CollegeCode)
        REFERENCES [dbo].[Affiliation_College_Master](CollegeCode),

    CONSTRAINT FK_SectionWiseFeedback_Tab
        FOREIGN KEY (TabId)
        REFERENCES dbo.MstTabs(TabId),

    CONSTRAINT FK_SectionWiseFeedback_Section
        FOREIGN KEY (SectionId)
        REFERENCES dbo.MstSections(SectionId)
);
GO

/* ============================================================
   4. Prevent duplicate section verification
   ============================================================ */

CREATE UNIQUE INDEX UX_SectionWiseFeedback_College_Faculty_Tab_Section
ON dbo.SectionWiseFeedback
(
    CollegeCode,
    FacultyId,
    TabId,
    SectionId
);
GO


/* ============================================================
   5. Useful indexes
   ============================================================ */

CREATE INDEX IX_MstTabs_FacultyId
ON dbo.MstTabs(FacultyId);
GO

CREATE INDEX IX_MstSections_FacultyId
ON dbo.MstSections(FacultyId);
GO

CREATE INDEX IX_MstSections_TabId
ON dbo.MstSections(TabId);
GO

CREATE INDEX IX_SectionWiseFeedback_FacultyId
ON dbo.SectionWiseFeedback(FacultyId);
GO

CREATE INDEX IX_SectionWiseFeedback_CollegeCode
ON dbo.SectionWiseFeedback(CollegeCode);
GO

CREATE INDEX IX_SectionWiseFeedback_TabId
ON dbo.SectionWiseFeedback(TabId);
GO

CREATE INDEX IX_SectionWiseFeedback_SectionId
ON dbo.SectionWiseFeedback(SectionId);
GO