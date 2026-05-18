

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


------ MASTER FOR EQUIPMENT DEPARTMENTS ------
CREATE TABLE MstEquipmentDepartments
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    FacultyCode INT NOT NULL,

    DepartmentCode NVARCHAR(20) NOT NULL UNIQUE,

    DepartmentName NVARCHAR(500) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_MstEquipmentDepartments_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId)
);

--- INSERT QUERIES for Equipment departments-------------------------------


INSERT INTO MstEquipmentDepartments
(FacultyCode, DepartmentCode, DepartmentName)
VALUES
(2, 'EQDEPT001', 'Prosthodontics and Crown & Bridge'),
(2, 'EQDEPT002', 'Clinical Lab for Prosthetics'),
(2, 'EQDEPT003', 'Chrome � Cobalt Lab Equipment'),
(2, 'EQDEPT004', 'Ceramic Lab Equipment'),
(2, 'EQDEPT005', 'Implant Equipment'),
(2, 'EQDEPT006', 'Periodontology'),
(2, 'EQDEPT007', 'Surgical Instruments'),
(2, 'EQDEPT008', 'Special Surgical Instruments'),
(2, 'EQDEPT009', 'Miscellaneous Instruments'),
(2, 'EQDEPT010', 'Oral & Maxillofacial Surgery'),
(2, 'EQDEPT011', 'Conservative Dentistry and Endodontics'),
(2, 'EQDEPT012', 'Orthodontics and Dentofacial Orthopedics'),
(2, 'EQDEPT013', 'Oral & Maxillofacial Pathology and Oral Microbiology'),
(2, 'EQDEPT014', 'Public Health Dentistry'),
(2, 'EQDEPT015', 'Paedodontics and Preventive Dentistry'),
(2, 'EQDEPT016', 'Oral Medicine and Radiology');


-----------------MASTER FOR EQUIPMENTS DEPT WISE ------------------------

CREATE TABLE MstEquipmentDeptWise
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    FacultyCode INT NOT NULL,

    DepartmentCode NVARCHAR(20) NOT NULL,

    EquipmentName NVARCHAR(500) NOT NULL,

    Specification NVARCHAR(MAX) NULL,

    OneUnitRequirement INT NULL,

    TwoUnitRequirement INT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_MstEquipmentDeptWise_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_MstEquipmentDeptWise_Department
        FOREIGN KEY (DepartmentCode)
        REFERENCES MstEquipmentDepartments(DepartmentCode)
);

-------------INSERT QUERIES FOR EQUIPMENT MASTER ------------

INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT001',
    'Electrical Dental Chairs and Units',
    'With shadowless lamp, spittoon, 3 way syringe, instrument tray and motorized suction, micromotor and airotor attachment with handpieces. One chair and unit per PG student and two chairs with unit for the faculty.',
    1,
    2
),
(
    2,
    'EQDEPT001',
    'Articulators � Semi Adjustable/Adjustable with Face Bow',
    NULL,
    6,
    12
),
(
    2,
    'EQDEPT001',
    'Micromotor',
    'Lab type can also be attached (fixed) to wall',
    2,
    4
),
(
    2,
    'EQDEPT001',
    'Ultrasonic Scaler',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT001',
    'Light Cures',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT001',
    'Hot Air Oven',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT001',
    'Autoclave',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT001',
    'Surveyor',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT001',
    'Refrigerator',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT001',
    'X-ray Viewer',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT001',
    'Pneumatic Crown Bridge Remover',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT001',
    'Needle Destroyer',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT001',
    'Intra Oral Camera',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT001',
    'Digital SLR Camera',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT001',
    'Computer with Internet Connection with Attached Printer and Scanner',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT001',
    'LCD Projector',
    NULL,
    1,
    1
);

--2
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT002',
    'Plaster Dispenser',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT002',
    'Model Trimmer with Carborandum Disc',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT002',
    'Model Trimmer with Diamond Disc',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT002',
    'High Speed Lathe',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT002',
    'Vibrator',
    NULL,
    2,
    4
),
(
    2,
    'EQDEPT002',
    'Acrylizer',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT002',
    'Dewaxing Unit',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT002',
    'Hydraulic Press',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT002',
    'Mechanical Press',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT002',
    'Vacuum Mixing Machine',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT002',
    'Micro Motor Lab Type',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT002',
    'Curing Pressure Pot',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT002',
    'Pressure Molding Machine',
    NULL,
    1,
    1
);

--3
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT003',
    'Duplicator',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Pindex System',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Burn-out Furnace',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Welder',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Sandblaster',
    'Micro and macro',
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Electro-polisher',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Model Trimmer with Carborandum Disc',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Model Trimmer with Diamond Disc',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Model Trimmer with Double Disc',
    'One carborandum and one diamond disc',
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Casting Machine',
    'Motor cast with safety door closure, gas blow torch with regulator',
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Dewaxing Furnace',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Induction Casting Machine with Vacuum Pump',
    'Capable of casting titanium chrome cobalt precision metal',
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Spot Welder',
    'With soldering attachment of cable',
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Steam Cleaner',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Vacuum Mixing Machine',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Spindle Grinder',
    '24,000 RPM with vacuum suction',
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Wax Heater',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT003',
    'Wax Carvers (Full PKT Set)',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT003',
    'Milling Machine',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Stereo Microscope',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Magnifying Work Lamp',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Heavy Duty Lathe with Suction',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Preheating Furnace',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Dry Model Trimmer',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Die Cutting Machine',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT003',
    'Ultrasonic Cleaner',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT003',
    'Composite Curing Unit',
    NULL,
    1,
    1
);

