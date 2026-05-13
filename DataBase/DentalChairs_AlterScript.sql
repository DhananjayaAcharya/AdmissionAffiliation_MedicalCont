

CREATE TABLE DentalChairs
(
    Id INT PRIMARY KEY IDENTITY(1,1),

    FacultyCode INT NOT NULL,
    CollegeCode NVARCHAR(100) NOT NULL,
    CourseCode INT NOT NULL,

    CourseLevel VARCHAR(10),
    CourseName VARCHAR(100),

    ChairsRequired INT,
    ChairsExisting INT,
    SeatSlab INT NOT NULL,
    SeatSlabId VARCHAR(10) NOT NULL,

    FOREIGN KEY (FacultyCode) 
        REFERENCES Faculty(FacultyId),

    FOREIGN KEY (CollegeCode) 
        REFERENCES [dbo].[Affiliation_College_Master](CollegeCode)
);

------------------ALTER SCRIPT-------------
ALTER TABLE AcademicIntake
ADD
    AY2025_DCIDocument VARBINARY(MAX) NULL,
    AY2025_KSDCDocument VARBINARY(MAX) NULL,

    AY2026_DCIDocument VARBINARY(MAX) NULL,
    AY2026_KSDCDocument VARBINARY(MAX) NULL,

    AY2027_DCIDocument VARBINARY(MAX) NULL,
    AY2027_KSDCDocument VARBINARY(MAX) NULL,

    AY2027_ExistingIntake INT NOT NULL DEFAULT 0,
    AY2027_AddRequestedIntake INT NOT NULL DEFAULT 0,
    AY2027_TotalIntake INT NOT NULL DEFAULT 0;

INSERT INTO [dbo].[SeatSlabMaster]
(FacultyCode, SeatSlabId, SeatSlab)
values
(2, 'S01',50),
(2, 'S02',100),
(2, 'S03',150);

---------------------------


CREATE TABLE UG_SeatSlabNormMaster
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    FacultyCode INT NOT NULL,

    SeatSlab INT NOT NULL,

    LandTier2Acres DECIMAL(5,2) NOT NULL,

    LandOtherAreaAcres DECIMAL(5,2) NOT NULL,

    CollegeBuiltupAreaSqm INT NOT NULL,

    LectureHallCount INT NOT NULL,

    LectureHallAreaSqm INT NOT NULL,

    LectureHallCapacity INT NOT NULL,

    ExaminationHallAreaSqm INT NOT NULL,

    LibraryAreaSqm INT NOT NULL,

    DentalHospitalAreaSqm INT NOT NULL,

    CreatedOn DATETIME DEFAULT GETDATE()
);


--- INSERT SCRIPT FOR LAND AND BUILDING-----

INSERT INTO UG_SeatSlabNormMaster
(
    FacultyCode,
    SeatSlab,
    LandTier2Acres,
    LandOtherAreaAcres,
    CollegeBuiltupAreaSqm,
    LectureHallCount,
    LectureHallAreaSqm,
    LectureHallCapacity,
    ExaminationHallAreaSqm,
    LibraryAreaSqm,
    DentalHospitalAreaSqm
)
VALUES
(
    2,
    50,
    2,
    3,
    6000,
    2,
    150,
    100,
    200,
    300,
    3000
),
(
    2,
    100,
    3,
    5,
    8500,
    4,
    150,
    120,
    250,
    400,
    5000
),

(
    2,
    150,
    5,
    7,
    10500,
    4,
    220,
    180,
    400,
    500,
    6500
);

--------------------------------------