--4
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT004',
    'Fully Programmable Porcelain Furnace with Vacuum Pump',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT004',
    'Ceramic Kit',
    'Instruments',
    3,
    3
),
(
    2,
    'EQDEPT004',
    'Ceramic Materials',
    'Kit',
    1,
    1
),
(
    2,
    'EQDEPT004',
    'Ceramic Polishing Kit',
    NULL,
    2,
    2
);

--5
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT005',
    'Electrical Dental Chair and Unit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT005',
    'Physio Dispenser',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT005',
    'Implant Kit',
    'Minimum 2 systems',
    2,
    2
),
(
    2,
    'EQDEPT005',
    'Implants',
    NULL,
    10,
    10
),
(
    2,
    'EQDEPT005',
    'Prosthetic Components',
    NULL,
    10,
    10
),
(
    2,
    'EQDEPT005',
    'Unit Mount Light Cure',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT005',
    'X-ray Viewer',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT005',
    'Needle Destroyer',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT005',
    'Ultrasonic Cleaner',
    'Capacity 3.5 lts',
    1,
    1
),
(
    2,
    'EQDEPT005',
    'Autoclave',
    'Programmable for all recommended cycles',
    1,
    2
),
(
    2,
    'EQDEPT005',
    'X-ray Machine with RVG',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT005',
    'Refrigerator',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT005',
    'Surgical Kit/Prosthetic Kit',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT005',
    'Educating Models',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT005',
    'Implant Removing Instruments',
    NULL,
    1,
    1
);

--6
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT006',
    'Dental Chairs and Units',
    'Electrically operated with shadowless lamp, spittoon, 3 way syringe, instrument tray and motorized suction, micromotor attachment with contra angle handpiece, airotor attachment, ultrasonic scaler (Piezo) with detachable autoclavable hand piece. One chair and unit per post-graduate student and two chairs with unit for the faculty',
    1,
    2
),
(
    2,
    'EQDEPT006',
    'Auto Clave',
    'Fully automatic front loading',
    1,
    2
),
(
    2,
    'EQDEPT006',
    'Steel Bin',
    NULL,
    4,
    6
),
(
    2,
    'EQDEPT006',
    'Airoter Hand Pieces',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT006',
    'UV Chamber',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT006',
    'Formalin Chamber',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT006',
    'W.H.O Probe',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT006',
    'Nabers Probe',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT006',
    'Williams Probe',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT006',
    'UNC-15 Probe',
    NULL,
    4,
    4
),
(
    2,
    'EQDEPT006',
    'Gold Man Fox Probe',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT006',
    'Pressure Sensitive Probe',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT006',
    'Marquis Color Coded Probe',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT006',
    'Supra Gingival Scalers Set',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT006',
    'Sub Gingival Scaler Set',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT006',
    'Arkansas Sharpening Stone',
    NULL,
    1,
    1
);

--7
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT007',
    'Routine Surgical Instrument Kit',
    'Benquis periosteal elevator, periotome',
    2,
    3
),
(
    2,
    'EQDEPT007',
    'Surgery Trolleys',
    NULL,
    6,
    6
),
(
    2,
    'EQDEPT007',
    'X-ray Viewer',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT007',
    'Surgical Cassette with Sterilisation Pouches',
    NULL,
    4,
    6
),
(
    2,
    'EQDEPT007',
    'Electro Surgery Unit',
    NULL,
    1,
    1
);

--8
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT008',
    'Kirkland�s Knife Set',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Orban�s Knife Set',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Paquette Blade Handle',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Krane Kaplan Pocket Marker Set',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Mc Calls Universal Curettes Set',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Gracey�s Curettes (No. 1-18) Set',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT008',
    'Mini Five Curettes Set',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Cumine Scalar',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Mallet',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Chisel',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Oschenbein Chisel',
    'Straight, curved',
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Schluger Bone File',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Bone Fixation Screw Kit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Bone Scrapper',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Bone Trephines for Harvesting Autografts',
    '1 set',
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Bone Regenerative Materials',
    'Bone graft and GTR membranes',
    5,
    5
),
(
    2,
    'EQDEPT008',
    'Local Drug Delivery Systems',
    'At least two different agents',
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Root Conditioning Agent',
    'At least two different agents',
    2,
    2
),
(
    2,
    'EQDEPT008',
    'Micro Needle Holder',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Micro Scissors',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Magnifying Loop (2.5 � 3.5)',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT008',
    'Operating Microscope',
    'Optional',
    1,
    1
),
(
    2,
    'EQDEPT008',
    '3rd Generation Digital Probe',
    'Optional',
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Bone Expander and Bone Crester',
    'Optional',
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Distraction Osteogenesis Kit',
    'Optional',
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Bone Mill',
    'Optional',
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Bone Graft / Membrane Placement Spoon',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Bone Condenser',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Peizo-surgery Unit',
    'Optional',
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Centrifuge for PRP/PRF Preparation',
    'Optional',
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Soft Tissue Laser (8 Watt)',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT008',
    'Osteotome Set',
    'Optional',
    1,
    1
);

--9
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT009',
    'Composite Gun with Material Kit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Splinting Kit with Material',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT009',
    'Composite Finishing Kit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Glass Ionomer Cement',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Digital Camera',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Intra Oral Camera',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Ultrasonic Cleaner',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Emergency Kit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Refrigerator',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'X-ray Viewer',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT009',
    'LCD Projector',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Computer with Internet Connection with Attached Printer and Scanner',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Electrical Dental Chair and Unit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Physio Dispenser',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Implant Kit',
    'At least two different systems',
    2,
    2
),
(
    2,
    'EQDEPT009',
    'Implants',
    NULL,
    10,
    10
),
(
    2,
    'EQDEPT009',
    'Implant Maintenance Kit',
    'Plastic instruments',
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Implant Guide',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'X-ray Viewer',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT009',
    'Needle Destroyer',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT009',
    'Ultrasonic Cleaner',
    'Capacity 3.5 lts',
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Autoclave',
    'Programmable for all recommended cycles',
    1,
    1
),
(
    2,
    'EQDEPT009',
    'RVG with X-ray Machine',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Refrigerator',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Surgical Kit',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT009',
    'Sinus Lift Kit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Educating Models',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT009',
    'Implant Removing Kit',
    NULL,
    1,
    1
);

--10
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT010',
    'Dental Chairs and Units',
    'Electrically operated with shadowless lamp, spittoon, 3 way syringe, instrument tray and high motorized suction, with micromotor and micro motor attachment. One chair and unit per post-graduate student and two chairs with unit for the faculty',
    1,
    2
),
(
    2,
    'EQDEPT010',
    'Autoclave',
    'Front loading',
    2,
    3
),
(
    2,
    'EQDEPT010',
    'Fumigators',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Oscillating Saw',
    'With all hand pieces',
    1,
    1
),
(
    2,
    'EQDEPT010',
    'General Surgery Kit Including Tracheotomy Kit',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT010',
    'Minor Oral Surgery Kit',
    NULL,
    5,
    10
),
(
    2,
    'EQDEPT010',
    'Osteotomy Kit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Cleft Surgery Kit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Bone Grafting Kit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Emergency Kit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Trauma Set Including Bone Plating Kit',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT010',
    'Implantology Kit with Implants',
    'Minimum 2 systems',
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Distraction Osteogenesis Kit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Peizo Surgical Unit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Magnifying Loops',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Operating Microscope and Microsurgery Kit',
    'Desirable',
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Dermatomes',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Needle Destroyer',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT010',
    'Ultrasonic Cleaner',
    'Capacity 3.5 lts',
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Formalin Chamber',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Pulse Oxymeter',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Ventilator',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Major Operation Theatre with All Facilities',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Recovery and Intensive Care Unit with All Necessary Life Support Equipments',
    '2 beds',
    2,
    2
),
(
    2,
    'EQDEPT010',
    'Fibrooptic Light',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Inpatient Beds',
    NULL,
    20,
    20
),
(
    2,
    'EQDEPT010',
    'Fiber Optic Laryngoscope',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Computer with Internet Connection with Attached Printer and Scanner',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'LCD Projector',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT010',
    'Refrigerator',
    NULL,
    1,
    1
);

--11
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT011',
    'Dental Chairs and Units',
    'Electrically operated with shadowless lamp, spittoon, 3 way syringe, instrument tray and motorized suction, micromotor, airotor attachment with hand pieces. One chair and unit per post-graduate student and two chairs with unit for the faculty',
    1,
    2
),
(
    2,
    'EQDEPT011',
    'Endosonic Handpieces',
    'Micro endosonic tips, retro treatment',
    2,
    3
),
(
    2,
    'EQDEPT011',
    'Mechanised Rotary Instruments Including Hand Pieces',
    'Speed and torque control and hand instruments various systems',
    3,
    6
),
(
    2,
    'EQDEPT011',
    'Rubber Dam Kit',
    'One per chair',
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Autoclaves for Bulk Instrument Sterilization',
    'Vacuum front loading',
    2,
    3
),
(
    2,
    'EQDEPT011',
    'Autoclaves for Hand Piece Sterilization',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Apex Locators',
    'One for every two chairs',
    2,
    4
),
(
    2,
    'EQDEPT011',
    'Pulp Tester',
    NULL,
    2,
    4
),
(
    2,
    'EQDEPT011',
    'Equipments for Injectable Thermoplasticized Gutta Percha',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT011',
    'Operating Microscopes',
    '3 step or 5 step magnification',
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Surgical Endo Kits',
    'Microsurgery',
    2,
    2
),
(
    2,
    'EQDEPT011',
    'Set of Hand Instruments',
    'Specifications required',
    1,
    2
),
(
    2,
    'EQDEPT011',
    'Sterilizer Trays for Autoclave',
    NULL,
    4,
    4
),
(
    2,
    'EQDEPT011',
    'Ultrasonic Cleaner',
    'Capacity 3.5 lts',
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Variable Intensity Polymerization Equipments - VLC Units',
    'Desirable',
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Conventional VLC Units',
    'One for every two chairs',
    2,
    4
),
(
    2,
    'EQDEPT011',
    'Needle Destroyer',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT011',
    'Magnifying Loupes',
    'One for students and one for faculty',
    1,
    2
),
(
    2,
    'EQDEPT011',
    'LCD Projector',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Composite Kits with Different Shades and Polishing Kits',
    NULL,
    2,
    4
),
(
    2,
    'EQDEPT011',
    'Ceramic Finishing Kits, Metal Finishing Kits',
    'In ceramic labs',
    2,
    3
),
(
    2,
    'EQDEPT011',
    'Amalgam Finishing Kits',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT011',
    'RVG with X-ray Machine Developing Kit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Chair Side Micro Abrasion',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Bleaching Unit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Instrument Retrieval Kits',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Computer with Internet Connection with Attached Printer and Scanner',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Refrigerator',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Equipments for Casting Procedures',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Equipments for Ceramics Including Induction Casting Machines / Burnout Preheat Furnaces / Wax Elimination Furnaces',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Lab Micro Motor / Metal Grinders / Sand Blasters / Polishing Lathes / Duplicator Equipment / Vacuum Investment Equipments',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Laser',
    'Preferably hard tissue',
    1,
    1
),
(
    2,
    'EQDEPT011',
    'Face Bow with Semi Adjustable Articulator',
    NULL,
    1,
    2
);