CREATE TABLE DentalCollegeLandBuildingDetail
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    CollegeCode NVARCHAR(100) NOT NULL,

    FacultyCode INT NOT NULL,

    SeatSlab INT NOT NULL,

    SeatIntake INT NOT NULL,

    IsTier2OrHilly BIT NOT NULL DEFAULT 0,

    -- LAND DETAILS
    TotalLandAreaAcres DECIMAL(10,2) NULL,

    LandOwnershipType NVARCHAR(50) NULL,

    HasFutureExpansionSpace BIT NULL,

    -- LAND DOCUMENTS
    SaleDeedDocumentPath VARCHAR(255) NULL,

    EncumbranceCertificateDocumentPath VARCHAR(255) NULL,

    LandUseCertificateDocumentPath VARCHAR(255) NULL,

    ApprovedLayoutPlanDocumentPath VARCHAR(255) NULL,

    LandSketchDocumentPath VARCHAR(255) NULL,

    DistanceCertificateDocumentPath VARCHAR(255) NULL,

    -- BUILDING DOCUMENTS
    ApprovedBuildingPlanDocumentPath VARCHAR(255) NULL,

    CompletionCertificateDocumentPath VARCHAR(255) NULL,

    StructuralStabilityCertificateDocumentPath VARCHAR(255) NULL,

    FireSafetyNocDocumentPath VARCHAR(255) NULL,

    LiftLicenseDocumentPath VARCHAR(255) NULL,

    ElectricalSafetyCertificateDocumentPath VARCHAR(255) NULL,

    WaterSupplyCertificateDocumentPath VARCHAR(255) NULL,

    SewageSanitationApprovalDocumentPath VARCHAR(255) NULL,

    -- INFRASTRUCTURE DETAILS
    TotalBuiltupAreaSqm DECIMAL(10,2) NULL,

    LectureHallCount INT NULL,

    LectureHallAreaSqm DECIMAL(10,2) NULL,

    LectureHallSeatingCapacity INT NULL,

    ExaminationHallAreaSqm DECIMAL(10,2) NULL,

    LibraryAreaSqm DECIMAL(10,2) NULL,
    HospitalAreaSqm DECIMAL(10,2) NULL,

    MuseumDemoRoomsAreaSqm DECIMAL(10,2) NULL,

    DepartmentWiseAreaSqm DECIMAL(10,2) NULL,

    PreclinicalSkillLabAreaSqm DECIMAL(10,2) NULL,
    LandCategory VARCHAR(50) NULL,
    IsLandInTwoPieces BIT NULL,
    DistanceBetweenCollegeAndHospitalKm DECIMAL(10,2) NULL

    Remarks NVARCHAR(MAX) NULL,

    CreatedOn DATETIME DEFAULT GETDATE(),

    ModifiedOn DATETIME NULL,

    CONSTRAINT FK_DentalCollegeLandBuildingDetail_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_DentalCollegeLandBuildingDetail_College
        FOREIGN KEY (CollegeCode)
        REFERENCES affiliation_college_master(CollegeCode)
);

ALTER TABLE DentalCollegeLandBuildingDetail
ADD CONSTRAINT UQ_DentalCollegeLandBuildingDetail
UNIQUE (CollegeCode, FacultyCode);

ALTER TABLE DentalCollegeLandBuildingDetail
ADD
    LandCategory VARCHAR(50) NULL,

    IsLandInTwoPieces BIT NULL,

    DistanceBetweenCollegeAndHospitalKm DECIMAL(10,2) NULL;


--ALTER TABLE DentalCollegeLandBuildingDetail
--ADD HospitalAreaSqm DECIMAL(10,2) NULL;

ALTER TABLE UG_SeatSlabNormMaster
ADD
    RequiresMuseumAndDemoRooms BIT NOT NULL DEFAULT 1,

    RequiresDepartmentalAreas BIT NOT NULL DEFAULT 1,

    RequiresPreclinicalSkillLabs BIT NOT NULL DEFAULT 1,

    RequiresFutureExpansion BIT NOT NULL DEFAULT 1,

    MaximumCollegeHospitalDistanceKm DECIMAL(5,2) NULL;

UPDATE UG_SeatSlabNormMaster
SET MaximumCollegeHospitalDistanceKm = 5;

--ALTER TABLE DentalCollegeLandBuildingDetail
--ADD LandCategory VARCHAR(100) NULL;

--ALTER TABLE DentalCollegeLandBuildingDetail
--DROP CONSTRAINT DF__DentalCol__IsTie__638EB5B2;

--ALTER TABLE DentalCollegeLandBuildingDetail
--DROP COLUMN IsTier2OrHilly;

------------ UPDATE DESIGNATION MASTER -------------

UPDATE [dbo].[DesignationMaster]
set [DesignationName] = 'Professor'
WHERE [FacultyCode]=2 AND DesignationCode = 'D010';


UPDATE [dbo].[DesignationMaster]
set [DesignationName] = 'Reader/Associate'
WHERE [FacultyCode]=2 AND DesignationCode = 'D011';


UPDATE [dbo].[DesignationMaster]
set [DesignationName] = 'Lecturer/Assistant Professor'
WHERE [FacultyCode]=2 AND DesignationCode = 'D012';

----------------------


INSERT INTO [dbo].[DepartmentWiseFacultyMaster]
(FacultyCode, DepartmentCode, DesignationCode, Seats, SeatSlabId)
VALUES

(2, 'DE001', 'D010',1, 'S01'),
(2, 'DE002', 'D010',1, 'S01'),
(2, 'DE003', 'D010',1, 'S01'),
(2, 'DE004', 'D010',1, 'S01'),
(2, 'DE005', 'D010',1, 'S01'),
(2, 'DE006', 'D010',1, 'S01'),
(2, 'DE007', 'D010',1, 'S01'),
(2, 'DE008', 'D010',1, 'S01'),
(2, 'DE009', 'D010',1, 'S01'),