--12
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT012',
    'Dental Chairs and Unit',
    'Electrically operated with shadow less lamp, spittoon, 3 way syringe, instrument tray and motorized suction. One chair and unit per PG student and two chairs with unit for the faculty',
    1,
    2
),
(
    2,
    'EQDEPT012',
    'Vacuum / Pressure Moulding Unit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Hydrogen Soldering Unit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Lab Micromotor',
    NULL,
    3,
    5
),
(
    2,
    'EQDEPT012',
    'Spot Welders',
    NULL,
    3,
    5
),
(
    2,
    'EQDEPT012',
    'Model Trimmer (Double Disc)',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT012',
    'Light Curing Unit',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT012',
    'High Intensity Light Curing Unit',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT012',
    'Polishing Lathes',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT012',
    'Tracing Tables',
    NULL,
    3,
    5
),
(
    2,
    'EQDEPT012',
    'SLR Digital Camera',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Scanner with Transparency Adapter',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT012',
    'X-ray Viewer',
    NULL,
    3,
    4
),
(
    2,
    'EQDEPT012',
    'LCD Projector',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Autoclaves for Bulk Instrument Sterilization',
    'Vacuum front loading',
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Needle Destroyer',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Dry Heat Sterilizer',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Ultrasonic Scaler',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Sets of Orthodontic Pliers',
    NULL,
    3,
    3
),
(
    2,
    'EQDEPT012',
    'Orthodontic Impression Trays',
    NULL,
    3,
    5
),
(
    2,
    'EQDEPT012',
    'Ultrasonic Cleaner',
    'Capacity 3.5 lts',
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Electropolisher',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Typodonts with Full Teeth Set',
    NULL,
    3,
    3
),
(
    2,
    'EQDEPT012',
    'Anatomical Articulator with Face Bow Attachments',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Free Plane Articulators',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Hinge Articulators',
    NULL,
    4,
    4
),
(
    2,
    'EQDEPT012',
    'Computer Software for Cephalometrics',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Computer with Internet Connection with Attached Printer and Scanner',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT012',
    'Refrigerator',
    NULL,
    1,
    1
);

--13
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT013',
    'Dental Chairs and Units',
    'Electrically operated with shadow less lamp, spittoon, 3 way syringe, instrument tray and suction',
    3,
    6
),
(
    2,
    'EQDEPT013',
    'Adequate Laboratory Glasswares',
    'As required for processing of biopsy specimens and staining. Reasonable quantity should be made available',
    NULL,
    NULL
),
(
    2,
    'EQDEPT013',
    'Adequate Tissue Capsules / Tissue Embedding Cassettes',
    'Reasonable quantity should be made available',
    NULL,
    NULL
),
(
    2,
    'EQDEPT013',
    'Paraffin Wax Bath',
    'Thermostatically controlled',
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Leuckhart Pieces',
    NULL,
    10,
    10
),
(
    2,
    'EQDEPT013',
    'Block Holders',
    NULL,
    25,
    25
),
(
    2,
    'EQDEPT013',
    'Microtome',
    'Manual',
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Microtome',
    'Semi-automated',
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Tissue Floatation Water Bath',
    'Thermostatically controlled',
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Slide Warming Table',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Steel Slide Racks for Staining',
    NULL,
    5,
    5
),
(
    2,
    'EQDEPT013',
    'Diamond Glass Marker',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT013',
    'Research Microscope',
    'With phase contrast, dark field, polarization, image analyzer and photomicrography attachments',
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Multi Head Microscope',
    'Penta headed',
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Binocular Compound Microscope',
    '2 for faculty and one per student',
    NULL,
    NULL
),
(
    2,
    'EQDEPT013',
    'Stereo Microscope',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Aluminum Slide Trays',
    NULL,
    5,
    5
),
(
    2,
    'EQDEPT013',
    'Wooden / Plastic Slide Boxes',
    NULL,
    5,
    5
),
(
    2,
    'EQDEPT013',
    'Wax Block Storing Cabinet',
    '5,000 capacity',
    5000,
    10000
),
(
    2,
    'EQDEPT013',
    'Slide Storing Cabinet',
    '5,000 capacity',
    5000,
    10000
),
(
    2,
    'EQDEPT013',
    'Refrigerator',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Pipettes',
    NULL,
    5,
    5
),
(
    2,
    'EQDEPT013',
    'Surgical Kit for Biopsy',
    NULL,
    3,
    6
),
(
    2,
    'EQDEPT013',
    'Immuno Histo Chemistry Lab',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Computer with Internet Connection with Attached Printer and Scanner',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT013',
    'LCD Projector',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Cryostat',
    'Desirable equipment',
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Fluorescent Microscope',
    'Desirable equipment',
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Hard Tissue Microtome',
    'Desirable equipment',
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Tissue Storing Cabinet (Frozen)',
    'Desirable equipment',
    1,
    1
),
(
    2,
    'EQDEPT013',
    'Microwave',
    'Desirable equipment',
    1,
    1
);

--14
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT014',
    'Dental Chairs',
    'Electrically operated with shadowless lamp, spittoon, 3 way syringe, instrument tray and motorized suction, micromotor attachment with contra angle handpiece, airoter attachment, ultrasonic scaler (Piezo) with detachable autoclavable hand piece with minimum 3 tips. One chair and unit per post-graduate student and one chair with unit for the faculty',
    1,
    2
),
(
    2,
    'EQDEPT014',
    'Extraction Forceps',
    NULL,
    4,
    6
),
(
    2,
    'EQDEPT014',
    'Filling Instruments',
    NULL,
    4,
    6
),
(
    2,
    'EQDEPT014',
    'Scaling Instruments',
    'Supra gingival scaling',
    4,
    6
),
(
    2,
    'EQDEPT014',
    'Amalgamator',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Pulp Tester',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Autoclave',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'X-ray Viewer',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Instrument Cabinet',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'LCD or DLP Multimedia Projector',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Computer with Internet Connection with Attached Printer and Scanner',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Staff Bus',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Mobile Dental Clinic',
    'Fitted with at least 2 dental chairs with complete dental unit with fire extinguisher',
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Ultrasonic Scaler',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT014',
    'Ultrasonic Cleaner',
    'Capacity 3.5 lts',
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Compressor',
    'One with chair',
    NULL,
    NULL
),
(
    2,
    'EQDEPT014',
    'Generator',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Public Address System, Audio-visual Aids',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Television',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Digital Versatile Disc Player',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Instrument Cabinet, Emergency Medicine Kits, Blood Pressure Apparatus',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Portable Oxygen Cylinder',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Portable Chair',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT014',
    'Refrigerator',
    NULL,
    1,
    1
);

--15
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT015',
    'Dental Chairs and Units',
    'Electrically operated with shadowless lamp, spittoon, 3 way syringe, and motorised suction, micromotor attachment with contra angle miniature handpiece, airotor attachment with miniature handpiece, dental operator stool. 40% dental chairs shall be pedo chairs. One chair and unit per post-graduate student and two chairs with unit for the faculty',
    1,
    2
),
(
    2,
    'EQDEPT015',
    'Pedo Extraction Forceps Sets',
    NULL,
    3,
    4
),
(
    2,
    'EQDEPT015',
    'Autoclaves for Bulk Instrument Sterilization',
    'Vacuum front loading',
    1,
    2
),
(
    2,
    'EQDEPT015',
    'RVG with Intra Oral X-ray Unit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Automatic Developer',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Pulp Tester',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT015',
    'Apex Locator',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Rubber Dam Kit',
    'One set per student',
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Injectable GP Condenser',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Endodontic Pressure Syringe',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Glass Bead Steriliser',
    NULL,
    2,
    4
),
(
    2,
    'EQDEPT015',
    'Spot Welder',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT015',
    'Ultrasonic Scalers',
    NULL,
    2,
    4
),
(
    2,
    'EQDEPT015',
    'Needle Destroyer',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Formalin Chamber',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Ultrasonic Cleaner',
    'Capacity 3.5 lts',
    1,
    1
),
(
    2,
    'EQDEPT015',
    'X-ray Viewer',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT015',
    'Amalgamator',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT015',
    'Plaster Dispenser',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT015',
    'Dental Lathe',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT015',
    'Vibrator',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT015',
    'Typodonts',
    'One set per student',
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Soldering Unit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Band Pinching Beak Pliers',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT015',
    'Proximal Contouring Pliers',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT015',
    'Crown Crimping Pliers',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT015',
    'Double Beak Pliers Anterior and Posterior',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT015',
    'Lab Micro Motor',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT015',
    'Acryliser',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT015',
    'Magnifying Loupes',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Conscious Sedation Unit',
    'Desirable',
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Pulse Oxymeter',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Phantom Head Table with Attached Light, Airotor and Micro Motor',
    'One set per each P.G. student',
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Computer with Internet Connection with Attached Printer and Scanner',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT015',
    'LCD Projector',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT015',
    'Refrigerator',
    NULL,
    1,
    1
);

--16
INSERT INTO MstEquipmentDeptWise
(
    FacultyCode,
    DepartmentCode,
    EquipmentName,
    Specification,
    OneUnitRequirement,
    TwoUnitRequirement
)
VALUES
(
    2,
    'EQDEPT016',
    'Dental Chairs and Units',
    'Electrically operated with shadowless lamp, spittoon, 3 way syringe, instrument tray and suction. One chair and unit per post-graduate student and one chair with unit for the faculty',
    1,
    2
),
(
    2,
    'EQDEPT016',
    'RVG with Intra Oral Radiography Machine',
    'FDA approved, 55-70 kVp with digital compatibility',
    1,
    1
),
(
    2,
    'EQDEPT016',
    'Extra Oral Radiography Machine',
    '100 kVp',
    1,
    1
),
(
    2,
    'EQDEPT016',
    'Panoramic Radiography (OPG) Machine with Cephalometric and TMJ Attachment with Printer',
    'Digital compatibility',
    1,
    1
),
(
    2,
    'EQDEPT016',
    'Intra Oral Camera',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT016',
    'Pulp Tester',
    NULL,
    2,
    4
),
(
    2,
    'EQDEPT016',
    'Autoclave',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT016',
    'Punch Biopsy Tool',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT016',
    'Biopsy Equipment',
    NULL,
    1,
    2
),
(
    2,
    'EQDEPT016',
    'Surgical Trolley',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT016',
    'Emergency Medicines Kit',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT016',
    'Extra Oral Cassettes with Intensifying Screens',
    'Conventional and rare earth',
    4,
    6
),
(
    2,
    'EQDEPT016',
    'Lead Screens',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT016',
    'Lead Aprons',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT016',
    'Lead Gloves',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT016',
    'Radiographic Filters',
    'Conventional and rare earth',
    1,
    1
),
(
    2,
    'EQDEPT016',
    'Dark Room with Safe Light Facility',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT016',
    'Automatic Radiographic Film Processors',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT016',
    'Radiographic Film Storage Lead Containers',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT016',
    'Thyroid Collars',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT016',
    'Digital Sphygmomanometer',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT016',
    'Digital Blood Glucose Tester',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT016',
    'Digital Camera',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT016',
    'X-ray Viewer Boxes',
    NULL,
    2,
    3
),
(
    2,
    'EQDEPT016',
    'Lacrimal Probes',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT016',
    'Sialography Cannula',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT016',
    'Illuminated Mouth Mirror and Probe',
    NULL,
    2,
    2
),
(
    2,
    'EQDEPT016',
    'Computer with Internet Connection with Attached Printer and Scanner',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT016',
    'LCD Projector',
    NULL,
    1,
    1
),
(
    2,
    'EQDEPT016',
    'Refrigerator',
    NULL,
    1,
    1
);