(2, 'DE001', 'D011',1, 'S01'),
(2, 'DE002', 'D011',1, 'S01'),
(2, 'DE003', 'D011',1, 'S01'),
(2, 'DE004', 'D011',1, 'S01'),
(2, 'DE005', 'D011',1, 'S01'),
(2, 'DE006', 'D011',1, 'S01'),
(2, 'DE007', 'D011',1, 'S01'),
(2, 'DE008', 'D011',1, 'S01'),
(2, 'DE009', 'D011',1, 'S01'),

(2, 'DE001', 'D012',1, 'S01'),
(2, 'DE002', 'D012',1, 'S01'),
(2, 'DE003', 'D012',1, 'S01'),
(2, 'DE004', 'D012',1, 'S01'),
(2, 'DE005', 'D012',1, 'S01'),
(2, 'DE006', 'D012',1, 'S01'),
(2, 'DE007', 'D012',1, 'S01'),
(2, 'DE008', 'D012',1, 'S01'),
(2, 'DE009', 'D012',1, 'S01'),

(2, 'DE001', 'D010',1, 'S02'),
(2, 'DE002', 'D010',1, 'S02'),
(2, 'DE003', 'D010',1, 'S02'),
(2, 'DE004', 'D010',1, 'S02'),
(2, 'DE005', 'D010',1, 'S02'),
(2, 'DE006', 'D010',1, 'S02'),
(2, 'DE007', 'D010',1, 'S02'),
(2, 'DE008', 'D010',1, 'S02'),
(2, 'DE009', 'D010',1, 'S02'),

(2, 'DE001', 'D011',1, 'S02'),
(2, 'DE002', 'D011',1, 'S02'),
(2, 'DE003', 'D011',1, 'S02'),
(2, 'DE004', 'D011',1, 'S02'),
(2, 'DE005', 'D011',1, 'S02'),
(2, 'DE006', 'D011',1, 'S02'),
(2, 'DE007', 'D011',1, 'S02'),
(2, 'DE008', 'D011',1, 'S02'),
(2, 'DE009', 'D011',1, 'S02'),

(2, 'DE001', 'D012',1, 'S02'),
(2, 'DE002', 'D012',1, 'S02'),
(2, 'DE003', 'D012',1, 'S02'),
(2, 'DE004', 'D012',1, 'S02'),
(2, 'DE005', 'D012',1, 'S02'),
(2, 'DE006', 'D012',1, 'S02'),
(2, 'DE007', 'D012',1, 'S02'),
(2, 'DE008', 'D012',1, 'S02'),
(2, 'DE009', 'D012',1, 'S02'),

(2, 'DE001', 'D010',2, 'S03'),
(2, 'DE002', 'D010',2, 'S03'),
(2, 'DE003', 'D010',2, 'S03'),
(2, 'DE004', 'D010',2, 'S03'),
(2, 'DE005', 'D010',2, 'S03'),
(2, 'DE006', 'D010',2, 'S03'),
(2, 'DE007', 'D010',2, 'S03'),
(2, 'DE008', 'D010',2, 'S03'),
(2, 'DE009', 'D010',2, 'S03'),

(2, 'DE001', 'D011',2, 'S03'),
(2, 'DE002', 'D011',2, 'S03'),
(2, 'DE003', 'D011',2, 'S03'),
(2, 'DE004', 'D011',2, 'S03'),
(2, 'DE005', 'D011',2, 'S03'),
(2, 'DE006', 'D011',2, 'S03'),
(2, 'DE007', 'D011',2, 'S03'),
(2, 'DE008', 'D011',2, 'S03'),
(2, 'DE009', 'D011',2, 'S03'),

(2, 'DE001', 'D012',2, 'S03'),
(2, 'DE002', 'D012',2, 'S03'),
(2, 'DE003', 'D012',2, 'S03'),
(2, 'DE004', 'D012',2, 'S03'),
(2, 'DE005', 'D012',2, 'S03'),
(2, 'DE006', 'D012',2, 'S03'),
(2, 'DE007', 'D012',2, 'S03'),
(2, 'DE008', 'D012',2, 'S03'),
(2, 'DE009', 'D012',2, 'S03')
;


INSERT INTO DepartmentWiseFacultyMaster
(FacultyCode, DepartmentCode, SeatSlabId, DesignationCode, Seats)
VALUES
(2, 'DE000','S01','D010', 9),
(2, 'DE000','S01','D011', 9),
(2, 'DE000','S01','D012', 9),
(2, 'DE000','S02','D010', 9),
(2, 'DE000','S02','D011', 9),
(2, 'DE000','S02','D012', 18),
(2, 'DE000','S03','D010', 9),
(2, 'DE000','S03','D011', 18),
(2, 'DE000','S03','D012', 27);




SELECT * FROM [dbo].[DentalCollegeLandBuildingDetail]