-----------------------------

CREATE TABLE [dbo].[DentalCollegeEquipmentDetails] (
    [Id] [int] IDENTITY(1,1) NOT NULL,
    
    -- Primary Identifiers
    [CollegeCode] [nvarchar](50) NOT NULL, 
    [FacultyCode] [int] NOT NULL,         -- 1 for Medical, 2 for Dental, etc.
    [DepartmentCode] [nvarchar](50) NOT NULL,
    [EquipmentId] [int] NOT NULL,        -- Link to MstEquipmentDeptWise
    [EquipmentName] [nvarchar](500) NOT NULL,
    
    -- Requirement Snapshots (Nullable as requested)
    [OneUnitRequirement] [int] NULL,
    [TwoUnitRequirement] [int] NULL,
    
    -- Existing Data / User Input (Nullable as requested)
    [OneUnitExisting] [int] NULL,
    [TwoUnitExisting] [int] NULL,
    
    -- Audit Trail
    [CreatedDate] [datetime] NOT NULL DEFAULT (getdate()),
    [UpdatedDate] [datetime] NULL,
    [IsActive] [bit] NOT NULL DEFAULT (1),
    
    CONSTRAINT [PK_CollegeEquipmentDetails] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_CollegeEquipmentDetails_MstEquipmentDeptWise] FOREIGN KEY ([EquipmentId]) 
        REFERENCES [dbo].[MstEquipmentDeptWise] ([Id])
);

-- Index for performance when fetching a specific college's equipment
CREATE INDEX IX_College_Faculty ON [dbo].[DentalCollegeEquipmentDetails] (CollegeCode, FacultyCode);


Alter Table Medical_UGBedDistribution
add OralMaxillofacialSurgery INT NULL;

CREATE TABLE CA_DentalLibraryRecords
(
    Id INT IDENTITY PRIMARY KEY,

    CollegeCode NVARCHAR(20) NOT NULL,

    FacultyCode INT NOT NULL,

    CourseLevel NVARCHAR(20) NULL,

    AffiliationType INT NOT NULL,

    RecordId INT NOT NULL,

    FilePath NVARCHAR(MAX) NULL,

    FileName NVARCHAR(500) NULL,

    CreatedDate DATETIME
        DEFAULT GETDATE()
);


USE [Admission_Affiliation]
GO
/****** Object:  Table [dbo].[CA_MST_DentalLibraryRecords]    Script Date: 14-05-2026 11:36:24 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CA_MST_DentalLibraryRecords](
    [RecordId] [int] NOT NULL,
    [RecordName] [nvarchar](500) NOT NULL,
    [DisplayOrder] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [RecordId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT [dbo].[CA_MST_DentalLibraryRecords] ([RecordId], [RecordName], [DisplayOrder]) VALUES (1, N'Central Library Book List', 1)
GO
INSERT [dbo].[CA_MST_DentalLibraryRecords] ([RecordId], [RecordName], [DisplayOrder]) VALUES (2, N'Departmental Library List (PG)', 2)
GO
INSERT [dbo].[CA_MST_DentalLibraryRecords] ([RecordId], [RecordName], [DisplayOrder]) VALUES (3, N'Journal Subscription Receipts', 3)
GO
INSERT [dbo].[CA_MST_DentalLibraryRecords] ([RecordId], [RecordName], [DisplayOrder]) VALUES (4, N'Back Volume Records', 4)
GO
INSERT [dbo].[CA_MST_DentalLibraryRecords] ([RecordId], [RecordName], [DisplayOrder]) VALUES (5, N'Library Attendance Register', 5)
GO
INSERT [dbo].[CA_MST_DentalLibraryRecords] ([RecordId], [RecordName], [DisplayOrder]) VALUES (6, N'E-Library Subscription Proof', 6)
GO



-----------------------------------------------
-- =============================================
-- Create Master Table
-- =============================================

CREATE TABLE MstDentalPreClinicalAndSkillsLaboratoryAreaReq
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    FacultyCode INT NOT NULL,

    LaboratoryName NVARCHAR(200) NOT NULL,

    SeatIntake INT NOT NULL,

    AreaRequiredSqM DECIMAL(10,2) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE()
);
GO


-- =============================================
-- Insert Master Data
-- FacultyCode = 2 (Dental)
-- =============================================

INSERT INTO MstDentalPreClinicalAndSkillsLaboratoryAreaReq
(
    FacultyCode,
    LaboratoryName,
    SeatIntake,
    AreaRequiredSqM
)
VALUES
-- Pre Clinical Prosthodontics Lab
(2, 'Pre Clinical Prosthodontics Lab', 50, 200),
(2, 'Pre Clinical Prosthodontics Lab', 100, 250),
(2, 'Pre Clinical Prosthodontics Lab', 150, 250),

-- Pre Clinical Conservative Lab
(2, 'Pre Clinical Conservative Lab', 50, 200),
(2, 'Pre Clinical Conservative Lab', 100, 250),
(2, 'Pre Clinical Conservative Lab', 150, 250),

-- Dental Materials Lab
(2, 'Dental Materials Lab', 50, 120),
(2, 'Dental Materials Lab', 100, 150),
(2, 'Dental Materials Lab', 150, 150),

-- Skill / Simulation Lab
(2, 'Skill / Simulation Lab', 50, 200),
(2, 'Skill / Simulation Lab', 100, 250),
(2, 'Skill / Simulation Lab', 150, 250);
GO

-- =============================================
-- Table: DentalPreClinicalAndSkillsLabAreaReq
-- =============================================

CREATE TABLE DentalPreClinicalAndSkillsLabAreaReq
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    CollegeCode NVARCHAR(20) NOT NULL,

    FacultyCode INT NOT NULL,

    SeatIntake INT NOT NULL,

    LabId INT NOT NULL,

    LabName NVARCHAR(200) NOT NULL,

    RequiredAreaSqM DECIMAL(10,2) NOT NULL,

    ExistingAreaSqM DECIMAL(10,2) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedOn DATETIME NULL,

    CONSTRAINT FK_DentalPreClinicalAndSkillsLabAreaReq_LabId
        FOREIGN KEY (LabId)
        REFERENCES MstDentalPreClinicalAndSkillsLaboratoryAreaReq(Id)
);
GO

ALTER TABLE [dbo].[CA_MedicalDepartmentLibrary]
ADD
    Titles INT NULL,
    InternationalJournals INT NULL,
    BackVolumes INT NULL,
    PrintJournalPercentage INT NULL;


SELECT * FROM DesignationMaster WHERE FacultyCode=2;

SELECT * FROM DesignationMaster 
WHERE FacultyCode=2 AND DesignationCode IN ('D013','D014');

--DELETE FROM DesignationMaster
--WHERE FacultyCode=2 and DesignationCode in ('D013','D014');

SELECT * FROM FacultyDetails where FacultyCode=2;

--INSERT INTO DesignationMaster
--(FacultyCode, DesignationCode, DesignationName, DesignationOrder)
--VALUES
--(2, 'D013','Principal/Dean', 0)


SELECT * FROM TeachingStaffDepartmentWiseDetails;

--DELETE  FROM TeachingStaffDepartmentWiseDetails;


--DELETE FROM FacultyDetails

SELECT * FROM FacultyDetails


ALTER TABLE [dbo].[FacultyDetails]
ADD [From] DATE NULL, 
    [To] DATE NULL;

ALTER TABLE [dbo].[HospitalDetailsForAffiliation]
ADD DentalChairsCount INT NULL;


ALTER TABLE [dbo].[HospitalDetailsForAffiliation]
ADD Has24HourEmergency  BIT NULL,
    HasCriticalCareServices BIT NULL;

INSERT INTO [dbo].[MST_Hospital_Type]
(FacultyCode, HospitalType)
VALUES
(2, 'Own'),
(2, 'Parent');

INSERT INTO [dbo].[MST_HospitalOwnedBy]
(FacultyCode, OwnedBy)
VALUES
(2, 'Trust/Society/Missionary'),
(2, 'Trust member with MOU'),
(2, 'District Hospital'),
(2, 'Taluk hospital/Community Health Center');



CREATE TABLE MstMedicalAlliedDiscipline
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    FacultyCode INT NOT NULL,

    DisciplineCode VARCHAR(20) NOT NULL,

    DisciplineName VARCHAR(200) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_MstMedicalAlliedDiscipline_FacultyMaster FOREIGN KEY (FacultyCode) REFERENCES Faculty(facultyid)
);


INSERT INTO MstMedicalAlliedDiscipline
(
    FacultyCode,
    DisciplineCode,
    DisciplineName
)
VALUES
(2, 'MAD001', 'General Medicine'),
(2, 'MAD002', 'General Surgery'),
(2, 'MAD003', 'Obstetrics & Gynaecology'),
(2, 'MAD004', 'Orthopaedics'),
(2, 'MAD005', 'Critical Medicine'),
(2, 'MAD006', 'Emergency Medicine'),
(2, 'MAD007', 'Otorhinolaryngology'),
(2, 'MAD008', 'Paediatrics'),
(2, 'MAD009', 'Pathology'),
(2, 'MAD010', 'Anaesthesiology'),
(2, 'MAD011', 'Blood Bank & Transfusion'),
(2, 'MAD012', 'Community Medicine'),
(2, 'MAD013', 'Hospital Administration');

CREATE TABLE MedicalAlliedDisciplineDetail
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    -- FK References
    FacultyCode INT NOT NULL,

    CollegeCode NVARCHAR(100) NOT NULL,

    HospitalDetailsId INT NOT NULL,

    AffiliationTypeId INT NOT NULL,

    -- Discipline Details
    DisciplineCode VARCHAR(20) NOT NULL,

    DisciplineName VARCHAR(200) NOT NULL,

    Intake INT NULL,

    SeatSlab INT NOT NULL,

    Remarks VARCHAR(500) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedOn DATETIME NULL
);



ALTER TABLE MedicalAlliedDisciplineDetail
ADD CONSTRAINT FK_MedicalAlliedDisciplineDetail_Faculty
FOREIGN KEY (FacultyCode)
REFERENCES Faculty(FacultyId);

ALTER TABLE MedicalAlliedDisciplineDetail
ADD CONSTRAINT FK_MedicalAlliedDisciplineDetail_College
FOREIGN KEY (CollegeCode)
REFERENCES [dbo].[Affiliation_College_Master](CollegeCode);

ALTER TABLE MedicalAlliedDisciplineDetail
ADD CONSTRAINT FK_MedicalAlliedDisciplineDetail_Hospital
FOREIGN KEY (HospitalDetailsId)
REFERENCES HospitalDetailsForAffiliation(HospitalDetailsId);

ALTER TABLE MedicalAlliedDisciplineDetail
ADD CONSTRAINT FK_MedicalAlliedDisciplineDetail_AffiliationType
FOREIGN KEY (AffiliationTypeId)
REFERENCES TypeOfAffiliation(TypeId);

select * from [dbo].[Med_CA_AccountAndFeeDetails];

SELECT * FROM [dbo].[MedicalAlliedDisciplineDetail];

---------------------------------------

CREATE TABLE MstDentalServices
(Id int Identity(1, 1) primary key clustered,
    FacultyCode INT NOT NULL,
    SectionName VARCHAR(500) NULL,
    RequirementName VARCHAR(500) NOT NULL,
    IsActive bit NOT NULL default 1,
    SectionCode int not null,
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT Fk_MstDentalServices_FacultyCode FOREIGN KEY (FacultyCode) REFERENCES Faculty(FacultyId)

);


INSERT INTO MstDentalServices
(
    FacultyCode,
    SectionCode,
    SectionName,
    RequirementName,
    IsActive
)
VALUES

-- =========================================
-- Nursing, Paramedical, Technical & Allied Services
-- =========================================

(2, 1, 'Nursing, Paramedical, Technical & Allied Services', 'Dietetics & Therapeutics', 1),
(2, 1, 'Nursing, Paramedical, Technical & Allied Services', 'Drugs & Pharmacy', 1),
(2, 1, 'Nursing, Paramedical, Technical & Allied Services', 'ECG Technology', 1),
(2, 1, 'Nursing, Paramedical, Technical & Allied Services', 'Imaging Technology', 1),
(2, 1, 'Nursing, Paramedical, Technical & Allied Services', 'CSSD', 1),
(2, 1, 'Nursing, Paramedical, Technical & Allied Services', 'Physiotherapy', 1),
(2, 1, 'Nursing, Paramedical, Technical & Allied Services', 'Medical Record Section', 1),

-- =========================================
-- Engineering & Allied Services
-- =========================================

(2, 2, 'Engineering & Allied Services', 'Fire protection', 1),
(2, 2, 'Engineering & Allied Services', 'Electrical', 1),
(2, 2, 'Engineering & Allied Services', 'Air conditioning/Central heating', 1),
(2, 2, 'Engineering & Allied Services', 'Medical gases', 1),
(2, 2, 'Engineering & Allied Services', 'Refrigeration', 1),
(2, 2, 'Engineering & Allied Services', 'Central workshop', 1),
(2, 2, 'Engineering & Allied Services', 'Ambulance', 1),
(2, 2, 'Engineering & Allied Services', 'Water supply', 1),
(2, 2, 'Engineering & Allied Services', 'Sewage & waste disposal', 1),

-- =========================================
-- Administration & Ancillary Services
-- =========================================

(2, 3, 'Administration & Ancillary Services', 'General administration', 1),
(2, 3, 'Administration & Ancillary Services', 'Material management', 1),
(2, 3, 'Administration & Ancillary Services', 'Medical social worker', 1),
(2, 3, 'Administration & Ancillary Services', 'PRO', 1),
(2, 3, 'Administration & Ancillary Services', 'Library', 1),
(2, 3, 'Administration & Ancillary Services', 'Security', 1);

-------------------------


CREATE TABLE DentalServices
(Id INT IDENTITY(1, 1) PRIMARY KEY CLUSTERED,
    FacultyCode INT NOT NULL,
    AffiliationTypeId INT NOT NULL,
    HospitalDetailsId INT NOT NULL,
    CollegeCode NVARCHAR(100) NOT NULL,
    RequirementId INT NOT NULL,
    AvailabilityStatus BIT NULL,
    CreatedOn DATETIME,
    SectionCode INT NOT NULL,
    UpdatedOn DATETIME,

    CONSTRAINT FK_DentalServices_FacultyCode FOREIGN KEY (FacultyCode)
    REFERENCES Faculty(FacultyId),

    CONSTRAINT Fk_DentalServices_AffType FOREIGN KEY (AffiliationTypeId)
    REFERENCES TypeOfAffiliation(TypeId),

    CONSTRAINT Fk_DentalServices_HospitalDetailsId FOREIGN KEY(HospitalDetailsId)
    REFERENCES HospitalDetailsForAffiliation(HospitalDetailsId),

    CONSTRAINT Fk_DentalServices_CollegeCode FOREIGN KEY(CollegeCode)
    REFERENCES [dbo].[Affiliation_College_Master](CollegeCode),

    CONSTRAINT Fk_DentalServices_reqId FOREIGN KEY(RequirementId)
    REFERENCES MstDentalServices(Id)
        
);