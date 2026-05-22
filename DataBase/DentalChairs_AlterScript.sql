

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
    DistanceBetweenCollegeAndHospitalKm DECIMAL(10,2) NULL,

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

--ALTER TABLE DentalCollegeLandBuildingDetail
--ADD
--    LandCategory VARCHAR(50) NULL,

--    IsLandInTwoPieces BIT NULL,

--    DistanceBetweenCollegeAndHospitalKm DECIMAL(10,2) NULL;


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
(2, 'EQDEPT003', 'Chrome - Cobalt Lab Equipment'),
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

--UPDATE MstEquipmentDepartments
--SET DepartmentName = 'Chrome - Cobalt Lab Equipment'
--WHERE DepartmentCode = 'EQDEPT003' AND DepartmentName LIKE '%Chrome%'
--;

select * from MstEquipmentDepartments
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

    AreaRequiredSqFt DECIMAL(10,2) NOT NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE()
);
GO

ALTER TABLE MstDentalPreClinicalAndSkillsLaboratoryAreaReq
ADD
    SectionCode VARCHAR(10) NULL,
    LaboratorySection VARCHAR(500) NULL;

--EXEC sp_rename 'MstDentalPreClinicalAndSkillsLaboratoryAreaReq.AreaRequiredSqM', 'AreaRequiredSqFt', 'COLUMN';

--DELETE FROM MstDentalPreClinicalAndSkillsLaboratoryAreaReq

-- =============================================
-- Insert Master Data
-- FacultyCode = 2 (Dental)
-- =============================================

INSERT INTO MstDentalPreClinicalAndSkillsLaboratoryAreaReq
(
    FacultyCode,
    SectionCode,
    LaboratorySection,
    LaboratoryName,
    SeatIntake,
    AreaRequiredSqFt
)
VALUES

-- =====================================================
-- DENTAL SUBJECT LABORATORIES
-- =====================================================

(2, 'DSL', 'Dental Subject Laboratories', 'Pre-clinical Prosthodontics & Dental Material Lab', 50, 1500),
(2, 'DSL', 'Dental Subject Laboratories', 'Pre-clinical Prosthodontics & Dental Material Lab', 100, 3000),

(2, 'DSL', 'Dental Subject Laboratories', 'Pre-clinical Conservative Lab', 50, 1300),
(2, 'DSL', 'Dental Subject Laboratories', 'Pre-clinical Conservative Lab', 100, 2500),

(2, 'DSL', 'Dental Subject Laboratories', 'Oral Biology & Oral Pathology Lab', 50, 1300),
(2, 'DSL', 'Dental Subject Laboratories', 'Oral Biology & Oral Pathology Lab', 100, 2500),

(2, 'DSL', 'Dental Subject Laboratories', 'Orthodontics & Pedodontics Lab', 50, 800),
(2, 'DSL', 'Dental Subject Laboratories', 'Orthodontics & Pedodontics Lab', 100, 1500),

-- =====================================================
-- MEDICAL SUBJECT LABORATORIES
-- =====================================================

(2, 'MSL', 'Medical Subject Laboratories', 'Medical Subject Laboratories (Independent Dental Colleges only)', 50, 4500),
(2, 'MSL', 'Medical Subject Laboratories', 'Medical Subject Laboratories (Independent Dental Colleges only)', 100, 7500),

(2, 'MSL', 'Medical Subject Laboratories', 'Anatomy Dissection Hall', 50, 1500),
(2, 'MSL', 'Medical Subject Laboratories', 'Anatomy Dissection Hall', 100, 2500),

(2, 'MSL', 'Medical Subject Laboratories', 'Physiology, Pathology & Microbiology Labs', 50, 1500),
(2, 'MSL', 'Medical Subject Laboratories', 'Physiology, Pathology & Microbiology Labs', 100, 2500),

(2, 'MSL', 'Medical Subject Laboratories', 'Biochemistry & Pharmacology Labs', 50, 1500),
(2, 'MSL', 'Medical Subject Laboratories', 'Biochemistry & Pharmacology Labs', 100, 2500),

-- =====================================================
-- CLINICAL LABORATORIES
-- =====================================================

(2, 'CL', 'Clinical Laboratories', 'Prosthodontics Labs', 50, 1300),
(2, 'CL', 'Clinical Laboratories', 'Prosthodontics Labs', 100, 2500),

(2, 'CL', 'Clinical Laboratories', 'Conservative Dentistry Labs', 50, 300),
(2, 'CL', 'Clinical Laboratories', 'Conservative Dentistry Labs', 100, 600),

(2, 'CL', 'Clinical Laboratories', 'Oral Pathology Histopathology Lab', 50, 400),
(2, 'CL', 'Clinical Laboratories', 'Oral Pathology Histopathology Lab', 100, 600),

(2, 'CL', 'Clinical Laboratories', 'Haematology & Clinical Biochemistry Lab', 50, 200),
(2, 'CL', 'Clinical Laboratories', 'Haematology & Clinical Biochemistry Lab', 100, 300);


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

    RequiredAreaSqFt DECIMAL(10,2) NOT NULL,

    ExistingAreaSqFt DECIMAL(10,2) NULL,

    IsActive BIT NOT NULL DEFAULT 1,

    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),

    UpdatedOn DATETIME NULL,

    CONSTRAINT FK_DentalPreClinicalAndSkillsLabAreaReq_LabId
        FOREIGN KEY (LabId)
        REFERENCES MstDentalPreClinicalAndSkillsLaboratoryAreaReq(Id)
);
GO

--EXEC sp_rename 'DentalPreClinicalAndSkillsLabAreaReq.RequiredAreaSqM', 'RequiredAreaSqFt', 'COLUMN';



--EXEC sp_rename 'DentalPreClinicalAndSkillsLabAreaReq.ExistingAreaSqM', 'ExistingAreaSqFt', 'COLUMN';

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

SELECT * FROM DesignationMaster WHERE FacultyCode=2;

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


-------------------------------------------------

CREATE TABLE MstDentalBedDistribution
(
    Id int IDENTITY(1, 1) PRIMARY KEY CLUSTERED,
    FacultyCode INT NOT NULL,
    SeatSlab INT NOT NULL,
    WardName VARCHAR(200) NOT NULL,
    BedRequirement INT NOT NULL,

    CONSTRAINT Fk_DentalBedDistribution_FacultyCode 
        FOREIGN KEY(FacultyCode)
        REFERENCES Faculty(FacultyId)
);


INSERT INTO MstDentalBedDistribution
(FacultyCode, SeatSlab, WardName, BedRequirement)
VALUES
-- Seat Slab 50
(2, 50, 'General Medical Ward', 30),
(2, 50, 'General Surgical Ward', 30),
(2, 50, 'Private Ward (A/C & Non A/C)', 9),
(2, 50, 'Maternity Ward', 15),
(2, 50, 'Pediatric Ward', 6),
(2, 50, 'Intensive Care Services', 4),
(2, 50, 'Critical Care/Emergency Beds', 6),

-- Seat Slab 100
(2, 100, 'General Medical Ward', 30),
(2, 100, 'General Surgical Ward', 30),
(2, 100, 'Private Ward (A/C & Non A/C)', 9),
(2, 100, 'Maternity Ward', 15),
(2, 100, 'Pediatric Ward', 6),
(2, 100, 'Intensive Care Services', 4),
(2, 100, 'Critical Care/Emergency Beds', 6);

------------------------------------------------

CREATE TABLE DentalWardBedDistribution
(
    Id INT IDENTITY(1, 1) PRIMARY KEY CLUSTERED,
    FacultyCode INT NOT NULL,
    CollegeCode NVARCHAR(100) NOT NULL,
    HospitalDetailsId INT NOT NULL,
    AffiliationTypeId INT NOT NULL,
    SeatSlab INT NOT NULL,
    WardId INT NOT NULL,
    WardName VARCHAR(200) NULL,
    BedsRequired INT NOT NULL,
    BedsPresent INT NOT NULL,

    CONSTRAINT Fk_DentalWard_FacultyCode 
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT Fk_DentalWard_ColCode
        FOREIGN KEY (CollegeCode)
        REFERENCES Affiliation_College_Master(CollegeCode),

    CONSTRAINT FK_DentalWard_HospitalDetailsId
        FOREIGN KEY (HospitalDetailsId)
        REFERENCES HospitalDetailsForAffiliation(HospitalDetailsID),

    CONSTRAINT FK_DentalWard_WardId
        FOREIGN KEY (WardId)
        REFERENCES MstDentalBedDistribution(Id)

);

SELECT * FROM DentalWardBedDistribution;

CREATE TABLE MstDentalInfrastructure
(
    Id INT IDENTITY(1, 1) PRIMARY KEY CLUSTERED,
    FacultyCode INT NOT NULL,
    SlNo INT NOT NULL,
    RequirementName VARCHAR(500) NOT NULL,
    RequirementDescription VARCHAR(MAX) NULL,
    SeatSlab INT NOT NULL,
    RequiredAreaSqFt DECIMAL(10,2) NOT NULL,

    CONSTRAINT Fk_MstDentalInfrastructre_FacCode
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId)
);

INSERT INTO MstDentalInfrastructure
(
    FacultyCode,
    SlNo,
    RequirementName,
    RequirementDescription,
    SeatSlab,
    RequiredAreaSqFt
)
VALUES

-- 1. Administrative Block
(2, 1, 'Administrative Block',
 'Dean’s room, Administrative Officer’s room, Meeting room, Office, Office stores, Pantry etc.',
 50, 2000),

(2, 1, 'Administrative Block',
 'Dean’s room, Administrative Officer’s room, Meeting room, Office, Office stores, Pantry etc.',
 100, 3000),

-- 2. Library
(2, 2, 'Library',
 'Reception & waiting, Property counter, Issue counter, Photocopying area, Reading room (50% students), PG & staff reading room, Journal room, Audio-visual room, Chief librarian room, Stores & stocking area',
 50, 4500),

(2, 2, 'Library',
 'Reception & waiting, Property counter, Issue counter, Photocopying area, Reading room (50% students), PG & staff reading room, Journal room, Audio-visual room, Chief librarian room, Stores & stocking area, E-Consortium connected with National Medical Library',
 100, 8000),

-- 3. Lecture Halls
(2, 3, 'Lecture Halls',
 '4 halls – Seating for 10% more than intake, blackboard, microphone, slide/OHP/multimedia facilities',
 50, 3200),

(2, 3, 'Lecture Halls',
 '4 halls – Seating for 10% more than intake, blackboard, microphone, slide/OHP/multimedia facilities',
 100, 6400),

-- 4. Central Stores
(2, 4, 'Central Stores',
 NULL,
 50, 400),

(2, 4, 'Central Stores',
 NULL,
 100, 800),

-- 5. Maintenance Room
(2, 5, 'Maintenance Room',
 NULL,
 50, 600),

(2, 5, 'Maintenance Room',
 NULL,
 100, 1000),

-- 6. Photography & Artist Room
(2, 6, 'Photography & Artist Room',
 NULL,
 50, 250),

(2, 6, 'Photography & Artist Room',
 NULL,
 100, 400),

-- 7. Medical Stores
(2, 7, 'Medical Stores',
 NULL,
 50, 200),

(2, 7, 'Medical Stores',
 NULL,
 100, 300),

-- 8. Amenities Area
(2, 8, 'Amenities Area',
 'Boys’ & Girls’ locker rooms, common rooms, teaching & non-teaching staff rooms, change rooms',
 50, 2000),

(2, 8, 'Amenities Area',
 'Boys’ & Girls’ locker rooms, common rooms, teaching & non-teaching staff rooms, change rooms',
 100, 3200),

-- 9. Compressor & Gas Plant Room
(2, 9, 'Compressor & Gas Plant Room',
 NULL,
 50, 200),

(2, 9, 'Compressor & Gas Plant Room',
 NULL,
 100, 300),

-- 10. Pollution Control Measures
(2, 10, 'Pollution Control Measures',
 'Incineration plant, sewage treatment plant, landscaping etc.',
 50, 300),

(2, 10, 'Pollution Control Measures',
 'Incineration plant, sewage treatment plant, landscaping etc.',
 100, 300),

-- 11. Cafeteria
(2, 11, 'Cafeteria',
 NULL,
 50, 800),

(2, 11, 'Cafeteria',
 NULL,
 100, 1500),

-- 12. Examination Hall
(2, 12, 'Examination Hall',
 '125 students',
 50, 1800),

(2, 12, 'Examination Hall',
 '250 students',
 100, 3600),

-- 13. Hostels
(2, 13, 'Hostels',
 'For all boys & girls within campus',
 50, 3600),

(2, 13, 'Hostels',
 'For all boys & girls within campus',
 100, 3600),

-- 14. Staff Quarters
(2, 14, 'Staff Quarters',
 'For all teaching & non-teaching staff',
 50, 3600),

(2, 14, 'Staff Quarters',
 'For all teaching & non-teaching staff',
 100, 3600),

-- 15. Play Ground
(2, 15, 'Play Ground',
 'Indoor & outdoor facilities',
 50, 3600),

(2, 15, 'Play Ground',
 'Indoor & outdoor facilities',
 100, 3600),

-- 16. Auditorium
(2, 16, 'Auditorium',
 'Capacity: 400 persons (This includes the seats, stage, lobby, and restrooms)',
 50, 7200),

(2, 16, 'Auditorium',
 'Capacity: 500 persons (This includes the seats, stage, lobby, and restrooms)',
 100, 9000);

 SELECT * FROM MstDentalInfrastructure;

 CREATE TABLE DentalInfrastructure
 (
    Id INT IDENTITY(1, 1) PRIMARY KEY CLUSTERED,
    FacultyCode INT NOT NULL,
    AffiliationTypeId INT NOT NULL,
    CollegeCode NVARCHAR(100) NOT NULL,
    HospitalDetailsId INT NOT NULL,
    RequirementId INT NOT NULL,
    SeatSlab INT NOT NULL,
    RequiredAreaSqFt DECIMAL(10, 2) NOT NULL,
    AvailableAreaSqFt DECIMAL(10, 2) NOT NULL,
    CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
    ModifiedOn DATETIME NULL,

    CONSTRAINT Fk_DentalInfra_FacCode
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId),

    CONSTRAINT Fk_DentalInfra_ColCode
        FOREIGN KEY (Collegecode)
        REFERENCES [dbo].[Affiliation_College_Master](CollegeCode),

    CONSTRAINT Fk_DentalInfra_HospitalDetailsId
        FOREIGN KEY (HospitalDetailsId)
        REFERENCES [dbo].[HospitalDetailsForAffiliation]([HospitalDetailsID]),

    CONSTRAINT Fk_DentalInfra_ReqId
        FOREIGN KEY (RequirementId)
        REFERENCES MstDentalInfrastructure(Id),

    CONSTRAINT Fk_DentalInfra_AffType
        FOREIGN KEY (AffiliationTypeId)
        REFERENCES [dbo].[TypeOfAffiliation](TypeId)
 );

 ALTER TABLE Affiliation_College_Master
 ADD Status bit NULL DEFAULT 0;
 
 --DELETE FROM MstDentalInfrastructure

 SELECT * FROM DentalInfrastructure;

 -----------------------20 MAY 2026 - REVIEWED -------------

 SELECT * FROM [dbo].[TypeOfOrganizationMaster]
 
 select * from [dbo].[Mst_InstitutionType]
 WHERE FacultyId IS NULL;

 UPDATE [Mst_InstitutionType]
 SET FacultyId = 1
 WHERE FacultyId IS NULL;

 INSERT INTO [dbo].[Mst_InstitutionType]
 (InstitutionType, OrganizationCategory, FacultyId)
 VALUES
 ('Government/Autonomous','G', 2),
 ('Private','P',2),
 ('Trust/Society','P',2),
 ('Defence','G',2),
 ('Missionary','p',2),
 ('N.G.O','P',2),
 ('Voluntary','P',2),
 ('Municipal Corporation','P',2),
 ('Others','P',2);


 SELECT * FROM [dbo].[MST_MedicalCourseType]

 ALTER TABLE [dbo].[MST_MedicalCourseType]
 ADD FacultyCode INT NOT NULL DEFAULT 1;

 EXEC sp_helpindex 'MST_MedicalCourseType';

 -- Drop existing unique constraint
--ALTER TABLE MST_MedicalCourseType
--DROP CONSTRAINT UQ__MST_Medi__3CFBF7726D5766A0;

--ALTER TABLE MST_MedicalCourseType
--DROP CONSTRAINT UQ__MST_Medi__3CFBF772D6C791D9;

ALTER TABLE MST_MedicalCourseType
ADD CONSTRAINT UQ_MST_MedicalCourseType_CourseTypeName_FacultyCode
UNIQUE (CourseTypeName, FacultyCode);
 
INSERT INTO [MST_MedicalCourseType]
(
    CourseTypeName, Description, IsUG, IsPG, IsSS, Status, CreatedDate, ModifiedDate, FacultyCode
)
VALUES
('UG', 'Under Graduate courses only', 1, 0, 0, 'Active', GETDATE(), GETDATE(), 2),
('PG', 'Post Graduate courses only', 0, 1, 0, 'Active', GETDATE(), GETDATE(), 2),
('SS', 'Super Specialty courses only', 0, 0, 1, 'Active', GETDATE(), GETDATE(), 2),
('UG and PG', 'Under Graduate and Post Graduate courses', 1, 1, 0, 'Active', GETDATE(), GETDATE(), 2),
('UG, PG and SS', 'All course types - UG, PG and Super Specialty', 1, 1, 1, 'Active', GETDATE(), GETDATE(), 2),
('PG and SS', 'Post Graduate and Super Specialty courses', 0, 1, 1, 'Active', GETDATE(), GETDATE(), 2),
('Diploma', 'Diploma courses', 0, 1, 1, 'Active', GETDATE(), GETDATE(), 2),
('PhD', 'PhD courses', 0, 1, 1, 'Active', GETDATE(), GETDATE(), 2);

SELECT * FROM [dbo].[InstitutionBasicDetails];

ALTER TABLE [dbo].[InstitutionBasicDetails]
ADD DCIcertificateFilePath VARCHAR(500) NULL;

ALTER TABLE [dbo].[InstitutionBasicDetails]
ADD KSDCcertificateFilePath VARCHAR(500) NULL;

ALTER TABLE [dbo].[InstitutionBasicDetails]
ADD OtherDentalCollegeInCity bit NULL;

ALTER TABLE [dbo].[Aff_DeanOrDirectorDetails]
ADD RecognizedByDCI BIT NULL;

ALTER TABLE [dbo].[Aff_PrincipalDetails]
ADD RecognizedByDCI BIT NULL;

ALTER TABLE [dbo].[Medical_DepartmentOfficesMeu]
ADD HasDentalEducationUnit BIT NULL,
    DentalEducationUnitAreaSqm DECIMAL(10, 2) NULL,
    DentalEducationUnitHasAudioVisual BIT NULL,
    DentalEducationUnitHasInternet BIT NULL;

--ALTER TABLE [dbo].[Medical_DepartmentOfficesMeu]
--ALTER COLUMN DentalEducationUnitAreaSqm DECIMAL(10, 2) NULL;

ALTER TABLE [dbo].[Medical_DepartmentOfficesMeu]
ADD DeuCoordinatorName NVARCHAR(200) NULL,
    DeuCoordinatorDesignationDepartment NVARCHAR(300) NULL,
    DeuCoordinatorPhone NVARCHAR(50) NULL,
    DeuCoordinatorEmail NVARCHAR(150) NULL,
    DeuMembersListDescription NVARCHAR(MAX) NULL,
    DeuActivitiesLastAcademicYear NVARCHAR(MAX) NULL,
    DeuMembersListFilePath NVARCHAR(500) NULL;

INSERT INTO Mst_Course
(
    Id,
    CourseCode,
    CourseName,
    FacultyCode,
    CourseLevel,
    CoursePrefix,
    SubjectName
)
VALUES
(
    437,
    2014,
    'Phd',
    2,
    'Phd',
    'Phd',
    'Phd'
);


INSERT INTO Mst_Course
(
    Id,
    CourseCode,
    CourseName,
    FacultyCode,
    CourseLevel,
    CoursePrefix,
    SubjectName
)
VALUES
(
    438,
    2015,
    'Fellowship',
    2,
    'Fellowship',
    'Fellowship',
    'Fellowship'
);


INSERT INTO Mst_Course
(
    Id,
    CourseCode,
    CourseName,
    FacultyCode,
    CourseLevel,
    CoursePrefix,
    SubjectName
)
VALUES
(
    439,
    2016,
    'Phd and Fellowship',
    2,
    'Phd and Fellowship',
    'Phd and Fellowship',
    'Phd and Fellowship'
);

---------22 - MAY - 2026------

--CREATE TABLE [dbo].[UGDesignationMaster](
--    [ID] [int] NOT NULL,
--    [DesignationID] [varchar](10) NOT NULL,
--    [DesignationName] [varchar](100) NOT NULL,
--    [Order] [int] NOT NULL,
--PRIMARY KEY CLUSTERED 
--(
--    [ID] ASC
--)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
--) ON [PRIMARY]

--DROP TABLE UGDesignationMaster

select * from DesignationMaster

GO
/****** Object:  Table [dbo].[UG_Faculty_Details]    Script Date: 22-May-26 9:58:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UG_Faculty_Details](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [FacultyCode] [int] NULL,
    [CollegeCode] [varchar](50) NULL,
    [DepartmentCode] [varchar](50) NULL,
    [NameOftheFaculty] [varchar](200) NULL,
    [DesignationCode] [varchar](50) NULL,
    [DOB] [varchar](20) NULL,
    [DateOfAppointment] [varchar](20) NULL,
    [AadhaarNo] [nvarchar](20) NULL,
    [PANNo] [nvarchar](20) NULL,
    [MobileNo] [nvarchar](20) NOT NULL,
    [EmailId] [nvarchar](200) NULL,
    [StateCouncilRegNo] [nvarchar](200) NULL,
    [AEBASAttendId] [nvarchar](100) NULL,
    [ProfessionalQualification] [nvarchar](100) NULL,
    [NatureOfEmployment] [nvarchar](100) NULL,
    [TeachingExpInYrs] [nvarchar](100) NULL,
    [PhotoFilePath] [nvarchar](500) NULL,
    [CreatedOn] [datetime] NULL,
    [IPAddress] [varchar](50) NULL,
    [IsDeclared] [bit] NULL,
    [PrincipalName] [varchar](200) NULL,
    [PrintedCopyUploaded] [bit] NULL,
    [RowTimestamp] [timestamp] NOT NULL,
 CONSTRAINT [PK_UG_Faculty_Details] PRIMARY KEY CLUSTERED 
(
    [MobileNo] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UG_Printed_Upload]    Script Date: 22-May-26 9:58:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UG_Printed_Upload](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [FacultyCode] [int] NULL,
    [CollegeCode] [nvarchar](100) NULL,
    [DocumentPath] [nvarchar](100) NULL,
    [CreatedOn] [datetime] NULL,
    [IPAddress] [varchar](50) NULL,
 CONSTRAINT [PK__UG_Print__3214EC27586625B2] PRIMARY KEY CLUSTERED 
(
    [ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UGDesignationMaster]    Script Date: 22-May-26 9:58:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UGDesignationMaster](
    [ID] [int] NOT NULL,
    [DesignationID] [varchar](10) NOT NULL,
    [DesignationName] [varchar](100) NOT NULL,
    [Order] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
    [ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[UG_Faculty_Details] ON 

INSERT [dbo].[UG_Faculty_Details] ([ID], [FacultyCode], [CollegeCode], [DepartmentCode], [NameOftheFaculty], [DesignationCode], [DOB], [DateOfAppointment], [AadhaarNo], [PANNo], [MobileNo], [EmailId], [StateCouncilRegNo], [AEBASAttendId], [ProfessionalQualification], [NatureOfEmployment], [TeachingExpInYrs], [PhotoFilePath], [CreatedOn], [IPAddress], [IsDeclared], [PrincipalName], [PrintedCopyUploaded]) VALUES (3, NULL, N'M001', N'MD010', N'KARTHIK', N'D007', N'1980-02-01', N'2005-01-01', N'', N'GDFHD4576E', N'8102102101', N'abc123@gmail.com', N'5345346435', N'75457547', N'MD,MS,MBBS', N'Permanent', N'12', N'/MedicalUGFacultyList/Photos/98411214-0f74-4b28-909b-341158ea192f.jpeg', CAST(N'2026-05-21T15:18:20.197' AS DateTime), N'::1', 1, NULL, 1)
INSERT [dbo].[UG_Faculty_Details] ([ID], [FacultyCode], [CollegeCode], [DepartmentCode], [NameOftheFaculty], [DesignationCode], [DOB], [DateOfAppointment], [AadhaarNo], [PANNo], [MobileNo], [EmailId], [StateCouncilRegNo], [AEBASAttendId], [ProfessionalQualification], [NatureOfEmployment], [TeachingExpInYrs], [PhotoFilePath], [CreatedOn], [IPAddress], [IsDeclared], [PrincipalName], [PrintedCopyUploaded]) VALUES (2, NULL, N'M001', N'MD002', N'DHANANJAYA A S', N'D001', N'1985-09-01', N'2011-01-01', N'', N'BWDPD4686F', N'8660119707', N'abc123@gmail.com', N'5345346435', N'54645645', N'MD,MS,MBBS', N'Permanent', N'12', N'/MedicalUGFacultyList/Photos/a09658b3-4dee-4563-b8d1-71aa2a9db2d1.png', CAST(N'2026-05-21T13:28:33.663' AS DateTime), N'::1', 1, NULL, 1)
INSERT [dbo].[UG_Faculty_Details] ([ID], [FacultyCode], [CollegeCode], [DepartmentCode], [NameOftheFaculty], [DesignationCode], [DOB], [DateOfAppointment], [AadhaarNo], [PANNo], [MobileNo], [EmailId], [StateCouncilRegNo], [AEBASAttendId], [ProfessionalQualification], [NatureOfEmployment], [TeachingExpInYrs], [PhotoFilePath], [CreatedOn], [IPAddress], [IsDeclared], [PrincipalName], [PrintedCopyUploaded]) VALUES (1, NULL, N'M001', N'MD009', N'Chandan', N'D006', N'1980-01-01', N'2010-01-01', N'458948449744', N'GDFHD4576E', N'8867656063', N'abc123@gmail.com', N'11111111111', N'5345345345', N'MD,MS,MBBS', N'Permanent', N'12', N'/MedicalUGFacultyList/Photos/d211088c-237a-4600-89bd-fab9b5d23be4.jpg', CAST(N'2026-05-21T13:05:41.893' AS DateTime), N'::1', 1, NULL, 1)
INSERT [dbo].[UG_Faculty_Details] ([ID], [FacultyCode], [CollegeCode], [DepartmentCode], [NameOftheFaculty], [DesignationCode], [DOB], [DateOfAppointment], [AadhaarNo], [PANNo], [MobileNo], [EmailId], [StateCouncilRegNo], [AEBASAttendId], [ProfessionalQualification], [NatureOfEmployment], [TeachingExpInYrs], [PhotoFilePath], [CreatedOn], [IPAddress], [IsDeclared], [PrincipalName], [PrintedCopyUploaded]) VALUES (4, NULL, N'M001', N'MD001', N'ABHILASH', N'D001', N'1977-01-01', N'1998-11-01', N'', N'FGDGS5416F', N'9867545344', N'abc123@gmail.com', N'5345346435', N'34214234', N'MD,MS', N'Permanent', N'13', N'/MedicalUGFacultyList/Photos/92aaa8fa-b4fd-41b0-8284-880a22afc58e.jpeg', CAST(N'2026-05-21T15:22:17.453' AS DateTime), N'::1', 1, NULL, 1)
SET IDENTITY_INSERT [dbo].[UG_Faculty_Details] OFF
GO
SET IDENTITY_INSERT [dbo].[UG_Printed_Upload] ON 

INSERT [dbo].[UG_Printed_Upload] ([ID], [FacultyCode], [CollegeCode], [DocumentPath], [CreatedOn], [IPAddress]) VALUES (1, NULL, N'M001', N'/MedicalUGFacultyList/4e44e8ea-c640-4f93-af4b-e98aa3d86046.pdf', CAST(N'2026-05-21T18:24:34.590' AS DateTime), N'::1')
SET IDENTITY_INSERT [dbo].[UG_Printed_Upload] OFF
GO
INSERT [dbo].[UGDesignationMaster] ([ID], [DesignationID], [DesignationName], [Order]) VALUES (1, N'D001', N'Professor', 1)
INSERT [dbo].[UGDesignationMaster] ([ID], [DesignationID], [DesignationName], [Order]) VALUES (2, N'D002', N'Associate Professor', 2)
INSERT [dbo].[UGDesignationMaster] ([ID], [DesignationID], [DesignationName], [Order]) VALUES (3, N'D003', N'Assistant Professor', 3)
INSERT [dbo].[UGDesignationMaster] ([ID], [DesignationID], [DesignationName], [Order]) VALUES (4, N'D004', N'Tutor Cum PG', 4)
INSERT [dbo].[UGDesignationMaster] ([ID], [DesignationID], [DesignationName], [Order]) VALUES (5, N'D005', N'Tutor Senior Resident', 5)
INSERT [dbo].[UGDesignationMaster] ([ID], [DesignationID], [DesignationName], [Order]) VALUES (6, N'D006', N'Junior Resident Cum PG', 6)
INSERT [dbo].[UGDesignationMaster] ([ID], [DesignationID], [DesignationName], [Order]) VALUES (7, N'D007', N'Junior Resident', 7)
GO
ALTER TABLE [dbo].[UG_Faculty_Details] ADD  CONSTRAINT [DF__UG_Facult__Creat__7EC1CEDB]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[UG_Faculty_Details] ADD  CONSTRAINT [DF__UG_Facult__IsDec__7FB5F314]  DEFAULT ((0)) FOR [IsDeclared]
GO
ALTER TABLE [dbo].[UG_Faculty_Details] ADD  CONSTRAINT [DF__UG_Facult__Print__00AA174D]  DEFAULT ((0)) FOR [PrintedCopyUploaded]
GO
ALTER TABLE [dbo].[UG_Printed_Upload] ADD  CONSTRAINT [DF__UG_Printe__Creat__038683F8]  DEFAULT (getdate()) FOR [CreatedOn]
GO

------------------------------
GO
/****** Object:  Table [dbo].[DepartmentMastersForUG]    Script Date: 22-May-26 11:28:04 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DepartmentMastersForUG](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [FacultyCode] [int] NOT NULL,
    [DepartmentCode] [varchar](50) NOT NULL,
    [DepartmentName] [varchar](100) NOT NULL
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[DepartmentMastersForUG] ON 

INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (1, 1, N'MD001', N'Anatomy')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (2, 1, N'MD002', N'Physiology')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (3, 1, N'MD003', N'Biochemistry')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (4, 1, N'MD004', N'Pharmacology')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (5, 1, N'MD005', N'Pathology')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (6, 1, N'MD006', N'Microbiology')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (7, 1, N'MD007', N'Forensic Medicine and Toxicology')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (8, 1, N'MD008', N'Community Medicine')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (9, 1, N'MD009', N'General Medicine')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (10, 1, N'MD010', N'Pediatrics')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (11, 1, N'MD011', N'Dermatology')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (12, 1, N'MD012', N'Psychiatry')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (13, 1, N'MD013', N'General Surgery')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (14, 1, N'MD014', N'Orthopedics')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (15, 1, N'MD015', N'Otorhinolaryngology (ENT)')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (16, 1, N'MD016', N'Ophthalmology')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (17, 1, N'MD017', N'Obstetrics and Gynecology')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (18, 1, N'MD018', N'Anesthesiology')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (19, 1, N'MD019', N'Radiodiagnosis')
INSERT [dbo].[DepartmentMastersForUG] ([Id], [FacultyCode], [DepartmentCode], [DepartmentName]) VALUES (20, 1, N'MD020', N'Dentistry')
SET IDENTITY_INSERT [dbo].[DepartmentMastersForUG] OFF
GO

-----------------------------

USE [Admission_Affiliation]
GO
/****** Object:  Table [dbo].[LIC_ModeofTravel]    Script Date: 22-May-26 10:45:39 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LIC_ModeofTravel](
    [id] [int] IDENTITY(1,1) NOT NULL,
    [Modeoftravel] [nvarchar](150) NULL,
PRIMARY KEY CLUSTERED 
(
    [id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[LIC_ModeofTravel] ON 
GO
INSERT [dbo].[LIC_ModeofTravel] ([id], [Modeoftravel]) VALUES (1, N'Road')
GO
INSERT [dbo].[LIC_ModeofTravel] ([id], [Modeoftravel]) VALUES (2, N'Air')
GO
INSERT [dbo].[LIC_ModeofTravel] ([id], [Modeoftravel]) VALUES (3, N'Train')
GO
INSERT [dbo].[LIC_ModeofTravel] ([id], [Modeoftravel]) VALUES (4, N'Others')
GO
SET IDENTITY_INSERT [dbo].[LIC_ModeofTravel] OFF
GO

-------------------------------------------

--DROP TABLE [LIC_Inspection]

USE [Admission_Affiliation]
GO
/****** Object:  Table [dbo].[LIC_Inspection]    Script Date: 22-May-26 10:34:05 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LIC_Inspection](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[TypeofMember] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](150) NOT NULL,
	[DOB] [date] NULL,
	[PhoneNumber] [nvarchar](20) NULL,
	[Email] [nvarchar](150) NULL,
	[Address] [nvarchar](500) NULL,
	[PANNumber] [nvarchar](10) NULL,
	[AadhaarNumber] [nvarchar](12) NULL,
	[AccountHolderName] [nvarchar](100) NULL,
	[AccountNumber] [nvarchar](40) NULL,
	[IFSCCode] [nvarchar](11) NULL,
	[BankName] [nvarchar](100) NULL,
	[CreatedPassword] [nvarchar](100) NULL,
	[BranchName] [nvarchar](150) NULL,
	[CollegeName] [nvarchar](200) NULL,
	[DateOfInspection] [date] NULL,
	[AttendenceDoc] [varbinary](max) NULL,
	[IsCompleted] [bit] NULL,
	[AttendanceFilePath] [nvarchar](500) NULL,
	[ModeOfTravel] [nvarchar](100) NULL,
	[FromPlace] [nvarchar](200) NULL,
	[ToPlace] [nvarchar](200) NULL,
	[Return_fromPlace] [nvarchar](50) NULL,
	[Return_ToPlace] [nvarchar](50) NULL,
	[ReturnKilometers] [nvarchar](50) NULL,
	[Kilometers] [float] NULL,
	[TotalCost] [decimal](18, 2) NULL,
	[MemberCode] [varchar](50) NULL,
	[collegeCode] [nvarchar](150) NULL,
	[designationCode] [nvarchar](150) NULL,
	[departmentCode] [nvarchar](150) NULL,
	[Facultycode] [nvarchar](20) NULL,
 CONSTRAINT [PK__LIC_Insp__3214EC0799265EB8] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[LIC_Inspection] ON 

INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (1, N'Academic Council', N'DR. SRINIVASULU NAIDU. S', NULL, N'9880656516', N'srnvslnaidu@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0001', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (2, N'Academic Council', N'DR. B.V MARUTHI PRASAD', NULL, N'9980145271', N'maruthibio2007@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0002', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (3, N'Academic Council', N'DR. S. R JAGANNATHA', NULL, N'9886334494', N'drjagannathasr@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0003', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (4, N'Academic Council', N'DR. SIDDIQ M. AHMED', NULL, N'9449019175', N'siddique_69@yahoo.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0004', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (5, N'Academic Council', N'DR. MOHAN APPAJI', NULL, N'9845471569', N'coorgnanda@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0005', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (6, N'Academic Council', N'DR. MANJUNATHASWAMY. R', NULL, N'9845504280', N'somagudhimanju@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0006', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (7, N'Academic Council', N'DR. JAYASHREE KHARGE', NULL, N'9844036363', N'khargej@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0007', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (8, N'Academic Council', N'DR. HEMANTH. M', NULL, N'9845459666', N'drhemanth@yahoo.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0008', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (9, N'Academic Council', N'DR. PRITHAM N SHETTY', NULL, N'9008400200', N'prithamnshetty@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0009', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (10, N'Academic Council', N'DR. LEELADHAR D V', NULL, N'9449717171', N'drleeladhar_dv@rediffmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0010', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (11, N'Academic Council', N'DR. SIDDANAGOUDA A PATIL', NULL, N'9448565198', N'drsapatilms@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0011', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (12, N'Academic Council', N'DR. ZAKIR HUSSAIN.V', NULL, N'9342123016', N'vzhumc@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0012', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (13, N'Academic Council', N'PROF. MOHD. ALEEMUDDIN QUAMRI', NULL, N'9341072974', N'drmaquamri@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0013', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (14, N'Academic Council', N'DR. SIDDARAM S PATIL', NULL, N'8880788800', N'drsiddaram@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0014', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (15, N'Academic Council', N'DR. ANAND J HOSUR', NULL, N'9449065665', N'ajhosur@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0015', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (16, N'Academic Council', N'DR. VANITHA S. SHETTY', NULL, N'9008004542', N'drvanithashetty16@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0016', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (17, N'Academic Council', N'DR. SALEEMULLA KHAN', NULL, N'9916877900', N'saleemulla@pacp.co.in', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0017', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (18, N'Academic Council', N'PROF. LINGARAJ.S DANKI', NULL, N'9590054297', N'lsdanki@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0018', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (19, N'Academic Council', N'DR. CHANDRASHEKHAR M.V', NULL, N'9880298342', N'chandupharm@yahoo.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0019', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (20, N'Academic Council', N'PROF. DR. THIPPESWAMY T M J', NULL, N'9964186525', N'thippesh.togalerimuth@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0020', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (21, N'Academic Council', N'PROF. (DR). A.T.S GIRI', NULL, N'9845022057', N'bospgn2427@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0021', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (22, N'Academic Council', N'PROF. V R AYYAPPAN', NULL, N'9886291325', N'ayyappan28@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0022', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (23, N'Academic Council', N'PROF. MOHAMMAD SUHAIL', NULL, N'9663407439', N'msuhail@kanachur.edu.in', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0023', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (24, N'Academic Council', N'DR. NETHRA. S. S', NULL, N'9448171403', N'nethra.surhonne@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0024', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (25, N'Academic Council', N'DR. SHRUTHI H.P', NULL, N'9535504757', N'1111.shruthi@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0025', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (26, N'Academic Council', N'DR. BAILAPPA A KOLHAR', NULL, N'9448102222', N'drbasavarajk@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0026', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (27, N'Academic Council', N'DR. SUNIL V DHADED', NULL, N'9844101555', N'sunil@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'$2a$11$J2/7a/G5V7LRK.lMQFP9LOYZf1IKA7fxlqJZPQ4KhobmEDIwD0gum', NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0027', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (28, N'Academic Council', N'DR. ROOPA K T', NULL, N'9880380503', N'roopakt25@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0028', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (29, N'Academic Council', N'Prof. CHANDRASHEKAR S SAWAN', NULL, N'8503960808', N'cssawan888@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0029', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (30, N'Academic Council', N'DR. GURUBASAVARAJA SWAMY P M', NULL, N'9886001140', N'gurubasavarajaswamy@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0030', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (31, N'Academic Council', N'DR. GEETA DOLLI', NULL, N'9886431714', N'drgeetamailare@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0031', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (32, N'Academic Council', N'DR. CHALAPATHY D V', NULL, N'9945000037', N'dvchalapathy@yahoo.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0032', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (33, N'Academic Council', N'DR. MAHESH G SALIMATH', NULL, N'9739106823', N'drmaheshsalimath@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0033', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (34, N'Academic Council', N'PROF.BASAYYA V H', CAST(N'2001-12-01' AS Date), N'8618339489', N'basu@gmail.com', NULL, N'DFGDK9963K', N'586552563225', N'PROF.BASAYYA V H', N'999993939933', N'IKJU88765', N'test', N'$2a$11$XlhnmEXqyXbq1FEPRAwfx.5JPOLftZMCzoZQNd/vX6GySWykfPL.O', N'test', NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0034', N'AH297', N'Statistician (Minimum
A.P. level)', N'of Hospital Administration', NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (35, N'Academic Council', N'PROF. SUDEENA VIJAYKUMAR', NULL, N'9740262494', N'sudeenavijaykumar@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0035', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (36, N'Academic Council', N'DR. PRAVIN G U', NULL, N'98444455599', N'drpravingu.rad@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0036', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (37, N'Academic Council', N'DR. AJAY KUMAR G', CAST(N'2001-02-12' AS Date), N'9448455269', N'g_kumarajay@hotmail.com', NULL, N'gdgdgfd', N'65698458651', N'DR. AJAY KUMAR G', N'999993939933', N'ikju88765', N'test', N'$2a$11$bqA2oZqH1nXkti1kG0YYfebyNRggfR78WXTINdvKvCdu5IEdOVWOS', N'test', N'test', CAST(N'2025-02-12' AS Date), 0x255044462D312E370D25E2E3CFD30D0A31332030206F626A0D3C3C2F4C696E656172697A656420312F4C203231353331372F4F2031352F45203231303934322F4E20322F54203231353031382F48205B20363837203232335D3E3E0D656E646F626A0D2020202020202020202020202020200D0A33382030206F626A0D3C3C2F4465636F64655061726D733C3C2F436F6C756D6E7320352F507265646963746F722031323E3E2F46696C7465722F466C6174654465636F64652F49445B3C31333044443637363832373635303442423337374339444432354142434332363E3C32354344463033303236304631383430414234373435414635324336423041433E5D2F496E6465785B3133203130355D2F4C656E677468203131382F50726576203231353031392F526F6F74203134203020522F53697A65203131382F547970652F585265662F575B31203320315D3E3E73747265616D0D0A68DE62626460106060626060AE03918CD52092290A44721D468830AF02B3BB4024CB5DB0482E982D0516D700B359C064328864DD0966D782D93660F5605235074832AE9300B1671980D8D5E920F2FE2D9088F021B069AA40F2EF6B250626A0DBE680448062A3E43021FF3330CDDF0C106000D4BD13850D0A656E6473747265616D0D656E646F626A0D7374617274787265660D0A300D0A2525454F460D0A20202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020200D0A3131372030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E677468203134312F4F203135312F532037363E3E73747265616D0D0A68DE6260606006222D06560606DE6F0C420C0820C4C002146561E058C070B2FE0003C31EA8B8484C8AE072F5430A40151D1D0D0C0C2C4012AC00085818985D4581B438104B80451E30083056713C6698CD788A6115D35FC65E662B060F6615E666E69BCC7399639862547AD65ACC08A86ABFFB4BF01183ABE21998F5AC40A3BE026946206E05083000D0E41FD90D0A656E6473747265616D0D656E646F626A0D31342030206F626A0D3C3C2F4C616E6728656E292F4D61726B496E666F3C3C2F4D61726B65642066616C73653E3E2F4F75746C696E6573203131203020522F5061676573203132203020522F547970652F436174616C6F672F566965776572507265666572656E636573203339203020523E3E0D656E646F626A0D31352030206F626A0D3C3C2F416E6E6F74735B3430203020522034312030205220343220302052203433203020522034342030205220343520302052203436203020522034372030205220343820302052203439203020522035302030205220353120302052203532203020522035332030205220353420302052203535203020522035362030205220353720302052203538203020522035392030205220363020302052203631203020522036322030205220363320302052203634203020525D2F436F6E74656E74735B323020302052203232203020522032342030205220323520302052203236203020522032372030205220323820302052203239203020525D2F43726F70426F785B30203020363132203739325D2F47726F75703C3C2F43532F4465766963655247422F532F5472616E73706172656E63792F547970652F47726F75703E3E2F4D65646961426F785B30203020363132203739325D2F506172656E74203132203020522F5265736F75726365733C3C2F4578744753746174653C3C2F475330203635203020523E3E2F466F6E743C3C2F43325F30203731203020522F43325F31203737203020522F43325F32203833203020522F43325F33203839203020522F43325F34203935203020522F43325F3520313031203020522F54543020313034203020522F54543120313037203020522F54543220313130203020522F54543320313133203020522F54543420313136203020523E3E2F50726F635365745B2F5044462F546578745D3E3E2F526F7461746520302F546162732F532F547970652F506167653E3E0D656E646F626A0D31362030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4669727374203633312F4C656E67746820323430312F4E2037382F547970652F4F626A53746D3E3E73747265616D0D0A68DEC45A5B6FDB381AFD2B7C6C318844F2E3753028906B9BDDB6B368D2ED02821EDC449B689BD881AD4EDB7FBFE7A32C59B13DAD9DB4F1834B52BC9DEFCA43A6148514460A6D855142D9288C163A6A6108357C34C2DA208C152E2A619C08FCD18BC823835032625C144A6B2FAC148A9C14160B197CB75A28C7DF4928EF94B046A8281D16145A62B07542AB880DBCD04451601F6D02DA5168A783C052DA7B2D305547E585D38224FE7124484BB48D20C226CE0A32D109E7045902528F32A03F080AA4801C1206273C24D590D22B2EA3F02CA5C7778860B09F371010227A0B11B028BA2C49942C8222E1034480F81E500D3E0688E0A51601A279C5AA8108001B88CB289228D8370092A42050853A300FD02C741E20A28B28239741A0EA8296028A7601E060052FA31550A5B70A6D034896758F12F820B2F7D8347A402323B04490504EC4D6129B2A89B5B5E60A4041C5A8002D41442509B0B0A192862B3CC602189B0A160A113A5250312A3C18D8948A6C6D80819994024AC2564A29C081EC8AF179B88982A2A26757508C28F29884008EA078A054A4C51F7FE447F5ECEE66F4ED6872715E37379568A69FAB172FD0B18FDF59FEFEDD697EFEEDAECAF72F9A7A32E6F6B3EBA6B99BFD9EE7577573FDF9637631B9CD3F8DA6CD75FDA92DF65E5E4F66CDF3172FF283332CF24148544F84C9DF55174DA1346590CDF998C1C795D5191CCD459B692AF3B3CF1F1BDEED753DFEF46B519890418D3D8A90ED02843599E9313895C14C3B00E13364991E85DB89269C1E68C21376DF058890F9852682CE10D54F8FC2870CA1D9A38075EC0E5040FA683B14DAB80CE9FCC95168A3333E713A14A9D8010A93C5DE22DA52A6760122647DD6C4F195F131F6E4201099B4401154C687E7D3A3F019CEF11E055A8F32C8972F5FB21B0CAF2EEB71C2528FF3D1DE786F0E692FEA40DA6B3DFA3B40443EF37DFA227617B75B404666A6B713C852A6778AC76899F5616C48A6D4B2533C2E65D50E90C9DC8E15049DA83EC51898EF7154E0270072035660E0D2BBF5685689EAE178FDC8F4F7783CE00A8B4C681E9D8F1F0FE85E6A3648CDB8E7ED1690CF649F142D3B78DC29208B34E8E20290CDE26E3584CB6746488B8E38FD588BD61CDD1A3C076FF2B793E9EDE8263FDC17AA0575FCB57979D68C9A2ABF18099586FD39BDACA6F5F8EAD9E965356EEAE6DB73EC7555CF9AE9B767FB97938FD5732C7D777753DDA29BE1B0A8B30B6E84A8F2FDBFAE3ED497CD35CE0C9F1F8EEE5E55F5D57503543E3FAADA617B5AB9FCE4667435C3F53F3F998C9B8383C9D7628FDF27B84F68890B2F6694A9F3A4BEA970269290E25DFAF076745BE5078747C7C7C7BF9DD7B7D5EC6DF5E5DDE47634FED7D9DEC1E4E6F2CD791AF6A1DDD94B999F36A39BFA627F7C85EBA9CC5F572328FC0A08F337A3AF2D5A8DCDF3B3A6BAFD377F4EBAE13518F2B4BE6B26D3FC3F7349B485CC852C70632FA940AB8C45202A95C2294EC089AAE5EF5CE5D1A58EDC53922B604B180BB77C945DDDE1E68DB5D28F421416B2739DFBA231ED586E2B555A4A4B58EB86CB9436F2E7D285822773AF49CF3D6DC930BAEF9A37E1F6BC1EE6F5797F196C91A600072DBACAA879E552799FCA92BD6934AB58433F30C4E1E9D1D9B719F47A3AFEEF841F76D888F8783E79797AF4667497775E961F7DE0770EB9A4767E03E2299D37632A0F60030DCC84B87081C7C1328E1FC2DE6D01B175CBCB113C0D8367FCBAC43B1E8F2F26EC263DC2BD573D0ADE58E6E793F7E31A832AA1D29E0B3C8F0D246FE52290AC568340E2AE4520A1B12E9024A53EA1B4913CE35E209995407A09E51C22423E4EEB61E49895C8E9C3457943F370B19B858B959EC345C3A17C84272177C1451116DA144ED95287C20444140A7ECD21551A7420B11A8F60828F8782542C1D56D2B2749A19217B7CB06061069E8B15952F432C1C6EE0D650190D2F55C2A9B0B24789F5901815A2D33AB4B5C20C7EFCF4A5C6EA06B0B4579CE49102802B68AEA37485890151EC0A92B624A58B20815463471D4B220D011C4A8CF3AA24E3D23C32B1D02411F60A25C639C683EFCEB7DF3D823922DE796CE07DA1A048ED5CDECBC41216282842150A255994C06E7D69C8A771C6B0BA9004A04063B13E62D658E8CCF13C8B706E331494546844082B3278AC1D586198EBD0073949A21D4C9233406F0407C20F712F0B17344A2AF0BD8C56A531D0A9618B8A0D7E18CC5905B3966372E876F7F384D75BE7094F9BE5096FE679C2DB75796288692531B8ED1343F8C98941874562E037FA616640DFE0885D9B19084981FB90192C9F1FE15E66B02B99E115D471565D4DAAF7A7C7B793FF6D9C1E9CF4F3F400901BE5078E6138AB372B6EB28261C957C2F6BE1237F39520E7BE12D43A5F5901B6EC30416FED30E47EAEC3DCA764727892B8483FA664AEA764FC1A10E99EBFA8157FF9C7EA317B9F8D99CDD89853A66363724B36E65AAAB2CCC6F8C84964ACA532DAA462C0C93A42C57C8C794FC7D1D23221F67D615EEF8858373F9132E672F33E70373E9F121303BB1A963D239332B130FE75753FAFCF7FCCC83A047AD095769F0F8910700D2B5B6F8BFBC113CCD6C113EC86C1E3BAE0F1EB82673DBA95080ADBA75CFF2BB918B2D9965C0C1463CEC51CAD7031BD1241AF17075022A8DFBFCA0C0859501D21A32D0999C3E9CF848CFFB20B42C3840CFC649990F9C4C4409C12F7A2C4BDF8C91214CB5B90AE9682914E140CC6B7AEA5609E5A0A063A92A857542262C344B9402912E5C21DD6A8D0522E6CC4940B1CB6A55C4CAD987231854A14CBB4140BE312C5E27383A955902DB5E236A8555088311EC3D48A424BADB8CD6B8286B5D42AB6D4CAF8965A05DB522B1696A9154885374CAD6CDAB7A5557E4EAB424BABC00A582B4CAB0CFD0D9D529C2CF50FE953C97F5D4D86598E9615AFB81FC6516E1DC6516D16C651CFC338D2BA305E01B61CC1D16C1DC15AFDDC088ED20ECE40A3B6E44CFC17FAC499D2B3C43267922B11FC062AD99FD6A39B4D4F3EA2E1C9E76C77F299CD4E3EA414264E702BC25961570F8221A025BFD9FE3E1E37BC8FC7EE3E1ED7DEC78798965D86FFBBC3D63E436B7C6637CF52273B7C966286B3A4E9EFC0E915FCA11EEF8F6775DF3EA9A7B3E6F07A34ED34317000FEAF272CF2EBD162486F95E9E7EABC33CFDC13588E59FA7F2AC91F76F6C871FACB1F3970962E2B7FB8E903B56D1FA46DB7ACED27BE08FC7337178135FEBF1EC9038D111E648CB86C8C273E91DE3EE589C427D1920986FB3F4CF14A3D44F14A7F5FF13E6EAB781DBEA378BFA2F83F3BC1B73A00EE6B5FF707807F98F657403CD004E64126B0AD09FE2FC00033E76AB70D0A656E6473747265616D0D656E646F626A0D31372030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E677468203330393E3E73747265616D0D0A789C6552CB6E833010BCFB2B7C4C0F1198479203424A692371E843A5FD00B0176AA918CB98037F5FE3A5499A58326876679835E3A0289F4A252D0DDECDC02BB0B4954A181887C970A00D745211965021B95D917FF2BED62470E26A1E2DF4A56A07926534F870CDD19A996E8E6268E081046F468091AAA39BAFA272B89AB4FE811E94A521C9732AA0751F7AA9F56BDD030DBC6C5B0AD79776DE3ACD85F1396BA091C70C87E1838051D71C4CAD3A2059E8564EB3935B3901256EFA11AA9A967FD7C6B363C70EC328CC3DDA218ABD7665B13FCDC5E2D1D3C202D987958DFDE4CEE284B4C25B3086E819111AC631A203A2F4DA3EBEB38F12A41DFD2B615E9B6031C5F3ECB098A658C441F7D1BF41D9EDA0290EBA2FAEED977FB8447D0E884FC6B86CFC7DF0A12C714805E72BA307BDA896FD0BDA37B3570D0A656E6473747265616D0D656E646F626A0D31382030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E677468203631323E3E73747265616D0D0A789C8555CBAEDA3010DDE72BBCBC5D5C91B1B10D128A040124167DA8B4ABAA0B480C8D544214C282BFAF33C7C04D90D2484974EC79CF197B946E969BB268C4E85B7DCEB6AE1187A2CC6B77395FEBCC89BD3B1665A448E445D604C4DFECB4ABA29157DEDE2E8D3B6DCAC3399ACDC4E8BBDFBC34F54DBCCDF3F3DE7D8A465FEBDCD54579146F3FD3ADC7DB6B55FD75275736228E9244E4EEE00D7DDE555F76272746ACF6BEC9FD7ED1DCDEBDCE53E2C7AD7242322604939D7377A97699AB77E5D145B3D83F8998ADFD9344AECC7BFB125AFBC3535C7971FF1B27E297FFCB98E19892DFED2E11439258956CF3AEFD3096FDD9D52C3D81D838619402E9A0C452F41280342C262DA44DC785EEBB90C1E8845DC835D094919A03CD8182E4026805940E07E3430F31F36FD90986FAC168C4ACE361A37A0A3184A7A96374FC6214916AC5711B04A4515283026BCDC8A275DA0CBBB7706F83FBC9604E14A3D586068D52CC9527025D4C9718D46706119C1B4E8308291A4E832492328691BAA3F0726B696C9FAB2956D2C78A85551D62B140C1E3046809046B06153173A0606901044A9961A29065869005794D9728B69FBE0DEED7EC62121A1203C1BD958CA648C22A202411129CAE037A24B2004F6D67C45E669C52364A2986D9A2D0A9C1222A6B2783A34D697A2791474AC3AF1C2E92C27C284F393E5BC27CCA953F5B062640858648AE9632208BE5B62B0B7ACA25503B1DD29366380ECB269445FD643755D5773F05032597494D17380AB8596A816AC9FF385C71066A8D32A97870E4C60AEC908B8F46DBB3BBBD621E174376AD6B7F27F03DC497417B0D14A57B5C55D5B96AB5DAF71FA32ED9090D0A656E6473747265616D0D656E646F626A0D31392030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E677468203332343E3E73747265616D0D0A789C7D92CB6AC3301045F7FA0A2DD345B06447798031240E012FFAA06E3FC091C6A9A09685AC2CFCF7953521695388C086D1DCAB33CC4C5256FBCA684F9337D7CB1A3C6DB5510E86FEEC24D0239CB4217C499596FE12C5BFEC1A4B9260AEC7C1435799B627794E93F7901CBC1BE96CABFA233C91E4D52970DA9CE8ECB3AC435C9FADFD860E8CA78C140555D086879E1BFBD2744093689B572AE4B51FE7C173537C8C16681A638EC5C85EC1601B09AE312720390BA7A0F9219C82805177F9145DC7567E352EAAB3A0662C65458C36182DA3F7A2BA7A6E885D94B112D56BF41E62C4395E969727D0C4EFB97C1F65D9F631295DA00CAB5CF048CA045E62B1227D4CCAB058B17E4C5A204920698924812481A4D55FD2BF5E0AECC76A13BDBBA91129E3D96FEE34916971AEE39667E7C2A4E376C5114FC3D506AE0B687B3BB9A6EF078CF6CA8D0D0A656E6473747265616D0D656E646F626A0D32302030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E677468203931393E3E73747265616D0D0A4889BC574B8FD43810BEE757F8082BB5BB5CE5A734B234FDC84AABDD039A200E0821C4B2237118B1F0FF25AAECB83B0903EA81260727B15DA92A7FF5F4F6EED3BB077573B3FDFBDDC3BD7AF6E161F3F2EE79CEBBC35E75FF77A04079832A24549F3F74AFFE500FDD6EE840834B0A3486C84F8AEAF37DB7FDF30ED4FD976EBBC7B7A09049AC1AFEEBD0920E41054235FCDBBDBE01C0230F0B402E13285EA090834E29C937EE78EC85283B990365871AC0944D9B5D6813DE21FE2B140E26BF19FEEA8E43F7E2894A0FC344570A5E536ABA3E53CF878FC2F3F80F63F1A2DBFE245211455CF0FC74A60865848C625A166912693E4F3056DB382264FC3E1B39BD010413F760D26D2699BBDBBCB1E5E350A884888E3D8F7DDED4C91EA8873AE1DFB3806A82AFBF51DAE5FA3B63185CE55F58310B17C118C8A6108483105C00EAF42C8456D3F42CD54A15C48B79B081C38C07F9E3657C40072246D9F019186BC0063636E6E434B95F5490A26637FA351ED6CCF560A35B3AB0E5DC68D24361BAF132E1C550DE28A3D9B9AF8B6CEF33257B41A5A409656F46CAFE12730E4353D2A29B1FF42AF1203050F37D8CDAA2F229EA144FD9817D154B7CFB72320F35F2F99B1788B201F150F1550E7BDE8925342489E098177831E54D5965A706DBCB181764977CDB0DBCC39CADCF821FD8E3F8BBACC7BAE652F92E328593431E246BFC76A7247559A4B493634ADA85C9C945507FB923353E0456A7B044D08D089A024F7D3F5945C2243E3A53D1B2B339F3136AB2BF07F33D431744A1A22AF67261344F83DDB9663731555583F70AF4A20E9BCF4EE9251E6A318923C1ED59773BFACB9479DDCB9B58366E2F8B9396546CE2C3D9C9E1AE16272D12231B39B284483AFA137CA1A66E2E0DC54F715F4BC4CE8D2502FA31E98B979FCDFF5B8B19A70C09E8A9A2A5D64C143DD7B09D5B4527576AFD59A70ACD0A95DDF9E21713C15C6357119C9682A5B0AF20D85BEDFD5C7038AC2138180DB810ECD6101C41FBB960E2B66C0DC1DCC92C02EDB1AED17CB76DFC6193B9425CA2E5BE22CD6DE6D67016B441A39BDB2CE01A82391349B1FE368D8F99519AEE04AB801F9CF60BF0D9FCD72D5B72ABE282F85E6148DACF112FF5FCD4465C4B22725498B014F4B4DE1C23DF4517A1C537106E3B5B6B51DBCD638DA676B59270A2124EE69863798F4107D85732ACC5F81AF69D5F26F8C6F30DBA2BF83359D4C92C93505FC2B8A0F528381598DA092CE94E775037B98316BC25AB6EA8A530A677E3E21BF555800100291B9C580D0A656E6473747265616D0D656E646F626A0D32312030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E677468203535313E3E73747265616D0D0A789C85554DAFDA3010BCE757F8F87A784AD6C636482812042271E8874A7BAA7A0889A1914A128570E0DFD7D9C9E3152AB9918235787667ED593B71B6DBEC9A7A10F197BE2DF76E10C7BAA97A7769AF7DE9C4C19DEA265224AABA1C26C4BFE5B9E8A2D807EF6F97C19D77CDB18D964B117FF59397A1BF899755D51EDC8728FEDC57AEAF9B9378F99EED3DDE5FBBEEB73BBB66104994A6A272479FE863D17D2ACE4EC41CF6BAABFC7C3DDC5E7DCC3BE3DBAD734232261453B695BB7445E9FAA239B96899F82715CBDC3F69E49AEA695E22EA707CA72B4FF7C32C153FFC281386334A7F8EB3440C49E25FC939DFA2EFC9CA5F45CFEC3968B3945106A4A72066D13F0548C33469C13661099983B66009B505CAC212BEA0A9121E366189D91AB49C2534EAD24958422F405B61A0B08446DD5AB18441797A1696B070CA4EECC78DA227094A6099A160524A783F8960BB793478F19C94B03CC30613611546339230DF1846EA0D4D2FFB457A92B14053B639D006084C83E6316B20B86EC23E936583C9A2A3CCA3CFF679317612649F690E036D0204792B192D50B65540AB371F18E513BA97BE469BD970DF53C64929C309B38F76AAE76233B4A185443695B71A91D21094E1DD51E864A5E738E90ACB905B7FD203CA6A7242F236298BB6921BA0B117A5773BAC6CB95194C556F9460975AE5A63A9F23F49B75C97CAE18D4AC249739C1A35FF3BE9783F8ED7F8FDF22DAF7DEFEF5DBEEBF9C21DAFDABA71F7CF41D77663D4F8FE01E548BA280D0A656E6473747265616D0D656E646F626A0D32322030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E677468203938303E3E73747265616D0D0A4889D4574B6B1C3910BEF7AFD0D15918B94A6F8111D83D3D8765F760D22187252C613709E46092FCFFC3964A52B77A3C2C4332E390C3D8DD52A9DEDFA7EAF9F7619A87E9CF510C8FC3EDEB2FEF9FC4DDDDED1FEF9F3E899B0F4FBB37AF5FA5F4B0A7DDAF0308100E95F051896F1F86B7BF89A7E1611E40425002A4F68EFE5A14DF3E0DB7A3FA1B05C9CE1F07A3B4042F5CD0323831FF3BDC01824AF3E7EB1B0E32E88D61ED5FC4B056D26C0DA3772F6238481F7F46C44649DD1BFE8B427663422BB8DA8061048CF749E7777B9F76861FF6E9DD7CFD06F420BDD996837C7B81AC782B7DF819E5F051DA6D35F4C1264F39D7D344CF26ED2CBF1CBA45DD16A7B6F812D5B18452BC023BDCCEF36242EB6D03DC88571730D00541858E812CB82041D58C032A6E77FDE001A02617C000D8897E3EA1E274DB408B96C4C7B4CBF2E00CE79FCFF04255847E9F71945C7EC629858D9072800FB6584177C8BF84B01121BB1983681A2811180947D642456735812390AB65A168F6FB2A11E222CA9EED54AF49BBE999079ED4F9B48BF910B962377490E3F32E8B838E818FB98D472E16BF894C779C054BAE613DCA56DC69BB2D8323D9870387C61B999658CA94FCA4D8A2C9A9C9E96C52C455ECB1A96AF2A9588C75114F7BAE67C65531DC42F0F7C9F9A69BFA9B339553862515509470FE28E1383ACEAE3B4E2EBB911796142094D0F02268ED9B9A906B8298FF1196D8DDF7DDCD81B1C797C5A9212AE8ED5C1CA76B48C14874C259C8164F84F4789EC78188B65773118FC94B1B892C155D1E449CA1512608249E71D9B0366C57191916BE510419459DA903B30128E2112D6364786BDA50D44408B4C22DA90913CB2ED6266E0BF452BBE97F1271D2CF79EEDC441DA5529D9F17ABA8A915A8898090ED37E245C6982DF0E8088630A20B3D5660574665242D28CEFCC6C4A7C28AE2CA2363D9C9E8539580DA2453CFB69D66ABFDCF669AA83BA5A37185F6530EA1DC1D36ACA4A0FDE1CCC2F409C280D2862E43D98239A3CB373AFCB18EF390D2EB88790638AE942AB709E7C1DB9AA9C83712D3AFAA1705FA53B9DC5E6D5CEA8E1E55A5C7D01DED4B59A4CCF797F14894949C579AC61E3B029057998EB4A122C5E3049F9F645CD0E008C147496E77D0127AFFF06CE73AA2CF767EE4EC451CF81544CF68A7F52E3281AFCDB5F617BF3D357DCE3861A392D6B4E6D2635279D6E2B92C0F877926AC5F7DA98D79DBCFC08CEB6DCCEAD0C12842523CD3C5C38A7FDB08C16C54E2B48ED699487CA3ED2D3BD0441D96E90BDBCCB8CE9DAE1B40DB805B4653E079D51BE6627B3C56FA36B2399AEA5D5847C9E723287F624D23D34CF9DC82F645B076C03BF19F00030085F6B7050D0A656E6473747265616D0D656E646F626A0D32332030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E677468203232363E3E73747265616D0D0A789C5D90C16AC3300C86EF7E0A1DDB43719ACB760881D132C861DD58B607706C25332CB2519C43DE7EB2173A98C006F9FF3FF15BFAD25D3BF209F41B07DB6382D193635CC2CA1661C0C9933A57E0BC4D7B576E3B9BA8B4C0FDB6249C3B1A836A1AD0EF222E8937383CB930E051E95776C89E26387C5E7AE9FB35C66F9C911254AA6DC1E128835E4CBC99194117ECD439D17DDA4EC2FC393EB6885097FEFC1BC606874B3416D9D084AAA9A45A689EA55A85E4FEE93B358CF6CB70763F3E88BBAEEABAB8F7F7CCE5EFDD43D99559F2941D9420398227BCAF298698A97C7E0007FC6F270D0A656E6473747265616D0D656E646F626A0D32342030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E677468203935383E3E73747265616D0D0A4889EC573B8F1B3710EEF757B0B40388C71972F8000E04A43DA9089222F0062982203012C7408A83EDFF5F64F814577781573EC14E914247EE72F8CD37EFBDE5FBE9B84C3F4D1F272594B080C205149FDE4DBF7C271EA7C332DDCDF83B087EBFFC35A145892428A02423963FA75FEF95523A5A51D69DBEDC3C39B942F4BF03F2CD456FAAEFFF107C6D2B7E5BAEA9320227C15F541960840C8B4E2972D1F98C6D94E6871D5EA5C854454A2A6DC5F287201324E2A0F15EE95382F6BCAAB8FC7D05FB1DA33ACCA8D648B702658A99E6E70197A57BC30619466FBC12AFCBFDE38FB36090BB371FDE3E8AFBFBBB1FDE3EBE17AFDE3DEE7E7EF33AC6C3039F6E62CC848DCF84BD91600539920A5FC0D8074923CC4D18334B7442490DFCC730CD4FEFB3154A00C96093624D127D0EA66B69839ED91FF8A7EB9E789D394325A7946802C0AB4DFB2D09B42C834A302AE76AD3D94DBD02C148B4CF20BCD05914D84FC865C22EF3CF39CBE4206190BE7B2B79C798C13BE6817F33175C289980FB68F8CCE4B35190D885862BD31CD285B8F3ED20DDD2BE0AF00165E930EA48A7AC986CD5950A7C2E37099360082E0B86D25D805F841CBB433C1FFAC2A81F9A53814A8C724F6AFA74D2537599C6CCD6BD2DC296854B134BB9430309F640DF6BCFF6F083AD64D63AD4BCB1256D08965156120CD1621D07F87C556E81064E045A436F29F84DD07ECD3A57240D6EDFD78458275D8A5C4B0A5ED5A94AE50C9C53746A8C7BD6CD0DA246A676AD2FF4FDAA3EC9E83C1BBA1137EBBF6D0CD54A042D8D3E8F3B6E5BC360AB23D5D40D1DF31004487EE13155050D7B0CE6985D6059D6AC6E3326B887220A7E8EF932C031AEE736774238509EDD604FE565DAACC73C3380B08FA0D2A9C26832162BB125ED4BC4C0770530AB32C1FB8B0EEE1E583014BAFD94095EF7D980C8AD580F7EDC98C817C31BF2EC4644E9C30A4CDBE376C0165BD4C01976115C6E4BD9FF6EB4979B1BEFD9193686FCBCBFD67EEBA5A797D98F0EA4852FC0384F7F7426FF8FD4216E562FD4BE2F8CE051C0A55E097A176FA0605974FF7E913868D834D2071F3AE42F959162AE67DFD3BB849A38E753B572B87BDEFBBCEE23B54A73BA54571633F5F45CD5568FAF3A422A346CBAECA9147D96E022A376C0D7F45A4FCD3E5DB30FBAA82F2DE55CA37E2E06949C8DFE295F681CF2436DF3450315B0749FA5F4E5DD92FDCC839ED8BB16E9763B2A68C995D4A00313806A09B58617C6FBE1545B21FB0B9BBF8A5FF38519CECEE211F42F449B4BBB6F92C3AB6DA09FD5E72A5757CABEF01CA0B9F916A89227D58E9E289C3579B6897F041800547E71D70D0A656E6473747265616D0D656E646F626A0D32352030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E67746820313035323E3E73747265616D0D0A4889B4574D6F1B3710BDEB57EC3129A03587DF048C05E4957428DA43912D7A30822040D2023D186DFFFFA1331C0E97941547EBD607C35A2D391F6FDEBC192D3FEE4ECBEE97DDDF3B35A8C1831E42D2C33F5F77BFFD303CED1E96DDDDAC3FD901BF5F7EDF395063D4834D6A4C6958BEEC1EEF955276DA9B013FB813FE8509C0E1937171DA6BFADA3AA5602E0FDE4EF4562933ED43BE055A4138F26B88F3946F039C2630E5E0C7654B8C418F4ADBC106373A9018AD9D2C590B06C32B5E8D7325280A863338B057729FFF2B9DEF01E08D7498F6B63D6EC2292764021D99031F8DA9E4526C102CF93EE696AF813B960F78345F4D07B475CE0185D641751FBCD83C3C0B99404FF87CA22F8BB7530965E68861561779E19BDB508582AA46587D6A50CD15669718CBF2E7F76C2D4B35156154ADA977C37BBE7FFA791ED0C8DD87BF3E3F0DF7F7773F7D7EFA6378F7F569FFEB87F7D3F470C4B72F06EB84A6587EE747530255314CFF83836531623F8EBAF150E3BF114B616835400C05E09A3DB852337754E0CF9976D231CE332998104E0861BBD720B50E473EC714536C9ADFB8CC847069605BA3814FA3F5176960077157CF53647A963E2E8D0F3A96201E4AF7619285CA9242A328A82699EEFA5C0E27B595B8468DC6F47CB889B09D0D3D62937799C20C9C10D5A9D408DCB56A184E9220CF1DCD8950655E2C5CB61A441493BC20436CACF064732B1B03D4782D2226D86D8818137A1B8F85639423C94E5659D41DC607532BAACEF95AE131A5165B9C04418155A6419560DF3509DE4F8D61DFB7502AEE096BB6AE5EC173EBF5182EBB75CE5639BA74167F071A039C621672ED7388DA7662EEA64E8B8548545D822E67E8674EC8BF3822B715DEA1E2422F8D3872B6B783E897C565C00822347FF499A7BDC31168E3B09A9769D60B575D042AEB63223459A0A87033622758C54BB5E26B28081A18FC0E46F0953807C634E329135AD8B33616A8FE38D1C85D708C1855C65E9DD437D4A0997D01460F0D746F32FB34D421FD46B3AF7A78EDECAB067237C552900271AE3D6D8D2292D7EB1F4D2BA4B57FEA8823BE91547E9F49D15C161A87522C4D9BA4C7F5B3A9CA1EF2896C2799D6F47A59748CCEAB73E9007A40F6826A32672BB695856FCD97E2ECBA6D193799AFAE35FBADF580F7E2EAA9B60699C5853515A85D7BA8B3AE25D8B56887B57A3AA339FBC998AE97BAD88A4074979B54EB343915D00422012EEA0B21E0CBBEA86E6A7CAC257028205282DB8743D5541746ED7B3EF743CC09B46E55BC6D3EA467941F95A9EB87E2D5A82E0534F582507FF36C001547131B1FAF58930020CF972E4E821824D6FC70986AAF8BCE83EA7FF408DBF6CFA92047989D26AE2D8E5F862BAD9A7FA3ACAA7EAD27FF9BA43B5C835393F55B48BA89BE0C8CB792F4D5C32B257D35F0981776AE2ECDDA6E010EB2F6E7073FF522764D9D3F0EFF0A30007FCF767B0D0A656E6473747265616D0D656E646F626A0D32362030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E67746820313133383E3E73747265616D0D0A4889AC574B8FE43410BEE757E4B88BD41EBF1F52CB5277D27D4070401BC401ADD00A16240E23E0FF1F28BB5C8E9DE91912348756D24EB91E5F95EB2B2FDF0EB765F861F87BE0231FAD90A30B72FCE7EBF0D337E3F3705D86A749FEA247585F7E1F84D62CF85179CBBC1A97DF869FCF5C04CE85B8C10F9E57C3859F22489FD36254F0E45CC593F2E9557019757A5E4D3C659970E1C25CE2C9341B84B3D1E58FF778CAE282475FB60BDD7D71337E812D59A132860C6695C91983825CD8FB6AE8F3722470E91DF3A2091C2C3919973F8FE8088269B1018FDF0B10844C45F34271E8FF062E05079166F0CC0C9BD2CB04A1BBD75043C4F403C4C881F40C3C0A5E94DAEDC636AFCEC42EBF948EBA6078F1A1E61802B10F937E492EBD34F142B5B8E00B0AB93EF82AE5437901E48AFEFC4CBB8ED580938C4B3D2A27981494BF09E2EADC4B35D6BB427EBE5DBDFB7C117410A560C637BE6040BBEA71D5A199DAC6237D94CD79C11FB899DD9D443418DA5A052255CF612C0518F5B6B7CDF5358A5412DCE968F20BD788213737F8B96C1DD35D6AAA3F132F21A7E282ACD8C7F5411250EAC75220AD647062DA1428E3937B3B53B12C5555D2D1AAFA307EC4FDB7EFA711943C7DFAEBCBF3783E3F7DF7E5F98FF1C3D7E7D38F9F3EC6789DE1EB9BCE9A62C1409E8D60CE955AF12EBE83816551A4DF33D958A8FEEFC4928E565590CA01F28DFD2B7523E86E98AFAE40D36AE9265DD73C588CD2E5C3D45AAFB54EF5418D3A979FEB8E7C6D31F0E25EE9012B81C9D75B71A3BB2755549BA2D56D374E1C30B9B2ADB042C5AC9675E8ED51E19BC305EF3D03E55D1D1DED3989039DDB407D2DDD3E070DB109418EFEFF06A3A041F28DA184264D28AA4DDC8A7B6A3459A466AE45F4185E4049CCAADE854AB64457D8AB8819890C0CA58C4A1E2520D7C931D54A15744AFEB944E00C75C14D59390C2D74FAA112A428D5D97FD9E6A05849A4479C7A2AACD708532069F60AAD5E7EAF058C5BD2B31AC3A1234BEECFF7B68328CB646827D3E4221D41C4BC4545116881AF0BC836B0A26EB99D27C47C07870D39CAE27751BCA236E379ED07A516084A9ED8FAC25D4000B20B37629468F783D2D08A5499562A26EF462B9C4A5C33206A255CC3E2123CD7E0B599900B95C25B00CC7479FAE14682C09C847010D336E23A7432637193D63DDF674C1454A18406AF40BBBCC2D316D0C4BCB35A88AE0470B7958DE3E703BCDD8E50D63FD07388FBD324A47CA3E2DD9244AD9033406A5C7E854EC49C1D6590CCD11DE636C33DE6FE3EB300D9D386B556F6CE02B4DDA8DECB5C536E1DE2DF18F5045D37753E2F5AC7EEEEB4EB52A2BAFFEB9D24AB2DDDA18CFC93A32B5B699B5BF22DEA1454F2C1F13E28C64597AAE3B5194CA2EB0D9036CA403D0F670BECC4384AD403A76DE9FA6BFF7CC45549344FEAA9D7076AE2D0100B85CFA80360AC2A9A2E89197119218317CDD5763595E404DD432BE1D0C0E7EC8AFBBAB98A01C39BCA4F73CEFC5A367A6B194349D0A4C9C0D088A03B32A3EE5E4930C9C3C4173A0F687CECA6543BE1A276590D097A33AE93C1EA83C3BED891C4E7F15F0106002250AB620D0A656E6473747265616D0D656E646F626A0D32372030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E67746820313037383E3E73747265616D0D0A4889B457C96E1C3710BDF757F0680798168B6471010402524FCF21480E813BC8C10802C3760CE42024FEFF43AAB8353992EC1941BE486A92B5BD577C456D3F4FEB36FD36FD3749218505255C50E2EBE7E98F9FC4C374BF4D378BFA0B04AD6F7F4FA8EC6CAD5041CD4E8BEDD3F4FE56825FA20A82FE704709F62401D6F8E776955B83B3559DDB5BA99D8ADB3F57F908B33667A9C9533C184E4D2A09414690FC41F9F9F4FBEEDA3C9D9FC10C794AA9AFCC53CF8E10746E5658D3A444FA3425605B3845ACD04A150F5A64C02188DE8E6C34FAC8BB9CD0A1FE714179DB563303F0B3EB327B23DEE6CAD65F17413E6EDEFDFBE141DCDEDEFCF2E1E18B78F3F9E1F0FBBBB731DE1F69F79BA59B1240CE525BB17D141A1306961642A17B5D89F2537C8580DB56E3715775515A3DDF61CA8C4C35FBF78CA9291C30F42A31830976C063D961FE1287C7682A75CE66A2123990D9A99EB4E75B93696E4E886BC8CCD60E7EC60DBA08398F636E6A6A8ED21377D11557E99B6224DF9A4CAE6B7B087AF638F07575DB43B07C67CED194708FF180255F68BD4EA83620C2B00B6B06DF561CD470AE5D09870CE74007400FA47BDA41A623DBD29AAB245490D3C26322C08712B8DD4E0E0432675CB2E8E261C7271DCB4D441C65896254CC50CF701E652EA6F3472E72221C96CDDD904CC8896483061015E8FB7A9200E1AE93F914E925965395294C8D844F143472C9FEEC12B5E974F742617AAC99C6CCA6F60E2CB2347E0A510863B470689B5321AC23BDE66DAB356F60011B7B091D416D3DD9A8EEFA24F400553A2BE0D5F97E93B92F1AC7E43834BC6D36ED106C953CDD8324FC1CE57ABD5736CC4177B0FE70C1576ED675BEAFA71F25F82DCA0B05BFD9B344E9B55219AB4CD45B5EC8B38597A6DDFB8A8C4D245448726D4C34BD9B76B62EF0E5337D40EE3A3B2CD8224A77C5B34F52886B69D5C5C5F226D895D39D79D4635ADAADA952EC52248789207CD1C850F41AA34761CFF8D5234321CEA8CFF800152F55B9300A28DFAE7A61975EE2984A5590737A10B650267A191D45F806756E43A9BBB938C2DB1E9C4D668719C149D9AA53589DB5017E9E4D7D238671B474A6CFE651F36FE1D9174B1149B47B6AC81CFC3309D021E81178B1A603F0D3A23ED75596DCA6EB4DCFEB28C2AC9DBB42372DAE18FB857AC447D5A496E9D5FBC0CB05DA51A6DB5C1A1F1DC358649AA8430EE38BA0FA789517B882301BE8407915452615C620E4AC9CA79FDA8BAF5F1213528099A5E1C0A4CFCA0BA0FF6B826FCAA7236DCB2C5B8C217D64A5810BAABA20285F6FDB05E5DB2DBF2F111778A64726F13F96632C65AEF6921023D839249190268D50BA147557BB18F6EA7D3637F7A9784F561988CE9DE186A08D5036743D64EE0618F775728A96612D0B3CC2D5C211689394888CE09191E9A2B0019184F49DDEB218FA149CCC37E59216BC0052A581C578C4549D7220B3074B62A0960EE70E303EC7ADD4B2A47A4A76E27F01060039936CA50D0A656E6473747265616D0D656E646F626A0D32382030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E67746820313031333E3E73747265616D0D0A4889B4574B6F133110BEEFAFF01190E27AC66F295AA9D9260704074410075421241E12870AF8FF07C6CFB593946CDA20B5D964D71ECF7CF3CD37B3EF86DF8360821940663DB23FDF868FAFD8C3B0D90F820BED99E0681D7D4AC7FEFC186E26FC2C18282E14DB7F1FD02347CD802EDEB1FDD7612D8490E3FEE7B0DD0FEF1E379DCD60B2A1B9954FB0B1DFCF261C37D09878C15EA6FDDBB713232337EF7F7D7960EBF5CD9B2F0F3FD88B6F0FAB0FEF5F8EE3E68E9EFED349954F908A6BC3C05A8E261CF0899CC4DDB842B61660CD68C3D5DF8E10BFC07694E1EAA671A5F28AB83404B60A8F84B6E32AAE9DE88BCEBBE30D7D5BCCDEC58DBADB076E363F8911647B9ED9A55571ABF3F9F0C69B62EB7EFFFA3CBA906307233834B15F0DDC7280240210B806B8C2022EB8D97B5782C57423041A62CC2BDA40E352B84D37226A1863179892A0BBDB42A8984600FA21CD9431562A0125B74B806A02018A8408384742562D2E2B87D986E7B60703C42E4718C2F0048428408450CDF2A4368C060BDCAA03D4AD1C35B49CA964D477995A2012C8956B7969836ACBD1A39A007AEE77C9EB42F1E36C8DA56C6C4928F66718CA91CEAC46DD6E0A07982979ACDBE2032DC6FE20A1B7B982A80AE36602D79F74DBEA0BB9208DE4E461CB85C5D238DB30BD8D40868DCE64F0220499E24BC9124FE08124F99507679C54A744067DA04C52EB0C682C19B5C9BE84A2C40EC022818091674E1CED5E9877A74FD98D70044DC05679059A94D0461AB39EC08509D50121F3BC846AA37A1B21A1E83283C9359B9DF717D5F4CC178E9457452AA48A756272A913F019E478952ED64AC2F650237B0D9505DF28392272307B9A284887A84497B021990A0C309D3E07D50EAB49426A9DA76ADBF52D2F718F0E94DA5571B950DEBCE4DE36582C9F2AAA127B13A78A6AA236BE85E940A0D292CFF201C171E14FF9F09CE6FB8FC98ECE31CDC0839A83AB034FE896265F29D3E842B2891EDC7B1B8B6D8A05B7229F05A4B4D19F1CEB0249DA4527F8D8B224CE0F30B497B22BD80DE7483B42636AEEDEE7A97032C2383096001104B7B689F05AC8A20D27027D283005D983395238EEEA1C0998AA53950A20699247F227B3765695087DA01F3F77E3E3FDEEF276B7BCEECE050D3A623C471DC62275BE0ECEDA25193DB0BBA8BECEDB95A1DAFA2C0995412C633FCD8B7154548B47C5C7CE9D058770B2D78B67B66BB9977D3CD26CA371C3FA61BBF6844007B9DDD2FF940A2FFDD8554A3D3F5ED42AEAE3B57851EDD20BCB81DDABE088C48B20C61D2F48EF525F2D6F1F6558AF65575054E5FB4ECC52B67C783FE9DDDC29249073D038F79F5ED3A8AF5203A8D18717319D7ABC2C3D3EB0C8EA3AFF4545AA2F03BAC5AA9F32CA5BAA2FFB683E4846687ABB677F051800A179775B0D0A656E6473747265616D0D656E646F626A0D32392030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E677468203735383E3E73747265616D0D0A4889B455C98E133110BDF757F80823C55365777991224B4927398C8404A2250E087100060901D270E1F7292FBD6599E984E43069B7BB5CAFDEAB579EF6A1DAB6D5BBEAA90201C2A012D62BF1E75BF5E14EFCAED66D75DFA8CF2878BF7DAC506989B5F05A82AA45FBB55A82B62AB43FCECA61A41DE7F8B804F4BBB05082179600710B003A2C6A13771002EAB8704D409F6236C1C667838152C48A6379910FA5584E811E420A9FE40615D04D7737FC67CA618C3B6BCAF93803206D02A65D6BCA2E410AFED4CE51AE2EAC95F1B2A6296BA8D7215174BED482DBA03BACB801A800AC1E336302F53824B1571D7B1D63C0D4C1D1B17C8911CB975F58D01805CA009A5D61C75F5D11771EC3AEAFDAA37476E28D58D259DED0BE96DEEF79A36B3371A3FCAAAC0F5ACE5285EEB9305985A85D58D0D41AA0921EF509E7985D466AD8765D4FDC34A481520C64757BBFC4D42CA4DFF3E2AAFF5C0CB90A48A30D4D2E63CDB7552F18496B84B592EADE55E80A5E1C82682DD3F1EA07C796912ADCA63AE852E344E8890C7AB0524ECC0AE8134395E531F9441925DAFCEF38A1ADA57607C43330DF18B944D3040DE33960C79FA7AF022F158E702E30B54294DE4E6B45E582A24E292ECB1C681BEFBD91ABF21376454BDA0C56ED7AD5D894C28F5CBE38B820FB0B67B048539AB65F53AEA5DC9A6E720762B17E77550FC77C199BC9ADCB9768AAAA69A6CEDF374C58D83C0F296BE7B7192D6BDB4EED9A4782E76E50FB95789DDBB57DD3084E72FF562C9721AC37FC06D292F82B503C083E2889842190C60AD23EF60C2571BD0CF43E9F1E9F7DAA50C47250742795E47F2D5F7E7156A3ACF89E8A8D6F56F1E2675E2CD22FBF8C56F1E3E31D97F63C86F5273162B9E453A2B2BC14879C8E8375733249309E0B7A110325988CD1AFCE223280387F658CC1329CFA0508FEC55E2A3CBBEF47218EB7FD12985EADAB53C91A912549E3B1A2B963453CC9DC92194EBC5CDE531857D7F7E6649260463A3D67ACA8B73C9D4F64007976AC2EC1182C63DD4DC7EA04C40DDA7E152AE29F000300667CB9CC0D0A656E6473747265616D0D656E646F626A0D33302030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E67746820393439352F4C656E677468312033393033323E3E73747265616D0D0A4889EC577D6C14C7157FBB7B3E9FBFF0623E8CBD763CC770067C679C2621189B9885F31D1817E24FB26BA0ECF97CC6500C0E410852A93DB522A08556915A5508556952A95514B565D6EE1F6754091A55CA3F4D15A97FB452AB862AA91A358DD4FEC13F34ADFB66F6EEF085506885E4469DDFED7CBC8F99F7E6ED9B993D5000A012B2A001494FA766FAD54D4180ED1F00A875E933A7C98F6EBCFB0D00FB7700C113933347A63BBFBF3A0AB0E363802A76E4F8B9C90B1B5FAE06F8DA6F0056BE3295494D7CF957EF0EE38C3FC4F2F41432EA9E5C9DC1FEFB58D64D4D9F3E7BEC6AF09B00CA06809E9BC74FA6532B57D43C05E07C1E80BAD3A9B333F55FADFE1BCA43A84F664E65665E9BFDE0CF007B71BEEA0EE0BE9697FFF8DBD3C77F7FB876DBED90C1D500BEF7DEFA36DEFE21F0F7BE3BD7FE714487D0209215429F03DBF29E7FEE83B80E77AEDD795187A2248F9AEF70CEF29F631C74C150B135E12580B27AB48B08E687A85B8B85A9BF862FC0FF00CADE82571EA4137C03AE7CCAB8D71E85FDC00B90FC6FC6A96FC0F947617FA9A1BE0DD34BED838484848484848484442994D717AE2FB50F0F8B32E3B3E3AB848484C452428185EB212C3AC87353424242424242424242424242424242424242424242424242E2D121F033985C6A1F24243E6B50AE2EB5071212121212FFAFA8D8033F7894F305FE04571EA453F6D6BD3A81DF3E789C84848484848484C44342CB97265004FD2DA4B0A75C85009C45BA1174E454C25AE885BD300413700A5E0D9AA482AC251D915F2C2CA00EB99F6CE1BD45BFF4C7ECC35FDECA7CF8FA1F9FC9DBBA0F94201415145505503FA9802E07CA1EB8B4F585CEE3BCDA7C8F7C570935FAC0F9EEC1858751D2FEF37939962CFAE681F3A75F38F5FCCCC913D3C7BF78ECE8D491C9CCF821EBB9FDA323CFEEDB616EEF79665B77D7D6CE2D9B9F7AF289CF3DDEB1A93D166DDBB8617D6B641D5D1B262D8F3537198D0D6BEA57AF5AB9A26EB95EBBACA6BAAAB222541E2C0B68AA02B1044D3A84B53A2CD04A77EF6EE7344D2123B588E13082AC64A90E238E5023A59A266A4E7E42D3F435CDA2A6A2936DB0AD3D461294B0B77B29C929638316F6BFDE4B6DC23E12FDBDA2FFB2E8D7603F1CC60124B166AA9730C52109963C33E5269C5E9CCEABAA8CD378A6B23D065E651576ABB0C7EAE98CA7D4F728A2A3D627BA3C154235E8146BA4BD09D6407BB9074C8B2452136C60D04AF41AE1B0DD1E634A3C4DC719D09DAC362A54202ECCB0609C950B33E4285F0D5C225EECA67B39A7C3B813AD9EA013A98316D35236B7B13C8A767B59FD8BEFAFB94BE2E47571EBC262A9A1B989354709275DF70261AF0E5A8BA5615EDB36CE8163D548D2719368FA3206B17F98A035F5BC6D31E53C9A247C257C55FEFA3234C139CE31C22AE84E3AE51E73F0D534BA0C86CE85671B1BCDF9855BD09820EE8845C36CBB41ED546F93B712DCA173730D26692895B4C73C7DB91F586F596DBE535DB3B89329CA444FA8F35EFF5031B20AF788F66142309226E88945714D9DBCCA74829BEE443584ADE02836816FE428AB883BAEDEC5F97C3C2B8BE894B8B70133807EF497524E2ACF0946F4DBC0BB3C4F8AA986F2429F45A3ACAD8DA748791CDF29FAD823E8CDEDB1333995D2199D6083E183018C6DCAEEEAC0F087C3FC055FCA99308E04CB0E5A3E4D60DC9805B3236A33D5E1929B05C9AA512EC91624C5E10EC54CFE89D8F4AB58A8B5F8D4EAAB5724A6BA98B2FADF8833BEBC7F98F60F8E5924E13AF9D8F68F9450BEBCB328CBF7D88AB8A5196ABEA71A9A9062521E2C2A73C2AA6681083E4191D413B9F21066A5E02824C97467B75FDB95E1F0430ECA2DFC958F12CDDD6179375957B494EE2EA14BDCAB76357438D0AAF68F8CB96E65890C53CD37D8976F30E361C40A93388351DC99117C720B373B79B10D6662C8E25C01F3CF67E5C9124523DFB7113C3BDB63493CE85C374949D275DC546E213B4E894EDD79F54DF54D7726E1141227B770FD92C192976D8CD594D2D51EA35CE2BA131E681134631A9E223A5BE2976CF66CD4A66C3C4AC3D4CAE05ABC2EA80E8F3871ECA9B0D3A3CAC541CF542E0E8F59F33ADE311747AC595551E3CE4EDB5B87326B9EE05521B82AE77226270827A05FC1D0CCAA21A16FCC9B0059210D0886A0D33905042F54E02990CEA93E4FF70DB50A4326DEFFE95CC0979805ED00F2423E2FEB6B6FC86B8750A273C975C01B0784D08787C48865566E31BBCC6EB347DDAE6244386B1639D751B75B81B91E65BB627838E79060E794ACD76D1AF362A6A1BC661635392F5BE4A1E75C6DD14468CF5FF8E8DD158C8E59733D80F38B1A357672F093169D58BC87C4C1C4F3FCB9A855ADBAFDC398815C58D969542E12133E9029941DA667C37C756C3F3D17462665044F6B54F2605793EDBA047F14A392DE6FF9351729B1269CC966D9F182AED184397197ACC6A122AFE69AF81952B4F6A582B553688D77DC823996FE546BE83D530EF05A3CC27DEF69A0BE7DBCA57DA3EE41770CF331CC9AB9E1BC1F482E6BB2C50CE8C915E189222EA7347E134CF2BD44F82187C724DDE3A9FBA2A25544EBEEA18909D4E0052FDDCDF8B2C264C2E65A946F1A9EF8F755521629F18B444CEEEADD054AC953FEF675D9915272AA482679C16F94C826FF98C0B5882D1B66C70C76DC8E1655527CCD2EEEED2EBEC1BBC4E05DBC3878EDEC62D9740A5DC4FBA62F4D91B10719C41AF723C82F6A977F39A553388C47396F899D88964C8967828247144EC497C3B203C4B1898367883288C136082BC3964CE2E7134DF17363C05FCF001EFED8A4DC611C0BFCB519AC1CCFB3C95486F2C395F17CF7A3CF7D0CA077306C31305C97620EA18B91242AE3F4AD2CD8DAC71B7C66A23495E15F7693FCC32EE37F72A0BB223A7C362341C336AAA811114B0C1C6EB4715EA55DFEDD78C889622496BB752ED9EAE2863F846755A035BDDFC1738DE82449C4AB4E19486110FA3865E344BE6245842BE278F1B4B2E9A877A83C7297239E93515F392466151F116CA0A0522E1EEC3C1F656A7D270AF9E295A131712FE08BE2C12B8BF461784DCC2A838FC65D3492BF36FCF17D7CA8517861FE30E4D8850B00F3DD8B281707169F8407595DFFD0010303DBEE8D9CDF51A5C5F84F5D0BCDD0A245B536D8866DDB6CB0B925A76D986B5DD3F2CE4FB58D700B8BAA6D9C8D36B7CC6BEBB5E6D9EE1633A7D1B9BA554FD4EE68D7081EC11DA226589FC4720DCB0D2C0138AC3D867C1DEBAF60C962B986E5069677B0E01F36ACB994603989E5BB586E7189D6AC35CD92167DC77AAD01C736FC8BF1EA8F6DEABAC2E7DE67C74E426227A38E21C6CF891377C480C1400D84C6CFC66E4A5D9414028DB340026934A0D582E6844C55171EDA50873A48C73A36D88FD0AE5B512B8463B3CC49BA255D5ADA66A5542B6313A505BA6985496DA07F8C31A9BC7DF7D981A13169EFFA3BE7DC73BEFBEBBCEBF7EEC3122C52194D011A20619E6518B58C1A8036A01F1800F2749EF07401BB8031E0AA1E51A4B2D481C5987B59EA195DA5B73FE1D7AB9BB3D5D68D7A35FD683CABD73C92D591D559DA8A2C6DD192AC7B4138ABEF9D97D5A5D57E55E88222FF78C826D9B0481B26BE0392F1D7C9C218C97444BA87920097F2721E452A4D5779FC0363928198C425866F32591B9758AAA8C41F2AE01A9FA25292F967FCD36C847F9A2E2EF10F841EE21FD371600C90F8C72897F825DAC52F8A9C43068101600C380D4C0179FC22CA05948FF84764E11F920F08026DC00030064C0126FE21A4959F17873C5D0A3B08707E1ED2CA3FC0B23E80B4F073B0CEF17398DAFBA9C072FFB06E787D3943AECE1965E539A3D4E6CFF03FA46ECCC58EF2E04E63478D4A9554478BA5CA54F5226C3F7B6AE53639C3FF927679E523A185FC0C25018E999CC1C867C8053402EDC00E200FD659586749059E058E004900BB0CD20AB8F824F00E709616020AD00898F97B290C93E1A7539EB01CB2F177F99B54868C9FE26FE9FA1D7E52D7BFE76FE8FA6D6827F4243F9972CA142A449CD0C60A6D85F6216EE4AFA5AB4A652D54C2C7903B19D207048106A00DE807F2F818AF4C3D2697A293519A34139829BAA2EB5FD20B6652B6CB8A671536A04B08CF8AFB61410CB8063C5CF11C3C84AA109EFD076009E1F9F6776109E17972372C213C4FEC842584E7B1EDB084F0B4B4C112C2D3D0040B22C37FF6EBAA7BE540C3E3CC15B2F05E64A91759EA45967AC9C07B45A11B0631B71FA76A6A90B1C38A776E8DACE26CF32A53D732F505A67632B58FA9BB99BA92A99B98EA65AA83A94EA62A4C1D65CB900A952927EEA82E57EC4C9D64EA31A62698EA616A3553AB98EA620125C32B52AB17EB2AAAAB7448FCE9A0EFAFC3D3C7C22B90D10AECF90A3C13C6204F039A5E5340725566C9B39C4257A66B82D9FA8215FE2EFC7D26D07002B761822E0006DCA0096CA3097432810E2C9041A00D1807A6000DC803BB1213EFD7A505D20704813660173005E4E9D399023875E5A6785C9F9898B42F37F106C0C027502A512A788532C7EAB07AAD0F4AFD0E6671B206A7E6E401B2D9F09D555A622EC9B0A2A1EB45FFBC5E44F9A17CBE9FF78B47377F36A7FB5337F0E8663F4A7946E5D03DEC87E43460E7B1E5E461D5D0CB28A1D79792C32CF41272F057A0FD29C70634B3A43CF3E411562C5A0DC9371C7F95AF38321CE665C7A8FC2757C6C052F21FE17965483EE3D82BBFEDCB98E179D5936150232E9D3AEC58261F9BD4A9BB11389C92FB841A92BFE9A8971F77E881CE6C60530235C522AFF5B4C80FA2BF88638BAC24D0E7901C746C925766594B459B217921A6E0CD9A3598EC5C873EA8DB09CF0979E9FAF5810CDBAACC331D34359B1A4CF799FCA679A60A936C9A632A37CD34979AADE662F30C7381D96CCE331BCCDC4CE69919EDA2E225DCC0997956A1F20C421A74DBCA8584D01F7DCCCCE9214A7E498AF1D8BA308B25C73B28B6C595FCC73A778615E0C3CFE80E33BC5929D6144E2EF3C632266D6D32E08D254D8D5F691E646C7F1CDE24FF0E3E5D9A9A334C13AE3DE5F8246C1E26C64AF6EC2B17FACB7BF6C5E364B7ED0CDA83A57525CB1F88DC45B4E7A4F7F665BFC39E134E1E8CAD6B4E2D7DF9E539E178D2AFDB9A063B96FCFE3A9C8C87D9E7EC6A3432CCAE09156F1E96EAD8E7D1B5C22FD545E2F158866DD079E462D7C0C3D6B9A6F3CC784B0B1EB9CCCE2CEF7096578DF6E05509055E7E3E55EBBCEAFC7C9D6760823798A88A4606ABAA744E998B123A2751E6FA4FCE643538D5D53AC7A6D2A4CE99B4A98293ACD3290E07284E874E61B3C9A1531C6CB64ED9709BE2CB51F6DEA2ECD54792D86D8E23CB29BA38CD29BA088EF7FFBD3AC35E2F4BD7C63B5AA39DEE68BB3BDA09B4279FD9B9D52E4EE4AEC18EB808E000EC69DFD2B155689C49E3EECE48B2C31D710DD6B6DE25DC2AC2B5EEC820B5469B9A075B95CE48AA56A98DBA3747E2E9FAC625813BC6DA7B6BAC258D77E9AC5174B6448C551FB84B3820C2F562AC80182B20C6AA57EAF5B148DFEA8DCD83660AC757B566759A171660DBB6E31C1FB65977D4E97BB8B6C2DE573E82A3CB512AF4C69333DCE164112042F343F3432284BF960815C36DC985EC7DB515E523EC682E6485BBC41D266F774FA287ECD16D91EC2F810BAEEE1E91F0ACF426FED7855834A96C8E24BA8962C99A75B164F09196E6419309DE76B1A4E48A695F616134A38D679D0BE05C219C92748B287C2B852F3F3F47FCEFFBDF93D3ABC4BF40E5A369A638593725E252D2196BE2782234B560ADAD2DF8C218D0DF1589381698605E9698EE23376DAF97B275126B9E46774FCECAE5A23BA7B32DD124319D925B974896F756C6BAF56EF5747A5B9B43C5D27D928F42383B2F849E0F3D1FDA0FED977C4AA947967840CE3707E4C282886CCA8BC8D3BDC6BD641CA159C06CE34B34CBE0213B91F6097059E89BDBB4CB222E34FF3B9E9A991C888ED231B68D8ED118FD8E5D45ABE3344C27489CAA22F4137A8A9EA3A7F1A66C81672FAD4531C2FF1C9BA59D201F3D8F77E5F3740ADC47A98F46C8C6ECDA15DA457BA4F7D16A0F15512516D3485DB48F3DACF5502B5D307C8B02F4307D8D7630556BD6F66B07B417E917342CBDA57D4185349B3A504E699F19FFAC9D47025AE90774882EB003F9BF2205A3A860FE94BE4E87A58D06A67D55FB17665041BD988381D6D02936CEBDE8BD933E6176F694B40ABDFC5C4B6AAF83E5A08DB4950ED3085BCAEA7985B1555BA39D221BC6F8067A3D44291A42C9D06FE81C9B61BCAABDA85DA559348F56633D27E85D362EDDFC62F7CD2032664496E6D27244BAE8B7F426BDC7DCEC35DE659C61F41B15E393DA199A498B683D66FB125AFE8D5DE77D28BBA4938607B43015232FDF13D9A637E8129BCD7CAC816DE073FFCD7E99C046715E01F8BDFFFFE7D8D9D9DDB1C10718E35D88DDE02DC11706C30A0F2048A3607318BB86602811882390C4E12894232584A39484905207480AA5883B211816B21C91DC8656ADC098D23429AD02843810AA7224A206823DD33763907A484DD4285254CD7CDAB76F7756EFFFDFB9FFB0A7D826FE0CA8B4623E3109A651BCD793F573548C8798CE9AF856B147DC9533AD0B7690329203AFC2CFE09718204FC3380B9FC3F7F02336984D60AFB28B7C9DD825CE2813C9EBF130135E803D700B93B12F8EC4C7702A2EC415B8163760239EC64FD840369A3DC1AEF3A9BC96BF2D0611156296582A2D977E2C7F62555BC7ADDF5BB7EC027B398CA47A5842BBFF296C22CF0E43139C25CEC34594D08F41228C11ACC405C4627C017F813B7117C66995D37811AFD01FDBDFF12EA3BF6D26B30C3A4B3927AAEEEC193AB4AE63AFB126E234FB1BBBC3D378377AD8EDCD637C0C7F8A76B582BF441CE41F8ACEA249D814E702A94EDA2CED94F648BF926EC8BAF21C1D184EB66E6DCB6D3B6781B5D2AAB3F65B71FB4348A11CD27F103DC3C568F71389E994EF3AAAB837E10FA853EC3A632E0EC061149909381D6B711E45F279DC88DBDCBDEFC56314A5F7F13AED39C0BAB87B7E88F56683D870623C9BCC6AE96CF7328BB3F7D8E75CE17E1EE2293C973FCC6BF8643E9BCFE7757C1F3FC93FE017790B6F256CA1892CD14DE488A878584C1073C42671595C96C64927A48F654D9E292F9713F2A774441AA08C50462A35CA1AE590F2AEFA3DAACE77E020BC05FF74E105BE840FE107E14556283AD153D129AAE70930899731AA54B61357B24518670F48F3E4FEAC3F96C30D9143B1FE0DDBCC5A587F5E868F62054C67F9EDD6E48E6237BDC5C43B70551C23DF4E91E579B28E8BD9755987FD74E02AA1357FCDF344949F803FF3F3A8882DF017A1611A5E653BF808AA82B7C500A91A22FC35D8CB6B71111C644300B4BBEA6AAAE372DC4D73613416E06D6E0367E554457DF847B0149E607F82ABD4C72BE1159C24A6C08B50880BE1326CA7AEE8213D29E7CA29F83B364DAC621D300E4CEC22EF4AF001E45247781E6BF846F93A3B0B73A04968708EBF4EBB6F627B7999B8218DC2A9D4018B6039D4DA4B60BE542DCEE014E05805D9E2024DB785BC4044E8FD599A2AE368A61DA2EE3E427360202FA36FD2A97286515D54D284D848ACA73921A882A6518F7F97A6D82988CBA35902A64841A4A903204E58A360ACBD1D36D853E049FB65E849F36085BD902CEE848F610DECC465D602789A9E4ECF526F0F9386B22669A8DD93AD62675905ABFBD7FC52B4B3311DFE4AECA50F03A4A3B04ABC0F15506AAFB6FF48D5FD204DD80DF0389D7E9BC9CB6BB4C2777803145AE5ACDE1ECA9F267FCFC3487B879D851A4CB567C0703806DB1409262A51CAF13E3C43FE2E80C96C943D9B4FB6A6511CD650144C8AD61C9A3F3F12B562A9B803ABA9E7EB68DEFC9CFA6637758ED3FBCE557C8FF570F7AB83EFFE3B7CF37DA4D42F461EFE5FF8F4EB871E663C3C3C3CFE5F28F95AA8F0F0F806325359A16CF1F0F0F0F0F0F0F0F0F0F0F0F0F832A8AF7B7C63897B7878FCEFF8F2BF2473FF83EBF7D1867B787878787878787C750080210989000E0A0C8A336C969504DB607600493473D014D18CD04995A566C68FB17CF0E1067C08D2A3464BAC2D566EDC8C95B5C5A09474A395447E5E242992944D0241406B9837B49A12DC85B068A0B5601F00AE918ED0723EA8DBB72C5A5D2FB3C1A3ABF7339012EC4DD3AFC664CDD74FC4E47E88BD9ADB9AA1B4ED5269467D17F76E0EDD65206BFE13DCD74FEA2B62D0977EC7638C8511F184A6F99744B6AC4F8F46695735B132E3AAD14C269A8D6B505A5A66B45D7AB4A2FA802400D18819B13163F2F332DE02504C596500E9A5A59D1B0B7AE5E58FE9C0930A9338EF5D9872B9CFF9A2AD4D3883FB708875B4F596B5AEB1917C18CF0FB0EFBB3EF8E1278E0F8701ECDB07BA65174909FBB6D92DA747915FD61409048224C9FE6B3E55E59C81A2C6B490EF873EE64BD80D664A2054E43B875CC4189A81A422ECA4D7EE4877B61E75226AB4456B626E609DCDB6C5486052724989F3CACFC36834C3D451281A483253A1DD01E3785A89EB00ED9D17BAF2A582C69E1FE437E6F1039876E38675A55D3A99D844591F4B5E842013CB1D2FCCE470160E56BB647665C8928CAE2150D3061AB6053AE8684215A4D99F91C7FE7B7A0B7D1F40D3CCAA4ACB09FB30CB0C0458A52F6C1824B5508864BAFB4DC2BE69EABA2E57FA3A67651A41BF3F8166BCCAD002817685EE916206AB8C308601C1B50009BB25EE187115C70E299FC775DD556EC51D7BE0049BCC9056D3B5FF38A720A3EE45712319BBF7B1E62A09B73E4BDDF21C3CDF2CE6198A2AAB922A5421774AEF9CCE64BFA66B018DCB29A91D533BA4723983A745303948225DED12C1542D2902D128853D97AE255893510F468217C667808A99A4EC9F818C52102D8D52020A93220569A969A9C9291D599075CF8E1414F7292EEE5D94F3AD9CEE914D7867CFD8C56366CF2AFFC1DAC665563D96ACDD963FA4EC9519E56F5827A5232999C31EB79A8EEFB0AC5D130BDE28CE1F7265FBA55BB95DA96D603D801CA28C197C8493AF036AAEBF3D868C94C34E5AEA99D34C8741B55B4CBF132C35184862952C615F8B3B0AD5E735F34147D3939DDB5248E73E40A6FAFC41507D4CF3CB4EF4FD8613713F45FC90F32BBF41A1BE14BF9797DBF7F3D2DA9E975E14DE46575097353418A74F372425A79544A36E954621A3BDC3CD2C25ECF7CB95B22BB92B852B2557AA09FB33B3BBA331DDFD85ECE49805DDCA71EB4773A5E2ECC049BAEAA43FCBD17224D4C35A7251C81592CE01837E5055649AE3B863CD555C234759152483C1AACC00B80B817CBF985CB3808E2F377BDD748BA634166B77A6A6DD1BF702576698CF020BA91D59862AE6EACBF5DF5228F547F44742BC87C80E7C3B58CD1F137303F3822BFEC17895C746759CF19977EFBE7DD77ABD97D77BFB506DBCA78F35165E2E436CCCCD1A1B96B84D30870CC12E0E4792829A386ED2FC91485C55A346346A71AB8672E31AD1D0CA8A688253240869AB34D00A14A4680BAAA888007BFBCDDB5DECD256C95BBD996FDE7CB36FDECCEFFBFDBE91049162858454272FA1DAE8797C526897E6C8C643D48FE803FC0161983EC273664A91E5304B595896124C921466053005D37265394E628A120483511425499655B24FDDE6BD66CA3C4A0D4304464EB05E6104474E9B0C4080B9E8321A7341654819BD49D31E118BA3F0D93216C1971A814AC168B61142B810CE480F6708F3B329E455B6A9581DA15267BD6C37BB97A5817B874F6A333BED550EE0556056FB0409AF8CD3A166A0E59CD6BC99263CD4A47356E1E754339921B6A66AE8A5B1A11A3BA92261D4764C5CD176CCBDAC6BF57964CA3E04C45E4354F65A434343276E3B6682BECA655DC7A8B9C7924BBB00D052F6ABE3B29174CE5DAB37AF9EF125E46A5F421A01B33E2147EB75F3F40C783A2391DBA7CEFEBE34EA4BE374672722FB258902125859300994A2476B4CFF35E722165B6D75F5D8A705341CC0DA211CC46BC256472D7E1AB3E72653BF9E5CCD8E3EFCE75B0B97FE987EF4A085F9E8612D73E3A11778F430F0E87B109576E4A7F6E83CEA338B3236D7B9BA3C3DC2160F635075B8EA25AF9741E07F9DCB60CAF774C35430C482611EC9FEFDA4D91987FAEE497F455C23EDD28AB89AAF957C0DFD7F3A595A9EEB077F355F93FEE4536094C9ADAE56EF0A71AD6B8BABDFB053DEA50C1A7FA01C947EA18C28B7E52F141542CCAB29164D5334C5643097503EA7D5C899355532B17683C16A733ADCB6DF662F4CE37E502F123A361BF2F9895A20BB5D5164C1FD1F72E19E2617EE825C9C4EB9CBE5B7B991EC6D9D46B802BD837123E9201FCE716489B8B437B82DB8374807FD764A07F3A994BD201E76A349CA6986FD6B3523C78388D343FD7F494760E6704E750BE2D19E538FB42E1F8E9BF6BC7E1028130531278020409F9B12216006ACD91243724D15FB920AB0C6E9AAE917218A3400366914924A42511B357323C125EED3512C673F4F3A1D09CDEF4898E19693AE84EAB7C0ED81BB380FE2AACE921306876D848E26C55E87036105408CFDD07E2C39CD1A9953222F3C566BB185E3417D6C4501BA8602C90968F058D79F80EF30F5FAD8A5DD1F5E69AF5CB5287BEFF7ABB676CCF0B5FD0D1F1E3CB0F8E0BB93617674C9C55D6F5F2B2D0B2E1E98ECC39157DE6810F989013A56BF6BC1C65701ED2DD9DBF47540BB864AA9221DED2F1A29462A93E2D23C89ADB5D4BA3AA895C6E59615AE0DD4B3EC7AC333966ED705CF55F693A2BF3A6E15DDB2DCB17DE9B8557AC393F5583D9E2A6793B5C9D9E6DCE679D3C3D75041A9C6DA48D54A6DD47CA9C5F294ABC398923648B7B82FAC0FF03D59C5C5B42CAA0A2A7189BC868CC52E5AB403897D8524022C00991D6C1D7C80901846E70AF83B9542659A02D89C7255B2F71EE35429F8258329A54C552F6B58D5925AB7B657633C4951A456E5721BCD4C50A6917C462330D338598652CF7234A25E22C19826AB2A47DA39BDD10ABAA29D2BCCEE4C4ADB6E16F2E98FD99407B73907EE3329739057F3CF085B109CCF4CBDCFFF91BFCE6779C6C337F34B789A779379F176826DDE4D66C0EB62C69B749671EA4AE970C7974E8376BAAFAAAA9D8079621A44D37DC0DC5043BAD97493E03C03A207B746124DE0E934261C5A729C2E1EA14349632F2D62594646B1049AA77A455ED113CFAAE69839A1A73EBE5A2EE02F2FAF8D9BEB6251AB0D32696CB1C6A23AF8FC1CDDB07E6CCF27039BAFBEDC7D207472C2FBAB81E77F36FCC2CEC3AFFEE48D87EFBE83E9D797CDA6E4072D94F9D287BFFBE02F97C648D63308C9EA07CC2CA4615EC7DBCC501156191C60E2CC5C6605D3C36C6738832618048354A41924440B5874713CE690D150F9A68005BFB70817517EADC01E5A61EDB5C2DA6B651891945C8DD5C5EFC22B91175D4637E0F84236AF90F62435B2DD8829D0493E0722BB8D0826AC8AF2389910746A596C5E30369595EA1BA067A637D5F4BD7E38D93437673448E813093DB147EA1F86645D1BD3FD905F9E4546CC19684E1CA1EB4EF4723C89F468342F56B1E23A585E1B4FD694E78AB5C19FCEDAD4BC66DDAC397366AEB3B899F2C37D0B1B8F542C68EEEE9FB84AD6700BBE4C6DA43F02988773E7151AAF48CA06EE92178521A8074C1D47C844D31914CAC0A9E8049D34207BC8394E0E1371B2914029B0AD5BF66FDCB47FFFA68DFBA98F37EDDBB7096C84B3A3B80C0FE32B707AB49F47147507D2C92F61CBEE1E67714885AF843FC4BE5A1F1E9E34E37FE0B2A32837862DF9FA316CC98377D86F4F8DC1E8FF8DB935F51E34398A5BA6C608DF608C80EE8F0AD3C6A8DF608C8AEE8CAAB9312AEA415DCC1A6631E2E13465431E548142A81E35A3056809EA404FA30DE839B403ED411793CF6CEC5DBA72E5DAD53B5F6C68DAB6BDB2BAFBD9E0A28526615E9241C0EBC8E50D36550783D54DF46A573C6C5155BB6B71EBF3FDFDDFE96999F3BDDD75D1AD9BCDD6E5298A6B9C95829F7F5D97DBD9B57B7357D7E6DD748FDF287FABA6A6DCDF83429F8F2742E397C749EA1A0A85D4CBE3EA38C00CAC71624EBF753F1CCAD5EAC739FF279CFFCB1F56C012F0D7C663D18A7C5D94AF6DF9BAD0CF3FD17EB27EB2FFC976D913FF5F781F7D351C8F87F791E27E2C128B048935591F85EBBD582412A3969372C2491E502F3FF69D381A8E47A3BA33BE48FA26D792F23E71DE472CFA001461684D7E1A8B45AE43031F042345FEEC0528F0F968A876622158FBC3E138E5CD3B4DF260DC26C3FE1C0FC76BC0205137ED8A90F6CF11128EB257501C6D4ECE1B8CE01D115C59DD504DAD0AE096005EE0C42D8E94839A6FC78306BCC3802B9906862A897951B9B71229A25742356E97CFA7716E2B2D539526804BF3D81890482C168A6570E8B34C54FD2CA366A291707AEAF269F11A2AE097A962C86C63C5B159742CEAA66CF99A3C7CDCCFB456A5BEDF3170A82B70E18CE0EAEC1B5CD8FE5A7F67A950B17ED70FDBB78EBCD27A01FA570F1CEA0CD0AD43BFF96EB4E3ADF7373C8264E0D358C7EC32CFFCE796CEED5D5499DCF7AF538F7E39DD0108064EAECC6B4C1FCCB8183526FD0D263C8FC37329CC0455C5AB508A622BFE37CF55CF1A451445E7BE8FD9F960D999F5CDCC8A83CE6433851934EE8C268AD16C231A0B2D84B085BB852E4454C42DA28474828498C6427111516327E8226221E9846DF2036CD3185084E92CB30FEF5B25C57B70BB73DEB9F79D7B12CD880C6258A96179A9F6954E292B4256ED02979B7631E674208E34D7D1E27836CFCE93530A78BD0F03988058FE94BF6473EBCFED679D34EB3EBF59B09EFC2D77E50FB9F33ABFF5E6EEBD7EE7A8D20091F06944E2699BCDEB7D012B025A021604886A356154305A656BF60B9B3CB061C986451B2E607A2A9713AE0BAE97F9130E2B1C4E572E55C8327BCC08732A9C95A8971012E8A5443323939854A86F7BEA0B6760584AA4E17C3ECC14970CC914398EA592C6D1BEADF114174768EF97B5FDBA7122AEBB75CC2278AB74E207F90C06949C4F0F74F961430ED80074A88A4306B10F0770B0A01B7BCBF4D55E97F546FEC91BD191A539B2F3FFEDB7917180AED66A1E13BE72C84947F5A73BA9854E1885340CAD899A9168566411CB13A296964A66942A129FC1541494552172374716A3A13BD624C3E39E41EB5250E9715A1F23555D850BA8E70710FFF3FD986DEF7E4FCECD5F6EBCDB2261F7EDFDB98FEF57EF8C3A7076FDE9EABAFC04B3331753573AAC172D3C6C3DDAF459E3255C59EC5EBD8693F357800100EDC5F41E0D0A656E6473747265616D0D656E646F626A0D33312030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E6774682034343438322F4C656E677468312038383538343E3E73747265616D0D0A4889EC565B6C54C719FECFC5EB656DE36303BEADA1B31ED6C65E1B878BB1BD3666E3BDE04B00DF80735C12CEEE7A8DA1185C706948F3B09542828E416D9A280F79695A45158AFA300B7D58FB25B4528B54B5955A2925BD49AD00A9118994872235A5CDF69FB3EBC57629222A2DA59D6F77CEFC976FFEF9E77A0E4800E082242840E2D3D1993D27076E03F4DE0490DF889F9925EB27E4EB00878F03682F4DCE1C999E36DCA300E117B19171E4F8D9C9131FFEE65980D7FF08F0C30D5389E8C45FEE6E1CC388DFC5B2630A0D6BF654BC8032C6838D53D3B3CF5F8F965F009036013CBD70FC643C1AEC3B83B14F7E8CEE3F4F479F9FA92ED5BE897E27F2C9CCA9C4CC95ADDF781560DFFB00A5AF01CFB5B0B691FC68EE77874BBBEF38AB390DE0DB37D6FF80D7B78A2FD5DF9DFDDB05ED39E730AAAB6C3E07D6859E4FC3705083BBB39FFC427B2EEFC961758C5BCABF0273A0410FC8F8D320002F03147C80FDCA20393AA55ACE9417CB3B705309C13915C00B2B80BE312C9BF33AC0895CFDEA4A2EB61FCDD5B195BE07A1E01A680507A00ECB00CA54BD014DEA69F0A0DCC775EC7F9BB21E9A50AE435F63E17AE45ECBDCE27EE4D5D9BC03D8EE3424D1DF837A1196F2C28BE07E98FE958BD0AF42E62ED611CC3D84F533D8E73E94776229913BA15BEECCC4512E4379A7A313CA502EC612C6769FE4E6A904739C40FF5AB4CB9C8B399460EDE6DCC5BEA437E15B9F656E041E2D708F5463A97CD471F91EF907DB3BF0C1A3EE47404040404040E0FF13D2A5CCC2E3CEE16151E07E727215101010789C9020B3E0C4A281B837050404040404040404040404040404040404040404040404041E1DD4EFC3E4FDEC726726FE9FCE4540E04981F4E6E3CE4040404040E04980F255D86AD7166C7F202F0D7BEDFA0F105BE9933BA15B7D0F1A1DD7B05C82467512A8E33B58D7814BF93154A8BBA15F7D1B6A94D7A14D398CFA155BAF92D74183FC35A85567A141F9393414D4401DDAD7A849E8572E814BDD090DEA06A890FF047B94EBD0AABE044E7535B81CA35083BE5AB51C5CFF9E59F9EF039FABC79D83C0E3857C00DA7375D7BFC2731642CFA3CF4E404040404040E07F184AAED48264EBBF440D25E9B7A0C202EA9B80A0E4040A8DD00C9B611BB4C1419881B3F022BC256D91DBE55F3902641569F5FEE4AE9AC9600B826D16994F43144EDD8709994CE6C67D7EF1CCDBCEF73F95FECA6EFFECF6A55B3B319AF450A350014ABE04EFC2D59233255FC6468E7B0D2559C68FA7157C742A6A81838B45C500A550C6C50AA85AC2D9E8AD6FD8D4D8E46B6ED9DCFAD496ADDBB6B7ED68EFE8F4E7FDA17064775FFFC0E0337BF6EE1B1A1E1985FD070EEAC6F8E70F011CFECCCBF02028F7B1BD7BF5BED4F7F8E3D759F9095EBFC0C173B3A74F7D71E6E489E9E35F387674EAC86422F6ECFEB17D7B03BB7A767677F93B3BDA77B46DDFB675CB53AD9B5B9A7D4D8D9B1AEABD1B699D877C6EC3FA5A774D755565C5DA35E5655AE9EA92E222D72A67A1A3405564099AC3346212566F32B59EF6F5B5709D46D1105D623019415364398711D3A691E5CC003227573003596620CF9434D20DDD2DCD244C09FB698892B4343EACA37C31440DC23EB2E53DB6FC755B2E41D9E3C106245C3515224C32499845CE4C59613384E15245AE200D265C2DCD907215A1588412ABA43329A9B247B205B932EC4FC9E02CC1A4580D0D8559350DF10C98E20D4727D8D0B01E0EB93D1EA3A59949C1388D31A0BDACD46753206877C31C41566877438EF2D1C01C49355FB52EA4358899BEE2093A113DA433256AF03ECA7CD86F8855BE70B3EA9E8AC1CB83FA2B4BBD6EC50A571D255CB5AC57087B6B585FEAF5F0A761600C267B23A615C18E2FE0140E8E12EC4B3E67E84C3A871D123E0E3EA6ECE81234CC2DE631C256D15E3A651D3371616A2C0623673D976B6A02F399DF434D9858633AF5B05D6E6A4443B5A9B5608D9CBD521D20D5CB3D2DCD29AD2C3BADA9D5A539A1B864A990C8FB6CC9A6736970243FAF12CF88F6E37660244E30139DE2983AF823D10156BC03690843C2566C02D7E3285B15342DCD8F768DB767055E8D12EB0EE0FAD38F3E5C6E89E62C0EAF7607B8C877497EA3A17F51663E1F6B6AE21BA430882B8A39F6D87A5B4BF399B4CCE88C46B0C2E983219CDBA8E16FC5C9F778F8F2CEA5031043852587F5AC4E20E6BE0C81569FC164937BAE2E7AD6EDE79EE4A227DFDCA4B88FBF675F03EB98B33EFF2FD52AD684A7FC4CAA78803B91F50F8ED2C1E1719D842D3337B78363CBB4ACBF23EFCB4952D68113CE542FCE543FC5AD3732AE7303FE0BBC111A3E6AF6E151C31CD99AA0AEB865232BC96EC50E85FBF7503E3257F4621E4BF53AECFD3F912E74E206B62D128930CDECCB3E0D97C7F3908DD2998F792BBBBAD72C3726E6F72DD7BB96E9CBD22BB6144C58AD9707C7C62DCBB5CC17C1CBCAB22294442CD38AA633C918251AB5E6155DD1AD99B0B9B8FCE9CCC29C9B452E18388829C9DFD24CB9C7B22652A078C7741670A7245B680FCE196C9FCFA02CE6A31EAA27B093941F8A3D63661025197A53543A3F9C0A48E747C7F5790DDF2BE7C7F4CBB22407CD5E23B5117DFA3CC1ABDEB6CADCCA8D5C215C814109CFD265D969F3DDF30180A4ED556D83ADC7D312D836E7A24D82785ACEDAB46C47F5764701FC0288A7D5AC27B0C856D1E6CCDA9259F6A61CDB891E8D7B1600DF1A603BB348A132A6075CED017FA02BD023EF927146B8E9325A1690DB25C1951E6997E44E61CC11DB9C9692A9AE807BDE8E3492632691C96DC9BC0D33E7B42581B0BFECC0F7DF1BC1FE71FD4A0F607CFB898C5E0E7E5F62124B4F827DBDF05360DFA5717C814D62CD8FB049F154D38194BCD767D7925D5B03343C810C5EF00DD1865979C884C15994EF0EBEC2FF94242D21F17BCF0E6E695D8B9A94D350C1BFC58E2C57A7F26A84177CA17A37670F08EE677B6F7AD831373B6EF8F294284BC688859BD8CF77B2DF6EBC9B17130FF66E968C47F919C7431FA768184003D1636E8F8101F97BC5E2AFF978149BA9F5F99ED809DFB290B8F9A531EC5AF6F2E1B0E410310D62E2619186753CA88415604D26F15D4FA3FC800C65C73384771556516B14DB022E84E1668578634D4613941F6FC617363BFBD9BB6980C1A8CEC06D59D46212A6E88D2019C3D733477D3FAFF03FE3A3D104FF0C99E45F2189EC1B12D3B56787477387E9DF59AFDAE0268E33BC7B2B5B87F159B2638C41D827F97CC646726C94124350F14948505B6E7080804520B22D1C52026353039DA6361E2675279052984249020D76D2E032D88C4F27A032D0E2C94CDB49A753E8DF0E05A7A53F3A6D1A2769E9A493D67D76250399E14F672AE9799EDDF77DF7E3DEDBDDD37962089174914B240E2BAA9353E230FF93B3ADDD8B4C141E2E3AEC5E71182B7B1B36A5AD2AB1A91D1BD8ED74AF718B5BDDE1420D4968E2B5183ACA04CED17920DA8B5F95B9DB9BDC66D71F58C4AFDB9B099645AFE29967B6CE86D8C50F853D5E539ABF1C4E7EF1949FC799D399272F476F427A0DAC2A176FED36A58DD99332D3BE893775CDDEB04C3358C4D6148F539C3D3A7DB5F5E12DBFD57C2CBAFE3917125B8BC74C308F6C641F4917481951D9DFD8872400FDD0CA2D53D3ECAF29B6446D0CCE6377493BFB3319627F2277001B71C2E244A911E8417906C89999641FA42211BF91867A1F176A55D7F827B8C35AB8C8FF53F6813446161315863B56894B786E5BA150B6F0E4F24C21B5A4D67F2798C76E938F0089DD6677F01F5CB44A553FEE9F0E2A305076803828252A1966BF2726201183FD2E5559E51FBACE7E0DFFAFD8FB64BB68F6BEA514FAD1E12FD94F48112EEF32BB94F55C4A1514FA49B0971DC1593909BE094C01D3808D74B31F9301E028300ED88803AC0275C03A6E61A36C14F33C8BF60E701DD00D1C056CC8EC79D85FE2CCCEB19DA4026DBFCB4E9079D0D7D871A1EF421742DF81BD1CFA36EA5C87B2F5D350EE3F95B5BF897A09F48DACBE0EBB0B7A1275AE3FC8D6F7B37DA2DDDEAC0EB35EAB5C7506CBE17703F50043E9044A2790BA13FCC50A4CD92B6C97182909F543776714E9EAB73C9AB847FDA9F90BFCC348693F52DF8FCCF52373FDC40657DF6C4C5F26A696F521A60F317D88E94356EA592FC6EBE56FA16027E00618F2DE8BBC73BB099E046E0AFBB7C1C780615E63DF401E6B30AB436CA755AD6291ED48AD30FC8D57D90B48B5C15E482D28F31F7D509B93C71722B420AB0E1EDB25BC5DA939F9DCDA955A58965144BD142C6009F22D4022C5E04AE04B4018B0B1845559A75E614F93DD32310AD40169800DD806726CF5615A749DF949AB4CB0248B582D09C8E4B21A0FD086C1E1E020EBE4FFE9C04EA0073806D870B571D8DDEC79208EBCC431A9E7F9DB1F98A0E6046EA23C05CD41CD813807E21CB03A6075C04AC0DCD30AB4033D596FEE7DCF6C1B1E3FCD3DC062780B602DC0554E81A7790968464D414D414D41D44DE973CCD0097603AD0013B62900F70F3CEBABCFFADB815CE19F1631B33E83B7953E377C8B276BA85943876BE8B11A6A041A837EA3025454543478B465BCE57ACB8D165BBCA5BB65A08535A467265396B7DE2FB442E77AC95AB0D0DFE008AE94C631B3387808B80330A282EB8046A01BB049E36015A75B1DD008AC03E2400E5A5CE07B16AC667DDC3E247CBCC4FDD217FC0CD730663DF5C4BAE057718EC5812180A1EF31F8C74474A6342EEC26784AD8D765E387855D05CFB661A20D3F3BB66459051A8138D003E4901B6C33CEDDCDBC7FB00AF400E3808D6DC17733DB2C5DC0774C1A633E43593A4F25252538DA8B0A6567D029E5E3A62AF49CE037041F12DC28B8D2286856EE352B3F6B56BED3AC2C4641AA2641384E08F6187383CAC5A0B22EA8D40415F4369F788822CD139CCB99FE45F0D3827D46B147F9CCA37CEA513EF6286F79943D1EE5CB1EDE6E11B68522150B9ECB999E14DC2CB8CA98AB2ABF5095CDAAD2A02A41859EA1189D8404970B7671A69F5C74841D64CE55FA0909A3276A056A543CD185D0192B1084FCC70AAC85FCDB0A9C81FCCB0A1C57AFD1CFA8785AD07B56E55D35388FFE9D36D978FDD3AC7E4C9BC828741ABA033A42025487BE6B050EF2F81FA1FD29D4DF2115328F7F9BB48A7643B449D8DFCAB6FBA1E5EBC4A8A72DDF3731EA29E213A3BE6EF9EEC27ADCF21D827CDFF2ED821CB5743EC19D5660891A2CA43B48A5C463134497F84C5AB2237E053DEF82AECD348E583EDE2ACC0748D3D596B614B298CFF21AD548AB184EB53471916544135D2C229A98B48BE8420BA8434C5E211542654B3B885E722FEA77D57F06AEF20B27FFA00EEB8CFAC76BB8BE4DA8FE813659A3EA6F2778BA2CF5862F4DF5CBEA6FB4ABEACF2BD37493A54EFAD2321CD77D69895E529348B28958895E56C77D3BD40B9AF09ED5E0C5AD1E0AD4AAA7B52DEA9B3AEA967AD0778D4F83ECC6156F823BE65BA5B60446D5357A9AC26D04309891A73EA57D5D5D01F3F2346D4A8DAA4B2BD37C2AF5E863F4B2BA0423566998CA4575D9B3CF365C9196113BDD67F8EC7BED9DF64DF667EC2BED4FD86BED6E7B997D91BD582E929D72819C2FE7C9B29C2BDB64492672717A66CAF012ECC3E25C27975C1B679B283B25CE207E904B5496B07BCCC758548A6E0851B3284AA21B436683379AB6CFAC37977BA3A6DCFA5C5B92D2EFC55033A557F16EB6B10D4B949B065D78196D9B2094D60D1E7171ED1B3C128BD1A8399920D14EB7796F03AE240FEFB3395AA89494EC6F2C6D2C5A55B8624DF811D49E65EF834FA9F7E14F6959C83C19DDD0662D3B7FBE2C1433FDA23C338372D45CBBC1BDB56D42DA237547C213520F9758DB047D59DA1359CFEDF4E570EC7E18A9907A1046025C78588A54F03052415322AC458461BD5644C2C98A8A4CD07BB48907611DBD27827664FAAAC410E8AB950BC2A4725229FAAA94CA79181646A633C7C39DE513EA109D39F289E86C110F4AEA3A427C3A0F4936E80848EA0DC23DFAC0ADE999E9C4882EC6D1694C8C43E98398EA4C0C164336469211E3FD7F7EBA42FF43304D75DCDA9E88746991762DD205B49BAFED7FB194BFEAB993DB6F7107DEACAADA3B132F72C5CBCE2DAD2B6C6ED7C2EE6447E211EE04777768E124494436B625134657D8EA303A225A4738961A19581DFDC25887EE8FB57AE0119D0DF0CE56F3B146A28F7047B97B848F15E56345F95823C688182BBA3E44A3AD6D49998462ABB7663425CDCDC3B668C7FB61A8C4D9B34AEC91959ED203AE2B3682E7D75C6FCCCCD742A60270576DB036C85DD8A4DC5500B323EB2A3DB0D2E3BA42CF655D4E980BB510F192D2C8D7C2F77FBDBDBD7B7B39EDDBE705EFDD572A8C7BB1793D1BA2E69A67F02A16300311D3680FC728BF1F086C339E8C6B713D5E1D3F6BEBD6BAF5EEFFB25F2DDE355D79F8FBED7DF6B9E2CE749446B2AACB4D6F84109A8878452A2109D120215E315E5145D02624D132DAA44D3D96A860D4632D238C95D2EAA00C45CD08539D4135516FD37A2CD463A6AA6ABACCE0EEF96E4CD79AF91366ADECEFFECEBEE79C7DCEF9EDDFFEED6F7F3BBAA0DAC98ACC8ACA8ACEAA76922393A392A393AB9DD8C8D8A8D8E8D86AC717E98BF245FBAA9D92FA929B3A22A5C9FEA4DA245590549A54995495B435C93CBEDC74BFBFD6AFC6FA0BFCA5FE4A7F957FABDF0DDE183562574A5295FF3BBF2E61264A314B7A5ABDBB25ACF90B9E1697043B5244EF5A8D0B290C290BD14D422242E2425242B2434C812ED5955AFB74AC4ED6597AAC369451DB3D899D58A5F471133B2DF1AEF36EF3D678EBBC669B5BE3D6B997DC3BAE8970E3DC1437DB1DE716BA65EE12779D1BB2C45DE251E3BC85DE32AF6EE28DF0C67953BCD95EE3F308D8B7225A304625252D529A78DC349FB7719A4FAB345F48A3345F307CB9312531A9237AF9F122F5B150CB7740335A24AD132D8766F0271E4FD0AED07EA03928E7F1D7B40DB41DC12BBA83EE901E9E9F168C416E4C9049C375FC8EB8CEF1DD76B3CE9BF8B8CE19F9B84E1FF8B84EEA151FCE7A7B72A7C6BD7E41A92ED8CBE311DA79DA2DDABF6846C7EBF8FA97973C9E83B945288A11760B3C290E1E8A628A25867F24983BC5453131085A70BA329FD83446FE7716438A4A50540466172B36AABF5A147CAC2458FF547803F5EDCD62C0F4878FF64CFDAE0EF632ED2AED46E005FBD04C4564608ABDA49B7139FADD7F0C88C20A54A115EE48471C440D17A8F7A8E1B2B11C7D518BAD7802B3E428E31949E9B489FCE7E372D6076162B01AE7300A33700D97108D4C5C90A67C4F3A0AB9C3EC6E6FF2988905760F5B35462AB660AF4C931CC4F27F866ACF5844A1D2D6200CD1F6983DCBB3DFE09AB4B21F2183FFBEC193DC479462299A620A8ED887F4B415C663A3CC919B148DE350E124380BED54F4C04E9C924CFE1B8059E66CC84ECA9EA5D8206152632FDAEBF82345C24B7CD35B58408FB7A3463DA753CD3A44A0359EC740E4F1EEAF704E9A49479D62DBD8DE7635AF6EC45D15A33ED31EFA11837E188B77B09ED1388DABD4385EE94CE9B699382EB7CD59FA968912CC46193D7F8FCF7E883DD2513AAA300A5FC51EB6C550DEAB4435BFBF03759229B952230774B5890B24DBA76CA8BD6E2DDA61043DACC2017EE39EC4B10DBFA0FDBAD869E9149BF8476FB28713B00675384E3F2E30EEFFC07D69475C566FA8523BDC6EB2D7E84B238AA26E18849128C04CBC8ADF72540FE2537C2F0F54085BD63A87CC6C73C72E636C5BA3377DCF62EB1CBEBB82A3B41DBB89D3ECE59312C15E74938132582649A5AC90DD724ECE29573DCBA5FF96DEA68FEAAF9C2EC6D844BEA9395AF2BB91188EC91C813718ED65ECEF261CC2610995D6D2813D3ACDE77F543D541AB141D5AA0B7AAEAE741E9A79814B81BF051ED885F030CBFA320E25F88051F84E9AD387B632458AE40A3D5FA27EAF9FD04D74A4EEAC7BE9213A572FD0CBF55FF417CE0C67B373DEF4337966B3272FF04AE0B8CDB46FD7CB2E977EB5417B24A02BF36722B3692AFD2B2466600EDEC4422C66BE2CC33A0AF9DDD88FC33885AFF1778E00E459FA9CCFAFBFCCAC9B2B8B89D5F2A11C904372582ECB8F41283F11ADBAA86495AAFAA8496A2EB15CD5A9D3EA867E46BF48162D23D6EA5DFA1C571DC7B1269EC8301566A37BD413EDC9F08C6FF4F9C36F1FB57B94FBE8420081A703BF0CAC081C085CB7C3EC2CFA1F850E788E9ECEA797AB9983D5C407CCC45DF80C9FE34CBDAF77458961C6874B24B3A13D472D59FA523AF593013288184A0C9791449E8C97C944A994C95B522E6FCB3BF26E3D56B16FD5F2BEEC223E96BDC429B928DFC82DB9AB98C44A339BA3541B15ABBAB3A7A9AAAFCA52838949AA80285433D44C8ED046B543ED51A775331D45BECDD3D3F56ABD451FD427F53F1DE5B477629D24679833C929776A9DE3CE59E781F1997433D9AC3507DD166E823BD49DE2AE72B7BA37DC871ED7934D153EC773D2631B4591ADFECC7EEFC47F9758B7568ACC53CE6BEA22E745B82E34F3652823E6AA217A9A5EACBF3413E58E8E90F3B250E7EBA97683EEA3EEEB0219A6F68B5FFB4CA29E8845B0B2595D56F7D475275486A89B12ED2C958F55814EE55695C59C70429D72738302FE0C12D5EB52A30EE9725D6EFF8044B3562E9AB5EA38229C4BAA192E7256CF572BF9D0172A5F55608493601E209F717FDFBCC678F7540BA49D3EE9ACC5351DA97EE0B6710559E398BCE0B452635477D94CC67D242DF1AD4C47A1BC8B14F944BE96DD94FA9BF446E9AF7EC6D1DAA67E2E5DB99B38A69F9593BA3172833E4A6B152AD9EA8E1AAAF7B975BA33F77375F812B3454B1C73E7A712C02B9C01CB551B725A3AD9E484C4231C2BC9F7F702FB828C6DCE9A0AE6D97ADD1E831187D1EA28123937AE1123300FF1D8CB1C5C8038B50A736C994C20EF0F207F2A70438A58F1922DC3E85B29D78BE6CA4F2E1CCBAFDE27FF1F21EB67CA6DBC2A119C5935887682771639E964A671E4DF0A620246F36C0D96B93BCD09644918E04404D632CBBFC218AE3957F8FDA79144FF4662BDD39E5E479099A7F38935810CA410F37054145EA7CF3D39CFB39D0C32EF0A3B853DCCE71AD59F6BE261E4DB9548E5D80DB6E5B60263ED7A3B0A93906337917F67DAEDE882F926570D33314E0239F6B07CCAF5E8AF5241DECEC079F2519484E316B185FEF7349F60A17386DC996C17D95308653CFC8CD078AEA257F1326E336E19BA069D0203D547B68F2EE40A751183EC46EB93C6986CA79179F7A1DA63C83D656869AA99BB15CE4415477FDBA2B9C4F2EA2853A5CFE8EF9D42349486D2501A4A4369280DE5FFAF3427C2A8B7C2A9625A700FDB968AA31D7726417D1F4B6D9340EDD1953BB7EED42F3DA8739EA78AE94DDDD3876AA23F75561691430CE51E2B17A388D1C4186AAC3C6AA797FECD7E99073771DD71FCBD5D69D7BBBA565A49BB3A7CC8B26CEC4587ADC3D8A8D19A2B041B6A30602E61709A98046362BB33698014B70D784A3B1C6987900E5398F4A08532011BC70B344052AE5C256D93765ACF14A67529A451A793B8864CB0DCDF0A0894E40FFEE9349DEE7E66DF3E3D69B4DFF77BBFF77BBF1F6431AB206F780CEA9B0EA8FC3AA1F653ABBF27211FFA2A64643D50EB7C1D32A45E600B54B35BD14EE0396037D43E2FA01F40B6F633C872FAA1B250D05174026AA1577275E319A834CE4106F73A7A0372B137D12FA1FEFC35FA0DD41E7F4043909BFD115D84ECEA12E467976196F5B778097F01EAA1DBEC208654C86A60F56D74DDBA6E7DE36743F5E5B533D3D90243CC18353599775862960FAC6FDA9EE08FDB3F74EC701E10368B1FBB9FF72CF176E79F2D7CE3D3F8AEFAF34B1E0E5C2A7BBF7C96F45AF02BA1CB91FCAAB5D1C7E3EF550F4E1AAB5D5FBB7EF28F1E284EB5CAE7A77C79DACB0F56CEDC9FE3EAFF36B392F5DDF74B43F527B46B686868DC079BFF23ECD5D0F81C72ACE19D86BF6B686868686868686868686868DC0FB33FBAC39C951A9F2B1ED3D0D0F8145DC0FA7BF806F0AD4FF8CE17573636CFAD9B9B99F761D3AEF9D5F3CF2E98B2E09585A1852F37CF6CDEDC7C71D1F64517176F5CC22C7966C9474B4F2FEB4AD7A69F5FEE5AFE6C4BF38A0757FCEABFC25FFFDF59D9069CBC87B780A13BB46ED3D0D0D0D0D0D0D0B81F1081BD08E9BD7A844844A3F6439BA44587097C9C388128441327FB905EA710278E9088A5D5CE0046AE3C4A7F12BE271089CB118357E3E54894B8D1E458720E37929C3D964429E87337A0A98C780E239D42FFAEBF1D611A9E87DB3112C352588A542EF6597DD60034D8AB43378AC85337643DFA1815E94E21F8EFE1F1617C56BF1A1991889E51551D27F6231762C64FC94C62520CC9725D2C4F814FF6025F8C755F37B725905C118FED43833027857C68D04493269937403F2E9B1062759CEC8CB1B2EEBA8B1BCD8C64ACB69A7006A53229EE726504774A702109CF98E69119138F31CD333489C4540A7E668D825AEC274BE3B144B4CAE9B0D3A4DA52FE627504AF2A5D444D0D87EB741DA1BABA10DCB88DAC88BB530D0DF5A27423521754878375EAAC36917EE2E7302B1372A33E7556838AEBBCEB9A91342AE3D7FBFD8158EE198CC4B0327EA51FA68394F1F3723E745C2234EE49D05C3362DA281809D6BB09266D420A9EDF4F936E333CFBEC2482E91E3199589D599DB7D3ED16ACEC1ADD2F8435C88AAD9B3CDEEFFA1E5F274AD2687A6CF4A6096ED9612C9952974BC29D692977E12ECF0032CBB45DB542D4FD56956A03B2EC2E1BF8EE360821279CC4A49054C3D7645BAB9DF1E0C45A7782F4E392A75CAE546D6DE58287B34378C2BA8972EDE4CAB2ADD9DFABD6088C0F9343600D0F2AC42FA8D69097E5E9699BA8176C3A9B59A0AC169A330B052691B21A694E3415B21ECA6AA0390FAB671065CD6310CDB94C22E1E049BB60760A66C2914FDA45939B707858AF8725ED1831844347DA11C37A3C0A59263388B123C4980541FD04FF69174593C3E974B9DC6E8C09755020ED3C9F9FEFF5EA74E4206DB5D90A0A0A0BF57A4AFD2E4873168BD16830E4E5D19419CCCC324810458F07B19CD56AB73B52E65EE1A0A97785F88448880A619205B6D7C3F47A0EA214415314692130B1AC68D10FD50DD3994C72496E2433CC0D8FA687C74646D48FEAE6511F77B739C6EE74473E6BB4571F929EE64EF78644F561B9E702174F7BFA9D6E9250E80F06DB792F692BA429E8CB96768B81260146448C89C9AD762A6AABA90983DF47A5AAAA9B2DACBEC0FBE351DE17F7F15152BDA30E3FDC3ED2CFFB48DEC7FBBAD3078E4F1F47985FDAB414CF5DD9D4B2FFE88CF1EC3FD27397660FB674E0A699D9035E7C6E1E6E6EC4E7B235EADD98FDE9BC9B3DC2859B1146F3B3B3880DFAAD8847F5397FF0EFB4EEB3129B8DDFB412EC2EC68A76611EC356667E622E6EA430D5639FBF5CB5643A3396CC992E93CA5446505A9D2BE6E1F7B07F55B77594969512710E553B288A70D8850282D8F0DC23DB77E3AAD1F5DF9FE373CF7A3ABB36D0F0E80EBCE51D9CC0E31D15D3DECFEE3CF3DB17B7ECFB1E02552150B530A76A564E5549B9AE226FA69E04395690C5238C1916241551114AA648AAC7717381FF5D962A8AE1F16D517CDC29386D0E0ED1F144C2168F958588D0AE47B6EDCE5EB8B67ECF6C9FAB7E83FE4B15F58F3E9B7DF2DDECEB59DC1198FE37BCFACCBB87B6FC38A7A9237B00EF42E79180D6E434952D26160BA79D2423AC70BDED22198C689DCE9267432FD964A341576B71143A7A1CA443C115B2A1D0D262212C2E7137C884989D9E3D965643E1B0AD065B6D428DAA15777A06C1D4469BCD68505D029C211CCDC94E24406CA9BF98BE15FF720180EA68EB6468DA10B0D92B6BEB1353DAB6650F4C2CDED6C89B183B531BAD9CD1DDD27658D5BD03E2FA5AF42A32A0C939DD5E2453065266E4DA3823A7E22D0CDEC3BCC810CC26A31AA76093744992AA0D4E923E44E5A4808AC0DDF11785E55CD87D35D786C2B2FA9EA6F12BE442FD2A548537AAEF398AD8F14B7DC61A463D3292C69A3A663A3BC3505FACBBC0E0F2F249E5726C45EC42EC52EC1A4BA318AE6336FAD785F6971C2D39167A2D74D17F3130147AAFF86AC0F8505EB982BFDD3F6102871462B8FFED088E28646C80D4734EEC54F09E817C590AC7F2153CB59F33954F388E5721883DC49F654363514F11B1BD686F1151A490D1FE43466C54F076180FF60489EDC1BD412208E3032DF4469AA015E22F322BC7F0DED8A9180167007E6050E64FF204EF8A1EC305F80ABAB56CC35CBA13BC2CDD39A236C31018C05252A62B954967D4B05E1999FA949C08850B4A598B8E2AF6F97D25BE804F47E903E6D252B6A8158775C1565C60819ECF50D68A592644455A71A129BF154912979470EE28A8F81A5CB0AB3AD35D080E4A70637242B942FE69A01D0C4146943CFE483B98205FA1FFD9D7CE99608DA49404EF875BF59804B888A01E124EB5A17CB7BC4670C2068856E5BCE95F64570F6C13D719BFF7DED9E74B62DF73E27FB93B3FFBFC2736758843CE4E881791E34F1905424025234178A485012BD9484C47098C91224A483A4405831650432ACAC62AA4424282096D3555685AA9BA0D69AB28DB44C75A5A0A9EAA2DAB065DB2BD77A6EBA49DEE7D9FEF3BF9A4BBEFF7FD7EBF17ABAA0A87D373603AC580B529737EEFA955F32676F5751F9ABE3BB02EA955CACEEDDEE8231B5E0CCB81C4D165C1D6938B9EED3CB1895F3C70E4A9D6D53F199A35BEF3DCB36716C4FCD5364BB3B574A8AB75C96C7F7C2E29F9F6DED68DBB7F4AF1F024C543AF6529A781161377F35FE5417907F92ED96DD96DDDEDFF317FC02FA4615A6B436DC155DA66759BA557ED8783F2A07A0A9D1187C31F86252E0C24EC2CAF707BBC36971D229407AAE10C6AAE20E2839AACA848F0F1165A3D391A0C6A1513E03EE743154699DD0E6E71F096A6713C3701E6700AF8E6589F30CCFA0CFE41FB1C0646B8330CC31440F7C7311CD680C61E628841030F63882B4313E008B86336FDA36C0B2EE06C130BACF51F31B928604A3066C381B3910D70BFAD2661A12AC0B18BF356387F65BB61CF811CCC05F7803D704FD0CA653B98EBC972D9F96BDA8DD2CDFC96F2F5A4DBD2EDB7643B28519D47C1BC4D1BEB4248B3FBF236EE4297DDAE415A1AE9D2F8AFFBCBB855D0049E75D76AFD1F4278D858DAD71840BDCBA6377500F1C473ABF6AED8DABB634B4D588E2597B4FCE0FCD0F3DF7B03F096A5AF8DC786F6E7378FF7C51A1EAF5313584B9DDFBDF3F799990294A84BC0D49F9EA32E41E5027065D19F9A2F540E0204123FA71295F3070051A1EB2D748BF3D225D055826E195E1B5409926CAAC7CF05BA411F8000D82468E392CCD665DFFBED7B54E4E827C285C25FEF8164F1C0BBFAAF5CC17451CA31149B4392ECB8848881E59AD52D5560D9292B8AEAF35B354A2523D1344BA3B5ED2933276ACC3C32A3580E5615CB322996BD6679C46D26E3455C91B24BA5F4E18DD26269217E8CB46A1DD22ADCE66A274F491BF126B20DF7F1FD8E41A91FF7970F90FD8113D2097CCC79825C922EE137E54BE45DE92AFE95FF2AF983741DDF953EC59F92FBD23FF17DFF7D522D4A4B141820807D24CE4F882A3A4A14D1A37A158F0D0A8ACDED7429EEED44C2414C5435E4C42E67377588587238F2F01DC309890B4212F09FE6B8E287CB8331A3CC862544DD92CD26DAD43C78608812FD0F3CED309C79583BDA4A00C9C37B86236838963B3E7720C7CF829B074D25AC94A9C4F86406DBC2D70E6732CB1C4CBFA388D6FEAC839A977ECBAE2B091F870B00FFE2FF633FDE75A54968A2A709DFC45707C8653B948B44F2FB452900F242C128EB0A044409095E17124B28598D7679441B938F84B78E753FC18CAF931114D004ABDBE5F1BAB5747D437D03D081A778C1A8A914A29F4FFD7D4DE81B4F4EB7B555EA73C09FC2E07A63F6F1A93B2B1AE3DFBF7D0FFCF2FDD658202944A392AFF630BFE6CB97F6AFB044A37C8D56BD16D86164EA8F6C6715E238FE36651FC225B8D9F0D72603D5AEE65693016E3F19D08FC92FC7CECA676377E4CF629F24CB66733B62BDFAF1BA63FAE9C86BFA75F97AEC7ABC84CFE4E127A3D2C6FA0C43941A4AB16CFCC5ED4DE986564D432549D519E1380D8A3FB520B2203A207F00DE8FDCD03F8E0A7C0444ED7518B9AD8AEC229E8827EEAEADA97B34B238B50AB457AE8E1D854ECCE14C1B581DE9CC7467FA32C3199B5C2BD72DE71016E408895726792B44C44B5AF5FD91E3910F7421983132CB33EBE03AD469E9B4760A9DB5DBAC5BE5AD4A37793AB235B623BED7BA4FD9470EEA7D99ABC91BC9BB910791CA0E9B1450442D84038A470BEB110EF1D55C3A1188A0D08CD9D53AAA09C5D369D13323EEF57A604D9CA1EC852A50C546269336D33C96FA469BE7A6D8E5E8FC8566365CB4BE74AD0A4A48AD0AD5363E11985D3D8BDDC08FA6CB0D7E98871C0D1FF28867C512BB33C5F120C8033E0FAE19D16A6B45056CAB2E932416ED761A43740E240CDBA420BB94861A336F806B9CC63D017C948613CB261389A69602C5DD54B62791ED994FFDC52C34F38E62A6420795CE2686EE5CC10467CE2469B6CCCD16251C463ACEA2D94A244C959E9B4C85E33E020459A954A0D55A1589C2A85E15F755E92029CCD2419854E9280566E928A6CCD041ADA546E7A2FE90CE913A94D601E0A85C37B149F86A244CD506D91E90CBE5B85C0F97787893A3A44FBD54289D17EE8D758542A2279117BE18E9F2CCC80B9FD1E4A6C372A1CB03C5EABC3035D225D6E485BB34B119A2898D107D085B6C88CCF9F114F93FACA5F5BA86FAB493D9C3705A630A4FEB510F1383FAA2B43B1F0A03FD6D15D0C881854FF4DDFC78AA4F6F8B7AFDB1161D2E7E75DDD1A11F4EED8CAE6D3C7478D9DB13EB973FDD33F6D6B7DE3E38A75D8117C8BC35CF7DE7525BB43E9C435D3FD2AAA3BEC8C56736BC220942F39E9667CE78BEDCA29CDADE7A68256FA18ABFF8DF7FB648543722A0E834E78924099230899281A3D231724A3A553E2E5D2C2FB511FA026017DAE9DEEE3980063D2FA3A3F25974198965C8C143FF22D4812C491B7646146AC42C6350016082CBA325E3C1E396B88A401EDE1C7326CE6180F368EED841FB493BB4E751D248BA4478966E0D401D3EFBBA13049CCD4EE8940D0A68B129E803922F40B78826DC7C8F45D7AF332D5D229B6B292CC3D92F723D2D85C91E4A9553D4DB4DDE6E2EDC9BA4F457982CE0774CB804DD8AB54C88CA55A5559EA855116772656E1A6C959699A0C46B9FC971FF45020501D5F95C4F56A1C41C5431562174A911A45A68632F74A9C865378D9B4B2C0A7B739118D9C99A5B11369B45B74DE5AC770D5E2B1F0EC6A8CE97479887631D6FE0AF0502736EBFD27F63D7B6C24B7BAFF606364C7F7E79FAF54B83E3A0F9CDC3071F29575C72A965F3B4FE9BF181E9DFDDCC4FFFED859E33AEB1330F26FEF52E58797991A742A9A54A1FA64ACF3C9A87BAB40566CF3A4A9552FF3E7C04FF87EEAA8F6DE3ACC3F7BEE79CCF8EED9C2F4ECEF6F97C7EEF7CB693B39D4B626783E6E3A235CDB68A266C596093A2B6B4FB8005D569D7AE43420912D0514D6AFFA06803AD548CFE0164A2F463980EBA0C24A4098A2A756C427C34A052B5AC19839575A2B5C3EF7DEDD06ED312DFDD9BF7EEACE8F73CBFE7797EBF975AF6487B22FBA467DB9FEB784D7D4D7B5D12A36139A225796F07DA177F3A8973A2A0AB1C31BCBA1A24A642627A2E140AE258AEB3931313831332E264494EC98EECCA2D7275F52F2FD1DACBF79854138647CA90C35226AA9834EBF12651982A284C1514069302234F40025510D8A610A79BC261636B133BAA093576866CB6D37E8F8179ABF53FB1D6EA8978B2AD43B22299645B621AC53BE0A485F569A4B6C7A6D760A3791B3A77664EFD499054457C7C36A857BD6FC325C0DA33883985B527178336860B62EDC9DDD69E005EFF071B32E581791302781650E340F0A11FCDFEE9746782765E0E3968E8D5C557EBBBFF383F7D19F5D57FF7CE43BBAC3BC82E7E763E95B7F6D7CF9CAFFFFDCCEB9F4BA00D484131B45E831EEB065F3B0988F5A3830CAF11B7FC68E2C9C4779C1F44179D979DE5B2381DAB0815EFBC38EF5B1016BC07C4033E5F5A57356258BA6A135374694145120AE93E55F4522808DDF1128C7541F526241523133298D6CF1DB58B5C41A283123E0F9697B781934735F57222A189BE4551141647E8F4C47925EF849787EFBAE44EB2EFDA535CCCDB7AA1075E9D8D2FA620D55D5079F5FEC972A57CA4CC973989412D31542506B564586906759A6DA619D4E9C3A5E59FA17D1C0D3414668635F4EACCCAB5998B35807B660570A6805F855403973A8B37D04D83B541D07A88995739E93F366A5E291D60AA82BE6C2B166D41B07D3E3B1ED76CD406F9E5D42C429ACDC1EAF8ACDDCD30B7B116617F6B4906B6D6007BA4A1C4B7BAB6D9B861425BB33F6CB2E98A8423A0C3B481618F6F28F52D66D02687155A44DD4F644B82658542F27D0FD4DF9072775EDAF598333C9ADB7DE32DC7B1534A3C3DE5783ADAB21DFD7DB9875B70EDB2597CA29EDB963073F5D187B24AAA67F8CBF5454B91DC6DFCDC579239ABFEE6E3931D6D1C7085005774E04A01ED65293ED7534549F70E6BFB80CFE3F31FEBE19FB54FDBBFB6FFC09FB7AF78AEF86F786EF87D309108F3C09E859605E100B047F4FA7DDDD84B02812ACAB84151F56ABAAA104300BAD09DAE165508B17491D4D50C31ED7CCE2F063C2D184804C02A05CECC7039298773944356369BC130CC65EDDC22D785B82EA7CBEDAA7479BA0E0A82EE45135EF48A177969F02D7221C69110A343887124642435C6118D6D6A8C23DAE1E247E4E01AA8C12064E0B91A9DD680176FCFFC9F16801A7C2839EC262F6A6B5720C71CB063CE56DD405757A6A525238A4A068158FF9B9243C934C891B1800CA7663338A244192D14468BBE8FD082718212002851C4A6198E28744EEBEFB8CDB6D7D800F7D10BD71F98085A16CA8EADBF1EF4A7F24E6FEDB433958906FD3A1097FF57D08C8F3DFC05A0C05B1B77D4CB13F75AF5E947494C8E5A566FEA4BFC6C635D7F63F383398AFEDDE0C63F04372EA11798524CF93D1B8A38968DE7B01495623835E00E6C19D82B56A295D8DEEE83D183B163D163B1D642CF9ED67DAD7C74A0189F1CA80C3CE379D1B33CE009F05F6F5D1AE0EF1601E5E8BB864C396096983F9F60FE8C4E40EADEE8DED5FBEDBC128D1A422ECF8772860FD97A3240714C32C89202852C6984C393F24119B7C91332A61E312FAFCA1ED943B195C1282E9E644651C5EFBBADFEC1C90C6ACBE8190CC1F31D57A25F9391E8FDCC3DE5EDFB9BC883F0831EF4D80C78C601407E656485622EAD3979D30D4A29DB2B89562EDB95EDCEF24200825F1B09AF43295D0A7B6D7F810B9A709252A1759C2F2B1450AB152A708D2847A7281AF41A166F532D4134EAA9AEBFB7378FB191EF54E2C009D737AB2846BEC4E705308B93B379DE08B1AC67DC6EF9765F932B4DD7072EA4E860D4B0FD30CD6C65D2019E21748421ECB10C006232D0DF4769048F78AE0079A69E3A53AFED9BFBD6BB0B1B9F19D547EFC3C1D8262DB26BF91BF5277FFBDCF423C70FFDE6DEA776DCD9DEAEF29007A68E7C7AF7D917FFF9CBFAD2A18C859E7E6484643225EB8BF5ADC39FBCF98BEB27BEFFABCF7F26DAD561F6037F6836781ED4630C9D66EA81F15D539FFDE9B84B4BCF59D5D5F74E515CAD5275F5A62BD36589F56389015D6A8707DC76BADD8E0CC60083F5B0515DBDECB22636D883467C54E2FFC66970E4E1E881A3C805E0EC8363048E41A866EB10974E17877031E1C7DC484F0F9D26CF4A2B2B57AFB213EAA106BE74D6A6D73FDB4BBD0E74F05C65FCC8F8B9F1E5714FFBF8E1843B30094B0CBC6D2586A1AB09629474B5488C315D1D2606D6553F31DB75552526D8648198655D1D222654C14CA7D5E1A1A1D6563F2E160A89842ACAED06760D74C14029C3312AC611E39CB16C084615A7DCB834BE657C699C4F8DA3F131CB284F96B69470E9F086AD7F8ADA9F92AEED04811A94E6763281AA0D52E782DFC6B9214F6BB344AF83666C34F3A07A6A0462F1F02090EAA5592DDFC3AB71A63969AA456E7016FE3BFF30F6E382A88ADC08FC3062D94A1F7C28ADC82D91614C022A7D5885C8C7EB52F3157414EF0141B21D07AF679E048A94779CDACF9DFB33B1DA7E76ABB7F67253ABE00E1E030C206DBF89BEFA5843A1146974FBCD43B7E40A3D5FDF769B783D7EDB63C0BD7E8EC37B817B3A778969D70EC2520B61E4236EAE1C235BC3DB07445DC5C488EAAA4C8C98AE2262FA74354C4C390CF623466398F22E26529EC53CF4D598E1AB880BE2B2C8AF8AC81127C52D22BF595C12CF89BCE8A18F898CC36275F5FD93F45D58D45D8D05A6ADA90A5920CB8477C824D942F825728E600AEB26C092990F88CFDCCEA60331DB69E048CFEAF1485862AE11C63AE6198278CD35284ED6C7D47E0D2DBCF743E585C2B3B25B1F107FBABEF94DB6A65971F5AF7C18AA6822995571DD988C36B76F8EE0ED4A45F95AE0476D4B568B1C458EE55A382E368AA9B13276461352670C23EC44DC089E8CA04895F79F8AE5823E2D515DFD2FAB0D2CAE9DA435A30B97D0BA250C9FCF115DF180F85DF1C762CB2BE20571152A8B9BA5FC871B61A5EC64358E5B1760825B4E5B55DC7B822C7F2F6ADB9B2ECEB0A037330789BE59C7959599B991C13073ECB53C2FC5557F201EF81FE1651F1BC57186F199D9FBDAB3776F6F7DBE9D656DDFDE19DF87BF76399F0D6B9F730BC6144A1048108821D70A85B63498802DBE511A5A5451489B446A1B4A5B59286DA350A1160C760D6DC17F2491A2A42AAD5A4A5AA41229A408EA1621024DB0CF7D670EF3D17F6A7B7766F64EA7F33BCF3CEFEFA9E9C21541A342EF42D0C1F3DC93078B40F1C6E948044B80E93746FA25D18311AF3726F7EBAD72C3AD7A5861F0D604B7D619639DD985F779A169CF4FB77DA15F8F379B6D296DB661F1BA7B53BCD8535F3D72EE3BC5FC1C3DD6B8AE63C12A6168A6F60970CF4B50FB1EF22F5EFBB1BDF23999406A78116D277BE51DF6EEF63D1DE78367A5C06684554F6F2BC8B9833C45BE44F69183EEABE4887B4A3A2D9F6D3BDBF367E92F5949ADC0824C7CC49B7D091DC80EA1E3F8A8FC876CA002D22E22DECA98582735A2066C890571B9F86DF44EEE03742B17122BF40A1BB793367781BBA2F70DFC13F23377948C064F2CF81DBA8C2EE03F918BC20D7403DFC49F046F56DE9268B42D9ACB65EDDC2A7C047D4F7A2DFBFD9C78D2C77CDF8D5BA1445DBEAE776135AAB6896C232145A3BA417D3490491AA9AE14616636F536BF8555CD19E01B664DE50DD7F1497EC3C7C8219EB062463A9EC8CFEF36F25E8FC7F0863849C462462A5EDF95EB34BA304209598A40909C8FD01879D75D65E722B69D4358CACDF7F6DA687ECED3296152591114FD7E79AB7C5E2672D2EFF1FBA351FD38CD7775A5D3A9EECE4E00B8E329AA693E9F3745BC81FC773DB26D5B9E7D5EBCD58BBD63649E5BE94A2B24B24FC22724603CF2A9DB6C8538738678670AF17E154A00C030D5DE0718EE0F75430B7B7F83F300D8EBB18E98701F814FC0CEC1295030F3740528A4A094A75379E5E16F7901152A5A502BF8C3701D905B9B5E50DE8281B2912205B8651CA8830D8F2DFAA03D40544503833D4FBBA2D5D632DF5AD0D2E329F6159B7A9E79DA0D76D0A854089A11273B367D6554715C45763074DE61D941F06498AFC68715B61A3F0943B9E900E1F401D700051BAE6CCB48A0295FA0B1B35B482519D404FB619BABFD1E81EAFC4451EE605AB6D0C66E6CD359C7C1D1A8066E06A728974CE1FFDB5BC21D73D15CF65EFE4C267E9F9F6CC0B7F7FE62CDD4DECEB6AAF652333F6FAD53E71EF1BF05AD56738C46B6E3CC13466336866F352FDEF8647484DC2C85F6F641984A519ACCE1DF97963EC6C7095A6E31EE86D2FAAA7EACAC4BD769F5C0DBD1C2A2C81938B319C84BBF82336BA20FF9993514A4601399D84DAC215F213BC921F38879CC3C6356E2C4187ED96D9337743C459EA923D0758478223AD708772782314389D79B3113D9C84502FA474D582135F54408C0A1ED2763E42DD78A6A5C671A9794C675A6817506B9D082FC69900B2D38145F5F7C186ECA9E78FB368BBC1370D23E2A324B64AC3008AC608C0082AB4A88475785083563FEDBC3FD8287EF9530135DB365FBC39AF0C88EF0DA27EFC3010FAD1D9ED7E2DBEE7DDCB6BAA1BA26B5AC8D7CB97F8DA95466F73FFBE3AF6DC43BFDA5571BE699DB844D2F9ACD50C04677F7E4F195B1EA48EB7654F63EDF2DA8A34D3CBC8ED74214CB28A0C9BA940E65428D1EDBAF76E36EAB8F6EC11BE9666B373D8C7F68BD47FF4AAFE11B5492280E6A3E7B912D74D00EFB735488DA299AB4051FF5DA9A2634A10CACBA50A7E6D076BDDD2E64976737A23D6807DDAD6FB30FA183F49BF61174D83E86DEB08F664F64DFD7DEA5E3D9CBDA07F4427642BB4EAFEB57B277D067DA5DBB61315EA22DB2D6E23E6DB5F59CB64B7F87BE6D5FA417EDABF4AA2D833389F184193366C513ADDCB5801203F17A85A7DD38772C1675108E20AA23AC53CAECEA09DB8AD854B32D6A610BBEBB364BD73522060208D9762A1DB0D701DBE8566BC234E347E327E28C23AEC47DF121378BB398B08F909490190A83F70CCDE18001BBCFB8719952BC536413E88656092470DF4CCAF8081612760E045A9BBCCC420260216C421FD224D0C9003846911984612991CA022EDF1487D2B04315D54101EA6863D317463447B3230ED841132A5F7D181AABE156626A09A21E2528A0D794738E4E7C0D19410CC2EA547F5A6432E376D054987184383781C72D80251B8C1F619C475EC6C2A2A9DB46C30ABB94B657CF8E46E4A52BF13EFC4FFC11DE67AD991DAD6958614D8DDB6BEAA3539F78B64FEE7821D6D8D0903307851D6BD3B5A9867B7FF3F0E5E4A1072F1CBAF712C2D357A7AF7B7F0EAA4CE1BB5C954B0FA9587D0563E22E6F7F8560B596E01469A99A57B5ABEA07E4EF649AF8AB12095561C9201167C920213045D4479822EA55358C21EFA98988AA26C00D5E7743A9E338288A9818B302AA28F09DAC545786C3A6622BAE222860BCA7C3B0ADCA0C38B1C9283BE6CA50067CD8550AED6E069B197C34732543325511F611D5F1B89DC0E30948513C35293C358D4DDF74833C4FE9E9F5AFCF3844999B985006069B3848C1FC6396330A65954C4C1C280B04A90E76B838FC4A7E8E8D8AAC91A445555733B8801C7539FABCFA45B456DD829E53F7A83FC2C7F0AFF188FA1EFE0CABFF2698E5DE3E34D08407404C6710997EF3549D5A20F03F9C82BEA342A3190539BA350E9B0EDF1F0C3E8CEA0E90399B5E7243AAA346558728D570E90E44C64BC3150E7CCC85F2F09F918843DCF04C7762ED89FF303D723556114344441553956280D3B548A851C3FDCE7840D7599E899810055062EE31B8AEFF5F6926597332F056A19BC90C5F62029C3DF90D23B91CD4C8D4D7D5DD55DBE57D72D22FC833FABA77D0B370F2B70FD4F6CBDEE62AF85668F1F487DE5DDE4DA81219E88F5C71730EAB6FFA8F058F299E9D78B7FF00FE96DFD31390D248A84EFB449A8F09964090A008A6600BAEE01596D43255CC2AB49BB56E2DA90DE715D11449488C89445C52B3E159BEEDC5C16513CB9481A63B6C02B85C98E0889CC546A8A122392B5995942BC32DC8C0B40547FC308B7A61A604A516AC13B8A981EA16A479E0F668899BBE0E86619C4412A4CCD3FD48F055034D0FF7FB4456D542136B20260A2B28CEEE733B34A8665861ECAC869554924CE000DE5FDA53BA51BA56DA7FF9FCDDD1E70FBEBCF9D4F94F0F3EEFDD54DAF25FBECB3EB689F38EE3CFEFB19D3BE397BBF82DB6CFBEB37D67E7ECB3CF76FC02318198550532CAA0554421C203C49A51912E2104366008E828018A3AA0A550FE286CA52C134C4078338C0DBA16366DABCA5EA4D2F2079DB469A2AB0BD5F86752317BEE4998B4174DB19EE7EEE444B9E779BE9FEFF7DBFC63F337CD55F00A74C113BF3DD3333AD6BCD23C777627A460162C3DB193AC1D7110B346FB5F1ACE186B7709E964295EAD94B2FA3AFF883012FAAE3AA41F08311BFC1795CBEA6DE176E863A525D0CEEB6AA233DED93E5DCDE97DEDCFB70FE95B75DB0D04C15032342FF461E0B6601953E1D7CA476D1F2B1FB5DF52FFA6B484AA7258659D06DA6320094C5426E0F74665148EA45361B55B5E20635966BC29D5E7F36296615D28C80773C16A70286809F6E8C616CDEC2E211DAAFA691D1FD1AFE93775939E066AF140CD1CA8C5438C73520D3BE943277578E79B19BD0EDF3E1B5DB1F23F72E4A48A6BF39F206F9F3065EE0A746A2CA1DD926F90439DAD35C8C9764D6400B2E92125D916F2C7D544B22D5100254486F640AA0071412EA0C9CDDDB60DF5F4920A2512A8C9D3CD3131329DECA284C0301122AD6D34000EC3B021724D381396EBCCA7E707C261D69BAA335F8C0F783D546A5E12AAAD546AEC63A919CD8A1E8DFFE6BDA1B90EB2801EA36AB5FB68C92A12178831702C94985F7CF85392343C02491AF0C585DFEFBDFDABFCF0ACD233E15507E76EEF2D2CC49B9AEBB64A24694C93464C03C6D5BCF18DC76F3AE74C99F283AD8B0FCE7323436FCD41CB06A2372F4A409AEA2DF9242C660E80A9C509CF92AB7E580F3B602F7A9DFD25F71764357355F415302D624D07CD757CB39A657D2A6F42E2499635B2DB10DA8ACCE819967598B45897E4CEBA3172F3EE883BE7AEBA2DEE1EF5B136D5AA8AD56017EF883830E7901CD8D1D3FEBFB4F967B23A8D1A51685777837F30A1D2AA351189871236FB143B6EF1C715392EE316C91BCB40D81A24B2E4C8906825B7518F98216F26D8C96465034E5F06641719481FE8323E746B53E46772FF8876CDA63AF3E1B9013342629DF9E4FC008BACBCAFCEDC1B1F206F69A8B8A0F1EF19DB6531027B42518C608E4B139AF620F897A4A796A9639B76F4370EEE6EDE68FEB57F6FEFC651D80D24A7C14B44E31B2F0CEE79E55BE7AFAC1DFD6AE7CFB8D3C7ED11CB73679FABCC5A01C23B9083FDCD179AEFFFA3B9D3FCE98B6F354F372F8EEFDAF543E8FAFBF1AD1BC8AEC908595611A5ABA888E7D35D1B57FC863CE25424A33170BD94B82E5FCF987A941F65B05F6AD3FB159315ACF1447C0E5A0C837850D9049BF05A696D647DEC3BF1DD301A3994390127E2171357328F146F4B643BEC51B6B71F56DE8663F8B8722A7335732B772FF328E370211F04B14B256ACE57F44AAE5F793E3B25C5E25008BC92C0456328AE0A8895046754F64942282A57713AAE28310C1E8C41398923984925DF668CA3D066FCBB0CCF2C649633A6BDCC5106334838192AD6615F95EB50C3E110E69C4E00C4BAA246F75A5C32A6EA930B4A287A2A8A17903088A3E7F93254CB43E59B6553B9C85282B0741D584A1036E6F3528278E9432F2588F7CDD28A4BB4863EAE09141F7C6DF8416D8DA619F4C84ED0233B498FC9D0D868F0041FB5E1AC66B4D140906F3C6E9EE0EA0CFA27ABA636CA5B36BF97CFF90DBE64F2A22CC53372B60079910C7A2C5D40B2928B7414003D3E87DBB6C1303983C3346A5E4271D237ED46DFBC3FEEE9548D3EEAA1764F2EEF9FE73B733C470C1E267C9DC44C4D13C695B4B9CEDC25E1329DE642C0924564BE1C1F0875D499CFC92410EA5C180861D5CEF11440DC04800C026986D767B50E23764681C2E6FFC18831CA2A744CE288D0C8B2AAF97AB3548838443E9478AA44B1440B107C7EEBFDEFBF7502FCCB770F7E39C31DB2FEE2FA91EF5556E28D18A0B9FEDFE1D4FDE3759BEB89E6A61D8BEDF835187B71CB113769455B1FFDC96C217C9A86DFA0E73CE03A90060E386C3321CEACA2A4455B000BB0B5B55287D9D59BE569E5A049302FF32F0B2C0B2E135A2C0E8B13A5AE55CC23B611C788733D37240E4943D9A1DC2E76876DD431EADCCE8D6A63E6B102EF72141C4547295C0817C325524270C61C112352329929CC8499B8DB9C0BE4C49C948BCE28CE28CD75CC4DF5DA16399EE5172517696109242C14A49250EEF5F7067A834B3A9616961697969696FBA63A4D365BD26D1392B22D52999ECC55865DC3EE5DCA21E650F68DDC58F69AFA4EEA8676AD72BFE2F91A3B4D40835838051F00862D007019D54DF3AA8ED2E17C48080F4A82285E0E1B4F8A81C31E02AD2EBBD363B73B357BCA694E58E9D422C343D223D5BC49563D567C12AA62AC082025205107B9CA675BAFB6E23BAD10693DD57AA7D5D45AC7A317A593A2C61336185F908EE87055BFA73F22665C9D53AAEA1F901B13D2237A8E58B459BF02B35127CC06FF84706A356D0D81F4F083C64362BB0F87C9219A4853D4698D724506A20FCD69B42AC47FF68088A3F1A041AF6AC0AF694C22BDACE418B79AB0A5AD0594E40C1B769381C991DB29197B01D9EC69AD9D27A6CC3993A9B88B18339B6D31D4A35103A6C3440A233A222AAA91606E5D69EB777C935FA9996B4B6A4052015A83688FB3DBFC5CA739C77516721C8DC94B84AABB54CA0702798CF3A2E8C94F3385ADF91662DCE706F2268F4CF9EF994C713418675B1F7FA873CB3A96632D5EA299361153BB36CA5AAC85915B0B229ED0497B4249244AC572A1C3B087F254D389B8AB7672E9AA9DDACCBB3F7F79DEBD2BD38BD2BBC1409889C7838BCF0F6CDE37B5D2DE3CF6EA539FFC6460C3B4B660740A0981DAE8D1AF6F797A6661DEE6FE175E7BFAF01DABA55BCCC2EFF6EF5BBEBDAFA33F2DBE3BB2A777FF1F4A01298B887666923C78DAC8832052ED54FAA00FF785FBC4D5B01AAF0EAF16D96CB43BBA207AC8725018B31C17180C6191209B8FC6AC06C965C62F2309F31C1BADE36B55B71534546D7376BB38123117A253C4E9EB58AD06592B65AE95E2D54A996B8DB5F9244D3458ED347E0389BCB84C3C2A9AC5CB5845BE479F556D06917D94C53EF2D7CF46BE51FB27DDD51FDBC47986EFCEF7FBCEF6E56C9F2F3EC7778E73F6394E7277D8865C08F569341B2169096D2A12980BD2CA1ADA6CFC10432016402B2D052A8DADDDB4326D5A3755405475844070CBA4B269684203894DFC8126AD028951A40A8969191A83C4FBBECFE18736CD96EFFDBEEFBE3BF9EE7D9EF7795EB52081F66BB60A8B6F0A147BA10C6F704A0897408A0A37A55E54A267516E315F2883DFA353B790BD9BEB05764DBA285D741D0082AA3643A45212ABAB3566F6CCB84E04A52654F9A4C75D0E486411263092C9C28465FEABEAC17607E430427E10CE0A11FDD5E1CF404B63CFFD16F637BF5A6F95563259891A9CFFDD705BCF9207B38F7A19520C45C6BF8E3F033220D4AF535320035DF8470D3FEE8066AFDD2E39B0E933DA50F4879564C9A27BE8417A57983433666E516651AE2FD397FB30C7E4735E8E1872B60BBBC347739FE5FE95A57B43405A8974ABAE6BCDE9D6765DC3D39988AEA9E94CB3AA027D254C2BC8B5E76BF5BF9F866F180C6E9D86794103F8B6F335E2A22F711CEB8B1EEB032BC63A2CC1D6EAB37E53340A3513E9270D2F86AB33484813E89F3E5B294B0EBEC5F98573D2B9EE908E6EA0C41B28F1064ABCD12ACB7B23F8E6081E419A1B09C17391143C1769B6677FF94870AB55E0D551429F0796BD700F72B78A0C3C5A845504496E01D9F481D5BBA696B0A05064D316DFD49ACEA4093A6CE6CCB690D189494D5931DF890B7C5A323B314B30615786372C1EF077B02200E6635B6181D0CE901A1650E3C88DE32A113401384E8D07B90573178722883F6E75A33452C32C504024808F9611A9177431F027FC7A71A8105B7DE7D2E7B71CA3EFB922B1B234DCD6DC32F8FDB137FFFC1CD0412A679ACBF5AD737FB974E383A3DF1BFD27214F3C6F9AE5B66D7353AB2E6D5BB9FDCC35C2DC6B7440CECAF51BD4C790B3C4128898D37C98D6892982583E3C725AC1535210FCF1B3219D509810B04A7645F62AD2DC952BE771DB75345F94A534AEB0827742C1915F52915F9A2E964B2876D828FA6F1899D23FE407FADD74E0D3F827EAB9C4C9F47D863AD1FC51E237D40CFD09434D52C7E813CC64EC9842FD9439123E221F558EA4A94DB157E2DBC95DFCBE34B55659131F4A6FA43731D43A66945DC7BF1C1A8D517E7A081B0EACA15EA429235D22BB635FC5FA439449E7198BB5629642019B9C76D21BD257D2D4140D1FCA4F62A1B4C12B09A55D09284C103EA21602168361F51001595D95E62E5CB8006A6E15A889E7697E14A3700D0BC7242D1C62C1663D9ED2F45AFD80DFA430B4C1320CF07B516054289A86502F2B71308BEB6160243182A1B907713CFE85A3F8CA11E5AE422AB79D981F1B8A9D8CDD8D51466C436C4B6C5F8C8CD5882F678CF48FD3AF1F526149AA36CF566F5601467A1BA83C4035340D44150D0A40DDA0F1FBDFE32828465BAB4F3EC8B3550BF83628451CAFCA5ED8973DB256BF3D23792C1BF18031BE3613F1782B0257AF4D85BD86B2814A079C9E765661A4141E08051184C14B6220761174650F82374633E0FD657058D27200B23414261C6F6850AE4C7DBCC22CE7E773E63C99939AFB9F21DA5FEEEEC24771DFEEE9A3446AD00CA6DD8D0FF6903F581BD5339469725D6D8B5E7BF8B740D3F6CE96B200EA0B40A956BFC14C00947A8117204A1BF89CE1F0EE7C360A6BAC1F963D22472439472305991058CC06508D7B1504D6C7706DE6E82023B23CC7F0BC437B8C1C52239E087E1A042BCB9540DC07631244FF36182CE6CAF64A6E941CE18E7174962EB01D82255A112B91D7DAAD9CBB98F61225E76BF4B3CC80B0421BA647981176941F11471223CEB0BB897E851917C61263DAEBC51DE40E7A07B383DF29EC167727766A13C99DC677EC37C977D843C9B7EDB79D83EE0F99F7857723EFAAEF277EA2BD67FDC87ECF39CE4E7293C264E2B8762239D972CC9E66A6D9B37C2D71DAF983739FBD2F3C6CB96FAC1CB3373A63EE418EECD6C6539BF56F77921B998DEC181718E006F515D6804D8E6A6BECD54E60881962D70A0192C1786011938ADD9ECCEB2EE309DC02335A3079698FE6704952686ABC594D66190117582F27436A006EF42272407A20B305E9D1C125932CC7F149E019532916A301592289A816B1ECBC66C922B84B2E95D5729EDBAD79B5FA96694DE08D5A7DB31F7558C61005A15503BBB5443299E2781E3228A625C142D26E61D956C78E3A8EEDD20C03CF241D174CDD889CB32CCF933142E0799665B8A53FA73F7441CE4EF9651796A11E14FC6CA75372DC7DEE1137B0CA5DEF6E70B7A0C975F7AECBBAB7D92FB81704ED4C42F89430B004FE6F5FF0C521F18A18108FF52CAD11AF4D37C878AF7AE766B3745395E66651AB5698BBF5B83B43A1C1CE03A189063B9F0CD889A7F8FAFF09FBF4919142BD2CF832522FE4F1230E0335010A031D272471D4B2946025050F86030EBA2A0B15B401B666C05C46B924228247B002D7DE94EDC629390668323D0E13A922EE568A15984444E0D605062F90B8A13B88C5915C2EDBF83EB5B840ED4C9999287F25152DCCBF65CDFF71FE72DBFCB73AC568DF52FC9E5AEEEEC0851B96114B0423CDCD913C21B575973A7112273A5A94EC3240FA6C29B3FFC1B9C0371EFE8CFCE69E78D6344DA735B3678E210E6C5BB7281B09CA2C0D96F2C5BD733AF1E5779DB8C5864C0C23B0FEFA9DC0C1C0AFB145D8B2C0E127B5C0372A3E94FC8A0F0D424C63BA4C561088974C64124C4C2C823ED617649978A9A8C02D60FE39322C45E84462D03414D1DEA2C7A0C874764108191CB8A4AB88A5C87C8753127D0EDC54F45B5AE0B1099C126BF5AB7E0A6E124572AF8AAB6855453B54C94C31BD1D2466830E0520A00A9B5EF0B96CCF41EA5C2D5CC66D3041A5F6FCF9BF160ABF97AE5E761D601AFCCD42F25091905F5C8CCB86EEEDAB1CE766F8805C9027B089E25BD861E170996E91951EA9B2AF4272C9416A90EE33FA5A077BFCCAC116960F3106D6DA8F0FF0FD427F7960C9F29EFE656B84578537B9FDFC7E213CACBCA1107A657D85D8C016B1526F57BEB3740E9056C4C4FAF919CE132DC113E1B3277ACA1260040169B1410C1828EC1049B157053AE1E7056F95BA5EDDAC06ECFF505DADB1515C57F8DE79EE7A6677679F1E7B5F335ECF627B6CAF0DEBC71A132FB183110DC12DD860D12DAB2482AA722AEFA2A29246CD529A46094AE2342AADAA2020A2A99A48C5716C589256751B0809AD25DA1F293F6AC5AA5C2921DDCA950842220E3DE7AE0974A5997BE7EEBD33F7CC7CE73BDFA73FAD73FA0FE31AC5883BFAB3FD1C843DD9566AE3DABAE0BD95F92D59AFA0B4CFB7D1B6BC4536B854359D8617FF057C016974C3BBF4006924163ED19D2156DC2A59539690B5562CAE64514BC349D6BBDC20914910923B9E0996E9816C2C9CCA74CA5977C69047E492CC6B325D91E90854F2C10706BF5BB58B8562D1DE0E9ECF069107173628FAB594D56EE640F3DD585DCE6995C240A5087ED2F666708E6DA7AA8438C3AB94E4C6C15866F07331DF38DCB5319210FD3DBDDDBD9CE474D43838C96C301A38A94BC918C41BF54788CFEF89BB22B421B151CC4448AF236DD0AEB4E28B6811EA6E80539FD41F411358B5906BAAB1A50564E3115AA405908D8522F8C63D33033E48669AB30956EED94E881410B934A3B1E69C3BD36340EC50C667546C96B28A92D10D25530B4704D15EAF404D57323D4DD8D6405B03AD135A2733A1F7FFC621CEF09CAE0B31355D963F999B50D58E9806BDD909F89ECD65876B76A24311FACB0EE7CC84A0AC19531BF18CE61448C4020A01E5DA95EEE9EEEE017A60BAB536501D631EB436043A361442A7D313642AD70B6B805060881B7EA1B17BD3BE1FC49AFFFCEFDD3B07AC24974A5AA9E9934F3EB231E2ABA9F5686AB07F727F671FFD79EB8EA1B1DE877FFC84B7EE47DF19EC1CFAFE58E373FB1B1A5AFBDAD7A7DBC6A69AE30FDACF7CF9E1D18D01D9D5DF7B7CE8159AEBAF6BCD67B6EE03EEB873FBCE327F417C91844823BD798F3BDE8A89C8011AB2811850895E83F9AFAB6860902A54042A0EB10E32858AF35D385F55F55A22704E3F8A106F20EB84698120095B4EC51C07C10CAEA232B0685758BD6499BE68CF6BEF43DA831E59ABBB49B8050FB78075B806D7C6443169111D88481AD539C43F6EE7D62C5E43E73FE7714855939697510A50C73CF616D69EB7808F43C173584BD233D239694EBE1E17C4E4A02BD76D24BFC71F127EC23F2BBCCEBFE9908765DAE708AC736DF6C702437AAD4A84708880A8FF6A279D71714AE4F262493C2BF2E2676A8810BD515535D7886BD235E5124A709A76F1C4A5B90C570774E75D575DB20BF8E37C7F972B6FFDE96B550706A9855E0B536F3557ACB09D1607BCB599CF2B5FD0CF597235D519BC22270D3E66D0FA1A3D42EA74458D38E02A2E9806AD53C2111295C20641F862EA30BF75E408A40C64498E16C7C7C36F070967961DB5B313A053C365B93233E1BC8B57AC75004550EB72157FE924E2749DB5C1EB456076AFE1966E7CE6972FFCEDB5636F8EFC6ACC63E8911637F5B76D7822B3F7C489C7BBBA9AB89B17FEFBD71B3F2BF5F5F173AF6EADD71293AB4DABFF58BFE1833F4CFF3E1C00B5BA0570B60D6A94C93520CA661C02BD5BA5B87A89D95489595889551A2964799C72DE9C3439344B738839330A7565D61FE046A173E51CD6AD68270F85048A849D1BB85861605AB88828F22590AC0FB6B4A54902BF70AD6BB7C845FCBB849DE08B76C97BC27B22F201F19058222573367CC9B86A2C917F89CE1E3A4CC7F4D1C8BE445ECF470EE9C5C8F3BE17FD53DE29FD757A863B9B789BFE915E962FD77DEA588E5C376E505DE2B6F976FB8EC58F19A5C44A42F61AF47777968801471C68894409D27C0760276F964C8E981A78AF1113E39A324F99D3E63CB8B02573C57499FBA31F7BA8E772C872CA10DEB59940069B6CAF2F03412AE65FE22ADDA1BEA4726A4A231D244BF264924C9169324F9688130738F2C6C1FAA3F5DC483D3D594FEBCB54CDFA56244A244D32A40E292B89D260C3E005EE65C2C0572C6CAFE48A85D5426EB9C0A067DB03954A81158865DF5A1AD6EC8C3E163D18E55F8902EB17C6217F7A7B7B692F782984162932ED354B341DDDC20A182651D33214D5A686FC3BFF9656A5558AB6A9103EEFF1384D937786CAF2F5D909A7CC47CB8EC8CC04BF0646C8562F234E2A0116B9AE3461C0843EBAA82A7D06AA64C96FB3AE1D7DF5134A679FFD6D67EBC6985749241E787CD3D74F3FF7E8233D69FACDB9F7A8F4F135EA7E697B32950C1E8AC7B63D7AFACCEDC1F6C3A89786EE2C0B22705E9CB471C5FBF4523295451C364B3A83A0A30A47064D6244438C02438A8144E745F4192AC2D260B361F4569601D8D07185117987FF2789A27880AB68DC8764A8F9B34E3737EA0F10B074726B2BCF541072610A0EBAA67A1641F3CC332883EEB94B88DFF0C12A62283C8F4B2393519A8DE6A35C34AEC06D941063C5908014083B0C606B081E0F9C39FCC73052EDCD6C0E0B4E1A95A4543BE3C905BB4A97F6FC826D23012DE6720B0315F4898BC8C717480A6CE0F0703A8509F5A0DD9ECEA79E129E129F174AA9B3A9F9949C4D95521C49855A82F6A838EAD8651F97E5AD3235523D35C3356335BF107EDD722A25CFA7566CCE308861BE03B9A140657EA8DFD8617CCBD85F33613C699C24278D37E40BF2FB2D4AD2E15FA76EF6C5FC43C1E8BAD0E6482C3A1487658AD01A646F2DDE4A5B5BE3BC12278AA91A287A7CC17CA8143A1BE2E3A1A91017FAAC794442EBDAD49EC6F6FC709734D83EF874957141F9AC1673FDABFDF803CB02745B41C2D518E312ED1EF1D6276DC1B1CE4A3A9A0D620B706A922D83B688AD8C6A69956473BD980F900D055A2CE4403380620890B8E2B4CAF2A70CE1AD6587780FE103034C165415800F1440D73DBAADEA805A31D1E56DE7BEC23A7779B0B4EDF8D2ADF70EEF00DAADB75DD4DBE63143E136E5CB9576A9FFB1D49E87F64E4FEC3DB065D3ED4B97E8F0F6DF9C60EC7B7BF1F470C49B287C48AF0D4D66767CFB832B7F47E43F0C2CBC939F260112E567EF437E9323049556F50054899B356E46C3EE6047965003088723448313BC50C6C0D8C97ABD5EE811256C7965226B3227E3DFB85A669C0DF364A17CE723B6023A57CE63D6089D8AC2E806D53F200DD107768EC11F84406A61FE9E0C88064BE414901C6F30CEE3AB9BA83ED1810FC93622D435D990A7659EC87910BDA76441FEA9F09A3023F0F8281942C38C4D22EC0381780CE2C42E440BE981D142E30EE190DB1D8FFDBF78B017AEA27EC85DCCE5ECF56CAFB0534C8B6C9D6F9F9EABCB937CE0235EAC33FE4775F5C73671DDF17B77F6DD3BC739DF9D7FC78E7F80CF717271E2E01FC421C54712D2D4214D80B48478210890A64E99925885554813E92863692B2562EAB6A089545ADB4DED1F85D461A612259D22B676CD1A6DD3D43251A8C43A699089AD084DDB52F67DCF616367BFFBBEF7EEBD7777EFBEDFEFE7F3F103C5F4675D863F1B244F65E9CCA77090004F90BA622C45BBF73634A57CBC571CB21F708DBA873D851A0171222F88D86A763EC14FB32FF3A7AD2FCAA76A7FC2BEE559B4FF9EFDD4764DBEC77EC9D9D583C2413C016F372DBE2FFCCA765700FC14AA5F603991C4130FF194CF88DDECE3627F70901D140FB14576DA3EED9DB3BF26BE6629E345F1BCE597EC9FD99BD67B16075E151023AC0AEC24B164EF6661D3CE83DCFDB6C9C1245C4EF2A87635AB8E3A4F38E79D379C26A7D3F73B13822FB80AB06422F4DA4ECC27468F9A257BFC351F225F44F808BB62BEACCD85C65D275C332ECE75CFE198C2288167319BC033F806E6646C6078137C1EDFC43C7E53729A9869E2575CA3A12624431A90384692A590C4DD9590449E4484BD943A039D1B9C09E44BDFFA24214C932360D640A3C804BE8AC4A5F4220008D109E34ED009206DDA01CF00D000B8D008D3DACA4C8EA0CEA112CF20969DDC4F850D39A89AB8C40870B7AACD59AB11CF5643C104C76259A162482E59F0555ABECAB58D96A5D2B2545A226D19929875CADEAC37A464ABA1D094F17F0A633FC9158250A53899B2F0A7C531A7B34A016E76BB34A60855A6320E2E8C55FD0F171F4A0A3BEF26A46CAB7B03215582905A18D011D2087F0D1D39727AF8543CE8FCF047AFDFFEDBC5B357D74FA39F9965EFE1CCDE93ECB68F9E7DF6F0738EE9CF11FAF436127EFD66DB50A4D5789E0176D6CF30DC71F3CB8CCEEE78242B68718A877183C05ADC2009C1A72359E21196EA11266DA4C237FA8BA192C096549A3228084A3C813F1130CF82235AC0CD30B67A5B19F916541E33CDB9B5257929B7B226AF55406F89088065F92AF9017D839E8D047089B1D1390C4C356AEBF908AC84EB110D60C493C8455409D0C7F8C4A8A2514CFBA17D8D2A02498A373E84B8EBE404B75F59214C9B84F1F6974273CEB928D7C575597BBCA7B85356F359136A8E9F08CFF2B3C23C9E17CFC9E794F37151E621BF8D368CEAAC1F4BA5003EB30995024299C3467073603E7025C0069488E646FA808CE44443BDAAF058B0C8101865B4E79D99388A97D9FB0BA8412F23D9A88ED523D5A6C8676C3614214EFECEC183296ADBDA2A3697ABD8480BB586CB1F4ECD4A8884C6A834212D49AB122F791BDFE5784EA8F0B9918A33F7AD81CB1341D1DE0EE68B915B4540B91C80DD7AB13DB7AE6447602328BEA95A9DC315D59C51CD15F333758E881F6DA01A8132060A38E80577A42CDC31C4B14040816FA8C8E0A017C7EA15DE8C4520340B63D8425C744B2E09856C32216F8A037C34E90CA7C151336965733A096A97CA5DCAE52A540E44AE33E9446FF8B5ED7BD7AFD7C73ABC0B0B438B93CF0CB5A502EE643E188C3619FE3BDCAEF537A636354622B1AE43EC704FFBF47B47BBE2AD8174F89B767BCBD7FFD0D10390F0D857DDDC1F41596C639E60F673EBC47F8DEFA8AE811F46E7321C13970BECB186637B59A6816FE2F7BC1432E5B6F617C6B71E8D4E14664C33E693EE173C33E917B79FDC39D3FBDDFE57DCAF78E6FACBA64BE692BBE4F920F541EF5261B570B370B7E0AB09399372DA910916CC3FC5F94CCEC7B8B84C38EF63BC9DAA22DBA46A6B954514ED768788A734A46AE5079F9554C03D8D7C46873547AC51A556E5E6B5B7B52B1AA795D1B9C5217D0A64250C35AAC958753EFC76F84A980B6FCCA116A68461ACE199CDA3BC01BD7903BAF28D24E4F2030EE428236CD8C7313A81A1A2C03238CDCF75A2CE32D76258BD794BB3170D78A7BCACF732FB5B8687A0EC63DAE1928517BCBBD1EEC6465BDF7B5C02F03500E72CD3C7258CA09C40E38999C47C824B78089E27AC249412E96C1337358806C9BB55439443E5C392ECA095CF4A640854EE1A966A08C0412D184331EABBEE9AD44C0CF5C726624BB1D5982926919170E95E89A40AA8FCD55049A2891D0D151205A3F02AECB9B940A6FAABACA98234F3836ED42D9349DD2D2117B2B9265C1F03B8941FFCDD50C83C97951011177D465799BD6CD8E77228D792E006387680430C27732C47B6D25B9BA21656E5C8ED097D27959F9377E49E192EBC8B9E63C2C87261DAA3EBF749380176AC15D769654D2FDE92F5C9FBB4A11709DAE893F22DE0942345C8641B20B4FE0581A49CBC5604AC02565394C978180CA854FA387C23CC022E15EFAD0159D4498F7643839E220958054837642A5210B1F0A7917ABC775FDBCE48DA5FEBF6207354DBD2926C49B570FC8E687FB4496B883EAD0DFA917F5BC0CFF4A6FB424C07CA8598C7CC393F3310EFF3337BF4C110EAF274FBD15375FBFCE8E97DB56D3E18EEDBC6EC6AC987506F3E9D31D8CE1060C07653BB1F3DD9BCDBCFECADDF1D6276BA3BFD0C452CB95D278FF7F044B3C47F8F064818E440C51102AE93144A0D4B930C3E9A96D56C1338C40595AA404827D85116EE1BDA18B82A38EC2846FD18E5306AC62888910D2395C3BC91C937323E6F6759B8B138E6F5E6F9765AE385DD8D364836C90A1ED2EDA99157460F8C6C1C34F7448157935403B048D28D0030B9794342F28450BBE98F5E21AA329D022D99D94A67A14D3080A26B3A551745FCA32D68A70787575E3D79F017BAC4F166CEA67FAB75F9F5AEC71B83E1847FE2378F8D8C7FE3C7FF7AFF546F95921646537A1639F347BA5203BB0EED4C7EF58FE644DB91CBA5B792A9B39FA327EBBFBFFF7BCB869917DD351633DF333175D111CD3A949060E2CC62F5C49EC9C367F66DC9783C5A877838D812DC7C803D7DECF8B97D1DC5E3F3C31DFF7E3E39A42522DB4FF4A45C2E13D018A61A90FB4BD0B219F6F223C85DDB6A90F4205B140B85698B2742DA9E1AD2F08054A59107959B06D5B71E898482274AB03C483AA2E154BA2E8EC226AB957D2A4CD708C73D648D78F9C13F4BA4172AF74BE442FC612443E58E61A39481AE1747A04177588008A85034283128754C0A68812D6D8830379D61EA94DA469300C1D3DC9CBBAE134E70E70E7C5B4A0CF40A159797AF6E9197F54ACF8ABE2C1392F050230CA55412F8697A863BD6A56051B2A45267F90FD9E51FDB365AC671BF4EE2248E13DB759AD8491BDB8DDBFC3071BAAE699235BBB85BB73B766C2BB7756382DE7AC78F9360D01F1212E2802BFCB37FD096133A906EA055423A9D107FECBADED60990AAD334F1073D8A8406480C4D071A8C5D4585B649A02DE3795E27B70255F3BE4F5EC7895FFBFB7D9ECF43E180A740C05368E055BAA4D225952EA96AAD4A4CBA6CD265932E9BB09B6D9AD320F8E72A1E80E0D1553C562AD5AA1DA6A048D18937102561175BCD5B1B32752F5825ED966B6EB1C2D766A11B1007C5A1A55AABE6BF545BAF6DD67C3647A66AB3B5795C726BC408A9858CBCE6135D79A054C8E40E0DF0858C74286B1632436BBE98EB642B39676234539924466E8CA1BB04F2936589D7542BDCE2C9259E88FC3C7F91FF35EFE731150E9618D372F4D25469B6345FF22F955A25F65289405D2CAD97364BFED26CF5ADD7545B7A88988CBCFCD89B811AD0EFB097865CAF3FD87A441EE0CDA709299EEA0B84B8C1F4505F40EB23C1502AD88FF000A981E2C3C22233432045DA48B8F07C797FFF5A880788F007C5C1BC4FEEA1E4205372488E00E03629352022A069111F121D7CA8023F8C52B40DCA716F75A43AD65D0CE606C9E1B9EF4C1C994F2B317ED86D3FD3EB8EF03E7D7278D7170FF5D60FB6F7ECCDC655514FF59663A42770EEF1CB5F3F70E233EE4FDA3F3F69A87D96951B928E90C9EFBF581E3DDAEE7BD1D12D4BE16B277C7B7FF45C4ACACE33D09634600882BB22CC003BF7D45FD7180B8A533F8ABF274ACD113555D4BDA9A20F4C45F585A1AAD1FA02C16D6A13086E529B40F0FE15FC7438AA76AB10041FAC76CC79BB6BCE9BEF526F1A70BFDCE45173CE7C0DD060600E5C3FCB118E5239762557F10BB8014E01B2BD0985666346BA35E381B5BDE18D6020C8E3F6755464D73751833AC6A4237ECFEAF3CF778289092F70B56A959B7639C270CB1C8B3FCA3086391054707B0FDD3E3C331CB6B251EA9E288B268952F7E0CE3CF7A89826A8DB60E5AA67382BBBC33134DC806BBFB5D1DC98A13D59C7385ACB22B3D6BCD5B296AD6D2B60585316EBE26061111F1919A5736D8F379786BD393B4867D7D152A36027E5D040B490E90113E5B40923634E0A9AA0B4602B75861910824A0FDF0A93701DB960657F0527576C567C5F1284A816B554D7AEABB8961ADB33DA52C9944A66D579B5A52EABDB6A405DC9AEFC989A072F7B0B1D0338B0E52137D0006C4DEA58876E09FEC0183364119C718DE142BBAEBA7011415989534B281D986EA6B6D011231D5E86DAA47CA47E2AFE5C57FD85E2F878B1D818FF96B66BA2BD7FBF930E0733A9BE7C8CC403E7F040A3581C6F9B8F8D1375907BAA314D5E7AE36386265AF30CFBE4B3ED83E47CE03C68BB40B677D48E485EA16D9FA2E353BEBF8A499F061D11DFEE8AF8F7AEE2A9D873008FCB517DED499B9E02C187F41408FE484FD1F194309EA2335C2187AA16F2B000E05748A4DF9798F2D646192BC0CD8D8E786DBB2B5FFB06746B577E98229C466C7C1ECD6A256AAF404A75ED29BB65BF1D7BBB7FD9E60C78B364FB2458D9B47DA9503E674CE432F9490DB7C44D2BA970514B1B052198582331372A318C10845F162F2A445923AFB88DA22706F7D98ACFB193C914A8C0D3B69F6A3B44B56DE97ACB20A241668D6563DBF019067EC4587BF2C08DE2078C95A2FD1B1395611FB94F19B2715842886C1C910E7C7EF2CEE1FBA011C044A879CDA6E7C64BDC467A95AA726BF1940449B72E5308ECB1EB0C55104DBC525F2626F60FF6897A1FC9C4D2C867A4DBB141E981962DBD9A4848056E2DB4EB8ACB9042504AAF05B757CE48484E360A0BDDF6FFE2EA44D8A32576FF8FC6F276A3618394967EB9FCE94FED325369F9255375124F95769E1E2EDA8DB6F1E80BF7FEB22F9B1D89064F0E9E7C9DFDEE0F6C93AA8D3032C3F805C8A455DFBF7668CD4E51FCD0E868082817998E848EB0826A49E0086CF237AA270C5CDB8394B19CA3930E9EF869EAE528B038943F9C040AD1E9728AD3E5140773337E01046D57A24B129175FF109F4C0DE6E90F6163F233A09521A6023AED19A3B43256658634815E9A00F2BD1216A2D40ABE0FDEE139789AF696DD8198C7F6FAFA3A02FD0E8CB1D76F401E062D433260BC648059EE9A58D7EB6C0F2711F8FF5EF80DBE15690917C437E50B3D6FEA17EB9779BEAED553A7A5D3F269FD8C3427CFE917D8F0BDCC96CE2E85BF1DBBE1BB21DE65EF8A5BF23F7A424DB9A936F59AD1AC1F1417F9AF8AA1325B948C4163A85CAF919A14EC95A6C90BD271C39F954E9293E21DE98114F8B8FC9CFE5EF83DFECF7C20194E487ABFAE1F60F7895C441695684AE81733319D3BE69BF61F0B9C928ECBC7154E13FBFB33FA31D6DF2924E53195EA9F483E3E57817BF40D8108AF828F784ECB0902FC7487AE044A5770D3EFD0CA80AD01AD0C10FC9B5606C7A9D79E7215C52AE4A90D286914A99214A9D2EEB4241256EE511449D35319CD0154CA0DF06C38C32329E5B263B9F244253336C9949908E428CBD0E306610D1DD87498B0714258623086AE107F8E15794952F92AC324D7C887EE2754E1579108CF81FA354DE523C3C292C06E0B6453B82DB0F3C2BAC00AE564F2A24AD4945E2775402BC62A971947722E39EBCEA6139872C892D3725867B6565F235FBB6CBEF5159A06161667200900DD1E91161F62781F5A96858F30AB81879A0D0DB78CAD1F08476A34CEC61CD58E7D53BA7E36D40918F880DAA929D21691D6BDF12C1EBB1E0C9E82FBB3B8B8B030C3CC2C12AF2D6216980568C9AE3112D8260E5D999E87FE125EFD2E082F2FD659AC7C917A0427B92E7A53D89B0498DE814C8462ED4AF6148134E3468864F87270DF041EF50F74C77383A9842F97877797CF6839AC65BB29DF8D34F1C951C6C36EAB323A94AB98BD1C170C2AB41BC3BA36866D16C12A97F418AFBA13F28EDE3D2484CC2172EE852F4FDCBBF7F2C0B0A53DD3DE3F94CEB7FFAA3987DBCEC16C6F448C19A9DEA24CA4C0B9470BBF9DEC1184783F6B18AC33FE87F6EF5E35CB31DEB248AF92DC4D5E696F9EAAA9C4B2E448D2FCA46FDFC567D3721673D45EA03D1172542FF9E94ED64B02EA50D68B0B1C09129A6D08CD3684661B22608380090782BFD3DE48E8E29C80D087A906823FBD8BE708815F405A09C12BC82870E3224A9CE696782F2C60F218C11688747B1D1BBB1DE9C68E7E27A750628BC7694583D31826482874116A33424B155E9407608297F668E00198202413FFD5B234C15D1E735D6D25D793DB495F1249AA79701467774F7D7C942457A29FFB0FDB5517DBB6758579494AA4294AA22C99FAB17EA83FCB3415EBC79615C7DE443589954452E3359163A570E60101B23506E66C48D2A4696A146B876E4561EC650F039AEC21D8D310A5733AB55837A3DB821558103FECA57BE853813569D20D4381355B22EF9C2BCB71B011E2E5E1B9F71E5D92DFF9CE772766BDC4F4CE7A17BDCBDE55EF551828C87A583814237AD89A8A7B52F6B23BECD9074B12AC12431276792B8C4C2554616A7C5526B332599497E555F9AAFC0FD922BFA3EE9050DD8D4769FA89685A206708F224D54C6B038C4510A95612B6B41285D553E2A8079A97FCE3954EA9341A70447C8161177159DEFA4F796E77880A21CEFC59858A7C5A99AC59EE3A738C177754266FD3C4E2D134F12B7A5DF4A3BB1AB56CAF8664F153E387458FE9C4AF9F35E82823579CE98D9AE98D428F19C55133E54A998E2B5308952984CA350FFE5BAD37AFD6AB59B55E0030FE6DFA716C4DC23035834E37E874A3089FD6B4A1A3A8E034B8FF8B69C379C5200686FB7B6604871659DACF628CA28BC670D1182E0DEB2A8DA16569F16D6F7ED88DA18D600CB8FFAB69C3A11ABBD5FF08D00B7134D59FC9EF3F8034A0558E364C1C936990C38DEF365E69708D396B25E74BA66DC274DA22A0E67990C12AB9B000C2EEF13A1EBD22B9ADF89E32B792005AC804835E6FD1BD8CB19D13D3101EA2DB048B70B43127F8721517CD0597C6D3D2645831010CEA338A657A57A677E51A3CC73D9A169A360FEFE92B9A34D4C05160FC93F6168BF335D40DE8ACF5720B8CAF686FADD69CDF4A29D776ABC0CAE9098FC0D067BE5D2A21D103AE5BF6EAD1F9DF31339B9F31FBE1CCC099DDFCEC66C0E7F7F97CBBBB07D06C705CD868FE5DE55600E5CD4590BB869DAC3689266A7AD8D7661FADC58A7A380786698BD5F470E550CCA587BD6DCEB11637F470B6CDD9D7E2653D3C0386F9F57823552F1F0D37F6897AB16E4EEAC32223242B73C7F0C324D3B26413ACBC45A8CCE4B23EAFD404F5ABB812D1AC4696B596C66A6D52309D457DD448ECCE16C972B155648BE853EBC7CA895A2D529FADB32BF5D53ACBD4953A5B878C7FD7A38ED717E79B6DF638D4C1577C6D72F2352A89B71431EC96C078FC69F732FD2C6A63487F3C4AF457A74591CA2878B9CC162B1874870872D8134BC84E7B323E9490A341E270C61CC9209085326D744531B36010D0C4F01AA539617F3EE3572D1544E9AF9772BE8A94B44CB745E9574B36002596AA7C698CD62B6414283F2086556FB755075C1E6F9762C67AA529052C0315CCFB847BB6DD8255F8FFDBB531327BB27FD7B7C7E62E0D9C7AAB7AF04C54B54B135FEB4CBBA7A25E891F4CCD154ED7587660CF4C27579BB459A2E9C3138523BBFCB96A67AA940F50C19D72128FC1DE3FE91C1A39F9CD17ABD5C69E4B9D73739A1A4924BC4ADC354B7EB43C6A160ED88C4EF5C42838A1C83D07BE9C194A173B03C727061389C1A90639F1D374579803FFC90CC3FD0BF86F8CE377F25F81F25F962AF31C6D1DA2538D23918CE25D3C94D0454A6422651191B288A826709A1AC00E554676507BA406C62794CBC0F8C21CC2E12A13A293433450888608E93E0CA15309AFF7A4BADE158BD4E852A38E8C28E10C9D09B2892C552139BA9FCCE5EDBF8502ABC0198333893D0967222F04D22C65A04C066AECFDFB0A487500D6D3227D07EB28483BD820D76C93CD898C8AB98FAFC6DAC8519B2E20D78DEF4C88B41A8B945F44CA35A2CAA24BA52E554497AA16C699101D19A28E10ED0CD10745AFDE23191D290847E87A61FC893CEEEAE3ED1A9E81C7428D3C4935326E2CF614CC91825840D6C816660B8B85E5C26AC1B28B2726B557E0AE55B0B60A1B05B655208BE0582F702151D5C3CE36E7345D315D0F270EC5443DEC38140FE9E138D08A391ACFA546CAD9706E5F9089E7C7E81327E271A7D32179D584B02A9296489CE2B27845BC23F2629BFDC01CD4C7428991883EAB2FEACB3ABFA2AFEA2D9D6374456775D4057D4013FAE2381004C800CA0DC00C8FBBD79E3E461A989CDC26009AFEFD3E3F67E5937ECE1B2416ABCF12E8253FE4FEC219F8310B043405E4FFCDBCE094D8405B9441A7B2424CB1738924E0656DC92925504894A8502D7573FF7F527E4B8C420EEF743E911A63A4FAF39F549734D561CB3DD399729B63125FAE9F3F677360EA7A6672CE482F731F7C589D9BBED4B9702CE20F2612A921E76172FEE533AF76420B6A0872B372921CBD762040339385C2F029F71E64A69309B1BB76E466108428D594321594DDFDA862B3411BE031DBB0130DD38D4E9E0EE3BD49D1A624996E05A688BF4DA10ECAAF87EC3EECC771019C3C88280CF01E8A518FAC500DA95001C953BD8126CF876539124628D2928770849A47FF04029BFBFB5706C82FD477D53F928FFAFE10FAB8CFDAFF37891CE8DBAF1E1B788DBCD9F786F3E3412162E60B7C642F00F54A84DC1AF828C09A117250ECADA69F479818B077390CE0E5C906B6B3FC22BFCCAFF22DDECADF974DE834E52BB03DDB1BDE5BF519CF2A5F7ECFA83F584055596D0D1FA9B666BF71FC861C3E7823C21F7CEEF8FC078CBCB9CEF0704636D7B1D4EE9DFF0D13E0F20CCF78B8FC5DE5EEE08E5BA842CDAD0702D84D90507FD231C426834352D23AE4727A342644021A51FBC0F20960B9ED8A46063968066C5E8DF15BA0E96E9EB60F284F04152FE094EC9D375D67D9B3D68BD245C7C5FE17D5B3BEB34171A1099B38D8B8997D41C5353908E700BCF41BB6498CD40450DF6094B6706F6D8911446F5BF8FC9D25D18692B8642088F380608FD51A8FA5860AE31313DE98D53AE0E947D4423562998DCBA7CFDD79E5CEC5532FFFF948E1F433575EFDD6E5EF54B8EB6FFFF0FA4B8F56AEFDF897971F9E2F97DEBEF4A7CE27577FFFE59B8B0CBBF9B073887B1FB0986226D9233BB0A84F99C8D37969042F9215A126F9DC7E46E374376575B7A652910874BDD6D38D94C93504999D0A4C6ED8E8E71DD6C0FBC0D65EDC14810C1A4D3A269A562145799DA1BCCE10402F703628C80794C2A934C874A97B7D5DB905549DA188EE91F57B4C7EF3D14D046A5E42CCFAD094A4A93DB03A8A6B37655DB7D6AD2A565CD417E620158D1A8C1AB63A520CF13B6031365C0D2E00915052BA5C4BBA1C0C74BCD1A5E3DB06A2FEB23485689E540E2ACF2B6FB8F8D7D3642A5D9AAAA69F4FBFE07A21FD7DF182EB42FA07E235E1AEF8B0CF9E9D9A1F6B8E2F8DF3E614C988DCB0DEEF0679E77F3DE60691978A33A9E8E15498D9C7F61BC31C3FAA4C105C092BE09AFC3E473E1791562576515A91AE4B9CF4B9C6BADBE49439A869B3D1E528BB12254C5489B6A2EBD18DFFF25DFDB16D5C75FCBE773EFBECB37DE7F3AFBBB3639FEB1FE75FB193B849B9A93437D61F5BBBAD4C20DAA0BAC9DA0EB12DAC8907DAAAA9344368A303D1402741A84426FE4068487464EDEA81AA75C882A1CE5B05A508A4B2FD11954EAAD998CA843625E1FB9EED36088948F7DED7F79EDF7B397FEEF323C5A7A6EEF8CDAE5EDCDA2C539E6D7448E4EAE0BFD508442DB9C7B19C5F263E8C22DEA88EBA7C427663CE9B1BCA8EBA460CA8FAB0A9B9C70C18162B06C3DC823652EF6CA3CECCD609EDBE0C2EE75853889E9976BAFC95A6EBEF67A7FD6254D36980D3FCDD00878025B4CB656B61E2B6086E5D14B766DF44D5229B6E073BBE4BC168C3467B668B053DB7E3C4EEE7F6CD7E6BE6C59D63F991A8B56BD5D03699C1B09C4EA859D8E8F67FE57387B63CB0CFDE3B54CD7056E3CA9107A7BF79B973EA58581A5C7D6F7F2D91CD42441C3EC41D981852FDC7565F3C9CBE63EFFD5F7AF50FB3F7AB0A3A241F92F139C47E1EFEBA0EF9F92245BE33190D98D4C4986A127A41717DAE4AF6FD4FB2EF5C9204630102FC248D7D496A7592344FD18920736A44FB35BE0C2A9343F8FB779B87CD632667E65DAA974308B6497EEA607AFA1FF722B77EDB772C7D564F93E572F8DDC3EE636ED68D0BA84E3C29857F80E62372C68F29FC93246F12A227C53932964C160BB74D07AECF54C7DBEDFA2DAF11B30F63389046D811C9666DE91B0E975D84C922240976691A79266D9AC69DB984B995F188C540C890C1A1CEB9C16DC95EF04E701CE3C2BC31E904DB09CE4AB20845269049269306CC19F306CB1832E68F0BC6258337A60A3F7D8C92FBAD04D1589E6D10FC6E963B8D4E3DD04D0A16D387317AA106FA0004E35244D5A8E4AB9EBCB729045F7E94CB778D3FA290506698083F92667A43DFC3F7B4BE6FF97B6A0FF73E7E64D3DD1B33E93D61253C3814F47D66CB6A69FB06CDC3FBD27AD2F440983BFDD65B7795CDB16DA1C2FED57BEE3551D83311EACE0FBEF0E9381177600EAD2DB37F424C0D3B1E598729B3463155B3896EB3A0128C804A3001524C174C2FB96FA6A4E6DABB54E42542A123645C1A7609A6947228251E8EF030CD039FAD0240D1A53D9180830948640D1DA6F4199DD51591196FD5EBA88E55ECB1AB238D8E1318A123685F6ECB97BB1C7A0B412329C9141CC54842A9F06C71D8D55D465376F1F028FF14CFF2D9A26B6B020E25BE9A601359450472C20F6D9D204A926A23BAE0A78ED85448679AB5911E57B6BA7D0BD5B55E2797DC6AD5C7E59662E1001E8AC0ABE02E6B6556512AB66895F3A2A58626BC5FCC9D929FCFF01E9727EF294CD5666A7335A7546B82613F8BB47BD177D1DFCAB4B27F4E5FC9FCA57CCD712D7D2DF35E5954C6CBF5F2638347CB27E0047B829B0BCFE973B1B9F8F1C113159F0412EBE1DC5E67DC537E63C3EFD3429C8B84947864402BC4CA0BEE05CF29E364FA6446544ABE7C796779776DB2F664E1C9F233FE9FA54FD7AE73D7E2DE82309C60CEB3094842155868426989395F69826E078A6A423B1F4BE8491D64DDC0274706B5F31132B8415132699FE8904CDAF109F81D53A9168719863C54FDEB9AA636B9ED762852250F967D530150DE4EBD937A3FC5A59A5CC8166724989266A47989939A30666BA6AE55920208E54513A6CC1973CEE40C73C864CD5F81C18C80F1CB5DFD17E8BE4EE32635DA2BF5BBF62EADA5A03E6155D1712CAD0196A80A9D651C47B123167C59EE8A2D69D0AF78D0F3677C62C8E7139FF5574AFEA3726B4265E41B373BF506C89D9B9D6E4DCB2E88CE540CB76F23539AA09212CF1792861C70BA9281541C9C05218EAF7922CEB8F27C1CBA72F2F4D3C4C7E35EEE4F5C1FC91F053EC93BEA13D060F075C69BDA222CB28BDCA2F823DF7C785E9F8FCDC71736FC20BD38E845E354825906BD154E13ABE96AE6DBE553995365BE3E41EC54206F68963BAF59607B2C16AF189ACB258FA5138FA979AC0ADE2AD3CB6D79E58432EE374883E6612966D14EB332CDB5EB4B412BDDEDBCD8BD12B4CA6AB0BB96D25D4B52700B05B750ACB2A190EF7C604B124E932C4EF6E13E3EB2C007B6E2C37D7C38072F35402FA6F4FFFEF0D94C504AB37D2C5BD1F54865D8A5798A7CD375EDCC74D1A524B0589A567A6E707CBCE7CD09C705D284C4D00A4623D16897ECA888A6033522AAA8A9B90C758C448849F061E753B927F66DFF82919CFCFEC5F35FFBFC742A1CF5A552F11F1FD8B6E7C1D5BF0D0E9E7A6AECBE5A4056BCDCE9D5374E3EB273F053F94265C7C19F1C5D487874D8F19DEF3E606DDB3F7F87B567F68751C9AF22F385D6FEC96E76BCCEC4D8FC3AE6CB0ED80A32DF804D684DF4AA4417BDE120F0415A06A944069B6BFFA6521A249A4ACD2579825EF29DA02894A548C8D184D812034ED4C8954BED6AA7D553C7ABE80EABFFCD6A5AD44B042E42DBF0BA1A7FC5EB6748A1F70B0D0B3B44AA1911442906E18743704F08E876360218F71663C05333C90B444079AAAF3C1EF01F74097252AAAC587C7C8E5ACFE040FCB6B2962EB549C658B954AF5F90DB72AB8ED2454F8E6088BDCAF8F000777AAD499864D9F18185C082F65AF8B54853BBAEB91607E0B80EBBBDBB7D93DE49DFBF54DEA9865553E5226155D339204D28F60270E1A1DE69B9219605A777941C3AF276F89DF0FB612EFC5028F6262336E1865D3650962BD5819706D80106C0E1E033A1CF06612E084C500EBE14BC10BC147C37E80C4EC57F7EBC6F255708476C96EB37D19574905D3633E32BCB4494E50E0E2D030A338397828C8EE6907AC406B188AFC400B90D1C4DD78D33D3E09484A6E05E9A967AD8B52C5D6E53DCD6C269B483049135926A72B9D1407A740C757913ECBC72A5964F6D0998E9B9AD95BDC5EF6D7A7C305A70BCBEFAC7ED2BBF98D852C81F38589B3CC87E391579F8EEDC43883F766D995BE19E67B2ECDE75F88B9836C1191227811588469E7C346E793223D1CB2ECB769046169D4ED4953899A7F481A9F4530E1637CF92894AA61F6AFC6AD6291A7ED53950F68B2E0139E22C09358287A95E2DB5F1B747E3322E776E7411DB2ED1EEC2D5D27A2FB7C7650B53C28CC0091ED110557F261BC555BB4B8A2010A08187A00C28FCC0D01DE4934E6D9EEE21F77445107206C5A8E1A491C7C8E1693FA4285588472543A4A028551433D7436980B4D8C86DFC409B0B04B2E308576A06D193B689628F823985326C98FF61BBFC63DB38CB38FEBEFE75BF1CFBCE76ECBBB37D3EC77EEF07B6CF59EC4B622B92BDADBFC25A1A0DB5D0A2A84330A1A2A13591BA6952AB787FB0A4122808953F3640A92641C71F8836699374D1B66C02D47FB6F50F5A6D952A102A83413395A94C93D6049EF72EDD82C0C9BDCFDDF97CBFDEE7FB79BE0FAD3F17CC50531829B4F57D857D7A58659307694F533CA811B3C49AF861466377E902C9B3AB787737C92342A0E4D1E789F1022F08459D3633317401E3383E8117F0BB38845703AF75494251CB89C444F247C9400F860BC9204D4F7D3B41213D8DB766FEDB2B42A98344853C4534333B7ECA6ED03BFFDC2D426912B3B9B8948BAB39244A59319F43152C8E41354293D8EB6A16CB72068CE4CA53448EF0422CBFCA4617A1AFF13216DCA4E725BDD6656824132EB90FD215EC23E316B79318B64C37F8AD78315D30635B1FD59E39B5FBC0543537B20F3F7CA453F9DE63ADA3C1B39BD717F6E6A4D2D45BBD478EFCA0875F7C78288BC9E64F7B13C3FB03CC574602047259825CDE805CD6037FFF229797390EA98948EA75C83B09161D9640F0CF1711407163E3CE9D4E1D2A531D266A3BA71E92792ECB72DC40117E27A4D2340F52C988F425AF35484402DE1E2086EEADE8F43C6F57BEF84F789EBB7EEB6DF19637FD5CE2ABFCD7E56F2841A0E67B8B823B40ABE137FBDD9492524BDC005F94F44459D6155D6D732DBE9D68C9AED256BFCC8E73BBF8DDF26E655C3DCEFE8C7D91FBB9FA527661E057E815F617DCCBCACBEA2BD9D7D9CBDC32BF2CAF28AFAA6BD9F581EBF227FC27F2676A6D81C3F42A4B434F34BD5879C88F9AEDC7BD7BFD689A7E2C95FC28495EEC76955C333E700A4DE3E9C089F029FDF9F0F7A5F901AECD36F9A6DCCAFE3EB25E7C4F65E6F833F2AC121C49EC93034939A5255156D75082973450CB0BDD2AA72ABAAC28831C9FE2383EABAA658E8535968984432116AC613201F60D45544590573114BC633C16F932BFC02FF37FE0C3FC692E4B935DEC46EAE7D82BEC3BA0F2D39C72525DC359A4230EEE379E6872F4BE95BC1717875C1A56A22EE2D6A1B55BC56F2C8B03B837E0BF0D388AC6E578B259A4A856C44A656AFADE24E58ABA297FA08036E47BEA068DD3F286DF46799AA0BC9EF56DDD6CD891BD950AF8BB0D2CAEEF1CA14641F730F5C0997812A9E069F05997793DDDD701C8FD6D05225706DF0E4D0BB8251E42974FB6581DEC122CD8AF71D4D4789686535122A20ABA940AA220E86CF129C837D05666C87332A0AE64B1DFF730C9241816131C8D5BEC8F40C7864BD8304CC394F06F72A6DD7FFD468615069AB8D24C95725B6BF6D695B4559086826789A19706B72281BED17C8C8B0B8484246DCFFD8F82E1E1BAC8B10874D5F7EFDBE14BA0AB6A707387AE8CA226C502D555CA72C419321BB24821128F5041743AF57AA6256E5E83CFFA0E755D410654EE5D94A472CE6B82BC113A3B9012EB8FB2C18590E59DFCB92AAEA2930413E1A4852DC13F7BB55A2B169DDAB68FA3D7EA4C7626C55B93DEC524AF4FF2DE7FF662C2A1E99CEBB8695317758998BA73CC39CE9D703E241F5A9F924FAD283D6031E97AC75DCD169A45C7B1BF3D9C579442B6243A21DEC81B55A3651CCA9CCF9C97CF1BAC4046CA23E641B41F1F60C6D9BDE53DE601EB803DC7F4C49EF4433267CDD93DE725F12C3D98AC8957C815EB0DE72AB96ABD4FDEB7AE3905140E3191FE5086238CC95911DBCD3C2A3E2A4D841F670ECB8FDB678479714E3EA39C29CD9139A3E76466B91732B346B08F3B829F159F9542A01E98524278CC807EC48CA4897AA9A8E9C8AE6A28CEC7B47841D1B402C86F89B54C28CFA7BB5D999475966139A66C5B29DBB620258839C8722996E5C01929FD659EA4789E94CAE5415949C9B2621B2545CEF0A0541EE6610DDF01B969F8CE5201C725BA25A218F822A8ABA25828E83A0AD09D1855E11090B3BC86BF8B0862F12FBB71AB0B375B2E5B827E3FFE240F5DE0C54BEBE849BBB48AD96E7F375B9F50F03905BFA6BCABFC11F8F8E3721D40905DD1E3048B30E954B442B449D6B0880CD40F2C8876F9FA3103778D9E1130C09C5DE24E9B75F65500020B568ED791857BD65D2B605137013FB5CE311421D9091BF76C8C6CD1D6EDAE7DC15EB7AFD98CFD44ED73C7B671AF3239A5A81B9BB7A14D9BDAA600EC5261077C2DDF56C1C6D18562814241A55EAE3346EDDDD8F69FBFBEE17786C0099F1731E005FB001CECCE3D95FF8790FF1D19911D63C73CB44CE14960CA346D7A262B942A86988A76682BB504314989926F657684140D7717332D4243BFB775B1DF870CFDF88C01C505AD404CD28A6154F56A79218E22C4A245BCD169340036F0443E6A223E694C0A169F330FC8B3BD8D4B411F3C7DB80705FEB7BF6BCA667A0C5FDAA7A5D86B6FA6CC162E7ECDDE7AC7FECBD6BFC8D6CDFCE8180028A4E50AD5CD7FE25FCF8E656241428219B194EADFFC187F36AC27B500217DC7EFFF2330BEB9120C8C37FA2891B20805FF0A441A0D7577B8D6A8C1CB4D23544370BA3A3CC8A55A520C8CC2CA32AA69920FA67A9D5269DD1BE8447970EACE2676F378BE6F3E362FCD1AB3CD1BC28DCC4DF366838B3B064F8472749A3F297C30C4E4DA4EFCE870C8E9843B62471A353A56AB39D81E170E8A07A53DDAB8B1DF7AACD96D1F560E9389F6496646981167A499F44CE627CC82B8209D97D70C2D168E8B71295E2D8805A950B5793B536FF362FB10777478A21DDAF62065B8EFE746F1287D9067EAB8EE184D990F21873E83E6E4F32DC769B71E00B05EA7A500684B09B8EE8FF4999E3740CB9974DA6C365D5E88461B606C1846319A6EB3E192C47CBA2E61C905639C8EE64F2B131AD6EAE4E9D24C29509A2FE192421CA7D5A87D6CDB666302DEF86917BBE13043148629BB24E5BA249A36CDC14634D5684461F6652E9A6998441146EB86CC07A34DC6CD51A85639EE9143F11CCE15604EEA0E9D10300992442BBF53D3E9B7A11AAED5342DCF47C1F45E7E3A8DD30E59C5B1255DC10A3D455474BBCA05E54FCA5D254477D0BAAFAC0586510331F83B8BAE63024F96500337D6026FA216FA0FDFD51ADBB67585EF252D89A42591942591A22C997A3F2859B22453A6AD44B493388153A7CE960C4D3AA141DB745E672C7650AC4930A328B6A46BD73546F7EABF7418B021C8302FB6536BD886E58751603FBC0D18F600862018E01645166DD96004DD5ACB3B978AF318D649B83CE79E4B91F6BDDFF9CE7786A9C9E5C83AA4B676B7D1DA6C095B5A439B6B412FD6C9DD86B653D841FD5A17A1D6D08868B3DA3292BCEE7EED15F77C27558983658F315F906F0B1B0DB2EB1BD6D67B8C46A10111C19A0A5FBE0D9E83116AEEDA2B6EA136BFB646CC1AB3E600C340F418E4F0E94683C882393407E9FB53D40D59C919DDD02EADB286A4863D75F03F5806EB036BB2BD62DD650685BA4CA23021D6EC91DC759BE9E9AE3B64B8E8C41B26B2076C26CD93A7DD798737122A4FC4C59F9678C341A880374A60DE71C182CB8A98BCC748AA64881013C9EF409E5A8264C9D33162479E045D86001B20C2904C8F2108BC21C2C8993EA3A7C32BFE8EF19062EA331498993D3E43677C46BAE835323044C66FB0D6C3FC46C61461F88C1219F06689BC1D06F9F955F1013B3DFA41FF35C78F2C1022737272B2D2D54F09D0FB90AC37B91911E4425F2FDD4FD8607926DCCF114A23E2E91152B3E4935F92AA84C676E493A3C7EF977C91C11289A65284E8ACB95ED5AB5522B3827831138975FB470F1E8826B13E101F383ABF71E480D19ECA077ACC0BDFDC9BCFB77F1F0F268F5FFFF1C4E15D4073BD925C12A2D3D3CF28BE10909C1C3DFDC376F3EC001D8F7BDD92D4585B7B529453543C6EF3865EDCFE78A60A59E76C8FD39BC07325EAE8433C072A5ACBD2E84C0AA742D0D9807AA28E7A09CD89962B1297B25C8AB825CB2D35779A1EADA5DD866FBDB0DED821C07BBC13663514F28AD4B9122E81EE44F6D839F20EDEEB2D235429DF975C371A6BD0E75A4C43FAC581E24F8483479EF8050A6E7F8802DB77900265861386E003C7728515009A6EEDDB19AAA7D2EF7F56FF8AEDBC9D62599B8709300AAB7995241BF7C495A4368475CF6070BF679A9DE63E1F784E7926389D3BC39CE5CE065E545E089EC9BDCABD1A780BBDC57E57F98EF633F4DBCA7BF61828224DCB65B31CB63A8A00694372A57B6D489251038A52CC725EB821A7695603A265E1275985EDE2981CD800E81C2676AF154911BA71C35F9B2AC48C105F91242540B44AF022876F727738EA0437CBFD9DA3B9F93AFB38FB144BB3F30C309519D2FEC0AB98572FA9947AF1A91C2EE4EA392A1728572E477EF012F0D021E82826371A731B5B9B8D4DA8E35B87F69DDCFB3EAA4F6E6D681D2E220761510FF3906E004B68E91365C2036980E708AD74B24063437076A59D8E61053A8650AAC9A84B33219EB40E56F3408EED139B07AB7BB0DB63D1E460452F97247FB90498C749ABEA3BF1155F3E1FB9B92E3A98A886B389B4CC06DA5FD7170F8F3C562D468C3417DE1F1F6DAFF29180209501F6A9506A5FBB84FF9D497BD86E17B41772C45DFFF88BE7BFB637972DFBF9DDC72E51CB7DFD31A7E02495FD7BED71B4697B03D1E8598278D3896CCF8B5FD017F00245514DFC6793A5910DCE05EE6DD229D345512A5DA44D9AA6035DB69FE33E1441CF63843A9B5E9B047A3F24DCDBECD640916C5270990282809DA8930D1063E2E6A76D6F7CD8741C80B70710EADA056F37F13FAC7CDB695A70702963F78338355DC83E52C9234EE5FA78896F6EB75748D302CEA6E924AD0C6FEB25015B73FB7D33CEF3E0F949D8661B1B459275876427618922B7491950661FAC9030387F59212BE0FC75952C261263A324E580DEAEDF20977B93754D23C7870AB5FAFA7ABD556F916A7F74B6305BA1260AA6FE5AE135FD72E1B2FEF6D835FD5D7D43E7A6AB27C666C76EE9B7AAFFD23FAA3AA6C6B0CAF09930978A2656C2EA85A82D136653316925DC77219649E843123DC0EB43238F5770A549EF355D23893CF24D011114D37457931E37F3E94C1AD9D53E8EE5060A36818F775DB22DDA289B323BF69B316ACC94E2C953898B092AF1662A303AD6C44F2E47AE7CDF3A9149381372228DBB70365B3561034A720DCA714DD8844A7CBA566FB5E64483A48201FFA168106A1F28EE396BFA6BF56C6E777D579DB26BC95ACE54513D3BA2625205B22FBF0C75750E358E05572B1594E7B8FC48D371EBDA4C1EF9D3A9A6A3057500A5EDA16E86562390104B332A29039D2A50827C20DB0A50C09152B52C7AEF93BE68E5434C24A5C15F2E4314D260508C0D962B3A6485DFEFF392142149013904F5019FC22FCD4672838DF6D0D3BD5E8EC99FBBE9647B736AB6ED8C8FEFBE7AF5E4BBF39F797D4FBE2F5A342289DE6CF9648F427FCBBE357CAA0EA4AF853F87DF6BF4F0E2D68F665459EC8DC727BF4A1D39B8BAFE25E3582ADA1F3B5CF0F19F1A3CB08210852440691D509A4415FC3B82D3262E9DEF60F59A949565E406A42E89764C00EBAED8F3591F5219D591A452C91DC426EF2336A9588885AAFDCF0E62150BB18A028C40597750140953166229D1D5DCBE6D21169C3F5A8875ED20D6E5820DFA1F88BDD1711F822CD97402DB4A77614AA7DED6F1AC8EA38C6B25CC5C882633613515A556C28E0B312513EE4BC544574E93684A5692E9AC94CF3671CA2CE77F2DFB109AE2315FACA44401E008FD6A3A190F5C5216154A51E0817A998ABB4FB92EBA28D79B7C6050FF3F48DCC1618D80B0D66A81DCE9A0507A1885727FC9E31D28154B85126DEF49F67BCBBDA8E4C9F7DEC721F48A40C15C365BF1C948E57CF9A6E36FD7667C080B3C7800C48A60B747C234C35A40641E05E20324020CFDD58710B79B021AB6D0B9A341202E12F8C5A20E823D1FCDB64F86F7ED6FB36963FCEA55C79195E3CF3DFD8DB4CFD8D336C6625E594DF4BF301C951282933EB0B538B3270990335FA79E78EC57BF3C3D31F1D1F8F16A18C7E3B887D33F4B5D0752BFEC0D0F65AF1F278C5C03C839E845E4C7230F38D11CF07AC889FBACABDFEBF33B6C0C233321FB51C6214BD8C20AB670832D0C6127A8420B79E0DCB5604322AB64CDE994A507DC06E4860A654D2CD76FAC0B50DC3460EC0519CBA436074AA5CAACBC28DF9169559E9229132E27E405B98BAC2E0F0D572C9B2F766C2C61593311502ACE4CD83111C599B07D22E64BB946BD61DF5E97C38F1C0E3B87B00BC79DD6F3F5E1CA82134F39F182F38E933AE19CFD0FE3E51EDBD475C7F173EEF5E3FA153F123F62C7B1AF7D6F1C3FAFED7BEDC4C1AF807142084EC88C09214ED6262401F188680A030A54822DD3A0D0696D51B53F3AED8F893F52D6B1AE6368D534CD6AF70741D536699D3486B4A952F7F0BA7F18D25ADFEC1C3B2124940D5BBEE771CFCFB6CEE77EBFBFDF5113EA9BE69BE899D1FD1B25EEE3FE2A4AD6E861C9240D8907D52FE003FC64D44B51C41E9D2210FE77D156C8A93A5F799D6F14271A88D9611389C6D7ACC525F7B0F08C35D22B6EDB16B229E4EDD6B6CE26D842FEB0E62C25900158937BE073AF079CAD5A661E71F8AB6412FE4A9A002A90AC6746ED7934491E51B4AA350FE94FEE359EEA2AE0CA0D42EFC8966D3F0332B250DD870B3C54F959381EFF91E8AA71A1DC0E4BC16C3618CC66A409DCE00F22BE721BB2F03AFC2DCAC196F70141FE1440F2C70015133F92420E5B364A38908ED1F0BA6880FF84EC8DD518A9EDFFC7486DFF794BFADC7A0C044F8BF964FD7780781BE6D763A86788A1C0C3DBD46331BA6788D181CF6EEB1A313A3003C624FB250520075AE4B70EE0011CE80219D00786C05E300966C13170129C07BFCE4ECD1D1E2E16C747BFF65277727EA133F0D56966B05F4DE5B21240A1779B9349061826902447DB84708B4E67692B0C9C387EFCF999FCD673A7E3D1A3870CA6911221EB4997D0DB3531D66E1D3B7D686CECD06972C6A56CF285421DAE19C0FD6939C12D7FB48C6B448EE3741F2DEB9651A244BD65DC7DFC535F07B946ABBBDB58BF69F113EBD10EB4B85D31818F7A56DBE6D5D6BCDAAEDD976F1A6F6E37DF979B368ED94DDFBFF67BE4EFC282107E0D5F1EF2113EC2E09ED81545AFB7F948842746F0B566C513C485476B6B37C24234CAC0882044E087F8A6388EAF0FF1EAD7708F7C035DC26824FE9EE723F7D1005E439D12FEB633E802DF8F72B15A3FEABD1E0E0B8473759128479D4F71D81F84B010421DF4244D8979780E79A1031CA96B50D3D69EB5E7DAE54005EDA4EAD6CAC75987522FA8F412ADDD0318A3914659096A2938444D52C7A8154A4271C844B0D75551457AAFE2079313654B66D7032B2EE2DE53A9240EB9447F4BFEAF9B87258D62BD9111D8163992AC67AD1EC715F96A855277111E9E2B4CFC3C15CD855D16BB311871A65A540A355FB713BFF1C353578DB6988BD728BC81DD815789CBD7FC3DD852907D3CF68A80951510158F9131F233F4D49B56EE8B8B68461017C89DE4C768C6B2F21BF1329A29AC7C4AEE96CE2143317D8E639E47E353D2416C30008FEBF9427A8590814180AB152F00D269E925500007C0B7B33D53BE4B2370647C7A6061801818F0B9DD111F6DB59CD75ED5125AA9CDC2947DBE9ED9887F9CE91192A99E1E9B2F9CB28CCB220C8D8AD13F670DAEA87260A8C4E672606A68486960D14F646A77A21CAFE73994B533955AA552C9547003B9CABD0F74B5CA1DBCDF68E28FCB65DDBD0FEA87589E2B1B12655C82C0683B616C6922DCAE105137E526C268E48DA8EC6B27EA769D26627A7AC308578069098F038D6952EA0A491A51744C48135D3299E44C704FD6A331B6AA8DCE369BD6DA6D0FC7ADD2A52573A83F5AAB05521D7AF180D6BD2520D2FE5487E1C20535DDBDF7CC48747FDEDB9CD83921FEA4B5C3453381B658DAA180DF25288D5AEB945EB2F8538CC16E54A92D6E33EB0AF49723D777D4DEECDBC5C85856E5D9D94BCCD6DE4C0E78B52CABEEDC9921667770470E4F0F864C9E2EA7C49E8A38FE6E76795CE6D0D05CE29BFB69C1EF312A60838DECACF41544E634F8413677327FE5203C383595CF640A79AF974BC56CCC42FEAA1AAAA5520FCD70431C0C7330CFE5B9D24B85BE1798D2F0E8BE52291F0BEF73DA1664C3BB92DE8E4C9D52A133A53CC24E4C8093533333548352751951AA636AE0A973E23783422E552EEB6A77780ED1AAE03984AB816CF93160B23AAF1089775E2E43F50F8D8EA83CC6645EC3B456BE23728DE2FD2924094CD21C8F3F065288773511925F6E9FCBBB830E436B1BEBD0DA694FAB351664944D1A783ED1A7F7AB1C3DC89EFC095A43B7E4D3F715CE4C5C29AADD0CEFD06EC4EB2DBF7184DFBB2D20EFBE2DBEBB81ADCD4A194DD2573C894CAB3B2831710163BB5169EA08DBDCE7F6C725646D4B4F8ED140965530F914FCCB829692D796B68D0411635D68A497286E223EF1CEE2EE664FDA37315A5D873DFA6A8252A0E32E5967BD887468009DE038F85E3652288C37175BBDDE623A9D1BDC6E6098A210637839C88573B0982BE6A6172666A6C7F7B899D9E989FD3BD35B0A18AD633C35A8B4B3478F86BCCD2613948462315ECD86C074717B78D6C0C840036EA6CAF328DD449F400EB112EF34748859EB2A77F86AF41169441997DE1CE2AC97D58F570DC6A8C0C5C37542486969B24E6E1D7B9A5C05BC61F025BA95E629A7B595D1B4D8ED0E53B4CB263DA6660D16AFDBCDFA6D0D344A6D739BAD50DAE396796309F395767D47D22FD29D494FB338A766B6C645B5B095D56C04AD65B6EC3DB90BA326BF033D94C1DCE20CD85489C9B3DB8721ADA6D699748F6E8F27B85CC04836D96D3B3E2FA577783066A8EC18E8250ED6AE65063C2A3C56FB47FA9EA0DCB57078B2CF8B3863FF768B797209E9770CCC6623DF1882B343278788978DD0A8B25FED87FDBDBD90B1DB8571BFBBC808C17897E016C25DA0281B647349E00B229EEFF995E6BB3A99166496AB995A055FB124375867358AB588DC33122E97511DDB30CBD89A724C0D650921C24D6F52DBBABED614DB7058795DB00821B9149BBC381CCA054D4DA6761DB24C9735429B5D663565E3FDE2DF286BC4E75F15D5E262F2F8F767B273FD1E24497B3B63B5A779AB1062945698B2247A6246F2C617332FBEFD42C260771B0C4EB38650B75AFCFAC4C17DC4D2E881A8A6B6E41B4CB8D6A4B3E3E22F5EDAC274F5B6B9838AD610EBEBC4B2CBDD8A4BE44A19F645312FFD07DAD717C1B7C05BD9610A0C32FAC553A1905EA1B05CD4BF3C07E726E6AFA6602A1E2F32F3E179A89FD7CFD3972D860B0C6D733334AD07E1AF0C32176587EE4EEDD32E864E9C4844CEB25EEF3636026C4AB9051F0EAC0A25D28A58A96B85ABEAEAEE88F7BF52E1BFC413758F28E82AAB76C8E3DB5828CF6C71FCD3BC13A1E0FF1758B8C91BC91BCFE477679FE6A0694B774FDCE88F4D7EFDBF8C975B6C1B6915C7E79B8BE762772EB6C79E783C9FC79E6F6CC7978C63C7D724B613E7D2A60DD9CD86D2DD92B62A847645B54BAB82D842B548ABB640A97601A9484815454280B40F48D0AA858A8765231ED06E61C5431F58553CACC44A282BF1C00352E3F27D76DAA62A203CF26DC6D2F89CF3FB9FF33FB8F88527C5F7525167A7F895675B25B3F47FF6BFFFD251716DFF2722E0E0B37D93A6248A620DE61F948BFBE67BDDB3D8C06762234656168E08AF084C5F007F17C0B20072424BA04D018802F883006E0BC0102401658D70366BC8B224655D376DC6ADAC85F8099EE6F18712C787398E07A0C475391A7180E3B2E98C1BCB9AA6606892CC010B415795C832F9096605D352DE1C1C18802651E9885731F0B8AC7897D4FCA5F39BE0B47A49DEE4303CF83426E4F179BCB90E1E67869735727DBC54C13DB20E2A00328F68994867324982C480202DC948E174B66082BD2A38154E6746A3FD13920D238A5CEF5FAF8A820EA108CAD710E0530B5DC67AF0CBB1CA08871023E9412D075F7F3D9A54C3A6C223700A9C221328825FBEC3DCA30AD40CB54A1DBBF9CD65105BD5EF3065BC59A59872172DD5F1E15B9B5945BE00F099B162C95D5D5969CFCEBB0016A499B61B80F26D660B7B89AD3279E23CE0E1D16C02EFFE2616C3502AEA96BAA561A3B05EA93EE6B79A2C4396D83C5E87DCE05D661C26420867AB13846D1FAF4586C093DFE3AF916637E11393ED1AB876F4DBC72693FEF2DC42481DAF370A713B9793CCC6DEA3B3679496C5674A1385B899AF55C76423138EEECFB6D6EA31ADF9C517CD71058D78B3393A333E57083B963735550F4D1E188BB22C231BE9EAE298B7386E714A28C0D1219AA5FD467EA634BEE8C51596611EFCD8E7735B7B1DFDC0FE3A4D935EBFEFE1472C8D39EC505FEA3A873C500F2D86E8131E68773A54C2B641125BF8199C65BCBCE2DFCFA8B77146F5950228D809808FA8DF1DEF740E3641731CE3F4ABA89BF1E33C762A981E922F9C38AD02BC75ECA13151EF5706861AAF85D43AA6073863CCA3411C867884D6F0F0AD4765EE49BF88443146833EC2FC13F14A24A19B338B4B68FFF997CA4E73E9B99554EBEB2DC389476547887BBDF573CB9FFFF5954FAF5EBEF3F2F246CA80418963354D611DFA6A28572844CC3C543B5FF9F9CB47AF6CCCA6956C45CF640B2361B535BFD0424B177EF3EAE977DF5C4D2A5280A759C58E1395C6B04A0F63B62C6A823ADE352F1741AB089A856F15E856065CF48345012C30609E061CC94B42CDD9B9B772D7736C2E17A959DA048A40C7D5A0224D786E1C52BB2103DED67D8216CE0D1980C3C7A3B5817094DCC5191864811D42842FD3A1AB1FBEB5A0E6166BD35F7DED6B9DFE91422311509D5A1AB42494F7A2F1B5F523FBBDDE97AF1F51B2595762EEADBCF1F67AFAF8C98D1C96368B64BB92A65F2B366CBFF3E01586E71825DD3976E1D0C99F9E6D038601988A2AD6D4091CF70475AA5B4BBB4851829A862840D3360208156B956C513450110551D0806E5A010A1024D730842C4C4B825B791C6C87947E48C20E10C0DBDE547762C73094091165E2EAF0D0896224B0BB2625CF54B4E197C11C1A30C0EFCE0953961057EA2DA73EF85DAB6BB1C4D908C9D9E62D462F8EAE96D716A76268725B2CE178F95825CFDCFB786E5FCAD73F67161A56FF2A6A8E46FAB760DE0CECC93F37F71025BC668C7E819C7570F4A4A3F470F445EAA56E469635552D5236C00FD7CBDB2E14A2C8952999E24537AA4655C8BB79885DCFD60DCB86108F5F6C78B686A13E09F8BD4711BF5F1E848B637D126A7D77A8FAEE20934C2FC386730BB5FEB56C33A3B308D1727AAE7EC5179FAEF5938D8ECDFB60B7C5D8F45669BEA8F7BF21C0E96AFF7BF92957ED3FC07B610CA148B187C772B6578A393B6AFFE12EB5B73CD01803CD1C5890C14986881E50948590D9358061CCF809D541A27613422A1E0FFA88DA0752BF190DBA88881DB7CA4E65A876EF2EF17843B1DF1D2C5B03B1EFB0EDEC963A0E5AC373A19AD46546D70726FC19C9B331476F7FE6ECD2C6F78F14DDCEEA8B87B3A83D9153F1D6F20BD3734287DE3D73F1DED5E70F7CF7830BB5572B2143F50BC1C81E1FEDD26F573F77606CEDE2CFD656DFD8D8E745025A4800ECFC348DB317484E8DFFAB547FE1F28DE31BEFFCE0B3615DF4FB68453744D20B0DAC76324926A9B3DD495F24822C31AC20CAB20AD3E54CA18A26212A50AE0213D0832BF04DF823E8EBEC7CF823E42074AB65D107F909CCC22D140D6760C4256E6C7BB093124F4612737A8BF070FF910436B777DE3111D561CD7592A5A75848EACF92A10FF7979E0F4ED7C05FAB6D5B0AA4A6CB7DDBAC16CCBEC5DB9D667FA4DE49F03CEC34C0DF6A5D5BA4F54F32B36331C709E5172ADB7FA9CE8F6A08F9E2935590D81E9DF5302C316F769476094A8E13199B1FFB986425835FB670564C2ADB0D988A8E78DC81D43D5021C89B34A486C893D29379F9218E24F9EC3F1DC403B6FA2DAF6189ACD9AC82DF579B262B5A0D0FE9B97696EED9539EE59079666FFF36DBCEE9E4DEF1871FD1C7F0BD3D2AD70DE8890CE543C50C55841EBEF7CD582C755725E3A6EC0DEE7E77FBAE7A1FF30686F70BFB7CA48F3EE53D7799CF6A923E36D18CB1ACDF1F6ACEEE73A2CDB2E30F5B21CD082A82668ABA21D162B253057F666EC0F159D4BF9E9C9D99364B8B252310CB43050F1D2918531C39EEB553B48BB335E81D385BEC39FC8FE7A8EBDDC5B939AA054411B4445E2C355AE186E86FB45AD89BF10A0F78BFA68D887E1199236173443415C5EF1F5DF0D06839D930D1A8D9403CE87965383E8783ED4A2341BF9244504BED20850DDB102AEC579A4F2185C548BD73891B78B432B16CEBB8C56A954B02B66DECF94D8ACC19E2D4002E0CCBF378B890CDB65C23F865C07F6842C3028236330494B58DB16AFBF92FCC9C03BDCEC6A75A917E2C0DB94058EEDFE112BD763F450A3B28F1FD7C7B340434BF6DE9E1588C63EE39B40C1B07DB67FB3F995BCEF819842C5990D588060EF7FFE44C150C84E2DE64E2DF74575B6CDBD619E67FA88B2D5B12494BB22E244589A42EA62452D4CDB46C4B8EADD849ECDC9A38891D3589BBC489B7CE598AA54DDA2D6B973409D22EC92ED8300CC5360C0576EBBA6E682E45FAB006433138295604CBCBB087ED6140B0744F7B592D618792EC35C9263E903C028173BEF39DEF82C6F9618D17C59EC848AEF94F201C8228BB5D4C372976F848BE8D118E12852ADD47334C5012BCC1A8550A0A041FC158BDCBF4C85EBE0FE3F4B0B1D26246634D7EEFE4D6E9B1EE2AE4236B45DD2313820DB9B8D0C7FECAD4B42CED59BDBBBE9EBF92BFF52A136AE35BFD114FB73D31BBB5F989C4AB650E8D4746D4506B7651BCFFFFC6B3D3899F558FCE85E05580E7B1730443A498A1C14D876944FBDD966E2980FC7E259F8D29111108911291C8FE848013C42B047A86800962178164026CD88C10C1698442294889316E91A78310926807C7B2597F2060C9F2A8EDB59814D84BCDAB72FB9E7E9B5ABD6DF6BD7A436FEBAF7EB75E5F23480E8B0EB552AFD3FD06FE27AB75A3FF213A22E0EB4936C01C0EF7324C158739BB951D196C0E670B018BA5F94EA4F98E858A57B5E6F95239642583C30679BF9142F74456AF251A7F8B8FEBAC24454A9B9268CFEA7B24DBF879BE2AF66206644A21E418DE6BB02DFCFA307EAF63FCE2C40FAA87BADDDA77097891B844A07D0424682068869680F00061BF0230064081001A90004C321E8A685400026E06BA187F880FD0E7DAD8530187C3CE77479698D30C9A67608A01830126C613DD2DE0EE3D6C41A4E3AC5FB9871FCDD00FEAD3F5957AE3EE4ACBB4EBF5BBE73B95A75EF737563089E091B3213E7A6CB0D3154044DF77462B5958168A31AFD828F544C70ACD0BC6306B8968C964108363C0C3B6D67671A5CCEA9F487BE38FEA44CA8BED373B99428BE280CF2E9A29D474ED12766D2FCEE2CB55B59499CCA0A90418091814A604540A4D86D0DEBEA37D688E5962D0ACF38813EDB61FB6A3397289440ED3C36942A22441BA22FD507A5BB24952CE8747AF119CECEE51646BAB08E2A48633E96381B45EEFEB68A7194BB1965A4A6DFBEEF41B9765D3E673D7BF78FCE6B9CD5BCE5D7F563F717C719BFA0149CB134B33334B359926AD9EC4C491A90D876AA94037348EBE7B61EBB6D7DE3FFE855B97B67BB33B9EFFF1BC67F7B1E5F972797EF9E82E4FF4E0E2E1EDFAC0547DF1185EB5039FF3E7301324DCE3B6DC20EC781922A50A2A5255CEF0BA658913FD9C3C20BB7B7BF35E3FEF7394F2B2F89990DDB8DD2E729D1AB7B216B6A153DAC4682C6EEE5AC9FB64D45EEF6B70A37FD7C291A7F2234F9F285416B46EB136DC684447D590233A3104B35DE198E25136E5B8F8C8E6A83CE921EF936EB1B23039B538C6BB7B9A9FCAC3492F8E1D56AE32843667C6927D6253B138BA6C91F2CE6C65B7EEB5DBCDFD8D37572D02DEDF0831423CA84E4F0EC1540136A5604939ADA0C3F12FC7D1C63814E2500B43290C7B7898E4A0E89BF5A12203B3F411FA244D1668309CFB9CC79CA79C96A15ED867830B0847398FB9F925ED65169E63618185ED2C8CB1A0B160637D6C8C252F9338F6BD40229664C9E88FB45F6B88D204ED8A466A5A256A7EEB79DF01BF72C01B0E58769C715C76908E928C8DFF936B4999257B223C41A8B8EDB5B863B2A5DDF31833F5AE33887A68C6DDFA933F8849F18CB5B04E30D3AEEDD88CE0318A5984B79A375FDF7F6657C6679D7EF5FAB35FBA7976D39EA9A0AAE97CF1E0DC6C6AF50F1DC27DBE45386FBCB646B8E62A7AF12BDE1D879717A6E9E3B72ECCEC78EDBDA5177E5FF38B216FF7D8F68CD7823EFAFFFCC32DC0C2B794BC465CAD7A2E0EC14503142C51525AF1A4D30AE424132175249B4DBF92869369389886B134A4F591118FEEB4E7244FC8EDC915E484028A0DC059DB200779B665E6E92CEFEC75F384B36DE6D43F5A9D9036A5B9FE5FC6AA184DC65468D3C0298CED79CA54214CE17A3D02365B87C08FB9753BE860865BDAD60E3E5FB158EA9837A634C936BFE3EAA3FB7A7925DBD74CE62A911E47A492875F08333B76CA894228552CAA2E80FE0145E31A1B9801ADC0F10AEBE28A332A5BA4E043B30935FF658C725649F2A4263414576B29AF68718743DCFE7266635EF2BB2CCD0F5885F7D824F81481D5E5722BB95220355D0ADB91A9F23338D7DDC38C9F267E778DEEF56DC154C58AE4A9BA0D2A2FE4513E3FBA3521C7E3BDE6D8AED15F12B708F455E21B043A402C132843809B08E31D2268FB591A76D20BF4099ACCD313D85AD5F19713B03D01910410092A81120943BD6440DD80B2B1C54046C82E8F4F4F4F8D9B1456E5502C2684CC0E63C2BBD6624CB8DBA26FDE3FF356C7FA880BCD9DD6BB8159DD7A349B5C2C168FC53AE506E1FED2114B640ED971C7E3AD6B9A12CF906B84CFE9C5520E5D951CB10171FF40548FD2E2D87E439DAB0D88D32777C6C606550F13A07B2CA2E8D687CA39D92AD5F242D8D899D70F6DD5C4DAE24462548BD1FDFE9E187A4B99CF2949AF9809C4AB4343915075DB81726AE1A912C5503D6EDA0ED3E3F5E1888BEC8B9593D1E1F250981BDD521FD1EB93032EDAE5F7E2FD50B1D6BE89B91E260E54CB61FE0D0ECE70973934CA6DE550985339C4F908B88AE038FA1A425934861085048490BB4BA27C2E9A773B38E8EF92DBE2DBB8FDE7761D5869ACFCC5C4ED8E4E75EE18ACFE27AB6E1BA612F93D9768E333E5E8B77B8421B599CE95F9AE6FE6C612342939C9FB0F4A1BA4DEC6A05C4907242990AEC8E8439F52493EC0B3EFC2FEF8129EBD411CAA164EA5606FEA680A5DA2E12205E77AE1EB0E181C0CEA844D1BD381D2055DD3495D67CA4141F233213F0CF24610CFFB37096C86A60DB642B5D9263ADEDFBEAFD7D84E506A778BD8E31DA75D6DF1616B8F5B5E1AAA866D4C7F64FFE79E497CFC91C3EC137F2F56842EE416CAF3E3D1E9C98A97F1F560251DD2703A0E65C713CDD3E553496332E1BA710D7EDA8E07DE542DDBDCE32C6DA819B233941242C5528983378562DCD7CA4C0EBC7E4B2B119FAD6EFC0FD9651FE2C859C7F179262F333B994C66329399C9CC643299B7C4642F999D99BCECE692CDF672BBB9B69BBBD3DE6D5BDC2ABE8B88C279A5A71E728AD5A3F89760A5155B5AA87F08850A9E50443D586C45EEFC435010290852107C43DB7F6C33E76F26BBB7574D48189EE7993F7E2FCFF7F7F9F655945545D5555312D927E1926449D2562B824A526AA556AB545485E3484A564885F2644590E117FFA94AAE5293852CA2389D3F24CAC83FF097460375802B3BA03F89B5B8CA1EA0207616C922F88A64253115498ADC6C4910259E8F45C73DCC0E01389970D1B3ED36938B7E4D3164BAE522D6ECB85629FA93133D5372EC188B2CC42B425D5ADCC039AD51957365D9B2F4E185DE42486D0703351DBB2CE5CE9FB38097D818FBCDE46B4FE451B369F0BC6ED31C87D1ABABBE67BCD4BFD1C71FECA3F53E1AF61EE8E1640F3D49A14B14CA5288EAF5538397C21B21FE7488BE18A233211A84E8D3C195E07A901A06E8F12AAA0661379731BE6920CC600D9C36686330C1B0157B301E0FA86A379716C25E6E74A2C9C1B06A61C565BA625B069FCD83A2048E2C5613BFC31D0EC36282DAC9E79EC7C313C9C965932DFB2BC95E3D4ED7B2B3625959A6132404D66050423A8F32FA1C5711F334FDE6CF1B4AD3B105F4959C2AB3649EC9BEF1AB14A02FA71A0CF241CE57CBD16FDDE89DE85F4EF40BD975EA6548768A168B82A52F7E866E7E64B26D642C0BA7C452AE6A9ACCE2DF6841683593112BDC0A6E5919AEB93B7977817F76F1ED547378AA46C495D0604A9A50895D444E9E6E34BCF13894248CD966A6DECCD7436F66E8A14905553DD44FE7992943335393FA787839FC7A987A0872CFA010563D3D14743D2C97751D0C14B5B3E6CDBC5CCDA4CCEB1EFA921797EE32853E41218F32297BE609B39907D26D22D39C9EDDDD9DCDA693893745D3E9241C99AEF73EBD31A6660FEA0A85184997FFAF99E32770483067F7036862A85472D393FE3EA2FC7DF61BCCC141F2C72D7B9DBB77BF70BC5D2814387014F191650DD1DD9249FDE392C55780E0E381BC54BDD0ADD741439273B0344EF5FBA9A49A6F0BAECEA7994AF953DFCA97F20423962974BF329A5DF0BF7CBBBDB3EEC9518FA8D5CAACF8DC5394C4E7395959897E20F74E3DD47BF640F287D3265415CF8902C9696DF77BBFE43549C8158AB934B22C2178F8F4CBD1EB3B737705A83445F12C6F187FBF491BA6C9168A541AD9365AA9BFFF81DFA19675BA6FC6F5050A4F9309055D9B6CF13C0DD521299A3C6420DF6F01F0B4285AD1288DF6144D5014AD54522CCD765A2D8D774CDD06D8514412D1BC2EFC6F2562445F4FA4F696FF1E7191D9DBFBC90A792835F72636BE1BBD5EF7587889248B47E3A4BBBC143FA14CB35C2ABCFE4A452B946B4534921B4DCFF86BBAB2358AE4B5B14547FF94AA46A31A0BCE8A50941A52F41A124F74C13F52A9D8926DAC45CFBC696DF9BA6D179BA783D7D0F7ADB69AB3EEDC016F8AA1BF659EC4DDFC57E1298BFDE1C7BB983059419853254B1304E0FC0F0C84BED585FCF5F1177029F31D20FB47270D6DA45CCBD4AE0D4699C120334AB19BAD56811D8DD048C3D802065FAA1D602E725F450F63149ABE52CAC0407A771F38231691B8056F01840471BF4AB000B21103CB9AC71FD11DE4A23E4E2D719A38E6906ED85B12C7A1B179354609C96ECBB576B5B4D2202BE1B993CECEC0FA646F43CDD477BF305FBBAFCEDA82BCDA5C5554CF165BD30BCD742723397D576BA94C51AD325C711038A5C686339F0BC3A1477A8F6E376941CE4922C77325674D5BBBAFC141FC4388BF93B98E39D8D6A40C98475EAB54EAB8E3B06C0A3A3085F3329647F938601C02C68E03EEDC421DDFEFF810F57A07E43180FA13D6DDA0DCF7C694AEF37827786BEDD4E38F841B8F5DD9DC3AEF7072C3AD8BFAB05DC988AB679FB8F87CE6FA071FCB79673EB4B1FED133ADA659123981939B276DC2F5FB9D1A8E340C6ADBBFF3362EE17F4911CC1CC3161FC0881F612E604137DE1BC25E07FF23EC9D3BDACB277B10E718C388FF64AE627BD8E5C9C9F3E7F37BDDEEE86CADBA63EFE97B7ED31EEDC15778A4A26A423E6B1684F35DBF40CFF5307F8EDA39B55A7308B594B574C3D071A0D14E0CFFE0AAFCBB2325B923EC1BB782E5386117B7B9805DF8606AD73B4B3A41715D01AB2431882F42E0F77B315811D92C61F1B57EC0F5FA902F51128152805097670055E05498C02B9125E2B7E1FDF81EF5E17837FDB234FCD8FCF7AA73AEB3B8E95DB4C517F79BE1FD84CBA67B2FB43FB7B5B535FFBCCA339982D634A271C968696C26C516A69ED7EB9DA1A88AB6136D8CCD528E4E6F06DB2DE19DB7AE569B8E13562FA56986FFCC3E1AAF2D2E5DB1AC0BFB9B3F8C7E7AB146F30C61DB145FE1D9399A7EF784D75694B391F6E192C4166C9BCFA9DA450CFBAF000300523811E60D0A656E6473747265616D0D656E646F626A0D33322030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E6774682033393132392F4C656E67746831203130353133363E3E73747265616D0D0A4889EC977F5054D715C7BFF7DDB7CBB204C598417EAD7DBA8238F24326A22B415C611750442182019A1810458C50761415AD98D4514B4D88A96DC78975263A9D76F2874ED7449B266626A6496AB5A671269969EA34692664D24EA36DA68D2615233DF7DD45F7C11296DAD6C9CCFDEE7CE6DC1FE79CFBE3DDF7DE3E30004E3C060EA3B9BD2930B9CD130FACDF05C49C6FDEDC692C78A9EE12F0831AC0DED31258DB7EE4AFC547818D5540ECC4B56D5B5B2AFADEDB025CF808387BAE754DD3EA4DDDFD7994917C30A7951AE24FA5BD4D65EAC7B4D6F6CEAE0335BEFD00CB041E79B0ADA3B9E92FDE13AF014FFF11986B6F6FEA0ACC702679A8DF41FE4660C39A8027ADF77560FB6A20F132C45CEDC5FBE63D7EA2FAE1F18557902CDC80539F6C3F2FEC7B5FFCE45CFFC52FF39D298E2AF28D8506298AB3E306D8EBCEC3FD17AFED71A69899C2947C54B4A4EEC41124A08BF642239B8B46205EA37135307DDC2D6FF616A0FF01933046E9DBD1AB176259A43E5B1C7AC3EBFCCFD6FA48E247D16B4BC6A2AFF2D1A646972B62FE559163ED37685C7FE43EDB3A548D650CBD58E6B175A34AFFCE907DF80D16449CD755DC3596312CE39D42628C1733FFE3F832BA63C61AE3C11EFE5B3444ECFB3EF684D7F93E6B7D24F13AECD10FA16E58BE43B7E2B584AFCE45FD49C3E26D3246BB1E39D65E4EE39E89DCA75F44ED68F30E17FF52E6D1DF47AD9E3E641F3A501231E6DB48B58C7914BBA21D4FCF47AA7D6FE46BCFDF44DCB0B67D28B2D4FF89B268C7BA39660576F07A9447EA8BE946798C03E5F671B29F7C2B2CB131581CCD185A0F92EDBF42726C2C92F5DF85953B901C4DBCFD17D1F959622AE909FCA3E163885CB6C45B6D312B914C673F7568FCD0B586DA760C96D9E7583FDA1CC86747A476FE7B4B9E883EF69DD8113EDEB0B95446BE6623FA87E5D2DEB5E6E579919FD1B677ACEDDAAFADEF155B0226E93F1FFD5D237C6C5E4C8AD94B648EEE2F7C68BE5B47F31B143F8B44DB55B823F69D43E2B0B61CF847CAA5BD8239DA1534683E78C816686750C02E204D3B8424AD1F0D6C9B7C476AA7A9BC190D7A1BF9FE8BB8827922CEEC03D50B50C8AE511CC5683FA573980AC3ECFB192651DFEC68D7F675159D6BB00FEFF42C94949494A4B41FE3E311FBD6E12D4B3D41BEA7B47C1CD6DCE8FE6FCE8307A0F105F27D60197323EE13F0EB035704A3E5D176603DD134ACBD1EED44F3687E51CD7567F4EFE1FF85F875A409ABCF458AFE2C966AEFD3FFB57F6019FF14257A232687FBEA5E549A761CE6113F24EA885A22C5E2370EF34DDB893AED18D2F86154F3DDA8E46F639A65EC775060DA673097D84A5411CB896C8BDF33F098F986CECF17F5FC2CE30ECE8FE762BE66834B3B81222D48FF472E5BFFE350BD70E87EB133A824CA6E27563B895C8A4DD70298A575C2AD6D428225F671B9AEA8FDDE1DC8193A969292D2FF4FFAAB68B9DD1C7C2292B8135D3C11357C3DBAB41788FD54DF8E5AD1CF82E81AF4D5D6A24B3F487DA2FF086AB5CFA4E5F114D343DF5541F96C1039F523C8BCDDB9292929292929292929292929297DBD24BE074DEB94DF9283DF99667994EF4CD3E788F431BF37C5B766E83B537D632A292929292929292929292929292929DD39B1A7EFF40C949494949494949494949494949494949494ACD202C8236A082FE123A60FEDBFED315AE021EA89326231913BA47FDEC8D103CFDEEEF84A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A4A9135F0D29D9E819292D298A4139C980626AA9A9D6ACCAC7338A9EB3A74F621D56BE0850DF750291E6ECCC06C785080322CA29E07B00E016CC1361CC6313C8F17599E2BCB95E3CA73DDEBCA7715B80A5D5E57B1116B2418130D9F11303A8D2E6397D1633C997EBE5F1F1830E7110F0399C832B32EA4AC4BB0024D588F0DD86A667DEE66D65966560F652D1A96B59BB2F652565056367085E6BF827899BBE8E9F4318DE21A5CF640B3F6062FE545C8FFE4BB437F97A7F7ADEA6BEAFBA86F8FF8017DBBFB76FF693E05993B842A7337841A8947882EF4603F0E8D61D73B4376BBD87DBE981FE02FF05ABE81D7F1367E895FE67FE37FE79FD2B59980BB91843464603A8AE0831F15B4DBF568C04AAC661A1BCF12580A9BCC3259156B600FB136D6C136B1CDAC9B7D8F3DC19E6207D949769ABDCACEB0F33AD775F6816ED3ED7A8CEED063D94C96C3CA988755C0CEAE9AB3B91A5AE12D3168F4330FC6282B9291A1B50C36F21DFC51D30E5F9BECFF8C78C092C7BA6620D76C1DB6766ADB787317C3A7316C4FA82D6C57A816B62F54A39D196565775A3CAC1861C57AA6B4E26A469F94B5E02E33E3855043557827C4FD2811D7DD0821663245C217939D2AE107C8BA25E6B517339A21AE3ED92C09AF259B2DE11BC8E648781DE43526781BD959C46C2A5F22EB918853424F0513F3B42C0C219E5DC544093181F0854822FC21D2885242EC4D06B128841873498822A292581A8A5F16A222B42F02316E35713F514F2C0FB112F2692010A77245C87723D12461620F5711CD541E1FF223580AD9351236996C8B8489FD5B2B6162EC56096B20BB4E22CE353D1D4D98D8B710AC836C3BF12D2A6F22DB2161DD64031271176083843D119A2BC19E827C3211EC20D94D127692EC66620B954F93DD2A6167C86E939877D1E110E29C1C9398D7FA39E2792A7F41F64509FF9C62F242D09A5D591207F9B972240E1AD745E7C1453E0E3A57AE7B250EFA9FE9CA97385E264B67C44567C3F10AD94289E335B2745D5D5E2ABF41B658E2384B473956124FB98D048998A7315122EE32C3271177971190707A4A1A9D127E8D6C9784DF204BFB6BD079D769EDC6A312F601D9C7885DD46E23DB23D1E9BE327A8927A97C37907E5E62A339F543C266926512467BD1AF49C43DDECF258CD6DDAF4B5885B7F4E1950F3DF8CD86FABADA9AE5F757572D5B5AB9A462F1A2F2B252BFAFA478A17741D1FCC2FB0AE679E6CEC9CFCDC9CECACC489FE69EFA8DA47B26248C8F8F73C63A62EC369D6B0C597E7769A311CC680CEA19EEF2F26C517737514353584363D0A0A652AB4FD06834DD0CABA7973C5B86787AA5A7F7A6274B300A51989D65F8DD46F04D9FDBF8256BA8AEA372AFCF5D6F042F9BE54AB3AC67989578AA4C994211863FA9D5670459A3E10F966E6EDDEB6FF451BEE371CE1277C91A6776168E3BE3A81847A560A63B709C651631B3A065FA0B8E6B70C48B61833CDDDFB43A58555DE7F7A54E99526FB6A1C4CC15B49704FFCD779DC04751DD71009FDD49C8E6D2489325B884CC326C286C423882844359926C48484572606700DBDD6C4243ED415B6A956A1B150D6EF0E23E94FB3E270FD4C48B80788B27DE172A9E80A08247AFF4F7E6B79B0FF0B1CD67FFF9EEFBBFF7FEEFCDB1B349925D4B9B29F7ACB46A6DF99DD1F9ED194A7DC89FD6A03784A71B961AC6A4A81A8C465BAC8BFCD640BDCC1A38E768360EB9D1CAD7CB82965F47B1AA9AEE051C56A22F43D7A267146C5E3F71FCDC4C3896E9E1CB38A3C8B7F210BB4F13FAE3EF15EC0D3BC4F179BD722FADED01A51E0DABB9DA605B53EA3D420914FA4DCB19923D9DF19EAC29B2A739DED33D3DA47BE5A50A8662AF6B9AB2ADE67AAD201F67DF7EF9F042BF66A979A1FA489334DC18D5CBCA78DEEA0C2B5086378170EC58836D430A313E1CC241CC94A7A1DAB00AF55956A65EC2014868F21ACCAC35EC29B1695666A9A58422B1595661B04CEE4B0B464365DCA0ACA5571B1DCAF0AE236D459A67CF707C7D98721F96BB1417252F18351A6658B9214F03EECF199AE1F15A0113A7CFD48D46535E253DC31A7804CB79ED15ED5938B6F346C707CB234FF2B934C3E9514D79B590D0CAF14B2F198B8E0C5C2EBB29AF68C958CD707894F830AC121B21DF9D53070DD5575A21BB5439B5B4C2E335BDFCF93F5BF2C4F694E8B35C67D5CA40A27B4F5CE77F6E8DA3E586066AC1C6B2B336784ED1C4D80663D57E7C9F4E792E620B63864B5ECE8A7897EAC327173927CAD8297915B3354B99AC197AA36EEAB88702930D796CF25CDBD7B7AA56AFAA9E6AD8573B7697D49DD3627F315B96E24577BCE12CC53D58EEF7C42FABDD9E60B7BB9B15E77557C6BBB5A84BAFAA8DCAE27AACA0A2E1138483EE9157196E2DEE59848F66399E6E7A7958C77F20E5D1707B57737DB42D1088CE0A869A46CB1A7A654354AF35C67AECBDD6183778E6C8A57A2A558EAABA92827C3C7B4ADA74C7BCEAB680635EED54A3437E17CDAB3384D3E12C0D95986DFDD16774688A12B0B34E999549D9D0644356AA41C3658FF774E0CBAED9EE4DB013763BD2EE50EC9C2B9E73289176277319F19C13B904E602764EFEE0226537E114E3711BD41AE4E5B9DE6C8A864CF9E152DCB89478392C877E996239F5CBDA1CCE1E69568ADE5862A5EA25323F4EE6C731DF43E693706338DC0E9C1CF94C8A86743CA77043198AC7C15B519525B5F6AEAE3AC37BC873C2F4E2569B8E986A58C97E3CFB137D13316E828C10D213ACE64858EE439962C8B949BECA8889DB365E10432AAD6454488E55C088727B8EBC1D3129826B830B68CF6F46C36A362DD32F1735669AF6ED9C612915FA685C76D64CCC930B159AD19EFA30FBB3898F428AAF45928CBD29B506331E34B198C9939494869D47747445421ACE768212A9C5ADCE67698A8799463C1213F21AED48F1C43A157958AA2F353DC54A1E8C8278C9F7A983E54732D197649ADCBCDD6A890DC0DA19562A769477D6A98C4DC0D94157A5DC0B5E2DD8AA1CBA5F96A96E576AF46BF164919BB62B25A1DB4AF75586F1F0E7FC5464F4E2F864977C46A4C66A1C6436491E791ACEBBEAAB6BEFDAAC5FE73DEBA7205F975F0EF2C6543CF83330A098D1F313D6347F41BEEBFC6CBA9D8E465DE93F3E81E7CB95DE2D928A48568F8DCF514B71CB8F51C7E377AB3A44598970E20FB542A501311B711891A016A883F0DF46AE9A1FD3AF0E12C5B9FDF7A1B901B117A1767522A90F28EFB0DFF4D1CAC747D4B14AB13A4699A28E86A360311C092F812360111C0E75D80F7AA1A64C51FCAAFC285E2D7FAB97B20FAD31C8F557872A7508A7FDAE28D63A8D485032D5014A19E22842C5AE07600C33B31173118B108711A7112E6CBD1F2A16614507E66A18AD61B4868A1A666898A1293D9C3F88BE39B9EDCEEF455F3FF84EF4CD07DF9233E434FBBE61EB6BF21539454E922F39F20439CEE431F205F99C7C463E259F908FC951D137197CC4D687E40391D3131C1139BDC1FB22A710BC47DE25EF90B739E42DB6DE246F90D7C96BE4557298BC425E262F9117C90BE4796EE210798E3C4B9EE1B24F73E453E449F204799C1C248F9103643FE924FB58F351F208930F9387C883A483B49307C8FDE43EB297EC2182B4893EC38045768B3EC3C12EB293EC20DBC936D16728D84AB670DE66B2896C241BC87AB28ED3D7923564355945EE25F7B0F44AB282D3979365642959421673DE22B2902C207793BBC89DE40E96BE9DD3E793561225B791799CD0426E25B790B9E4667293F014811B4933F93BF91BB9815C4FFE4AE690EBC8B5E42FE41AF267329BFC89FC91FC81CC22BF17178F00BF23BF25BF2157935F9399A489FC8ACC208DA48144483D099310F925F905B98A4C27D3C854628ADE2381417E4EAE2453481DA92535A49A4C26579049E472F233524526924A5241269072122465A4949490F12440C691CBC8A5642C1943469351227B14282623C92564042922C3C93032940CB1511D227B305A854C0E2605249FF8C9203290FC940C2079C4277A8D01FD892E7AC91BBA9FE8351A7899D4482EE94B72481FE2211793DE249BF4226E92C51532B9C24F98EC492E2219E44272014927692495A49064D6749124267B9044924054E2240EA2D838BAC87FC8BFC9BFC83FC93FC80FE47BF29DBDACE35BFB881C67983C4DBE215F93AFC82972927C494E90E3E418F9827C4E3E239F72BD4F845B071F93A3C28D1BCCF111F950B88BC107E488709782F785BB0CBC47DE25EF087710BC2DDCE5E02DF2267983A55F27AFB1D8AB2C7698BC425E66B19738EF45F202799E1C22CF916739EF19967E9A3CC5CD3F499EE07A8F0B770938C8098F71A103DCF57E16EB24FBC8A3E411F23079883CC8D21D2CDDCED20FB0F4FDE43EB2970BED2182B471598BEC26BB587A27D941B6936D64ABC8C273D7B145648D079BC926917539D828B226810D22EB0AB05E64D58075222B00D672C81A0E59CD21AB38E45EF6DDC3912BD95AC191CBC9324E584A9688ACC96031A72F220BC9026EE96E8EBC8B23EF247788AC6A703B47CE27AD242A320D709BC834C13C91391DB488CCABC0AD227322B845644E0373D9773347DEC421370676C3531706734F5E50917B246D52EE01C47E4427625FEA95B902D186B010BB11BB103B113B10DB11DB105B115B109B119B101B111B10EB11EB106B116B10AB11AB529A72572096239621962296201623162116221620FE4B7EB9C045556771FCFCEF9DE131CC8341104160D0D474D94674D51EB6CB48DB38850928D7072A90E12BD3A119AE0F14215D57F7B329B6B6EE9625ACEBD6E664E06886A968A669F9A2D22C1FF948531313F391A1CCEDFCEF41667CE07E3E7DB6CFF6F9EC3DFCEEF77F1EF73273CE7F2EC30BA805A1632D65A8F9A879A8E7517D4285EB42030C028B700D39162CACC4DB8A7F1C677823F8D62A24B8BD66BEB55C84670905042761226102E119C278C2D3848709BDBDE11C0F111E243C40B89FD08BD093D083F01B4277AF89EFD36E84644204C14C082798084682C18B43A9667A421841470825841082BD063EEA20DB30E4B7A873A83AD459D437A83338CE23A82F5187518750075107505FE0583E47ED47D5A036A0D6A3D6A1DE45BD8AA3780555CD4AA9D3455E33DFF253A93953089309930832E111422AF5A10FC1464821FC8EF05B7ACB518448422B8EB5A2280A5E9B65598D28E03F77026C418922D06B99461848531F40AF2C9390414827F4273C41E84748233C4E788CE020F425D8098F127E4F684F68472F3E9160212410E2097184B68458420CA10DBDCD68426BDB626423EA3AEA1AAA01F5030EF82AEA7BD415D465D425D4459CEA77A80BA853A8AF51275127505FA18EA38EE17477A176A276A03E427D88DA8EDA86FA00B515B505F53EAA1AF50E4E7C0DEA6DD46AD42AD4623E7DA1917A5C4C984E18E735E3572136963086DA329A308A904F788A3092F024218F904BC8218C200C270C2364138612861006130611244216A12BC14AADBE8FF06B4212E157842E84CE847B099D081D69361D08F710B4040D41240804469F48B02D452A281FEA3436F633D43ED45ED4A7A84F501FA36A517B50BBB1D16B51B3C58E963F8856CB2C66B5CC74944ACF794AA51247B134C3532C8515F72E4E2B16C38ADB22A6157B8A0F16074D771449D33C4592A628B248D04D754C96A678264B6193997E924396B2E413F225598C94B3E47CB9507E51DE8B81E065F26A798B2C562B9B6C11F203BDEDA5F2025988C4BC003233F1703B39CC682F74B824B7C725695C3D5C42EF4B2E76D4C5846417CB70E5B904AC5AE5EAD0D9CEAB7BBA5AC7DAC35DC92E9B4B7CD6E1940A3C4E29DDE9749638CB9D1B9DDA12679953A8C4956073861AEC131D13A4231318AC171408476D1214AFA873AE137CC0E0BCE0B3296C3C36E0696CC438EB1869AC678C34DA9A2F8DF2E44B96FC947CE129EB48E9496B9E946B1D21E5784648C3ADD9D2304FB664C94EC916865A874883F1CA41D62C49F2644903AD99D2004FA6946EED2FF5C7F813D634A99F274D7ADCEA901EF338A40C07EB6BB54B8F8ABD2CF8B70412F0A720A134A13E411396175F102F14C41F8DAF8F170BE2EAE38492B6CC145B125B162B9AF024D029C6125316531E5319A335A90B515F10511A2114984BCD42B2D966AE351F356BC05C61164C65A67253A5494C37E59ACE9B1493A6D2C42A8D1B8D7B8C62BA31D7E8348A2623F7455B66B8CD68ED6637192C86AE06F1E1AE861443BA412C33309BC1DADD6E3374B8D79EA24FD7E7EAC5723DB3E93B75B19FD7293AC1A6C3C4F9502554504219882C913160E1083104A7B49A4559ECE2060C016881B105909594561DAC0C48AB0AC91856C5E656751CC8CFB6CCECAAA0B95520650F1BB292B1F9435732E191ACAAC8B4CC6CF267CF9B07F1A96955F1038778C58A8AF8D4A16955A57C6DB3A96B85AF014B8626E5B865B7BB30C99D8427548E1B238532FEA8607846CA853C53E8062C490A38DC37036BDC1CB21A72CBB932DE05131876AB61EEE5A82549BF84C3FD9F4B7EBE83FD2F7FF9FFF701B895F9BE76076E44BE19709FBADBE4E6003F829700F81682FF780EED15F0C0DBF02EBC071FC1A77091E9200F66C346F80ABE81EFE01A7E728359148B635DE0BF76F86669278041DC0441100DA03428677C6F2867F001610C882C442F5AD3C91F51229473B7C67C0B7DD5BEDD416110AE5E1B2EECC0683D3BA7340829DC577A715F98C3D7EA15F5C14B7C95BEF29B5E4E01B840862930158A601A14C30C288159F047980373E14FD88B125CFF199E8779301FCA6001BC007F8185F022FC1516C1DFE0EFF012BC0C8BB18FAFC212286FCA717F09DA2235CB334BE1357803DE44FE1396C1BFE075F837FACBB1FB6FC25B18A308F92B305201FFC0E86B18E5553C568956052BC10BAB6035CE8CFC1B5E356C8235F00E722D4E731DAC870D508373DC8493DDACC678E486DF72259DDF872DB0153E806DB01D3EC49DB10376C22ED80D7B7E52666B73847BB5F0317C827B6D2FEC83CF603F7C0107E14B380247E138EEBABADBF29F63C501AC39DC54750CAB4EC219AC3C879554473587D4EC69F50E7BF1DAA3708285C06526C0355070C5A7B7489DD04BEA1CF9F4F87496A97DE6F3A8449F4FE8F5E6D9ACC01EAFC079728FAF5F6E9AC65B58BB123B78A37F77EEDAEEA6E950BFD7630DEF05CFEC6AEAC5B6A649F0FBD4345FBB43CD79D5EB3637DFD5DF517A87FB02BA7328A08727E16BB533D43DCAFABBC72B4E600DEF32BFC7CDBD3D8ED752F7F9B53C1E780DCF1D40FF0C3E1DEAB0D39C67D5499C8553CDEB534DF973F02D9C87CBEAB91E2EE0F3E4225C42FF0A46EAD1BB3D7A6BE47BB4ABF00334E004AF436380D7784BA6117C3863FC8AC1042682CFBFF2475569989605E1332D8485321DD333033332137E6109BE2513D69C31DF96D1DF2117AA4622582B1689CFCB68D686C5B2B6F8DC8C6709CCC2DAB1F601B998E64C2266EE611D58C7A65C6BF5CA98E66B2D58111D50DB8525B3C9784E6256D615D7DD580FD693DDCF1EC4C87DE87747FF21CC25AB4C850C1809CF4083F6B4B013EF1F894F95953FF5A9AD5D0E5150A15C55527D4B1BD78B6B5816DB891D318282939AC86C50A1CD81F1DA02E50A6BAF5CD0F655EA340D4A1DEBA65C029D58218EC6CFC1314D3F988EB75A42C6D25AB43A6146809D22134B6E374D9466FFCDA635DED586FB2DC81254EBB7E0D416AC26B82664706870936DF69B2EF1AE36332CEEAE36B345F3E94705D8723243E81D6C8861EF0D3376572DE7161BD76C878D874D110136D774D16FE17D5AB0C5E18BCD3AF31CB288F000CB68B28A3B5A7DAB4901B6FD8645B6238BD2B7682B5AA7FA2DBA55F4C536B56D6A63C693C5F6BA836D6CBB286E757C5E42A2A5B3E5BDDB2D71694BD6CEFD23BBF51EDBC47D0700FCF73B9FEDF8CE67C76FC7761CECF323899338B613274E20764842094948E3BC1CE74142CA800E09FA1A8B56364D5AA8BAC1CA2B080AB4552914264A49C2C36D29EDDA886E436C5D953251A065D526A13223B68E3FA0B5B3DFDDD9218156ED5A3AA9DAE9A3DCFDCE8EEF7EDFEF7DBFBF3BEB7A5A9172D9B69961DFE0D89CE62C98769095B8536EDE0C0759576FCB3B94EF9DE5852FE6AA63BDCD2918B9AD703BA7A820E562D1953BB99714CB3D228FF22E7FF22EFA42C77D2B67389756B27C9673A5C1D2DD7ECCBFCC7FBAECE7651F965B905F95C702E5815F07FE52518D6CAB242A072B4FCE55CDDD8BDC9CD7F83FB766DE361EEF1EF878A62A77CA32E4A5AADF7D4D3783F9D39A8391E0BAE0F05DCE8634D37A43FB598743C7BF5CB5F05B9AAC9E9CBF65FEE51ABA66776D6DED7E4E5D31F2D3BA0F16342C88DFD7CC5958836C5E38595F5F7F63514FCAB545D71AAA1A76343CD770A0519AB219B9DC646DDAB3B866F18194238B8F34B7345FBBFF4CCBC5F0AE94BDC8A1F0B1F0A9F03BE17753CE4FFB387CB5D5DFFAE3D68FDA56B7BDCB696F9CB6B5FD4287A36365C799CED59DEF71224B911F461E8DAC8BAC8F3C95B203F903EFFBADEBE257FA5BD73FA291687F7445F46074147985F5DBE8EFA37F8E9E8FFEB5BBA5BBAB7BB2FB62F7C59EE29E721E8FC7E3F1FE6F6D476EF5DCEA0DF66EEADDD4978114B2BAFB5E5F62459621A3FDB6FE08F23E8FC7E3F1783C1E8FC7E3F1783C1E8FD7FFFE8076203CB06FA972694FCAEB5F293918185C3FF8E903F35276A6C458933C1E8FC7E3F1783C1E8FC7E3F1FE0B7FE7F178DF1700002100C9470417843220006210004D6031683B0928B807E840053C73B4B636A3507C0A1D62600E3C033200847B422A1CA38CC6205D2ADA206851D407C51BB036104C7C78E934DA9C5506DC67A1FB52FC5C3C33715A1170C727E3C51EA8B028D83FB50C138B4522DA5A84953A1D7E9FCF5B8595963868AB0C633F2BF19755097C5E332650A73FA9C2986328B8F079B3A02E61C3862C95AD1E2174D97539AA8C0C418E99B2FBE6C81B9A687FAE4188678804C20CB1D33F9F6E5FBBC8FA4742EF34653BF504DA679BD03EF1965076EB5F42D9679D78ED6727B12B8148954D344491985092B127D7ACB1794CF31A283925941975069338432123F2170E24761AEC3A82D0D90D263B732E7BA212654437750B7F5BA80656E00097462156D31E7905D8A6AE1C25E5B0918E4D5D09653323BB94A2F514D04299D64112B49500380D15B4C31E83F92173880452A81448A5CE6C1B4D9B094A0B68AB5EACCC0E2BDB85ED401F0C0695BA40B9C2A740895DD2D7EB33C4BD30CBDDD7AB3FEBF5AD7B626202EA27FA7AB961B107B85CC6D97338C60CBEC5B58A3D2E57975DABE5EE99536011CB04B4D5E1F09741EE46E9C4B4C0828F4A45DA728F2F6096E29D494318A7B24B5D45256A91143E25CAA4AB7C950B9C0AD15BF0045CBDD496AF110A249914C4133215898B74F934FEB842430A04A456753AF101AAC58D00E07E549566E002E5604F3AB739D8D6630652A321410CDB3D56E0F0C5B0A131D2E08C41C1B8C723B6C55271DB62D01E9264B694E899A39218CC1B0B89DB507C86B82B1877A1E0E201E88E7BDD71549FCA00AA4FE3E8373B4BB1A70B95344E5BAC8E524589DF674109D130356E16C092228CA6154C81AB6E0F71BFA3A677CDCF16270F580A0B2DB06EEDBE87E6EA8B6A5C65BD75B9C943FAE2FA79C35B03B585DA1A734574E1EE53650D6539F017756B3AAA7255CE027C458133B7E5F136776B6D4926E16D7E107EE4ACCAD3268F18DDC1C4CDC2FB8A0DC94DBAC21A0020689EBA8A4B8534EAE927B9EC8D9980EB14F60E90013D1C0016E04845E988C1FE31552B1E83D113A5C56CA8C531B8742C24E960424DB826E3416683F235890ACC78F21BFE1E65CAAE96718D5FA2F4FB51E98834A91E67BA5FA336634C829892C2A50211A10D763F563B7C6EFBFD91672E0DFB1F68AF351222014EC824F2A2FA650B9A86DA0BDC9D3F695AF0837A37454833F0892C3A4BA9B359B4E1BDFF7E7E1F0487A3CA6C8751697298CCF90629EDA2838FED5FF1F08BAB4A2DB97332F42E80FA97A9B237519529410E58CD65E90DA0C276A185D1806D0112A04FC5A88FC1A29044D66264C333C660DB584878BB1220B7C6A1C6FB9A3FE0AA069B5535C21935F266EFE19B879267D80A697CE99FFB3A92D75D4B4686869F5CB56DD0833D3D9678AE812B8696673FD9DBF3CCA3D59F6F2A7FE800BAEB2822C10614510138CCC5C39435B6252497A8E6A8E6A0880C7A0A4DC8F02ACC636EE0710A36391CA2AC74C567B1D3A65A9CECB45143148D8544B32ADEC5448B5A26E07667326B83F1F83D3823571AD85D4D445B14770C5170845C92F81193196CBD44460885A820925EF88444CE8CE592E4107C8F192F478B3EC92589C8729AD1D24F2627481D7A18387444722BA977325DB271EA966010E5CB094EA4F22556C5B06D212D950DCCD9E25C396C12EBA5146C14679268F82AEC04AAA9EBC7D158A5CA12C5A62E8FA3FF10B1C1CA60A32806BB8F86AC2D59EC528A224CC5E7627236A108B0090B29EEDD69A7EB68669ED2CFCD7426518024CA5117DC2891914276FC8834C7EB74F8CC14CAE200F329FEBC394F2F4DBE40E873CDE65C0399349399A4488436F8488193CCCA47B9AA9FFA04DF25B4812038CFE56ADC6492EB51758D01A7FC356C272861CA9F99B91ECD7C9C62F7D7C7A5CC1E3A8F5AAD0177D56BD08DDE398854711028B09024D0AA668B431D834BC642EE8E5471306B06F318E2D287D69E383A4837D97772957436672D48FE32057ADAB12F216C8E15CC6A7FFBB5044729915012AAA27F38D2B7635545E58323D1820EFB0DA59A294C782C334B4568AAFB97AF2CDD75E337D1FE233777B6FD7279AD518AD765E76711B67C5BF5DA1797AD3EF870855A0D0B0AFD26878E24B539EA44C25C6830A989AE839F3EFD6C62B44F6771987C5CBDC211F4C6A10179E96722C0B61D0B119961EEF90DDD0666251A4F1FA75B8B2B086E75D5C011CACCDD7F2AC7EB707ACD948DC8244422B4C14FA747A9D5A40D5DCD00EAD357D3A0E62081441ED6B0D9D4C460EF8C8686EEB3CCD5435FF6FDEC464FCD866BEC36D4BC44E2654B2165F6B15382FF61BDEA63DB38CBF87B5F7672778E7DE7F8CE767C766C9F7D3E3B39FBECC4CEE5CB4E9B96448BD37CF48BB669D72EEDBAB563EBDAAD685DD7AD83B2D10D28110DDD56D03A56C6C780A4696BE8364135F1D7262688C48440A52AFF742CA24208A4267578EFFC912C03C4608AF4DEEB8BEFFC3ECFF37B7ECFEF37096F109FF3CA0D346CE3C9CAB1E6FF42B9E452264C0760E77682DF96CE96A32C89041F8F938AD3E92EA0E333A24AD324DC5C06627AD84553CE2B4833C80165F1D68C2D880EA81036B9467DC7DBF4D5525A798831C5E48B0CFB365435912E8A203E62500D259330C8D9B92493B2E90BA375C55329260583BEF8A9FEC847EA164474C905C51712FC48771BEA0B49E93ACCC8A4E90025244262C243A3C56771D6970804123E162B9E46296F1CDE17A874F30F955589461A71E248C0E293DB42530D926B59F985F93F59181223F4CEF7CCDFA8DE7F2A95B606B5E8C21D0C89B68BD63AF854993FF102C1822E70A154854B929554ACD6FA02DA32ED5592F03203BC6D23B29E07D61A4607E48812A06DFA8EA64CD60272F432E4679DDB14B8AF42056643D3A038D362B039B55829E730E371A694ECE9FFFF95950C97121B0E4B418E737C3CBD762FC6A7C2CBE08A176C0D21FB43C1542CE22ABEE569E7511CA71A1431A8B8C94CE4F9708B2CDA17B85824CC2218467B1431A0B8C86DBCE8A4EA42D9243A963EDAD1F7D5813B5BC912C192F8C978DCE26D958A526C747428B2F69B6BD01DA48D26081AF6200A86166F122E2204EC704655354A3D7A156A142F5C49E05A9AB4DB60E38D069D25E5AF371EB1F15F6894FFF281656C58B16086445926D508D7D0B76E9E99BC7EFA2E787D61E2FA64BEF86163FEC9BB771E1FF2370E3CB953BFA2A75F2E4E8DAD3B77FB0767E77FB27DF0DC3F2EEDF9EEE19EFEC75ED97AFFF73E9FED7BFC555D87411461B0973D40064F94E7B068BA824E000608E82F72B580091987842E2676C164A28385AAC141623339C7305D198D0695EB6829AB934FF45C25E4E0CA198A2F176758EFF1379FDC5F66505A8D20AA327AE8F0FAA6E25C626D5E7EE8D1EC86B407FBE203AF1DEC2CDE53ED9FE7E27133DFBDE3D8AEDECD51AAD81FE8DA60D4962326606D25D0014E96672AE9672305F4EA34F0C0E6B93AC3FA494B73E5D4CD7AC5287E3484678CB03246CD2CE59ACDBE6B082AAD322C357D0E7CF2C761128872F892C9B4BCFC255F43202B513061A6EB6AFCFB1E7F2AA33C3D5441C3A96B67D6F14D39B9FBEE1E89238B0FAFC4C511B1C9691657EFCC3A7CF973F3AF9F9DFFF1F6C197FFFEDAA633C7F7CBE9368FC59142DFDF7DFE704FDF63AF6CD9F77D1D29E7CB48C943A4A4412F78B194B1199BC2C8E415F497B02332E88BD3729629A013D31EC55689DB067DDC855C8EEFAADCE88256EE52CE3FCC57B8B70200C314CECE1992404FDFD4FFF49265DC2D610AF6313871BC172B7B449EE738A4252C85C31574E56BBCEDC96852A0F1438E889A8B8E548006ADC0BAD4AA86C1A39B147F6E7BA7906A8ED81FB092C5D7DB57D5A79A1F3DD1B6BECD13A0AC2464238646FCEA40CA5DB457F137D924E11895DE7438DFB36F7DB7BD2EA2F52B8BE120369EDBCC12A6E2A906B557E7F3ECE24D28B143A01FFCB432F17BD0C98B62524CD20D05F4A569402BFA84CB001269BEC464E01FD759C948670169CED13D0D843CCA1900E30AC8E6659CA2F36F8C295907DB9CDEA4868F98334CA5F2E9BC7589B5F00A6CF5B4B6B628A6F2E795A6D3843D3770FC47F7AC3EB8B9C34DE1D03AD4A5861EEC4F0CB47A12F95D7B77E5136B1EF9F667956D43DDF56602C5CC168A4AACDD9689E5628EF8BAF1BDE38309E40B7B5EB8B785F305DCAAE28BBA297FC4CF47BBC34D593596E8DA706878ECF931A5CEE9ADAFE3836E21E2A63DFE0647A8458895FE7F10669D862EE40388EA00182DB31F30411772C1C998D84A1A58C304084B8C9544E26FDF795707E97FFAD29243A862D05F696943777D6098A63774D5A50BC2E21B64C95491D8D7741B859F1364173D3F5705929D76C98237EAA2745300CFCEC18E3C0F35A208064B679F062E28BFF233A28B76F1BA97A27216976FC449B06549CA6A59C41577CEBA61F96D7F800B8CE1F28A2FE82D64681E5C1FCDE9F432A593E438931963089BD89D8C681117538B178FD184AB33ADB4782802E94090569C16D271256537D30AE3A03004AFA1190B7E448C3A089CACB72EB8B1EB8C83C64D7C34A8C7105BBC6DAE87317456A64FAE364ED2A03391A02109E7736427CD3B2DA160900E14D06FE4D89C93CE8C444712410A638591AA8283022EBB149A2BAE69ACE6B4CD1A7B562B314ACEFA6F9FACC60C59238855145F357A7BCAAE47BFB4D3F3405C3339A2AB52DA9A084BFC0A7D9B60A5D59976F8C154FC5D2DEAD252F18C87C46E201FE2165FBA39A1F9EAF0BFA13730D2D3126F5239AC76B553B012845570622D0BEFF082CDD8E3F78932476094C3BEE0C7DEB73B2D046E71D62F44B0DFDB780B4170B110F41CD05DDDC6BBF12658F708B8EBA29397E8B0A5800E5FE6C3F00E1586C2FBDC45100E0951A980D872B534CD0ABBD9BDC45EA0870B691232AC912A5D91193830AE093504C332A68E24F9CD7A1260023246DDBD386F0E627E3C46D5701D19B5CD43E13DC53D5D8445688D35AB7633850C9A18B13B2577C86E061A19F41412DA118C3808CC6CB5BC55A883B2D6C44503D8199B9DC411DC4C33F4ABC5016871C009B8DCC2C3C00B62A00D6C980AA7AEA0FB01057CE8D98B6ECAE1A040017D2F57DF44B98F498874ED3DF58F2AFAA08AA8AA190A0BF3B46D3C55406AA6CCF782EC5C5677A96307E6C6349DDF180D16BE244510DD37967C4FBAE4244B13C098A641874E52F6259181DD0A6637EE7E786D71DA2BCB5E6478F7A9F1B423A289F1A18E40F1676C3893F8F244BC25C0241DB1DE8E972EC4DB650E59DDB9BD2FE9AF13C3D8D7C3A277D59E3E698D16A56BA4EC46E488A034DA161CC1787157634AB417FFCA065488FB2D8B7FC64EE21DA015744D3B817405FD0DA00187B4CE340A88102820F74F337BD002C25E8AAB5915559B0AC8BE29F37D207B67766CCE580CA9016BA6C7B62414F0150C6B30B08E5CEC64AD90EADBDAFAC89B27FAF2CFFEFC506CF4336D1E9AA8B1D4D062FB8806A54220D2BFBBBB25DF26D166D2847D474E081EA7B5F79977BEF4CCAFBFD25FC77B3D6A52083BC986C60675CB13035B9E1E955C82AB8693FF497795C046719ED17FAEDD9DD959EFCCAEF7F2ECE53D662FEFE1DDB597F5B998820D06631A9B00A1055AAF7148024A9A94347185CDA50655A421428A88A2A6AD2A50A534F2DA8445505A02546A080ACDD18A284DA5B4122D6A2AB58A1AB5C2E37E33B317762B4B9E9F91C57CDFF7DEF7DEFB6546028A6417A068421E14FA2532E33F8144D0821F4234B263A952D38450C6DAE72815A54A1CC6EAB7511595C6A0D7B5E5F4473F903E5510E83A79EBE406E95FAD434FEF7EECB16D4F6D1271DFE9DB87BBD5611766DF7E61DDB3DBD38B7BDA1E9E81B9CA7C8A43256DA877AE2554C60F5DA0BD66AF19D12D65ACE92D4E84A5D640FAE64B8609D80E7E4E5363CE9350D82DD5CBD2CA5CF1159CF1B75AEA475ED1F0B83CC2C5B7E542F12E389224FC9266B1B5BA269A24E9269D74093B02AFA83D42D0C6A835D356D129046CCC9FE120B404ADB424D1B6A0AC86C7C18D18A8DE8F32731464A9D72E38F57A0139050AA830CFF336B28C75CC7B276CF22ECB4543CDC9EB40F50ACF350F56B82CFB100CC74B3FC2BE02A551945CDA65D69D16C5B49B85E29D3C475CED48488C2D28C8A54A8719F5CF18E2B21888CBD56D5BBA47BAC91E5440EBE7DD6EA3BD8C3F5F4261E315FC75A0713F664214F263F45B0EB83439524C19CB97BA269BCB58CF5CEAD1CA906BF9438EC9507685050FF0B533D7E1AFDBA47C2FD4685545AE729C14285AAFE13323FB8787A7B7A7535B9F1DF40C0897B4306540438B3DE76EB558FD63DBBF1E3FFED1E9D1B157EF1CDBF89D1D3930A3594FC8261338B56366CBD6C3DBDA0C868F194BA0A52560A1C3ADD28823A83558397AE8C4CDD923EF9FDA6476BA9AE31554482BE86D1265E7FC6C199F29D9027A006701D9A245B68C3D51A00381651ADB2F6B50BA4E25D5FE1B10A9E47A7857BBDD5A59772628C3211D046094930C50109ED80BCA09B2A8875189C560AF4853D533719756A3032D1DC79EAB9E2BB56327A0760B325F844D7D6D81E18A4A95981CFE822BEAC14EB01EF5D30697FC694FFD83C45F6983CC0803BDB484ECF0FFBE4C9DC345F47358750D2EDAABFCFD04BEB60A152E223FFEFE423C6E5D95B9823F0FF94A8FCF202B62F08F0B06640D177D7ADE59E46B13CB2B03E316D39F27E5F1D54B0B61FF6368AA29A93A8E616E82F8044C361CC97A79ADF4EE8AD985B5264F4A0C673D0692E60CD23D2CA763B584D217A1E30CD8979256E6BAD2E35F6895F5B4B4112BB1461D49C1B6E8388B60927E2C398D765313525506FF02FAB423E122A81D30416B2802D5F373A4A227A0244A0B9A8A84D4C086DFF81766D3A2E48EA8E579586C2D7C972CE612200E57AA13BE7F9DB1872B13A5AE8122AC42430B6D967808966E7B81F619924C3CEECB82D54F1578E4EB98885BF5844B9C704D7195912AC14621A129DF0B338538F4799A9783501D743F56F17BECC17B5243D481AC63A1AEE98554504C39195CFA805CD5EF8D3B8D84F47B1CDE8A62526012E29BF142C2C3DE21FF68F0C4BA426F84DAEAA469BFFF0E6F8410A8233AEFBF577B5B8AB471BE7C78F13A9E8F76F98D6D91EA9EAD86A976A3C45CABA98C0B252799820710C6D959D4476DB2F0117B1B0953DD308D1FD24A8810C590BF591EF4CA6EAC565B2641D4B12057C7C4DF4452A63FB416DC188E63B43DEAF3C51D7442BC6DF2B458E977826BBC38866318ED88FAFC31073D1E6913A3D8AFD7BDB4DABD6E68D02DE18DCDD06657B3B473F3A921FFE8574703D8AFE4FC2EA77BF0C67150CD63A09A661492BDB1193F0B0BE3C6CF21063940228D937E9937D4A32BBCB1AE78AA3536383A796CF0C4CD23B3378EAF5D0FCFE96BDF1B92FE29F415D76F9CEC1784BE89F51BF6169C78EBF10F4E6DEC39F2DB97676FBFB4A9EFC8BB6746671E49E5764DAF1D3FFA4832B76B46F66DD8D70BC02E17A4AFF6395173097694978B2B211EECD1304F516C507E5A26D806CBF9B096AB64C35EE63236374165C5902856F3D485EE277FFAAD7DCA56665C6C42C4DAC21B0303534321E91FED0973D4B1EF994C4FD88C7FBAEBC55D29E94AE354355A7D76F3BEADB91148CAD2F996443F52E699253F837906511EE5E6692F2F96F1B325240053CE2DF05E9A8DC9CE639DEC901F240B73BDAE7A8FEA3ACA5D806AAE105F7D54C7ACE80910C56AC5960FFC332DC70A47F78F79A3070AB3378ED5E66E0D77F953FBFA384E3A5D03A05701C035E58EB86203C31193A3E72880F09E0CC2AD93EB671EDFE68B66794D061F1E9DD909803C0F80EC04400EA10A227701910CB8ECC04590CB9F2DB473313E0BCD95C46E5E361F678C87E83BDFDD6DCB0334E7E5A550575E41476E332D5F7CF2BF6B54D150825801552D08DBAA1D5710BBCB7ABBE2B1ACB789D8D4E40A26831BAAE0411A1B2BBE38D5D5D231927544833E6E9CD1495779B1A7F3DBFB33FD518B59CB5004C970EC9FC279D1241DAA81F90B31E01B3A30DCB963B08363DCF1DED01DA70BBFE94CF99BA5BF3707B3F2F6AF59BA474401D761B4E5221AC0A7CF8B5931DBE42AE33F2CA1A6D4254C0B78331088CD79F8B1F79531FD79D71A2A366997639CBA3ED0B47C97A9868C0FD5C451DF274D1568B9CB8E6CEDDF9A65A9594344FB0E9CF95AC73746F3669D0627742CC3260777F705BBA2B6F0C0D8B6B1D5E1EEBDDF1F4D6C5D97E6B4144168F5B43EDA3B9A6ACD044C9135E3DBC7072258F7C8771F4E720E97C968F1583D613BE3F4099CA7CDE96B0FB5863383DF1CD8F0F468B4C9E2E09A6C3E474B6BB3CED662E39C21AB2F25FA42E9C13D301101B8B01BB8E0459E394442E098B71A49AE8CE5E685094659C73496BCBE784B4D7E0DE836E42788A8BB796E4936F350BBCBB0A433C888187404AE63C1FEAEAE8ADFBF5643A997B1879C2EC8488C2D04DF3F0ACA15063D8EA1C065E4C5A741B7ACF8CC79462C7245A12E5AFDCB45ABEE7795ED6950DD70DFC1379F7AE28D83BDAC2B1D0C813BBBF39B13894D39A7DE9D122349971E7BFD99571FEFCA4C9E99C5F755DD70F1EC436339C1951B19C68BD577EA7C4827D4E743ED25640367CC2FF86C8CCD52C6A70B8CDEE62A5AA94AD831815B28375EE5BAABDC75D5481982FB40676783DFA5AD568D16BF49197D7D99DE7522474937F49425D79EEA74E9C97FE35F920657B62D9E36EBF431AE992108BDC544BCE28F345304CD19EFFF8D3070663DA9B544FC505F6CE93F9A5D505F0FDA5AA0930C8B7A5229365DC61F2A303DACCD6E08FAFDAC0F602DF07636578C16537E3DD11823C1C06B653B92F9BC093C5C3D9BF28A8C559B20FCC4FF6BE7BFC457496C1BD7197E6F16529C19923324C59DA2385C45525CC44D942C6BB492A2497983A578A1285B92173576AC58B61C230E8AC216D01646003745D2F6901C5AA0450F8D2545A1E3D62962A10B90A3E1060552A4A80F0D501D5A0749EA8854DF0C4959765320758B1603909C0167C8F77DEFFF96C71746EE27D5CE9DB11DC36E15F963FC4724EB12E289013747563F55E0E6AE787BCC42E1EF61BF21184B4720926AA189DF622B386D8D0583627DA82D9931E9B0D39597B57AE6B1E573EA8D0AF605A7A30982D6B2151CDBE01014A4CE2F351B1AEDE53B080B1708DF0406ECCC92923197B1F1259711204D13148C7DC628D3CCC81A6C852BE90FD71157685B3DDAD75F4E56DD5AACCAEA9A56A9DB910AA5EC4AF2D7F8BBA4D2160B24BB9A190D5CAC7E6F2B369FC0FA5C3EF4879BD44CF5028ABBEA269CD4497CED44DD712F9AB73690B80958ECAFABAD3A7400E42F9F2C51AD68F0D2CBA669CA53865DB52ADB210DE07ABD346EEB8CE2ECA5A0F8A93184CDB5A82A96B0BD2851365526BD6D04A3E3E02BD5D3CD5A941428EC0A6B64E504AD63ABF37095D3CC58BD462AE00DE81DAD2E33960DA7AC4843288D49DDA3B7B7384D95B723C82D329B1FE317F0DF896E01BD37203670E09925850199C5E115E0F582AE323624B01C6E800F0CD05066E270230EE3E5CD5F22BC95301F8F87FAFC6568142C1FF110BFCC5FE33181DFC34FF2B89AB7F31843F03C612B6F7E24A81835CCDB8C2C2CD81E86723DE5CD3F0B0A74D2735F600A0430867BCDEB81DEF54000F94F69A2582C968AEB1CFA1C28CEAD17E710426B5241451BD622A8FFBF7F26123DE896BCDF8358DA66FEB144DDF0EB570849ADE4353FD08BA514BFA00BF8DBDBB8D4B5B1CCC278A4E785958571CEDB17E99DCAC7589AA365947578E2B9EE53DF9D0C7E36D9339634657A13074376152B97B3AA4C77BF7BE4D9ECE8B95DAEA4BFD7AFB3F25695D963B0BB6CCE166DDB81C523BFD7B8628E4E21191733C04B9B1F13803C0BFCA007BC52679572246F6193A8C605B02B82023453C98483202368F52B34C2235286BB04A527671966F369A378056583DC92401680510204994500094A3DBB8954AC3EE52310840DA5F736FFB3E4D744A71158E59C5E2FE527103FF6F2E1F6D1CC908B36F95BEC6D268AB145DCEE888DE10707B3BEA96F8DFBAA5F70FE819829124BB6248E26A283ED3AF89785DB8B59CED3D5769456530441A969D249B1B44C46B35455CB47ECAADD8BCBE7D3B3FBA22A3EE9AB7E3098E9D8731C4D72166509077E0F24C00FEAF85981F736360F54C008EDC00E5CF545BBCAD0BEA4CD11EFC02C88A2AD48D3B0100D4AAB0F96E1F092A090565F09DC0DACF7A25731A0AFA1E86BF9F97FFA206927AAB6C74F944264B510226BC44E114AB410526EECCA8D874EBCFE6C6AE0E20F8FF90A0309BD82C4752CE789673B8E9D34C70AB1F8AE4E8F52C1C88937CD4EA3DAE030B3C2E595F9C53B5FDFA932B6E8D546A7A92B8CB6DDABD7B367726EBBC74E59FCA086949C202F82F3E0DCF289D2DE59513A22A9BDC05AC64ACB5E6F49771B2B812694C7E6410904A04DA09FCBC43FEFEA7DD0319D3D700B2D360F327058A00E158015E7F3AABCA90C0B37F09C140CC49656B9BBDE1B135F6ADD468CA81D7FB87B579C49A4ED3539DFDED7A4F5CB1F5D403DA216CA882F87A7F93110F5068FA70E27DE4CFCB47F3177F8728167AC51973B6A65349E4E4FF468B271AAB0D2F19190DEEAA664B88E63F98EA1681DCB5CC285B06C2270B249DF393226212FCC5E19627995C5983CB7742975A8DFCFE1CF08BD3B4E7E7BAAF22155AB7B14ACF4E512B6E181CACF1A57886F62D0E44FDB43DD5EB5DE69E90A9BEDE61A072DEE16DA1C70989D06B5DE6194D8BAFA8B8B69923409C1BE7363519262345C8D21D93A6268017C63796C5438283264F708FAF9DB480F660083F8D183E3D8EAEA593D3A46A95BD8DB28A845B1F9D5D1199AFCDA88B92CB23691717EDE167AE0D89F1D1459EB064938FCD64881CB9379316B3C62A9B7DE946AD91905E94AC77D768BB0AD3E01FF2B0C71B02617E8463984C8CDD7695BCCE78B3B34B2EABD2768B2781ED174F4D4BF411354C8758EA807854D25ABAA3E84218671506A0549285825BC57F53D49955FF03C0D551B1B708AD1488FA2D5BCB6FA41B55D67ABF147DE23E7C04BE0C515B0706A375EC68EAC6453BB5528B096043AD613DB8D8E059DE750199B17A885FC67FBC6FF967B317B5AE4691A94E0F0F2F385186A39F665554FD65A8699A5F6C240195A6F340D8B5386884349A441A0547225EAB818127AF65748EFD73871D4B6604761A48EB9D819F4DB68221AF5C1F3B8297E656EE186C9103FF9FD13D3D74BA1F7445C75DA3BA16E5DAB512397514D04CDB586D22DF933597E5AAB13219FD2BAD36E67A7B7D9E05290988E65F9C860F4093DDBAE7EC2EC55442EBE6AEA0BF6CDED8F840F5D191BA50C3E5B2A5C9D2B8EC8157279B3CB1A8C702A46EED97DF1387C2B9CB2F90C54BC7D28A8D77BD3CEC04E97DA202AE116B53525746CD74C446DA74CF4E15D285DBD4F9E061ED43CBF53F711DA92BE854DA0010B63CF0B94D6314CA7BD1642E56FB8288A302382C2988B4BD21F47672B82AA8086CCD85B0F2662AC9706AC1688144FF9889A7FD446707B924103B665C6F8D6684A84A5F0F729635B4BABCF440FBD7AE4F8B583BED8B1EBA55D9776D092155B9987C9A964341368D6B40DC6CDD158B2956FD8EE546E1F72DA29D18E7BBAE19F1A1E5C890F66A3FB66129DB3FB3BD47CCA27A29643A8ADA24C1A007188D7505BD66A1DC13236B01488A31A8B7073E0416D10B304EF1062FE33286101102C81E5F7109304F606F126811184358C005956C382F82EB4A2EF84EF7B72C64F818A55611CAE521819585018D117147F17AC75770DDC45996FBD1EFF8A7313C5C0FA4411A1DD21B6BEB084F7FFF4A7A5B024733AFEE5C8A0736F5262498EAFB6B92A7FB47417FBFAA747226A246C38463429BB0ECDF72F2C5FECDE79E127B3675F3F1EF9043F5C8A64C2260C3E0C05D3C53E5E6BD0CA350E93DEAE57AB8C066EC7A5772E2FBC7B75B8FFFC1B13ADB32FB87AF687910A99361F62AF2117D901E6EA9CE85960114D3EE27753C822969319B3A7B10551C7B1AF0A916C6B9ECD8ADB4EDC771DA283AFC52A6BB13584E34D407DB57BB6C545692F6ED79E86836C131C0913027B8D68A2500A30F106CB3F58AFB698B6EE3BFC3FBE9C63FB1CDBD8C60603C636BE631B1B9B600C0E7840EC83390E1787E5628CD310B24B92C2D234B7B1AD5A9B6B272D52933E6D5AB4EE692F13A943D95AF5618D54656AFAB055D93469AAF6386948EDD43D2C59D87EFF737C030C0D243A927D388673CCF77DBFEFFB7EAE26E61D396F25EF302D21BBBDD3442FE87452B8346F4F9F9F7025DC78B3FB97C9A6A32819A571F47927054B78DA517276D11F051F18CD5ECF7628D54AA30B746AFCDF3FC8F380C81CBA80CE14DADBF5F6008623AA985E80F7C2845E1F3DB102659B46C3FD51C5F716A625D29350910B2FA5BE9968C580A5921DBDF03FC7E5C36C176767EBB8B3D09797A02EE3EA33C0AF87F0CA97EEFB61BEF9805147C194355889353A3449EE0CA58662786ED1B9C9F3E159E8DC6976D8CE143BB784A1140E8FCFE08FD954B7047FBECD983AD781EA3E7879A6B9BFC7DBA812137263A7C70E116C1FDAC7BA8F091DDD331C32768622E66EE8E8FB7CB53ABAE40A49CA28B1D611F36C4945FAC8F5AC5F42C9E58C5C0E6EBD7DA117D8927D28BD84B4E80EFA45E1D6ADF93B98A57B8BF97CEAF02C3E9BD7CD0F78E915D1D1B83C6549CDC3B18856449797F75C796DF10EFB3320ACF093C4C2EC22664C73853DC77D873BCCEEE3066885571254E14DA7372DC5FC06538E44067E7DC998E4995CE5391C1028C59C128187E1623DE23336CABF6AF81D57A0760311C41684E9772C01EB966A3034409DAA44B49E5AD1788ACB54D7B13D4120EAC95E0FCF6CC4D7A89410B2E64E7779FB3AF6E64177F57E666E2D6AC5E9E6B5A2BEAD50D152BDF6ED52255BBB07678E6AD538D2AF8EFB1246066651A9A6B48DD6FADE10F1D70D448A8C63D96B3CED3225BD89F6D992908ED16A98683908E90609232DD638FB3C539B85B459548347AE653BE06F61C04D9D6ED0E0482629789FE473D0CE4B285B4826DB3276D0CBDD0EC680FB97E2409BB64D8BA2E10E1996C89164861DC32703099B21005A792FCA7AB8168E615145008211DEC7028089AE0C7405FDE7A2CE5AB92CF91CC0D4182DFADE307820A6E05755146C18DC9A148C3AD3AF4E7893465A2D9348BE0A0228249BD935983C7D707F55A35BC0951C01073D8D8E17C2E1C85C3DF6CD23ADAD433406F8B43F026FCBE9E4D09CCE58075B61E1DBA97CC2859D33938C0CE13954A4D9BD9C9F351697908A67E215E421605B724D3EBB1D5B2F89BB76CEE2FE498EF0B0065B69AD03AF889112CA82617A1BD619E606E9D732CCC1AB23D38BE9B6B66D16C3DD9AA4B07322017FD94DD0F56D74B370EDDAECADE3D807173299FEF4419C64B3B767BB783FEC67FA67E158F0623ECC97CE2EDC626F628DBF96387E7001F3A0BEC47E97CB7169B6A129CA39B82026AB29A54924B10D4A4B36089BE19626B8D9026B03ADD935873B981FD1BF6B39DD37AE8E647F906E634C419E59AD23EA0C1EDBC3F0D5D7848996DB7C5BBB5DE819464DA2AEE56E025F4FFFB68D14AEBF009BE333F2316C4B97900E4DA1F1A57DE87DD1EF9002996116A7262C581BDD41DF049BC613184B5A7CC07CA13BA826D42BC4F797A5DDAC9333F263F8D96AAE6EB5C4313467D82A3FADF495526D153F0333ADE43A5E4206BD44ACB4443C9E1EAB5269EDF1782216A56E9B4CE1CE8E19FCEE569A928AE83A52AD6FD1EF09128F9D4EC91F1C61B352690E3BEC218B4A65093DE9DA0E2F2174640A5A65B035055D12520A1668B50B8891A33C628BE8C785F171FF058C53C197F39D82F2F0C1B2C20747D40C53747771DA8FF3423EB4377A812DA2F6AE543A74164FD26C729A3D844F46127E4B14C7C6101BE64A809663036C6C35578255189EBAD54FF9D9D15677C26D31DE066E498D18D9CC0139AAB446DA3D11AB4A658D78DA2356E0A038056B85AFD1FC3E6751F2466B7D14A70B50A40FB82A1499EABB83C413A7EB9929DA42DE2AD5A6DCA9C1A0904121C8A07974AE303C1CE8673F14E5D101A4171D4524B2830B064E06A815D1F2B22600C701DB8AE8BD8271E6D0811E3C05C793EC01BE0FCEB0FBB97EB69D23ED4C2BC78CA004A60DF829655295DBF15EF734B45A763ADED68852A32329823018CACD8E78AE8C12BF82B3845050F59690C309786A546B8F8900CDD0A690DBDD65D5526B8F4A848969196DF779F5EB932AB4B9DAEFED6D2F269574013BD47FBF2466192D4F01AD6ED3ADFD65CD5F6F6268157479799D9278B4E67E8EC82AA6D44598B1B7D1CF7F8B2E8B96EFFD349FEF3B19C37EC47ABD0607BF75F59DEA7BEB7D60EE0D44E3C4325C34F4C1C12A80AE38832639F60D5A7A63E48709331EB233C993ECB7F0C9A1442CC0620299497690DBC339584DB94A94878E27727D5C017DEB032BB05AA9173BE06CA7A35889C30D7A915DC48C7AC2561DB9F6E7D23002A30A87C0E8334792637D2209391815086F12729090C9404E4E47B855A956633931B4B526DBBB4A2565F5D8A68FDCC876E03C6498521ED6549BA012F11730C7E368A86036DB120A5C21C78D362C936838309AD0E1811D48DACAA5DCC84A4BEE5ACEAACF8A43E978EE6628FE62FBC1AA51012B83C557C0B6E7181B8C54B1E1913D303BE7D1CB717926130C98CDB480C9D14020761AAFBB77CFE783F842221E3BC383543891CCB3597CC22582B618DFB3136C375785586536CAB0093D1BC0D3687704E0AE4780EC29572F855C617F113A870A5EBDD9BE4021D7585B7976FE097BE5053487F24BF1146E07CC9CCD86BAE6E698C4A130C2BE66A863F6AF10C9B8229F8EB361B6B7D7E0C7CCB42453880166DE35B024A684E703801F1810B20628B98F19D1F2060521B32DCAC2470245259CD7B36508878A57EB5512E13EC4AF37226BE7CE4DB8466D2A991892962435466B43B3CBA8242685DDC8C4FC5E50FF47FE58BDA5514B910A4A4AEBDA02DD2DDCCB6C9BE8ABCD488E66AF9792DC64D351C082C6D1E79DFC5851474BA580E1C7021D6BB991114A4E517A7B8B2FA855D3A473ECC224F14040987282FE6FA2370BFDFD4DFB9BB1D25DD3D3CA532ADCD39AC69A2EBE8E71D72B4F2B9BE070FD08757A5D17D953EC2BAF749EC0484F25F7B31011A67B91C4EBCD2AD83ECDCB9DAC95ABE7AE022D4B549ADF6D30FEE5863620EC36F7CBBB4D68E372E32883F8CCB5AC16453B638E7262AC232DA380F5DA6FB69E0F7BFAFC8487B5C37C6016298DB10D586C62885107776ED29970617A291AE86DB41A5AF00793B449B85735BD460DD02B935CAAE663FBA98963AE85A9A9E23A99CE5E9FF62B55AA46B7A9C5A6032324EB1CB1F6A9AD2580C4A879ED2DF12FC57F42FD683FCA136889100D4D1D8A8FA98394B8C7960AA73E4A89CD2922F5F7070CD1C810CC830CD19A211A3344E6CB877AA2414F207D9D5EA4D6EB8FF688FF1363DB2DBEC10F06456890187CD8935267893A71F693B8650C350E34ADE6677203ABB91C6CB6ABD8F86672B91CFC987BC4BF691BA2C1CEE6F854F583E914F1F5CFAE3C3A36F8C9A0483248A8B77BFC4CE50BAC7BBEF0050E135809A54EE9749150E80D0DFCCE5BD916224E905C37FF8AE5626880A24F74FD9FF66A8B6D223DA3FFD8E3DBD8CED81E8F1D3B8EEDF832BEC5978CEF89938C49487CCF95DB1242B813D80596C2C2262DD54ACBB26CB548A517A92A2FBDBCF4A50B0917237645D5A2AE2AEDBEACA2AEDA07D407248A544B150F95962A49FF99F138268140B76A1CD9B1637BFEEF9CEF9CEF7C14D7597C0F1131CAE36911D79F897F69D0CC1A88D8BECB93810AA922A2A1BF94CE8D05BACF7C72F6F42F8E84B51D115B209C0838FDC9FD1F8EFBCB1D489B965CF96C34EF4EB975A3C354CA4DF4E4FA17CD36427A682A5D89E8C53391506B6F47E5DD8900D9A27619DADD22B9D83D309DD972763BED6276C53B3249DA681C09F7ECF338F7E72BF3DB8298A273E59BDCA82990B60D8EB4FA93CBDB8311918470DAAD1A3A66A4C2EC465B849DF085F86BD00B73C1DE05DA3A5A154DDF042D2D60A82A3ACDA8BDED603C95A7FB46ADA8335B45061782856215D9CA60CE12F62F3DE122444475F56F7774648E68FD463202FA9797026FD7A0D06B10D97E6D348A846B0F385DA71B234F2AC89085291E6F11372F64C68646C5541D5CFE8D49F117D9777FFB66CFEC445C2B178B50A94AA6EACC1D1AE89E4CB4B9B3D9AD9EC31FEFF246F75FDDEB1D1ECAF99426AFCDE66BC59E250E24BA8603A4CE371833774513F6C0A96B339D4A9D5EAD31DA482B45CA74469D811E4B6F77D036BC72F1FABEB39FBE3FAC7175FBF70AF25B793838DC357630963A3646E38E84874D0D176096FA4A720AD0E038AF9DBB305543F8FC7A981716AD7E93A6BAFAF79B4A1C2969AA48995130C1826BC8C426E756361C986B011D0CCC6C2C80EDBFF05A6F876DBA161962DCCC97AD19A2009A3691E0C1FB4AF02FC295A622FBE34214101EB397F2BBBF5776344638B29C2DC4DB8706963F115E9138A18348D941BC4230FD99A33F38C0F6CBF1D567C8C7920A20410718E06BBF0F0CA2FBC0C26D8418B021F3B7189326CF1FFECF66386D6B5C997737FEEBF99AEA25106CA661B5064586CCAD3F37D137B9ADA777DB64A67172F11C0CBAF09CB08648A93B952FF5A479869039F11FE13989BBF0DC333731EEBAECE8A935872FE1AA1BAFB47681EF0A7FD599BF07998F817D42F55DB06E0750C17B03708A6E2F0683066EAB6A6180C1A19478F396216D834B6E535A32B363B006B7A447ACFD295FF4AE266C9AF7A506D1EBF61BF1BDFA7E0337D6AFD797C22E241D5D7021B1A9E042F26F240417120C5748D62F246B94BF7889107085F593A0AF3E357035892895881243D40051A23094CDDC6230CD105F0B126669DECBDAFE9EB645E1D51712BF09056B07AB9F41AA809E350A7EC39FE1068CE5300A5AAD34BFD4F479EE413668A06952D542B1E01254E66255D6C2640B7D43C1543E58323503CFCAADBF16807D925EAAC134524B73CDFBBF7CD72BB4FB323193BC981B795EA1B244DC54A45DA975C6DDC1A90484894B8D5A47C2159A6A481C33FB6C76BF112BFC6834B9732BADF5968B45CFAEB9A2BD01A7481B5C27F68DAFAC75FD91D1516320E30EF47988CC918FCA0DFF830CD0E0FB7506FC040BB995B34160857EF6CF452552E67C4D25F89A12FA9ADFE4CA3720D2710041A8976A9A06CCFFC5075FCF13C957796203B09F4DBCC2139F030582B10F3A626EF5098A422C08E00127054FD08BCE0200ACF01E83119BEF14531531330ABCE06C659F39AB8865819194B994526FB8BA4FBEE607B8E0B21650D81A25B17AD6AD0F4D14CDCC55E7CF5D3F93EA9DBB337FFEFA77522BCB243DD19F8243D3D035D9979E4C989127A73FFDB0B0E542F59DD39F5D2A642F54DFDB72723CE41B39390C1F83BECA4958E385959FA000D6E88739E1C77C8D0B1D098CA59C0401D1454601482C11EF4025114117912A5264D454A12DAF19497305A4AB48A1A900B80B046048E0C967E341DBED6FF9154D40785E403F2F1F011A99D6C0E53314C0B030EDC9F666EC8D3E30F96C569F09F3142B13E1FD1FEDF0AE3CD3FA066813CC0DD6F84CAC6B6B2789D4CEDDFF2087DB42B69529C192D0874253CC7A7B7DFAF2070BE7D2B3E35D302B7857FE3A90A7C70EF38A11DD830846C189BA62281C3A25A302661CB361614CAC1663B070B6F9B12A32C1604CA040E1A43D4F722D5F371368A448F8415D2BD82BDFDE840C5FFB4BD0918AEEA1723526D79BAC3AD21F841259270D675F2A65515BEDAD4A092A12175D2133C6EE1FAE4CE7F2D246719CA4B3142E96293015E987B5E7579F889EC2DAF3E0B19096B2A2D02D17EDA25570DF1C64E0F4444348E851120E11ECB136C9B00E90B42745E2A436A935E01924030D8569635B20F328DB26F1150C1A951A290103A2410D4F858E80E804D8D26B813D5A76B7DCBB27A0A9ED81BF6C9371DB262B30C6FEFFBDD81AE8E8734937D648BED2E7F6523D84FF69FAE895097A772E6250A17295421960B6251C718FDEDD5B1E2BF7BAE9E94B93FE11A69390A362B14C255750E962C441DB3554DFC8D8481F85584B672A1EDCD84A063BDB9DA4CC6435B798BD666BC06E7174326FF433C74B7E958EC471D2666C73E865642BD96276EA6D7EBBA5A393D9C57324B926791B5C053FBD0F52A2103804A64483200B4E89066EBA7CC4FC453859191237E16F650F65091C27B287D0F27BA03C9FB34163622C67875253C7863C8F43C5C7E32178DB117D441D2BEC783A54BE885711DB822977B98A0CDF509401BB1DD0F02E5AE370D340F96BD97EADC1B0B0F400EA390C7F74696D14DA82E621EB0E1A3EB249F9D6E5D221C4311A6F72BBF588929B3260300A7B1C2B817A8E723AB5FCEAC6D9C53591548E3B42094BE9C4B07356474A94B8E228E149BB9DDD3E83C9A210CB952C0DA5661A362731BEEDCDA4DE819B5AE3B33F3F7CE0EA4CF8F7AC5EF4C41F8219BDDDA4954931393A677190EA169867A991F3E3C89FC2C976AF118B05078306924AD8832973CCF702FE7A37677F6036E796A0FA018A39311E0ABFF1FEF60A66F4B627C32B7BF279994226235DEC4C37AEFE437405BD01BAC10F7995DED16AD53D3EE00CB299CAA80E0AB61C84742E3A73ED6AE10535CBAF31D705F95D6064BC3AA0337FC90DB5E832FD8086FC8639CB0A7E8BEFE0A73CBA81FCCDE8135D51EA9CE1A4A57822E7384EE859533AA66CE7A7BF007BA887875D2995CC75868975A07F0EED5D2281F6FEF906B038AC2476C97970104CDF1ACF66E983514E1D150B4503DA016FEA9D9583B9E9696994AAB085EDCC256161B773E5CE922567A822ED37A4C3AC0EFA392DD0101DD8FBD107AC1C60DB7F49C3CE87C9134286D4CB44F97EA628AEBF9B4B160CBDA3A97A7233B8C44977F99D716AD883CB5154AE90CA9C3E83C56B52FF4A812B25A4EED73024B95D5DEDCA95B79A007A399AE2DF15775FDE1D52E36A93A7FD3F9C577D6C1B671D7EEFBD2FDF9D7D77B6E3F3C7F9EB6CDFD9F125F14762E7A3697249BAD6699B342D2095D1642B305AFA49946E084D080D9060D53604D336BA49AC821595B14D6D1A5A57DDA60A950A698B405BFFE10F243E841012E91FA8A8D2B424BCEF9D9DB85D43BBC9D2F9EEE43BF9799EDFEFF9FD9E58DACF7A38AFBEC9DCE3B0B6DCB5B6617EB0CE60F4FFD10D565731BFD4167A121AC459B446B150879740A346A922AAD111303C5F189111E90B663C6EA249FAC845B2628ED4641317D6A64AAD0DF17D419FE070E45B1A5E44864314CA7FC6B10B718B892D2B81CF42DE3FF25A738B5C39DE4210EFCB6C485026F5F1D97BD3D019D89806A7CA5CD45F91154F23B4226E4373722FAE34C533EA89A20FA8989F0793B5915AADB669AF88B1CF576A3EDC43FAC43E6CB7AC63B7B8C816CB051CC7AE15BA7D8D3A5B2A3B5CAC77D75D3C7CD2709B3C68F7714F97E04F77F5AA3B111F2B4FB5D044D2AC9CEABA3751C477D63B54C4A5681B63C8878C91451DDAD526BA1B5CB55018D514C9236E44E26D5EE619061D6E6FE078ECCBA88B9F033F98FFF28931CCEFCC53595C54A38F8FCA114C74D0F8AEB17BB462288A5119DD4D838333C79E3CF6E441B472C52CFE87DB9EAA9DA88D65233398F883B56D98F8BD1348866DBF199C48EE946BC0E9704C7F199FDACCE34E77869C23428B12ADCBDAA7E8F2072E5FED93A714FB32C50A283785538AEAD80076C75FAC55F9D11615787FE65EAAF23EFDBE9A12CF63DBD16B598923A9FF60A3C00BE4FD9DA243596B9107EC9C75D16D671245E44C8EA730EFDA9EB2E0780A5D6A7A0AF33AEAB243E0C07C62681756DD7DA87C483C343D7D4824D5495C16A32580BF74F573485B4BFCEA446DE750AD54EBEB337701158BAFD728DC6E81C676E3341B121C69BD846DC7D6DB161AABFC59E47A00BF225E6B756C7F6A03435AD7045AF9D4BA9BB5E86C3F6CEB0C6FB5B25EF43D00EB1BDB5DCBE398F96FADFE8B7C9FBC0ECAE0EB8D4C2464AFC019A44A028920F93BC7B3021D1ECFD8D12F532726162C7182B6F30D4E7F7867240A4B37EC3464890FF06B67937038AC30CC9D49C8DB86B7C46AEFDA0DF27D1C08B5F610BFFDD49E7DDF9ED06CDA5028F2E9FD46717FAF6067A4A83BD5047B606A6AF0E0D35F836B37565C5BB757625BC7E0EEE61D845A59FD88BC805077804907F5BCAC25EAF0FB0B018DD1D275386D09C0D272E39A1019171A616E98081722213CBE647C40782FDDF58346E8600991C4B0B20482D60012F4077BFD76CC83E40582A4A9955BB4373B56AD8C195E7AE516C312021AF9EDE5989B7A8F617E4F7AA205432F4478F2555AF42AE2C77FF206DC14ED0EC864B62D2932080545735EF7F26C380C7FE4F67234C54B08577AF523FA4384EB21F062637F8CC67C5D1D1D72BE0EC72C2126F78932450E0CC88375685A1E8B9447C6BBC7E5A220D506EAAB7FBC80BE3BD0B725E29301990CEAE3C19D9C8D6F18F58F699AC39886D08D0862208C0202523424DFB02FFAFB6D522C09BFF21E0F0F3BFC302CD9A087CCAE9FAE478116A65A4EE90F19D74D5AD636974A4369997A11C2939494192A9537A3AB7F73342A0B3D578E0AE479087F497A22055DEF5205729E84AF437714D54841E5C9D34232BECE248C73DCF2DFD6798D69022F7114C5635ADD6E4C2B2659E2978F088D2B8A9370CF282B2F900B88E50CF88AC3F24582E3441051EB70742113E123A13A9CB3244B8C24C6C3BC7F9CDF41ED023B6C1E1A75E41068D711DA8616316DEE7BFE1651A6914E97F4FA0D234B183DD5AAC34DB7DF4E514A1B0BBF77849B9AC8154390FDA62740AF2C7A42FD05B31C15D90FC8AB8CBFA3D7EC575D2BD7C20A2B87BC84C98445B227AD075CA43B1C5CFE35DC1FF1BA5C8A1E46C81E0200DE24AF02139C769059BCA413B224115E066D7A972F26DAD0071875F8CE3CA77B9BC1C16B8FE2704DCADA8D9F4597F316BDC30E10268A99661979C4A213348B25F0C8CCF4CCB46A096BAFFE346F2A96ECC71D13D1905FE3C2E925B486736BB653DB5D884FE14D6C87CB97D518C9496E6272E59A3F4823BF849AD8E6612917F28705E2514E169803B1F6109769EFF2C5D4A81752C54A2C1BE419391A28B525A2517979D9A564010455F8367C8C8E834E94D186E6D9C0409D786B01A4D3A0A74EBC69F9A564520D3C5328F0EA4BB9D9DE17F813E49CBDFCE2A57709C5099B07BCFEF6E34583C1F3A669828DB9E32C1ECD4184D1B0F62682874DB5173E66989D69ED8B9BBB2607B4DCAE277655F8503E99DBDC99E07D8A3C76D8AA1D1849BC5B4995129E5C2A590AC3BF881EB764A47241945F4B5B3B036A2011E07D016F311F0CC79570654FDF332E6FD8178B47A308DDC308DD15C60D0CD00B7AE6F944F10A710EF84196B86C79813FC18B1D6FA566C347C5B9EE73F4894678421640146E2C353139D69EC53BD39DD6EED42D6B0364D880620F4F7845B3F66D8AF7143B944C3198F0BB78C588C674C595FF42F7C8C3FDE1F7B8A01E8D7667E295B8AA8704F2BFB5B93D1D82920E559091B9049694199E21497458F9675A2BEE39BC355E35C349F3A54C269CEF4115DD07DF81113A0A8AA0321F027A9DB868897CE0677F8811B1D46BD22C7906F9DDD54BBE40AD23F72B16415ABE612EA1C3F4129E6AD76C40D4BA184C606D39641C456C503042D2AED4DEFE677FD239F58D317F3EAB2B020349979B777992A558DFD0E060BA6AB8398E22C88A2FEC1502D19F3E37F5C48481C6912478833E311A9298886F626A6A4750F30493A80DC100D2E21546402E5301E5792E5CC15A00D0492C58B2377134CC91B93794D9F29BEE961A6B0CE3B5EA6A99B1360025B83E591B39B63171E12BA94228D1C676EE1FDC32DD1FD1AC4787BB76EA9C62A83143E17E1BAF26D45C48E082D9A8DA97867F77D8AF76164BBB8F0C224D4C4D2302AC2305BBB23D6344F25535DE9B57536613CB49D4353AE80223E7BBD09E766E41F57A55A34EBC6105812A8A1CF5EC39E3AA010D23D4FE7C72963B15B2EB0BF7CDACD338CDE0E840D3ECFF6D630ADC21110C64AB3632169E54D595B352BAAFBD7DA45BE33D1CAF1AD5AD9DA75FCD4FCD6DDF7E784BF26DB2BB47CD454448DE4EC4631D718973F3C174262622DD7E7CAAF6F89499DBB6BF3FD8B7D997C847502D45E1EF88EB4C04544161C1E703A25227CE5BDEBC9172FDBC782C754639933F1E9D138FDB816309EF9D8BDDCBE56BFD6B99C268460AE5AE4461FFFBEADAD64911D721CDD28CE6F30625E64BA2247AA639458FC5F4E0FFF8AEB2D836AE2BFAE6CD0C67E532246786C39DC35DA2B85922C588B4281B92E542912D2F4D9BA4685C1B7580C43065D96E0B03FD2B0AB4752A0770DA7C594563204D8AD6B2552FA90ABBA86B7473802EBF365A14DD3E84B409FCD598ED7D1C52946439A248EA49E0E8DE73CF39F78CB0CB01C7096378AE6A94FDA20DB377BC869D157977482F8407B2B1F6714EE219321CEACD5876209CAF1C1C0D71BCA80410A2918FFA109F61FDA88E66D00BE8CB13129AA3BE8932C84D2D82E74F52DF4625D4A0169B223758E2B8D2209D7C16467515050E11778B2699EFD44E66E62EAAD3179C798EAE5C91EFC85896A3CD0B95F9E716A367D72706EB7EEDE15A6DDC5251377ABBD6C8E3C19A35C0B8A659624AA56D444BBA1EA67BEA0230AA241158AF1671E1C90DA7D6AD7027F60CA7D2D6473B4672C621D52521DE7C71541F70F062C8FFF5917D23FECCEC99D9995726A343E940281936C2C95D2F56433BB455C9F1AF5C460D7BC55C5A8D78C5482AFE05BF325C3607FC22F39B78440E38F3D36583E77945722A98C5BE6C3D919D1A09A9A9915872B75F2E05CD86EE6DE40B7B8703365BE4BBF18C5D0D39E329590DB48F691AC5A84197A18B6E9DE485E7F12FF112B86A111596336E22E32092803A4E145432BA637970DE3CA12FB044C59BFCB46FA760A329E29A7DCA6CB155152F81564341604866D03F12817D1E0AA634B16FA785B1033B34FCD7757614462B09B3FD83DE79A3919A6662E7E11AD1ED3E60FC2078908A6228BA8A34EA03281D51576E88910F0C57AB53F2C3CE90D73615DB2DAE6A6D38A07659A3C2826A51182C1DDE55C19E9F680E15C62786FA55612F2F7234CD89FC7B23D9CC8EE16CA687DF05C06F188D01634BD4BBC804FCAE200DC5A9F760138BD43F9B76A499129B69054F283D24BB4002948FCB1BCA4BD3DBA058B56226E892E33C9A862F0870DB11011B67BFB715CA6FB092960885323E51F67CF52DBBD0AB5EE629A3FDB76DF0DCF1BE20F33486BF88A1E01BED8FFDBAD51315849E54E4B945105D115D273A750398DB414905B7D6D147ADFFBFBA68310A78ED146A2E57E244BEF9BA62916E8AFA71D32B3A9627E6A3CBB5F97A255B6E6517F40D788D776C6BADB00669A5F689F4DB7A2623565542464DEF62C928829A0A124AA6334639D21B7F32EB2FAFF3335E289AC52323D39FF619A542D9A8CF95D4A77374EB19AB0EF81A2DE6CB033E5397128D03B52E5B2E41FF3934B49C50FA6A73A0A0E34A7A3EA1475BBD96DD1D87225AFBE466FBCD11A55D02A50583294B696130E6A0D549215E3C5AA91F2CEB9B3AA842C56F3F5171A7568C6AB01757A1560F6C46731579A9EBB0E1C3905104D1B8E49C8F7FDF4A594F28ACCB0AAE9B16BB4B1CAFE60E7D65F6E0976613D903E70EEE3B3B9BFE9514CCC72385B0530AE4E36313F4A3A98503F9CC4C6BEFD4A9B95C66E6E44C7C2C67E883F5546A6C409F218A7F9E7A847F0A15910458BD1A298A0440B59300BD48158B8508C392101868B9CE6C0C81FAF62170138DD7317C32048E7FBE610CA4E09EA4CB0BDE13D18BF1C2B146F385517F2704062A66780490262170EFC25C4E50FCCABF6D22C115D6E03F389001209B2F16E65EDD4332A0997D2391B43220D1DAC34E664F5D33FDC8099A68CA7EF1E7E979D3A9865B2AE14287FC54E1F15DF786063A31E2091E74E8CD500F312B729C6857EC769F3FAC6C64B4964E9A6E47C8CBD11473DB1F837796E1DD11ADFDB3CD4418830F080CC7BBA350651DB8C04295E368F216AA51AFFF249A8BE664E326F5CE0A92075E2B414E6DBA3563BA5459346A6C725E5C54B445B61359D748F924B99219B8C97D569F2C4C8F2C4010B8BBE81D994D4936CC603633F952CDDC598AC8502D6F13C203D5783C97AE4F8E6512CDCF5622A3B910406CE3595B20B323948A0D34A61B59FA5C614FD1901C4E3914F6F81CAC5371F882BA5FD5B31395DCAE219D97EC5230E2D1ED8CEC92835E9F5FD532139D24B54ADD632FA132CA5D43F1489A4CC4E5714A9193E9CB8674D97372F01DCE62FEFD3570C4FB771FDF7DB0218C8F6C094C1B429595CAC9EFA87BBCDD67A63CC75F6A3AEC0EC738112AF19F79071C4FF96346846539B0CD50C8B40B1C7BE4E8C7242F9D0622310CBC9C2669EACFC984CC3A8D0E8756F112EB85FC3AB42CC4AD281E22AEA2C4053ADBD25BD1ABEB41DCDA3CA0D86D62787FE5583774EB67BC64167D51379FFF62B57EA0AC0B7AC7E585EC80510D03FF3B31BC97BB6BC56262E7C11A354B584FC34BFB0FD5D184493DD73B933D60E07BF85B50711A95AEBA63919BD4D5154F8C8FC146F811E4BB58CCEE6FD91750CBF241CA28F848C0769117A81A2A8255483B686B63A652DD32758F5EF5ECA47116CA1119DB12630F8DE40647C2767A8963253D1D0EA775917E95618ED3A29664BD98135C3EAD9D935D22C38A2E89FA93E673815E699B2CB42F46A3D42B826CA3A1DA28BE47FF17AADD8D0EDF88C64A5AA1E01D22F15A8A79DD635E9E6B34BCE324832A9CB7D22A34BC7420D30A2CF41A1807FF19273D10FA1B05770D26E07359875A6DBB8ED24F6F6EC38FF49BA18E422FD292BF904A1703323E44E1192C058AE94C2120D38B1C23EAE95024ED13F0E73075040B5ED87271AF805FC6F83358D2BB1868BE0D1844ECF6F65FFA88E87A1F1159B610E9DC465DE89D001F9DFA089F077C62A84EF2C287D7785ED401A095982668DE9BD48DA62C6AC1962A385BC229FA6C77BBF520E98CB5630A160DAB55E8DAF262DD63F5AC69368E3A6F6BD6CDAC86D94FFD51A43D834933ED936DF3F82816B48C6966BD940D2B8A9D81B27F88B11E72D9B0E056DA77296AB7E81418D619D008F386F16FB18D7500F38AB7908B7A743DEC8507326FE274531262CA5BC64967FC327B0AACEB0E7C8354EE13CF72F54C2B46240E33A97A7AB77531E2BB647CA4626C030DF0EDD36E1583FFFF62C5AE3036085F0BAA223A6FFF1A1C96A55535A8F07E7FD4EE71BB25CA194FC299F304138198DAFEC8E60A924D87E83BD47976164948465EC014DF5EB109B43C8DC61FDEEF86B00DE9953ABFF3D0E1B1C6E143F5F602F3BB996746F7CE8CD5DAEF92EB3029EA1CFB76FF3ABFEF5CE7D853AE736E6CFFFE67EAFBF7D7DA5F6387F68C5626E1D95E81EBFCFD7FFFC1887D19966D1645206BE39B288A54FCDA75894D069E754DC1481FBCDF0B023DEEF6DCC3B3251950FF27BD4A63A338CFF07C73EEEC3533BB3BB3BBB3D7EC7DEFDA5EDBBB6BCB786D233031C5C6668D6B6C8EA4205368DA0009B428A551A091DA2A41AD94AA5212A2FC40CD0F28B6052CA12A51A1A187505AB070851B9136F407B48E1AB5280AC1BB7D670F0E37557FD423F97BBFD9DDF99E79DEEB79FF8CB4F6B84789D9B548D67BDA2291568F813278DBA3D1AC623028D968B4DD6B403F054FD2B48ED7123F30580C3463301B3E1F8CE67C1CE7CB4563793FC7F9AB73C0FB95DBE88FE4D3556CEA1C80BF55C5F6D6191D1F03743B3180C65F5AAE528887B56E19BA5F68AD51C51BB3B232EB6C8BC7332E56EFCE844319C80C4F2614CEB8F568076BD0529416F4F59CD10CD0F466E3FDD6608B62342A2DC150ABBAAABDDD5AFE133A8EBC980313A779AC84BF3C6BD2599D183FA772FF5E5373108E66981A5D59F30300C73526A7F85D46B0F964578047D4B7785F6BD0DFE2E54A919E8EACEB5DAD514355B3D3F2862F26318C140316DEACFC13BD43FC0CD45A18734C6396125E3AAB75FBED6B290EA2E64A371C99796C6AAB9FF61FAFFF8E517544BB57AFAFADC6E57B428AE5021C17C8C5E21D019E0F742CF5C7F2EA8D7C2CD6A9AE9D6A9E3D0F788EA308441E3BC3126B0182FABE8F04DBF19E0D1B0A3DC591C2D1C942F7D8E642B7FA2BBCFC0762947A1F7C69FF39F8F04BF0730C5F37A3E5292C9D06D23E00179AEB84A15A3564EE53BC2BEE8DB63A481A1F25796752896564922A2F19782DA5E1ED02FD8A41A8596A2DAF480443FEA57142BE7A42FEF113A865272013C139634AAC051E8BFE411A9D7138C24E50977446A8BF9C95A376EAA0C4D060C1099DE8153C874F621C26CC608CEE1C040089A541FE5CA9A59BB7468157AD1639A8BD5BED9264476FEA053D853EED48A5F3B994D616C12A15AC13F7C093167186D8031DFD28862A77D15592C48F3D7CB6F25F9F4D92A2FDFE3A59146562466F82229ECBA6D3D95C5A6B0FC3B32B77F11C4912144E13FBB01D807A57F918EEA48E627ECC770193D13D3890479F613446E0FB66458FEE08D60DF42C5D5FBC5EADD420EB4D56C952ABD8E114512D8935BA70EB86D18DC3B4948C38230E8E681F6A931DED836DB8DE165502291B418D5D2C6FBBB1507EEA3DDECA6B543933756D7EE1996F2CCCCFEDA4340CC11825C0B30DF098008F170BA8756BEF8C49A4CE032C0EF3A0CF6745595B030470F8EBF5BCAE661234CC6CBBA9AD150F87EA8D5332E126B96DB09DE01C11673429D1231B478B14614F063D1159474CEDC6E56716E6AF4DA9E24A03902EA1630B37D0B18B06C9086034D4D5F208E0F95AC584FE45FE0EF8F15EC0785C026664DC0A2C11F8AE19E0A71A3D40CFDC23F4D4278DC7D8411FF7AD5CD5432331157246640E4FF6242C62BC2701F444DCC1A48D245F7DBD7CF2F4E9F2A963BCC43124C3D263274FCD4E4CCC9E3A3106DA97600C1635539E023C77AA78FCE7300F2ECD881C761EDF8599300A176738595B0BE7FF4D8F9B40772CF1DE24619001514AA27B57AEECA3295B32E80EDB7478A2372EA24FE0F49363340B7038897F03AD3F7D1A0DBF0E55B92ABAC74E9C9A8568DD55BE0F11B40811F56D6C12F6DB606FAAEE0F619350A1BE497C05BF41ED6FF4106890907B5E9C3E13A51CA1D5FC6AE821575AAA90BFA84C3F98DB88506D5CC32FB2A24F76809CB1E91D09454938B4E5DDACC52F3B7CA20659917AB3A79978590B0286867FE802944C8A827E52EE79FC9E28023A67E51E711334CC9701EB8FCFB8378DAF21FB41975E3D1B8DB5B67591D1152574ED6CBFC0D0A364D1A4DA4ED7C4E413A47300EC42B2988FF63B8B3A5DD1D91FCDD3586A6878B46BCDB5F1367ABC95D934E78E09EE71B8027DC381A2B558D33F202A844C0686D296FA02B3C4252BDC55FFF845C19A5FF6AFCE8A9FA88E1355ABD13A1EB1EAA224636E8C21E6068344A6AE1F55A52251B0256ED25A4EB3C7F7A420AAC6FE206241252B610B83421FC2460A2B9E90C8E0812DEA178CEC3EDF764194840301A84F1E4FC842053FF421AD35A2A836F26F15C4A5777D37FDC47336AF852D7F144ED97CB05E86664992D02C51BE7C59636CD8A1B4FA297285D24D2114A837544D790105E03B753B7CB8FC11728177E4CA3DD24526B0416C04BC23AF1FEA210B8E126E3B1B0CA5D259329853ED828122D79203C6126E3F6BB30F8FF492B63EB85F480E648205DB00CB0ED80AC10C8DC5566D5F9BEDF9FB509A1C4A51EB17E590411E824BE9DCAE4C99A7FE0FEFC088C73466BD465BCF3E62D53DF2D0A8A95C8679C402CFC0967489C20B8EAFAB6C1FB183A28FBA95A88D45F26DD8D8631E25626771F96941144D2F3AF7C222BC64472DDD9E885583ECB76D7853C11D05D3B6DB6459FA89F38E03FF6B53E45634D59C986FF8627EBEE189F944137C184E37272E0B1695F65F5FE6C5EA9A78E9D62DE07E5DE536394E76559545F80266C1A7A1EAB9F1539816B3A3DE196E02A6B6BE696A0B06D3E5E27299550F3EAA35057B23944537EC57E0E478DFE15FBDF0BD5F1ECCF51DB9545DCB9FB8BA36173A27BABDEEDAAAE0B603575EDD30FCC3DFEE57D7F53FFACD8BC5C3E3E9E4C64323C51737A5126387D439B2720FEF209BA04B78A6ADBA127E621613F4BA12FACEAC7313B5195CB904BA674ED53DC1062248613A544D926CB0960F22DEA177B7848219B7F1F7924B60088D5187248AB3873CE1949DF1AABD9D60393DC1ECD5D1423C24FBAD3C334DD20422343A16500C4217781B185A81F59F03FDF9E9994C102E2C7F1EFF0C4A5C1475CD2A4ADE5142070A6C4E90083A35C1E74B68FF343D099CB5A8F3E1A29057BBF8C35923589B356A0C66A177B4D519ACB34A578157F5A3DAEC89B7699DA05D0A488A99A539D9FCB7FC1371C11CEE8C748CF7260C8C410B7D8335774E1E5CB3E5E893CDF2CA67C7A7D1C76AEDFBAA0B7AA0C6160FF89B024EEE626A6D21EF7035FB2D0EC5C1DA623E8B4BE205C52B4506F7AE69DABA634FDFF7F5F628BC715FB942BC066F3C8E4D9DC332F80705C3503132D41B191A8AF412465709BF3B8319D9F3E80044CD6AB4FF4C97192E5BAE849E9D1928A64AC872DAE7A306266C25F4DC34B54D8D1C75045433A94600BFA8BACD544D380146C4C5070EA41B21A5865195940771F6E0CE17D124BA09E2B51507CF3FBF6AEFC6AC9165084AA363FE4D7ADDC73671DE7100BFE76C5F72673BE7F3CBC5679FDFE2B7247E3BDBF17B4CECC48E63F246DE48D2C440D2BC92A401524A4ACB4801F585492BA8DD34A655FD03BAA962532109C11484FAC7FEA94635AA4D68D25436EDAF8E0E69635A3769C4D9734E02296543D09CE44BA248B97BEE779FEFF71173CDA3C960575467CD4CA4A64805BFEBA5A4D3D1E76A8DB4A3C11D18C8F8C418DCB3A1225C151F9C6FDA756AD8A78BF546EAA6DA9CA73A4EBF9054E97465329DCBC418E5186B64B5BEB4C391F1694B699B416F5195B0BE747545AD93315A8D252A9BA1DC44534AAB997176CDB7C6C6DA236528C6B54FF17B18FD9A54580F7DAB46DC4864D16D92E5D1D965BB5088B8F2A8F532ED909906DC5A3B5CAD4BB21CB14798E391E25192478A43C3577BDF7ABF36AB36073DF0607C367AAC8AE27BB7199884F5B4FCBC44CFD92C5E9D78F5AF324656229228CBC0FB18CBA53C916C15795EA62E7068E16D3037E10F7CBA19979F96A89D16A3CFEDD4A237C43231DCD9CAA5F76F79D10F57DF440448EBDA5F04F74456A8440049236DD710373A8B28914A743641E849B35E090F2278153D0FF948A0BF5826FC3191230F148B4C7F431EA8360581F61611291A0C3FF861F82628A1F542F5E0D1C20653633357603C2EE5C1A0028E80BD4CA052D27E5F5070AFEEE58B07263F3C9434A786EBFC9D314378F683E9BD3F1DF119A29D35F1E194A5F087FEAECE41DAD5C835EF34B1E1CEA03BCB317BC787F68281C193394F55D7F77A42435D59139B681D08B62CE402EEEE17D381C1F606D6D8D4B50B4D35B4B4A68C41AF9BA91E5F5DB4C6033E2DE30FC6CD6D9D1DF0B946E09ADCDA6AE747453B3F2ADA7970DDCE979ED24EC1AD9AFD4B0B6F5D18B5FB0F2C1D85E7CAC2BF14AED6484D3347CBDD2DF0ECA5517AFEC60FA19DBF9E9FFFEC5DDED063FDC7FB9C553D0B3BE1D951D9C3DBF92E54EB82D00F278EBB8254A1BF4C90949E12C303512BE4E6812A2A0F929B447D0119FDD57A287E4B27BFFF31A452FCA7E002067748AB41E8292682DFFDFD66B98EC2D0D23209CFABDA6E801DA0F4B73829168DB0F6728228B7B33ABE738ECE894554B54D6D800578592812A0825249E9FDCB62B59D6FBE7DF0BAAFC1358D22495EDB7FAF78CCF04002D78BDADAA0397AB8B8F32BB407AB1E9405B6421B89ACFEEE49CCDAB6F0217C44D96B22F816ACAA69A312C7648CF25E0CBE2ACD6D9181464E0AE314936A9A460E27864FEFE1348D877297C03F094A823D222CD79A8CE8A269B682253455065BB5C662A42B5BF765FCCF4F1E486EE8DA03755D8177D8870C5F41BCE81F13D2B62E5B5BC2D6D6664B08CA60EFF91AEA9A86A9BB148BD1411ED56C979347D5681465078BEC5208BD412BBCE387B87AFE9FABC167647565FB89FC4CFD8BFD310AB22A9311DE96B164A83BA633A72733FBA472092CDD94647FF4B9384435E5AE19CCFA25F079C2CD272EABCB1DD9BEEBF4F37E7DB437929A6EAE7C67E847E341A5564FC975552C67D51A58AD3755E9CAFA1F90AAF5659CA6284FAA0153DA0C8C8926299B45FB905422D03E5A6C8C52C1DD0D51C38F886A86A212A601BB1626D14BCB429E5408AAFFD94115DC55C9E7243AAFD5EA6525AB6B249C5B41292CB967851A6783BBA6C9A19823D58549B4F063B0F79BA032EE4AA3C76E96A35FE2525CC8FFEEFE6FD64185A2C6A11E378BA286608AB21701DAD0D37705C9AE7DB242A2AD481670571F436CF8639ED8B53F2508FE8F10400A90FA3C7A6C9908C445AEFCDA97CB6212B4B820BC099CE94FABF99FE02CA916139021759DE6AEA3EEAE6303E1DDBB728E4720CE39B4892CDE00F07A80274169021042806500D608B034C052000B012C08B000C06A00E607B81BE02E803B01EE007835C04C4060046278E9A4E0E92E07BA08AF06D9957BE40B143FFB9F940FFCF83E3E206EC60F5D987BE167FBC2A6E4100C88A83E34736E7AEACCB0C7108601B1A7DE5CB8AD74D439BA3B55CE462EDBAE67023B02EE4677F9E8C8F01018E83BB9DBEBEC39D2B11E11C9D6C160DBD19CCFDD7D30E3E9DF91D11523226E0EDB957C48706E8D637875C51A0FFA348C2FC48744379FFE7DB061B35B53E26C3125CE165322B8488EC194085E144D3E55C3666B0F7FFCEAB1E57DBEF8E12B47162ECDFA0B5F1B423BB8E08E30AB0FB7FB021D2116D59CF8FC7473E6FB374EBCFEF9A9E6CCC9CF7E30F166A7C9D97BA26FFCAD8E0A67DF7168110B1B362BF43C68D8EF2D998A0DFBE8123B2A9AD868D85FFCAF861DDA6CD82C6CD876AB4F2FB94C6B6422B4442ABE4D323A08A05B5D6AC4495C24C44942201B26848A2A0B636628EC0D5C8CA1821271296F7E077C1D7A84B5482D92E6CDFFDBC386FD0F687E3598D968D8B32B7E1A738FF1F57AEA2236BEA9FE77ABD734FCD7F0ED5C9D29D7CB4B309946F589BFDE46521501B3B73D56252E15970A510C576EEB9D8A0CBCD1EF6292737DC7C1CF95AA51D6A61697A8AA2A4C5CA54571CDD7998A6A18878162F40CCC03D8AD55328549AFAC6E9E88D70CCD1CED7C3504EFB475ED8EA011DE691F7290B7FF836FD9FF7BDEFEAB408EC4101C841312EAEDD8FB313446C5283E0AE40919CC82AF6446306B047C1C8CD179C06DCC0E8C81DCFE072D3BF7D89A0DBE731C34A65E3EB73B3ABCDD4BE12281AC8CB0C777463C199F4613E9DD3626A5F85D27299E7635F9346445D0C675259C3846C02584D53BD4359DD8B1D0E7D2F89A5CFC161084BA8F745595A9184AAEA9641D069546A534D7184C018B02535AF45AB31C53590306B8A80A46AFC614153A959EA6647A9DB2A26134E5ED4C7A2402CC91E886EF9865ED3FB0ED381027C221B58B9C89CCA393CB769108F1E4C19F9768BB96CB032A41085D269860A651821F6DB85EF2089F045B33013C3913F46831142E28C9D7C43ACE6CE5584921432ACB3038E4129016EBDCF59E9A7435F55A8914C70A2368E136B0806D5EEE3A0133008E1A711D53BB6CDACA8A0A1A14E41AB24404DF90D53378B9159516C27C3244E1949C8153C2274333DFB53974B21804135B83E02AFA0A84A41E7D254111E1FA7880ABC4448E3BCC58E6CE8625F0C93F5BD75E7766ABA51BDDF44CEDFE73E323EFCD4434E1DE18B7DDA7094EFE6464EC9DDD6EB5B7C51FDD19650B5F6DEFB0476D72992DE969A92F5738B2416FD24AAAAA1B5C358DFFA5BDCA63A3B8CEF8BCD99DD99D9DBD8FD95DCF9EDECBBBB397D7D8ACCF5D7C02B6A9F101B6C176C2610E435ADB292941D02860E510289494A20648FB67C1A5219C5B82D4A805A19610112542AA54F5905225A56D54421B5A82D7FDDECCDA984010A5892D7976DE8EE7BDF77BDFF73BC206E45834B624EC05E750D2D694E1ADE5D9D678FD487B24D0BC2A135E94495BF9AAC676742D59AB7795D8B8125FB1D9DF927739A325418B558884393EEA3170C50266D5F980D17AC0C8428431AB72E46100C3433E47A8091EA54E18860339947A30ABCABFD47BAF4FAC79FDA9F667FAEA8286C49AC3DF1C3F3810CADF31062B43E174B1CE14A80A462ABD5A927BFEDDBD6DBE054F3EFB6AF7CE775F696BDF7BF9C54DBB3BBD42EFCEDE11E90A9DBE150E322B8F833F8961FFBDFDACCB00D69B35197368C749DF5AECBFF5331C8649F68258820F33DFF3EF31DF594AA5514CEDC735863FFDEAAC8507E6556AD88F9466A7E00A466D8A9F8049A18678BF9561AC7E9E0F722A727CB50A71F100E7E174F46E4A41913268CE3BC7545C90989E263A60CDDD80292DBB3A8DEFBB00E336F1FE3D1A631E9BBE2DDB56705D3592EBDA58705DE877B8CBC071A5A0CB84AFA0CB24E7B5CDAC7F42E328F5FB930E363FACE534B44CA156A204CB471BE69535458C4F68B9FC4E327F13E990509A9C5469718B6955930A6B34E48EF9DD0692B5F03A05CDEAD5537BE3289F4F427FA5615FC3B02F1F514A2CC0FD15217B0823E127BBB32A87C6ED30C2AF32F516341D41D4901BB30665AA667EC21FA165A1EBD6E1CC5F650FEF2FFA81ED453F3CC90EBFF7CED0FE7515F6F2CECA784BD29E829CB16A4F7F78E550F9D20A3E7FB3B1A5A159EFAF8ED4D55ACC427D2C50E1332CED685D8A42877FEC6ED8D81A59D254C373E599D678E3A6B69260F3EA9AAEEDB1A2CAFA36743955535D6A0DFB8B4DEE45791F1F0F872CC66049D25199A986135D02DEA41F3C28900BEE220BF91A7C74933F046F5284BA4FE8BAFC39D4F326B5EC61DEC4729F39E96F98B8B4EBC5B7B7A6EB272E4DBCF48BADE9FC0D4F664575FD60ADD32B5D1DE40B07FF736CA0F7E8BF7F74F8F61B03FD476FBDAE79E1D4E678E5E8D131B8C6D26347A087ECE04EAA281BB813C7CF092FB92FCB580DAC9A7574523D505938B55E29FBFB83ADC94C5225ABC09A0403652EED55CE6900DFAE65713AB507DDA1B85DE163752A0A044B2DBB3EC6D20621C0FBAC3AE52919254750670CF626DF809E380208D511ADD89BFCEDAE37F944F426EBCE18D295923DD920DA932E6C4FFADEA4BBE7DA137C796C8702DD7E448CA65ECE6362685D91E94FF59D82CE1ACD842B7AEB636A46CB80E75299EA069E695A7B604DA9BD7562F301F44F1C4F479C6188A7D6A8CF9B08F82CFF681E1FEAF07BABA37697DFCDF2091FE7B1198CC14051D98AED0B333B5E39BAE9208EA8B0EB06C8268760D7BDC47AEC532EDDE7533E927CCA72D1A70C9E31CC38941E29ACCAC5B0DA05EE6459A17A6643AA88C0D7624D0ED56C3BBFA3716C591A27559D9E4D2E1E5E50D155E50CB4AC6FDCA831B1340D4975A4AABF06926A43BCBCBFA554AB10AD8992AB1BF876E3D0ABAB53CEEAE59599912551B465E9DECD19B3C3A5333AA2DE988FF7F045C92641684E39145CC8ED0C98957CAA39520C50BA036EA539E8B479AD7A73C0678F766F6DAD1EEE486B6554B26304B3A66BFAB63C409945D6AC955873CB4C56351658B30772AA5E25E4D0DA93DE4E15AE6E6095C4E3D2A63C60314EAA9D297F30E954E719BD5DAFA0D4662DDA493B928DF1AA4561DDA4D1924F92F99FA265E8BBE9E4672AB0E8E042549F296C51BF27158BF1E4213578395A6D524F5109F23B5367B11EB443557C4A05485AF64B02EFAC12EEAF4195980881B838935F6385FC1A43E673E47E88AA1FCC89AA9EB7604845F0D31F9F64513B3F1310A179BAB35A5D57C006A301312506702D65A176C4948863A2309B59A59C6A8429180F625488342312BF1EBFEF71DE3B24E64E786BDF0372977C0EB5C965D752A3A7774EFC6C385C367AFAF98937864BF2B7541677345D5CDD1E337289C5F3423531974941EE3E78FBF8E08AC95B875EFB5CBC1E59B967FD4248BA6347475F3E3D22D8536D6B76409F7D1FD8E5386585CA3829E197D53061C49420650821234AE6A6DFCE32805E368964443847EE3BE9B2B186DCF4EF4FC3A0019B89ED59C6D719D6E9114BE97348389105E2C15B833D49C642B872A16CEA4AB2746840200610EC93CFDAC225280CD3CC99094FF008AF831A1C1A905E3330D0F7E55E25708F57394EB35A66AA02B897A6E0D38DAB56A78126955A35A6625BC81D4CD894EF333A965AE30859552A6BC8E10CD958D9E271963244823637A7559E92533291933F7F9FB58500B95E40EE3CE665B05112725A7914C905C45421A612B1D95CA10EB388CB919FCCD0F6394CDBD3D7A59264A166D80826EE190EBFBF7E366435055617CB0773FB2C2882C4EF8230CBF06279CEB23C200E958530E4A67B16078BD2C9BEC289F1B10C0E1426EBFB5FC5E5BC282E36CE636668BDDDFCC786CEB8C112AE8B54AF688A6B188D920293686F58B5258BC5C5D6F6F2D801947FA8B804DC6A47A2D8EAB61A6C019F4D149767F74C8E8AE282881E608D33585B50463AB9B3CA16A46A466CFFCC91F5A3D21CF9EB2F68CE39F22F40261F9FC20F680127E908B5002416A2655F102209CF8A193CE72853D6204913A6044E8416ABD45C4A284815467756ACF0C17E20DCAB58D2F1F2594D61FDB06E9DEC6B5FCBDDB3EEFBFF25F3CCE25DB9CDF54FF75519413241814ADB8617CCEFAE76FA20A27D4B6354531448E668557F2D4866637CDECA45656AA51A249366CC7583DB160E7E6F7599AB6A7965E3A6D612B4EDC91F0C979B1C2EBD990F3B9301DECD17251AC2B185657324B345F0560B20991E8539E8B27B39BD29E8B747BBB6B64992A92CEF580BBAE2879CF16141316F14BADBAC88238580680752E891428B680D62457A6471192401795159D749CA7A8E64086EFAD3AC06BEE4F87848C43604D8CEEAECA9AC28B4B60CE08CA5764AB8005A2BDC155B7CE2A22EE023AE08E950288E42020A3A50488F425A14D4A0072C495CC9234F289D656196C24FDF2387A30F2DC671B52B19F4973AD9BC41CBE91432854685F65136A13E51B650308FEBADF90D647E122D474F97955F5781D9A5E0CF75853D11F22482C526F222A361E414AB67EFFCAB94DC35750CAB7C2DF4E75551E5DF21C47BF21C7993FA33A99053C0B9FB61C4495E4407A83FC0085D18A9232F93DBC56714859134FCD7A838A22C8CF8C88BE46FA8DFC2085318A981674E88CF680A23ED64163D473FF55FCECB35368EAB8AE3F7DE99D9C7ECECCEECECCCEC7B66DF2F7B1FD9B577D78E1FE3AC6BAF6DC5B11B6849DA4D70934252D56DE244256E9DDAAED3A41225A82048E04B5B22211069C99B848406A950AA7E20A08648452AE25B3FA0A8412502558ACD99B5F3A8D35608AFF6A1B36BE9DC7BCEF9FDCF1F22E24A641022D3CD88CB884077742FC5C927640CB68E248A5C4212FE089611157FA45B59EF9BFC74F44D66E65EBB125F25E9E6556E857CD2F2F0814D8D8507E3F0FEB52D0BE3893FCBB1B650AC1412A458BB162B8585DF6C3DBAB3DAB1E3878D2D479EA876ECFCC1B6D1C73A147FE7D675A3DBAAF0BEC5D898834B057C840C8357098257C11774B6E9556E04F631CFC13A77EBB659613EC7AC54562A8B8F589594AA8206EE73CA1C434CACF5382C6E9A371815E933669B99A2E085F4745A885DF5BABCC0E01D84269862CC8C9143CF5299EC879B29A3CA052491AEB3D970368C4AE7C9665DB4467FBE2B30172001F707A969AEED97D4B34D7F72EB6AE38E316966675AA51C897BD841DF2B1C0AD96FE7177D4E8FC30439F207A239AFB590D3DAD29AD5C442274AB9BED196FEC7FBC38EFCA6913ACE70E24C26C608AACF130E788417A2D5352D522227CAA2450AFB8361C9ABF05AC7583EFAC0C6C7FB6B31384D15EABC1B4E338A1E82AD12FF53B70F0CC506AAB1818158957278CF934E3D801CDD3F6DD7256FBDBD70BC6F38F3BAAA327DD3D6134EE50DA3096E5DBDED3B6E01BBEF731CF1D5F82CAFA667F9CEF91377D16932C84976B73FF2DC406EA41AB69A6962E3CCC16C57BCB726A6BA5BBA6D9C85A26155D1EB0385B67047AB66B1B38462B8D69EF1FCBAED7DA1FA48AA3FEF0BEA8D6ED526F056BB2BE4D5020EA7239755E25ECEE40C2AB28F379572B18CA0086AD2E9E159CE23F3C1B6E1D6C1C74442A9F96E63FB8E2E15C87BA4BF49C98EA6AFC067567C057E59E795A427F706DD1206FC84F7B1460F8AC6B2B0DA53C4BF8036AECFD0862A91F7786E827527828184C22EBE03C860C03899FF4D8991522C550DF3133661F12DFC87773DF1C4FCED569D67C448D01D0DF8ECF86593C50CB762332FEA49BC75F15D1866D405153E0515D6502BEA42439750029F44020AE1933AEBB3057D023C2CD98BF85730E0157C5EE72C2D452AFAAA3CDDF91AB53CE3B0FC34C75C34FA175E9605D23853F31CE54A8EBA3DF746E11485320A6A3266DF5D7641B5930E4A9614A83439F5B31F0D4E8DB78C8CAA8588981C79BA3E3C3918AEF50E3EF8D735853505CE970E3EE4E4B57CD897F07295CEB51576CF94BB385AD127927C225F8D14C72AAABF3494EF6A84E313B8148B264242D0EB71E417DF726901BF20F8039A33994A1873BA1E903C473E405114B98C047C1899900FBF0287A4F0E9D3B2663B807A7F87F3B7AE5DBF66A8BD09D255DCCA728592CB075A21069E696F2F9718EC8C85644D62A968292208916284B0AE903B1073D1E4E4D4CD170FDEDCCBF146B9CC4CEFDCDC42ADB6303FDB4731106079C86610B2996E6613BB80347CF894CCA38BF80C1211830F9FE67DEC723A908C709BA7660705239168AB948D094926966FD8AD52785A8894A2C426694A28EAA4DBE08FA6C568C0137259317C23BC5D5B989BEB854C0863E3B9BDFF3AF4E2CD29439708C3507DB3F30BD0D7F9A58FC94172F42EDFCF34F97EC6E0FBABFC6CF435E6857BF9BEDAB2DDC7F783D1E13D635F9DEA0F4486F68C6FDAB3CEF70EE749F9BD504607D4329870B37868C3ECE66271D3CCC8C8FE474BE5479F1DAEAC2F28727EA4DCB321EB741746E096E24B9FE263E4FBC0F790C1F75FEB9CDB3967C380F8C02C330FD3F52110BED1447CFCCB107FCCE24E696A1A102F2A1C8D4D36CB09DA2686BC6ACCC54866D8C260D936E33F56CDB4CDEF757A441BB38D10820963A2218B020C7E0DEEA68CB206E113E70CC2670DC4AFD5AD56F72F52B3F6B69F50280F64BF83F5DB13F13F73BD66E7166BA2C7C15056A7E379C38AB5C6D442326065AC26DACC67D6AE6FED057609997A651C9B787E2011A29D61BFE89365FB53FE5434E20A261C226F1635B7DF2BC82ECEBF6620A375F5D5D3BAC1F528D4B70E671847CF185CBFA1DBEBEB63F5CE58BD1EEBA438E07A4A2F22AEBD3D8D0A42814885233594C64AFA7B1ACF4B481334D27143C32734AC694C6DF6B2744522D2D1664B340CC2ED9EDAD28037E1FA9686F10483F445DCA7BF646D2E7FF682EE2ECD40FE7A6EE3DE81F46035255A6CACD59FEECC68AD7EBB235249F7B276837B9CA55FEFC914836D2DAA05AE0C7CABC996E919CB75377A3567A414C9F4A6A5DF1647DB035687538C6921C92ED839978F178312CB38BC2E97C74EA7A3FE38EFE41987C7C5CB0E0B2B8B9CD2A2A702C5946AA1BDC912CC89B0F4299924DF69F2BF7715FFBFAD4B4AD27F3977254772475644E07976FEAE0834FE1F153097C8A483D3AD9E1515F8072B586172ADE63F5162B818CF9423769D1516FF4E5E3F763E129930B126A39B4D130CAFF964D523DBF1100DF7419B59D3E24B2A0E2C8206A8D00D93D00D51B406D5D0C42594C1279084628606A87C5895E0C1162FE2B300812E7C4E0FB167B3BFCFFE254B652BCCE9E4DBC9F79354F2BBDED92B7DF8521FEE7B65B9111AF00435687CAE36DCC5C57DEA905C5107C5DD43DD5187E6664026E323BB46EA93430931D216F5674362F48127EAF59DB5505F45AFBF1F2F6493B912EF936C9CDD9F0A88AADB6E51125A29733A3754F4CB2DB5AC379F4E087635915793EBD6F83CAD3D89E2B03FB8F13FC148C8EF036FE49494C5ABBC5791395676FB1C76B7688B18DB779E3C430E321AECBF12CCFE2188C4C93C3EC6F82022AF440AE410A9357FA3AC44A2F05FF566C4BD1211C83C996424887856222AFC66B2F99B8011417829B7F437EA38534432F25E42323E876CA056E74EB10283F23046D73F84FBAB2CA3BFBC6C4714C504EBBC2B900A86934ECA46FDD82A069B9F69D6CCF12C6D11649EFED826189F14070271FF06DA4C3F428F2233E2911B943F89F2A8827AD120DA801E465BD137D1D3E85B68165DD39F1ADBF1E4579EACEE9BE99A49EDDADBBA37F4F5EDB1ED96FA7F29AFD6D828AE337A676677665F33B333BB3BB3DEF5BE667767D75EBCAF995DEF1AFCC01842C050E1807919F32CA84088A808A50950AAFE8B12942093142225BFF2E8035AEC02C6763006121A0944A552950847E447AAF2A3EE03A4A652E4A5F7DED95D0CAD2A7557BA73AF67767CCEFDBE7BCEF7F53A7A41578FA9C799D5DDFADEC30776F4F6E87A4FEF8E0387F7328DEB06BC8DCBBE7F70E5C1EE578E2E399ADFBDAFB8CFB761303828F6F54BFD645B07DD616B4E73E98347F70DF677A4D31DFD83FB8E1E64D49DDB1415646E656E095827D047D09CB7F2FF7B20D02FC4FFE717C8ADA24A41D7F289EAD555BDCAD56BED3EF3CCFAD9EBB3F719E9E975FC99F7D7FE1F7527ABEBD921347CA3E5B45C0CCD2AAD79F839A3E5721AD987C6591FFA03F993FAB3B367B37A3E1F2372BA9E236EA09B9501347E839E1E4233EAED3C12929C56F9A3A6E5EEC305F10E9CF4A3B7BD0A07E2937CA630BB14CE4E66B33A19AE3E5461E0E401FAD9177A564FC309CC4C2F7993FCD6FC80A42D23B8033D44FE8EBC6AFE1AAE910A106035798DBC667E08CAA03C924AB1815162B88B07B661677A38E9845F6F78B2304A3E1EF64E9A47C90A2E1604C3099DC815AFE733D99C0B1F7C09D62C1E542DAAB0B032AA0609F9012C6CE04D4FADD44993ABB5ADAFAF5BB629C8BBED26646D763694C887B4651969EB604C571B59BBC8F0A2C9EEE679A5A52DB1E18DEDBAA96FE3A997DA23829597C20DD9B08566DC221BEFDE5078F9C7ACE8B6D09670B6212CF356ABC85BF59D270D9E9484795EACF3DE8FD7A398F721F236B9CAFC35F083F43933374AFCAA8B755BADC0CDFEC26C164C67BD6390AC403E862A3F83DA1E22731D1921CE3A24EF98119C0898704180F687EC7E958BFBD723BB20D81F25CAAD4D612F718C17CD2753912125A9262BB7399EE5C84F7D7EAC466BC86B14677E08118D5511DE244A38521375C43D783D59BB6F5230832BB535E533FF19AEA780F1BEAB542F7EDFB5DA7DF22C7EFE53807A9C35E473D47118E9388EB5CDEF544789F3C3C09B1D252E5C70FA6D6C6AD203437C5E99341726D951C41D2A7EBD2FB88E264F4AC4044D1B9D1C696C05747359425FE2999A913A6EB2584DEE8EE7962B8DEBB4B14BB9AD6F6D1E994AE64535EA671C0C34F9CA3F85E61E4D5B9C1285A6C59ABEA459205F937C9C59544B0996BBF2FB2D3FDD53BE726BD5FBED3C147258DCD24E9E78BB7563572CB1685DAE3CB0309AEC595FE57B0FF3FD0C47B80D66F64AC87711E8BE90FA48737C208AA38FAF8CB8E5A51ABC7671BC6BA9266AA25CFAB0DD675621F361F9E766833674F6B291E6A994617575DAC8E6E6B4B8A69AE9D592BF4EDE44AE4C2DDFD51EEE6C4DB3ACD34AD91D56455F926E5B907BBEFFF9DCBCDEED25FF023DC1986813C1709650BA4D09AAB235B76CEDB21C35D6B1A93D443B9C36ABD313F6AB8D6EAFBB25A2A4E26A79CDC2F29A72A385136DB4436870AB41A7CBC9490D0E25158D155FC04E74933C8177E106DE85EAF9873541E9A2EC67AD00F889CBC42F811544A129F9E1F39561408B6390378DCE37A4DE59B5FA76E8F4220A39AE5C709029E3042728548AE36D9089136728AB3B1ECEA9B4F96F26BBDCA4A8699FC3F477B339515092B285A23E22C356D642D9DD66999DFD8E83634892E159F2D70E99F658616B6B41D99A84D9FD439CED9FE36CADE18E4157EBEA92E32CC749AAC36EA762BE445C9D60132187830E8D43C82E081C064DAB956379A23DA369DE5B7918443C400692540D4E2211311A2ED4D0562B3299D212E43FEA78B3954A0B64A4866A8C9251C8C86EBEF3611FE5AA633EFE5A9D13790E73B2706C65C52C6C3C6B1A0B3D390C9A40EF88470CA8E3C43D60035E627A44146D9151E27E9713D83C81B163F137E3643CCE348E712805D531A69A824876F6CF9471C50D05086A8F808F1F0D1BA1684D73AA6131AA2DA44A45576D425E2D36CF7B71E96DAFA278AF9D78A3D8D2D9F7D762674A2F6BC9EE15E515E56E6AB2732010F0F982E40701DFB6EF155E9085C16FE7A967B5CAB4AE4D35C118543514EE7018A8A87BBC0F50FA4C8F587D132C42EB9FA06B68E7769073D0C992849BC8A880B5406ADDFBFEAE7104E9F3DD439A5EFC4C2AAE5F94CD2F585BF29B76BE787A5B8B81065AD7EBDB8B6B17842B31DFFC01ACD658BD8102169C73874689E92E37E3E5BD3C60DC13C7826F06C960D0E49AB0234CA109D3533B082B0BC3AFAADA1D7922D72827229EAA39C15D430ABE5F142BDF450089534E97CB599979208A662B6B7D407844910AB6440C84A14C4B7828D4220705FA5C049DB3AA9A83F960F988DF9F9120C6DF440188C24EE2CB2E31131A07B2532665596F2E5FA6EC777504D5F5457315EAEC913BDE19D8526536554DD589C01B809FF6D3626BC1C85E4C4040AE4AD79B2DECC1146798AA9A6B8F15FB3B222FEF0A37734778978B27183964F528BEF5DB77BCBBA734FFA5F7B6ADDC8F5DD83C6078AAD311EDDA587AF5158BE544785E2A12F457A6048FC032BEE2AEA1C12DA7F7B605B1031BFE89DD0AB040BE0CD7D38006367818296C9990430D3BDE6B889428254BE544B2544A927FE1058127BFE4048103D5D8429F0322085C023C313D0C181C48FE6E2D903084334F070F518703D903A375C2AB3417F579304E949289C2F04CA137CFFE2C6CA0449E09525043AC0A0873964844A2D15984F53371EFA2146178CA9EF0E1685076FCFF34AD13BA42B92E829A66A860950E858D2F41CD717E2C222EA42608984989950AEA1493D0F2CDE64B6AB1184F84F7075341C97AFA3DABC71F6B389054AA7BE0987D283A789EE4671FE1F58812B54B316FA58F38D31095ED51A5BAD3C8E741016447A2D686CC38F1319CBB888FCF37F04D523E8034D004244303B5AA5BD7CC1A4905D2EB349590DC9EFFC00C4D5B2DA87A6D4F295F2AB04256023E7E8AB630B4898CEBC5483230D810F34BEC3B0E81735808225ED0158A0D076D722240ECB13B1D0E8B94922BBB3951E4C8B150C8E20E7B2B7F92427E9FD32D3A437662AF137E8C0A045728B05B09213D71125FC1307889AF462CAEBB869EDC35FF373DA9D7190CEA29EB8525D59BDFFAD6E0C425386EBEF8C96F576FCA2E9EE7E9DB8C46D3F66DEFEE295FBFBDE5D49EF2D53FFCE047B185EB8B470EC7BA376255C3751154670564CF85987198BC2C82D165036C6882A66DC10901957FB6B9E5DF9D99AA8260A598BB8F4F340ECBEED9E655875616126DA5442C30DABAABB9D879C3A7A65DE9DCA27F735EAEB16D5B571CBF7CE8415E4AA44C510F4A94443D28CB322D4AB4DE9669CB6FC54E63038BE72CB1E3796E632CD9BA200D8C6649D3AE8051AC038276589B7D1FB061EB007B6D1AA44DD20229FA69433F145D8102C5826218B62103F2A51FB659D92525DB71B101DB0C8197022C9C73CFF9FFCEFDDF3AF957E3CC4CEF9FAC56A342797CD2530B91B48FFA7C0474F342FE059D18836886243999BE89DDDF01A0EF26F6F98E9CE7CCA9E7F125F3B7DE633F62FFC0122CCB6BEF8A66F76D80EF74FF8135414C01143E7EA0E7CC89F29875492BCA21C3DA49DD3A3FFCE88538D8C36776A12F19497A2962D25F50C3E666E2818F23596EF57C6B6AB8AF096D7FF3A5B460BE5E1EC3FFE2840E927032CE376AF9F69FADBDBD2F8938EE3E7B6A6E5D4CAD5E0F0A38198EF38E9D3E53CD5D7F027C20B60D6C684F6F7190643EE5CD823B3B054797C17BBBF7BAEADDCBF760E8E1AFF0DCDBE96AA5F7460FFAC3558EB7FD584DEEFEC20ABD9854B598497ED74FA0793C3D12F631898124A4412E0173399820B44A5C44EF095B51CA9805F4005B171F931F8F8EE5F47B053F62DFBC89EA9D0FCA67CFFCFC1B17C4EF7909FE31B374C3E60EF5C7535A9425CFAC105C444B5AD6E81D9C2C15A30AF211973709E85722B15E3F24AE7C9F700AA9989A2109FC9F2C0F0912A289FD6AFB9C9B873602F21CFE33BAC789FED1C1326D1A7B883C2549501E57DB857D09598AA4FC4E2F63EA288FF5E306B9844EED0408DC0541620CB04413C868B511750BAE0351ECD9574BC0717BC7B49770233ABA3ED55A1B0A4646D7A78FACD583AFBA236AC4D0CD6753C33F38FDFA46A5BAF19393DDF55B8BDF6B8A2F5D3BFE7433F443F34E891978985C001110D866FCB751700842283C61852F18FAC1303765E871D82D3BD6BD285A833D4CBBDBCFBA5C2C7C618BEFA15DAFC178B24F380F5DD8662C188C2502B87381F12B92443FE9B4437AF7A2686A6B041BC573E4321800D11D17CEDD452163283805B2E80DEF067FD061A21BDD0C5AEC4C95AE29ECF4D1BAAE3A4CB5E5C4F6314EE0C41F8C0D162B92A6841C140339A8E8C3C9DAF19AE4C9CED62F6065C8622F8C8A59B52C5E9B5DCF0C547B90E74B86232E4AF0C048B19555A68EAD3436AD93731C4BE0FDA83F4DA06C8F846E1263BF49246C993B28C52A4A56B0DA54DCF32F0F1E07785F6ED6DD43497499E87E3BBC054B8182CF87F7AB47371AC9612D86263A4D394299C1E8D4ECB15315744BA56B137385A1A89E969C344DB14C326F64CE3C3DDF3AB5868B8D93C331C6C3DB9D013918657BD8D14ACD08C424FF58A57F88472A97231148F11EB8BC7AE48C97A4D74DE5A5B1417C869C0332BA63C6B7D1714A8CDD88C445E46EC55B686B0EB303FB2EDD6AC2DE242DCA9616BB96BCA3012241E8F88CBF292BBD52FB0B3B43D95CF0233B2BF6C5CC09C4FC11BFFF05EE19DA62DC6E668B64A5805AF5615BAE1E9AF4F2ED4C087BBF7DC2F4E2792C860FA36A07D13D4207CADB7E2A9B4DA508E94E178BBCA5CBFD7AEBFBE766A794FB25EFD86CC2ACB34589BF645AEDB4DDDE7165F8F0D68B97B6E616A78E5CBE6C9CA886265A0BF3D78BD552999106E205CD654CD54A86516FE0DCF92BEB678D27D5DEE5E6EAB9507E225B5FE95557B05175B0D82FC463114FD868FF549D48C4C635BD326856B502DEC25562E380E70194B881781E404219F9EF7856A5E1D5B1A9E56A406AAC8E4FAD54FD2FBBC37D5243339F232AEE5978FE444E3BF1DC13DDF578EB74C5F7CCB333E613297609DCC319620D4840DC7131BEDB28BA08208A4F5AF111D0BFD5F2A9C3447F15680632ED1FB91837BC74354933D76839DEEB3D07992F25C1179604ECD684B7110C38BF8E4E5F6AF7539FC9C914F8108F12E7806AEEBA07458BA1B859B4E2FBBB4E1D603C4C1C66A0547A1C633C1A6CAFB30217B82C6745582885D4A4E8A020CD0A6B13E5857288CB4C57BE83F5061F557CA9F480EF52ACA82A3D4A9EE5D940CC2F424E0EAA462AD6189B1F7C0A65D602BFC643A8232320BBDD48DC2406DE0C856C0399BB28B732CA51402B877AD340270A4A53FF7F31EE54AF8B7128DB5AAF256A6AD4EEB4D34E87A8E423E3D3B34B839065A94AB3A5D5A4BC399A68CAED3F39FBCDB347279796B1ABFA13C510CD72368737EC0DBB5857395728794341BEAAA5748EE738292052AC3C3F6F2C7948EA6B486B39701B2F132B5D822349B4B71B445C84D024D840048FFCCF04977D4634A584DB0F51676D107E687707D3D17AAF41339F619FFC1EDB2C5CA021A42F904C4050342F76846129C2C3B5BF1BC09E6B5F3709AE825F5AFAF723FDE710C194904ECB3211BAD30541452B6116DB22D8F31F08F61D06D8F7557ED5AB579E79BEB5D09CDEBCB8B9D19C9C3BFA8A5ED207F5114D7535C6CA7A7DA852C72E9EFEF6E2378A8B6979BEBCB0BC38A3CFCBCA02E651D47E25599002E5F6EFD2B568A8D6D7A7F59BFA058F9A3611E5EC02EE77906647818B480253B542B173F774201B41FEBDC74DC1E4C98DF34377EE0BAC243B846C4200D8A39FDB7E4572761D30C0B9CDD8400E118699BFDA638C78C872D17F7CC2721C6BD725653C1B143328EAA35B8E37F049671ED5CDB96D36149DB5842CC893F8CBBB179CF91701F897000300BD2E2A490D0A656E6473747265616D0D656E646F626A0D33332030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E6774682033313530352F4C656E677468312036383936383E3E73747265616D0D0A4889EC567B6C53D719FFEEBD278EE338F1751CF272D21CF792A7F3008747125E8E63BB9414C80BB02324EC3C20305833CA58B2FE41A6A92A5CA0639A346D9DD6A2A99A3AB689637B7F38A813993A954DDA26ADD2A46E5A5554DAF58F9682A621AD83E27DE75E27241D5D61428D58CFCFF9CEF7FA7DE77CE7DC936B83040036980605E8C8A1F8C42EE9CDFD00817700E4D7468E1EA147DEFEE887007B0601AC4D7B27F61D5AFBC2A40A10FA198065D3BE83537BAF6E7BE37B00A72601A63F1C1F8B8FBE79EB582DCE887958338E01172949A18DF3C1F2F14347265FF9A9733B80540FD075E1E09323F153F6976F02EC9BC6F43F0FC52727DCDF2EEEC5BC15F974E2F0D884BC2786DC9E0F005CE3C07BCDAD6CB01EF8D2C13D8EF537ACE59C06F0A32BABC35CBF6BFFC98D9B273E8EAB603D8CDC3C83CF813AD7733B04BB54B879E25FF92ACC67B2281CE691A2A7E13950A11E64FCA8D00A3B71D73FC07565902C1D522567CA73720ECE93AF005B388BE51CF48399EB47299F8B233F80BE0FF510DC07F81A9F96CBB9046ACE4EF0A2EE4371A3DD40AE408BA5030650BC4A15FA002568B760AE36F7343420AF0AFD5EE4ADCACE514B9E820398DF82F60A3E67365EF859BDE1DCBBB1D6AB9C866DA8B7A3DE8EFD0630BE15FDB0DC018DF2B9CC25B483687BF16C9EE071CC6F415DC7E7C09A1EECB30FEB781F9B30E75A30BFF37ECE4AE0B3412073ED7FAA7B0AD6A1AC79D0FDF03BF21FB173F0EA835E47404040404040E08B09E9BB990B4BDDC3BD82FCEDE1E95540404060292141E682154505F1DE14101010101010101010101010101010101010101010101010787020BF82BD778BCBE732973EEF5E04041E1648CF2F750702020202025F14C81DD04818545A4640B54C824AA6609DE50CEA0EB0110B1493206C232F41B9F22A142AA7A110ED6DE43D289607A154094039F93114F35CCE14D63C9DB94EFE0E9B955B50CCE7262730F71DF4FF0855E49B5044364091115F097694A2A5DDF9E7077E564BDD83C0C30FF20D68B5FE1AB62C751F020202020202020F1594AC548264F81FA28796F40F20F03AFA2D40D12A8447A1161AD0EB845ED809119880A3300567E1E7D24A79ADFC678B9FE651176DADF9DD4D92C9601D457E3D3421BF0BFA911F87C39FC2874C2673E5AE9F11FCBC647DE3B6748BBDFF87CB2FBCFFF2BB1B7066E93E7647000ABE0A1761B6E068C1D7B0D472A75C9265FC21FE093E26159283461EE4DB011CAA7341B2CA54CB6B6AEBEA1B1ABD4DCD2DAD2B56FADA60F59AB5ED1DB06E8E160C851FDBFCF8969E27B66EDBDEDBD73F003B76EE8A440176DF47DFF704C518138B621767EF4AFD131FFE62DAFF87CFD51FD831E8DFB471C3FA759D1DED6B57AF6AF3AD5CD1DAD2DCE46D6CA8AFABAD59AE3DEAA1D58F5455BA2BCACB4A4B9615BB8A9CAAA3B0C09E6FCBB3E65A7288224BD014D2C231CA6A638CD46A9B3737735F8B6320BE2010631443E1C51C4663068D2E66FA91B9F7134CBFC9F4CF332595AE87F5CD4D34A451F6FBA046D3D2505F04EDD3412D4AD955C3DE6AD8670CBB006D8F070B68A86C3C489914A321163E3AAE8762419C2E916FEBD6BAC76CCD4D90B0E5A3998F162BD5261252E946C930E4D2506742066B0136C52AB46088956B41DE01536A42F151D6DB170905DD1E4FB4B98949DD23DA30032DC01C5E8302DDC632CCD2CD728D65E87EBE1B3849134DB3FAA9B40AC331AF7D541B8DEF8E30251EE56B38BDB86E90957EFD9DB23B2E4E5ED41D797661D6ADE8A1B2FD94BBBAFE2C6567FB220BB31E3E46A33807D6CA35E1981EC6A54FE121F60C505C4D7E261A61D233B824E53BE1BB32F737A68578247680B23C2DA08DEB0762F8682A7406FD539E6445857F2673192A42541F8C681EB6C9AD45E3C1CA4431E8FD53A9723F2D5F9C696E4AA84EF36013858EAC612F58688CCDE70CCBA073ABA77FFE6425DE91F6385E084647287612D1704FED7C186B077DA41D6988A884556C149FC87E96D71DD3D54E1EE7F52CA746D5A87E03F00668573F581C896723961AF5067093DF93F9AB86F9399B79BDACB1915F91DC6E7CA6D8E346C35FDDDC74342DBFA84DA814151E1FF4E2D9C6A39DAD78FC1E0F7FC027D37E1846874DF7454C9FC2B03B09FE566F94C9319E999DCB2CDBC133D37399F9F2988637F917C67FFE3266AD9DFF73A825AED07827934AFE4B7ACCCCF70C683D7D43111AD263D9B3ED195CE499F9F6F95CD662AEEE88E296B396EC568C2C5ECADDF364EE44EC8CD4E09FC5B8D4A3E95C2BDE4A2322D13053639BCD316AF378EEB1289DB9CEAB0C75A72CDB26EBF42EF6D72DF217B567D7156C98D4CA3D8343BA6E5B940BE31B48D7C31A0DEB313D9ECE4C0F6B54D5F419A54EA9D32742B1B9279ACE5C38E966E15351DCC4B8D489B755864042938EF725FCD2F181A1C88C8AEFFCE38391A42CC9DDB14034B11C7391198A2F5D232AF3280F728772077A24BCE849D96AF0DD337E8069234B8C80E18FA4253062D6B998042369D98CA9E642B5C6427EFC8E1E491333E39F63138C59CDD8B4C9AECFB2AD985179E602E04B1D8CA409FED6E81E8C2CBC0FC63F59B419BAEC304896C9CFE3577C3559869F62E282F568BB5296AA6A9A26F694BDD0C775D255EA4B93FC543DAD7674A9A408A6516470E0B809650F8A628C12F8495172B2CD9F4675D8545F36D501530DB6F95F41E21668CBCC92A25469998F875336BB6F9A6B6B1EF79DC9A1367F571E71E29733E73961C0D4C9DE3623BD95CFE284C7CC682A1832AB0266786396DCD956DDB51C7D8AE2479940398F721DC582DD3BA115E50C4A0685181EE71D43F916CA5994CB9C6BCC666D7374B9898A19D5D8BB8A27A5628D8A7B8F913CDC3B334607B1E2A958613BCA8B241708B125E160F50C4EA2A44246A74ACADB62E8647D83CF48242B2A7DBFC46FE4EF431D5463404A96B88D0C240381ACB1A6DD34528DCDBEB7BA6CF86BE01A8A4C8048F8E3C4A84AD5B7F8AE5F445F526E8343927854B995528B7135E5E394C3E5F377A9CA47D08B22035312308B22C393CA0D38862223FD7CB279255F48399FB215FA54E45F038A328DA2C0591C25C3F7A370FEB594AB844FFF5ED2E134EADE4AAE58651A29B5CCD7DB55ACFC15FBF9ADF23A6850ADBC8DFA11D49750E3C5535E537EF36FB6AB0538AAAB0C9FC7661FD9DCEC66C3AB84E42CBB4BBADC00893784406A37772150EA762140D4A42049B12ECF316951674024810E539DB125238E6DA89214AA20386573B6039B8660661C1D1FC384719C49284AE280A3459A44543A526BFCCE49AC3AD364BEEFFBCF39FFF9CF7F1EF7DEB3C4D079BE91F1F9AD0E8C7706EE67F801DCD004FF3E3F482CE8397E981469B71B327F7A9C1B326A5AF15C7E961FD22EFBF9736439741FDF2B2D11ECE76FA8F3C8EF653C5E95DF3DE99F6D0DF077F95E320B5E77E03557F806F8174939A06692CD780CAB339EC7B3986616CB22902325DD9A6DFE6B894018EF87BC83CC41DB103F426643CFF3A372B618ECE7EF6BB7072A0AC63B8D13A32463E45B83710F3FAD4E08BF8F15BFAF47FB5BA674A545E2A5FC9BA4026058D4DBB06EC3F2F37158E3D8A6716CCD38B6661C598CE3D012FE1E5ADE834F39BF45DAF84DD20974C37620E4018915ECD346246AF5F1AFF14358097F3FD68EA2F670C693AF323B240385DAED907AC06B07F830D90830243FA29EC8D67EFEB29E4A67665E91EAF01BE9C9C3D27D757A2FD0F1A0DA8301DEC18FEA9538A257207D15459C7FFE82EE3C95C92BB0DAB1FB0D28B6828F03D78109C001B706CCA18134031CEEF5997C9FE5EBE74FEBCE4FCAFC4A31C0D763EAEBF56AAD97B3433AE727660C874F16955857954196E29D6739F21D4E592E36F5F304CECF46BE413E2B90FB2689B8AAE386CCCA1AABA29F6FD06BB1418AF074B52C7C441BEBA467FA5CADC9E416A84CEAB4639974E7EBEAB29947929B9959732D81735AA3675BA97F3F5563FBAAB135D5784E2AF56658197F00A7FF596EE91959A405E801D280037B6CC1DDC21E5B644CD7F8F80A4C7705990238F676059904F0AAE19F20B5C071E0C7C01890A36B5B0086FA0A8CD002EE04182296A3EC07DB400BD001F40083C024E022437C29C6590AEF0A70079006460107F66A09F25882B6000F920FDD8408D2CEBAEC1ADA4EDA693B6BE7ED8EF69C767F7B81DBAE5AB4C4B2F7285AA6280AAA6EF1B4793A3CBCC2637BEA3DDCEF097A58766A50BA6A2A2176C05953F94EF26EF2619207AA3B9D9D2E3614CFA305641498003819A27E94FC28F9ED17F9506C343611E343C9D1E444920FDD1ABD35718B0F2D1D5D3AB194DBC9A21AABBA99B6D2767A9C3A042DA7B574237534F356DECE8F7387E0E5BC1667C1D1E26DF376787985D7F6D67BB9DF1BF4B24E6F8F37ED1DF45EF7E6A49D83CEEBCE31E7A433A7DED9E26C7376383B9D3D4EA77095BB6A5DB6D331195FC36E62517BC06980910E70A7B6FCBA65107C5D973B75B905DCA6CB36B85E5B617085B2803062BD03BF0E7027A0FC54390CAE5065208CB7FB0DD4B5813B01C66ED80B4215113BC2FC91608491089D8CD0EB91B1084B4706236C305EC346749623C872446739829E237AEC11C485058491EDB0F61B86DFB0F61B869FB23EAEAE05DCA62D1B5CAFAD30B842596C5886AB7DF1B9EC35446C067703A30027E5E05AA0559784F260AF816D7632F3E8127CF0D949598A772424342D25D3B2404BE691F95673DC870B4A37300A70A24A02A855A5A941D625EB946F977C7C5A6A2A47E3ABF01555A974918B00231BC1DDDA2A07D76AEBA2F6F17D544E83C7B4D506EEF9A85FB3B6949F00FED3DFC14EE2BF0B968F1D44ED41DBCBC89C39B8B1070ADC812C7B5BEE0E882C7B4B46FD90CCB44825F142C6B1FE061DD7FCA6E66ECDDFD6FC59CD3EDB1B36FE11367E1A36CE868D782EFB1489A07A52F3BB9AF7D8F911E34F11E36711E34CC4381D31FAE96D1242C3427B7EC8F843C8F85DC8B81C32CE878C1321635BC8D814329E0AA950511224062B564CB76B5E60CF0D1AFF0C1ABF0F1ABF0A1A3F0F1AAF078DA6A05113843BBD8F6FAA41BFABF915CD5597971B62B951BCDC789B616DE856E9239E7EC6E85662F05C69C644967BB4B08532B908B24026E3902299DC0C992F93CF430A65F284887B988FF6E2C222583EED752BCD93E611347BA7C52DCDED901C69AE1259FA2F6986211FC85431E4A14C95401EC8D472C8DF955CA17F25295C8105FD8B4C9D42787A97445558FA4752CA2E40B332590BEFCBD3A3D3B7488C2E42357EC2A92CE88FA489E4E83969462167A51981FC605ACE4853405E97A9659053327502F23D99BA033929A3FB54BC2E12D5715E25A55AF7CB64119A9F934915A14D26CB21AD325905D92B63D720BB65EC8EEABA93F6529C6E9A22A6CEF4199932D1DC3C3391CF91A86EDE46AA74E42764522DC93A15246ED0B53313A9A36BD4BD8FAEA6BD3A8A2DCD0AB8C5A4590A797C7AE53E2953659095328A35A6D5327A0A2BB7626680C56A7FAED008D25081C2D2BC002721538B212532B51652A47A22A9C299510324A6932A90A6F2F24B3328AE522F49E988B9A4949EBC243E44DC0F6259FA19291EDA593795E2FD28E492B897DC21FE9CCCE2D62BEEE231BE70498CC2F5560CA6ED15BF35EF889BA990F8A5090FBB48FCC25C267E527A4064A3FD22932C11BD482C9DDA212EA67484374BD14D8A73D12CA3E8DD937A4ABC6A9689574AB32A876FC1F9453506021D330F88A3A547C4977114BE94FC86D86F168BB6E876B127AA069A2B769B9BC52E4C6427FA7C21B5533C639E102D553AE3EDE635B1A54ACF2191D2337A32A61BD6A7368B75C8000DB5AA01193C867369A1EBB2AA7EB546B8ADACC95C139FAEBEC2F025A61DC0F3F632D780EBB06B87ABC1B51ADF9C475D8B5C0B5D25AE59EE80DBEFCE77E7B973DD6EB7D3ED7033377EEBB159D9A931BB8CE00D36CBE957E274287668DBCF1483D4BD845137C38FAD74214FB0C496D5E9EAB244D635B539BDB22C9176D76F6DECA5F4E5269A480F7E9E247604D30FB684B33477D3D3E99CF06A9A0E2448A261F53C38A7D9D7F1DBB5A1314BA7548F6345E9C09AC63E42E992632F15295D77ECA5A62632E72BB5F36A03B18255EBEA3E865A66786D5DD97FFFE69595FD5FA938FD9DC496C6F4F9E2A6B4A58CA9E2A6447AF196E0B6C63EB68FED595BD7C7F62A696AECA3BBD8BEB59B553DDD55D704B7C7B41B89B1BD702349257063DB484CB9A17EDBFFB8D15E54D7F5C6FE4D7CB547D77865F1DF3EE7FB6EBC1A8CC1251E894BA42E93D633C60A79DC108F447469251E4B8818F1C86A7A239E83049D129DEA50A5B4433DC6543B491A6F8314539D36881AC6C28A780E4559B3AA9D51B9DFFCBEEF9AA6B3C65FF3CF9C9DB3EEB7CEDE67BFF73E3BFD8344C3A5CC2662D10C7788460789127F4CA48B25D1214AD4C50ED17B41815DA80705C6D93F2433A7A38B23B08B39DD2173DB64659191E43439D22629EB1E4982B2C8EE0E7A441D3A2A88FE2888FEC846EF15A9C3F78A0C6A1B85484742A48A228DF7FFB8B213FE874B521E5B909B9E94ED49CAF4246573679616174C71972E9A181E5E965B6023C24B7564E6C4AC29F6EF84ECD2024FB6AF34D7E30B2F8B4D7F0A3ADD46C77A7C65484F1A995E961E97EDFB38362E36C933C197519E5A1893F71FB296FD202BA6F029CC0A6D6631B6ACD4BCA7A0F36C74AA2D2BCF969567CB4A8D4B75640D7D214186A6A597D5434246E2D8E06FB96AD880D59219169191D0A2C9CBFD9DD2E917E15E10B6DF806C47436F4669234F42E933DC36AA5B7CB7781BC592B651A13C6EFC04E55ED02F226CBF6C7F826AC2E3A69E04E4BB93727CFCF373E5E7CFE4A28FFDFEA0AFDD4144BE37C9C193209F5FF9CE2225BFEDED774E9FE0F331B36E79BD415AF8BD89E965292949EE1C5F1807F9727BF6F666F8E1F506057ABDA04C5AED0CFB2D9C61BFA1AB458FB32937521EA6E80A67CAAFE2AE71A6FC0A4EF855DC359CF2DBE98AFE55FD6BFAEB8A94AA941AD2565755D754EB8A6E55DD6ABAE93E4F34B045650835AC83995EFF4CFBD82B8EB58EDDB622549A1FB6D5FF7683DF41E43B8EE10A9E3B57BD64E4FDE1BAB7EEC31F44CE74AE044FFD75394C84CD3E7FA6F7BF57F094CCE97BAFD7FC35DA9BC39CDD46AF4618605DE1BECE7D2B30C47A6C4E832730D5AAD1CDD8B23B06F793D5094B38ECDDC21A1CC6387CC1D931497E867418E2462B36F7BE184A17B684C927368A93E350A4A139FBFD0D790625781E5FC94014F2811E8E0D9C0D53D102F178131B6590751B85382339D8C1DBDB250E9D314C92ADCB1881346B0F6500FDF036DE91503E58C3A48178AC6A72F0E357D88F73B0301A6BCD8DE4928617906BEDC1589C96D132C66A83C1C8C502ACC5261CC475794D2A0CD3CA442F4CC42B1222CD244A1759DB11639EAFBFCB3A6655A109E93791EB5DE535065A5F230EB70CB1A630459AA1072117EF63372E895B7AE9448472041D4B5FCC47898EA28EC95846DBF6CB3C29D1A1D6165AD3075958C8B49A2D152AC23C6F3EB0E6E227B4AF27355D8E2DF804477187DC06CA483D2330C04AE53B590F5E2451D212BC8A3FD0734708C7A4B144C86072FE44AAE58ACED537C9F977B8876FF10F89921C59A006A822B37B6DA1B50B91B4308E3C066314A6E34389943819C3BB1BD42CB5402DD4BBF52523CAB86FC55847E14234698BF001ED3A8933F82BE3355052E49C5AA0CBCD57AD79D4371A5368C5126CC53E3C1453EA4B23F9A9844B0FE943CBE649855C516D9547A5EB89BAC45C61CDB15E470473651CB279732A166329F6E014AEE20EEE496BDE8CE6CD019226AFCB1B724C9DD2A3F458BDC68833D6183B8C23C663B3A97924703A5043AFDB7C9E430A611C26632E7DBD97701417444B98B423A75819424EE365B2CC9795F2966C966DB25B8E4B95DC96FBF24FE5562BD46A7540FD499D5255BAADEEA27DFAB7BAD288302E18DF874CA86D1B381CB86F35B4BC560F6BA5B5C1BA68DD73A2D086193F0089CCAE695844EB57E22DBC4B9FEFC4099C65DE5D76E03A1E3006DF8B8BD9D48A1A75108F7496AEB46E94A4CB2C592EAB648B7C2A57E4BA3C56508D54074217D55B0D51635591BAAB1EEB06DAA3E3F56CFDB6FE523F32E698DD093BCC5DE603D7F5904EF52A1FAFAFAD0E2090135813586FF5622EBA9879CD58733D91C09C1BC2284F421EE1151460167D34971EDFC0CC29C1C73880CF5049DF9FC2455C72F4B5E13623F10D6A1110C5789A528F10D4FD39462691D99229D98C6D10E649912C93B584F5F29E6CA27F4FCB9772462ECB3579489BA0BAA978358816A5A9316A1C61BCCA5285AA58ED249C54E7D44575553DD24D7453DD5E77D649FA17FA35BD5C97EA9DFA2FFAAC1169C41BC9C634E3B8719A96279B83CDF16696596C6E32379B47CCCFCDEBA6E55AE57ADFB5D7752BA44148EF90348EA6CB427E1F7220E4528855AF33F32985DA3F8BBAB54AC618D16AA5586A2FED3EA4F2F5176AB5ECF81105CCE5D46012C6ABBDFAA07A77FE4A7D557FA88A00C3E7A063D9C52AF147549A678CE6E62D1C57ADF135FBE16A3D411D52EB945B7AEB7EC652A3925D670EF5DCAC2EAB1055428A3B8CC678BC28ADF077E325DCA7FF4F99CBE9D381AA5A76A84FD51066F2796C5107B00E1B912D7DA8DD24ECC223BC29FB74B8EC66DE2D4415EEA2A64E5B23BA36410D70B95581EBE78CD03E19611D57CF5A7758F55764292EEA47CCFD972455A2B10DD718F5B3D253DA1B01230CA7D9F9DA613DB3F66F28670D7E6E7464053DC43EDD13A38D1AC63CBAF6CF019F99AF17CBB72A9EE16CE974EEE17637660F5ECB5E65F7D150943013D8459C8ABE8313D2815E3CE3BA8077F006F6EBE6E8A4B7AA45CAD29F19E1F80D6AF4304AFD25FB531BE9494E3390433BC2AD9B812DE4301531889189321A3E6292D1CE9A41CDB7B117C55963AD756686E9C5491926CD7198DDCB4D2FAE31EB07EE917227EBF02292A518E58149A8E0BBE2964ED29DD974CF2C30579A1F983BCD43E609D7F398CDAA5DCF285EC5377C35C2258BBEF80ADF31D713583D5D593FF1D422996FD87495A10F22515AE365F6C028F6ED04FA603423E9279722AC603D6DE51B72120FA409FFEB3D84F3AC9C96ACF32CCAAF473E43F122A3EEC73676C7C552CE934968872EF4D323099518954F79769F5DC33E5B419D2EE1263B87E5E8D555FA898FD1CBC277762D53426FA4F17F0258BBD1972FA54F57E2063AF2754D608D6EE1BD4CE64628DAA2AF794D14BA0652AD1895A30F4A0BBE86A1CCAA917CD963258F5A34A61DB5682EC3D12B3088DC76B097A5995BFFC55EB9C73675DD71FCFCCEBDBE377EDF6B3B7E254EFC48EC5027B19D380F2726BE499C0493263C06092F274BDB44940E05180A82AD53406DB31714C21A41270DCA4A0B541529D061A276B024DBAA0D95B06A5B47112A1305ED0F4F6344ED18B1D9B94E0AD9B46968D2B44EBAFEF89E73EEF37CCFEF7CCFEF90DDD74B76866C9C4D77CA5612DD57C84EF63EDA727F15BCCC46A9DF527FA23711DB55CEF143949E05CB099DF8B50CB7284CB2E93F67C37C684156C520E63D3691F56DF95EC51E25A5DCA8DAA6FA8BFACF9AEBDADBDC3037CC8FE876EA15FA29C301C381EC897FC4B4C8EC335FB35CB78EE6BC90FB519E26EF447E8FFD9023CF79C2F5C7824B85538553EE23450D458905671F7B43C47BECFF86897F4DF16049CB2373EA01BF939090907804EEFC3728554B487C01F194D697AE9590909090909090909090909078147C7DF3F895C4178A2B121212FF01BFF71F0FBC5AB6AF3C108C04DFAB68A83852F159E5739577ABFAABEE5457570F871A42FB6AB26AB6D65CAA2D239C0E77847FB450B77070E160DD8ABAABFF13EE48FC3B22BF14B649484848484848483C0A08432E42B25C1942146251C3190C130C9BA0B2043D92D1131452B0F404204B16239BC0D43B508FE450081DC8ECE53E0DA7C2EDDC74B82D154611D2E6664811F03B78075F480AC8A5D18C9DBA3023C8D03D64A72F2084D1C9FB9FC08CEC19C4A15CF48D77F128B2212BDE8FF2F0F0699B1C5082B208563E6652EECD3B9C87F34C26ABCA10B322C1921F44E30089FB17041D69834A6BCDB7626BB15695AFC2AA04E805F97906188BEDC34B662FD1146F4BC66FC47521AF2FE9E592ED5C536FF4661C45DA523723013F34479BA3B128B8DC1E8FBB2258595E66CC36B02C438935E3728AD7E02BC5AC27E85BB77851775945AEB3B1BBBBB1B1BB0BCE6D39726572655B5777ECF1A92B5BD397BBA3993B5F26231BC597A931323215B2A06E210767E90C419C95630B2250D06A8D8947C0321AA3066B12B043B0180C2CF043FDA643266CB2E62886EC34D016EB43F9EDDCA7F1B614892B97DCCC8742C0EB4221F120F2BD5E70510F85FFFD493CB0DED0B9B0B9DD0C8365BDE6D5752DAD567C1976B686EA3AD7569474A577C2E02A7FCDAAAE806BBD38E3CBEF5F679611D57EB4102D861AE16B0799977246A2471B5F8D9E894E96B145AA63B9F8EDE878F4A74DD476FDAE28AE667AB5035A2A02115C43533E9FCFEF8951452A5F89AF94F2211FF830E55D1060020C5367B0190C065B608197A155B5B6BA9881F6DA197293AE961B627536DAD58CC7C13F0E08F1D5C75CD09CC033A74C4A4502A70439AFF477A37E12D604E5100C2ADC0DFD80A15835AE55E7ABB1FABCD53D86EFA220F1839CC439125C12C4C1049C1794BEDA48ED925A2ABF166A13F8AF829AB31FB663FB296D437E036E48E07B6F5B3B5AFBBF25BA37199F8E27E3A4F026B9543C351D0F93C01327274574215F38C925799D29241EF079634853EAD53CCB4D0E692627C954C4677F281E0747B606B34693B1BCACCAC4B0A28D3CCC9C99AADC0F2688218F545604DD9EC24A5279DC2E27936D3052E405529177C9B3F41E38DCBC74706B9F50E25D1C5EB03CDEB2B67D77EFB357B7FDF8E3DF5CB45AAF9FDC7DF4F8D9CD57F6D754A7B73CBDA8D613F235DACF2C75F836BEDCE6EEAEBE4D793D8AC8CD3DEB0A2C3DC657A2E58DEB56B4BEFF9D916BCBEABF5E73F8D7BBBB36BFD2F8F39B47071EAB659EF4AC8E6C6A2B5F1C096C4A7FE0745737758DF5391C9F21046879BA0F0F1357E8D052A1684873568BABE803F87BF263F8A85C06E388528DABF56A958A3CEB3768D97CD6C7526C02BF24C8050EB80E7DFF881860125D9223384226A8013F8A431CB2C9585986E77426A329DB8D780EE1E1F581A8DBDFD91A8CDF4EBF05EDB2674AA3F56B769F4CFF2CFD613AD1DB5C51B60CEE40140448126D16A26D7546DB72C159490FC9BEA94D68E9117C50FE1A3E2EA7893A3D51477219C7DAE754F14B44550604A052A9FDFAE5E2F44F67846544CE53A7AFA8AC22F01CF6B83D1546519D657DA0D1332B0E96A4DF4AF79536D5AFF9EE28D44011B464C4A5D5E977D23F49EBC5C835A40FC2BB508E4CA84AE0EF6260338A2EEA622A05DD9A9D80664109E5F95AD0D69BDFDC2DCA884FA79244C07412F8CCE21625CC39839DB3CFAC71FAB63ECD9224A5B2796B3B9F6AE9D8F166FA6071D9A12FF1F22C965F57D7F0D4F35B5FBC262A28837EBC1DD791B56D1554F82384AC32B0D06267EDDC0DEE26F2B591B182A3C281B7A7CEE116E8BF24BEB5E6FE2D781D8248899C67508C5152624E55DAE57E39965B54E28269E766485A4262A4607EA244CD3D4F3435F5F440305335353D319BE76FC10C354EBE674265E790852C548B4E1F64628855C5744A2D1593179FCF866C8BF941CA9B4E7DEE13F0CEEF413FBFB78E4C173D3DD1B95EA9F19ED95E7B525BE6F7CF913D6D54B61E395001F885FD4E4EA98BF47103DC36D710F782EB84FA2CC78EA84FAB3114B83072BA5C0E85466953981C669B49290739CEB2C98D7CB6CD08050AE4347ED5A5E5EC2EE4E01CD8E1C28E129E33F03CE7C22E072ED2680D1A8D160F6840A3D8C18383E7B4B4D1E5E0359806934BEB2C2822F105B8C1099C9632198D0A853C4B6B04E318EC422E28155C7685C5EFDEE41E741F764FB93F7633859CDBEE16DC4BC995BDEE5137FBE24612A0CD5C7CDA626D4B11B79A2319EB46C25692B6C88ECB3FCC5064C78B675254562645959AC5467CD24BAC45FE66C42581BB305BC6E79FB05C38CC86C3732BD40B0E311D89EBD341CC5805E5609C3D21B92DE34D8F87A2A815694728B73467437A61ACAB093ED1C31F9A4B9C75A94D394BEC4606E76EF8C514EC7AAEC11BCAE1B20A0B954F7E9FAEB977EC070BF2658585462E4FA79737DC810FD225C4A75E32571AD9E3280715A000AC14F61D3081AE3767000FF85F37BF513C9637567C91BD5A72D7A7F81BD7551BDBC479C7EFB9F3DDF9CE6F77F69DEDD83EFB726F8E7DB6E3D8099010129714CA4026647422C992B26E2B6FA1905006049848DB34344C6D182005101D5D572008A9B46290B0566550FAB2651B1FD6AEEB3E742075D9901A69D318AD56C5EC79CE6663BDE8FECF8B2FF7E1FFBBDFCB5303168065E01BE16FE15DE127F0117C383B01DE4F7D989A89FE4DB91BFD52F932CB2FB31B7A44D3E26E596214C5234B82A266F528A16119395B97C4F4A806F30E234432BACE085A4614053C99B1DB193B2673D0293EAD7AC96B0BE5B53A4F3C16C7E3698FBB2A979F02B60BD58B3A83A6B912C59DDECFA03B17DB3A2F61192E83678AB77BC36F648AB35D77909640C34637F4EADAD92A54E1ED45D68D2C1C62045F4273EE66D4EDB6C142CE4C57ABFE20490774C508E89491D255BF5C0B14544C3A530BAA831A2A2ADC53D364B216C34CAE194600B3723D0D2F045B5B4F67C1BB3B7B3B8D1B2933DBA874A546527FA029F453172C1041180FA8EAEA861CD417BEDE30D486EA9C1F518B443B7083E6795A401E64AD88B1AB2BFBF78C976ECEB53FD6160E3FDC8B1FB87DADFFC5B95B2FEE5FF6C8F021307FDEAAFDCB3A8FE337D2856FFFF8D8F7077575C116A27F4BA3A2AF3ED5FBDD63DEC2F6EEEEA79AC1DC895211BADA23FB57AF1D6F464AD371EF16B906EAB706A4CB98FFDED00586AD8F4C9547AA32BAE058E88213678809CFF3154323FE1F85C6C2A3117B1FDFE71DE407BDA3FC196AC2752AF07E603ACC527ECC68F33F1419F23F1718090F47266D6F46D95A63436C27B5C3B5233CE2FB85879EEFE6BD9A8475E3128C914028C069F559DEEB263749847B93C880B5B53CE043FD0630BCFA96CB208721596AEB2C301E36C6E26CB1AAEA0E02FA427936DBB592EB8509ED334BB220B93E870ACE4119C7B85FD56557AC1E7C236787F06AFE08E5724260ED0CCDE054D870F9591DA322B038826E1D6342A40ECA60261194A07700EB1DB0B005BC6AA07880A8E8B5928188F2848637D47BB57C2E606D916BE2A9BF1FDDF7615D6BCFF513431FEDD8F6C5A94F4AE727A741D7B5B1933D55722D4DF6959253D70FED18BF7CA9F4D1B1FED11FECEC7B0D46AD6BA0E7972D5A6D1E696518F26FC0E29F091C859ED0106CBC8A0A878A89CA7ADF86E07AFD7862AA865CCF6F848B71FEA8FF551FF53D372D4B98A2D865C9ADA8918CC78D2B0DE13066F7A6231E2926E1528B3D4B835534A07F985A74B1ACF4038842CD45D45C0E333803378A98C009598110E6C196C2265F328A590158ABD9AE0AA5664DB3DCD8C7506397AB2617F2FA781F4ED5C413F1649CA0FEB7C229BF181083629568A334DDE40C1D24515143B0C47D11544CB867EAA2A23F40A724BACA6C42CB7C036A71852C882D01BF571460CAA3548217CA510F460528874638BDB0D5C3F8DB1AD3F8DA7F1EB9F866CFA12B07163DDBCDF9C2F9339DBBBEF9D0BA65BA2E8B1B89BD1BEAE3FAE28ED2D48DB17FBCB436E4B4DDFBEAD3470DD6B3ED38CC37E489DDA918644802C36CFF8678D481958559BFAD8AC1E57C36DF9F3F989F087C2C7C1C98097C116006D9EDE2DECC2871482047D9A3C451F6B038414CB0942C2C110BF955F94182640996C5F3303DB71EB19D605EB5BDC69C164827C0E80EA773DA2ED1B22C0515C5ECA8ABBB95924CAA03806952A2AA6529A1A880C29CB40B13391117FDA620FA89001DF05FF0668275350990713A83093C68A7680FDD4EE3ADB08CD1E7E91BF49F69CA436FA5713A973F6F5E31F15AB3D56C37D79A5BCD7DE69879D2B49BCF72FE7EFF413FE10F15F2302795E37C4BB55C95AB7C1ED6C7512157EF00D4CCDE816DB53019B42241E5E0DF6C73C5EF1A7B794B584D48BCCF316EAE32DC5F121C59B13473C00AE903804780E6793583AB3C8ADBD69228FB9A0534C212418DB807677826FCF476CE309CC5758FFBEA9B3ADEFE4B4E5FF4D5E6F4422DE476906CD8589CB66D35A48DDF5970DC569AFBE32B3F996BDA7E245F7AA63F27BFFEF352872EBA95E03A626F8FA8C28FAEB4F5F050D40BF1CD407C4F437C53A0BA50A46D0C9B2214C772074991140BC940183683351C86B39D58CAB63BD6B13BD811D6BD3B713073D176917DCFF61E3B639B61EF927759D66DD99B244BA2A2181DA9D4145E53D814970C8F1DD811C88C64C720F53A707C9A92E8A82C698A6AA7690377B6BBF076605CD1811E7A3D0332187079DC3178366D913C580C6A424B342A55A5053155A3E135A0C6E97269825B6A441B3A56A36BB8684F67DE02380C588B000DB5D28408A11313D77CA799B74E4FD602588872F02400516D2EE30AD733DC8CF55005AB7FF57E6D445C475A5886CCC20C71B00C9AF80060C6FFC1958F776F6B77AAAAEF6C5F3C00C938B7B00C1522A66D57C2FDD493CDAF40A07E3F6FE8C9B93557F7941E4774BC8F129A97F68C0E873D10A3D5F76E521AB919CB83CD053FCB911AA1BB13BB62CFC786B561FD85C4F34956AD7895F36BDE9544DED506271BE80D8E9D8E9DDA65E26DDB1435A94D1A9349F6617569A290DC9F184992C78CF1E419EA67F484E35D7D3A412F77070B61AEB53F08A21F48C11E25300533B30077F60500FF811450D4FC03F6A560DDD9B3663406B8982B100C2A648349B81A14069E58789C6F01D15003FA7FC6C9D537786BAAEA1BDE02AB21565BC04DCC4A3128BD7898184CF7567A612C4333EF36172BC721048EB7B111C01BE3EE7B9B897411DE90544888972021CEC949CAE380EDD7E31A14615A77AA8C8EB9ABB9C5408E79382A09576CDCA5631ED9B518B3272CBF83728B22ACE57A96DE0E58828BE0560D0D9A1E7EDFF3EE230CBD0F1A214FD9E0390042DDC061488ECB1EF89CDE56BA73F2E8AF1FEDF9ED0B75EBE7F997D4A9F8E1150B39E699D25FC7AFDE7B67FE52002DEF898ED4BBDE48568086A85CFFCDB9D2EF7EFA4EE94F0744018456D51ABA4EC634DFF2D24CD3C28DE7FA0E9C0339709AB3AF4834A2C402F3292540BEB681D682B74D81E700981425BBA2040B5E476B10F5D93D3FD28A05B9E0CB4102A9EA14FEC9A49293A5A4A234A19F7DF0B9A6027CC6D3146B3ADF442C96A526F8CC2585466FA0FFFB0678667D9926802CD1E80DFFE1BB5A63E3B8AAF03D775FB38FD9991DAFF739B33B33EBD9B53DF1AE89BDEBDDECDA5E25D9561149E3D234AF66491AA54DFA20D85189512B57060146A8A2A61535E9032C6803527F344D42EB208A2CE19220215ADA2257A8A8293F2842A4CA8F281462AF397776DD9890607BEF3D77673C9A7BEEF9BEF37DA980CA8EBD6BF5095DD613BA925DAF74D952C8D2784FF58BA93E5529E9295DEBDC441874875150BBBABBBA229130DD502A719C8B4B918DE246BA7168BDD007F8B71F797782D40ED468B536529BAD9DAAD96BAA0049A030142022E0DF8808E2C4E6C1E3AD7E7DACD5B0EB6357571764D584B0512A21432F57ACDA305BE39AD02262917131DC0AC0EDCCEDB203D6FEE79B1BFF83F6DE886CFA268B052F7C18DA5C5A4717D65552B862F172A519D3271AFB6E847A336E4CC2E4F5D5D237AEC7F02269F662FA099E7D927CA7DAA3B103F0A80AD5F598AA48BA1E571554E55E5509E8292940297031219E8CD3F890D7C34E2D725B6AF8A2077A3D55CFA867DE63DF8F03F544558D5D8CC795FE8B1A8C6AF31AEDD5AADA7E6D523B850BA795774CB469E5DE5CCDF730C30BE345942DB74E204B17FDE466E9C1B41937C980B567DC6968E523DB15DC6937D955F5343799C6EDCDD1F7AA91547BBB40816A79C19D2686685063C8F3232F78E7E0E099750AD63BDC7F46DA6E3E79CEA217F1EAA5524E449EB7DE1BA51B9289F5E26B5F9369D974BEBFD0B7DEF22596DFECFBCC9A408AFE615DA5232A78EF38FFC4CBEFEE1C1ABCCB651F88A507CD6218F7E1587DF9E523BF7E75FCEC976EDB7547496EF7DC1988B5C9EBEAEFD13FB22D353BAD7304F754867BCF91C195F9337A67FF20CBFDF362A09F030FEFCD79CB5B600B7F943F4EA6C8B3F02C3F3B38076FF8E6F8D7CBA7069788348BDD2D1BCE966188BF2BB7A3FC201CCE72C45F2E0B8250CE66733D02B6579EB37A6B48D77B5425BD4F1F2817950127A096429A68DF974AAA8AA1A78402147279A5703E07B9EC9B65C8760AE5203E0508212276DC1E3F1FF4FB795246F3337F060BA7CC5EB4C8821CEA6A1E0837287E1686060A698386DA5D4ECE19AB0EC2608F2026452A0E256713908856067F497758BD39DAE4FBB15559F557ACA44A857D9A222A5C324D6E6A5BD6ACFB27C405FB5436D28CEA112222EBCF2380ADB9B96A2DEA2257E12A968DB50416A36F6016930195D9C70CB8AC4ABC49A1B60A00D85DE94C93D96D77C39FEFDB922F2F0F6DCAEC6BFC767D64F3E79777ACA9E09FD4B0807DF0CF07CCD02E1AB8FDCEA76CB5E5971FEF510DC39908753F0253DD8DEF3ED87F437507FD5AF470632FCCDCDD970E796D48F15DC7B126D2E87678AC09833C592D1C8471782C359AB14FA7A63B4E76D8AE837BABDE843576235B3CD54158D98F1A93C6ACE130E6E05C5554B54E8A98078E72C63BE40504CA2BD5D075F847D3BD996A6636631BDCCDD0DCD2B557AE2CA34E42D65CAE5CA957B0BF0658FE4D2B8975B0FD3F58872DB98AED8EEFBBB6754D6EDE2D5BE88EA4A207C61E9E7E20071F343A6E82F2D92325BF7BEB8BB34D4E731DC10C14607BF55842F44AC3DE04B8138F25686FB1561829FE945C200E432EC0381997C7956F9129794A39A1FC4CF9BBF26FC5375ABC58A44929D9960C8A1DA2E11024A14D08920E62B80BCEB5E498DDA0A4F55616931B180472AA92D751377DBBBA8928B28A95DF29C783B21C278502213D4A22A82809020545B62521460A79E49CB4A1C852802364A0181763101BF2BCE5FDD04BBDB1A2A56FE444BFF54245A6BADCEDA1FE6222D999CBB26B01762D7B314BE7B36F6769363A509C831D673464D73958F74D068ABA45AE880AF398C964291E5094C9D2888591264A98CF4090705359D381B0E018405860464CB3C5C7F5FA3194AAE82F1003B7EE5B90C2EE176E612254587BCAB6B761947636F96EB56DB178F9D3C8F26507BFABDEE8F5F7DCD1E9A578D1A4DDF07BDBE378AA5AE4BEA5AFAFE96A97AE99F6DF2DD50E85D70F1B0624FB73DE7B6C7B0FF7650CC684CACA478E193C730DC64E4B12F69F4F4FF3253655C77D2551960551561481DFC0644D9CC90E9D6E505C3A9320A1ADA878448D6AE83334510E83A028430482F85825AE9380E00750C21A2A0D17A1E11027B88176FA051EF6F3C04F8C20978B814E99C461240E24FE6584C784DE92166375A62698B2B8DA8C988958D51496C5B30CDE943F6B4ED92716087E19596525D39C122B130B53E202B053D8B4FB1C212BA7AA665B9E08A230408EA9A3DAA43AA97D8F4C0BD3EAB476969CD578BB6AD7BAED19AFDED61D738A732BF79C6ECBE37412F555DE8ECA2708A2380DB3F229F194CC11C66A486D7B36EDDBFD73910BC687F1D68B55B71419269CBF6D98CCAD5C6EAD84E0B030B7F2F119BC07E73F9DF68787C12A0F629A7B0098B174219AFDB43DC0CAA05919ACD96550CDE6A1415F48F58EC1FCCEB2A62F3DF4504D6D2447772BE6C621C7D6A5D7E9ED8F9A1BA8617853DB0F5C9BB13FB0F4E3AF7C010F78EFC3B6373A0A3A35B0778CE0E95E763C4478928097AB7D47C4236D3FF02C4A8BD1F763EFCB8BCAC792DB157125C234E20BC7C27246CCB465829D314F62126D4A980DED2DF322B466BE35730C568798BB6177011BA41978869E709EE09EF1CDF027E949DF79C779F76F944558E4796A77714EB7D31386300DFBC27C4871DF1FBD5FFEAA63DC773C7A5C99115E8BBCA62CC62F73DE9D7E7F9ED84279975BF2469347775BE580A6A41A2571114B645BD506B6584E1D56A92A4849894AE853987B1C637EA52AFCD70DD2B64BCD4B97F658E6E573BDCC9EDCC9EC490512A2A1A48369B7E14847639118750ABC64609EE206B47318859D18057C7E037899E2086D9E904162761C4CB382BFD641765BA7F9354094D7B11CCE724EA9E4985BB952F54A251A914A3EFCD0B995BF9D0E94D010FE0327075BF12537AE5EE54BC46CFDEC81D5084B0B3AD0AFB9A8A666D20191389039022293410305292FD2B42D0C9BE1FB33171A4F379EBAF043780E8ABFB877FBA3779F385CDB7DF0D0738EFDBEC6D1C63B8DC64263E9D305E0210B4F6FFDD5F38D0F1A2F9D7C647D15A27FC1EFBC4751C0927E42EC2F21FA6348D36F9D232AA2DF575219FAF7794BDBD33013B91ABEAAFE4BB7777332011FFA0F5D4717E2D4533CA3F2543C2B91ACFC1FBACB3EB689F38EE3F79C7D77767DF19D13DBF7E29CEF2D3EBF1CF6394D48E260924B81525220A14029E93CA05581F21EB402A563A9DA500A6503A48D52316DA3EA5E822AB6D216920C6D55576D55D7A9682B12FBA355B5A26D9A9A6ED252AA8ADAECF79C13DA6A5BAC7BEEC9C5B1E4DFF7F97D7F9F6F33DDD4480270F03AD2DF5F1F7F3CFEC3B82F7EC4B19095A8C7877C03C1F22C39C8AE6749F640CABA84488F41983A83546EE588EB152F67963D349CE2A7664302B4F1A36E5235A3B2284802499B51DD41AA0C8B116B719026241D82C0E9D1CE79B1B1827F99058D5BF03857D78029C1847D118C9473DB2D93CC26167DAD3AF0F50589C4C20A39805A6A2F1CDFF0373DB27F74F4497263EDE91D25239532BB76F876E1DDE5EF8F5E3244F254F50279E2D4B34771053135FC192A681279B4CFED5D2DEF964FC57C015334EF9617372F3636343F68308D0445D03CC5D3FEA2B329B137B1D778DA7C3BF17BF3B213782EFEAEFC997843BA21534E801D27AFBCE2D5D8DBE032C3C62DE152C330F41A206F1A51D33446CC674CD22472CD7AE271E39A316DF87863D0B86CF82E1BC81072CD8669A50A8971F417573001765BF285261049FBA3AE1B06805500A206A2DC204BE4F81C997B5F18F7916E9C6D49C15098D18C6507B14F17E64F78AC88B99D0795B007F3D5A90A5F9DCD7AFC94C72DA05879AA5A065BC6FE3CBCBB528A608FAE6093F6C851F4A62208A9A5E744E5584AB232A939D19C83D2322C763CEFA0AC6839849CE0CB76BDBBECBA9AB8B326880C1CCB105BB2036CA9596C8AF5A0BA8956E01DFF43EADBE35E7E60307D0A311DF9225E8AC09A6BA075B57F46F33DD7AF1DDFB6E89BE84E3791EDA8ADAEDDBDB6F4CC91811367C82DB5D1AFAABFF0E263271FE8516B73D7C6555F8ADC423E573DD77670EBE9EFE239BAE5E6077E1D9CB684F26E492CAEC9EED57D74180539C6A68B2227D879CEE6B311C7D0EC96391DB90E7B53F670F6706EAC7D3C37D9DE545288215241681C2D7163C410D7A176901D63AD403D439AA26A2A52C7E174DD991C22645E26E5B158D6E6021617E2B8E65033E7DFC3EDC99EE67E1C7A35F40647DB592EE437A9B9AD3E736E2C3880D6A19D68041D43145A4358BC455AE38877C38DF23C37D4D03E8F0BA800AAF0E815B5B520758FA3D24B339E7B6D0A7CD5BE0E0D79ADE2B927206965186B5B2A11FC4795E9A90AE2A7A6A7EA7B6FFB124D2E58759FABF9423E8E4C652D7B4BE8616E7FE851EEA9EC41FB7BDC8BA15F86DE0ABDC5351095E1B5186D87816D9B4C90CBF0625EFD158BFA2101A6F143C68CB4C5675AD54A17482F160ADE934EDFEBA1ACF2E1E8C6BD31C575CE7EBCF29EDAA76FBBBBEF2DAA7277632A35E7C6895D07DB368F4E3CBFE6E357EFE8710E25E46403B5B5563EFBCEF6C579D329E8AB1ED9BCF9A9B39FC82DD14C9624AE7EB87F45716845DFFD8FFF60DDF3D778B64F9B8F55ED87EE66A1BB35E2C509C280F825CAED0666C8797C63BB66B8D072AF19FE226C48F41EC37C0E1A8A9AC21B46505338A0DBF764F9F3A4A2327286D0489E0B10BB101639E71A40436A900CF648BC883471503C2EFA448D5791A60EAA23EA71D5AF4EA21C2192E75ED6F110E4AF4F5786CB3C5C382C54CA9E4756CB50F58F08BE0ADD52DF00747A912B0245850A9AFF059B1E849A118A6DD1962FB4D63D242CE8CE57BBF35E5278E070CF1AC1A296D64E8CECD41B6FFCE30B84F4C7BB579C443B71458A373FA05E808A1490CF3D237292418AB7A58D9CF998F9EDF077CC9F9B7F306F9A41781F49F87804B9D3B70B1076243E224C84DFCC5CCDFC3D13A6CC58983734DD325BF52183795DFFC4247F12BE1026DB028CA620C3503545348C9C565008A3258299D3140501C167B25B5A82C08CDA888AD6A93755523D502CBAC5C1E2AEE28F8A5431C0312A43323DD9EC600EE50E38332C8907CBEC7419AEB3E454DDB8EC594332F44C90BBCDB252E1542815708874A6C1E461B6E8C134EB109C010B2E71D9AEFF0F36A5E1DD08AE268CF5F40CD4CF984FDAAAE7B33AECD3B46900CB79AEC414C94BE6C03CA9F35BEB779C5E6629F97BD095E6D2D24843EFF49F7EB1FEC96DB27B2FB534A5777FA3BAF9C29EE50F9EBB4A66EF5FCE09A954A1A0ADAC56FFF9EE79C77D738C3CF548C940580B1EE8EEBCC7EECA0461C2A9EC965BDA2F9BA8CDFF6C8CE44DD425A092F0B030268C0BFEB820C4444912080A298404C61E0B2B0D6C20A4B0BA04F8EE8EDF3CEA76080CAD050806C88361F202B4A410A3683A2348B093620186F6B39404033816A028466F600998FA41C86DAF5DCC2F693705412626518110D0136EA3C6BAF06C3D8B58C930B7E9C7B67F11AE6C595A56AD8ACB173DB4F0AFB67790CB654852088C052CE6D0B2828DA7058503156C2A6FD8B2487870FFE5B552BF1DE2C3657CD59DE7A2A80522ED409940E8D86040244860368AD17810980833753D88C5A2713851DE3CC07A51E7FBBB732B6B79BDE6AC2A0D9047E2F769025F403A628B714DB517832CEC82DB276E4CFB3B7EB330984AC539A5B1756BB542AEDDDE2F270B6CC4CB5261708B09D0A3179D707FD7B8A4E12E7E49B25F3D843E9B4F673BB35DFD6813DA58D8AFEDD7F73927E78C6917C909EDD7FA6461B238D9FBAFBE48948F26A5A28F43C134E7A828E1571DBAE8A0D6A4AA855B939C16EEE50907F5F27498516829292BD2F1344A3B19255DEAED564A14F22B142122911714D1D2806DBB5A3B95AEA2CA117ECA5B2DA997E733C9D66832D98A9CA361E4F468E1A8061FDCEA68493E8C02D4AD9D25F5819CD4BAD2AF481BBEDA1384057795B45D4E92DC5257DA222591A6026EDF38BAF2725DDD1CA84B602DF185F004999EAE4E4F7B3213114F6050370CF256405EA002D186FBADBFFC7F8DBFBCA2AF3CE3E0072CAF520106004470A32A2F71BD616FC94010431A843054A76B14C71880B31564AEB4AF7E043C680063EC44C84ACF9E0E3C8B84A64E6FE8E056A6F1E9F16D407B2A031DD5AD030526DCF5DB772A4527D657FD746DDB827DA8A1F6D3D56CA2BD409E31BA0BCCA29F1DDBA176CF47FF9E775756DEEC4B54AF1C9947A752748BDC18FE0FD5651FDBC47DC6F1FBDD9D7D777E3DBFDEF92577B19D73723E9B4B7C3689716A5FC34B092410B64209C323DD064B9B75B1B3950D3A16C25B5BBAA1B51BA45D2A41D1A0153085150619A882690A1A12DACCFEA0D9A40D3445D3C69652D480A6B521FBFDCEA1DAFEC83D97D3CF67F9799ECFF37CBFC2178034F72DB92E6D854FECB1456F8095E0D35D8E4898922429EC67FBE7B1F19717871605A18F0B49FC2EA43037CF7F42FC85F80DD682B5E39B759F996573643D9B4BEBEDCB32AF657F4C8D6589025A49CFAECE5EC881EF53275367DA2FA6AEA5A6221FA6A6B27F4B31596A39B5CAB38AEBCC6EE4B6D187B1B1EC0970015CA06D1A057617DE227F9A7ABB85C40A3D85AFFAFB0A43DC11DF3838B1E40AB853B0D0FE9EC2B7F3C44A1AF7B97D781E7D4B1B97BB9707698D66684A493629494949CAEDDA69EDB24690DA135AB7B64BFBA17654FBB9F681F63BEDCFDA8C662D6B40CB43C378431F65187CBD978ED05BE9176912A7F37417BD937E953E4A9FA47F4BFF9166AC74882ED384D74D13BC3D2E2AF0DDF23635BF124F8F622555C5795D56324E5EE4B7F083FC517E9CBFC253B7F97FF19FC1EDC9EB0E36C3E37005589D4931A9268B4932B94C5EEA94440997EE6218C3A3AF57992233CC5C61C87A18708C61E1169E009775562FEC2EE07AA1AF8017DEF3015F08FDDAA69EA6E27C088414AC956DC55BD3263D2665064D1F9BF066936EEA31F5994853E089B6F5B0FD5BF61BCBA6A274CF54662BCAAF4B7059CF964A430A52CD0F91842ABA738A0A0FC0D5330BBD0D3B373BCDD644D590E2465757CE90CE39F63ACDB63BDADBE1140343B5C976DEC6D7F13856EA35F655BA6D4938666109D209318F48D6782EEE105C0266AB6704108D2D215A058C0DDB056089C24B1B9917903B8292BAB6BF8C15363202862A250CFE818A8255E03309AEAA38945892D1F8C690446A6CE129545E489CD5365D2B87F65ABCD165AE9DD2D278E7E9577A9E9F00594E6F7A32110CC73BF3C5F54337BEB97F8C7358BCF66048480F2CEBD964D9916F8C0452E983A3CFAD1D387DE8CBCFB7CA756EDE272A4D2DCBBBB4957B57543A12A38F0EEB1156E2572D5D7D18E49E5AB7B875512C844850E6A7C9109CAF1CD608D6E94EF70A1AE3580E077CC0D5207213E0233D148BEF2328216EB53A869C4ED6CA61180BFD904E05DD32ACE7FBABB3B2D1C4F927323D7255C69B655DEE91CBF231F9AC7C55A66487037306C4001E48B8DC3A0B9A599DED61AFB255E8DD024D6B2A860BAA1866848562301029B2134814D61BF17D4E2CA25183245A4E6561E915E3A85C3B2A2F1C95FFE7E8C305B7CB4E2381AD100E168ECE52ADC64189B49BA4867828180EE266068E74898C36823A5B40C0EC0ED102EF63E6782308DA05018BD042E3FFD538816ABC74A31EDB652A33E5FAE18623F4BBA693F44592DE43EF67F06172D8322C0E4B474CA30D6638542BA55EE042254605374A0B5562C61889D05C710BEA2683543918DFFE83BE537D3B6FECEDDA9E1B8B52164503FBCC96AEBCD6D9B2B8B1034A98B9B99D95EA2B6FFD676FF3E2ADE489759E700897E67EF6A86F3896EF5C72E6CE873D4BD0B65C333F4D6C81732D86DDD75F7860060D0CE8654E0A93F8646C0ADC057FC5290B0D9278C2FB8CB88DF9BAB89DD96E1912463D673C67BC13F825EF05E1526C52F8BDE4C280CF83118E7015BB037BA40AEE009C045EE860231EA878F88F5DC0F54F3E6EA5222B49ABD3011C0A408548078A28EA21C6957102700C9C859F088E4BF7E0AC7086C5301E4E530BE750BCD0A464AA1440B73A637364A84043DB21435E2A506F7723AB84E425BC9D1E32CCD24C856D873CBB20D739649A3848355A5530D3903BC9E007FA995694F31A638D86E779BC97D28B095DEC981CBC7C67DB4B53AF9F5EDE96EF66CC1C273647334F77B6AE6ED9789FFFDE0E10BC76E5F5F13736E596ADF95A3110D0BA8FEEBB9F57162156D64256964356042C0576EAB137EDEFD97F65BFE827DDEE561A135801E7C41443F3C745613256D3CD909FF3E0B85984375FBA482BFB6C501D42DBB9450F703B22712F055F8561340BDD22F4252C8FF30923810E982127580BF0B3D0DA04D51A65289C8390A1A87B61BE7AD4AA8A97D5632AAE8A7110D7112FBA0F7DF43165559664038BDA46F8CFC728CA2964487958FB6FA6E67AA0CD84BCCCB00F663E030F4A35643E87A6299AB07B1AA498849BDDF1A646B911373BA4A827DE8825ECF022B922503239150315605092302851CBF6B2A71C2D27CEAA575573D931ECDECE0DC7CAF24BA903DCC1D49BF651FF58F2A4FF74F252D2B1DBF9AA0B47552CF51A74AB35BAD505BAD505BAD1DB7BB1520D1E28A3B33523F078901A6CC5B21EA3E28F4BDE4AFCC14CA7DA1EBDF8D4E08A73FD4FF7FFB27F697F9EB13577BCBC6A40E2253593E29A36AE31757D7AE3056FA49E8C74FF6443E1D89E0F46EFEDCC3C098203FEBA7062EEC021AFF8F63BBF3815F71CAC750151828CF9B07A90D5379ADDABBD25EFA0B7DFB795DFE1A524CBBBF835FCBAEB267E9398B24FF93E21FE6DB70CFBE0BCF4F8321B886DC460F43BC470742F71C071D7FE771F93A0E7FD80661805B5413D4DD02553BD1F032BFC13A0E97C28EEA14C13403867B3327E545D2BACAE5F0F4433FEE73044102A36C41EE5C9EAC8A0A8F3AE2C1654A3C5E896E8BD2819AD979D408418A6D905F28C28B86B31DE9C31BAC606DBA90ABD6520B2406009CDBBEEB9D23462505150B3288AE1F26666E790D19B2D4D03F67AC5E810B826EB241E7A18DC1C768B0216F4FA0520B84202E07CF052EB8B8432024A0A2A7205446A34D6361E2AA01BD68FCA3C86D54794E6E6994DCB9F6DFF4A5BB46B62477560C3DCA943373F8A49BE582692070F2E7DE38B4B9FF18F8D1C1BB97217F8FE71FC9DEF8A6EAD772C0653D181614487690012AAE89B7515983D6203EE34639468662932A16000C82ED66EB3B9E1C05758A7AD41A426A3A0413443664362A81822C6A13849C7F7F840CAB137098FC07D6C5175CE51841642BDAD122A54F18047696B0E8432BC20477518A33F92D53FDD4E81D42D0C9317929EB0559DC079AB0A27E42DBBDD2DDB50CEFFCB76F5C7B67157F1FBDEC5E7DFBEEFF9B763FBCE675FEC5CEC9CCFC9396912C7FE2669BA50DA2E682A2329AEAA3554B08EB5495B82A806A1D0A5FB813A546DFD03D08AF6C7C61FA85133463A064DC52A049AB48809B4ADA056224C30112852E81F50BBBCEF39DDCA84A57BEF7BEFEE9EEFDEF7BDCFFB3C704435296A3D66CAB3E6618162780CCF82E739CF050FCF78B0E780B55CF3DCF2D83DB154D128B27AF137CAEB6806F14C143603E63DA037008BD0E366D767810B59AB0FF0EDFCE655D83D3A1DD4EF4D81BB1B50DF1B144661A4839DABD8A9DE92B4C4A1A05A25D50FBCBDCA96C54CB9B79C333F06518AA8AD36C58722A1DE10BA114C7DB6F15EAD1C3C7306BDF3EAC9F99DC3E6308CB13024E5D8A7B9F1C6FCFE6807A7AA286EEC629F7A64BCF8DCEAE7B7758FF629CE8428845C8251BE38FF086C13B3BBB983BB0E956430C3CC2EF40E79B803BB855AA163D179A6FB9CF693B6CBCE4BDA6BFA2DF55FDB5DAE5E67991FE087527B6C0E285BCDA9C9DBE409F959C7E9AEEF395FEE7E79CC4D26D451C5AB4531C30DDAD56055F3163D55D3EF67F75A4CBE1D52BE4AFC035592CD995522C9204251D3A8227A79D91F35AB2B5C1B090583B45083C9FEF31E4FB2C872A45832B9152E413C90C7A5F345FB7836294C5805E7AF514D5CF0CEA9093431111D5CB9BB6601B077100DF644E7EC2C9A93EDA8487B1CC713AD304AE0211042AD388A845179941D9D50303562CB88918065CCE215CE468259D30057AC89045336599328D97C81FE9F0CD602E9D4CC0225CE42E148E16C819B2CAC15D8C2FC6EA0CD169F82EA5DAFD05DC71B75A8E52DD9A8CFDE814CD9B0CC74B2A4E55C69E42BC0908B1B944A6F31E3209115333FB591AFB7A86DCB7C99A9C2777740FC2820276513E098726B38D096A6873860251510E13C82D92F1CD9E23F393A05867BFB7B2C839DA616C5EEFE96A0B2B7C7DEBAA7C74A38AED5B9B7CEB2EC0FD0D07229103D7265273FD73DDC5FFDF16F1F9CFDE2DE6FBEF2F5B5E9F1FDA71E3DF6E4576F2ED5770E4E3ED85799EC4E9D38A40C7CE5A5675E14E25FE6BEFF78A9B36F68E6DC43B6214DD5599D9CDEFB8C522A3D6CE89F8A91B9F15346E9C2979EFA75F5C4CAF3471E7F7179C4F8CF3F45B9DCFBD0CEB19828011A333B18A66D1B74FE02BA7199E1EFDEBAE41ED0AD1AFE74D9B4ED60D9497D4D67ED361B1FE6B37C9BE065D24C41F6E2342EF0FE8BBE2B3E368E98802AFB56D8EB444CE754399D493B55D99BC92454595961DF2707339DAA5CC864501C1E65A287DAEC6945F1F9BC2E87EC44CEAE60802823B500197FC00C90E172808CC131300827460944AE1344BE1B445A0501D91D205834DF0E2021805281B7032C0EA0001DC9FCAB3A92F5259D2DEA476920AA65FA21CBE0CAD2E0CDD2E0D0D2E0C9D205DDD2C407C5A1332D32D7D599B34CF062B772A8985BCDADE5386A5AEE1F342D0DB563697829EB5667523173B1EE3D2D4242130B321410AA5EC15B3314001B0C7814DD3EFA51EA0F58063D0780AC4609A065E62880A1BAC517145AC1EE9A62FD47D053F3013F6A9D05C25E3803C8F5919800228E6B3E9ABC4AB076CFFF140541549F8394CD43C68A7D2DCA0E43590400B10FB04FB46637DE4E41F23E1BD0F85FEE5E18FFDC135AE77033DB13F3FBF3F1CE5D052130D4CC0EC5C45C1558FB9F3E3336B378A179EE70D9AEAA76A5FD0BE887C78794FEF1A67B269676A82A9F0A1FE65E7BD4747400B3E8029299B13DC6B89904739D84A50531521344C6CF246411FB71828FA8B29F52CAB4579545BAC8445539F133F47720FC3C7CAD68F6991779C413067912BC5F7439690C1260659CD8C93A09A7793C8257F6B2DEAE688480FB080DC66099AAE554C6B47420626952EC36CCA5083A1B41D648183949A4498995A503D20569496A2B4A35E92C2C56A59B129FDCB30AC0031B77BB6E814F6BDBF0E6C6561FAA6D584862853A8F3EEE2C7D81FF8D33C4343B32BD8F90E9E9B7F4B1A6BD2A05F551DB639681907DCDA146FC607F9BAAB2E9C841360D4B88DBDD5F3577A006F47C375362FE48C7C85B440A474DA6171D320E958E1BC74B4F074F19A74A4BC65269B5F766AFBBD7E23C3ED1644AB8C41654B944075D5F26FA61FABC5F2879781A44B8E7A710391ACCD00AC7912013C7F154DC8893F864FC40FC687C21EE8CAF70F6E56C3E6FC539F2FFE2FC61214FAF893E6C42725DC9DFCCB34C1EE7D9FC1BECEF991EF6CF564BC756D43E8A185E6FCE6E0038E7EB34701B5B51ABCF329F8C9C9DDF5A5BBD9A8265E41E5AE67262BF15CE1932355DAB4D4FBD25C69EFFDAC913C35AB680588C63118577210EE5C76D87A76A34BAB5A966E5CEB63323FBE7661E288C74777B70C89111C5CE8EE0F091C8063B6AD68AF60EC0C2ED80854F0216EAE86F648C4D85062EB33FF7BDCBFE95FDB7D79674B6BBB389743A9DE94FECF5CE788F79E7C505EF77E2DFF5BE20BC807FD47EC9FBAAF02EFE0B0EB202879DEDEDFE4EBFADD55B8882A42E2DA81945242585B60E47B7AC336E003F3E184977C86A58755110685CBB76ADD6B856DBA0D39FD5748A8D4A9C1C635446C72AD68D8C4D1070329990241F422C48D9E573CBAE70222287355556017759F01AC2413924AB722693E952653D93E16C5759606CABF0D40EC9178407B120EC4F2682E04BF04AC904167C2C721832A3332E27EF9B1380975F4AEE9380A793A8AA66C221D70DE31F06FB0D0319401043DB5DE87DE70A3ABAACB9906B055DBCE49BC3AF231F23208984139342524EB2C97949920546A6B5DFD5A5D144C10091456D555BD36E6A6D5AAC68BC81384661F6A0754AFD80F9015E421F072677BBBEDE58DFDCAC373EC09B7B28E583864B095F6C37DEDC8C36D669D9211A26C7A29EF73D81DF6C5BD4A3F93A5DD51971204A5901C2AB8C25EF5F6347C55159B4240C64907AE0B80EC0D8DF07990529A58478DE6E0F845B6068A51B670F7DB28CAFBE37A614087AA932FDAD437F380D234B33994A765DAE74569BC92D74BCF3EDDF8D0CC6E3AAA3A383EB5D9869FEE2CD681A6A3BEA8B549130F48A8599F70124E45E1E105285DCC30CC09A9FE2E3013F5AF223C1C6F00C966D98C798770399B7501258BDCD424920F998567838034FF23617738F9EBB2906BA5B1848D57F092FFFD826CE338EDF7BB6EF1CDFD977B99C639F1DDFD977F65DECE3701CCE24598CFCBAD0C41012B20D18298B02ADA0DACAA6A43FA601DB1C36F6535543D9CA54F1078855AB409ACA1A0A5135894868B4DD3F445B07EBF6C790C6B4B22ED2B456D57EA861EFFBDE05689934CBBAE77DDF7BFD5A77CFF37C9FCF33EFB82EE76B21B6D04062789E03731CA03891A3B9C39A745A3A2F05CA525D9A9316A59B5248C2FB2BAE8BED4567ADDB4EA41097B28F692191C1550944EBE001E19BBF27785BFFF395BB321778F3512C73E8E9B75214F30CE2E5217A0C6AC33490240D46D4BEB0D041D5A821AD0395C82106ACEF53F29ABC40DFB8A03B79AD1B0DA0AC37F25ACDD085BCD66118D0027A5EB316E8DF5D32E020E8CB6B83680C4BC643796DC83058DD599F634150ADF5EE0FAAFB2391204B0D31B5C16E4BEE883421A24F82BD3B54DDA59AA79BE79B8BCD6013457C4C103481164A2905018A8269E4947259B9A604A032A7D0CAED9C5E5AEBA05B0EB9E55C76AE3901E8CC39B4739B12FAB43EBAAFF45083A07A4677F7346E36E8D38DF38DC546A08C2E4B8D4043196E2ED09F9DCF617CB0BD1E88B003C1DDDA47AB76B2E66503E6DB1AFEE0173F2A2E8B7735033B017FEF51046963F3E54A3AC345434C8FD96556426B55C0B0192EA5023E5A667A5590E655AF99156BB6885D79047DA8CDDB0F4249CB86DBB261D50A696D398BCAE6C22CC0BC8278E2C8918DBB607E4FF3669366F83CEFF2B0799D0B6D0B6D0B8FB56DE3169BA17E7A1BB38DFF3713C47DD8CC931304689A28A43A33E445CF8BF13AAA45FF9C4748432C021DD43FFEFDAE6D8F7AEBC892B9C07973C1BF2FFABF4316CF7FCE0D50F7E00A20FE417F1C27D093F8FFE883FB43B2C4E2B54F04F05BA3DF1A7BE4506EFC87E37B9F722C94E7036949B633F62EA73DD158E9B21C412EA7BB73E52ABAA7120D08BC7C78FBC6ED3B1F199FF8FE899523075C4444212BBD171CFFFAA65CBDBE12D9972AE02C302A9F01C75B301FD74656228FD519220B076891C88247E77D282F6C3A88E9FCDDD7B8813606383896FA47AAE30E0821322F308177E8EB81DFA60271A68A983D701DFC314D4B420CA9ABADC5C49C68BF225C16C220DD25E735C1237513D1B9A14710B91352CF62528F1B88DF6DC3C865B382108B28FB4381209B5E0053F34B0080853BAFC19DC92A384851361321EC1E8FCB18DE6514FB820CB2F235999631C8CB08E2650CF132ACAE4717C4DE32CE0D19E3BC8C495EC6242F6392176520637C1734E7BC43979D69943688DD1D9FDD894587383EC33B3EB33B3ECB3B3ECB937722208677BAFCB26359E65D883741D95C3497CC80E943BCE943BCE9C17BDE359535F7E09DB0BB781FBCA3950F26EFC5164947D1A7F70FEC1904EFB5650FE41F20F8AC47F0D955821730C16757095EC0042F60821730C10B9F2478D46F3E891A4E04F1368594D58FE6FF11C80FC6EC95E6D1ADBBBF2A8B2824AD6A4294ECD4CE2D5675C5F2C3F3E0D8F0BE9181332B3F3A4000BEA03C064E3F55CB1D5EE1BED0CF7E2C0CD1CBDC72E756E0128AC3289503DB61F28D14B078207D2E1C33A380621326DB16E6323048DE3792D120346D57088260CAC00F34522566D8337562E60736B8D8C27CB7ED2E1A4B064D19D0D863E061081AA70CDA10244DA225B8C40152B8D0B9C4A2A3B1BDC8C75C4ED1D119B317AC6AFF0C564ECF79A3CB9363E26AA7F52172D5E832E539A8B64CE47013C88905BAA0A9599566E48E7807CD3066BA2BD5A5740518212A59E829332AE86C93542AC9662CD0CEC72CA006622AE8882454AA2B94B0285F636CBB64974A4831911856BAC100D80C368B07F9D034D3E25BE2B432CBCCF173E2ACF2267D558BB4D8E9E8B4D04ACEB1B3D159612E1906883F6626108600AC4E88730D9DAEBA52426708F122E4ED430EC5FE34C1CAA15F7F69DFA11BBFB975FBDABACD8918D75CEBA85654360BA9C0956FBCFB8337BE7306745F790BD8C3A37FFAD51393C35B147DC314C89D6B65E2D883D6CA9620DA48E954193C0D15A91C16188AA5DA354664C576A6A36CA0CE2AAFB1182638CC17CC2F0DBF178369C3399A60DB25D477310553E3183626164111A65352C5F32F36F3831B5C6C610FCAC2F1CA5285EEA9C0CA7865BA12AC483E964425C8831E1EF2E3FC22BFC48778A5676C86740933245978748C92C36ABE389FCC12FB6A42C3E93041CA9F3889BD4AB656BCAD157F6BE5BEAD1FA208C050B2ECF56738216322AE977E39CC9A6B92AA52B0CD8C6915D6248B1630557429A51C0B7477152C8AF25D6B7B456E300FEBC3AE812FAD644B6D99AD35C1A7E596329DF99A316DB5EC6FCBCF1A27E41F275F545FD44FE67F2A9FD5CFE52FCABFC84B9BE28042BE9D44E74D14508276AEBB3F43737134246549C6DD0EEA6C88BF718F83F219BC92E819FAE83D424DE07B95759B773E7E76D7EE9F7D7174636FDFCE47D71BEE8009F735A6565E6ABAC94281CE25F604FE80BBC6C3CD6CF99B7F3EFADC7B87F5D44B8706B6FFED1F1383C731638D5054E0CB28028AC08211CEE406389917BD9442828CEC5FE6D39A6BFBCC87ECECAB5A954C33AAB72C88C4424BEE74451B9CE08ED934A744DB5D2143A95451CB88AA586440BC3391A0F4339A4A50357155CB105435F25A114753C688F40A50AD21C5EBEAAB0B8FE32243151935131126A9C8EB608A0A82A94BC7D825F6261B40F1F83AE4A8A290D01274A264E85EBCE9A41AB82EB1E92CB150963ADD451D4CEB80D2459DD67F5F1ADBB1DA817AA88A7A8AE565F1160156AC06B68D838325C1816303B5A13ED722C4B257E5D6EB4B2D0F0D98783C813393682D76A4651240987CB6D1BFB1B1B63AC646A29954319E052C5FEE5F6137D8E188D91378F9EDE7A71EAE6FDCB229C874EAF5BDCFDCE81F10D34A0041C1C0213A34DED9950AE17AFFE93BB7E8B7918F7AE973F0F35C4F5CAC07C568511633C5202377CA570B57CD77C4BF8AFF12D9A25828F58BEB4BDFE55E305EC89FE57E622C70170C2EC487A2E1629C1FE64678067290A7A55E8D3A496B00E0BA032027D54FE1620E1E861DD449A98C16DCF2FB7652534EA6B5540A0B2BDA722C05520BE009A82A273BDF97A49069B3926A4A9C9FC7508ABB60B7945BB873F3429BCCECC003186993E91D544ECCD139EC0D8E135C6FA6C7F0FC5348BFB51888A584FFB25DFDB16D5C75FCBDF3DDD9897DF7CEB1EFEC3B3B77B6EFFC2376938B13E78753B7BE366DBA96FEC836A808C86D47E85ABAA23541FC915505BA56541B624305CA98A856F6078C09D1AD51BB4C42AC9586B601520B08C15F5D85A28996796BC18C21B0C3F79DDD941F4BA4F7EEE5DE7B79EFFBB9EFE7F3F996B05DDA55DA5B7AB4F4E5D2F9125FEAF125E826B4653E417C868FF139B0B8FD94D2FA72F450B09AE470CE555160FE9C3A4C299F32FE5C61C772BD061DE5858BBE04C8A88F4E8BC0129F134E567D15D9844649C310EED651562A111FCC8336AC2E4D262042EE55BA608FE41E584F6FB2085BB83DECE2F6B011ED2FACEE5598592E50FE7254ECE4A210E478101A29068D18814650DA136750B54EFF91AEEBA4AA2FADFC7131106EF73083F61760BA3BD19DF72AE2C072F5C05C4E87899C0EB3B8F0DD29D2BB0D383A96EA8D3A92DEA5177088ED7407ABB6D345A081BBD06974527B16FDCFE97E381AA4FAB5C5760F5705EB91EE071302A3DF3A5DF090EE075F925E5AB9B308740AFDF22B9489E3C0B5F7DCF50C9A836CA1D406DC8643A66BA5A962B1AB6406D9627A86A98401A341F664332325C8A608FDC318F32D925A776243DF44388133B59D4FED9E3CA2FB934A524AF59F9D1A5C5739F8DDFE8DDFFEFAF62DB1608F12F55C695D79EAE0981553FBDEF8DAEE9D67A6F3FE213C7DF2E4DAFCE0D49643E30FCC1E3E9F26C4A41C9759F92B73866D22153DE3884FFB9F0E306EE30F2075095F027CD870D8239F60309FF00FFA1DBFC73FDFB55FF4339E252C3ABD9CFF52408B61964584333886CB871479211C0E3910FD10FDA424A8DFECD0E5D0B59027A46A945DE00384F882596CB87E100CE04E09E40786A8DA5CAE55A178A3B6B051C1D25BC5413C87E6707058365D2D181A8BB48965246802978CE1A5EBD74946DA30A1DF7F69E668B0FBB12FBDBC916DB65E9C6DBE76BFDD3BAB5C9E5D973A83FF61CEBCBE40EF5A5D59668B9E1FA2143EFD2AB2E0743F808AC0BA66315D8158201FD81A60CB8167E33F8A2FC5D9F7BDEFF99894E3174A49DA100E850C4E0AB16F7BF18A1783D873A6492C23649ABA65A44C93E3B96E757F97BFDB8F522908008FF87C47C1759E1A7C1E1C3F0F269FA7269FA7FE9EA7D69EA7D69EA74E9FA7FE9EA7FEFE2A8F098F13FC559E41BCC4333C35FBDD16AD1B2CF0F956C7E75B1D7F6F75FC3DED2FE4DBAF6167AB63F369EFA860302E5BD8B05EB218DB3A623156D890B19C279468166163B1E3F2C58ECB17DB9BB93C1402B37F5BC4B67859BC267A44D5ECD8FE0EF1577650DB7FD73DD29F46ED3F475446EAAEEF875FD755BA9EBF3647F5034C839B14F305DCB1E3341732998EDE77501F1D73879E5FE5D6B54E4C7EF5C15D47F3D9F5F858A82F66F5E6C6A9376F5A8F80293F36BDF5A1C79FC75FA026BC79FCB3137A48DB851B9DCA30048EFC3D403F8E4F3A5A0F8318DC837A303BA8CF4466A2D3FA2B811BFA6DDDAB53151746747AF14CDC2855955DCA6EDEE3157D86978DE0482C6A44DAA860CEE015493694A595279C4304C513B1787C8A486142248CD01E22C293181731627929010C2151B61C941C8991621112938888B93808A3D7CBF371E48FFD5D5A18240E99261E52136F610796B81294C0E730433FA6ABD883A7E9C9162BBB4AEE096366B6A43B022949FA3EFD9C7E4367251DBF04F7607AC14B7816935720E30A6D341A7390774DB5516B44EBAE9E533C7A22651CEC2997E1153C9E1A2888C7A4D74F710351F7A11045521D4B97DB6DEDBF3B17BC1A2DD91C59A787D5E961192918AF62DAC0B773E342B8EC7632ED3EBCE027557C970F394C290E2C1D7804E0C150C8253D18F33C24D89F5B3F2F2722FDF88E1D8CAE79F6E8487F190FAD191F6FBD19677E77C2D4BAD2E9A0A2A71F6E7D1FDB8F8F1A59269DE6474F365334CB83AD294F1D70B6F1EC4522180213A0ACFD9390BC9EC561BC0D6D13EED366B44FC53E3970483B143B38F0446C29F6664CCC8572E17134AE4DA129E1007FC07B20F08CFD027A41FBBD2AC0AE822D046C910F780D5E5615439638CC61D600710919E1BC9CCD5905D1B6A73435AC696A4010A2A03CC21E84C3481011C6495B53452180BC72D646167DC41CA759B70ADFD089754B97C320011CAF21FFBEE28DE2EDA2C7AD0A8470AE548C443422DB3223039C4E84EBEB4B644BD94D594FF6AD640171D78073D5C1E23DA877366A3B1ACDDA3250EBCECDFB37BD53985F857A8754AFD6EB41801900C7B4EF299FF20D14DA908B1DC851FB65F9A3906FB75E9F54F15500FD5AAD806A006307B5FF0391A1C5595BC3948812692732FE4BEB379B360CE03BC5DCD0B9CFAF2DAEC7E581894DADBFED2F6E3EF8E0812DA5A17518FB7C241ACB8D66988B67EF13C1A9A7A29923ADD338F69DB5E9358034B7EEE5E6C75AFFAA7C7CEFE4C4766732E3F7F7E6CF00462BEFE3B3ACC428C883628EC054510B691C56D96D9B295D2D4BEFA0EA0E2867717224C94AFF7C9B4DE2B30BF0BDAC45889DE21E4113E8D7CE7077215728ACF13C997B31F7D3DC2F73EC21EB17D69F2C8FCFEAB326ACAD160B25A00C05A0CCD2CACF4C699621BB9E9D81F28F5AF507A82A844115886697CE43C977AB94496121BED4ABF762C4D382B5BB7F2853C098C90B50D9056E9278EF823641884118928FD96023D5E72AB8F215FFC8DACA8F93946A21CB00D636CD82687E500389941A34872B944F4146A164ABD7822E6E34B75C633E3787E7E770B2C3AA6E090583C8BD9139325C1ABD5B4D51FF0148752A296ADAF16B8CDEE7B4B4AA4A5A1F8ACAC696B2394E8E1C79E8FAE9CF7C7372FBC6AA6AE6825AD549168282E7F9A6B5B7CA5A9637ABEC61169AA73E1D3105CBF264E5596661F67B3F7BB4345318DA1A49667AC744C5DF134914335F448841A310F90D10F90C1A43371D82BC4497BC86CEDA3E1A5D1ACE875369CB30CC14082E6F9AAA65E8A629E140444DF70D9368613813C6197189E4B2594922BCA1EBC0A5DDF6E7A25135DFE7A471FA2616119120B2F381B18E1C0B36445D7DAE8CCB10E2F1F24784B87637C6F555D1A2AD1BE6E54E7A74225D1C9C5C701283A3416524DD9329664AA1311D0DC9B68E95C8687058C7830A34D44E565CD63B7E1CD7F0FFA2907151880C53B16B1749F4453A390288AC16B71410125EDF52B669E2E1C3B8F6C663997D137F20FF66BDEC639B38EF38FE3C77F6F935BEE7ECBBF835E7F37B623B3EC7679304E2F89C3728E4AD2804EA246E485840812D1DD042610C11A45254323A09A8A669FF54D5FEDBC410D31013D2A4596BFF20085555A74A7DF9A742745A56367543EDF075CF5D121242B2765A6DDDF3E6DF73D63D9FFB7D7FBF5F4352F12512264AA942D2EBB439EA5A47E33C2957CFEFF5048D18483DB79738B6FFCDAB8769F4EF5343693F110EEB025EF659BCF83BAFD7595FCB30564757FA02F683384612C2BA89C0CF64F6133BC4390F0510CE7A288428CBE2FB7D5D25A1D78A538C08A9887C2180B07899C1D2F95AC72D2F5888AB1678D1022D6AAC1282598B56DE386AB3C0822C84E5A45D9D736C6DD66F1FB75FB593A2BD60BF682799FE3FE0EA13A715FF1AD382164E1CDA34F95A7AA7B5B35B393A5569E0B554A76268E7D954074E02BC93CDF82D2482CE492288876AD4FFA32E033FD3EF0116205CA7CE0180F3D66BD74C6EEB4DF816FC09D0F298BEEA92302C17AED811E0707E78388F2FFD1EADC397AA303761045E217258615CB70049BE8BEF77105FEFFF460F45F4052E5654796102F08A12861F625BB4B84777EF9BF7E8EE7DF59E3EB9B207828DF6FC73E57F807213F6ACEC317E8B3D46F0F0A671D51EB4E19EEAE33D087C7E132DEE41600A947423BA7E60003470023F880111FB6F016C05036037781EEC0733E018380DDE91270F1C1A1C1A1ADD73FC472D6D2F1CAD4F8EEF0BF76EB31ABB641D30E2AF4F08B725C3E1641BB9C7974DB308B97CFDDB5F3A7C7862AAA7E3C72736657E306DAFDD394C509BDB87F137582EF19ED289E95269FA04391534DBE2A954343805C48FE75BC5F9BBF36A40134511DD9D47F34C2B1EA27975B8FAD2ECA0B8D8A33B8BF66B8C9FB2C727C08682B9AC94892DF58EA5DEB9D42FFF6E58335FDBAFFD7DED3CB2E6FECBFF47BE97CE66D397D4E6A1D4243585D591D29CC19F5F494D4D12B1536DAB1E758138FBD8B6FAEB743693D18CE13BEA6FCAA8DA3E548D2FA923F20A6ED278A6FC59929A3EC113F8061E0CAB373B891B782B23E6AADBF0E8723A9D25842523C58007F7D56D1F64D3D9141EA89EF1B6D2031F60F510C03139FC1A053F203F23BF244982B493844967F98BFD0D1DED09F37E59A08CB2C19C35DE20C96B35728DAA10EE40B6E60669909D6C820F894241208438700BEEB49B34B8DDC100165BB13CB63076EFD3BF25A0C785FA1612C055F0F42DE084E653CCE7F9F2D818CE4458C372189332D8913956CAA8E2998B319BB44C133E609AFFBE571ED892D849B6B6501602BA9C1DF095C079F96EB8B194E82899A3A6EE3E543C6A5F205E2ABA3883AA1FAB3E4DE0EBAF1755527F88A0402F5063980F007D04C7B07E3009CECBDB47D0F0F000EADC2CA7910182BE38329B5D830809DF7331F4807F801888E3A40108027EC49DF9FC405980E86C5F68802ACF267A6673B9347E3EA7C1EC37B9CCA050512A954A4192C4059CB9552AD54A453D08098A6377E62BD5B1CA6D755AC117AACE576EDB5B3F9E97D4157B2B3E9210C3F284AA94EDA416F717279976428B38B96CBB4ECAE0B57652E2C9C7A28773B750C846729C1408E4B229029B11EA5AD04670BA81933AEFE65CF5AB58034B2AF748477D54A145C9A93B752AB465F0B972B27BBC18B5467674B6DAA5A17CA0379FDE6276D9A95AA7099EAC96F053D5B9ADB435D3D8D2E6D393DDD57D42539081E13074A78A0DC44CF5A7F162D28D831397EC4C113313A3BD3F1C94EA2CC8E53679EA681DE4225921D39E08B1619A7511BE7431F2EE2FCC66BB20203767B7304E6FD3D6A44A0301400D601A53587F7E290F1FE567664E4FEFE7DBF809A3D1564B4FC479972BCAF7F4E8A7E2B6DD8383361E7F5367A27EFF69783ADE94AE4F4D4FEFD871FAE514CD9F9D9A683A4DBD3CDB3C3A5B2CB635C3FA88CBA8E7FC51835325A3C278028EB44C4784E20A1D2C2C8C244AA87A4712190D145E6E5587A892594215590FD446D488656AB8E3581B110A2E5252D38414A181E2A4CCA6663DCE2152E422BFE575FD3F48261A52C448BD8354EEEB1C0D5125126970E894FB186644B1A6732EDD9933C5C33F7F4E2E17EB4D4CF3C074F7B663BBD2CEA8E465E211EC8B5693A52E16316CEB0B53BF7DED7AEED1DB0434B942B5662B5DE30C846AA4168FEEFDA89CF2618E84B751AE57B9D6CB8DDE75E66CA2338D390FED7A755CE27CBCC9CCFB586FBAB3A14E1418536DC86B46C85E63B23B380B11ED18DD74C1C47812FD5EC8FA782BED75D75A68D6AA4B76EF8E5FC2D14AA33E89A97783123802DE94870E4C4E1E69E34B1AF392C6BC3CC2EB417C607B8FAD6B11FA8B18FA11784483BEB55C1E194AF1F4D952D30835345BFCFE6C7373B1ABABADB83175E669AF5C831D5524A97A07B74F70C7ED46CC35CC4F416F5E84AD31C73922CC45A37869856A605DD294B331A6B8A30D2A69D28EE9466209561B47C30AD32879A8336708A2F7F8E55E2EFFCC608C96FA0F6E6DD911FDBDF2B9C14AC01ADAC20A0DDCB6FEA05E79B00EE3989C5219EA5CF142A3CA3429279C38EFF288450D71B21077E91FBBF2B313174A09820E05CDBCD761B1283B6F9938D651E7644D3687954A0D1ECC8F3F26EBF3D49A5691C5B599D2437EA4A9EB017973474B3B70BBC538080902AD4AB36DD000E9943F45A4E20C32BD6E83B69EE8961CEDF331B1590EDAE0D914439928BF11B3AB560A185561019F7FE1F6C293123AA6BAE84246445840B57CE77FA892386C6DA056EA5CB5C0E358B5D6D5AC6231E24FCA97FFBD7A2278C67DF9C4C917F30DD1242410723B0394199264E15BD551AF3CCABF5A2C1FDEB735596C6CB422CE186298FA089B9F712EA87AA8F4E8BFC0E77714CC613DDC3D8B4E9D9A3BFE224AE1E03414472693131D28EF032D2DBBE2E0998E8E19A0E677C24527E39F83736AA4128E1FCFE7E7CEA9516A2834471D8AEE1DA1CFCD36F4CF4A52AA61A350757BB55F3CAD87AB4E7B591357EB21FC4E44F09BB0C075DC46F791F2D7FF531689C4BA2CAB1F3EED4538087E57E2F8A865A35780441BA92601CC00E802E403508773B6B7E42E8EF30A753E2F0F819F27782F4F78CB0CCD320C5DE3800E07EE8D469EB062E9247AAC36D66AB5D968B3D96AA5FD42B4EE75D51B6B6DF40D1D2B5BFF4377B5C538715EE1F9676CCFF832E3F1DA1EDF3DE3F93DE3FBFA32BE62CC9AD5B224EC85254A3629C114A10D819006D404A182B62AA5688540A2256AD534559B8736EA030F0D22A48ADA4A8D1CAA48BBA4AD8A682B441A55156A55505455551FB0E9F96D2F2C552A4B5ECFECCB39E7BB9CEFB8DC0E1BCD8624D9CB0D9851EA0E3E1324A9748017289037FC7962A12B6246A47EB5625E16BB085E915599EFC04B6E59FCFD8AB9DB15BA2B4217FEBAC87F811980281C8D069166B5860C240D7E13480151863110CE73EC74A3FF766341438B01F4923A95EE53938699DB7A7EF91EF2BCECC1BC1EC2D8F2856719DFFD9FEF98C0D8E3B37AC437D0A1FE27B0473CB04C6E3237E1A6C9C245B3D8CE7335735BD71D6DC3F03714674DAED1B5B45F76537ED14FFBAD9CA257ABD982EE965DB6AC1E95A99F99DC70324128BD0BADDE6D34C0666E7745E03EE1F95D10878BF07BC05A0BEB258E02E41D519BB0B0828CD2889090DF86648544465766560EB64EBC76FA3BF583AFEFEE7DAC95634E21564BA21F6885E99CD79D6CA5362772F38B475F8C3137C39B9E9F583C26D163CB879AFBB727B12D6CA4E9B39972C48AFBB311634A4FB4F3C19EDCCCECD97BE055806DFEC13F4C26E61EE4DAE9763045B9DD8064D66997EDB4BD0DD956B0E8B1745A8F01B2EF797C6EB7A0FBECD06309A44AE42E124345F9BB1D68700D1EC9B584C799819E4803204A2F91E150A044B0249D182653C251DFB9549F7B6D2EF1BD6F17171766F49D57BFFC8DEB17762C5CE81EDBBE6F2A2F85E25C823ED338B4B33079FC47FB57FFEC494F6476EF9A7DE2EB578F1CFDE0C2536EC91D54088B03C0E23D8059804A52E576C8DD5655739AA79C49394927D36659A764BF2DA93B64E1113AA4E0DB43508A85DAC3F1E34A6CDD516264430FE64FDE07C8C4F97001BA524F7E74617EDBCAB5AFF53E466FB09E58F0FC9B895DCBCF18A339A7CA113BDEFAD5F7BFF2C23BA7672FC59212FBE1EFF6BE756C2BD96502B0EB7DA8B4449D6BCF3BADC86AF9227784FB21C7701C95CD8402C16026C39793119572F2324FF3694A15D5B7D49FAA26B568D96AA1298B68512C4CC692B19490CE07ADD620CF784A7A524E436BEF7AF4881CB28D3A740D6EDBBB83D3B6D3011E0209C5DB23475E33C8D19BEFACAD7500AE0148860BEB3A1E1C0DC6F005B3919731170115A3CB383565C8265DB7EF9BCD73921EADB796764D04E2B9FBA78A4DD9EA886DCA312F607BAAFD7CEB222D8BD999667FCBFEC5FEA76AC66F05626EE9FF1A8BB1628CFE969A8F0AB87FA7305F8B8E26B30C93C9522FB76BACC912D7295DD4159DB1E8169D1F57A590E7E14C4262880E592C6E5E47260D21CDC4B8B3BA2AC761045724B7EC1DEEFAF5098CF849DA5F156F5F1BAEA08EB826AEAD0D5AAF92CE496B83CEBD1B7ADED0746AF74B0CF84BA4326784ECB156B1772D555305D2888B398285FCF4BE2D2B34B7F4623F579BC979FA7F49C2E582B1235A4AD01712C5B01DF7FFBE69A1E8A5466A3B076AAB50CFB51365905A5089A268DA1308D83D9EDA4876097F4886A0E3B2E8D94AA59805E9BDEB71E9B20AC233EE961ECAEE3A58E95079832DDA5905641FD79F4F221E52232652897955482B9FA7454EF33766F7B7A75E99CFA44B784F31B22DD2DA410B9BEBF3BF39FAFA1FCE4F2D5CFCE8C4E4BE27CA5E5FC4AAD167260FCFA4268E5FFAD2A91F9771CA2E7C62A4342D5DB8A767769CBE7CF0E887179F1E93C6FC2AF40A6E6ABA01A83620BB8D636C186CA310CFA4C34AA3D0A01B0D4773CCEAF0B02127D82D4DA51D619DA258CD3BA6CAD80360BE57B05A0CB9C43E0C17836F22599407346F758770928D726BC46BE8FD218CD575E67AC92820466C7CDA8835BACCCBE544EF3FADB24DAEE7682D578970E8B42D62A4685CA84738A752C4BD3BD972884337FABF4D1B611BC6F519FA6CC288F218D85C52FB9F2131518E3A30E61543EB1DCF944356F81D2DE9284F30F7814DB5600E21AADAF6879C4EAFECA5BD6956A02956641596617971604E57E8908C86F425C80E57C7AD6EB1107B4C8A8F5AA05BBD3F252B8AE0542B1AAD272A31C1A15453D81A2E8313E5CA610E632E5CCEF58E93AA492581077F658A5049866AB46579E7181A0B6AC8C9CA2CCDA6A9B6F64D8DD654DDCBDB33F46897412983B0BC0AEC82BCD68178EC5E2F603D7755D7639877B8B10665FE8B6EE5AA116B69F7C9279F3D31AB4E9683B998DBE97266B0B1C995D892656E627BB49CECDDDC7E788796693F1999981B93D37EAF1292AC5E75FE29FA40AE12E2A8118BDE869A5BD495F682168896CB79DDC771BC4FCCE5F3D34DD1D36C8AF9E6540BCDF1A8D5147927BF17B53C08B59CADCD4DB159AD369BC86C768A085972629AA8CDD2AC895C225E962B3A34D9B6217340E2397978E2415AE992ACD2275CEB9610B845AF3B9018092C9050C465C8245D5377C5EC17332BDC7217753A90644CF044628C5F24879ED8255FB7013B2DCA3CCA280CEBFD5F9B59C713F2E848A602C3B22693A46BC940EF467D5C727FAA39228578EFDF4A5E765A23F5028DC99641A99C3C698039CD3EB3D43E7C2A1CF702E95CD298EC47B1F03F819D6041F68891A4CFAAA598084C5572E1FE3B48CD953C611FC64CB8FEDC964323EFBD0AF355A8897640A61485551FA3285AA7E75525A0032FC886E9AD6E60E8E08432D6FE0F4DA5F57572EEFE99742326B8E2F504FD74652AE9CC18A5D957761AAE04E66543A757C62B2116081BAAE47A870B4A76F6C026A84EA428F301A86E9C5A6A577904341DF70772E6904F54A482444B9256C8AA9A120C4D531AD2C6833E865254192A0EF89D8A0D789C1B1299DC7E00ECFD2EB94A7C2498F54AABA385D8B96EC0AB1E511AFA1C83C0083E2E8FF4B8ADB8999FA05FA6AA8A20C8A578EF8FB986E230F5DFC4FD4B96D9F9FEF7B74DDA955A16FDE26FC0735B301FEF1D22063202E483FBDF655ABD57E7DA184FCDD127B59222E0DE15E0FA83CF1EDC312D41B77170CC86C039A66536CFD20260D0A6509142709B815552613D2E39A7B13FECF5CB52D8A95202CF0B82648BB3AAC52AB323E5764538C848CC6EC0D213095ED7577BAB8393ABD3E9AC763AF0401C731DAEFF725DB5316E9B75DC8F1D3BE9398EF36227B99C133B71E2244E1AE72E9726B9BCF97A697ABD97F5DAEB9BDAE5D6DED4EBB5A5EDDAAE2ADB98368DA1539960EBD84A1108AAF1A2692A0895FBB0A1029AA8982A356380D02460F0013109BEF0810A10DAA5FC1FE7EEE888A5C47EAC48CFFFFFFCFEBF174684521FE6CF648E024EA1AEB962B51C5A6E4D24D632AE7823DF7B6DC776359088CA3C7A095D4757E5424250D589C94F7E4A05D656D562DC0775ED24AFC78261378354ECD1261FFCC5761392868F4812E3669C305F116E083F143E106CE3025284BC400A429A8FE0DC61CE0DA081410D27891FA91A8DBD66133390A573B06F306CE8A1C887B987DE087096656B90B69BB3577FF5E2F3BF7C756EEFEBDD672E74BF7EA4F79BE48E6395FCC2B431583F3EDD586CC5D15F976F7F697EE6CA4F2E9CFFD91767DB5F78F7F34F7EFF4239B7FCD6E7F6BCF1C2DEC6C53760D7380D74E13422449AD80DA26CF2000495CBC84C7FAB3AE775109C9B53388A93B570D891D63CB20F1339ED90994D1F6229D6FAA8F4F10635241E4EB5D6DC6F384F88B81BB613A4295AD6D73E4C1665D7F82C997CE4EA1313C3CBDF3A7BA970F8A4909FAB26DF843177C29E309592AFB70AEA507369AA7E72363B757239B76B6488E85761BB6261CA202E9B95781885636A6C2112162291B01A8F284A241232C17946130A83183E2EC7C9B8CEF9A034C425B4308CA4010947C4921C51A0B2182E0DE60958D2AAAE6095F70B5C5EE70EB622100E0075DE0A04B9154C8FFF5FAD85B7F510E4F3C33AC235E3DCA3E19A21D940CDA95185C3334426CFDD383972E9D4C7BB1FE93DDBFB5BA3601E2CFACF5C1E7E3353181A20D727EA6BD1ACE454B5F6E3F5C34FC67A6F9FA15474732A5E9B4D758E038700FAA84380BE16F1AA79400AC64123F8B86912B5276AA452336BAFD46ED43EA8D1B51AD11E1D86A467124A0121E07AB0C266C514D3068F5C7C8427795E1219C92F91125DD1D22323B9342B6AB4CB3540035A89BE2FDB70679DFE9CC177FF06E1FCD7ED5A0FE72BD022EB1EE4F47F56ACD8B7A2308842C4168850E23ACE61C5AE7A0A2024163098A9C2A3CFCD3416DA23BE412999F436F69EA8EEDF3FBAEFD4B9932929EAA547165F5E683EDA1A160723AE44D2337EE4C2C463F3C3734BA797E686C9DBDBCFCFE7FC217F36D47BABB2349D9D6966AA9978BAA0858AA385C1898BFBF242505014F4D4CE0B73D9E95A7E3C9F48E5DB8BD0C5304CC32AE048268E9ADBC207A513D26589DA2DA186840C094922B1829089D030420A42D0AFBEABE009D10D521226FC764DC6A3E1E6647EC3E3FCE1CEBAB5F8230C76A7EBEEFFAC3BD84F3B723CDFD08428B59AB449A3B3A33B59652CB7763759563DFBF23B47866C49EAABE40FF43D0D6D2D992AA93C004306F07C941EDFADA134F6115EE0A08BB0FF24E4DC982806353949B7DD725E36654A96D37C500E9241DDE712BD4442D6B003BD256D01FAE9F68D2732EEF52968ED9EA5F0B0CBBE52F40DB567D3627B1E7241C5A8EDA2568CF1BF7EEFC48DB395C61CA94ED62F3DFBFCD32EB5B6159DA756DD89C6D6DEB1BBEF1947560EA1FBF5A2AA3676F45C9F7DE6DACBE8DB7A3DE955AD7DFFD956807D4BC4D36699653D3EDF026D17689F48DB07066C200D763A14127DBEB68D146CA4CF66A3258924F334A23D4EFB16921043F2108BABB16132BD63391CEB428346A710C4A35A7013EFAED0967D1909C223AC1B2B0E77865E5FC1B5327690416DA34888163E9FE55840216C85D14EA677DBE8BD16680C935B2A26E3E8FED8C535AAE830B5BAB8F4C9BFA87395B4AAC60639BFD00BA255A5E056A224AECD09DC9486DAC6889EB9C230AC28F24A0CC562BA514085426E4C477C39ABA3602E24B1C1DCD6C1602E5871F1654E62F9B2C2232F8F281E1939C4E782B9769917CA7C912C7BCBB132552E23E790C44A4446478A8E0C163958A4B312BB807401E9541D2103608AA45A20188C4AE56DBA2A29B9823C1AC396D0190CF002A25846A6C57553685D0F35CFC069AB3FDAEFAFF479AED3C1CED08DBF3C083ADBB9B7F9A2C36FBEE279BEFF16FF177AEDC5584AF8FD814D9798B433B8C316F64B2504B360B942DCF44069D326328C9DA2AEAB2D7DED9FA5A44778E9CBA2AF3ADE7BEAF4A5B77BFF06758E7A4956880FB939D7572E4AE2E8283A5599D7EE5EDB360693250ACEB8F2FB9F4B099F1455D56FFE035D8BE901F05E88B4F32E56F27FFC8D44C9A70D82499C28F4EE630CC23945E19CB2C41573221663F46C76418A0852164991684462984984B288E5B21C6AB39CC0720E56E039992339DD4F486E204C5D8D2812E767B72046B6E31EAFFA4153FA446035153C4ABFAF9DF7312174573216283102314ADD1FDDDB40A407552C487EAA139E75856950C522B50E532B91047CD4A067CBD63A3AA42F566E736ACD587B67B81E75FE5D6EE6D17F14CD473BB85B54424A78702FA87D077B7FEADDCD6C8BB020ABC81D1B8DDF47374A55104127EF6043DEDE87C48307FD44477F97D4B817805A18E277BF9D25FC102480391C229060EC1D9B08A62183324582220C609E33A03F2631471C25BE63CE950B7BA82A4D2DC4730784F12815A7CC6613E59A886FCA4DB259659DA767D0E1193473207EA0EA74560FC46D8B575B4869A19629CD947709748068C8A6E9288C078E68D13D69501F736057C0C9B20E8F2700530ED0040D76779B950A485105E80A6718EC0341863A1D70BB067CBC159C5CDCF730E9763D818ABBEBEE7A2A70D3E736BB1F135929E00249D2920CB361B94A9AB6E1BCB042D9235460E328680C5C4BCB4A39AA04676139330C66DB647E57B31EAB75E262D12F3B738DD94CF3B972EBEAE1C973D349636EB9ECF60DCF9F6B1873D56870ECE8A4C7285686F25A3181DA6BA16135B57DFF56392FAA93ADB15B724A740CF81343B23A44661A479652B54A38ECD1A66B5AA956AEA6DA8F95EAC7F79AC1EC4CAABE3CA387472612E9EDE57C50999E998A3417145552A7067724328FEF2B71CEE4B11327BE37A02433A2BF6428A584F119AC145304E1B842CF10F3C459732C939930522911A9BA21867C9C1E32E09ADEBF677E7A82AFCA55B2DA684DEFD89EF1FB52F284F85F6971292B30B2B107FA58AB015B5EC0B47DC6DEF82CB0D400B596207508306D0BDC01951BC01810F87B4EE08C00B87729082E051805ED9841C12A2E260A4BCF66E6A0AA441D98A28125B0A088B81842508F491D524D8A03230A54588082DD0C5455B283DA0B6C22EAAE4E8ECABA3E365A5CA21C62EA0AFFD48C24853839B5A535BC75F985ADFFA9594909B2720BF20AB9F9794B547273BB58E9FEB5D2357215E44EEAB28D105363F5F9FB58C352594046DF46D165FEBFC966F64A3C2C2A2A1C0A92698C0B4B64E514CD5DFF71A7482AB3ABA8B0298867316F939417E665E7E46163FEFD2A53524A5D55555643329545CC2A682A633E03030340800100621DB2030D0A656E6473747265616D0D656E646F626A0D33342030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E6774682035363936332F4C656E67746831203133353237323E3E73747265616D0D0A4889EC977B7055C51DC7BFBB7BEE4D82449E9D02E6C24D20105E014A2148431A203CC3234181109E49C80348E0121E02A34851D0225098718602E300B5328E125A7118A5321D99113A2D4CDB7F50DA8E853688AD866A2D2025D8F4BB674F482EF7622E8395B1B3DFCCE7FEF6F5FBED6FF7EC39390702402B6C8042B0A4AA28B4FC5B1FF402963C0DC4FDB164F5CAE0B1D0B921C04BCF02BEE365A1F2AAF57F5619C08A054062DFF2CAB565C712768E00FE3A09223DA9A2B468E1BE8B938E30E22132B4820D8935499759BE487A5454AD5CF3544DD9BB80480316CFA95C5652F4FB9F6C0C02AFE60019FEAAA235A1F450CF7CF6C7737C30545D1A7AA97ED12F810D5BE9FE6FE85CFDA37ED4EBC3AA99F3DB645E45673D0C78EBE3C7CF68FBFEF5174FD5DFF86243425DFC5056132061443F3FFE03F14EABFDF5376EEC4FA873233553E743BAE5A18D3880B6D8C4BD90B403F00CD07E24E79510CE834DA3C5EF00E700BAE32EE514E2909383A2A87D75EE9EDD92FA5B78FD4E523538E46B8DD911F16E36F94B27B658EED86D8873FDD38C8F9A1BDDD7FF1EE7ED137BDC2F939362E2F88A51E214DCB60F351817CD475D429BE6755F0A5E8979BE2D4889DADE0B0319B76B4BFEEA0A4FC75DCA7170409D4655D4BE529EBBE6F13784D7EF98471E0E381B5119116F4D93BFA8FBF258EC6FD75896595EDC5F1B1F7921BAAFDFCF797746EF735E41592CB9374A9D34719CD750A62EDFB60F533121AACF2C2485CDB90DFB629EEF0B24FBB39011D17E0643D55391D7552D424E58FD2CE6C43AD7ADFCBE8BDDAA1885D1FAE296A1D0FF3E11A69F631784CD578FB9B1CC219723D5BF07A9F16791EABCCAF25EAF9C89D458FCFDAB631BD7184F8FF727708ED19173E83EE772535B5C1A52D53B181211EBB6B57A6DBB1BCBE25D3E8B5B10C7B8E39DAE78567CD47056A3EBEAC5B038BBA3F9FA176277F3F922721916FD9ADD717CB358F237E1715532F2A3F9F80E87B7CBC3480E8B7909C9CEAAF0B6A873738CAF0392E37279BEFFD0F2783D86F93EDFD2B846A91790E27B33F21AAAC7D05BED8B7C9EAADE9875A758F21072E407A894535C3B5EBE8971E2047AC85DE823FF8E4A51822251D5708EF54A310F95CE0C8EBDE43246FBE918E21AEB03314AD4A2BBF6919BD04D7D827E723DD2E4667493191815EBDABEA912FA4DEAB7F73B0B2B2B2B2B23B957B4BA63DF025C6E5E170DE8E3B6F7C05BD2875D5F651EEADB10F23CFF27FC4C6C09CB6105FF871075B3E19AA6A538B20A9BC8BA88F66C3C4DD6B5342EA65C87E039DF2ABE0BCCE0BBD2E7A417C934F6EB90BA891EDA3AED10709EC77C791ADD551D8A49BE331D29EA3DDAA158AF8E60086D0999E33C88F1E430A926E524484AC9125242A6B98C4639BF293BAB1F608E5A8159EA107AAA0A14A937B0544DC0007514B9EA38A6F11D228F6C23A5A4980C27E5A488CC23F97A4C447E6931E737305A7E7C2F1B2FAEF31DE2E7C895351829FF8454799067E43C66CB9DF88EFC0BDBCFF33DE5A3A6F734711C0B48C1BDF8CA17304C5CC520390D997202FACB89E828C7D2271F03E530A4C8998C3599B1631DF75A43EEFFE05C8CFDAA635A59FDBFCA3981B27B8D21AF22497E88EDCA8F42958BEDF265B295F51CD66763BB38484EC3272FB09D75A78A7DABB0CDF53DE7960BE55ECC941B3096CF06477564B9165DDCFEA38CFD1C92EE35472B2B2B2B2B2B2B2B2B2B2B2BAB6F86F437A66BF99DA96DE377A65B8EE13BF3561C7E6FBADF9ADE77E6D7BF122B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2BAB4689DDF73B032B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2BAB70C910DA930C924A7A92AE248D74260F918EBA7C4F7394218BCC23533DD2493EC921E348B62E47F76E78F95EE6B6B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2B2BAB31A7E71BF33B0B2B2BA2B3944911E10BA2A3E654DB8758556ECBA04479C613D07D9F0B9A313D11D3DD11BC3310533300B21ACC3E3D88F1ABC8E636290CC08F40BA40706058607460613826D831D8239C150705D704B707BEA997AA7A1C19D3711414649435FC6CD43018A50ED4539D22CCAC0C0E04056B3282B834F04B7310A1845345C657ED56A86B79264FD233F6B289127D5589585210D833F7EA6F1EF72AFDAE2DAA2DA8BB59B6B3703B59BCE8FD08B75FDB2DDDF5158C0F981CDF8694CFBB6C3FD3DA8774F4D54BBD41BCE08359DD914A84A55A72EAB7FA84FD4A7EA9FEA33F52F75455D5533D55EEE5E3BB447272471E5BDD00F03B88799C8E2DE8EC14CEE6421E661212AB04248D146B4155D44579126F244A1982B16894AB14CAC12ABC513E28762ABD821F688A3E26D71429C12BF12671CE538E282E373FC4E9C13EF2438AD9C079CD64EA2F3A0D346F415E9629C182672E117D7DCBCAF796B6F9280E49FBB852DACDC787AAB768B3F26EBD5933C4C2DEC415390167603E1FBC1FA5A9EB2F0345AD8238E88D825CEBB34A6AB7B7FA49A155746763B69C6EAAB197B505186D66EC42B5EC323CD3BA1EF4483BEEE410F9D49B2414DA44D31A85DB4DD0DEEB5EF69D027003ABBDEFA24D0F63538FA2EEB6750D369FB1B54356DBA4115D00E30A84ADA81E46196EB68871BDC93936DE0C901467AE8E7D128329AB4837E4A193A91311E49642C19E7E53BDE43E735C143CF3FD143CF99EB914926796491C9648A37479E87CE2D9F4C23B3BCFDD5CC238F7A2C24D33D2AC80C0F7DB20B3CBFB5A4C820F4B52826252CB7F1FC89E8425B6A105D69CB0C42EF7DB941E47973105148BBC820E6D22E3608DDB6C420F49E7B8865B4558477895845BBCC209EA00D19F49D856A83D8EAAD81881DB42B0D620FED2A83384ABB9A3CC6F2DBB46BCC5AC529DA7506F73FCCE306F70EDDEFA1CF618DC13D3347C8EB2C5FA73D66509FD37F9007F742661804D719E86788A74F20DD10CF7C023C63018E8FE7190E0C26BCE6F17C7B0DF01A0778AEE24FF236483024726CB0AD41E710EC60D0776830C7A0EFCC60C8A0F8840DEA3EAE4BDDA0E5DE05794F385C4BF04983B840BB816C61BB8F761BD9CE727B20F58CC1C7B9EB6110BC97EA8541700DF5D2A09F03F5CA2086D13A063EE9915DB879D3CA15D5CB43CB9656552E59BCA8A2BCAC7461F1FC7973E7CC2E9C5530FDD147A6E5E74D9D327952EEC409E3C78D1D93337AD4C8ECEF678DC8FCDEF08787650C1D3220BD7FBFB49EA93DBAA774EBD4B15DDB36890FB44A888FF3FB9CFFF25DE6F17155551CBFEF4D96C9A6D35A4AF4A9BDE365C6D6491AA4554389ED90599A340259F1BDD0EA9BCC4C4859CB47AC504107F5F3693FAFFA9F7FBB5671F74E8B3AA9500A22E0521528BB6855DCA97BDD359E73DFFB0D493F603EEFCEFDDE73CE3DF7DCFD26665BA2AFA08ABED4695FB7A4D5C8483F97558904A565025F4B121557DA68E91B33B9D2324B96F367596643CB6CD3D24AC82131D4DF270B4AEA1379251BD6EC844BFCA1BCF2A43E6DF812C32D6953E8A14232493564A177212FB5E5CB822EEE5D080A7E9EFCD5BB3A732A57EDECEF13F5CE2EC22E22BD5EEDA95BEBB75A06ECF5852D755BC47BB8591D4B154A153D3EE116F24E32E91999C8195FBA2DA7DB8D2FB99B63160765BDEF78F0C14642CCF999EE8AAA9476BA3A56A24A41AC1004FBF5AA8CDEA0F27AC3BE677BA9CB55DDA7F2059D51E46C6CB2D980A55B5309258333828257A79F5B29294592B654E28C60E42E368789F46041B15184D4BF64926339D8C88A392AE8DA841B96A598730E8BEC40C6D3B6CF9AE3D09C33C39A1A34CDEABE4AF25415FCE8DBBBD0AB6B73B2BF8F46DF7C29FA482F752CEDCF9517382F550395CF87E336EDEA6C9E205B8AFA5AA89F3F40F6259F3AB19B8761C2D5036A8F5EA386430312489E83DD53AEA91255D36B725AF8E5A8961E28E4392E5908FC7C1820FB5213EEA2D8B474AABE593A473689CDC2E338F4DA1C4D4ABA10B89579BDCE772AB43EE7A5EB2475D6A3E1F3945BF5789654426F3845CD254D8BA616F5ED2C6B1873CFDB5371E9DA4ECCE3D922812CD28F1A1E224582A6CB1479468787A46B390266D44A64C1B4C20F1562A9DC08AB625C3537E224BD64F8F77F4272A2985A533ABECC578204CD98C2765E34B4D09A03DA200BD5FCB20057386D8D028CBCBD709C368F45D430D588F3748E40154BD1CE25994D6E8C8867B1576A312E5D55559EA235941D77B96F3CD6667EC7A6D4D8C4AC6B663B5A25D32B4AA17E302C699124350A768ED66031E3605A4D79BB29378B2367A947A1561C571054EA2296E2A5ECD42D03ADB9839EBE2CE3293D9751498EB3BFAF1E17DDC9693F477BB548C79D2A9614FD4B530C4A8DA5DA5C50CF66833D057F610BED8B408D560235E50E3926F849F756671FB7BD5A8C5963D3C3E4CA16C375651D98A867AD0353B3EE225F4D07A6DDC3B665E7FC61AF7E1EE9DC4549178091DA2C6521172417D8D32415E2C6DE59CC0A5133DA162330E572C312461687CC12E5861DCA12614369D35096DE8DE5464BA8C9C2BA8564F150560BADD747D671D224587354D045228C32FCAB0B1EE06C676B369EEDC876DB3D360D298B0E93E428D97658E248B7D5633975F23969C40DAB56EFC83A8BC6D3646459234B96D59A328A9CCD9639A2F6C28ECF3CDF839959F748B720FFE6972C86F98F5661EF02AD21BA4F0AB2C2EBEF166F21F03D3E3DC45A5AABF459DA525B85B6D5568AB8AD5B77AAEAB0EE52C32CDFC6F26DA1BC8DE5EDB4F2ADB5164D361FBA81AFE820A61DE30AC70AF75A8C5DCAC6D2D2B49B3CE19CF692B49776529A757547862EB7D6D40EB2DBCEC927F1765D2B97380E31E372DDF6D468D9A37D09876432AA3BC84347E4812C8AA60EEF37AA54A6B5565206494C4747CDD35E861B75777B66BF26B418515B745B3AF4D99AE68606BC60B5BAC01C3EB4D73B53FB39EBA0D8C4941B4A1C2A52635E3848EDDD14795991AAECCB708D4CD15E0E2F8B4E279454E9CC6F49574DEA7422A5E06EC5525D3D9DBA632339A48FB96B239F39ADA976CF0B8337A5FD9101B59DD05D14517AD950461568744835CAB1D0B79F4265D37BD8CD44434CAA9BE8E8E4A08DA77652EB9ED468896EB7B07E1749D4202AC7F910EC8A7CDC174ADBB9E7DD34EE742434966E57372797FDD1D9C1B71FAF3FE12CF23F355E70B6405F91E9EF8B9F2DED31E22088F7BC708570BCE23DCDDC08ED54996F05CA79C199F5260B7C55AA1D75FBD28CC92D93073B14DD20768A133D7462B47D92B2E2B115853C6ECEB21735B29619F1356D9C07898B50B2A252389981BE726571A1592C72A2C7606A63F886A0AEF0594B6BE52A475F432B13263C23329009B545F18FA9BC9D934F93D4DC16B4FC69D5F1A6A995A53B478B9D1C16FDA018F013B55C8A862D6A495F9759E192F685458B871C7177746D5CFA9EF4E9696A4DB8C9A443BB9172394FEF5455E2AB603CECCFF8AC79AA94025EE2825E2A9EA3DBE9629A2F5555926E10CD275038FA1C634BB46D8413042AD066DF16C998DCA769DB8D7246DF9E8C2A55F9093DCF2FE8AAA95BA470CDE8B037A7A0682F57496CC692068E8EBE39FE2907FC40DFE56768245605AB0379614047F02EBA3D5AD2E5CB7DBAAAF8469266AA4B0E95681046B9E491A3D0B023C586E116E068AECDD477B5A79E9798EFFA4C681C375E29B249578FC3C4EC27861B32DA3E779094DC796B72D6C5391563F5280D6F965695C3B5A5B6A7DD687A4CFD51AEEA60C2C26A24317748B4BF9AB70DEEA19D0E8DE98BCAE972885D3C653F68DF2F06C53AFB81287F460CDA4F8919FB49CA1FA7FC89287F8CF247293F49F923943F4CF94394DF4DF931CAEFA2FC4E31235AECA7C5664AD394624DAA503A44E924A556713579B24417D5B7C41AFB5E91A754A17423A50F536A25DB63A43B441E2D21ED0FDCD1D16BEDA0097D3FE07D80DB0035C07B01EF01DC0AB805F06EC03EC0CD809B00EF02EC05BC137023E01D801B007B00D703AE035C0BB8067035E02AC06EC002E04AC03CA00AA800CA80394009E003DE0E781B60176027E00AC02CC003B880B7022E07CC00A601538049C004601C7019E052C02580B700C6003B00A38011C07640115000E40139C030E0624016B00DB015F066C010E022C016C0858041C09B006F04BC01B019B0097001E0F580F30103808D807E401F2003781D6003603DE0B580342005380FA000AF01240112B00EF06AC0AB00AF04388057005E0EE8059C0B580B3807B006F032C06AC02A4002F052C04B003D806E4017A013D0018803DA016D8056400B2006B001164044602D01FE0BF80FE0DF807F01FE09F807E0EF80BF01FE0A3803F80BE0CF803F01FE08F803E0F780DF014E039E03FC16F01BC0AF01BF02FC12F00BC0CF01CF027E06F829E0278053801F037E047806F043C0D380A7004F029E003C0E780CF028E024E011C0C38087003F007C1FF03DC009C07701DF017C1BF02DC083800700F703BE09B80FF00DC0BD807B00C70177038E01EE02DC09F83AE0286011D0007C0DF055C0570077008E000E03EA000DF832E04B802F02BE00F83CE07380CF023E03B81DF069C0A70087009F047C02F071C0C7001F057C04F03F6AEBC3BBC9B20D03F87B2705B169FA26254947DABE5504C4305520B21A5A0823D041FB400794555AF6481B7668D9A8EC3D1507A2C6111E501145706F712F54706F41C53DEAFDF63ACF39DF77BE3FE03B86DEF9DDCF4CD2D3966B9F6AF6AA668F6A76AB66976A76AA66876AB6AB669B6AB6AA668B6A36AB66936A36AA66836AD6AB669D6AD6AAE67AD55CA79A6B55B34635AB55B34A352B55A3620FA9D8432AF6908A3DA4620FA9D8432AF6908A3DA4620FA9D8432AF6908A3DA4620FA9D8432AF6908A3DA4620FA9D84361D5A8FC432AFF90CA3FA4F20FA9FC432AFF90CA3FA4F20FA9FC432AFF90CA3FA4F20FA9FC432AFF90CA3FA4F20FA9FC432AFF90CA3FA4F20FA9FC432AFF90CA3FA4F20FA9FC432AFF90CA3FA4F20FA9FC432AFF90CA3FA4F20FA9D8432AF6908A3DA4D20EA9B4432AED904A3BA4D20EA9B4432AED904A3BA4D20EE51F369BA3961532BB9FC1995966BB9965182D95D9BD98468C1AC012999DC444315A0C1681856081CCEACFCC9759F9CC3C301744B0568F511D0863728ECCCA636683596026B6CC00D3C134993990990AA680C9A016D4C8CC01CC248CAAC14430018C07E3C05850857363301A0D2A410528076560141809042805256004280645A0101480E160180881A1D23B841902064BEF506610084A6F881928BDC39801201FE461AD3FCE05402ECEF5037D411FECEC0D7AE1F835C00F7A821EA03B2EBB1A5C855BAE04DD40575CD60574C6B94EA023F0812B40077039688FABDB81B6B8F332D0065C8AAB2F01393867806C900532811764C88C02261DA4C98C4226157830E9062E4CB60629C0893507D031990CEC20096B3690082EC65A2B70116829D38B981632BD984900564C5A3022A035434DE0EFE62DF417467F823FC0EF58FB0DA35FC12FE067F0934C2B652EC8B412E6478C7E00DF83F3583B87D177E05BF00DD6BE065F61F24BF005F81C7C862D9F62F409461F63F411F8109CC5DA19F00126DF07EF81D3E05D6C7907A3B7C15B327514F3A64C1DC9BC015EC7E46BE055F00A78195B4E819730F92278013C0F9EC39667C133987C1A3C059E044F80C7B1F3318C1E0527C109AC3D028E63F261F01038061E0447B1F3018CEE07F78123E0B0F4E432527A2A9943200EEE05F780BBC15D2006EE941EFE7B4D77E096DBC141ACDD060E805BC12DE0667013D80F6EC46537E0967D602FD6F680DD6017D889033B30DA0EB681AD58DB825B36834D58DB083680F5601D588B9DD763741DB816AC01ABC12AE91ECFAC94EE09CC0AB05CBA6B986560A9740BA651BAF98F313548770F660988E2F8629C5B04164A7735B300C7E78379602E88807A5087ABC3383E07CC96EE89CC2C5C36133B6780E9601A980AA6E0DC64508B775683E3934035764E0413C078300E8C0555F8D063F0CE46834A7CE80A5C5D8E172A03A3F07647E285046E29052560042896AE0053245DE62B144A97F9E35D205DCB99E1D2D58919862D213054BA3817D0108C068341980C4AD71266A074AD6606485703932F5D8D4C9E4C0932FD4100E4827E3285FF7FA7BE18F591CE72A637E8259DE68FC635C02F9D83989ED259C6F490CE0AA63BD6AE0657496747E64AECEC269DE607EB2A9DE6EF6617D019C73BE1153A021F2EBB0274C0659783F6A01D682B9DE677E932D006775E8A3B2FC16539B8C500D9389705328117648074E918C3A4494715932A1D63190F700317680D5270C089030E4CEA2019D8411276DAB0331193178356E022D0123B5B60670226ADC002086881267D8261D6DFFA44E32FBDDAF893FB3FB87EE7FA8DE77EE5B95FB87EE6FA89EB02CFFFC8F503AF7DCFE3F35CE7B8BEE3FA96E7BFE1FA9AD7BEE2F1975C5F707DCEF55972ADF169F264E313AE8FB93EE2FA90E7CEB267B83EE07A9FC7EFB1A7B9DEE57A87EB6DFB34E32D7B37E34DF60DFB74E3757B3BE335AE57B97FC5EE335EE63AC5F512AFBFC8732FD86718CF73FF1CF7CF72FF8C7DAAF1B47D8AF1947DB2F1A4BDD67882CF3ECEF73DC6F52857A0E9243F9FE07A84EB78D21CE3E1A4B0F150529D712CA9DE7890EB28D7033C7F3FD77DBC7684D70EF39CE43AC415E7BAD7B6C0B8C7B6D0B8DBB6D8B8CB163562B625C69D5C7770DDCE7590EB36AE03B64EC6ADEC2D5C37F3999BD8FDB669C68DDCDFC0FD3EAEBDDCEFE1BB76F35DBBF8AE9D3CB7836B3BD736AEAD5C5BB836F3B94D7CDFC6C402634362A1B13EB1D6589778C0589B78D058696D6BACB0FA8DE5E437968946B134D6281A44542C8945852D4AB6A8371A8A2E8AC6A2A7A3819496898BC542B128B6502C10F3C4FCD83C71CCB24AABB1AC0CF41173631191107145EA23D60B118A45684084BA46C8A2451C919C8835A95E84455D2C2CB47051B8311C0F27F48E87CF862D5A98128F369D3C1CF66607D9C0EAB0DD119C236689D9B1596266CD0C3195DFE0147FAD981CAB1535FE6A3129562DF4EA2ED59689FE0962BC7F9C18EB1F23AA6263C4687F85A88C5508BDA24B8525A9DC5F2646F1D191FE522162A5A2C45F2C46C48A45A1BF4014F0FC707F480C8B85C450FF60312436580CF207C540FE3E68998ECC9C4CABC37C2F0599FCA6342FE575F506BC67BDE7BD099A37EE3DE9B5A6E8194686A5839E4EF985E9342BBD217D43BA554F3B956609A475E818D4534FA59E493D979AD03A90DAA17350F3383C391EABDBFC989EE1A5C1667307C06EDD9B3FB6E169D32EA8BB49771B6ECBC0736E5AA5592987482307636DC57B8E90DB085A8FF394A6B5D088366AA5BED0D156DA8850BC5551659CD6C4DB9698CF81E28A78CB35714D5454961D225A5F7E882CF9A57157A8B802E395EBD6695979A178564999B4EEDF9F95571E8A379A7D20D0DC3799BDC65BCA7D5575913A5F59A0AFE63CEB3CEFB4BA4F384E392CBA4EBADEA45B023ABF793DD948B6984F4DC9D64072B79E41DD6ED82DE65393DDEA09D879C6FC7CED938A4A83BACDB05944AEADD06609D872F383015BA7AEC1FFF99C87CDCF8957F6D557F153555DBDAFF98B47E51431873E73D6FCAAABE7B1F92FD23CD67CFFF5304F9B8FBAFF9C8AE0CEB175FCA85793F5BE7FF583FEDF6FE0DFFF38A4F16F4959FF26CB0AED1F76EB042CEA320FE0F8EF0507900245C512EF2C8F2E4B2DF3168F50CC03A5BCF14245404551191445991810C103444B3CC8F22AAF1415D3BC320B2B536CB755772D5BCB0E8FCC34B57566BFF39F8199C06B9FC7675B9FFDCFF7F90CEFFF1DF071FEC3FBD3703713923003D3918869988A044CC164C4C388384CC2444C402CC62106633106A3118D2844621422301223301CE11886A11882C11884300CC400F4473FF4451FF4C62B7819A1E8859E08410F74473774C54BE8826074462704E145744407B4473B04A22DDAA0355AA1255AA0399AE10534C5F3780E4DD0188DF02C9E41433C8DA7F0249EC0E36880FAA887BA780C8FA20E1E416DD4424DD4407554435504A00A1EC643A80C7F54424554801FCAA31C7CE18307F100BC51165EF084070C281368E5D91D6E50100957EC290B6EE05FF81DD7710D57F11BAEE0327EC525FC828BF81917701EE770163FE147FC80EF7106DFE15B9CC63FF10D4EE16B7C8593F807FE8E13388E63F81BBEC45FF1177C81A328C4111CC6E73884CFF0293EC14114E0637C8403F810FBF101F6612FF6603776E17DECC40EBC87EDC8C7366CC516E4613336E15D6CC406ACC73AACC53B781B6BB01AABB0122BF016DEC472BC815C2CC3522CC162E460115EC76B588805C8C67C642113F3301773301B1948C72CA461265291023392253C70BAE2FC2BCEBFE2FC2BCEBFE2FC2BCEBFE2FC2BCEBFE2FC2BCEBFE2FC2BCEBFE2FC2BCEBFE2FC2BCEBFE2FC2BCEBF1A0F6680620628668062062866806206286680620628668062062866806206286680620628668062062866806206286680620628668062062866806206286680620628668062062866806206286680620628CEBFE2FC2BCEBFE2EC2BCEBEE2EC2BCEBEE2EC2BCEBEE2EC2BCEBEE2EC2BCEFE9F3D87EFF347DF3FFB2F709F3F2436D6E53F66B6C7C383C2C4F6F05C2A62C912D7470F899458994E66C9902CD92D2764A89858BD2EB9B252D6C846D92B05F2A5DCC38725DE305A1E74DF261E5251C47ADD7ACEB212F9065F979D2CAE2A96A9E5DCB196B79E2FB177DE92652D6FC9F7A820DEDACFFAB815B27B49DDB05EE71F5DAEADCFDBAEDD525897D37EE2A2E752CB06CBAA12F72044FA497F19200365B00CE1FD874B848CE2CE4449B48C9631DAD5185E1BC9F308AE06F15D0C186DEDFCAEB11283F1324126CA248A611DEBB8B2BD364EBB9E2871649478992C532441A63A9EE3B49D045E99AC5D1B314D12F964664892B62AFA6ADF31C9AB92CCA79622A932F3B657338B5769324BD2F99C67CB9C5BAE33FE703597E64926BF0FF3255B16C86BFC5EE4C8E212BB0BB5FD45B25496F13B637B2D9B9D65DACAF6EA4E39205B64BD6C90ADDABD1CC65DB3DF91A2FB3242BB8731DC8304DEA1C9E56F6CBF7F71C5776B1AEFDDF6DED21CEFD4C87E92CB4F4C72DC47DB779AF84EFB9F62FF1C6C7FCAD41277622EEFC1BE76BE23FB55B6F6FE9DBBAE77E576BB45F763B1CB9DC9D1AE6CAB92BBB75A2F90259CC03778B6DD55DB6A396BFB6A99B676DD5F5AFCBDB9DAF59BF296ACE0B358A5AD8ABEDA7756B25E25AB39DB6FCB3BB2969C6BD795FDEB7A59A77D721BE55DD9249B258F4F72AB6C937C6DFF76AFDD6C7FB3637F53F1CE76794F76F01BB24BF63069F651D1CEFBECED76ECEED7F6ECD7FBE403AE6DDF65BF3A201F31A10ECA27F2A97C2E1F7275487BFE98ABC3522847E54BE5C3EA88FCC0F30D396C382DBE122862788FFBBC58C224EC5E4EB7920F4380F84BAEF5AA35CE7AD5BD938C50A1FC17722D9F529EA42BC5DC287EA89AE25DE61BA92479D62BEE03F85AFFC671438465B9F5022FEEB6A7A26F955B73B74267EEDD1D9D295D996186FAC57D65CBA3D14D5BE3B1C6B329CD73E6D5CBCB72E7CAA6954DF36EE56DB1F7C045670F0E7764B9593E092E5DF21D5BAAEF6F55B9D1E57E7256BE85A3D5A5F3F3F05B5054054FAD96250AD25A546151C5EA14E6D2779586B8B4FBE6F9D7A3B72B377574DCD9430D1CA5DCB46F1F4E28AA8A779588E2BEB017B0F25655ED53CDDD5975430DBF9A35E8337BB5D24B57BBF62355EA0C7EF491472D75EBD73D52BA7A074B573FA77E4E83BCC7973C71E8C9287B4FF57ABA86AD86CD9F092EEE5A51CFCEB1D5C8BF54C18D821B0734DEDAC49B366B599C3D77EDF9C5AE356D718B7EB4F5C2047BCDFA3B6B9E66AF453547675A5C2A59CBC456A1AD635A2794ACCDE0B6EE37EDF3C0C345B51BDBEE7451ED5FFE433F7588E870B463D78E875E6C17D4246879D0AF9DFA77FAA273D9CEB9C19EC161C127834F7609EC72ECA54E2F6DEA5AB96B7EB766DD967657FFF51A761FA8A7770F5AEF5A0F2F47ED28BBC79EBBECE7905A213D1C25842487E485EC2C594F9F9EC38BCBEB65D0F2E9E57F9B468606BD1CF84A72EFD97D12FBA4FCE7F5EDDCB773BFC7FBA5F63BDE3F72C063037206D61DB83DAC5CD8AA414D06650EB20E9E31A4F9900D43DB0E2B33AC77F8ECE18F0D370D3F3A62E0C8F22367460444AC1B25A35644368DCC8CBC116588F2893245FB47E7465F1E9D31A6D2980963F68E0D8DF189491957755C9D71F1E3668CF7187F3076FE84EE13C327B58EF38BAB127720EE9071B1718571BD719B718FB1C058683C613C6D3C67BC62B4DA8AF78AF78B8F8CDF31F9C9C9855382A614248426AC9ADA7A6AC1B48A5A43A76D4A7C20B15FE2E1E949D3AFCE884F5249BD92CE9A624D09A664D36CD34253AE698D6993698769BFE9B3577DEFBA68BDFFC592B7DFB1BDC905E6207377736FF31AF3BBE6EDE6BDE602F311F331F329F30F7431252A2536B56CAA5F6A95D4283A35B3B59E9E9E9E9EDEFF654B662E496B4093D3CEA69D9D1537EB627A83F429E917321A67E4CF6EE668D99C0E73E6CFB9ACA7A7A7A7A7A7A7777F37D75D4F4F4F4F4F4F4F4FEF1ED565AE79EEB1797DE72D9BF77BE688CC9C4C6B56D41DDA92F5CBFCA0F93BB255F680EC0FB37F5BD067C15EADAFB56EE8E9E9E9E9E9E9E9E9E9E9E9E9E9E9DD7D0B7DF5F4F4EE9744C4206289752F34F88ABB784A33E92ADD64E1C6E427FAEC141FD5532A4B73B5658B7F870E5E4F79EE52EDC54D6AA950F112A5DAB72D57C6CD675B40409B3ADB9EF3C870F7EB9CFF6FCACB2EB66DEB8AE3F7F243A4284A2245C99628C9B6449BB42DD7F2976CAB492D65F95AB238AE65D7CB92285D9676C3526241DA6DE8EA200DDA220F0582145850741D3A6CE83AB44190CC92D3680DD6BA855F86015E37187BE8960C01BA220F75900EC1363891BC73A9ABD8699F6640F71E9397073CBFFB3FE75CE287E673C2198641B9EAF5EA52BA7A7D25904DAFE0F4B51BD76F285F2CA9D9F4E08DE51BFD7DD17C50F75EB1E1D18C71C5CEB0AE3336ABE6C8F379B79DCB33C2191B9C8473297D29B5944E2DA5C04DAAAFFF5B584DA8CE2FE8630421E83292BD4CC63287070707C698CC9069247D8C736D6878648C1D1C6861D860E3CA1843FEC7EC5FEEED6727AA2EE6A4919B19E45B747FD0EBE2995838F0D0E60E65EA40C7E6DEB8C00A2E961785CE91AF25BF616F4F7E22A8F150533C208A81785328AE0AD5BFF1BED57FF1BEBB5B39FBEE39D6B5E960AE9D7D4D1219CEE5AAB48423DD9B12BB66FC9AC27934456D1285802A776E3B583D1D8A111FB150A8EEAB3A0E388DB555EE241F444964A2BF13EEBF43ED6B37E76505EF312AD4302B6BB7E73D60781A8604465E2756874246AF33CACE98EFC41DE4768F078FB71B66C71DD92387937143F2E2264E46B22233978C0F8C3F19AC211B72205E083CC63F8672B95C209B4DA78B45B539AB82A90E2A2B03EA607F1F4E1553CE1F4AA5A2F916702977DCB137FADCE827DC7074DF4D0ABCC0E6753435B99C1DB3D804EB638DA4690E8FE0FA36350B069BE07E2462A5A3B5B5437373C7AA9F1D6525CD88C53BFC58C425CE1BB15ADABA751F378BFF813F7AA429EAE3584176E34DB53FB8BD6E8EF7459BB892C727B2ACE8F79CA9CE829A2F20C461D0750B4AA151F41FC236AFB786153CDEAAF8C9E085212CC3D006A45A2B4C6FBE530FE5E17E280FF743214F0F59DC4316F790C53D64710F59DCF31E3380D0DAC265B0913908FB54869530DF2EFBE9EC75E67F976567BE59F6909951F2DE5F7A163C8C47B7EEF4F70BED15EC2E29934315EC9913A6516E25E7644C16A78B371CE403CBA9BA41322095ADDB248124BDDFBA63830B85F898B79549817829D9E0061227E73C90253913F47146226966D4A1E1C104B00E91E46961F1502F63182AC91C6DDDE470EBE8C491E3BB6A179BBBBA9AB1F9C37347069A525BBA3307B777D6AAFAE8FEDDA5C5AD85E1C8DE8E9D4F4D2EAD6EDAB7D5C4CF3CF2BDC25877A8D5E25EB05A7BA69F1BEF9DDE391A9032851F3038BD2713AB158D4D13D56B0FEFDBDC5A1B8D8D1410C2E8F0DA6D4EE65BA0DE38B5A61C439B5294628A5284F9734211E65B84628A524CBDCF0C221F0AE3344A2013F794B429EE2AEE4619D4877BE7DC33507C9657C80FA7EBB894BF2E02B1B944B882D3653BA19915DC336F6B5319AE82BBCB76C6DD57C1BD251B9E04708B29F223720DFA5C1B2A872B442B09A931A1600B436811E97232C38BC1FCE3B3BB4EFEF1ECF8D4AB7F7E7EF4E8FE1D51916739D123FA06268E4FCC9C79622473E49503E3CF4C0EF905C9C55E51C2015FB0CB8A4EFFFA8B377E75EFD2C1505B77D4A7E981604C735B696BFBE90F4FCCFEFEF92D66DA74A92D08AA04D1F259D07200B5A2D71C25C77309AC117D6A449F5A10486901C0A48581917695E813E975A23A25AA535DEA54973A25AA5F6554E406A272C93719AD60738EAF6BB14170B9A1BB6274CE0718E579DB37C99395259BA77AAB4B8D79406AC206619D9D79EBF66F6AB71C5975BC7DF38DC9CB43C7CE9FBE3477E2FCD359E6F5B7EFBE55A80BE89B6FDEFCD9F72FBFB4FB9E3A76EA43500A44CE9E80C87BD04512F79C6E519D58342A8B4665D1A82C1A955561D4BCDBADB5696D109C5EC162DE7BCAC40B26FED8C4A6E98A401C25EFA405D39CEB7EEE158F3F0D61A79D0AA6D01C24EA311D071E1B14D7C4C2D3DE8883C13BE9220E4AB66B3DED1E3F54A40262BE92784642FD92C99EE024AF58FD2901C37C57F48A3C0F43CD854B22D435CE0DF65E068B5E89DB198806C43A2431100D06A2AA583BEA56625A4057845ABFA84649665D585B65A78197855E7478091AE5A5515E1AE5A5515E1AE5A501AFCBDE386A890B105159D322AE0AEE2C272723A439D04E9E5E54B31BA86864E9651BD626C9E279DB590D2DE07EC7FE4ACC8D86DCA0C24E43FC420D364680181D3B2F06DBF470322802911DCED5452D06C17E5D50A2212DAABAABFF14BC02CFC3C05D2430E210F781B55BDCB37C1BCAA16BF5FC88C5FC61921F61921F6152BFC3924C2C88354CB4E1451F58B8CDCA5BDFB658CB4F29F929253FAD3E7E5A7DFC9492BFC20CCCA787F010A481349F4C66D36357B104A728097795B25341A82D73E919A226A8406A1D1AADE5CBC5E2E2FD624EE825898F776DE2841F03B36CF359A982BBE6EDEC549A782AD9E999BAAC1653EA46A20FD4A1E11195C88CD42987B34A2AFE7AE5E2B867395116E4D1432FEE7FEAFC8F73DB9F7BE7C9CDB399DAB2AA726EE8A03FF73405A4C0C307BFF344FFAB9FBF39537C67E595DD2F3CB95D97B8435A5C13CD5E73EFCBEF1F3BB1F0D2B6781CFF24D90E1B208A4A2C50D374339E0CCBC50BB7CFBDBEFADBC3BAD1A527A902B947E14493469F919D98CFF56343A678658A57A62294A908658A57261B136B6EF7909DF3909DF3909DF3909DF390CAE6213DB419E543D078F31A191415EF4179B88F9A2B6B0B65B841E677E15E7377019A634FDEBF20E38F652C3F78D681545FC961E8AACB644BA8A8D753BE182D7717E4FAF3369221EBE52F9D719C44CF3532DDD1F44679D7DB4308AE354CEE51319808EB6D41B15A062B42242E0693E148222832E38EE8C1D261B340DBB2C88C553F6AD8DC270DABBACAB81A36A58DF701ED103A4C685FC9354F345F6A6611058E28704481230A1C51E0E83DA8FDD2DAC215E0262905070E40592FF865E72244FC40A08D90F0BE4620EE50A239B2F1F5D75F19DE5258BB853F85B7EC44B3F5332EFA3F5E2F0EAFA7E2F1B8CF28B8AFE201A441ABEA9DE369A787A274FF75A365A3A0B92B78A06C6B7CD869EB7CA3ADAFE78EABF135E07C36ACC7F2696CDBB1426CA437E911788685EE2D468CDED6645F9B520F5273E31DE3A7F6F7BBFDAA2CAB9140137C0AF8037EB577720BFB0B1231C9B3460DFE2F443B884E39B548ED2745A78FE8374DAC84447747A2E14B347C89862FD1F025920E72C82A2424255A50D6CFE9B9466B06A5A6C8B1DCB3710D3D83AF6F99695AFF63BCDA62A3B8CEF09C9DCBCECECE5E66667766767667EFDE8BD7DEB5675D2F8B8DBD0B6EB91983D71417926D48AD0A431C82B984F2508A2A4A1E481A2541694A2F691E8AAAF492D4D8D8A14D4225A25495A82A95366A2522AAF2D0174B6D2A3589C04BCF9939BB5E131EC2C3CE61E69C23FFDFF9CEF77F1F780835B1FBF6FB183B00B24C7E62F7C583892ED9DE483EC84FF007C6ABC4342D2AD95D626302FC51B08750B362BC9CED9995532D3D5EE5E9EF6CC30EDE4ED1F0854B5356EEAF5CD424DCCFB7436C348BAF6F117E0B0A3F86C28FA1F06328FC180A3F84629E70786AFE2590C30D1B146EAC9EBCA7C6A04FAD56BCB605B76E236A39DB615B75AC5C57B2ADEAFE8442C4765F5072C006FBAB660D775F730821EB3C991CECA983C45FCDF3F4EE1F3A326473F5F42885029757556DE9731A2D749CE1642FCF7348DF38A46F1CD2370EE91B87F8C121C2C364510920F627FBC79DAAE22AA8BD792692198F7CB9295FC3224C57450840331FC08CE56D8D84F28642B188B25B3D58F13D740F75759335173B01503483210D24D6B46A33A58122628C092493637D91801293585BA3483AFDBACF1FF6396D8DCD00EA58408534E90A4E477B92AA039CA4C139A71649059EF404257E551F0EDCBD60E7EC24051D313573F7FBADF7973A93BC9609DE9B242F853B034E87A4FB7157394D0BC406E2929918D21E8F0FC36E3E3DF8E9329FFF46B0FB30EC3E13F63097CF1B087643F5A01F38D1F0F26804A718688A9708AFAB71794F9A0A201F833866628460FE0CCA85228A61EE0716A8784513530B4A78FD12B2EC7F08A0615229A6DAF8499D76F93557494B2712FEC674B41AB2D96CAC1451D588C87669353D1DD105B05EEF377A55005DA11409C85191DDEC0B89AC5337D2B6DBE56F0E6C7979DBBDFFB62EE4EB9938A764232BBFEF9BDA5F2FECFCF94EDB3B761E194B28553653A9DE87CC0E1159E255D32F26198C2A83C9CC60323398CC0C4695419029828E20D51193752FEF02A37A147ED3A14B9A23840E646A18864FC0647AD93FCEB719490BD07687CDA0D90B3370BA1FCD9F9F31173CE825130F1A48AA2D6490EF574EFEF21B2F39A45800DDEA4E0DF83B771C7C7234BB303059EF7AF5076307BE94245F7AFC8787071BF916DD20407665F8D153933B0FF5B9573ECD6C9E82B84CDDAFD2CFD031E8A20788772C3FC9C5C40CAA35836ACDA0BC9541792B83E89381F55638221AEA099D09912103436860080D6C260D6C260D0C21645D715E8C71AE6EE4FA94890EAA84C4CD85FCE3CD1B08AA722B82AD7AC732446B1E2E52D0AA8A6306AEAB502E57C9143F97D9EEBC37B51B08B732848DC680A599F63C6BDA717B98067D6B232DAC9AE319DFDEE367877A5F9E7AF12FE747B65DF8F0C2F93F3FBF45CA0E756E3DBC25E3631BBF483FF2CA9123AF3C964DEDFBDED1D98B5FCD1C552202131BDE3718EEDA73E9E39F5CFCF48DC7F6FCF4A31F8D5F387BA47B7053DC23256CB70FFFF6FCD8C47357A78FBEFBEC8EDDCFBF4D58ECA39C907DFDC408F1B68972D89B174A2C84A684502E998C2A21D44B08E612C46B315B81FFCD0E0B085B381230C602A6A980692A608C0548D3B950DE0BE3DB95231550A9281B20BB1662E30A965233F52DB780369A8A6A013D97AFA0A50B3370610CADBC328397A2AB6E52B3DC26A069324F7E86A3B212262DB849459265D0974AA75298B29493F125C35ACCE7A44EFABB87760F1C6BB237AB00A9B7AA6D3F36964E6C7CB41CEDEBCEF88EBBD9C6CAC8AEC070F1859F8D4C6D8C403185AEC30165ACB76F7238B1F2B716AB612EA249D7BA3D4F6DAA1ED8B9DEE7CE0D8EF536FE99D4C9EF8C1E54EC4C633436B00BAAEAE6FBCBE414E4F95690B1BA71F5FEBFE63D5E305AC5705631CC55ACA9550C6B75C9D655C91915C907468D0A7467492369F04115AD0DA29616F47AD10F5C12444717BC6AEB457DED72D03477D72E07F0D3673DAF78908DE7F3BF0169A204C354AAE214A22550AA3879300ACFF25A8543A3925012E441143AAB413A3B21C31B601AC165E477968572199AC15CDDBBEC4512B3EAEB45EB43CB2CFCBA945F02E9B91901C6ADD4E28CB96B166DBB3863EE4BA38D9BDE116D9DC35BAF9A486A8D89EC6B994AC6BF36A031E4D4A693AFD5AB4F4D0E284E6810597771D7ECB675F54D49A376F0F074AD3870F085DDB9C91D831243D948C669771646EAEBFB77F569C6C4A1C387268AE08947BE3B65C8D1B8DA119175D11ECF24C2A55DC5D2D8406F7168F7ECCEF16FEDE9F604229253502531243942095DEFD9D8D13F366814374CCC223FEA812AFF01BC6771CB8F2EAA157840AA80709F47A6FB734B3E3227C2FD6B0BE89E31E212C85CD6B1AA1B3034FCC784F7BD9CF73AC2788ED1D18CF919BDA9E34613BDB698146B4A9269CC3E80C68C6D5C689A4F38821693863FE459161A34EABA1412D8BB3F6EB1FC6BAC1092245D6459518775BE7E7F993A05BD658E58B0347B7F378822F588223589225A4691D38A224646519214DA93246431216328640C858CA1903114328642BE6AF3A2DC84F22687E8E9805B70A99AB7165CE5A4192FB17EE7726DA16A014DA483ED246BCB27ED16DDF7604AA14E7DF1CCD28927DE3C3D6206C6B8C4764D9CD8BAFDC478CE442D0643CA874FBF7566E3D0A92B27C94413A97B1FED3BB7B7BBEB2BDF9E2495F6F418870A3C0D114B12E72CC492487C3349A0A1674A031905A45CA02B00BA541058C2E2600E9034ABCD37685011D1AB801A50531D919A4A8B569A14CBC38208ACEB83AA27EA7550AFD773F55C70B1354D35E7212135ED27853C537F7F9BE9346499B1DB16297720ADCB3155E0ED64632F0BC44C3C14131D143806C0419285521A49BA4836EC74B32480D9C3C9527372D04D91AC8BBBFB2E358CDED3EEA08C6AEF81B7E27FB0F6023166D59E2D806C1EA4549052405A061902646B09A7A0D784B6E0D5DB83FE7AF80F66AEF6CFED990BFEAD8CE5FC4C370D4B2981B6825AF500F28E8B16B3F168D2EFA41AB71BB768DE9F0CC7521EDA051E6FBCC1DBBDF0AEA7648E0132F0D19C14D7236981E21B6F0EC99A878609D561235756A09323698F26DB266CC372D0439176788742E00EEBB29BB5AEBC87AA0D9B4ECF477412B396D21316D1094C7402139DC0442730D10974E779487405ECE011D1E121EFE03B6A4146AC31F87C41BBB62EB6BEB50EB52D7BAEC2003BA152ECEF2F49ADD3DD6AC5313FDB78D1497BD2B17087ECA42F070CCDA6F406E649A714D792592FED041F375AC406B76C7F57343745D95D5CE3D92F1C1F28CF96C0D39C1B064FB726438FE18575DFA15290E119ABF2055549F329D7920D541C4A2A0ADF3B53DC926D00DAFF5487DE99FE84E745FDEBE2343D6D1D79615910CB2050506FFE033601B1AC796F5903E4FFBD7005FF7FB6AB34368AF30CCF3DB33B7BCDDEF735EBDD353ED707BEF1DA2C213EC295D838011A8277D7850C0D2AAA088602010A85420321214DD4368A922AEA9FB6AC9DC65210A82A25E16A7F14A9A2952A81545AA90D715B99D8C18CFB7E337BD9D49667C6B3EFA77DDFEF79DEE77BDED8AC545AA3F2A0AE0A162DA2422CC416B9802AF6530E562443E49F59D2140D852AAC1CB9514E6EA0B49688D7271A080EDF4EE99C31BF4B749A798EFC2EF14B7CACD30E95928C4EF3C53F353A4464AF8DFC1D6F60499CA4E16439246B318CC4DE87CB075410F3830EB66259C5D5471B3F25488CC702847DC266E3ABA7882EA89777C7EE25126CC57D53BA71861D4392A5D8F336E4CFE1E1AEE92EB2E649AB3B11BB2741A4A9E2BE644AB38D33124423D52A1A20902E4A0C85A3055B9EF797245972E79632A3FE81B771D5D0688FFC5B6B2462C5E32F1E18AEB644968B55CFB485BFB4D5ACEAFCF9C76D3D715B87A7E5D9D4A5BF36A71A7D7863D3F0AA86B0C917223F0CF9C2A9D19E58AABDC6C02D5B3982BF2BB6C7EDF2654F4DA73C50D55BEB943FB457AD409CDFB9304D1EA1EAB166EC65B40B3927169B225624B53AFBA33A5FB78FF085A77033188D2C311B4CD4278844F514DE7C81DD8E753FBEBDE50BE502B6F0F615D8835FFBEC8F249FA02CD04A423641CC4A0916C5E72458007B71A50AFDA95E902A73DAD41243A0E839EA00F208E76EEADBD222E50E3EB5FAD08454B7B1BFC3ADA1582DCB47BBB7249FDABDBEBA6E784F5FD7C6AEB89EE168F247BE903BE4B5AC3E71EDF06B377FD86FF286DC62C8EC16B840C4DF32767ECBB6F3E946BFE867042FF2D68805F3C0023316C056A0EA2F6216A20DC33037614D6A34CE3943DA3347AB98A322EF22A07506E79C6448D39E3989CE035C1CBCCAB02D1FB9E6FB4E7E7EFA9102A370F2374752BF8A0F1D97CE9EC91E7BBE9A089CBA79AC47456CD5D1CB07369C1A6B9F7F90C8BC8DB041F91920BF6A6C4461A81BA0B126AD9AA02568C134EEAFA251C635AB4FC76699122FF1BA5BE81CC9B3D212757F254198DE352BE9D30C74215346CA82295FCA49301F4B1E210D96671EFF1DD54098599E0531E159792B3EC642E381CAB2F23BF847E0CCA814EC36ABD6C39A3C66B3CBC8C9375993DB22B84CACFC33D6E4522A5BF89A9886CA446C9D52192DA0CACC5E9EF7605E0F3D27080EEA5130ED40FA922FABEE8AD05628CB28D07312C404A9479212E5EC2E56845B99C5F92F19348869A351DE8BEF6274A8081D239FE12CC81458398067CE6824FF1209CA939CC96531BB21F7E738B5588EBC1EF28550EED2C203F201D58025B15D28F709BFDFE89C222C392C6E9C225A93DA6671C645C36F3D186753AE3D6B857EB850BF3D8F10C246ED1850C83A540CEF12672465413B5A3129B567EBD19A9C54BFBD8015720665E6A7D426CB5B04B1640FED60861845434BAD453EA0590D654C0CEE1C1C3EF95253CBE889F5B52FC5FE55C0107FD11E3409A175CF0D571EBC76AA6FED996BFB567E7BA8C5AA254F593C26CE57E1EBDCF1D6F3DBCE8F35DB6DB81FE04390B2BE803C6AF5B166B7851F3C7575FCE0EFCFACB5050296401E5B30A951F00C2F28D88A3AC036E788F0709BC41CCB321124109A278E906EA4A80D2AC213CB32BA88AA234F1E1BDD65C324B863E14947182A3E927FE26C806ED0CACA7E30830ACE60029DAEA095C3EFB1D6A072983A8ABB71556E2F3C93FF29F27C14FF49E1395F213E0015DAB048DE2740655A5346A905B0450528FF39BB17E75AC80A1F28E4A24119402EC50CC8FF32EA13832D2CC020F135BE99FE1E11C57E01B2C41051017D3FB6308DBF07DF1FC79A4ADFEF33881927D2665AA11A348C928798A19D8A02D32A994A2C62164D67A5DCDE73B7BEB8DA5D5FE9836460DAD2B09CD51371FBE30E2D6C97DB1DB47078EDF0B7D654B3BC81D7991C46BB5F607546BD10E94812778AC9AB3C380D59B662DF50B31489AD933535F6D6C64B4417CC593C61C5EC9896184DEA317B3C13E6056F4628F2A14DA183E9DF0D77EBD029AB344A7950B997CCEF6F0CFF3F54508D847AD4E2381CB6A7398BE8F2461C7A5A3EF004235E618CF6A0D31DB66860CCD4C81FE17B188E219D2C6805094E8A101E7FC93D0194DC8C5F87B7247ACBF006819777CB1ACEA0D7E6359CB803BBE02C6065415DC0EA33D0E2860BD4988AD52D052B7D86426F73123556C0AA02A99922CF456EC395B86332CA016BA494379AF4564540A1C89B85B4E65F63058F8A039D01AD6DC576A00C26AB6D3531A0C4425213D6D7696B6AC24D5AF49F80859BD335769EF445D3BE6F9AF240749B1D2A120DE6B62E40A20DFA139C9D22C24BC39D85F8459C17F182D117174971D9E002DED64667584BD0E10A9A5942FE0125C66D5EB38694DF215873D0E50A98D9A8530A54879C1ABC92C21B74AE50A537EB8A94BA66CFFC519D0EC8CA90FBE74F14DF7E160EEADC71CFE326E273FF32371F0C17F4691A10E9C006157D0A99A7F09B392F553F85DF002A7A9767F8650E74AA9063E554CC2B53922FFF7C110B196400D0541613ADCA5CBAB45AC5CED7922530C9E9B07B77206A92FF115F0B831081B382D7EEF4A16AF70B1EABC0C9554395040E3F8CD9EB70FA04A6371C0C84087EE0DDC170FF407FF8F1A5F25A39A3D32447D6BFBF213E34341CC767C0EF521487A65802CB2E3CA052706A59B018F634AAFB3266253A4054FC70D5622EDC983366C529DC98D78FA2E1B96074C1DB49C998A5D1C74521293F8F0A4AA2989E320B47A57A0E5E1C1FFF645F67EFA18BE3DFF9787F3217EA7F756464EF80181C80FBF86088F01FFEC3D935A9E3D78F1DB875664DEAD8D5D747CE499DC957CEADDF747E6747EFAEB7904F03C47600837DE0D6D7A85E9DF914D44380E43B0132213643D3BA8A87B6B4AECC30DC2E5873231D830316E6C68A879212B2C82D20DFB6D42338FC24DB148D45A30507B7A369F4F5CC9B05B9883A71BD980AB66F4A86277A57D8EAEC6FFCB4A32FE122FEF6ECE14D75F2D972481856D7B826D3FFF43681A6E59D819601158BCDD465C0A2026BC3B62A1E421314A2305FE5304F3DDC2685A046578536DB9E6D46374A07985C51FD83EA1DCC4A0F4E429CBD4A81C79ED5A1D00909C53ABBAFA8B6012AA4ADF9FE536F05A81441043EDAEDF852D02E33065ED83BBA365C79E899F14FF616B133572C0F37BFDA6330C87F2CA2D807F77D83E1CD36BFADB6AB5B744452DFBF71ECC00D40F2F8674757EE7BF985486D8F8DA9200646DEDC09A8BEB16EF3DB5267EFAE7379547F0CA83682934AABDAA8256C93095395D0344574E6A21D8A29347AAB84FB1D1D8EB687A8E754552A4C6077FFC77695C646719EE1B9F698D9999DD9993D6666BDECE5DDB53DC66BEFAE6D767DECD818DB9835849070B8AC5D072F886A858300F508256A0E2952D5A855452B5AD44BFD51B5122A3E60A5FC687E80D4368A941FA66A151285220225314DAA8886C85EF7FD66BCCB0A6A4B339FFD1DF37DCFF7BECFFB3C29E4C1B27F33CA83A8F5B93E2EC3C850F66179732CCACFBAFFAAD7894407F9D49DD7DD980F20697064E445BB27D6E40F7B18723FDFDC399439568B02908BEAD75FFF5A67A0BBD0E5DF1A0B0B8718DBA79ECE5DFAF9370777A714C9066444D24EC77FDA46926A754F3D2ADE0907E2A3C7869057131CE14EBDE55FAA427C18EDD794EA2525A90343EDDC7840AC417CECC25E33711926C4E578269E71062A44CF02E604AAE2743A3BF828B0DDA21D05FA765D09499D122101AF7346021BE60CA0595F59358D6AD6D40397B3C65CAE9CD5061F958DE9129ABF5896904EE06AE90DF103734CB3F67FF542A6FEB7F509F36625D6FA8EBEB92F3D53E8166C168200EDECD83A3ADBBFB5D01BD446A78A53636D99C367C7DB9EDDDEE534FA691BDD3AF06C3AA1B7CBED6353D35363ED7862E2F49E76D1DF2438048FE00EB8E94034E06DED8BB70E24636DE91DB343FAF18956C1ABF00E972C48E0EFD480EA89A503DA6047A22535328D78BE09E26B10E22B84E50CCEC02808A7452F4F09506317FD738CC115293C79EDF3EB80CC02E5471D4B65A3075144AAEE261E8BCDC75A1B4CD120EFACDEA2C5B0A206DDF6EAAD9A1522EEA1BB26DF8F85D75EA9DFFA39BB0BDC90DF6543451976F74BC345C481D174D379860809D8D84BB89799784928F91F5371BE46C5CBA8C3E26F64E17C230B3F1609EE27551DF960E0C42F5E98B9389F83F095D5B0648FEE98C966A747C27629240782920DFFD9E99F1EDF962E9D7F9978B12621D62FCE9646229191230789F906651706646FC2DE23D82EB4F705CC07A1F3EFE5882FC4F83C608574C6E10B94BC964D452C42015592328A4255F8009E7094ABF57ED91880D2D3F42DA880F6F434A88394D76BB5112728C6170F46DA6496AAFEDC4AF1CDA160D46D23F114018A80764702813047D9B6389CE80E9C2C79DDEBE79066A3D77E4D1E66383B6971FABDB0F7DE8DAFAC36D87B3FF66DB4779D4E322CD6DFD9C9A62AF8673AD3CFFA642E168DB2910AE1D55D32DB5B6A2B75461D64A3590189533F9192CC66C5AC2C986D312B6E8AA327E7C9B5890D2725A3E4536796D292A9586B2D747AEA638A535BB64434D9417E44AE50ACDC120C692A40F1771B2EC6435BC2928DFC82F894B48BE1405344B4915FE2B749BB84507112D64D540496F86ADDC2F24F20C4ACFD9EDCE7E0D07F397AED0F669B72FA7D80560FDCF48F01AD3476C68CD266621BA6622D04A63329259D52E11773560871416619A85CA022D9503CCE6E2DC5592958921AE5BC924C27551902C08801E023434D0AA6B46F9CD028AAE0F466F5221336032C00AA176F80CBD75B470927BFC793626B3018F732963B0EC71DCAE18EAAC1169174E2EDD5DBAC456C8906221EC6F20F9E5DA11809D4729CB73AAAB7075599B3902094F0EFFA7CD557A1415A3859C63FC0DFF1AA4E43F9577FA5AAF8340D3D56A7EAAE66011B01B03905D8346363264BFB08698163D50A812F34CB18AA5D341B2CC956B164ADA541F2F3EC0D747E9401F5AE7A0634F897A702C248028596223E0592B7BAC4DAF878644BCC43536BC4179000D1A648CC6971E0E7AB75CEC1CF117B8CFD232BD389BF677758298A5750168C601819036E6CC5A6CCBD0BC4EC95901B7E31D021471798904192CA1C13377808D994944196B7DE455767DCDA424841A396CB30CC123759C9302E29933B8D7A8BCC0B62CC5E1CB56AD4E931FD18482DD8066C75FD941CA4EC0287FFB31A1604A45989322BB156D2CEB3D5308139F91DA25FB007B78479AFCF2F11EF869B25C4A5BC9B6BE53D1E455AEF8A206575786395CC937F3534C54323BF43FC707038394C3A685F8665F1C90C7C6432233B508B17F042A682FF57776289048FE12C26F0F82496AB6C7CB60843E17D6F91DB7C3BCCF7129A93AB1076DDEDF25DC7324286E87B3B8363193C93E9186AABE090FAEF45F048840ADCEF9818B8C94E5258326F28B76471D5859E27A78B485C6BF0734D9B2E66014CD44E016F4C17FD3AE7F0E119DFF5325A2F622CE82D6311DC4BC19A1D81FBE58E0976E06619AD2B27F386CA4BCE4C1735B4B456347D8815A549777783E04B776F8ABCCDFF504651B09915DB9B4EF5F49279A1C9AF069D7D3FDA3B766AEFD6C1D3BF3B7ED6DBB53B3B30BBB38BB5B33465F30FEF3F9A997DE3B9F86F7F3032371C3CF4CCD0FC80CCB2562BCB4EE54763A347870A2F4EC44633CF74FBA15ADB058557026A3420B53F7FEEB96BBEADF9D6D17DC323704717E08E6E584E626DD8007605DDD132B02913EEA96C3C44A8F720F439E36DA08EFE3650EFA9E05FEA7E8F26C2202D042334748B9A2CC003DD9B5621189DC63C4C4F7798B280CCB15C894FF8478542169A972D934629858BF065912242906B8F912FFAAF9AF3E268A24E97CDA9163417C27AD22CB680B62FDB5071139EA74BAFC9DE3527647379BD868CBC913EF2C3A2B67374346117FD1E77936805BF2B83DFB5B7EC1A1F6F79E1FB075A2E7932FBF5D0A0BE23317276FBE0C15E05BF7BE6ADD7465DF15CEB093B8B729AB55BB6D54CDDFA9DD66D5161F7AB7F3CB3E395B901B16D3855BDB0EF40FF919790BA98028C43E45FB06EECCF86F669C22A1B6F2F0252F0FE08A18BA15806F8B00482193A120876DE783F40D790D8841FDEF7D184448570E85CD2893B95BB419DE1C683CD159C589226C84FBA60ED259A1BEF6AAFE0D6CBF424529FDAAAF1C0934513E96B0867C4F46C50B95B361790D00A57CBD24417F949192DB28C16A1D12A0B657AD294A19AF130D9A4D1C080EAB49AA2D35A332E08773244586C4AFFAE83C9D99F94BA874E5E38A4ED1DE996692B21727CA2FFF9DC375F0EEBC5FEECFEBCC6DA181BF91B97E2E2945840D45F5A3CF3FA9FBED327A811D929C96222186E095FBD74E0D5835AB316B54B011355EBFB966F6167B015C3B51D9BD9FB8D0AFE70A9B3772FD654C11F2D261233EEB7F047981D14BB435767B4D5F9B17C6E4F8EE82CE8052257C815C6F2F75273E36370449D999AC49AC848C15950106EE48411A0A8F0ADE6D3AB2E44B3C5627ED5107F90DCB7521FAEACDC7219EA490FCD6BABE5F9313E17CC11584128102C69AC3E97BF5786F5A78C0FB0E5FA1748C50015BE61C431FA88F63FD6CB2DA88DF30CC3ABD54AABE34AABF32992001D100249485842609082405A84640A0AC8760C98E3C4F17A901D621C9B34B6E350A7F634EE0CBE70CB852F3AE94D6FEA824FE9C11734A92F3293E9F8AAE3CCD43749C69D9D697AD5A431E9FFFFBB0219C76DED7434A38585FDFE99EF7DF7FD9E2F150BA243407A8CA510E7F313C758B319A1FE92DB377C3E01F289EF6EBFF131914C669F4F904B6C244EE4DE1BD87F6AA05EAE03FB93CB409A23B9D6EE537D803FCD56B75EA66CD07495DBED9E16A495DA9B1C4A0A5A8DA49A905650D9CE81325236CDFE648FB1596F36B4CEFDEC50A02F5EAF16C707FABBE6DE9D7C745FA6846F8852866B8AE3BD9EBD238F2E54EF107FC671579C09A48A118AB6D17E97D3E3E2356E401A1BB5569DCAEA7520379CFBDD8924493A32CD3DC75E6A95904A4A2DBC59D275E08145EC2FC803A37BD2FBA0075CBEB469E1F7B81F9BC554C001266C0E67AF574CE0B347F1013E04D0BE1558C2B567562939DC6FE3C673A9C060008F74A63BF14067A0331EFAA2AEC4808893AEF717E982A480D6DC5A1F40D1042FA001ACFD32FA576D8D25BCA8F2B88D63C7739A802B80639DDA4EE00B54BD14FA8205F5FBE101D759E104C8593B5C000FA94E6EC4A5FF17E969111F8AE04152240224B34E1AEAECF6060B25DD3CB743FFFAF0B6FEAF9F7906FD4523A4CE5A6FB102C0A3349BB745F32A850D66A59854CB45FFD854EFF440FB48F8793CF0CDA7A2E30AB55C2C2695729545BB797BD34B1B795F48FE2099C77E88DD87BE58C3160F0D8AA13198C42005D6A3AFD2CA58576C107C160DBEFD304C4D8BA9E1C1613C32939EC18767866726CA9FE7979809F8E6CA8F1563168EEA6240AA10EB2DC50C27CBC274808688D6DA021A620319828E691F44B51FDE7BA0DDA0911F6C9A61D7308ECD6867800950FDA5F2E72C38E1183A42CD8233BA2C1C0B4E6981C78099D752946538161C0543023A23FAB83F60466CC90A184ED0B4A18136986A6C40F0866910786F8B3DFE67EFE0099321F4F2D952E9CDE1E067903D68ED6789ACD9E330CA2432A998A41CFEA83D379D762E6A74845C4D2E5A5B7A028D3D21AB332297E03A95DADBBE151ED5A0AF1D0BC03C83C6A078CDDE1BEC991F0E8546CF8C8C93B44DEF716F3A8F4EC815720965D139EBD56A25E91D786D4AF4B5DBA3B7D1647E7739617744B34DED43514A67ADB50E3F220CB5C30458A75D0A99F42098C5AB9223980F4B62BF454CEA4A758A94F62424D1A402CCDAA4560BBFC0F44D42A449C2118261617E528785011D16067458A0D3B030A0C3107BC032935526FD76826A82C062C903AC257E4315518E70087A5202730605F6E1A927ADA83E68814FAEB1963C059F5D63D1C39614424C33BF8C55B3A0962CC19BBEC53EE2AD8C40CA26C4AB24ED30181D3A59EECACBD317CB8DD1A99F4E0CBE9D260D2E483EF2F7336FF6A600E700EE79B1AE2B9DF55BAB98B3581C2DBEFDEBA9850FCEE5FA32B89254931209F87AD40708676A29DD7B7616104FA61574770C74F70A20FE20D6863D44DD6D0AC753F1F9B8580F1951EF062DD3EBEB9AB5A065CDB0BBCDB0EDCD88FD01697CB5DE1BFC45100F82A6AE43866C23044022040E42BF2BD195877F02F6BBAEAEF9A3D3C42502BF43883E214404E108DFF7E52D0F0F52150AA7E40F1D0882C604EE3F7AAC0AFCD14F833C10415A0F2201EA89E68FD8E3A8862F7C1F70276579C8629496C23562CA217FC83A781282908F687F0CCD6AF01ED53DF50D02BFFBE3480B527CC56F7D74CD99AD0CA567FAC32A522915E320BBE2A347D3F3BF3CD6B1FBE8D5E9572F1F6C795FFCC662D781EE7A1CC7FD7503274643469B91A4AC3AB55EA3525A2DFAEE93374F2EDC3AD3D7FBDACFF7EACFAE840AB309380DBDDF7E8D2F8369B81B3B0F7B7FCDA4858089C0D22E70BCBDCAEF7601F0ED8271EDA0FBD7224DDE9BDF7E92D6696951C1ABE0E2399B8F8B30EE82964103300AF927B811FB9267C8D8064C363AAEE058F09F111FC70AFF8B465994A718C1A5C889B511551D6435B9847A45E0CB048815D2E80CD8BD6D6EEAAE4C2997E83477C160B258C0607A8B5F48DF6A608EE41B7A3C2A9958A2D19B29895C29B7C4863AA6F8E4F8E66FD5012336F2713136FEA3D1805AA3D2DB610A78013774804ECD6227B07534219A9A8C9E30A4C6A4E240055CD7868CC6E41C9C0C4AACB73BA9385A3940480EC36DC430991FCD3AB97C2ED4C1F5326D050FA32D2CD42E35A851D5DD6623867831F6209A0423814E8641CF6EF0D52651391B0BEBE59D1C0B2AF676706CB5A664E1F16D07B5F43F2D3D52E9B375DA2C70C053962469070808B824F535CA7430386892904BE5B148ABB9FB0721FA163F0A6EED14A6A9BC3C666B8F05CC945844D26E1BFC5B20CFF4FBA77E5C6EFC95092E555D2FF6F9334BBDDD7BDBBF6BA922FE281603F1AC6DC389A7AA79706C7924409024A99029548AFFB68121BD65AF4BDEC074D855ECEF48EF9595F9AB50E7F5A58989FCBE69F8D3BC7E3E1554423890E7DDF979F059C26EE28EB46BD7F2E9A5ABCC2A77315B995EE29699E38543857D4C5F21A5540489080565EA284AC0E54624EFCD96386B0E1941487ADE11D012A2F0C7318118112024D137F0C49633D2EEABCCC5558E85272D2F71ECCEB33A2834163A8A11785C5AC9C203AD258E0547229708F381B70BF40BB4CA0E61455B06A8C6135D25C267F554DD53ED653203D4DCC60B23F9A7AAF4BECC52A67B6FC2F6AF38B48A1D58C5441124584320773622A75C2837C690F7FA727E526F371A78EF2980F74CC07BBADB2014085A73BBEABD4D76A70B3DCCE15C738F5101F241A19619DD8DB6FC6ED1BD1DCEC0EB27C7DE19E57D247FC247535BCE942BA133C1911FF2CEDC3594987ED2994FBA740456D7C809103A7AF70B20A1E62A6521A7893BC08D93D832F2622E575FF2C0FC0DA94CD07EAE97EA75F53A2C190BC9B8FDB91233C8A5B20DA63097640205474185C258B0144CE48DD806B41488986D1F39F6CB38163E9B1AE458F87432CCB15BCF5BB6FD91AACEAFAA50DF4BE5BAEDDBC41DD87817687CD75D3E2AEE3EA35C2B5E7037983102A6144BB46E870C36F0B91B3FB97D4FD0402A0513E008B68A3488C512B30698FBFB9DCE0C0C816B475A12E072BD98CBCCEAAD5A48E9C657F213593F57CA25325C91E92AB430566137DCCE7CB8167E0CA4A8A6BE0EE9B1064ABC826AD85858A4E4E7585006AC126C4D214BAA36E61FDBFD76D1E8259692F4F78FFB781C5DA5525E0503698AE45ABB4FF5554579DE94CFBD37B0EF54A1CE5A5503D714C77B3D7B471E5DD8DAF69E37DA07FABBE6DE9D147493A5C1BB7319BB87743B7F7E7A6506A677A554EA2E96E1049FBE3CDD8652BC5BD53D0D3E95E04DD13FD32FB84E2E5456984BDCE9EC4CB9C29D645E2D8C158A8CD9962C780B1120CD0D5B9ECEE6384935BC53BAE453A3FBC9E076AE30A72F712C2C7EB2C2B18F97B7C1FA20ADE109921CC7FE9BF5AA8D6DE2BEC3773E9FCFE7F83DF1F935B613C73189E3D8B1F3E290609B04EC4B72794F44505ECC1A42DAE51894971646B5AD2D152A1FCA262AA44993F6A5EAA77D010A4B5769DA0786A6A9A9A60921516DEAF2058AD069639FBA0EDCFDFEFF3B1BE7859490E9223BBE97E7A4DFF33CBFDFF3A34BDD5A794959AF5EDF9A15B62C2F2D846DB85755D8A425E72EF51F7C6BA0466BF5A33541EB68EE8BA5CEED034138ED35953812443A76D4963F543B366BC332E74FFFB6859CA01FEBD89DF663940EBEA56E82A62A8949E23852D5D5FDC467AA1F113AC2077D6072D48FD4646B8F358DF2835277CEDF24B59BE876BE5E7062F3DF59314B458DACC6FFFEF8CEEA174811DC28DF3D2889707F7B93249A32C52790CBEFB856C245C68BEC288C532F409C57B386B63867A36EB25CC85B1DB2EB74F650B537C4B1D62D58C8BDB68F6BAA73EB346A15506171053DFB77AB189753FDB9A71E21D47B3C4127CB3A83FF6DD9AA9EF2F4D4B2BA0AB3C3E2F7305A06FAB3DB2157947E8C2BFA16710DFB7464247206D5F193A6D9261162D5B19BBA26383A7DA8D5BE3513C173AF774FE719DE44D3BDA7A4F9DC0C3F25F56523FE4EA9974F08C56297E61E34D69562C965639A57BFC0BE945B6E26005019C09A3F258908AD6F4A12115E6FA7249610D74C4278D0B5F2C29C6C418F7A9399B89133FA316B2FE7CC8E39C3AE2A2C7D9F877C450B3538F1A8CC02A59100504AAB7406C6EAAC77EFEF024A5D2F4CE9F3EC62D830443767BC7059F1D02271057BE880E2A11668CE8BB5B58BF371C4B183EF49C5AA5E9B4734F3796928174F493C9F14A047ABAF3A87E5DE1B57E8B5241232C32BD06AC157B711AF6EF971A0961FCA4B2220F0294944184E04724D0414B9BBC68B9402CEF359D528CD1067E31DBBCE997A75B8AAA1CE5341532AB546A7619D0DBEEA48B569272EFCF6C9CCA9F1667888AD309BCD56B79961759591BE3EAA7B5B9E2C5C563C7989F823F6E43BEFCC7D9047FE5BAA05AE8E0157A3F023DE3C8688F25F9ADB3BB7377EFAD8D248E2171FF017A573D9FCD89274DA449FE617842961006AFDDBE47033DFF86CDF517A2132A842DD4A319922F6CAE229C63C77511211EAE925DC1F65DC2402865989A09D8DE59B8DD23691573727947E0E4D9635447B551B887E612B6FB4EC5303B29EA3A9CE897934301627F088AD87A4C0ED92A54031200593BDC6551DF19AB6E16E6540AE57C45F3667F989C0178562B29A9050348C0694D2468D6CDBEA72D6A25721231F233EC67AD9B72F9AE27F4FFE8798206CAA7A4243D441DA8A2E4599659578D312856322B0AC1ACB789C73531349E9708E9F90E6F82121C5370A9A3ABD57D0F71159D4C4A15D17337359B0C2B1EA717CB54C268793920830731392B811C8F10C09591D09022725B2B8CD6A1892E4B8D2564BEE283B53A388267292B13A6A9D8E5A4E6734153E258FEB75DACA1AB73BE030690AEF15C95469998A585B8CFBFE04DD16521234FD3A3AFBE4AFE41B3A034B51B06DEA1DE6C2EF0A418B8D850B6A8A31B0E4BF0B861D4469253D7BA0075C21EE21463F25DE5389372EE5F35D4BDD2843F3E130178CA20CDD25765DFE0C383E4F54A024CD9DE5BAE0E07588DE203126F0E72BE88B7D3FC9FAA413B9257E519ACA764779698CEF11DA84206F29AD45A5718DA95A9BA281EAB5393ABA8A480F29D0277C9288C0A7162511C18FF192B8E605F2BA549ADEF80D652BD336F8DEEE282F8BE7EBC4A6F5682B6B5DCFE480A739C8411F95E5F0C219795D079073795B883351C55C4E4EC0984752B4E98C4690E2317D856B53A5DC9695B2BD94BCA6178090D6E7F34D952A2B8CFA087AC608F12AEE193E5F20AB433366C4194063A5AA33111DC8564AE95C202A75F20D8293A78BC1AE94A1EF943A806D209BAE9444B8BB332A89A5FB1D45D3A7CB087F295BE3DBA88F76E05C17BAE6DC812F11A8D26DBF026FBE49FC1A552EC38E8FC7A23E5F05AEDB8D43D168F7D10A54C837F33154C8EA6CA6FB0414F2482ECF4F4B423616E896B27CBB5056D167E62B9515F2500217D7A2A464FF89EC1128304211A62511E164BB25B10CC9B1D665DB29F8CBFB8AFEAA681E4A368F6D87E601C276952DB5CEFFA33B7EB0717B25309FFF54FF069CB0405CC67938D3BF4C7E93D12F040244EBC2823E3B952050A7E5CCFA2194786CF9C10C9FE077EFE6229227D74FE8258ED7201E3189C016E253E1F116A2D18ADB267078358F015C6209C113914484C1E92511A338140265183C2AB7A446BE24F35A24672DC5382DCBBF8D6A1987FCE17A3602B9A3FDC1AC57CF5094464B6BABBC0DEE60C26F22AF68810687DFAABDAF356869ABE9413BCF053D36066E52B3666F4394CB1DCE5453CD1BCB3F3B77A158FE4ABB9106021C89D1DDAFDC65C0AA6A56CFDC95392CE866F3AC8EA58D0E6B75ADD9C86A8203278754469915CD0370D9CF89CF717F4AA55C436E64B0D0CC8C4134A20DD535EC3AFB2EB258A3E1A8C10547E8A7444B38749617F993275B8E4893B9219E973AB2EFBA8D21A985AF11AA840B687B6106710C46849556D434B61E224C9E79E0BD35230F59B04E419E3C228908BB03061D466F0949A28CCF5CC09B0DBC016761446669654D2B96546F7343DD8CE3ED51AF79C09A7D4056F670C6BB55A60DF047FB43FBBC604A35ADD53036248356BF91FCB00EF411E80918903E580D53558D2F803E982A19E13EB460DA6ABE8FF55105FAD0A8A7CB09DDDAAB639B8AE5C0DCEC85030D46A3721A1EC0A7E79FAF2182A08836D86FDFA7FE44A48821224F72B843DBAC919CD9440EE6B47AF8F09B2B492197482F7FF7CD75F80DDFFFBA6EC2DFFFB8892EA59961F8376330594961D8AD36C5A804C3C099EB6672D0BDFCDD1F3206F8279260DC6E26115113E8DED60A72909842AF98F29BE1B1A9C660A602BE83A6184325FBBFD48F7F6DB31D4A520FBBF9467FCFBD64FFF43DFF301221482F2DA1162FDD95AC76C85DE1C44A387C2B6C071546A3E1B0054E9A57C2F0172E7EE0E88571F5FD5F8A7A9B6DFC6B118177530F45049FECB92726FBFDD3F74478055221682F1D46EF089B6F87010F2F64209062E2AE0F696013E3EC762F652B5BD43AEA4189EDF813A988B3C38646B6D663C1C9D2AA6CAD0F858C94F28B7ABFD2F476C0139FFDD950C7BCDB6ADFDBFEA8F7F85873EBD2C7AF1FFDE52B4DE69A167F4B341EF4D5B5CEBC2D34E47CA4D9622914166663B9A87D61BA858FDAC7F3A30FFD0D0EF6FC1B030B2937752AE0AB3B101D3A33DE54CD599BBD8166954E55B3E76057EAF8644B3073B0B526954C389D42D39E43F5C1D99EC11F4F44586D4DE1F1CCA23FD9B7EBE0115F07FF746E775AA575461A76D9F6F656C75204A1220E813E7E45FD99D803C9E70252C7B5B877047513C26824B2CB2A5DC6B0AB9A184BF6C553235E7560EF32495D8BF4C3AE4BDD08088E47B44C5B1AB185B65B32BA7A0BF7884EC48BA1ECC9087AF41331D21F400F67583120D08E4722AD90920E2B08CFE6F4DABDB7CD4815A940E7ECFFE3BC6A639BBACEF039BE711C5FDBB9F7DABEFEC80DCE871D7F5D6C274E70E210FC014E82ED78F9E02350418010AA0E19484B4BB5916D6A80AA5D276DEB26554CDBD4FDD8F8B1AD2C242529545A7F5468FD53A109FAA71F2A9A2AD1A24C5B2775D53492BDE7DE731D27C09816C9F75C9F7BCEC9F5F3BECFF33E6F85E28C9F06415DD8CDFCBCBBFCAB72C7D15D097B1DA3AB01821A23F96FE63247B637870A838381A33F180FC6277F7C2834D83F18AAB336DAC546C1F09B1DDFC9A5F775BBC5AEBDD9966D998140DB898B87379BACA285E3ED663BACB0B96D0DDB8E0D1D0B257D5CE9DCE5C9D3D7CE0D0A6D5B43278C405C06C8B8F2CF5DE37D9333E9FED9A93E6B684707A9A517C155BEAE7F1AC5D12F14D54E77E1B06D69F5EE3C1004C6BF2D703C1E829BAF08F3C8C4BC858C506F9D1E130B73261E264C2E13B9236B4DE4198B32F00879C26E7E09D72E460ABE01B7D6B900833030E58E4CC8A15C211207A5F9B03B421603EE95E5AA19B526D739A32EC5DA18D614B8D23124122ADCAF431174B9A108BAA2F9F6D44C4EAD893683C1A64E0FFE28FFC4D9A1968A53D171A5899C6FDF9EFBAF6833FA1EE2F789C4DEFFAC98DFF6E4CB4748265E58FD171ED5C790885AD02582D662DA3BEC3DE5651C043080C1417152BEDB9451112107552C0705D6715DF7346A44A28AA6487789F4A9A8C12E029457D9A60CEC6C5AC2A905379F5730FC6059862C26C8DD52AEB24C009C7393456F96D55500DD0D793D6E14261BB18744294022706A2336B6CD5B7B65F2A9A0C39C37A85818707B6F3894848F9A37D0A0BD0D58F8D47E17E1AF1758FA8258E937E7B51779E86B3CF8AFABFE23242BC340DA92FAF012F0FF35403D8FF6A0236A96BE850ABAE92B23C1034B30ECF5A40027450DBA88B730811A1C4D79BCB9FCA15D04B5FDA5683EB9510D3E88435304D6A15A106EDDE16F91F7CE982A0794C80957CBE4086FF2A1A270232E6B073D4E17D6125513823530AA62A2AFBA675EE97AEA974F754C0C77380D3A5D8D9EAD3586FAA772E903DB3CADFD0305FFFEE7073D5A45B7CBDB6352725B2EA059C0FB7F823886E5AD5BC3BA1CB9926FAA68087681ABB7D70B12888653118DC9B6844FD8FAE44B255DA786FDFDBFF41EDAE11342E976DDB3DA5C2503B40FD2ADDE5EF9099E82F8F8503BBA4CA2333F1CC76D4BAB5F90DC86F14B92DB30DE5572BA8D50C144262074B219D17588720069B98F28198893CFB06E378A47090FA2108FF96053DE4E6CA11A4C60831A454284775546103E2CC09E6054A1046CD0DB159F47E3764356E3A557E355432BA612AE871365D493991A6C8EB88C359831180DB55E674BCC53AFF9319B8634377576B75CC75A04ABC5DAC01BF4F6C8CE3CF3DB070984B082DB2830C887324A1763263FF52AFC52FFBA9F473314F2F2AAF268FD0FF91F7EC6635EFDBFBF31AD10335021BAD0BCE2CDCCE9040E75E08E8C15973A96566F2AE1EA204A675126BE207154BE43F83AAEEB02A815996954CDB4809869B8CD34CA6652341A1C91082241568B87A3D5A40FE61B0704AD7000CB70EC5D281731FEEF24C8F14F35ED3B089CAD5EEDA2CBAB8427801F523040C1A8FB3660EC70303375B6D606C9EBE26A57CE6F5427BCBBCEEA6E75B95B45A3855BB9864F5A4C0DA44830068B117FB96279B074FCFBCFF80C6B01E2184C46B38B5FB9B6D2268848D34E405444A36A15710E3B4F391944C1A9E202A54085138AC60E681AAB65FABC32F57F08EDDABBAA6FA5BF093E6B047B94384B569E780052D9FDBCC98C87022E729D1EC303557EA06214489DB3D13A67A3AC567C82C7E3805B8F27CE12BBC012BBC0924359C52EB090F48B231901974652017A6C801E1BA0C706E8B1010A50E03AFE1A0C0B8F6BAF140B3EE2192CD9426A20D2938F0CB9ABB285283D85494EDE521D077486329D01E810B991E68AC4782C948B85AC725A7D79FD715A3A11C17F8C1579943711556FE2A409A7BFA95A1450EDCDB968F2743F61A3B3C566706CDE114D3E5B712CB5D646A763136F18FA61BE677FAE9D8F8C16077DE367F24D6BDEC59BDCE05D1E9C59ABA7CFEF196E8865831DB9B00D4CCD90E6FF20EA71B4A4449D53A34E2ED40A6E8C2C75801B3300222D794C3CAF3942D25BA90651F186F07C919A42C5E5B19142D8EDCB6BE1225D55C515525BA845489A538DA1A95CB5C7A56E7A5C3CD6C3FF686B5801FAB5D263ACE13A3001C4C3C4193EB1BACC7C0228DA5000BDA7E0D8980EE1A0158704ECB760BF19FBEBB0DF80C30C0EE9B0878006407928A81E2A9E30FE9580EAA1A07A4873E389B198B5BB60B99D406A6F8685762BACB2135CEDD7742C42ABEF2C72A8340DE1742F617C852B7897B06E4E5F422A030E52586307EF541493FE49731CD9B250E60A7AB2090A4B4975364AD3B9D65C12F80C0ABE6B8D0CF349EFE9DF3F73EAD72713C9D3BF3B0D63F71B52EAF830B4322D52FAF8F0CEE3B966FCD9C9B75E2C6EFFEEC2333016609CC9CF4E26BB0ECD960AB347925D13B304BD8B2B3F656E037A61E8F1E6087A6F82BCB524589A6B2CCD3556D34196E2C32A6D88281348640289EC228F65028C4CB03322914D6C69A9D1B72F61FD557F41CAF3C360E3F4149AB4D214E2D8ADAA5E24AE34238BEA363FD9079E4FDDA9275B2B00A5957EB00AA5C043D24EA5BB869B417028B599B9DD79F4D589602E9BF155E59F5D94AC86D050693432F9FDF1E01B62E7DE4C730AFABCDCB777A4F67737E0CFCFBC7D6E906FEDF2AEA434D5AEF95C6BEDBE154E85C4A1F3979FEB7F61AACF06BDDDCACF76EDEB9B9A5119AEBB04E876A217950E6F7A0BF67314528E22C969D07214738E406B45191BA9C620CE88608C1A00F1B68C512EF839B1392F12E62A32AB946579AD979B9395856C796DA58B2A6815602A248F00AD567749576BACAB736EF289EEF62DBDDE8D4C6DCBF62637595A7C9BCC350C66261D1EC16834D6D9A343DDF7FFF02057CF2572018EA9635963BD04988CAE2EEBDE074CF298573D4DAC982E0E17BF57BC5CD4672904598A5196B214C677485397A59298A54E36BB843FCA34F9E2BEB85922CA2711D19388104A444525C25AE91AFE8AD034C3127B63CE289607BEFAE1BCB4F9B259678E7EDCCDDE134684C3C2B4C0740BDD82A3EFC3ACA40F151C77D56405F496856432163BC82FF30AA9655AD2642B99AEEA04336DDDD18FCB027BAF8C045E6816987AF5C450DF8765E54CBDE3AE96C6B057568E3D340127AE45A7665D0BD35569696AC5AAE88110D4EADEEF9C98FD46FB787FBB83ADA935194C727A6F4F3817970299913DA3994068ECEC986F676F483430E084A081694DE463E14C480C66C6F6ECCA04707D7F19B2C4E9B6FB9A6C6040A566C9EA4DB4F9BB824DAD726A6FDF9623F9CD66ABC89B39072FB87983C3EDB079DB1B035B82CDADE1BEDD488DA6FE84FE147A15FD8344F38FA8077F848EA1038079164DE34F177C21DBD90BA491E8E5DCDC89ECB1AC8DE36CD96335A51750E9ECCEA6E5E7067A0E1C1F28DE1B1B193B3C363DC644C7A263E39DEFF98F17C6EF0E942E70CBEE9D2F13476E5415350E97CE4A30FEC379B9C5B66D9D719C87179114258AA42ED4C5BA50B2282B922D5B8E6CC77663C946623AF1A57193C65823A70B5CD4F09416439B7598BB0DD90AAC79E8B087607B58B1BE145BF7300C4DD23403366C0F06860C70D102EB65C036AC2F498141E81E06F496C8FBCE21652BA9E3259502F1F0983A27FAFFBEEF7FBE4FB52B0DB0910D308412BC34E806A19F54FE091D25BC714BB97FDD3A976C36EC8D168F0299456531B50864C85E6B83D71BB0DBE1931F3560BF88AFD98858E20552E88B8E1F97E1637007956AD723F6F1E7B23388742B4069B0D261D477F30AEEC937A49B66653B139DE238935103C4B8F03D779666794FB2A7149A5EA926D67D1A277885EF44FA26F33D53FDD14C5CC03D8698AE1CE984BC7788F43EFC8D83918216D2FB4FFDF0F8E2F3C7F7DD10BC22ABF96E0E59A16C578077092EF63135A44A924F74658F3E334FCBA96E7F54E58F8C9F1C8E750D1CCE578F74A512BB44C7E8DEB135FAF543A6CB15B6CCC9A78FF5F53D7AFEC432AF46FDDDA996BB7E5A748B9C1C86E832B63EA6CFB2BFA146A90BC43FF3949AE9753CA1D7F18A5EC72B7A9D93BDD7F1D55E6CA31EDDDBDBCC58716F53B7067018F176186D62E31CB4F3B6BCB951C671024B371BF0AC5ED5BDCD866EF1032408782708A2CA66BBE461BFC47C2F6AF4594149E5FBF4C32BD5F8F76C66DF6DB70337050F517B785AEFEE0A089CC8B18FC5D38A7CA7D6EFF1F0142B7A60B0AB46EC17DCB7A915EA2239BF176BB5F2CA20FEF191F92EB34C95D3F0F62ECDAF58CBCBAE4173BEB9640DC30FABBAADB9E26C97156ABAA69D0319B2ABAC1D28413A0D6EE058874CDA2C433241DD5EC202F99C2596E69B8D25CB5EC5DBB09771859A0D58C8399D2165C84AD8D31C71583BF84D932443A750ED43C8E8D02CB897C8CC68C63A3B939EEAF6C001EC1238A1D01FCBEE4FF9AE83B29CE6FB4BFBB86A153A64BD3703E6CDFAF28F1ECD7B7D1E7FCCEFD7654E93C283C746CFD85ADFFAF7F68916DCD15D8B1BEA3D21515B5B980AF33157A24DF41A45513C9DA57F4151362DE6EF10D1356A15D3BA54AA29B8B32C2412051F76490F5329D42CA5D01CAB580110F872764EC4677D736213BC0F95CAFFF8108405288488171EAD149A8DB16AC5CAE2C7AF34C8F361CC20BA09C685099443C1AFA2F26B8910B4463AEE945AA5FB543216BDF5B307D7CB89E277D80FE0E8F8A5A38B0CBA5C2ACC2F615592DE496F17BCA94AE138356FD5ACB1B194D56FD1D6925C68562C0D276A76EE544782E358DEA8430882661BA541CD89E70FCB6DE922F63294A558B4C4589525190B09326A8E8CFCA93BD21F87B4B201F68FD7BCC306EE52F4CB07425B5163BBCE5277F309F61D414DE4B1B74F245AB50EC16986F7257A76971CFD11B7AF10EDC20D6225F2CDA16930EE201837B6124391DD8EEA1D30D480EAF57AEF850321C1C3731CEF115A5BBBFB0DFF38F8CD8FA9B708A933CF4E6152CBE77340EACAE4B949258A9199E60FCC6393153314322B93C7386A7579FDA9F5A756DDCD17A7CF5BCF5A53B9E87273D59AC61A2F41C780D837C6E752B38A45D94E84F995499780C96147B2CF771BE21D24A364E117DDCDC6F6D2ABCBCDC6AAB5344D502ECD8DE3F5AF369C0DC2B64361A065D24AE01DEEAA8D1FC0A0EE3BA13AF01B6D53E41F6739D1C507E37970B02438982482835D173483306D153B68F2AA91DB253A04C5F8BFB181FE0A8E79245B4B7B0586F3618F1325F1C14CEE3E73B9337888ADCA32D8AAE388AE4788235EB11D91A3DB8EE8AA41E6AF5197493C250F2E10235C2BAFC96BF5FA9ACCC4E661E2D2E40085E32C1B7B049F3BFACA9C357BD01AB00A85D448FF083DB240C59A598BC51610744A45C70026ECB31E3B278921123C38725E5F214B251A3B6B5123CA08D8C1487681CAC69A8DAC1564890B04DB95E08E074CD80DD557A17F1F868C9EE844AC26EFE1B83B88E9A9B80EE3889E0AF0AD5247D8EC980AD3D709718F2AA313E2BD1DBD6301CCF1E256937999F93D55A6AE91BE2E39B11F4939DC95E570579613A026CB29B8D1CDE1DE0CBBC555BBBF4D3A955CD2A9E4E0FAE915FC201EE0520E3FD09EF88F3D011122FA7B6772121799E9BE86B8CBF21C679F93B83847ED96ACDDA61D20BD99E87C41C6DFB8D2205F214725FE8EEDE936948ACB71F16D66015CAD0F0D6F4F302FF35A3CA8C755D7DC4FE7BEB63E6BF0019B835EB2FA0FAE1FE20349A8413431D2D6EEB913F3E34F5E3843A721C519469484DBFF5D383D955D3A419F6BCF6015D35B9F33EBA0621179B18ABFA3325B9F5675C903BF5DC09FD9244AD883040A396A049D6BC0A97CFDCE5573AE2AFCBD3A0C836115992ACA29A88743E91E9878288DBAD3C8C0C30903751B28456653A83B85723EF42D0319D7B6DEAE8A6AD032527E3407771F554528B28D94CFBEC3BC0CBCBE07BE68F4CC18527446C220344C81684F15EAA07EBD5EB0FFA13A1E9317DC170AB1372803291CD948828DB6D70893450A906E8E39F3486630961C02340E08DDAF0FFB0F3278C8AC239AA15B9BAC37DA9348F44464B6F516CB21C19FD4E319BFC8B658E60BDAED37627A42E5995758D1EDE16FFD5A92058615643773D2A3890CC43D0D1FE2EDA8C743DF10A1C0A4050973A96C7DCEBD005C0EA1019BCBF4D69FAA0F8108230228901F41C3F89AED43A681CC143293C84C20338E725DA8874579068D8EA1B15134D68BC68B484905D19C726DEB5FB85FC1D7AA1BC25F49C10A8ACF99C6D7AA07A67D78DA579B21CF61D9279405E569E5FB0AAB54B590A50CCE6467467F524445FCB7E235F86F29FE90F564F1B9227D0866F559523FBE8B35AF6F4C4C6C82E636194267A09FC23C0814F22248AAF1DA8C4F492A782BD663EF53251B3D5C440CD944834DCCE25091A62154597B1B20F62EE0AA174EE39DA0FE5CAE930234E0DA46C7E478A64D71A7DDEDA0D831E45E60B9D6278C57EF4924F7453CCC1F68FAB78C379A4F247370D7FA0CBC0DEAD3AE34149E7FA3E93FD3A2063997D404FA7D1ABD478B7E231A8E63D27CC0B7C3997E49146F3FB343DD17E04509A0F35E802E8A00DDCB330C58DFED70FB8E16DC100179C8CCA3100125EA553B0206400515A2BE849DAD0F7BDA581F0A432E5C85E1FE30D21DF70AB5A74248C499B20FC614FECE38854632684842520A804B98B3240DF4E76732921A9F516789A1116F523574A0E4C04218A4933B404AEA7C3CEC3C8FBD2C14B04D2CC710C14D73681881E44343B6CA44E35008C83053823F974C648212FBC1FBAC144C77C5B32A1251B8F58980FCB9543C1370B39B6FB36E35198B67355A6C7D5694FD1E8EE1251E3DD1FA395C18CEE397D19BE857B2DFCB322E37DF7A1D2DC08561A580AFB58C7DAD7591791ED4EBA6BE69AB1703252AD89362281F43E1300CCD3032E52199CE89285A85FBD1288AFC8FED6A8B6DDBBCC23F7FDE74A1289112294AD485BA51B46853B22C4B9673B1DCB88E13A7B9164993D65B03ACC3D63117C16D371445B7A766EBD6014EE3A4415BEC65D8739C3A8DD1A2280664038AA27B588376EB2E58B0622F838115C5D00143BC9D9FF42D696090B0EC43F1FFCEF9CE77BE3346D29AA2F2FB52A1F8BED02C7308CD92A410650051B17D3921B2A2F7847B82B4F528484581F633D1899B26B0AEB59E026A244ED8A7AA091E8FFC801B6EA60D09732F0463F4DAFB815839972B26822C45D1FFE1A4A291294BDCDA8D98C40A0991EA3272887E42D144960E4423771DFC493CCC92994894E22442F853FA26B2D1933ED2182055A1930F983172AF434C2B3815C4C18AB442E1B7523351988578890560B0E2AD36A9FADC47A09850DCEB951409B9E1420C4B82AEBB2C41067B9DDDDC985705F01AA48F3A5461DD75143CC3E10926F9157FCA05C4C0DD4F149D309E7A65ED87B138138C04311396049EFC6DED59EA5701D807A6E3BAC4670A4551555331FC74A122C3674E542543D492E9D8DDCB7C4C47184DE1DFE01EABA321348E7EE679365E195FA18E2FA35209B556A893BD6CB4B26818BAB2603854C3E939D87142FAA2D5EFBC1A7A869EF7C63418FBFA5C7F55EAD6C19DDDB9E5EDB45D62E72B4665D185871D65C1454ECCF997430B343C6FE98BAED50F755E75BDEFF0E63678F73A088E0DDF62FB42439CD7C6F45E7760BEA3DFB0642437BC67F189ED6A77704FCF15D295B9F1C1D976DE9A75F73C1AC98F98959D43B9404416777C6BD7D45C37FDD2516B872937070727CAF8EF82108E342A03EAE044CD7978482DE9B54C4456A452269EC869D9F623F51F09AAA156ABE52AE4CA855CBDC9C591893AE8092F57A17CE35DEA048AA32AF5724F42F17C481CBC56ECA7CE88F3234BEC33C4BAFA6B0F55BF7D67232924AA3878CDF5E3D8912517228923F5F7976DDB4B952C2DF77A179FEFBC9708D83554CF6EE2370BBDB99D99A633A4E9C5982AB25C2C9D48A4636CF3E448EFD458FAE7917CB35C99AE5B7B074ACD7C8CFE6ABA7FD80EA9256DA71061B8204767589EC5186E6B1F0C55EA879F9EAA4C8D1A03EDF79CA17C6B0F709CB0446353A8818E7AB835545901C4D19072255B7C2DDAA7AF0E5A6FF080F7EEED5572010B6EDF2258D5AC72C5CD468BAFB9D1FE207DD51DE4AD375CDE837BCB26978F96D9AA28A76CAE6E9C5F560F31D630C3174FED78E9657BF63BBB13B66526C31C4DE4890F591385BD0766F7DB936698E7194CB7227224A4152EBF72687EB6CC85252924CA623821879842F2C9D34F3E9E2D05250DEA3903A89EE724D0B15174CAC3154C8DBE4B3D06ABCC10F5935E4CCA9F490569EB9ADA6FBE2E6CE3BAE7496F6F569304A9D63557ED0BCDD75D613BA1BB131B4CDE66443D9C6A72CB7E9AA6CF61DF96E2E75305498D72F5D33B1F7ABC9B3626BF39317CD4E2A35E45B91F5B7BAD326C9742AE6996F739F873BF8293F5E1FAA1EFEE9C9E3F649B26E5B001868699C8AE1D731CA3B5A7549E1E2DD8A32066682F603E0BFD5E410E7A81605E72602BBAB0AC4B926EAE50277A49A4C72F8962D05930188A61B4818B463FB8A8795C267DDE273728306C605E9F931CE4C5F825179E611C687386D26978CE18B8E81A7D2DB8E86A3EBB37FA5BDEEAEF8207D94B87720F09B0526D7B49E1F1D9747C6D411E7868D89C681642A18058B4873BC6E26275FFF7A6A66111BAC03C3C556A95E39841E95475574D0D4785783A931285207B7171BA7FB0664D7FA32D4DCF26AD568E687A197F48BDC365501B1D27F897651989EA0AF5584FAA99C5C0E5C6D9E215F54AED5C665E3C47A6D3EA2A59393F1AF9A209BA46C0261A81CB6EE36CAD78C5ADA970F9916444D9F684BDD9BD7E4D150F1A572A6DED881ECAF6E63AC950EF6086A3393B49B6BD67053112FE3E27EA09058A7D300CFA743039BCBF991C56822C663F16E5108E087A2D3BA665B2DADA04949E21F5A77EAD65335ABB7374341D0806220944A31AF5157E0A7A76273A801E47FF2058DF4747A8296421993A0A23ED61EAD8CD611B7E2AFAAE15EAD8751E3D42C44C478F52C77B5685B9D43D671DB9D4530E2B589959883A3CDD065B2308466FA1DD374E5027167A0665807F5C0E84678CE7D0843DB7DAF7392227BBAB9FCCAD76273C25B873FBCF772468863AF02576E72F77481687BBCC25175EA01CB9E4222506AF10849905D77BCBDFBCB7B47B0B2E790F50CAA022F4B61769DE22E1336A0E5E65C77E3B67772736585552555F43CC2A47242499CCD11BA20299EF10ABEADFFD46848B6F999B6364378EB7CCAAFFA827AE4F4582B3B258D87D7C343F268722A6F1AA73A09529ED3B373BF3EDC9DC60356394D26AAAB8FBC448A6AEBC1D0EBF37DED107F4C8782B63EB1167B47EA1A4CD4ED9E3A528F3A7941AB73567A6998E08A1644CD630871573AC68ED69655573D4B02673917ABAB423A976EDFACC88CEB1DA2F1A1D295B4D345AB16C79EDE95C0E337A552D1951CD205C76F187F8459848EBBABC64C9A4821914063A475146B292E292DD2F9E49CEB3F39ED5DA368B4815A210618B4BEE668CE7B4EE9B42307D4C326CB6287CDF3452F08B6026127118389DB1D2CC00EB4F1F6E630A39A71AE3479A2AFE7C93AD63337BEB436B8B1B9FB7CF9F9A55DE7DAC0B4A751AFA14B3FF450A2AA09ECF5E95FA0CC021801842792ABD9C8A9DF770FDD5A719205A4AE557A8F4DB6EAAE7FD0BE0A47F477811FFDAD93BBEBF804E6CAAFF66A36965E3C44A3ACA468A0DA750741A85AD33638D0B7018C3ED662D971BA8E57308FFEF0FD4579400672C829ACEFA0EB1447D767D4040648C24D1209CC5CC9E4F5C659F23FAE99DF3967F507370853863D7EC65CFB389AB2EEBF17AFDB8EBA967BCE37963E1C107A7903C34D3561D15CE170E70C1A82255CA7C548F133042B15E2F949C7A017FD038D8CE713C2FC4639924CDD2435DDCFA1A1A9F4DA7814D2D749260990CA361EAA7802D0C095701D80D7089216AA517416A31CC5AE73367A40D5EADD30A88F545D3EBEFF0F68875FF7E8FB5A11F40A78EBF2B8260F27C5C55F1695ECEA86A460AB2BFBF9F53CB4C48865AE9321F147FF96138B08D5C436B1F3F8058C53F72218E26880309F9DADA3FE5A88F187D09881554F6AB07DC5A0EC5CE78A8A05680E42DEF132078208BBEBCFF5C5BE7F83FE1D51ADBD475C7EFB9EF977DEFF5BDBE2FDBF175ECC4AF24266F0C041BD01252203C543EB45AA19B60D2B4943864B49D10A85F3AC1189A406D29ADA6896D1FF665024A80A81D13D28234A90D1AD3266D528B8AF68D29D33A55635337B3FFB9D7769C005A6C5FFBDC7BCEC9F9FD9FBFDFCAFF6E642AF510FAEE3871C4CFD4913434DB0FFA36693850E2C438745E430C5FDD32EB5D2DCF6E1AC90FD4F273569B752B7E1B7A507A50863736B0B505F276CB6CD9BB3ABD7A41D3D895F66EF4D42C5E3BC611158DE29C36AD8627A88700CC8FA6A1616F5B8E555C3D1A53B881E1CEADAD3477D2696BE040FFE47E3B36582AD91BA6FA8D67A7FADA3169CBF0B775B06FB414EF76A4CCD8BEF58D483C0ED6EA21A67C6B65B495BA1626E2E12BD9D98CE5D59A068AF88D065735DF34E16CF8CA74DB0C3B98F27FACB1821E57B4E3D8A13A401D599F9EC8358DE0A453CEC0CB831BF7F5AFAA669318D25B4F40F2C14025FB1A70AE4B804607D6B519E3F91561A0D3C0323B80650AA2F38E329B7E37500BAD42569515E71D20CD4CFADDA63C5875FE4600720D45D4208FE4A5FCEEA3DB77D7263BB33B8F4D3D7764B2EBACD235D65718CB19F87B6A3FF5685B6D5F6F76E72B13DB66F6F6E477BC32999B18EE880F4DF414C6871207F069A7D1BFC81FC369B1CAF9A6CF8A93EB446CFEA8AF720C228A258EB8AE94A499584D3DD6123AD0899B6E08A8B119A89C95A9ED5A07CF6E47945D530F5ABE7852EB540E8CB93D859CD50C40266CAA9DEEE03736AD689DEDBDB9F15C66106B9D89D9DD45414F18F5FF3202560CC0A396717A828BFAD795767FC7D73AC5A15BBD7D81D681D843F7007F2F318ED15FEB7409051363D91517B3B39D4AB4A316C551E727252A7DB118F1C186B2E2E2F4CA73BB91836D087D3EFB44C0F98946A37B206F185E8202AE009F30DB73CB2E74A7F570CAE4808FFF5EB3C31CC332929D4BD47FB13AE22692398BA779366C018AADE41DB40C282AC4A9A0B295D1F3D7BD1EAF477616D0FE6A82900BE73EEFFF7B3FD93F72DE29335DB3E2B9DBDAEF345233CF33BE8E7B691903C0DFD8A53E7BF3E3B2ABBF706EDAEB5F072BBB46CE4FFB6B3511489AA66A502F34C63CDFF0F1A24FD2606571F1A5D54D2D885DDCD5865A437A95EE038B2C672A2F0C7B1BFB92324B311C2D2672235DBD9B0B9B272B79AFBC77A06330EB4A0C3C615833534A0E00F97EAE52A05E2B6EEDB5254591AD6848971935A27466E329CBCA5587B39B8AA620874478A2C94C480DE5DD8EB46D766DC6F5390DF6BACC5C22060255708D4827B3D8EBAAAE48C999EC0547BAA0CF142F7241962E2D4377585AFCE2CE1F7D951B4DCEE8D90BD38E5ED5A50BD3FA0C57BCD810B995A2AB2E152B6D2A772DFF5FA511B0DCC5F7D06556343B52CACBCF4F499224EF621BEAEF0C8CA4335EC1ED6669962129D5B4A1EDD35F3F80BA31FF3FC1F00C4DC3E584AF0EFED63FA0D052C48FE93BE449C60096B2C7AFA7423A90B9095C4FB5B440E56B56CDBBD212B9413F874AE487B6D298207B57DAE46DABA1AF56B72BDDDC4733DA1A93279D74C40A31EB0E0F6EDCDB6FB2C0180D476547CBA9EDF966B16DC9D901BF78A29D6C90B26CFDB71393A55E34DD1C03A624F909E81B83C806CCEB83480AA8DFA9793DC5A7A0B7BE5895382F950AB9B5D01C510B9A04724AAE8D75AB8A2F80EC66EB79D022301C382AD00F2A4C052CA5BBBB717E4BB746F5CD149934620A4FD11F52A2D1198FA7A322F511C3086ADC30E311963A4F52A7485E8D3106300D5909D5C3BCCC51142FF1E81FB226F3E0338CE6A0AEA34B1CCF5280A387FC84FA0DE0D846BC8E71DCF452FD66A964F40298AA9432221B0D9E1B1B332A0BE885AAC61923B5D29841C572B5D85C13995F4B019C8DF3D42945CAE0195B0D06E572509E94B5EBECE6C2A780CE3E1B7FDB4FEA24842447D13FA7783DE5C65251813C82C8C39460E09121523FA5294E730D2BAE71E4F748F255C4A94E34EA8459EA0D923C8A782D3093A484DBCC745896EBEFAF182DAC4A2DA3C932BA140400CFD65F921A239CBD39E00BFBC18625622EE8AF9D689EB0890CF42BA1CF86176149F202FAE1BC2995247101DD0475E4A5A57C2D2D31899AD6A211BE19EFB405494BB5FA9CB67DBEDD5880CD67365A5896C36603938DEAD9EE86D9AC06A9E538720F8354CFB53B749E7CFD042500B7753D95FDE5CF58A478AE93D078EABB7314AFC4A26E5221D99F907F15648E26219D3FFE18CA1C368880E43A1F829B2CC7DCFA35CDD314C58584AF7036A047E4B7C00229E2C52693BD718DE7456B019DBE9E323DC13416D099AA2C9AF15A54506AC251EAD506756A468EEB230EAABCD836AB419F30D020C74747212C82E66CE941509826CBA129AA58B0120AA2777DCAA170876B0373A7DF22BF4FB25AC2B63B14C4904A48A2F9907895341543A6494E96EAC74874961321A5245D25FCCE750F7DC98421AFF7044854F4A71B1D06BC88CE05F4E7AA24A4B48BCE8C927E8F390A8DEA36BCA1542D058EF2E95FCAD12E4EC30C26FDDE34CC81327C1BDECD0E9482FA8B6BEFA8EE576270530AF7611CFF7EE9FD12828DABFF4081D014B9E5BF48619FE31D54355179F86F0E871FAF1821CE8CDAA2A2A812DAE9C6C2308E588E1E37EAF34CC8C02C8AA06EA2BD4C89900899C8353C42CECEB302256F272AF7970271312F505518DB15F7FE52B39636A51DDADBB37143117FEA97E9A50D857C193EF5AB786FDA433B983757EFFD9ABFF7A1357B1F7AC6DE3B7ACAEB0BC5F2FA62FD3AD3355ACC8FAE87BD170912898FFF893E650E00D9CB135D786F781EDBA58E43AC7C761776BDC17455FD314485FBD9DD764A4A35423EAAAF21A9E816E8B868341EE134C447D3F1583ACA870527974CE66D41B0F3C964CE11D0315EE6691A2ED4877244665828975F9553C59824C58AA954AF23494E2FB6ECF2E36574993EE89F707D90ED267988F0882859BE21A90538EFB70938ACBAD8E4D237F0CD2ADCB5F191D5C5553274E859877E9B834C34632A8B3456CFC4639D3A2708662611EFB604C1EA8E2732A6808639112A155CC8C7B22A328CA4C8FFF112595B92EC6C22917344D1C9414C17EAF7D11CF1391123127EAF92AC38A1FE6109CB4BA90ABFA15ABBD8495D70088E0B6AEFA8DE3ACA1C703AED3413D21D5DB34444BF29D919D7C958D28F92437DBDCE5D9C3E1427F3487F23E6612DEA819D3E7AFC089DA5DE06D5912506FCAE4F180BE4F19B6247DAD9C92810854B150895C10703D844F85E55F143116E3F4569686BED73167BD0CB610FE63CECC1B563CAF37AB0F7FE4779B5C736759DF173EECBEF5CFBFAFA9DF811278E1DC776EC244E42E2D8799387098F84BC4820214B62644213C84064A5659075A2531F541A13DAA35A8740A3130D09C32B132D5B37B59BD8C6446927B593D83F4C5DD31526216D1DF6BE7BED00A9CA1F4BE47BCF39F6FDBEDFFDBEDFF9BEDF29B317FA84BBEF81DB915D80749A954AB30F22730A50CE406414C893C5C8A4326FFF5C89E3C0E66E2400F45E13C2248BC154002746E9312ACF041AEAFDC2676F7BC0DF0A1FA186E0F41D524E5F058E1844AB6A1A0502604618180301B061C80519E7AAF3394AC517E84C0E8E6288114AA5B5EA4C768EA2EFAA582925516955CC828A95418C799560DF96A92496A9F7FF0FFB6F930AB06FB46B28127F0EA52E1F7C6948FAA772959464043DDA238C68603ED8C727D2AF10C5F469B06F12F9A2A67B505470208E8CD1C0FAA3EC9AC22A96705683DECA49E0AE37C03DBDBCB6A9987725D99144C0DF8A57083F1141D00505FB2B48A258A55000A4BEC0C9154AB19AA4E045BCB9BAE1C846DB014E083FA7498F72F0877F2C55C968FCEF12ABCDE5B2321A73260327DD16B07B9D909073A02E5F4218AAC92A451047C0934B7C13F0F40B9C8F1E395BA214299CBFF4247F14A1D5FE37AAE5382D794DC6C2D92DEC723A5DC54E99C692C964EE138360FD1382210FA024BCD762FA2CFE17FD3C7266191FD39150A0E3A49A152E4A15EE267536C522841278EFF57A85800AF35C401942C77306686562972BF193629BC82610FF73E7C8CE611AE7159838B3564986B7D6E4DB6AB75660907E7A43BE9AA0C7DF4B0FDEFA203DF47BA5464183ACA1276F7CF8F1ECEC477FF9F314C530242317BBDB61407807103A5073B6727399CF97012397823B9BBD5F12907248D808F00D32CBB388BDA11C6461E11107729A235C1DE6AA2A8987BA43CFE13BF9355BC2A4526BE6CC052A4CEF181D1DA50875BE4197AF911253F38469F6E30F6F4CD25286A0151AE5EFF0D90F6EE1B3EFC9D47240CB50D7D33D80F77886C5BFA55620A2E502DEB7A01BBF8A1864862BF43882BC080105EEE3C0DD5BB76F09C57609E6D92DB0164FFE2BE379ADBBB3BB8352598D9C49AB20CB9A7C7AA3BFA99400C1A8030D41512FBC963E7761297DFE8C5C23A705BDD3FBFA85E51D232B6F9CEF85D32049CBF3846802BA374574A16C346DF8E98B3A165D2138C4211A26AC592EC20370EA2C3E58C8E17B72F0AC247ED350D65446283848B75585A9CE8E8DDD14C9161840F34A89D226BF11DFDEB17CE1750003E1038C3FC1DB972EE081D76479729AA400EEF9375660472C662860E4DF81A147D008CC0FC3FC8E387F068D40957E9E9C244ED3F38FF75B8BAB5DDD0EFDF6BA5898694B4C9C0BFDF67A685DBFCDED7BC99756F43AE218A336709C91650C72DE61303A78194E3FB76EADDC457E6BAD36E03FAE8DD2C1F56B6AE02C8550E633DA4677A25E348DBE819E4649716FC93BF6575A0F998624EC4C0A939736C53D1EB63685994B2DF1894F59100CABD15590937031882710C5C3DF6F121EB89C149F68111E89C9922D7176E2D3242BE80A6FD42B3CE785E7E075B50267B2D4118B9C90342B696820AB1E2EE5D6E0B4A9D7C3387B72116F14E6219BF0DB4A3F44288F20615A11120DC20F70D21A9BEA70D716AB4B474E4E0F1CEDF3BA7A8F8D146EEE1F2EE3ED46A5446D33E96DBC4CEB085A7DCD019B5CCE2920D74ABB992F8FF5D5968E24F637476777755715E012D6E6B375ECAEB7E8FC6DC1AA8E80FE80B365B2D9B3A93D66A99CDA35581C6AF670E9DBB8AF7AF7487F5978A0BBD5D930DB5FE16ADB1DA91BDF311CF20C0EF5BB2DADF1CD9E22B94A06B29855996A9253A3EEA272AB92901A4D262B2B97E639EBFD851B3C06BDA7A1679C242C359136AFA735162B2AA8F2182DBEFA07EECAED51A7A6C063F08D8D8FF9EDD1688C5CCCE58FCA50475117DA8966D11C4A89F9639BA6F7D2943C12D962CAF753E1EEAB443592239ED0A3EDA814AFC6E476F976BB7DBB9C0CEF4A111B623C2A2E8E344DFBF3F79AE82DD2F6E1FBECDCC4D7525875B13D114CE1AAA54842CC78F4A6906FC81F6CB9770CA0C9C5D393FAB6C6505BA1C93141BFCE123B7C3F09B6DA05632BC9F64444307731194964A9107DC79B35F8042EE8F58670F8712E30B0B44605EA111588AFA44238C784C386EAC1A67CAF950D0C7F73DBD6F9CE226BDB5C5FD3F776588B6838BB186CBC34ECDB36A68532C96082521619B49EC8608B6BE79EB99AB157C643D829E8DA96FE20C716D6967A1A4A7573B6BAC1BAEEE315953BFB7A8A7D757655FA7DECF56DEDE976153645AA4D4DC9786951E34095B7A73BEEE9586C2B49543B4100D084344F69DDF69D6824886546A3D6A496C914326B75C05E59CC07FB0E1047554E5F4DA1A3A6DCA737070A79BD3BFCE072D9C6B035E476C7BBE26E4B281824DECDE55C92623290EDE3E8BBE814BA2BE65C39F9EC423FD59BE8ED3AC8EE4F1181CB6327697903D5F5E215C28B12A0AEBDCB8B4363B025BF8829D9B1C422CB2E26C6A82ECB15FC09F2A14A2C8D19F9197AF22439B3D0207F96EA2F81FFA113F7664E6D39F84B2C4343A8112BA14C0456032211843399903C43ADFAAFAB0F09018CD0D4E638210C356BC458E22B53E0A220C9CFC4BEEC64E6C4BD24B8193A98C2B2E5E4506F630A2B9792BD826808881C79E4CBACFE8377ADBC672B4525E17241CE291D2F924660854022AA2244E9B91C8B84B2902395A02E4BC4A645891DCBE57A287AD79126C7241DAF072798E6EBC716A26593FBE6EBAA87A345C33FFBCF8F0EFFE6F46C875367D1AB78A39167145AA7C314DCF5D2AFEEBDFC162EBF39EBEE9C8C0CBC1AB39458B45238631298A26405DEAA822D370EECF9C7C1E685446FBDA3B46DB422D6E91E7E796AE899ADC50F9EF2F4ECEFDCFC83F6C8A1FD7BFC450D3E33FE9BB6B43958B5D1C76FA8A81869F77E719A72B4EF1BE8748626C6065D55DF3EF3EB7D6730BE305C31F0F5E75E68ED9A6C74CA241A4E9327E56B07E75BC7AE9C7B717AC3BE3FA56F5E7DE6A32BDF6F28AF61189A521A38AD5923932AA474BCB963A3DE13E919AF694CF477D8FA7ED8D8303F5CE38B4F3CC8138A8DB7CC3332315D650CD5B510C7ECB5A5665B45CC1E5A682BA86DD888DD201080872AE610ED474FA163C044B1F22CCF1E3316A7F0DE58D0AF34FA6AD082B1CFD887DA761FB86D73DB82473ED3FC8FEF6A8F6DEA3AE3F7F8BEFCBEF7DAB17DEDC48F6BC7B163831DEC248E93103B813C1D8A9334043BE449D8A0DC9657088F76A09657593B066DD7966ED3540D695BB5212504E289A9AC1A129A0663130CB47F8A90D026310598D4A81B1067E75CDB49C61EBAF6BDDF3DF77C9FAFBFDFEF7CDFEFA41F2593095A7D24B00BAE3E3B3C06573F128F76251E0F62FFC6AB10170DCEB2B3212ECC42767D71150D7FC1FEF10E178D425A49A5C66F1FBF2FC2A8DCA14762320DBF2F04166164187A30F1581C445C5AC62414DDEF87C1F3645A564B881C79603D8954574B2D18A71081887C71A10CF93EB344A8528944141A200B6494381701D47EAEAC21BD6F5D794BB59BF626DA9A057F53B894576A1D353DBB3B1D75D5210B479494E9CC5A5296622BD69437859C466570F795D3139977C79A7D463A7CE8F6A7ED137DD54A0A0A6440D0EAE8C85B2F5DCECEFFB84D65AF491DFEC5BDEF9C7BFC83CEF95F9525C3BEB52197515115E34335B1B267CF71B0F6D4F17DE9B0BE34EAF6464B594EA8A86FF3F9774CEC4A45184785B051AB25E0EE365BD9D753DE32F04D31D4F7C37DAD95A9F123270FEFF4ECC81CEFE0F41CCD9838AD8E512B8B8AB41BCFFDE554E589B33FFAF8C496DAF5A76FFE3ABEB6BCB17B4397BD23C9B9A21EBC1B31223B4F9D215DD820368A6DC66E4A9A67B4B5657018CB8037E29A9691B6F69787877BEA9DCE0A32036E4ED557B45F06BFC77A300EDC88DB2B466F0DB5B6F26C0AA4C63C73562BFF1236D738C67FED1F53CE45368F62901EACF4B9AA43E4B83A00518C4276FCFDCBFBECEDFB708CD345CD41D84FA27C10924497A749EBE82D11C5C5524089A7C6AC9E3911C56E6CC4E6C4C6313FFFB5E81F8B28E744F4133C24099BFFC080F03C3428FD8C441408699E05A8DC10FF8D054B7547A294475854A1F9DDA28976E1022003D59D15C6F8B13F9C39F1DB330311A329DC73F0DCF6E6BDA928C2196A6F8A6D18F9D69ACFB3FF3CD76E6DD977F1D94FCE01303958DE7FE61597D7282762D9E3319CE2048B4DD0C848E0C1357CB9A3D46F51E36FC84E03EFB3DF0075C3E8EB0D476E7D2FB966FCECCF6736749CDC37D2EC66F52CADE5398D4EAB5115E9B5A3E79F9CFAC69FAF5F3CB93E753E3BFFD38DE7DFE923688DF2D39F514A8AA0D47A46360B1B17AC5C5AC550F620DE01352EB6B01FAEF9B350E37E1FDB0E356EE9C253FC2E5984A5A0E23D20F5216BB27BC3EAF67BE92A2A5D49F7DFB3F9385B1A1EA56BBA4B7B4DBD50FBC6A455C885E1CA0E85F2971842CA8E3CD3EDF744C9D5D67F4F7CC1975FE6EC977CE13956D0CD2E1CE9E39C95930511BDB068B99CB94A1FCE59D5D0C80162C085BC45A30024BC85FF48A1A18F78C7193D25D7D0C77C80825B2B938DA5802FFBD027239912138FEECAA5196AF9F1F2038C5ECFBC5D0E68CE66E24B18C2078C1E20676DBCC9AA2581770FA39F9FF402830F9FE07886CE4EDB9CD2F5335A45E3383C810DCB6D2B7A2A079D3687CB061AE11041D02A2AFBF972DB3E9C9D069DB0AF635688C1574419B61E2EA4BB08834B96AE6423112FCE80DFCDB8CB02C108E1AEC980EB33710D497412092D1AE7CDDD3D4D04BF06DAF19589B03BCE27148A041F778729CCD7B2A533D2F82019249201B2EB81A54C6349C2C351B7C5B155BFF57F410882790D20A900A9FF2F3F4908A3C0C9C607A214D9D2F5407C21F4FF41982A000C0D3A8F6A4458B416512D1891CA5CF9A6854221A70BF07EA5633E701E43887D68071487A0D551C096FD2BBC63ADB93BFB7146A7633E727E1B4DFBC426044D2510703B305B65F6A0C9CA12A0E428A39BFFC4092C82EC4B6749F64989E0149E16307CBA640982B304B0E8E9C35CC57DF850C348576128FB04B010C1A18559A28A08637ACC83B52104AF40B1DE8261980D9E95981918A7984DAE0C304E924392148782EBB6B4E365CC70745A643691E8F194089F4BD27AF966B2B0975CD434B9064654D5BF7EF9D05B331311747D333311992AEDDCDFB36ECF7A6F69E7BE9E75E3EBBD32FDABD73E4E77BF7F6DAF88AEEF5D3BDCF7D1CE78FDF6F7FAFA3EDC05AFEF6300732D3C9551841713B07AF4E69326554666B980716A5506345D28E927072161E66F80E0EDFB68D37B411D2F414FA645E91104DC6FB901DFD65D785B8EA6A89C2A8BB8F32B5446290D0E93D161503DD59BB4244EA914C047A8395E67B6EB68B3422D651A16F70F5484C6C6736628E5AFC860BBC4619631F88EA350AE9C84F96DC0C6D13BFE1233CAB65D0ABBE1814533B2A3D32A87230A574B34AEA8E18C3815D8C4429D5C3B490DC06487625030CC72D2AEE706A2268B123F0D3D02928B4A2CF850C8694A845E1002C9CDCF497B1BE9BF097928D0BF2A6C54F2F0508B3B1E5A80167E9254328AF9A8C6A895130A46030CADB08BF3AB12E186B18E0A15A592133829E7EAFA7637F51EED0F5AD6EED9F837D92A39A324DB74C53AA8016DBCC161D62B1ED60F279B054F3C6071781CB08219B54656C3963A794FE78E96CAD16D132D5714FA62949FAE6C163F00F393C6BE9BCB4F58D61ED7247BBDC9266F32E96DC2B5D68C6CDB0CA6D5AFD6AFE6612DE1E3CA446F60C1E924139BF80C304F92238897A82F4759982C982604B84E2A08B02D4B39BB24792724778DD8EB0C2C885200124580D41D91A88BDA2E4A1CF45CA205552071B8D08317991D5EEACAFF994F830DC70FD4EDFE6C47F39EBE1A354DE20414E3AACAEE1DCD4D9BD73A57F41CE83CA86614042967543B9BB6B57B2C55C9AABAD18E10ECC2342123E486BADE5763E9B7D32B1D0DFDB5B1D77A8287BADED95A6FB4DB551A83CDA837339450E670366C08576F8C3969D66280DB3ACA154B5597B757DB5DE52E922D3632464E5BE47699022FEF6D59BDADBB462D2343DDAFC155EF5C50122158B77D58001B46799F0A086C46464D7B08025B99919133463F2BF4078A3D1960BAC80E28878901547C51B1D545253A06C3F7436855CDE426B268E625313F151553BF34574A24AC9B055D5255B5C8C6BC2C3170B03802171088905E775D5EE4E079414FCF67543A0D45C8D50AF00FB248586977ADB269AF33C6EC56597625F853ABD37D8756CB4952AEA6EFA01659642F2961F14685460E09CA289EBFEBC2DDCFB312C31630FC15C20157A098635804DCBDE80D7943EAE28CAC750A533B3200BB505B4B566740DDD48AB4718954B38855129B1027A4AA578B665F14E1F41568FEB4B8224D1AFFC579B9C636759E71FC9CE39CE3FBE59CE3C497C4D7388E13E7E290D8491C3B3EF10512C70E985C9698DC4C7002894948612AB42C4B45A137A65574ED3489157A5136D0809604B0E82AF1A16BF7A195A60A4D131252E9BE549DD0A649FBB235F19E737C4C2E8409664B7E5FBF7EDEA3E3F7FC9FE7FF7B368AC8C9AA685D3E458FC987138BDD4D82495838B110E51632EF1382E9C84FAE4CB54E26DC6A094E4885D28A603A1A98D95D6BED3E3E60AAAFB4D1655A9301338A15525C4DAFF92C9DB6B9F70F35DE98F960AE4559AC55575A543A95585BA631870F750546DA8D02BC485F81A9CC66115D46DB1C6B6F17093CA95759E75EC83D14DCC3CDC80E642732C555CF2A7F160BAFC8F47A5963168BDC426475B99616DC067FF73A3D14CCA2C50507583F142848AC04A07641424130CD46AF64E8219C8D5FB704FE48D0F52361FB9CA28226D82311F220C49F9090770AC13DF7EC6FE6F79E1C09D849BA7EF789A5D9CA58473D2942099958626FED691C3D33E014E8833D83AEE973FB2A3FD2B6248315B15D01BD8519633AC6DB8DE8C5FE5F1FEF724433AF7F38DA7BF9DD37A67C6205451AF4945E2552A814F1C5DF0E2B8D5A656BFA8D71FF58B05CAE31518B57A76B1BF6A459EDB87312C10268C78484F3DA5161079709424366B1E832A2C1C92CEA5BD627A5E3089B1B68FD7DF650589D5CC7F5EC6F2B19EE47D66A76F00720441F61793E13F232102C087031B1D64028B4E5A516BB0A25D0EF56DF162BC5B89AC2FEAE289612823F5306BD5EF19FAFD8B22154D0F2A2A8842EA32A2B08AA0C4C06998027FA356E011F7723116486BDDFDF2375980E51230E4CC7488CCA72A31ADE12CF2758180C9EC1C28C4CD2D8863B577543A1D5470FB79573F87CBFDA0AE5932BA08C120275CED58C6E080FAD6E78B0AD5BECBEB94E50503C5706A10A364163CAF5AE1A8F876EE2BA5DAE4BF508BEF63FB7343D7161DE5F153FBCD337C2581AD2BF9ADCFFF3913A4BC768DBAE23DD8E7B47A7678E96B6FEC89F9E759647A62281F180E995D33F3D83C6FA4F256BAB12C777FB2707BAADA6C89E614F081ACEFAC4E176CF585FA7A93CDA3F86A5FA52FBFB2B43FE5663E3E2EA7B75DD4CBBC5EC0F76D5A4A6A7210386A12E64C1795C4810497319606860330051289036C8809B95CC9AD52AF4B00950C35A4DF1C7C28D55E1C167EBFAB7326B602842B62480FE6B86855A4EFFC2D456FD176D2D09CD798ADC9E943C82ACBAA1F7C5CB47AAF6841B6809344622B1A3BDD79D3A9BACC5F41DB101D7E15F242B9B324B474F5CDCEFB8660DA5988E515F99CEBB2F18FB19FA65EFEF2E9C9DF44954346D282D2E550AA107EB5E581A561A4AE4DEC9B38981F3CFEF4CBEFFD7638BD732F5AEDDE9A6B654A8A296AD0D9DA0A42FB632618463C208C7849A3C136AB665424D810935CFC484822F1A3257165EBA74A0CA75F8CAC2E2A574D5475AFFA144F4608741EBE34623466578263CFC39CB847F5C18FCE55CC03BFDD6203F42C65E8554B8505405BE96C8676C15A66794A49194C21BD1D254F9BE2AC84C7581B0EE031E7E96C77E4EE8B7688A8110828D59072AFDFDAFB6C254A3651B5C24B92A7E8190C885ABC342999420C09050C52676A4B494D64C11DF8B14623C4CEB5542306F9AD29362EC6F1C446A482D2925EE1420F287936252CFD6A2E7E09F9D87A7E245F6174872E6667D39BC1177163BB1222DA927E0F85B97AB8755EE0D0CC9A7F4068084C06A36F246064209F72674E4537A1B6EB46FC09CA22DD8781E874AB5EA56142B85028952866AA3C906556AC23FD1BD438E4BC5B8A424903C1A183A3D54A30B1F4B3EC49A807AB6226320B567A72D9E343B2C22B28CD65B4A6CE53A477726D89C9EE6711145A68016CFC1190C1668B1018B32F29E5E7B0F63EFE9B133020578FA34D0621BD9469678385AECEAADC999CD78D770C953D322B7BBCB93A7C55E734D2EC35D60B3D13F89163DFF272C9E6B3F7679A6637ED0AB141102855CECEE9D0B070F84ADCEDE13F117E1BC848454219EE750B129E1F6A6623B245CA3870B15DEBEA3A1E4ABFB0015936DA1B93DB52F0FBE39D55C6C342A156A43B1ADD4643759DBFB1B3D43EBA06865869AAB3A3D262B80A2B2B484D4503285CD56BA0E8AC2A6C40C5B0BCCE080DFF2A438BA4E8A1A9E14D17F01294A2CFB2A4BEBD8CC2F62F90FBCB0715B50E4E2546CE08D4C3E128CB1F1993951F0AD9A3A27A22D5A9D9912AF7E2985964B20140BD187456A73ADC9E23229CFA94AD62EA26B3EF40F5B38D1A8511BF55AB920066B82229142F4C36B8F38310D7EF00EC7892F6CE644B603E9BC011D08ADF57C82B6213588171010FA879AE4960644956FD636F0E20A6CE240F17AC6EA0573B895E1F63DDE776CECD7B66B3AACC413A1F19D9D8B1F677C993EB78AC03181482A9454EF3AD4193A92A8AB4C9C1CF00FDA3968F4B35D9A9A5A339477B9E696E65AAF1F7C6FCE4BE9B47239A927A95252A433EA4CC183D1F6B18049B6191A71CC9D7A1DB43003BEF029306313D2897C9E3FA368EE0EA35162F1F128EAFC71009D0CA0A100DA14406D013490C5428C5A5656267BC18D4EBBD16E37EA75A34E370AD52A74F308829AE1D0B3B96F9695681CC6EF6EC16510970C956573FF6624F045E6CDB95CB83D0FA0E1C700D43972D7E91C197930C2BE3816E56620B5112758B2CB9BCBC076DA5E40D2F0532029F13448FA2918EE7CE2E4B0BF4245D5ED7E7E69B622C6D428A069438552B1D4EE89378E9CE9AF12E83BE2030D87DE1CB25FD578004AA31180D2C06880196D37A01FF45F38B1194A959454AEA4151C96928AD8E2251E4B5F1BF78E056D2C96BE74F550AD2B9186439B809CBC064A356CA6D262A0D2A965A418A7582AD56E4BA55A8AA352EDFFA452FB2328BDC641691DAED4D8F4563B8911E8F7AB6FD13420E93F9F80A40E9B9D035201B217D4729BE3D1666417FA61FE4E3DB97FAC284934E6C9C244264763EE2CBFE22EAC3415569A0A2B8D305926D13837825ABAB2B93B3759D175A1AE420C3751AAF809BFF2CDB2148DBBB240BF3AB543C52E3A546C0C3F37C3AF8E2CA665F42C1BC371A37135F7C152720B17D392CDFD892936A0F1166E23BFC86E6CB98D85102477779995EDBA8CEF2CABF951C58FF2FCB8027B9020DC1C2361AF1174C14583859B0E166E3AC8DF74904D1E52C2C054E2F6E3B580E8914D88EE7CE0645F77B94FFE0B5775B841C52DE43FD97C409CFC0B401E2EA7ABE5403EF2AC20CFD5A16D49FEB66F7E69E6C0BBB35E47F76CC4370C243FC1917C8D8519F1ED9AEBAEFC8BA1A5D79D990396F7A533D5D6C8543830E6379D7E79F1141AEB3B95ACABDE7B3CCEB37C62D8137E7E10587E36D038DAD765E6587EAC3AECD2FD97F43A8D6DDB3CE3004E522475500749DDB4ACCB96659D566CC9B224DBA2E52BB11D5F49EDD8511B59B1653B651BE76A31F44C9AF44AB6784B5A142D826D3D52244337B43132ED00F66528B05E5806E4CB10602BB2E653917D4ADC14B1BD87949DC889B306280C9836F8BE00F9BECFFBFC7F1435DF9C70449F5F7A37DCDBD6E272B44A9A7F1C7AD33074EFB392E63B9083F76BBE7DC123B66FE805BCD2CF8307E4FE72D29735EFBBAC177BB75F9AA2169CBC680098B45EF6659D7B43DC93641979F1FBC87BD610197AE61CE83E536750800F152A7F7A68D3C4F11D412C763A279C1AF3D6EF797FFFD07359DECBFCCE9DC9A5DBB2299B153E716B7ADA5BD02FB6FFA6E47B83C16500402A6856D7F3C207594724357D6278E49DA7BAE0B3EAF55F7781EF23750393D1E67CBB8732D9C5549F86B379E55EE1C725E1C725E16B3FD11580F0DA8F89D9FB85AF05E11744E16BA19FCE3EBCF0AF24F69FDFFBC47B422C39777E4EBCFEB6B63B9FEC986A777BBBF329F18A590E7F35DFD776F4F31387BF3AD9C71FFDE2F4C133B9DAE49E37B370F5A5F6BC09DDCFBAF23D7A05F7222E2429EDB3992A62FA0B2E464D15D1B60BB6296206DADED2A762DB1377F113B54DBCB1204877C49EF7E9DD9CC56265888FAF3A03BDA232382D668751F11FDAA82664B84A718BD29B2B588B83915B00123299C887DC6115A6A9343366468DBF235742F4924A129E2F0B4E0FE0F54833B277CDE913171B3CF083248AD89105CAE94C5414D1E8C50613192ED089221AFB989C96A80EA770B508CBB40EC3C3E2785E29946690E21410FB7449EC30A91C0F77D81417D1FE006F82DA4DF0900A8D62E955AD5143122A56F38F8E1D0D0673A83DDCB03D1D50922AB90CC3154C63FF446CE4B9611FD77670E787E81596E96238564982198D76AB5973A963EF789FCB9D0A5A2ADC9C9CB11934064643DB2B8DC1DEC94474F2C06BA3EF7A614DC6A0DE3C92DB5F5A737BD37D6EE779D31ADC6F01DAAF8AE62E986E952AF0FF8B9DB7AE92FD96C8F5AB12D70B84E9D65A79AEB73AFAA3ADEE490A6F4FC41FDBB24907B1AFA6287FE6D196E458B3DDD931DBB55FFCB0C3551A85101D4DBBF5FE4C5DC38ECE909C04D5C382D27158D0FEA7877CF6585FA83997A946FD9B9F1A0ED1664EA7D657189C5660185797A9F57786ADA4D66A60AD5A828B74FA9C719F85737384D6AAD719698DCE6E3354B5E75BEB1F690FA964843F3306A7BA66E5B6EC185E0D408D2039C9EA1197AE88AE2C780902A92BA2E7789DC95B1159C4432E9A56B9A654E24981A56113A2D7D7619DA741EA9145A16CA8656D2C805D5AC41FD4BA1D93B87E8CD59D911B1C26E0BA6239089F8784F85D8BBE461A1C01BB3B6CD79C81E3442EFF125B7E0BDD83A65CEEAFE1B64C1CF33541575A589BC5ACC58628AD4A3A774B4FCB692B36B8F46FD1EC132BFF955940421964AE545709F4EC1DB3274B666F2AA234AF0EB67E0345119CB494D5D303B82E4DA00567EB37823485B0ACABA3BBFD7E03A9D7944AA65CEAE299644A8B22B3741E787B2432D21ED290B8D42F54DEC4604326C73B4EBCCCD5563968B381E3D06F151A258E2BD5CAE57D1A3357C966E777C7D091D163D93A8A31A82886D3D366AD9C3131CEA681506E5C46C82C0EF40FB60A05CDE9594EAF5CFE1695A16860600E6AE2C995EBB219E846F5908B82D42F7D1087860535C7A91BE00F5E8FA81BAF85C37AC2E1E0AB17F505FEE66ACB87D7644AD216176835137973B8F19AB03A5C5FBD28E80B047FF34E0AACCED940D6F886B2AE5907EB99FADD27B35BF66D4B56D14CA06BF6E42E7B5B32402B3052A550B93665FCFD7BBBAB3063BCBDC7BBE38561EFF989E9CA9654D4688F0F45A3FDF516747BDFD15D71577A7CEEE59EEE9F1D99DB1691533ADA6A31C0E7AE4AAD4A4E3CDFA13631AABAE18383D9692563D2EE3E36E476A7FAC53AAA5BF95EA65D2FEAB65551B788A266A0E35EB04C52331B885ABC07A2166F6E24EAE87A516B65A482583E846B4D5556979721D00F96161986A5B179354311B27FB236AB597DFB234AA390116A9A92CDD6545757117405840AD20B7B390B7B298ABA1739243EE99F9108862106A416C37895A8D8925DFF841921CC3398090CD9946989456A492270D95AE8BEBCBAB9D03EA548972A5F0263A98F1AD6465B0397056B81E8BE7C676F61CA4310B1662DF1CB89B8BABFB38D336FE5F3BFC8471CFCAE36E0A035943DBE6BFCE868C012DD96E2E1107C9E9BACEF89980D9181547ED4698D8F67328F6C02B68DB4B48FC58C28D5B6A7A7D6D3996B0E0F6EE61DD604DF1B6CDADD13F064B24D819E4CCA666BEE1A40AFA7B71A3D5187BD3E18B486762ED3354D9B225C653C1AB5399B6ACDB6600C91B4785DD6072B1945BA9019E95438E0AB54BF80D03492164F05EB331AB9E48D70E65A75B53C5CE016E5F7768DBB4EE44D30369CBC215467AE09D27039B728C837EA18F8BD1D232E16C9464234492BD6A7F5B6CF9E9AF4F7B77AB52489E17215A1AC496C8DF43ED1E3C14C4D992D9EB117B77923B9F95CD7DE6DCD1EE69C3D31581FDD1AB5E6A7EC2DCD512C917EE5A5433B626A9AA6543A5663E4B4B886D1C4732F766BE02884870F766D3E924F389A47F7BDDA987F65B8AA2AD51F78B4A0D299608D06618D06608D8C880F692DD59A096B84B27262465EA9AE58640A9EEFEEE121AF662A1605A64078BEDB4086F8036538E0CF9EDED3F9E4F6961AC6B7F3F4E3333F1FF77DC4350E35A607EBD88AD8503C3D146630F699CFE6075CE9C77E726AE4D9BFCD0FF41CFFECF503BFDA1D6E15DE18856BA84578034EF24FE1A00441876164A074927D18FB7B3BC320164ACF1651FE62D5948FB9B1EAADA52FD1BABF4AA107CF7E41CF5789237895006348E6C61D6105B82FEFD5554343191C3DE591270B124A8A5CFA234129085C4911FFBACA18D5B8D8C35005A16539D66267C9BFC8E16E1B6BD5C9E53A2BCB5A693976F38812D539CC8C494711EFC9701986910AF9ED0310762B2BC814BC5514F68294FD5D89C0FF79F054183C45CA2E29C57A4E42FF12F01AC40FEF5D4AFEB08B2E62E88217C79150117D5F4AFEF0221E78B8E40F43F2077E44F29BA4E017F4BAC34ABDCB6C71E8E5CB698A0623134A127D9664ED4187ABCEAE3DAC312E17B1E50FD19DA8DFE5BA24A7E4380EBF2E113A9BC5506931A9B13CC594A27069D689F52E5D861D9E86CC0F95677E0A32DF1FF547C5CC6F2A657E52CAFC70EB55B79B084F5A6EFC50E68793A5CC77B75E15A429C4FF282FF7D8A6AE3380DFB77D7D7DEDEBF733F1B593380FBFE22476ECC4719C84BC9CA7E3C4B03C4918A10443A0411D9412421F6850BAA209D669DA1F6C54637D6C85A4036B5DD56A424342EA344D2BD22655139A88D0B4678122D191ECBBD74E48D250B658BAC7F63DC739F73BDF777EBF63BCFF3F337FC513BF02FDA0A897B8A776EF0FC623632D65F078184E00F41D91A1869A91C6C2FCC6C9369BBFB440C0BE11C3250C3C2B9C2F9686CD5B4A46CF8C95A3FDA957867C0AAD9696ABCC5AA59E95680167558990AFA5DC4410B89E4733807D100295454D2FFD03C35CBDFB857C18CFD5AE40FD61717F7301F5F1F7E556AB407D24A644E4A1DB5E2FE9BC0FC45F5C81C25768AFF1866E03ED498DF3BE48FAC5C734D894F2D43ACA679F3F5BECD42AE57B3CA3AF6F8FEFEF0BD9955C71D33367C68B9AAB9D728A44213924123ED0EAEEDED7CAE365275243473AF937B59E78B079A6CC124C04AABBFD7AB43FFEE278B8A07EE8C04BED4DA75E7A76A0522A5732325623D7989514C332E189B93653996FE060AFB7BDCA5A6C9A38D1575814E912283F0A55D20099637B4CF91850DE00948F2E20862CE5CD9B52DE9CA5BCF96B292FD4410EF30DF030E4D20CC902E61DC51C85BEF9E8DFB0B2844A81BDC1AA64ABA07F8781E36416F4328D459D453D8E2460FDBA60FD0A103FD2B042FA327419512385B07A322B6BB3AAE125ADF800C3614B8E60784C25AD8854FBCA0A29BCF853C364FD4DFC29A45FE96D28FE346D98C4EB6FA6F10DA4A736053DB5C2796A23E6815941BCEB371F7DE3F4F60A5B74241AEC0D585C5B4F0C0FCEF5154FED8D8ED6DBAE0F8E8E0CE9BC1DC1548AB78452B5814EBF716A7ACF14AA3BF386BDF19B4DE589D63A8B31DCD0E9AEDDD156E6DC321C48BD526E8D6CE946EFD47775C5F22B7D2E837362495710AE28371B7C15B58E8E446F8E55D15CBEA7C47CCF833457CF2B903A21DB15C58DB71D0E4968D13D695C94AC4DF65B7F08E732DDD1781B4A5FE20E2DA6DD9312E3E20AC0D7643AB196615F4B6FE176108F72A5CD7BCE4E94B6D73A391AA31889C45EDDE14F3CDB56806AABEAE36563C7BAEC9E91EF8CC7F7F7573B143FB540BC42DD7E9DC6D7116A99C6C28DAFBE783055412B38D694A73529214BD89AF1B9664162BDFD332DEDC7C53A987ED9BF03C85D14E92EF1B4575A0B201A63CB7F2702A45DD042249ECD1D1DE68334B161BE794466CEA0B27965B2109ACB642AC7EF6BB7C44CBFAC14EEBE9F562649E1FE7C1A3A6CA038B69A0A1B314E04EA673F3CFAC22F9E0BD5CDFEEAD85168E7CB7A0EC6B71D8ADB4B7B673AB61EEAB063C7CFDD7B772CF5D6831F7DFFC17B63A9B71F9C979FB971BCB6EBE48707722DD469C1F2438C220D881DA9CCD6A91D53C568838A9133D63E7200AAF33300F72722B5AFC89998F82554A5F9B3554E0BD3DC84D080611D6FD0F33AE6A1C6A020718AA1D13242AE32AA4D36B5C444CB8573A79C265ACE32B83CDFA036A9E4D44770BE4209583D98D90470F82444368A1CC9CE4C8F0D5DA92C821712CE607357195528CCF3614B06F55EA9D453DE2417CEA02597A97E5137A0FA04FA40231217EA512CC5ABD9415E61548C4E67C751C2C0F9348C141504C6B8B243379A48B5268A3F6690F81DB56AE41271333A49D24AFA5188D52BA404AD641F6EDB5DADB2C2A9A9667B9B4F06CF4C6214ADAA49ED8B8EBE36E2D5B7BEBCF713CC27050D89ABAD1A5AC2E5EBB5F906038BCA86BF7B68C2E5EAAA71388AED52559E4E615029B8A2427360F8F9E6E80B677E7EE026ADB6409412602787214ADB90D7B351F2635531B63BE9EC8E39BBBB9D315C61C96083571145ADAA56A50F665026266B4FBAEFF23CD99ED4C3C75C5A0A251A16800D9112B9648037C201530CDB157174BB389C4D2779F7DDB4F803A4F00BAB892B54AF103A18B90EE0C175FC5E4DEBF544DF104D1D6C7C87C307DE39D0B07F6B5825257156C154F5EE6B69DCB1C5E14E1EEE3C0C4193508C829E6EDCDD5E6C0EF4066AC7E3952C25A5708C906A025BA71B864E0E7AF8E8504DFDBEA40F9D4A9CDA55ABCDE715AC365F5F60B1397947345519DC56EF9070669D06ECD011DB565DDA1EB4394AED94D2AC650D6AA5DA5E68F1F51F6CADDB9D08331859D1BF0F2ADEB1FC902048ADE87F938FFD8F5DF1BF2F1704FD13039DF3BA3ED9C0131570413040A133935ED37B330B54ADD6DA132D9020B4EA1B52ADDD68E2D5D2478B8C9AA5C07168F406A9B57BF30BFC36C50D8D66E9196C6908BD881E2FB12DFD59E02421914B513BC5E5193436AB4585DD97B2344E4A95F47FFE68C76E3EEAC865D914A9000F7C2E9B6561F4DAAA0756643D309441DD31DA1BBD2B485DC2B83EB59EA082E21839A8E05D510513A471633EFDDF3648810C66C3824F351FFDD9AEFAA91E9F4242E024EC2AB292A69DADF57B7A3C8E8E43299BAFB8506335DAF2B07C5AC1905ACD524465E38B8CD33FDE5D895ED873613ACC190C8C5C63D572264E6AB01A1D4D53F1E8689D8D20097311A6B4F334D46C61C9D2F7300C450313DF86B3C22CC4E94F248F51F8C788604101B0A05988DB1A0B1AC8595040B02038037916CC096660130B12EE8105093737B3A0E0060B9AC5C1F797FC94C25860B13B399442EF3C3A077B11A95563FF54E8188AB8A6CEB398145FFE56CE49718AD5B0449CD658354E27A5B60AE79C1D30F7DF937698FBAF11C16B8797FF866760672987551F10661F13563978CF1DFB5C607712968ABE2C59BFC202DDD5D0CB1DBC9776C43E17219F94083DE7D392CD16F52998C736623EA3F5278FBCBDBFB4778B5F232328B9942E892603E3A7073D98B9A13355BEF7EC607155FA273387CF4F94BCE7681A8F358C46ACA69AA1C6CED7B05F26DF3D7F7A5784E174EA3CB3CEC2519C86EB98BD38CC59F56CCDE4AB7DA91F7EAB65F0C25F668E5D4AFBCA7B765645C69B0A3D4224DA2032D721121AC4853262244C256AB454853A59D429479D52B4488296E16829867A32CB1FC78A9458D7760FAA357268979653C245CFC24526870BCFC0C528BCFB00831F47F8E53B0BD08DCF2CFF6B4129B67F5D60C5F68B05B9D8DEB9026D218FF219CC13A3653C2C480CC165302046C3089FAC47862199E5DF899F641CFC6F4498844C782343641EB725EB20456B1C44A546853A74B95C23AE11EED6884BF8E36E898DCB5F0E5FE6FE40522C2B9252F4344981C522D6380A815FF7EDBD74FCF98B93AEF2F4A5B923D05E52585C91AEF281A93A7D7EC3CEB6D0405D8991C64E9DFBE2F2F8D6B71E9C3FFBE0BFB4976B6C14D715C7E7CECEEC6B66676766DF4F76BDEF5D3F76D7F61A7B6C3386C5D8608CCD2BB663076800F3D8CA84E2A80E498394B6AA12245488519B4422A92AA45651ACD67663B9544A62DA0FA1486D822AC4978254292952A594F2215278F4DC3BE307A64D1329DD0F7B67CECCB95E9FD7FDFDC9FAF6BED79EDDD5E4EB3B7DA9F2E32BA75AE21B9E3AFE03DC4DEF00115C0056A9453CC9423C1E46F1108A07512C80E27E14F7A1A417253D2843B2234720B0791C0B1B4E481E5138F854664E0B79460F39596D64252187F5F3690EAF404242D88B9DBC1CFEE6A4B98737B10BACD7A6614F58DFC35BADB0BF87B7904872C0E34D09490E68E475D3B1ED19710E9916F1E43E409516F5DCD5DCE55CFD3FC9E5EF49ECA1CA46963E8169871AC33BCC56600B23DE63995472FEABAB11A53EFA1F504C22E3F082D16A33DD1F36F19CD168B19991F00897C95ED91B918DB7CD82852D3BFCA209CE4587EC972C86EB9356C616F6485E11031AC320C6C419BF386391FC9093E390933708A57D487262CB94502E8C3221940C231507DF8383AF2237EE06B7C8DB508F1B07D30DE5BC84735A469AE7E917294E0B21072155393B0EFA57C5BC5CEE564E273DBD8AA9111CC9FF3FF4BD41A0AF5170D94D06AB9DFFE289C3CD72B0B1AFA16D5F77813771268666CD5E65F0A8A241DF0FC7AED2F55F0E7DE92AB31C76D9DDA2E08AC7BC04FA4E9E993A4EA00FA23E0A13E92CA63EB4493B578620D0411CE821543043280B78EC1448B40B38DA053870542BC6C25EAF036D85C47CAA26E1956404BE9648117B06B067807806B067406F07C0C89A59CA8C3B089C67F07411F4B217F44E1270BA1D903C4151E15651F126750A226DA1B7876AC5C6AF01A37844E5707273D77082F1476353B82078BA38B2022A6F571067F8BAA8BAE7A9916F8456CFB69FF8E5D18E67065AECC0A082CDD2B863ACBC7E7FB92AB76362EBC9455A7D86D06A437F63CBBE9EA2154AC3401BCD8E965D63EAD08F9E045A1D52368CF5D5A0E383670E36B9426B04C11972C5839104D0EAAE62D380BA8256079BD240ABB1748CB507DC7690070EA894DA9DE33AAD9A1AFB8EC2D9950702F958A35594201DDA92A845C91A94AA46F1148A2751228892011423E333E145090F4ABA51D285924E041801451267519C41B90022B354D666698DDB0B176E3C62DD7A19E0F55DC8BE3B585B2BCE3DBCA786E00D11B7BD886B4A1461C88AF8101445A822F1B7B444A528469BA40C1C60B8ED19DCF65678CC30F9BA2FC7687C86E122A8D78FADA2547F95ACCB9DBFEAF3CDC2B6E163A77CD6EC8CF8BC1187E9FE6D5EB481B6B39AD047AC235C1D8E16C2E259C9F5E02D0DB68F45930F3E33736686812F04E811F63AC23E8FCD209B7933C0B6CD72EF0F31FAEFF75B70771F80EE3E0FD4D84EDDD3666AAA09A54A9833920632537FA38DD4267D6EC2FAD90C076DD5340F314D4392D2604DE31E4C0BDB8A63C5178B866208272284131122CD1DC2CD1D9AA7EB290A76D1A961161E53AA03AEDEB58BA807F31E21F4EA96BB912A04885EBD0AEC4734EECB21F12F7A775E1EB9A635AA96069C07C2FAD56427A952D572B74255419F92DD1E037EE8C6FFCDFCC6C791BF44DAD670BEF3D4AF2AAD959D25BB91A50D6640FEECA6C35D1B8EF5D7A6FA9FDFDD369024C0DF66B65B818B1F8462DDF9B18B63CDE8CD433F1B6B917C5E8197FCB21490CCBE903F521EDDDCBE67DD1AFE11D89F64E9C67D2F635E3E0A59FA1D61FD0F09EB3F0D9D364558FF8E36932598B156298A7A245163844FF521487083DC6BACF039A9F813C0931212E716BD44EC25EA5EA2EE451E731C8F7AC645DC9E466C07E7E862554411A54309ACD767F0B876E954E2C27B71FA6A27EBCD59F071B1D22A19A26109C9209020E92E7DC96165227D056592D46004E7648A28935AD6EE89FBAB92126D44B7EF9F733858AB60A1EF3CAA4CEC165D99A4419A392CBA32D90E919E27CAE48F4499E0FB8BBA32F984748823538BB22CCA30286340D9244A5A51190FAB080E49198E44DBE269187AAE809A0BDD85C30543AE80E05804CD4A0942843A46618ABEA977C2CD19DC090A3EFBC055C13C2763F7710595944EE5A062882B4899A373AA50974009F54E24622ADDCD625164D645D13FF0290694776B04C7EE3284B019C8046E8A2B7B03BA43B547402AC10659504D592297CC4B7229874FAAFFA6958CC615F8CDACD64A179DF9FE93BF3896EBEFA876425C3933976EDB5EBFEF95816ABAF1D5BD957383A9E2919F1FEF7F61584D495355EBF7AEEB185682BEB543EBB79CA6E777BE7DE195430A27CAF21ABFDB2FB076D9BEE57B1787D7E49583A777EC7EFDD9CECCD66FBFFC56E7A9A94ABE6EDBFE46E55BE5440DCE55177D99BEC17E429B1816C8F075B0C4E82B689CFD1B588CBA650BFD677A94BC63D22D1DE035482C66DD92A0AFD0B3EC5FC162D12D1BE19D9DEC0DB05875CB9360798D7871BAA5085E7BC93BFCD2DFBA4CFF84BC63D32D65B07C9F5804DDD26F10609F4D60B1EB96BDF466F4B4F1185864DD320096616271600B518519FA06DD03AA3045B5E32ABC4439D134146818FD1A749A6FC13E115B609F27A580EA6E5DBB55C86344F12D54EC136C6CA1028F168554E27121B52293ED347D23B5FD85DDBB4EF625D3FD78DD963AE7AF2B571737661D817C39572CE7E44BC3AF1E696E1C9DDC333479A4A5343A7960C7D88650AAEB5007ACC164D7213CA1620FF3689CEEA6A254AB36A1A2E89D198FC4F1DC1C724C07BFCB9E84FEC7CAE46A3DFEB1D3BC1AC44F662AE4116E75A237D8A5B371596934E987231A37CB01A713E4C3A4205B599AB598E60D1C70ACD36763E64C161678C762A24FF49968AB5B129C362B3B86181A1918130BBF70CBC3123D0A312D519BB55FE84477676BA23551AA7E8E5EAF5A2D9EEBE909BE61C1F01CA17F1C5742FC24B6B6B4E77A051E1B1A162AF082A690C8EF352EC37C1386F9E40A886356B2BC9B1EE58507594EE25883D966FD69B1ADCAAA2A09A52662662C468351CE2A5DE9757BDAD7D86A9FE83E827A79FB995098E1DD92E87648DCF97CAF5AF2D6B53ADD4EA3DD23BA03B2CF2544D6F6D6C436EE3A543E10C639E880AA1984FFB0977A49FB0F6BD047AAADB33BDEB936DED9195F6B107C73E85FAA9F12366755B7BF2B3BF5A7F0CD301D0EB3F9F73B26DC1FE805458EDE1C889E6BB7341A26288CA310D81CCE4E55A8B018A6DD0670EBC8BF5FE99860DD1F2C159C7ECA362F17DE32F59656436F69295AC965E23562E0A50773DBBED359D7D31401DEA5ADBC3152DC90E9D9EE2D74E5BBCC5613C398ACA68D3B065ADBE2EB0A511099B481E5AB95AE64FB485BA8776B7A6343D0B576A035C24B9289B37B647750764AAD4DA1BA88681440F73879E37AA5B6E4F038BC219B6CB3F01EA7106CD894EBDA2FFE9BF2728B8DE2BCE2F87C73DBD999D999D99D9DD98BF7E2DDD9ABEDDDB577672F5E1B3CC6066C0C050A947013A6044ACB96D602414D0AC698A47968DAB40F6D421FFA98C85C36DC1A57A8012AA4AAA244A24A82AAAA52E9439F1235547D6829B17B66D6EB4B129A56B647EB6FBE1D9DEF9CFFF99FDFE044B0CB800E8CCF75E237F041936DB151339B57B31169065DB89E24492C3383CE1AA29AF466EFCCB3DE77D81796F2A3A59D5CE1513E6FA64D4A7AA5EC9DDA92AD9FC2C2B889854A53FAC567602151C06FF08EE38C6435013D7B09C80EBAC046A3205436E60BA47CFC714E98FD2BFEE4A9C71F789D06551170799D84F3893EC5CDA2B7691BACD96064A65AD1CF665F3455B31A54B315EF0322DCDD504D194DDE48E553792138831E18222644DFA956A9D2C3CC09EF9D45897C64E963A141E46AF49D1A6CCB941ED6322728EF9DA592685F26872F262EA5415CF8D6CC96A36BB4FE62822749024E4AD97CE9BE5CB23FEB75B7AF2D2474BF4B94557414C898141CB37F94B3DE35875647BB8CAF0E688CE06459411161E2DB449720464BA9D6CEB0C0B854B4C1E366048F106CBD8EA370CF3673EAEF821C9C87CEC9620676D0CCC295647906BD759DF57AD9DC0CAA1B1E8CC57449C73FD6917EB9AD8D8ADE768EF7DE5A928D3D638D7CC018B66A2EB7E9976BB0D119BD5D738E53BDB79E918FA64D90CDF29B0D616B9CBF61D5B4ADE9D4E763EB6A435B8F182D7C48DFF8ED114FAEAD95674CCF637CB1CE606563C18B5A77F6ACD9DBEBFFA1D0AA27321B4272AC1C4F16A362B6BC776DAABCFFE54D9D07F66DEB8F530CCFAB8A5371500C638BF76FEF720763C68E15D1A2267BDDAB77963C6ABC00CAC8430F8C8232C298D1508684FE7E8DA61560B47F00D92884EBA1FF04F7C23CD8FDC93CBDA5053BE1773DAC59B73E8FE1F4E50C878F120443CEFE9970C82125D02A12283BFB8A83276896467F038EA37052505C2EEE939F32761A4CDFC1E0C7C201B07D86727830027C3D8DBF06B50B631D582F36D1989609740993B05674C960FD5CD02FC10F93B989AEC0082DA32B468069CF13982669F8C71AD22E2AE362355CC5E7AAA87A91306BBA67CCFC03755BC3D552395CA4470D1B14E1CB8A76B1A68C13D58B3562A1AEDDF3AD6C55B554CE124DAD9BAEA6AA84E976B439773D25194A9B1408C5AD820DE2AFFDFC07AB0E8FA4763C172F27DCD1C1AF0D0EEEEF0BAD1FDEB6677A659FB1528CE4E3C77C7272452A5E8C4AC31BD60FA3C3DFF474ADCBAFDD9F76A6739548C74831E4EF5ADDD6B72F96DE8706B21D9D69556B0D4995D95FF9925A54965B63498FDE9533953E08D93A07D932F9F6794BE9C12E53E998240192A2B70C35291A61039F339071391AB5956E778C7B6FD9960BFDD1DD459D478DCB35D8D751BA5DEB18B7796FD56C9FD139B96C0C80AF993A5842245962095AE2E7F8B0BEE95B23A1723602F88E733CED4B57E2E5CDBA9F9012994270CDBEDE96F8F011B311FCAFE2EE7831162F6AA210D113EDEBFFD07970745B7F8CE60496E5452EAA723C1B37BE92B73B1D8CD6BFBD501E5D9BAEEC7FB9DABFB3E25393854044D7640F287DF3DC51FC3CBA081AB2E8E54A809C415386DDCD07BCEE3A03C0972BDCFFE47E0199966E9E9A0D787977BDC61870CB9B2BF8EFB7DF2F2CFAB86568456BAA950A05EBE0AAA2C0053FEFE4EE052391D03D4E14B8DF852291E03D5E7A35E87FC0721CFBC01FF407DFE5789E7B3708318DCED6D17EFC7D4CC3F2664CB7A0FB4E6334E6476740C404BA704D0973E7B0BEBB28F7F883471F98C2BC6A2E78FBEE9AFD066DEA563D6AC353920D29CE83141AEDA9F65648C479155111EDB856D49CAE981E418CC3E374F978027FFBF893B353FF1E673886C4498A5C797A726A70F0DCE4441F0E8C45301C44F71C44B7DB8A4E6F7843189DBEAA88D84D683C1746A133D7443FDB080F82932C42BD6AAE34E233DDCFD6B483920902C944430D9E108176BBB46294B08BAAA07A39A2A7BBBB07C779AFEC54051A458B31F9D7835393A75742643809319EFCD7B9B34F8EC307888DC2FB2626CF81CE2B738FF103F84F1609DA70B9B1102BF990AF2E4E84A1EBEBD4591035343A08FACE7B77E609BA5E132728AD5E839B4D82465F44D007A26B0E0F0D1F5A158E0C1E1EDE78D8F07F5F8A94E29A1E91643847AA1076A0B51B4EEFC867B79FDA34FCDD9D7A69D7C9E1CAF66A3050D95219DC5554423D5B20A35D734FD014FE6320E84A93A0EB066B21F4D3C00435B98C9F0DD604E8A735EBC6023DC7FF1B3D4F31CE16456E715AF44C2200839BA4DDA1488A4FA49CF01F0E938446FFFC928DB0BB9DB0CE9247108E1008C0A4E75E300F3FE4B3846D6DD2F37BBF30E93963E233694876C9833CF5D484230C23B24E9C31739BB792DBC068A9C9D1F51A6C22F47A0DF62C7074FCFFE168BF839DDDCF8BF00A4073DC8BF17CC851CA468B493FBCE1510425248BFD1A64B6C5991E2EEF4541C1510CF880A365517189F6939A9EEDF025F3926CA2A0D3ED96DC2EBE253F988EF40D6CC86CB2383A03DA91E0AC9BB11F3539FA9AE118DA101BAAC68686625582078E7EDF68C5F862312D75A2CEE981701AA5DF0C8BA23B1CA60626C26EE4BE302F2F8BF372D24763F0EBECCE7D0E55FBE029039DD3B570FACD5AF30994FB4253828D07B4B7C397174B4C2EB1D36763B5B5442F240FB85A4A0D1D5C19EDD3E3226DB73381B6DE36AD2B2CBA922B3B56D9589A244102FD6BD7E9DD413D1DA449806F44506CAC3490A87CB9DC226B85506A455AFD65669D1EB40B4EC9E76F71894E41F2459C9E98DF41F16E419439B290D172922C92BC2C7022CFB0B2D3E16B5F110FE6D34186F4A78AE61C72CF3DC157E0AF587CFDFC225FBFD4E4EB09C3A5265BC259949D9EE7E6D3ECE422628F7D96B15BA4EC746DC9DEFF91B1E5258C4D988CBD82E7466CC0D832BC68CEFE85E16D24226DF48784E0D1BC81A48F1D611D1FE2BFF9ED8C3F304633266133F41809EE24A92E8945DF206D167553B3C73C68F5ECEF318BA31EE332FE3DAC821D6AE8A90B4DDC8875C43AF89619509617E3CD9E79035EA824D0C7E5F4A9B08AD4E9057B32A7EED8A7683BA4BF5183FDE9DCE55AFA14A54E2FF8D572BA2497D37662196CABCB585B4E8F7CDD68EBCF06C004289AB67B12D54C584FA843C3D96E8F5394DD682BE7F80FE7551FDBC479C6EFBDF3D967FBCEBEF39D7DF6F9CEE7388E9DD838FECE174E62E7838510420A0868200943814EC8CAAA35628C31BAAEB454A5EB3F631FD226F5AFA6A052A400A1D9185DA7995131B2AD1AFB9236B13F2A8D4D436BFF42FB0367CF7BB6B39095499D2DDDBDCFF33EEFF9FC7B7FCFF3FC5E96ADFE4B4CF285A952E8C3DCAE1EDDCA7136D90B1ADBCC3A59CE1FD33239C629214514BC9AE63F8B902F3582773B05082420A3B0423330586CC9E2C394CDE3B1C166BF0D18D870222DB4B6D27C1005DF8212DD85BA2E7C1A066BCAA335B550867867F02D5CB3BB2E3C018346DDFE74851D592FB013A18199C2D0C15E2DB0F5C47E291ED1A0174212C089B44D490FC644A48F277A7766E5B3C9825E529D8184AAC5FD8EDF2676F5B7C427E6B76E7FE950376DB1DB059E975893C5620EE486232E49CF8FA6F3DDA233339A945DC138664408D8EF044668C4504359FFA6AEAC7F57640937CDBB90EB92F76B764C7920FB63EA9AF6BA2E958DB9C7D43555535575715D3B4DC18F900CFD90B28BAAA834F1A409D1D55B768EB3A3252B67A1C85D9C24B86C5516977F939565D0AAA205BC94DD0DBABA05F62C037BD64CA48941E2B99AAE8EA1D3844484D1E9A22DE06C0A48F0B565AEA3B7419214D0C5A2CB96E8A4F9288ABEE97B5E2FA1D2C2BA0D7C82968605BEE89B65DFF3746961DD0E766FD8C027AA698FDC47ADA9E99A82CC04076687063E5F0CBA235DCD8154C8E5DF3C3D58DADFA58C148677FDA0BDD093CE17A4163F2FF0A16C486ED505AB9A691DC85F6C1DCEA89E7831A6A66211DEA947DBB5507F4AF3C67B9A73E39ABE13999A636DCD7AC26773FBFCD5DB6240511CACD7AFBB8480EC8803CB9B01B12020160396D74E91BEF832BA78956059228F592E37E970C45850551A93FD7CC46814E737927CBDBA567B16CA101E499D2F47A0299CDFC871F4DFEDA0D36C7EB25AE92083FAC8F17D6A2EA6B2700AB47A9B93C144292692C1ED9B7A7765E550E960FFE04CAFFA1AAFC75525A639EDBED64060104D8F9E3EDC63B1B39CE008FA6C761B303BEA92B4FC48ACBD46FEEECED1A42404E38ABF4DE304627595E8228F9387698DB49824280167C093265F402FD23EF0B8EB9E027986548C184FDD938055BCE191EB1E897C81ECA525F078EB9E0CC4883450DEE4AB7B52E04918AB94BA2704AB9C468CBFEE6981988C11A3D63DCDE0091A1E0D7B08B4BA6DF54FD41C9D27DC0D0D26AEBE7FD5C9A33171190676B45D6460B0C4A3ED0C4F1349A8D671E8CCB0538B607A93B841D7556D4727C28DC5E3315B42B44352258FDF4E31D469DAE1F6BBDD7E966218ABD542319CC8D256C66EA62C0E09EBEA2D4499BA06682489ACD11563E1C032622FB36657FD0C023F7625167661EFD5325B04FFBA0388418726614D0636BA5CB64968248F80EB0275CD6C7330D5654650DD9226C0C8CAD9CC661BC7A0AD8CA0496E55C023CE4E9345D1EF62AAE7405ED3345C509971F94517F860C45969DACAE159979FE08923C4A469BF699CB0104E4286B35414FE4527D14F7C8ED841EC25668867882F125F269E4763861E9F9BF8427977B9EBF8C9C2C9D667E737CD070FCE8667999131768C280E9986F8544ECA954FCECF8E0DE5724363B3F327CB1675DF01AF3AFAA563E3C7064E9CDA722A7374AE634E999C0E4CBB76EEF1EC217BFACC7DB658BBA3FDD8A9B9E93D7DEDED7D7BA6E74E1DB3448E1C0A4588E44A7245309417FE08597E25F3BF2F08AF707D96153865BBFEBFF72B46601F95CFFA8AC6963787F2B96C265ABF8BF5BB5CBF37E62D1BEC8DF78DF316CFE376CB86E7377E8FBA9BCAE552E7F0E561369D4D87F1A8DA9981CF3BD9743A4BEEC4D7470A76902FAEC53EBA94CA65326194CEE5D2E8169EAC1EC0D78738FA1C1E51DFC960E196CE567F9FCDA6EF8181BE0B833DF8695F850BBA9149E61F8DC0E8DBA9548E0CD683AA1618DCC7CBFE984BE5DA610099AE922BE45DFA6FA499B942E0DAF43AF96BF20DFA23B09708DC8F27C99BE435FA63225B532797096F6419BD5AE4B91DE999F4BD34E54FFBD36D4D15D732F9F295B60A330F05BBFF81805BD9144A3EB85B316AB533ADA7DF48535C2DDAD55429E3F825A6AD52861546D53696CC4C4F19FB66B6D43A76AD527B648F21C842916824522BD81EA39C63A5E221AF81108DF54F6C1AFFFAFE6C6EEA1BE3A5F9186F75D8AD8A5DD95D484EF4343D7B58EB4CB6B0BCDB6A67A9DD418DB5C8B2909DFDD6CCA1EF97BB43CD8E90140CF0163ED8327274F8ECCB568EB7D8584F0D8B0FC94F0C2CDE251AF63EC35E36B0791DB02BD11F117E62C2E86934B78C9E2EB2929590E80AC709A68A77997C6951780E4B15DCC240A655F84706262C4757CA10E33555CA8D282C5AD6BA1716A0D99A126D16A4F5E2942CD984EA27AB3CC7F1AB8168049A2D9A156CF4794DFF8BA7490B55FFE0104507F9335DD2E09D27C90AF967FA6378E71FD5FFC30AB962ECF7F5868DBC86FD63C3A6C9DBA8D7B06FD4E7EF90CBF4DFC17E6F0D839261FFA46EFF8ABA47DF07FB7DC3FE263CFFBE61FFB43EFF0BF29481D94D02EBDD49F210759CFE272102A366306A5737A92189B88E5E0571604367AF8536A93A5D89023FAECACE8A8E097507A3D78DE9F4A0020380EF5D088A028038AC6895F5A2EEAC9475834A0A7FA7A192D0630DBE8F1425A3FF47CD069A356EB925504C329CFF5E9352139B0B1319B798DA51283C9571FFC3EDB0E787C7C3C1A7BA2E5F68DFFFCAE485770E740E8B9C60A54C070A47B6C512DB0F77F63E83EF47AA27C25E319C6FE2E5DB95A9EF1DEDF9E57B37E6E22D8C99714880C15EC0E02B06063F3778B30572AA07301826E66ABDB480F62D85D3E134A72CA333458EE09C596756EE5E2C28741B4E2C799136120BFE562DB780480F0CBDF80073A916DED6BD58AE2F58A2E5C5325DCB2C08AB25D79A268A44B162C47915803EDC91C712F13F1AD248B646AE993174189D9ED8D6C39B0B53AAC365A564BBC2DAF5684ADB3C2247B2FEF0C8E670CBC064873FDF1EB6DB180F2BDBA5BE44475E8E66D4F0684F0B75A5FBE95EDDCF330EDE230C0A160BEFB46DCE29D180C20A91FCB68EDC4487CA38459BCD230D3968BB968F2811DD07731DA3809D09B0DB696077CBE0D35EC0EE92C1CF0F0C2C1598FF80FE2BF4D41EA33EE95605B2E9B2443970525138F55CDDFD288993EE8710BD7ECA6BCC615C6AC2304A813A31D4A22C8A0D8D826E5A9ADAE480EBDF6C576B6C14D719BD77DECF9DC7EEECCCCEECAED7BBEB5DBF1FBB36B663C0102038049106910201DAA83490AA2BA540A2B67FF2AB51D356A54DFBA7A984AAFE6814B5692430262410FA235324944248B0D254294D50D4A88D64F52505358D57FDEEDD995D638C64EB6A7C8739DF77EE3DE77C2A7B5B12B46221E519327BE50D56347C37C8A9BCC87C9284CCA22BCCEF529EC692D5F259E60108112CAB790EC198058CCF03C611B49B603CE70D7BC3280D443D734E75474A9C9583E5BC7582276867C157095E4219C5DCB7C6362FDA47B003C8083D1B07ACE45A85B0CC7734592B75BBBE2171BF7A81978D8C53E85614EDC69B9AA282DA644C4538758A97129ED355D264FD4D564E67A022411171D0FCABA8082C9FF052F8617C7F2AA3F3ACA84ACDFF62598488C6C13BCDD75AB55E659E805ABBD12E52EBBC23752338D46764290BE81790E4C066D27F8E52436B85F0664FD35AB3ABB7789D3A63116CD7D6C54C4EB659FA7E57A552784711D542DEF14D993B3A327A98930D2F59286A92C231CD8F0C5936700EBFE8783A4BB0FFE5A9E337255DE2802512B163274445D4D39A3D5F2987A59CA6D9B4ED76CCCE34CC68F4D6295A396CACDAE0C53B06EE60A6DA7D67F6055E44B6CE5CE3B19AF7BCAC2970F7363F5ECF40E6F4BC2E15F35861642B70D2794B61F61CB9C67C9A306506F3A2307F9A970486952C9DB929CA1CC3708AF0F3E65B309346BE8DF268004DA16FD1DEFBA852BF84F7221575E1EF41721F04E34E20D5AF84D76BB856937A428320AF87D2716ADB80FCD0D234D196C525B0298B70B2C9F66B95B0816A38CDC22B464FD8202F2D4875F0EEE3ED890B9C7B60BAA5305C29F6AAD6480A93469E658914971CAAC29D25F38B64A9B67EFBE03B46C64F9C9F3BB4A190F07B3385A95EEFA21E0C96F67EB9D0EBEBC376795DEFE34FE7078A49F6747963ADE2EB498FB9E42583FA0323D9FA4049150AB52D78DA297ADA1FECAEFEE6EBD9819C712B91EB23AC461E8E6C50887B49572EA224780D423EDE3F2F67429D74C00F855607A8C740D567F50CB833A953F0C38610D5192BE8CAFAE81059B26835FF1A3FF2D3C75E4C0441E2EC911F3D3A7632B867DFD6030736EF9DC973478F3CFF580D60BFEE2527BEFAC3839387E77A976F15B73D4E53044D1568101DA129C2AF921461CA852492FDF07A05572A4284B31AE3A44CE1919BED3861550027AA0049B0BB03BEDA011F91D41E85DA79A2C5114D1BD1B2E490A7CC3E5690F9E6370DA8073FC3CB3CCBF222DFFCD367F09B2E3FC3837024595635D209B1559B68B8A69136C4CB92E6254C2F217C2C9A2E4D913485C0C4D3A027339B35BCF3F89133A8D72069521BE779C50B8BC511259C2686970A479E8C0B253F344A82F51145DC64C0E6A2173660FBB41236C80B0B23A9B031F264BB52F2D3F6BB952160DDE444ECFDB4728B9CD776D2CC73CC4DA88C938AD35F98BCE7E0A662CFE603EBF2EBFD971485858B8755C148087A796A6643FEE08F0F4F4C1F7D6EDFD0831BFB4D81DFA3DA1AE766DDFE1D4767B61C9D2B2712E78B4523A50ABC62EBCD2B9667A70D69FA6B3F39F0A59F7D7DBD99CE947BA2FC08590C7C8176E57449233D71CBEA790C41DBED070A37C9A5F0BA86D750A1434BA09C8BCBD1017815F5C3665D2BC129D0E014ACA54A84FED9155D99B0BAADF632727F079EC54BE62A785C2AE51B62F38660C2689B3705C1CCDBC9C014F050FC37E6436818CB72228F7736CFC66BE6D378D5BC8187E275AB669237418DFA5BB711E14790824C907B5A1AA691E6D5F80140BF03718C0D7BAB11757074BE0DDF8BF22CAAA01AED31195FF62FA8DDA17982870691648E47680BCF993097444F491287EF26DB5D890040289A185FD71A49C8959F382B9839FA79BB6FCB58BAB7DBE52451D7554B52B2A984A38BFCBB3196E56B630F4D77498AC62B29BF12483CAF496EB5AB3547D08C0D897827CD30A3DDCA05BC1F5A540506BB1533204C9A6DEA89652ED56AB44D77EDF0A22D9DB457A9E235684EB67C08C60B9105A382EFEBBE1F946DBEF93FDE0C805953948C2005A5F1B7052D95755D5FE3247519C3A4C1DCE2259E05EBC1879BA76235606EB7197F05DF4F1ED09B64EACDCBCDEF1A1A8A940E6607E4A371CA85884999264A03781CEA54DAA0B4ABB43278B8A06310B2967ED104D0112BAA5EAD5AB08B1DAA4344AD4E1A410C9BFFE3F209501F2B52A7F7237814079D59D00C7A94E218CA900B571A55E8BD2B4D10E91C765536DF4B56F91371328BE2CAE252CD5C6AB57F62AD9DDEEC8A74D6EE3CDB1A69AB6C29D961A3E5216EB24EE8A07CB01F8866C6490609F1EF58062935D30919FF1963D1F4E0A921E693F7B9858C295C616F88B693B17728494D663E82E2E01F347DD3F2455680DE730207EB37DACFDFF51DF82FACE57F33BAED1B02AF593A74229AD6D006B4877642AD9FC75F3CB3A13775013F0C51620A883186334572BA32ABCEDF62D482BBF6AC3E8171B2AE9660B2B0EEAE3DCFBAF5CA78E768327F9361CAA8D881E7196F9BE524E61831015AE325C4BC35950A9C94FA4B23EB672C084486EF9844833ED72C48E25697879FCD6D1D1C7FA8B779809708DD12CFBCE7394222936CBE9D858B3B54DFD2855F8ACF2CCD07743E4559D417F5A02C905369A11C189382AC72280820AB0E09A41AE900F524720A22EBD504C881B0C701ED8D7711FD2036D4EEC1EABAB995B9E1E99EED5FD9B6474AF80E1154DFFA7530BA79FB48E6646E6028BD6B67A55EB4B9E58D87B7559BFF68D3F95E26C5252A533B267AEA9ED8FCDCE919875AA23913AD435BD1B3D4678767D12418EC7C5F76D622A9C2CB0ECF86C8C2C832AD82F5A1F54F8BB72C77635820ECB911C3345D1C234110DC657129CA81B9616B16CCE58E776516DE2E6C0C1B9DF709FB51D6381627C29674C28C79571FC8081A25441724951DA76367140EBFCD6969DBCC05BAB0437173FDD92962390E7428307E93EF3346E6C63CAB67AA94EACA7AFA7D327FB958D5F299EDBBBBC70A06F3418B62457A2D3B5A4A362FB51BF7BE67B3582A4E6CEDABCE8E963529288FE67F9BB6818B319565172D921A713C75C22C40A7CAD34820AEE159821DE63A07A10663DA3251AB052167878DDC0AF26B6B30DFCE19345DED861196BFCA5B44AC2CFE2D38A98057E2981E92F0F9170CD710979F6AC3FE01242BC3F240D54C0FF0ED657ECFBC0CAE3680B6117C9B155480333B80D2F05B45157030F0CFF920E4698A9A8D63ED85F84F0B7C1036F8565EA217B5254E2BFC81A6A58D2B9E302F33FDBB8E6DDFDCD839289859C706857507662AD5993E97B7FC642A0B03EF7FE6BEF160B567C71373F893FF335E6DB18D9C6574E61FDB33F6D8339EF1CCD81EDFE2B1C7F75BE2D88EEDF8123BB173DBC4692E9B6C92BD249BBDC5BB4B97360B4B69D5AA2005A997455021B52F488B545109946C4B578B8AFA8A221E00418BF6059E7841C02320D6F0CFD849936E59EA8719DBF330E7FBBEF39D73FEC315EB0C0F9E1A14C581E901F497476B072BF8EC63542A70A89795959492D9593408D6355F412C481071ECD1F607E0E07E9BD6FA7E0E0E903674F4DF2095A8B25B47E2DAC3A8EED451D4830C5AEF6B3E3BD7BA31E6F1349E7DA67573CCF30D3E36124B8F0468215683779902BF3AF7F6CDD250FB9DCDF36FDF1C1EDA7E677BEDA596AF7F69A7B1FA52CBDFBF745BC19A43B36048D38682687DDF647560641708A6021948A615243AD593145233B84EF992CB29A754A878F002864C64A761A029F2271F0A6683F18010ED5EE12DD2846EDA39CE2EF2E0FC1523E7B1F2F88B30DB1A1E1778F85619CD8329CD6D980E3DF7CD986451BB2061F1E32F5782EEF1B7E772652C3318509AE09394DF872E0F570CF70E802913D5B133168ABB97698499C9295FA9DFA7E7714A1F2A344323E72B1E2E7DA6F11AFA4D07DAB639788FDBC7FE28B3D42C3AF253BC9D87E71723CEF3B4949F8E0667CE6CD77721CA00EA05139A5BC80432BCDF3F1C53418E4A22BC572D6D5AF24849E99C7453D24A927698D6728723FCB5DA394568A2796559D0E3014B810CA3BA5288AFD754858ABDE976FF522B13D4C278370626E2F33B93E1E6908C1BB42C21A51BF1B31BD79E9F271933D98A8CA59D66291308D4D27E82D4C36AC385F1F0D7EED86315B97F36EB02B6A1D5AADF64E1708273C62D364B6BA43963116D16219893C4B8C409768115AD82811038EAF285603DE32780C69B1E53B8EA46E3605E7315FA471FE2BF2FB2649F52BABE8DB90492C405FC813227A550369F8E0EA895427D83038355647A43438F9882F9302F98B7B7DC2E97D8F998A4093D6A22029178B8657F043A280284F83D12E6CB7BACD7E367D0478CC94C3DFE37FA71A7AAF03484F681A6661DE6C8F05E1246C2830FDAC9A4568EAA4391B5FCF1E6277BFE847E2ED91E755ED2F98E8E883C078F8759F52968C6176E4F26C6D31E1D81E306BD33514F9EBDB1F59C3CEC61CD3C8B7ECB6EEF3CB0155C1337266490AE5D6AC824456BB4362723D00C3DB73A3DAB67EDE80CC7F7F7FF0900A9741AF6D00EF77D09EE7B14E690F05EB05F451E0C6A198F8A9CD1E64F6EFE49E470FB35DD44AEB6B54B12BC8BB6AB083AFC5010967CCDEB530B37AA76A33BDDBA39397C8A34E17A5227C8197F657948C4FCD76B63E7F2F6AF32816234BFE613A05AC42B2116E4F39B53D1C2953716D3DB5BCBB5008B130CEF84219F48CD6D170559AE2E97E56244705B67DB35A73B5551A6E140236041B38158117E1FE34CB094FD36D7E34397F23A1CD5295D0E400AF4D4C30BC562416FE81CE07D7EAB48020D3AD1D9A08D9409FD9785D12445897B7C858147084A0FDE73DA6C0E0E4130844325B006FBE74042C82012A9EAEDFA603016C30EFB870D1C6AC6D1E4D330A17585A34B4165FA3DA187B05427867D5384D49AB54057D6C1EFF0C493056B6F7D6FF7FB172FAE2C79EB971B631B15D7D2CAA58BAF4F9E9A196702A5D86BB6E5F3A76716D79617007E73E7F2E5A96BF1E0D6686C6AC8E31A6884EA9722892D7475B052CEDA22B2DF52EDEC0D9D0ACBAD5CA95E43541EC82A0FC23099A7F66D8EBC4A0487432B27470C6A0532F82DA2452CC7D8D0CD59F9433A048EF3B80C723ADD09838011E4C81FB260C93DF7E6ADFE598E35C255B507738191D5A203F8B6EAE31B05BBD46C4F2DDEA88A3B282DD7B3896A9031CBE544FE0CF8CBCC0F5F5DA0A118D8448B8134A45A57F39CD75F5DCEE4548EBC5E2F5C9A8EBB5365AFBF18B5F6297595919FC2047C4DF5B33EE867BD6268F0292CC6D72BE6F76A315FC2D4828EE2B97A6DB5208AC5F5D1DA7A41BCCCFAB37224EB31B1FE9C1CCD7A48609A7D796D20B9F2F2FCEC2BCAFD9595E96B3577687CB3307D55B96F40769E451E021CDB829EE6D8335A1F804FF68FF9DAEF1475AE405F93FF8FAFE124D1B9AB371AF56FFE8035E1E4BB3A2BE7625F20C83FF20CCB4135F8E712C9DA38567745A32188C7FBACB217B3C847C08A5D471248F07D1A58BD6CF7AD5EF0E9BE1EC4BA3F400F4234ADF89BCC7D497BB30A9D6D8A2699DD585EA24A65773AE422181D894BA9A237DBCAD8CDD1C9C20E5A12FE1C318BA293DE8D3787FB856489E19930C3E9752C6374A4AA7E6F7562397F4B45FA1E10B06DA48EA4F7AB223CE11CFCAC2D8ADA6421F20B1565411D20A72C36AD7DA04C301D55211F23E4532C2DFBC58EA6C3794100823CB651968A49374C8934EE8C16E499B9E50BA37ADA6418692C9B9CB13E4F2E0A1F12B03C5F6AD87BF1D2A9CA33EBE8EDC454D645D20C3C0C5965B3C55CCB1647688133D7F342C06966799616580BA1B330E4E9D9DA32058865C8CF04F201C8619BAA8705140F5374E3C3430B7B083E4170651E95A7BB98E5B88BA5414E58B08A0EBEF388A470E21F445F30E45D100ED0BFFE0DDD09EC120603B14B3BED6E0AFD0E45C270348FBED0795561C71CF263C0616B480A49EE254455001209AD3FDCEDB81F6279B2E35FE0644F35B20185BA9CDCD82C058B11518BEB28DC16180ACEACCE5FF0A46D34C5D2689DE73B7FE752B6AFDF41DFC82D16DD7A9311D35AAC7D46CA589FAE8CE0348F428A87230FC15565C347E1867BE18687913C92D893E32A6E59D6D2AE13BB9E39B1EB4FD818F8DF361638EE625E5771B5DC58CDF2B691E7D65375C280137AADC51D7567261356D4BD9ACBCFA4F8F3C5F1E4B48B95B37E39E3A5D1EF26E74BFED4D24EB3FAEDEB63669D8E66E001438F874657FA598F3B3B912E8D8B6C6D2567B38507953934917DE0C1CE221C62DBC718F22315397344859306A6D8D7A17B7970A2F31F9DE80E5204CA74DE851442EF99296C5070328FFF4019180A0C453981517CAB0AF72B00BB6647FCCABCAB7ABDD5E70B8530F361D7CCB06B1892E8A9D251D79E30AFCFBCEB9875099F73AEC08B776EBD30BF70D701E7999FCBD8EE2ECEDF2E552BE5E189E7B989D9F191E6D44403BDBAB6B1B4583E1DF8AF1C7354EDF5A424B4AC94721DD5421915744C4C7434AD154CFEBDD6B7579677D437B63087C4FB5A78BCEB6F9436475458FBA0F5D50D487DB529870F7B7A255061C10B7FB01714259D2BE2B41C05F9B83838D94415F5E4CD7C0CC49964C3CC6C020D4565ADA3ECDC62CC449379142C74D44C15F9ACBC0DBC99D41DDB72DD7839D881C5B100B0BDA2E1146E20282D67EAA9A3176C074C0FE51606A10EAA921A26D2C0F807F7CBFE97B37C02C6080F83E0461EA6DD4CC780BEE0613AC000AA9A444D4139888D1DD8596479CCCFC9A9169E906AD2B94852508A995F415A9081F1FF0AD65E1601D64FC03E28DF46366E60686CCC6163007B165CB22983928931F3173E20F87D414686F593A88CACB8A10950E72EF67E2603F6AFC0E8E6D8080C5850A78A595154D18DA9EC6F17FBD7348000030025215A490D0A656E6473747265616D0D656E646F626A0D33352030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E67746820343731362F4C656E67746831203235353934343E3E73747265616D0D0A4889ECD47F6CD4F51DC7F1F7E7AE85B65CF1A0450B87F83DBEF4408A54854965A095F62A58CB0AE2BCB225DCC9556B23B1616E61BF0C89CB480E349B239B6CD9E2B21FCEC52D9FCECDE1D48C18B2912D645197FDB118A7C8E28F4986F4079B82DDEB7B577E48DC3F4BF687C9F3415FF7F97C3FBFBE9FFB7E3F873933ABB39D16B760EBB6C2D083AF5FB5C5DC1BA7CCE20F6EFDDCBDC18ACF5E3D6AEEBDA4997BEB8EA13BB73DBB6AF184B9B7F798D5DE7FE7DD9FBFE3E687BF5BB2F8D1051AFFC381FE42F1574F7FF21F5AF171E59A01354C5F50FDAAEA47950503DBEEDD31B72DEEB5D62273AFBF70F73D5B0B5F7DFA2B5AFFD4295D67B615760C9DFC52ED0BEAAFD1F860687BFF507064F975E6C675FF2ADD57AD6EDFB7DF7D60ED9B5B2E5A35660DD130B3D79E1F7EB95CBEF4F263A797BD3FAD6EFAD45FEBB2D66256A179D52FBEBFC1D5D79D3ABDECD4AABAE9E595CE775BD432F290CD9B9C13B3A4B55ABFBED5F6DAFBA386EAC5F6E372CFEFD57995D265BF8C87B6D73E22A6B6DA33FFAFB5AB9EB4FCFF322F7668F299020000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001F4D71A55A99AB9AD3654C659DCA3536DD2CF677F5387BFF2215B326273C6116AA58AC5C596BF698CA9F9B05AB947566E9F9CAB5CADE6A4B3FE62C9CA32CD2943F296FAA7E2C66E13BCABF62B6E039B3E69F3A6B7E52E501E5BDB865ACCA32BA736686D2E82CD3A4F2522550E6EB3AA352F7CE2C55BD2D669976D5D7286BA759668BDAB6AADEAF7240E51795FB547FA8DA32FB54FF8EEA8F288FAAAEFB66BCF2AC725039A436ED31F3BCF2A2AEB5D7CC71CD3BA93D9D365B18536A9C2D4C287A1C0B672A8DCAC58AF6B730AD2C535FA7CA1B955B94DB94AD6AFBBED9E5CB9DB554C7AC25A5B2D7ACE5563DBE2B95D566577F4BD967EE7895B977869C7BE73EE74EC4CC9D98A3B42A2BCC8DD4C7DCC82C957395B4B2A0C68D5CA37287F265E51BCA379D1BD9A771DF53F9A8AE1F57FD39950795DF297FA872237F4EB89163AA8F2863CA4973A3358AD61EBD4CC928D72B373837DA1577A33D2A6FD1F5A0A2FB8CFE40F1CA13CAD3CA6F953F2A7F55FEA61C8DB9D1B7342F5AF7DFAA9F3637A6F5C7EA9599CE8DAD537A2F7163F7A8FCCC1437B64BEDA5B81BFB9ACA1F293F5186D5F78CCAE7626EEC79E525E5555DBFA6BCA1FC53D1FEC7DE35379E54B4C6F803CAD795BDCAC3CACF945F284F3937AEB5C6B5C7F1C33137FE4AB51B7F53F5E3CA88CE6E95E90459C26A2CA67ABDCDB71E2BDAF68909B5D65B70E66A6274E2B5CABFE857729E44F9F3E6C9ABEAF2AC4AA2713AB55AB1F22BEB998CDEBAD6AC4427C5B64F4627A27D4371D3C64FACEFB9B9FBA6756B6FECEA587343FBF5D7AD5EF5F195D7B6ADB8E663CB975D7DD595AD4BAF58D2B2F8F2450B33CD0BC2F9E9E0B27997CE4DCD99DD74C9C5B31A1B66CE485E34BD3E31ADAEB666EA94EAAA78CCD912D7E49B3A72D9413FBB23EF136167980C7C62FDF19E566F3353E97046B0ACB5EF8AC951BEBAC55B43B76FECCD0D5B7B5B9F9FD272E190F53EDE9C3C91D6E49E5490F555CDFA0B6F2A14FDA28DB97498FC4BEA6C7F9FE6F8391DB9743AE563CDFA5BA72EFDDD54088A3ED9ABF674AAD2B2CE5B6F2ECAFE89236D6AB4B6749F3E37E6FCBC33977D7D1FB6C9A7CC260E5CB0CDF5AE941C4ECCEEE8F4D6386C8923DE6645C38EB799B7557E518B369254ADBC9AB57AD778C2BB06EF66F568CB1FBC4534ED95B60F7906D9E260982DDEA5275ACC9F7BA6C72B4F341D9482D2C6DC8C65AA9637DDED0F6DC80D4FABEB083BFAEBD460E5061BAE9BA69669518396181A7689EB5CB9124B64570EC7ACA65E8F6F66B4DD6C9441DFBE3BAF4AD8A9E7A69E86733DFB270EEC39BFCB34ED4CADA152AB6CC24FE9F0532B9B08EEF2ED056FBB83E125074A7BF627EDF67C4BA218160B9FCEF9784103862DDE9C1DD8E4E776F76E56936EA5E40782E87577963FA2971764078292AEA3B1797D869DD14BFF407B71A03F1F1D13970F3BD557DB91DB953E90F2335566FD8C165FAF61F55F389A8A97B24D7705D165A9B42BF08F68BBE7F5A6A34F1D82266DBD940D75372D961D5C13BD92D6B3AFAD7C1AD715CB2FA77D7721F03B6F1FAC9CBDC29E33E73F5D4AFAC4785A6F47EF4733CB13271F65313F186D79B0107DCDEC6050DADD5FFEAA7BCA5F4DE735C80E76468926EAF4DBAD9ABD39971D08B3E76EA82FAE4ABCF9C2B9E9B49FDD124D2C95B2D1160B45EDBEB265759CDB7FF49B48B538EDA7C3B76F2A17B6A9FC0E74C7F64267DF64D3E480CDD1B4A827DFD9D797AEBC770DF5539B77552F0D8352B4E2D466DFD8924C1F54DF812B96746FCC653B53E56FEF631DB9D5C79A52C754EFEE3DDBEC9A34A6D47A2C557946DDB784DD1B2AA760E0CC477E53E5071C3BFBE63574727C79D5C34DA9C3AA77855DF952A92B0CBA4AF95261FFC4CEDBC3201996861389D250361F947FF94EEDBFD99DF25D7BFA7C323FE056EA2547E7AD6B63B76FD8F0A9E8F574050385CA7F16D787E9B6547A46DF9931BDFFAD7BF277A613AF731FFDCE4AC9B7B5B784FE474AFD87FD7A8B8DE23AC0387E6666D717B0BDEBCBFA3606AF5973F310CC2510AFB412131B5383DBCAC68EB48B55C93193D494262540A025C118B114636815506814F2D054AAA23C953141C1540259844440719B8634A2101582AA927249A069A5366D70CF9C5D3B065A923CF4A1D27F7E9A6FE75C7C766676CED9757889B7BC0CCA55C1748375DE349567F2485CCE8395EA995521E7479B1CDCF4668A9198DAB8AA2D7D83E4D3987E60BC75AF355D2B07A9AAF2E6D0AE415B74C982DBDB1A4F95C3A2CB3C28EC5A4B7E769D5ECBD0684BE811AFA577B465ECCF3B23F2B32A6D6EFB82677AFCF3DC9F1F2908476BD5FD57CBADE30EB5CB6BFC7B9D9B5597FEB80B1BE286A9A78F74D3F08E265872F98AB92596FA43EF9EC855B23F1809BF1D718396EB6F880F99B14438982F97374DF669B2BC592357D1B723A7346FED144541578BB95AB1572FE45AAA9674A3A44E368E3D3CE1C6FECEF4D335FEB2D25F004EF77FBE36D92718919767A6FAE71744BC2B3CA396B4F44A3D75893797CCAA548F650937CF5B8FDDBCEB2AE4F99A0DF1B05C7DE46C6D5507E1C670B7F761BBE1CEC56A194898E3AB07472E752EF6963D79CA5E1733FD58CB4CDDDA3B9FB52FFF84F7CA277CEBEE44B77CBA5DBB465E4178817C5B355BDAE3E9BB5467A66791F75E4BBD4BB9B37DEC2E8EF6B9F7EE36B7DF511A37AEF78550259BEBC6E67E7BDC5D628D0E952A7FCD32C7179BEE6A5E3ADA2CE49D08E72FF56EAAFCFEA933EFA8939FAF9DAA92EBC8667393F77DA28BFA8188D6D73A606B7D6D2BE24782F2D7515F7BFCA0AEE90D9DF589816AD9163F227F30D9AA56F76ABD4AAF10F60AA25993A31DD4B3547FF388FCDDD6AB5A7DAA4295570E6A42D5658DD66962E5A09EAA0B8ED6E9B2CE97AAB3555DEAE745B871D5E207843D12FE24BB545B16BEA5F2A6CA8F55DE507955E5FB2ACFAB3CA7F2ACCA619567549E56794AE549956FA93CA1F2B8CA219547551E5239A0F280CADD2A77A9EC57B9536552E536955B54F6A8DCACB24B658BCA2695795E5EBC545C52F1EEEF643CF36CB1F9CCB365BF7D471E6FD828E3893532BEFB3D19AB9FB46B8ACDD54F6E595BBEFEE9A250C5B7BF23E3F155321EEB2E321FEBDEFE5479D9BAE24D0D65553F90BB9C2C76C9AD9956F44F570CCBBE326B76D4BE625644EDF6CACAA87C2CECE9BF8A44A2F6EF67D644DB96EBD6F2CBBA157627E646FF220FDE7853B7ECF32565D10BB220FBBE7679CE5CF5376597274D8EDAEF959444AF1DD3AD63FB75EB45B9EFDFE797BD2E1DFEA8A838BA778FE11DDB397F2D2C8E5EDFE7B3F6ECABAEF42A26AE2C2D8B3EBE52FBC93E5D75C8DE377546D43E27DFC41B38FBC494A951FBB81CD87BBBA172D37B3D3C142C880E9FF1061C3A7C4A9EEDE993EAD82EBE294FF46A8F6ECDE9CAC9C9681838A05B077A52677A3650A086383A7D861AB862F7E4C9D19DFD3EAB3F39C1DA9534ACAD5B346B738FCFEA49A64EF45C973CAFAEA466F5C97D87DCB7CB7D5BD267FD39F98FA4BE2AA94D4F6AE643A1D285A1D08250C183A1C0FC50CEBC50F6DC50C69C90511B12B343D3A6E7CD981EA8B1F26659812991BCEA486072655EB832B0A6E6528D2E625AB0DAAE5E53FD72B52F10CCCFC99E30312723332BC7F0F97384A6E74CCBA8A8CC304A2B03C622E3A261FC545C147AA0A4B2A4B6C408145516D51619A63629B734B33C37142CC92DF015E5D69ADAAC584D6C466C5AAC3A3625168E4D8E99B1D2582856100BC4B26319312326622DF3DB35B7A05934B7D7BB859A7C6DAB77E75BCD834678B93BCF6A76B35B3AE2F25FB91F2764ADABF7C9A9D9EEFAFAE46C6C97BFFE5674C407B532AF79BB7944689A709B3BB7FF286159935CC7FB7EE89D9470E77907CF4D4AC86FF279ADAE19A9B7EEDED6A990DB6879F46860C6B446B7A6F151775663E7E2F17FA1AD5BFFF43DC38CDB447AD0FFBAB9A5EE2279A577570F647B97DCB2BCBED9CD923F5BB25A3ADCF2882C9C948585B29013A91F1072091DD0BDC890D1D111B79B3E716E39379D8F9D1BCE55E77DE7BC73CE39EB0C3B679CD3CE29E7A4F39673C239EE0C39479D43CE8073C0D9EDEC72FA9D9D4ED2D9E66C717A9CCD4E97D3E2343979CE7D4F39BD25BE4C27E1BF2E8AE53FB2FB45E9688EDF7C225533F2E1C8CECF5388DB3347FEA6FA8BDBBF4CF5D487476EF87F2672F5A6916B4642E4CA7ED7C47DB6DBBF183DCA4AEFBE54B13F5DBD2EFDBA5AEE4F895ED1759FC1DE93BEFAE68A57C59EF4F1ABE2E763F5DEF14BA27BACFC82D89D3EB36DEAF5F9B173FCE2ED92F4B2F8A3764CD3EF69FBA124C49BE20DB1552C132B44ABFF82FF82AC8B8BE7E4DE2FAFF9F3ED372ABDABFCBED828368947E5BE35DDB655DD23A1DA9E50AF7B65DD5EB14BBEF30BDA59F175B15E38F2AABCAD2AAD43ECF85FD08ABE92BEFB7807000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F87F2484D0C52B42F892FE0DC21099A2DC9E98A9193E61F8FDD93E513B5C10AD1D962F8B86E7CE999F5F953FB52ABFEA15E30FFF7A5D7FFDB365FE0D9FF63FEFFBA61CE1D0C847DA5EFF7E3942403C645718863EC1F5156ABAD85C69D41ABA917B604BA616CC0C67EA9983231FBE969BF18DCC5A6BEDB73E7877ED07D1A858F4D9AF17CD9DA31911A370CAB4050F2E9C3FAF385494A16D2CDC51109FFDF0C3B31FA8AFF7EFFFE7A7BE7F734D36215145511C3FE77DDC793ACEF83E7A6F1E632AFA40C10B8EBDAB23D3665E04ADDC1411183C305A052D62A428495BE4262A6A91052E63AC851294455308A1813B3749AB2290821232A240C48F99CE9DD9B538F79EF73F8F7BCEEFDC73D9EEE9A85F0AFD11E57CA07E52EED5732680BF56989A50B5493AFE204ACBF3134D30A91AE4A939BE41147C038A622D6CE4E922C3E31DE31DB8478B3E5B3D86EFA401202C01B0129DDB066F2377228B971DEC71F28E52F2D0F393291C4911C24BE9E8E444C3D2F3B2A837A5986DD936F54E6605A3452E495AA6B252C8DE40449F1C444621ACD4765F5114DB9D4A6D27CA50C049998C3653864D463173CAAAD4FEC86ED1BEBD48B295E37C83735E08B95DE03CC763C137E537148B7686141E13601CC75D8135381CB00406D8D31B78AE155802BD8CC80FA360A5A769CD77AADEA96AC6F534E3F117AC8856C6B3B8FC415D29CD9DEBDB7BA19D18387FE6FD7EA4CFEEFBE3852B47D56FD4F1B1DA0FCDD47F8209ED70271AB8CAF0B09F6C8111A56D46F7D04DBB3349B315ACD4FF0D98463D5527DE596C90FF8EFA243A76FA12D897C0BE04F66F1A2609469A04234B8291EBC49898EB90F2121B9E10C4BB154A3B32C09168870216742B438320C28CB0843A34D8137433F79027C2BC6696B5D2AFE5EF081FD7C7B472B93CF57061EEF6ADF967ABE86C55317CA24CEC7D7D747DE1F3CAF3B5553901F41E9425CD26CED137D04277DC2B2913AED10AD0ACA4CD4BF40FD8CD4C16CF64F14C16CFA669F8FE36864F2331A127EB63DE2839B40AB9188A6B61982B1665D1F5FAE4B5B8224F139F5196F8C90B77CB652DB83F1A75AB67BB56E70FB63558BF782D0DFF04180070C513440D0A656E6473747265616D0D656E646F626A0D33362030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E677468203232333E3E73747265616D0D0A789C5D90C16AC3300C86EF7E0A1DDB43B1DB5D4360740C72583796ED011C5BC90C8B6C14E790B79FEC850E26B041FEFF4FFC96BE764F1D850CFA8DA3EB31C318C8332E71658730E014489D0DF8E0F2DED5DBCD36292D70BF2D19E78EC6A89A06F4BB884BE60D0E8F3E0E7854FA953D72A0090E9FD75EFA7E4DE91B67A40C46B52D781C65D08B4D373B23E88A9D3A2F7AC8DB49983FC7C796102EB53FFF8671D1E392AC43B634A16A8C540BCDB354AB90FC3F7DA786D17D59AEEE07711B7331D5BDBF17AE7CEF1ECAADCC92A7EEA006291102E17D4D29A64295F30302FA6F170D0A656E6473747265616D0D656E646F626A0D33372030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E67746820383739312F4C656E677468312033373833323E3E73747265616D0D0A4889EC577B6C53D719FFEE39C6761E8E6FDE4E9CE0EBDE24A4792C601E4B439A5CC78F5265AC04C264A34AD87142032B60514A814925D2C6A0379441D7A1AE7B40418CAABCAEAFF787930C358C499BD6B5549BA63DA4699D68A7496DD5ED0FF607EBE67DE7D84913CA4685D058B7F34BBEFBBDCFF9CE773EDF38200140218C010525B1359E6CF95CF12440EF9F000849ECDAA964AF9C7F08604303802DB629F9D8D6C945A5DF03F0BF0760ED79ECF13D9B1EAE2F7A0A60BF0C5092191D890F1FFE60CDB3B8E239A415A368287BBA6A04E5B7911A46B7EEDCBD6599B511406A06E8997E7C7B229E3A715E03887F0540D5B7C677276BBE5A86EB4B768C57923B4692F49115170056B501945E0756ABCD7641D747166E74765FB7D7B0308093D7BAEB19FF83E56FDA8D1B1FFE5D06FB0E8C2DE0F10CC86D3DFFF83C0464B871E3C65E19663D7938BECD2CA53FC23EC8DC409077C01730AF1FF74558F329E4813CBD0219FA1A24E1BF000B7E0CCFDF2EC6FA0A1CBD45DEB1BBB1BFE51A0CDE491EAD870377637F010101010101010101819B211DCB4EDEEB1A3E292C7FFCF4D42A202020702F214176D28E244376EA5ED722202020202020202020202020202020202020202020202020F0BF03CB65D874AF6B1010F8B4417AF15E572020202020F0FF8A8216387637D7B3FC055EB85D8CB5F9E33196BFDE3E4F40404040404040E01382E6A90E24AE1F420D25E939B040127517C868B1C37D1084D5B0168661079CB06A4A81D2D1F8B36C1623945B7BB2D7E6FC243E34DE7DE3DD97DF7930BFCB2D215961D62D1102406E0EC0522D0B6E7BA44533C262F658FE31FF43F3B4F5B75DEFCE40EF2CED3FDE75AD6FFDA0D6DBF360F7CAAE073A3FBB7CD952DF92C51D9F696F6B6DB9BF79515363837A9F57F12CACAF73D7D6B8AAAB2A2BCACB4A656789A3B8A8B0C06EB32EB05022415B480DC714A32966589AD455ABDA99AEC6D1109F6388190A9AC2F3630C25C6C394F9911A466EBA2952CB456AB39192AC7443777B9B125215E3F5A0AA64A40D0311949F0DAA51C5789FCBABB97C84CB0E94BD5E4C5042AED1A0624831256484778DEAA15810974B151506D4C048617B1BA40A8B502C42C9A8569329A9BA47E202A90E75A508D81D589451AB0643468D1A641518B431141F36D60C444241B7D71B6D6F33A440421D3240ED339CAD3C04027C1BC31A306C7C1B65333B0D8C2BA9B669FD504686A1586BF1B03A1C7F3462D07894ED51DA8AFB068DEABD6FBB3E5271F1B240E4C05CAF9BEA21D76685A9BA7E4031A6072273BD5EF68C46710DCC258DE1981EC6AD0F6113FBD729B81BD91F8D18D27EDC52612761A7CA9D6F440D314B6C8B6214A87DEAA8BE25865753AB1BB0768FD7ACADD526B26F416D48D10723AAD7E875ABD178B02E5501FADA3DE91A4DA999EF696F4BC9A5B9C6A64A9C79A1D831571899F571898733A97FED6C67255691FA300E84A12414AC24A2E2993AD963A413F444278621A2126619C378239B8D82404C97BB989DE51B0B1A6555D1AF034E80FAFE7BF32DF1BCC5DA285F0726B239991D35F4CFC8466BABD1D2C246C416C03BC51A7BB8BEBCBD6D5786AC5093B2820CDB076BB0B7F1685707B6DFEB65173C9ED1600815636C2092D31518729BA075B4460D12639EE9194FE57AE6199BF1CCA6C7549CE4EFF34F76A5616F9AFD75CA55E5A1D12E43AAFA37EE919CBF7F9DDA3FB021A284F458BEB7FD83F3B49CBF73D697978CF24084BA495E226ECABD38948FCE063325526C581AF1D7CA877AD8A03894DC202961438EADCA3DA3855EEFBFCCC9D8EC739232D93FB32CCE3E4ACB576974B5CED757CED3E75557AC53ACD7D244FA0737E87AE13C5F185F40BA1E5695B01ED3E399ECD890AAC8AA3E41CE90337A32149BB9D04C7672DC6D840F45F110A352170E2B81BE942A1D1C4869D2C1751B221332BEA60F0E464C229140AC2F9A6A405F6442C1772EB712666546A6284C817E09E7DC24761EEF9ED000C6B8D7C20D5C4F6424E036FB8C4D824486E46C726EA326BE91867F4013194BCEA3CD445BD066CFD9C672D1CDF9683B7A64E699047CA70377E6C05E1A81C1C8DC71E09FB1683B80BF1806E945F64396433D78E8057A1EBA919F4F5BEB3D637E073D07179108C8F854904E2051D0E8B9B4CDE1D332C8CB2A3837AB5A7D13D96914BA96727BFBF3BEB11FD0B3B01196A2F9ACB99E99CFA6B5A08FF3A52B73BC6309E7A63DE7B655F83CFE5A4CEB4022E0CC4B8F207D0DE938D2AB48562CE82CFC1E298B44E9CBF4A419F6E00AA77121A7BF829EC6CE6AF8BC8A9445A258FD693CCB69F8206FB16055A7D205C56CFB533CCB4D4F6196139F32D218D245A4AB480B603B3E8F236591284A27D17712083D495F32658FEC2FA4DF857D4884BE084E49020FAEFE425AE6BDF966DA59EED3FC32FD06AC412260D0D5308D4470D9A39876140886F79BED4B780BFBD385253E19E3C7B1E8712C641CB73C814F89EB1A128B1F4F9757B1E5BF6C3A4B79DE97CCC5CB72425A76F9D6601776834447E83650F14A9F46BE10790239BBEA213A0C0E5EA79676CABE31DCAF17C37B6925DC8F6E3FAD021FF220AD05370F7BD22CC9EDF3A4D9DCE2C31307A88B8738A9039621B7539BE9F3285354E3CD3F982E2862F51D34E54ADF25BA9FDAA002A3C630AADAE3BC440BF1660BF94906D3050EDF117F311DC4630E625B3C58A3845DDEC617DA66E242FE521AA2755085BE2FD27AA8441EA60B393F435F8230F2EFA49BEA3CD353F4EB3CEB39B6286EDF931BAD9EB4A3C437ED2FA03DE835E861BC80C37CF323E9A64E1FF89B68332C4622D8E37D28EDE343AFA3A4E3ADE978533ADE948E45E9387D409F41CF3318D341F742923E0547908EA3CCC6AAD2C4864E70A1A1D937416BA80B1B234F612B25B4D6A60B4A58652EB3AC9C87B9D2C525BEDE4BF4099CF327704D8DEE4C57BB7CDBA7680B3F4A5BDAE566094913C7F512ADCE5D0D2656B12BB944EBB011AC31F574A159E931FC1ED4D9207BF0ABF84FC99BAC49E417E497ECBAC955D4197F2DCF5FCFF337723C3B4DDECC7D28C8CF197FCB5F47DEC1C53692DFC1719408992257F0CBB987FC96645815E43764027A91FF1AF561E413C897229F34BD3FF16448268D0C6BFF96E9A862872557CCD68EBCE069CC0BD5EEBC5056E5F337921F92CBF83F8D87FC0A7903F2CB641ABF4D7BC8ABC8FF497AF9C636759D61FC3DC78EAF13087142940642766EE2DA86780607911A0423D7A9D38E5A1B8600B217D00C2852D1A4D5526CA2151AFE4868A32D90AE6BB5ADD270BB3545409BEBEB111CC84626D6AA9A34914F5DB60FAB3F649AD8A8E0C3FEA069E5EE39C70E302D5FAA5DF39CE7E49CF777DEE3730FF7FAB4C26778963E865FC1536B33FCE755FF359F965B9C5FE593B4115EB496C9299896266DC27249FBC0A2CA5F8975629A7FC02FD14A84BE6FF957A2F542D1FFA468B88EF1187F9767AD76D114ADE36FB324FB1B82F234279D9AF83B56440E32664DEB628A8FF131A33562F88C9031EE08FBC2A1F0B843F7E9213DA28FEB510F3F8B07C8798EFFBFFC159411D239760F644063FCB4E58C98D1CFF19DE4F7E2741C655ED5D22833AA46283D0F7BEFA95A2F3F45DB218E3146A163D071E8045E1363FC45E80874147A49B564A11C3482A749064406440644461119101910191019456454F61C2489348834883488B422D220D220D220D28A90F34D83482B22012201220122A188048804880488842212201220128A304018200C1086220C1006080384A108038401C2504418441844184458116110611061106145844184418415A183D041E8207445E82074103A085D113A081D84AE080F080F080F088F223C203C203C203C8AF0A8FB938324510651065106515644194419441944591165106510653E5270CC463F04320B6416C8AC426681CC02990532AB905920B34066AB5F3DAB168363DB8C42C7A0E3906467C0CE809D013BA3D819B5BD7290644D1026081384A908138409C204612AC204618230159107910791079157441E441E441E445E1179B571739024BEF8A6FCC2B7869F604937DEB5FC385BA3FC18DD513E4A73CA5FA282F2A334AEFC089D54FE2245948F905F39C6539E25E166968834445BF008D80E7D137A013A0FC91F4937204DD56E419F4236EF313A9D0DDA76EDBC36A1DDD06A26B4B2C61B5CDB5DE75D13AE1BAE9A0957D9C5F5681BAF57CF513C5AE89C2A8FA1BC0BE12582B257D57AF906E4DD80E76C0F3E1BF806A3F133FD6E17BBD5C56E74B1892E76AE8B456BF9B3CCA99E743A45F0734FB0A4B1D4BF55CC41117F602B9E4C6727EF3C212CFF53A2C4A62BB6C608C2EF4005681C3A0945A0F55008F24142B575213E697456879C86025007A4CB14D4D282A34953A3DB98E2F56CBCF8613DD5CA3C81D5E0AE5B8130AC6405B6C3AE5A8103225ACB2629207F15B12BB87397E013969847F7FB15BB6C89EBB00B96D800DB6705D6C206ADC06F45B49EED26E194E8AEAA0FE07B4BDF69893D08DB618935B0A015F0CBE82E24F2A1770D4BD23CDC57A59EAC64F25A6233ACD3129B64B49B02F2C6331785D4F46A20E98E222674778A259DCC58223E13AF8B3BC0FF8A85C5F6F8BD5E72C26EF94A6C8F5127A6433F41705458D13A198FF743A1EAA6F42B62DC775ABC85B1986F52FC48AC15674325379ACF60DEA7550A4B9CC471F392B15C1C1761910DCD8B61F19CD82F768A7D3EB45B62AF9896D3A4144BF24B93228101B7E15BF82CF1ACAFA4A6F88CF88E3044406CD2A7E5FAD2C6CAB891D0B45C015A5FC9FE65AC6F97AF24F7F8EE4889351A5DDA3D6D4C1BD4FAB4CD9A57EBD4BEA4B56BCDEE26B7C7BDCCBDD45DE776BB5D6EA79BBBC9DD5CB2CB4690B06D9B5D1E692EA72C9DAAEEE1B24421CFAC9CB9393D47E672479CC707FA58DC9C3948F103BAF98F016F89D5E13457E3ED6366539CE2BBFACC8DC17849B3779A9160DCD41283C9026367536835F9F77058DA952C315B369D6AC3390F9D74EA4CDB1431B6E2D499548A5A5B0EF7B6F6366D6DDCF44C6C91225D2D838FAED6C7ABEDE69BF181A479B13D65AE9715BB3D15374F0CE87B9353BC81D7F7C7A6F83269A9E49433C31BFA77CA7667269642D8BC0AC36E5E86300A484398BB8F741986E7499F0CC33DAAC4F98123AE431AE2EAEAC9AFE2FC75F52ACEC9645C614EEF8F15745DC5F888E654CC9C8F1E8BC18E011B2BF8FD2ACAABB3A48C6249AFAE26B6460D24044242428530FCAE530309A69299EB1E85F8AA213D0F437A542E077B14232A31CDAB17629A572326F87F5E437D4156ECCE8DDEEC1FF2F6A7BDFD4350DA7CE5F0F3ADE6F103BA5E18CDC90EDD74F8D3070E3E2F7DFF9099F30EC5CC516F4C2F74DF5CA4FBA6ECEEF6C60A74B37F57B270D3188A59DD4677BF777F2C55ECDD928CFE57AED30F7325B72C32D816395852E6EA8D2ED21D95DDBD325754E68ACA5CBD46AFCAD57F48EEFB44B2E0A6BED4D37B2B5EE44BEAB087D36D1DA9BE164F66ABDCD0539B3B5A47DBAE39895DA025C194B9D4DB67D643B22B140D456517FE9FC9AE65686EA876B58E6EEE68BBC62E54BB3C686EF4F6D1C2D2920C8A9B3D3BE266C7C0379272AB98C6FEC5EFD9B0BC54772BF51F8AE11FFECE2AE1F378240D2F7A6517BB72B9DCB02C72C161A2B8D93510379FDA8199681A52A56329B4AD5D687338545BA1B6B6BF64CFA0338849B0AC4C276B4116C40A1A753875693CEFCA6B5C1E15B2C595EDEB5FF805DEE0C7209CE3F888B5AE5B9D22468A9D3E797EC916D7F5541CC755E9D6CA8EF5C8508C0095EEABB8D1184265CC37161A8BE47DF9503EE242EBE4381AC5B87C955AEBC61D940D0E2F2C04AAD914161BD392F9DEB656B5ABC47959090653C161A6D6EB7F179B2D2CFAC3851DAE8E3AAC86CF2EDC904AFB707510DC894AF6DC0296AB42AA33A7A0CA2095BF1E168FAE6C4E0E25D7134FE99A6BB44AE93D5AE5F4E3AC45F6FC821E1CB2E7659F74FE173CD1DB2BAA5E165DA6DFB1D54CA722FB173D41F7D90AD64DDBB04BFF899F7013F439BD41CDB48BDE644D38BBB5D06EDAC69C8809D2ABEC2DFBB07D9BBE42DFA777ECABECA47D11FDE7E823BA8F19FC116FCC087D1DF1BB69886E3BFE4429FBC7E4A6EFD2129CED76B216DA4F9FE0F377CCE175FA01FD921DB5EF236B339DC4785B284A51FB57F6BFA98B5E758ED5CCD55EA1D7E83A73D907ED43F8A5D4492FF3A0FD89FD29F929453FA5CB985390CD38BF4A1DF42D3A453F642B1C1FA1F606FD8C1EB0A57C9FE3E99A1BC8B48DF6D0B769845EA68BF41BD6C412357335F7EC23F69FB11B97D36ACCE910DD663DEC6BFC5DE7527BABFD071AA429FA18DF577E669C83CEF76A061FF4DAFF61BFDC83A2BAAF387ECEF9FD7EF7EEBD8BB8803CD64758B4AE8E18050411DDC856A98F2AE8848E1A2B890F30444D14356A8C111513DAD1546D185BABF155313E6A8A05750735513B766AE223566335D596D1F88891A955A3A9B2F7F62C31339D6692B1CD74267FDCFB197ECB5EF8FDCEF73CEFEE9BF62188873D68E23E3CA832D4CFC20BED0DF6DB10C57AD23922056C673C54C0413802FF805B546E97C3202864CB87B11DFAD0CF113F435E9A4FF3C529E8C6DE16B1DA17611DD47046EA612FECE7D8FC051AE032B6C236F8431C8F2BF0164551319D10AB459D382D516EE57877808E1CA399500DBBE1281C8313A8F8FC341C8E93702AFE02DFC406AAA11B744FBA64857C20C3CA6F35580FEC02FB334882D63014E64239C7F6D7500B75701C3E845B701BEEA2077B61296EC01A6CC01B64507B1A46D3682555D30E5120568883324BF69393E531F9917A4D2DD1C7E956D366EB0D6B8775D2DE639FE4DA89E6F3FD308023BA90ABA21ADE85537CFA39B8001723F5C3E7F7C1D1F8345B99813FC12ADC8187F1245E672FA199F6D487F2D8EA549ACE715A446F50155B3FC17C401FD105FA943E134AB4173D4599D8206A44487C20AE488FF4CB6E325D0E93A3A5CD99C9500355A1DAA2B6AB43EAA616D08AB569DA357D91BED87534DC25FC570BAC52ABC6AAE5DA757125CDE548AC858D5CF7759C83F738A2C7597103DCE12CB4C614ECC4BA7370000EC17C1C8963B004176125FE1C7F89AB7123BECD1EB00FA4B3F654FA3E15D2382AA1C55449AF531D534F47E80C9DA546569E283A8854912E068BD1E2C7E205F661A6982F16736457886DE2843825AE8A6BA291B396281F932FCAB972957C4BD6C9936AA87A9ED9A8DE5507D449D5A49A34D25A6B6DB5EEDA246D8B7651D7F49EFA70FDA7FA69FDB66B1AB6C52EACDC07FF7691977BF031DA46AD643936F28D76FCEDA3257B9ECA7928E4AEB80DB9C2E2BC4447FECEDAE2C92BE3223BB5A0ACE1FD33712F64E16128D748F00744D900BFC3F3D4207F4F4FC0873816BDF22DF1827A8F52603B4FA3E5B48FF6623FA8A3008DA03502F0323F1D2F73BDCF812A9C8C33603B36626F7C05B3B11C4E538228C4C510B03792440307E34D6005B05016C3D3F08D17E6C079F8C45A2B5BC8793C9F42B09233FA1BF81B6E85FBA8EC1B3CDD044FA3713C659672BDBF0A91A957C47D56CEFDE8E50932453B0175FCDD01F46CADAF9C0B37E19FF089AAE78AEAC793F4AAF59C5C2B2FD9D9F6E3DC61DC65B085FBAE140672C75CE62AD9CFEF23EFC670A79B3C4B32B8AB87C36828865778EAADB06BEC357685FD923D15DEE7BDF7B12BDEC7F5DC1121DE11803F32CBE01C2EE13E1CF8CD7E7EDD6515C301B88E49D81133B81F1AD52CB55C6D5375EA1D754C4BE7682F86D55CD117B99A4DF660029C84EB700F5D9C1B2F74854CD6DB8BB58F8229F494D80FFDB1354CE39EEDCC73BCDF434F66F0298B387A6BB89FF7736FDCE4393106DE81B34898C81E4D60FB2E3E6708C7F919FEEFCD9CC10AACE53BC53CB5BBC0A7EC7734F6A2996C2FC827ADE4A97580359D872B1C6DBB5957579E0B793882CFBA0723A1982DF484E1B89333B01B7278B2E689A31CEFEFA107FA617BDCC4FBC6728746433BC8519790A0AB5560F7A2E7C47E7EC6D87C7F3D3FBDDAC01358C62A5AB21F6188C76190653DC91A4EA19035F8A76615ABA8C4AE14B3AD29F03E6CE59C04E52C3D4F4E97AFCA071CDC9E0F799DE7D7B7069FFC4F68DE97886E8FC0E6AF47263938383838FC1764FC5F18EAE0F01D64A25C207FE5E0E0E0E0E0E0E0E0E0E0E0E0F028A86A87EF2C3B1C1C1CFE77B4A447E4F9AF70E14BF4310E0E0E0E0E0E0E0EDF1E0020E445312040877E758496A687283718074A5A024C5D5A085E97A62C12FBD00F06D6601224A57AEE06C28102CF9D407E3800B9FCBBA78997F4B4949894988EBC204868F289034D41050FC0270FB02D0801E03555CFE60C78B99E7A809B3282A9A60A7A93335BAA64456AB4AB9726080CCD5CE646B737B1B530FC9ACBAF4B3F0A3F69F554053A5505A3C88769B80C057A4D77085DB52957B627A5A616DC290AE47FFCB1A7F10B0A3C3F28C9BB52C4F27203F99EF095A2D4F4341C9037200F05AB1491057130A60D3A8F5E9C4BD77094B5259C64BD865EEB2AAB9D26768A9266B56E981CCCAC5495EEBBEAAE5B6A4A7397A812F72C35CBAD8112A8B94D97AED863E1BEE3720970F93C667733D71466085F0E9AC2978C11B90243B4B236AABA7F247A458DE1A2302BF334C624E6604C6C4E4EE487F54D2F8B135929F1A247F35A9D8519DDEE4416B113633EFFDCFAFB176B249A559CB971ACCF03C9501EECD1597536072696C89228D52531277150C25309A5092A27B1679BCA36ABD44AB74A8EE9884071B11D5B7A5CDE4EBFD5510FD9076A0D7726277C69306E410AFA52D252282526D6073E4F9A873C215A52EB4B2F4C4AE5441745329DEF292ABB9B5A96DFD89CF2DCE68C43511916C5A564242624C4C6B7D2B5081D5230A64746765FCACAF4FB3BF93B5451BB3D631786C63E9E3D31BF62FCA6F029EC7C615EF6A067028129857D77A9FAB6FE43D6D5E3BB2AD64F18D225591E6ACA8A8E1D7178DBB6DD1363A3B964600580E6654FA32829E8760BBFCBEF1652A008D90B8246DBDE99A6AF779F4C236437D43E7C0D6E6ADB8DEFF2A2192EF39271C394D230CD386A2B3D46B2D981BA4A9FD1DD7C964A658931C99C4D73E426639BB9CBA837EF1AF7CD847572B9B1CEFC8371C4FC339D95678C73E655BA262F1BD7CD16B38D3966052D9515C6527339E9A3DC2534493E6B949AB3E825A9E7D11099670C3147BA461AA34C3DC9EC1E9D49BD65A6D1C7CC8DD6054549CD30CC786A2D130D7DA746FD7F342A984C5298868AD2F50C2D3A2A835BD123C835DCD522D31D599ABD8C76FF8BF1AA8F6DEABAE2F7DEF761BFE7E7E7E7E7AF3831C973ECD809496CC776EC044C622025217CB5210B718221509A42582189F8EE68286B1B2A4D8351C687845AA6AEEA046283B61A2180B455F9A3DA9409AD74651AEBA6B6A34893374D6203017ED9B94E1855F647FB9EDFBDE7DE7BEE7BF7DC737EBF7B6C8E1BD372306EA205749D4E2B54301919883B4C0C2232824FC02556D5D5585DB8B2389C536EE46847C9D8D4FC742D7C45638D821065583BC3B0C4248A51868048E0358CC4122289A220188C653296C7B0F97D03CFB1E3A401711027BDD93807F19276ADEE8C735143DA3062C4C66B23E0856B26CD249131D29056814DD2A088D2A084A2651296E86BCC753B21E4EF0EE5AAAB95D43F9454B15BC90FE58752C5454ABEBA1A3A942F872814E06E4EC16A47B950F5E8FE89D15011ADAA33808A65176CABBB2F23E3D45F2F9AB486860CCE16AEA1616A28AA1ECAC6807230251F0CA1F7237C058BD880AFEA39FDCFFA17FA67DCF8A322E6CE8325ECC1872FD107D0731CD0731562CA81BCE87EFA60A365A9658D61C034209D15DE95CFF87E29DF1444DEC88B2EA3534CC84BE42516835111AC76D96EB12B09396169B5EC94F72A1F8BA63DC21EF7AE39878443EED7E6F082D32E481679B5BC537E453E26BF2D73B26696EC66B364911C6697B3C2A6D8719FFD8C9DD8ED48F356104CCCB2EC4046D8F0ABE920322B6662BE51123CC35FE07FC55FE7597E74D087355FC4477C5E87863514A1440AC02CAF7BF60930292EB3B9BB590ACCC236E6536A63383B0475815C4643D55979BF3281AD8D08DA14B578284B772DEA743AECBCC1E974D9BC4C88F87C56406E2201C005DCFA8E93ED7FFFE4C087BFEEDB3FF0BEFEE61F863BD7F5A7FEF4C9406A559BFF83AFB8F155BF39F8CEA79E86D7CEE99FE3E673196FFE34B3D2DFBDA8BD57E22843754EDD6129435990071D49D7AA193E2366D42E675751C673D2704AB82F0883A5074AC93C262ECD73C4DDED4C8BD4EE68719F1204FB18D9F11E672A860A82DF205B80064457956C0EE0315C95B65850F1E1525CAA788DEE39DDA9C246AC00C21FFA4F6A452E9FBA5DE0FCE65C736EC6D2C5DD69F3167E8BB845ED77F6176DF1F0D98CD75BCFF3BE726455D458D405E1637739C17230DC57CEB31BF4870B2FF65CD21FEA1FBE7710BBF36AB865DF8643AF3CBF69F4746F0607B111CBD87D8C288F06CF2EDFF6CE4F2FFDE42D58E0286037C9360123FF2C5D7982C3828C5773FDDC4E8E09ABDDF26679506545C1229549E4B034259166699544A431B23B5D653080790CE1C54A24284244181458A178447D4B25EBD511F517EA7595551504872035DE44C8017C0613ECB6365FC61E54B07E08C839A7D0339986827BC597A8687A0F00248D51805016881A2DBBE05ABDEC42FD333DDD17C568430665BD80194722013B60A076F3567C46FF0A738BB7B6F465D6B42E98DF11660327B6B6D4FF3BB4F0ACFE2F84A7C671053E4EEA215F28BA8618661B986C8067F82287C3CA5D44779CE2101FD7FDF816E82AD373D8DBDF3C87BDFDE00657F3640E9C56DFFC1DA48FE3254FE618BFC51C23BA376E7C3CE70AAE50BEC51C05FDF38AF254618E82FA510FDBCBAE848CC9825C70FA0651182551336A45ABD01AB41E3D8FB6A3DD68047D947E76F3779FEEEC5CDBBDE77B0DA9C11D95357D9BFCCBDB24634B9A05BE36228FE64FD5F8FD3529A6DB138FD815A5C8B3B27DD7F0F0C6FE258B5EDA97886E1B509D1D5D849FD7D40577F9BA9ED2E29E7D033D3D03FB98FE72519E1B0A05CAFB51F8B3C9C6F0E4F549CAF4E17058B93EA94C5A1B415426A9F8F5A7A087C3D3B5F2BB69FD59CAFFA70F3B60F795D7C763D1E04C6D9BA95D33F5E371C3ACF6EC7AF6F8EC76C5ACF73FFE1E7323128F478ED1E25EAC2E56E7A7929E8CC2753E565717231DB4CC17D30EF2FDFFE9E67F1E8947A30565FC111DD3D7D2F21E553E4625E638141168E99FC662757F81063E0142177DD98B50E06BD1707DBE0DA41F472271A2CD28E90610EED0697F8C47E221106896F2B5AB8EB6E18431BEC97D8CE26871DA1F1070495CC341AD0A2B8AA4C91AC6385C5BA2695643AD93B1902A3386037C62A2B939178B8563391CCEDECA45C3CA2DC863A394C400A871A0E7729938AC14B34D4C2C5A4A5C858AA55D8F47B9907E73EED20DC9CCBE764DBF89D5D2A737BFBC22B26DEBC60A5F66F807DF99BF777B5F6B957EB37AE9A6C68E5DCBFDCC4BCFBDBA524B6D7DA3FB111C87BF4FAC59E075D4AF6D4D743779ED751D2F9E1B7CD433AD52BFFED56780D34F22C4BEC10E2315F95132ED12BC9A27E2497B8E78588F27600B686A44256A81BD55D1364E760096CE23CAC460518E9E490028C8169DAE52869E3B32339D2B369124506F88040301DF493CD1B623D831D255DBBBBCA46DFDEE763D8CB5A34BB38D9AE474D52E9CEBEA7D8A1D164DAE8D872FBF70E2565BF1DC322BF37ABECFEA4BF8178F74EF6C0F081C07FB0F2BE58EC04ADDE8607A95D3917090461617B3586291DB61E62D2643602F8F07789CE297F3A496C71E1EF3A258B2CB869FB3E1986DB18D94DBB00D7EB225A059235662753B5893CC29BC6D8C39FF8159121491FA6CA279029047ED8B460B690998192B08B9180C400D086A6292369A9FF830DCD483B652C6C514B10651867F3047F507EC14FA429F62F507F8E8B8288B460EAF2FA98D242B48F3C31F32F58F7E4B1F76387FCD9F0CD7BAD18C2772609F06ECF3423AEC72380276D56E5715B5CC6E17BD38680D926010D585CB0285AC01232BCC72A9F632C562F182872E959585EC0E4D0C5137E169374D4CC0E273316B8C660D6052363F612D782D1AA6694435E41075119A3640ECD1CC41260647217700C705430CA40FB6682239933F4002E13849D80DAF6F58E42929712CD8DE93DC121BD7CFBE1DEB28964BB460B1ED486B57714D55AC5AE95C788A1DB694272B2BD7C56AFBD62C75B18E4D2BF4BFDD5F99518C0247F446F22EC31A9A2A6B1A78423EFFAF000300BB519FBF0D0A656E6473747265616D0D656E646F626A0D312030206F626A0D3C3C2F436F6E74656E74732032203020522F43726F70426F785B30203020363132203739325D2F47726F75703C3C2F43532F4465766963655247422F532F5472616E73706172656E63792F547970652F47726F75703E3E2F4D65646961426F785B30203020363132203739325D2F506172656E74203132203020522F5265736F75726365733C3C2F4578744753746174653C3C2F475330203635203020523E3E2F466F6E743C3C2F43325F30203737203020522F43325F31203731203020522F43325F3220313031203020522F43325F33203839203020522F43325F34203935203020522F54543020313037203020522F545431203130203020522F54543220313034203020522F54543320313133203020523E3E2F50726F635365745B2F5044462F546578745D3E3E2F526F7461746520302F546162732F532F547970652F506167653E3E0D656E646F626A0D322030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F4C656E67746820323837393E3E73747265616D0D0A4889BC574D8F23B711BDEB57F4D136202E8BDF048C06248D14C0488018AB20874510181B7B812059C4CE217F3F55248B4DF6F4CCA8352D1F0623B5C82A76D57BAF1E3F7CFCCF4F5F87EFBFFFF0C79FBE7E19BEF9F9EBFE2F1FBF1DC7E3D369D8FDBA93831C1CA8C14735FCF6F3EEAFDF0D5F77C7EBEEC31F3ECAE1CB7F771FAE5739E0CFD75F76DA0AEF06AFBD5061B8FE63F7CDF0EDF59FBBF375F7E32B614EEAEFBC5F0A6970E3E7C17A2F7C68227D2FA5D4F437BE1DB0398F13CA0C5E5901EE5DE7F120B46C23AD3B0F9478DE092F170F74FE1396FAC7DD877B1A81A7B471904261C5A4D061F8ED4B7A0BCC6A4574A91046586C8C34F433E6FD84E7D75E4A15C6BD1652C2800F94C3BFE368F0BBA1EF1AF0FB899E8FFB501719FCBB8C568A88A14B1934ED3CD3EA71AFF0079B565ED2CA293EAD2A45FBDBF58737AAB6F852D7AB9ADE4949298C6D5E6A936252E1D4104B0EABA86E2E60F91886C18F1B64B95EF594C421CC9A24B7A0F4959E1B8A46609318AF8D9B9AAE537F35F5C3DA111C762B35DBAADCBA485FCCA15D85DFCDB89F1EC8B1FDAC1908E6D43C8714648A487D473000C1C6A7D5A6F9D5C218792B6EAB216D982D6C0F66F047EBEE8552CF8F3DAED1C613D595C29EEBBEE7F07423D531AA2E5C571A84F7B3F2139554AA0771E152EAF294DE32172EA6CFA925FC3BF66C9F7F44325A64AD39A67E05AEF45B81B42B018CCB0F524F2998CE016B265C987764961B9D7B7743915BD9C4CD026C5FC295CA6D24F61C9648B139B99DFB1DC85D936C4CEE1A37A1CBCEE9822833E742151843A5AA4F0D5132F5DA8FB12355DA75EC5949F240429EB75F1AA6AB86918776D1695BAAAACA5397688A0071D077EF2E9ACA28AC9F17D2E46985CC833C0B01CA3C0C995E58340555D898D556156E85D1D7D948FF629EAE95A82113D7FAB23191D137344FAB223FC87DF559714B390F85CA9CF2CC67E1676329FDB8B79D9AB01824F2B31024BD2E116C89A09A33D5DD326F208C608AE9082EA5AAC74DF293D68DB61C23AF752BF4847BC460B720946A7B8427B0ADAE116ABB5272DA2A7C3E0F15DCB80F65C1FB51091A9D886BCE77ABDE359A095ADF1763D24C20BBAB9A108FD34CF293E6D19A59936CAC99356E8251BCC51065B2B290BE2EA0A3EFE4778590763EA7F555CD0F5949EBAE2279EBF8A4B024DACF0AA12D6B1173C63CE70EC9A079620169148D76EBB77627F538A68279550D0DBA9BD8EE7CDBAD9C1A253D964D3E47A74B0784E6779703DA2E32B899A2B169E216B3AE908219EA53591BD21DC7DEE78A0824AAE7CE5A5714119E4BC4D89CE0E0CBA5EA9104AF493626788D9B08EE1B1F621AF2A566A7074C34DB3824D330D33D2760DD74170141C724F6DD415F1BADF4DFC4894515C933A3DEB2274CEC49D3B878120AFD6CA41311E7E6BF5F84EAC4DBF76E89E0C491F74E53BE8E691505408FC2B50355E34095FE8E1813DDB4C603D9259C6ECD371BF1FBA3F93625D9966F53DCECCB26E6A07FAE4451ED18EB2895101847AC752CAE79843A2F69B8D618FD4C6E6F1E97295117FB3E7ADA2020CCDEABCE4732EF9A1D3FCD1D1347C8540B3C2A469BDC65BA47D0143ACDA665F5E08744A44CA843F9ABC49D1355F32E560572B3B2B096F9886663230E8288D0237335079515C1DF11A3E1A00AC2E925EC6ECEC1A0847F340539C7C60CE4B009A82E4FA9344702DBA7669C116A908CF5C19C6F205B8E75849B66E5E585F9F89CCC09B8F57BB9CB36533701B931CD9DDD75BDA39E717B1BA42BAD44B05DFF6F057A75D478F50AD0F741B1607013C8BF66F617A938F182623B9B619B05451F46C89F9292B8ACA8E6C5595F1D736D3BB7BBB8E10367CE5718DCA7EBEC3EB1BA40A3416E9B121B2CB1BDAFC4D5FB62892DAC0FD1B867745DCA2F70707325715618F56829A94936D6921AF7997B2654C54638B44AD7B7D80A41B5D74508F0ACB20256B949085041F62FE84CC8E8DC8ADFE0719CF74D59CB6F40CC4837AF8D19A32A34ED6FAEF5F6E98B757765C49F5F1CEFE33EBD3B3194595BA3D46725BC3A9622D91B8D4D3BD6019252DD538D3A93319AF777C468C63A0461F512883727A309423E9C8C35C9C664AC713F55BE1522DAD925D51709AFD75B9888690EB3098D48055947F28BCEBC9BCB2199CDE585F7D9EC880E53CFDF11B99467D9A98CA2C221B2CA9D874E0428B30ECA9C6B86A83AA702D967F36DE2DA65CA92A21C57BC4405B291C9E2B6285BCB05A3D3647B8EA1F772813320E00C9AE7CF38FB8C00E48496C24C5374F5896D88895A35CC2627BEE55262525E6905D82AC3AEDCC9ECA869DAD47B992A7480D4EC98AF613882F68E79810AEEEDE2725D0652FE15B2F9CA232DDFEF2CBBB23EE03159649F2FBC74AE364CCC0F424D827737600AA5578064C09B63B8DA9F3B47E1F5AA9AF2A10DC3864DF5DB50750B32CC6042101E36175C469E504D865BD4B62582C731AEDA2366C99178E5192160C5E124472004E1932C15D97E3B0947F4F3D4477017FA43016535C9027C96100FD9B8D366C05E823B8DBE5B05214704388F7D3E38E7C8FEA9AC08B17CF0768CBCC2BBE4D829936D036B8F72072AD90C0C81BB4FF9336E4A8B32025F78D3145FD348E95E074F904F7292609F52F25C015A482B13D500F22A3CB8E7ECF1C0E148B8F112C3E7972AEFC2EA742FD2272FBA0D279F0E42AE339FFF8C2F7A594189D6FD23E87D5CC2CE96D8F720D0D53D12FB35C3BDD8AF013E65DB8055CE03917B01D8B182BCD2F41EC3B407E105156B680B42C6C90891E1EF6DFEB34D0C5A99B030E1D69D9BC80E1DB56FA0BE4F29503619D21971C4C64295B4E0043D19566343192F5070176ABB2536ACA1EBE723B15133DC8A0D33C3460DF029113BC994C93D180B4472995D0B043F01213D50BEC80ACB57EE237ED045FA7481CBDEDCDEAEE6B8E080AE0D6D45B537B75D62A6187E1EE3E68B508D81361DC5B42B9B3EA3489D4F5929F14B1245FABFCE5756CB06D10863E7E75C6DD9204611EC12441E65320D9A0DF97E933985F97D4DA6914E38DD994C4326333B35F271934BE3D9C5D60FDDE3640369C75319C0D9FEF12F6EC978C69C6043D7A7E84D62F3429BB75EE36875031E5DA0FE978AA1960716867CE77265EE839E664B1A3A7E32195965E4A50C20F25CA68C26B08D6FCA11350F1B94A7C98EF1E331DE272E2AD23DAC7F1B36803A1BC03C77CAFF29693581B215C06C04CB329B5DE0A482272E083FC0A8FB5AA16A021BD15C27240AF98309A6D759A173530CC492EF4B42CDCAEF8983DABE58FBF284264319D9A1EB3075CF5DF2D2504D47B517681814544B51FD272E399D0A224A328897622DAB117D7EAECBE42B5342B98C368ACF0E1F805D513132A675C051B14321CB026CA96B247C11F6EE6871E7D04DFB647367A012D17F3A1243AC9C623140A98A69CED916FDD0BC0F5B2F7A3A5FF794AD1A81CD7258422D4199BDDF3AD46927D04FE05026E52980611F67CA7024DF06351F562ACC2F2C89D7C0A0A0E250617DE976B51DD55EC6E65EC2AEB0AF425B4F5A6A9635E9D2800D535AFE714A5FF08CDE992DCC9E591C0AF84AD643457A8642FD85AF408BC05245F594653F4C0DA160BA0FFFD49CBF2513AB46B9EC957B63A327A9E45CCB7D8B636D4373BF5B6BA6B5B7E4A2A7DE3F1B377FC65953C68B14DE0EFF1B60F8614898B183D7410418AC8E243920AC0C94E863DEDDEEFD7507031D0786B2D3E01474C3E77F63548726E74B3A2C7DF30A3FFC2B7FD84BBAFFE197E613FDF8CB7778B4D77398F8620E3A2E8D610C543EDE9BC77A27F0951FFE32A56052BF9903D061E41CF5D3CA17E124216E9C63824C0C6FA578673FB6CF61B4B016810E549F09EFF036DE79A712F65108C1143EBE946243B4074D9EF5D1AF52AA85A16F00BBAB4074F7BC07277915ECF7E498F0A21E84756EFA528607F47CEB17C905A20BA3730DA1D4AD843278F383C712EA85149B17F7D1AF92AAA545787B42BD8F506D9207118AF0F2A8E1C14D5FCAF0809E6FF122FF1760005DAFD15D0D0A656E6473747265616D0D656E646F626A0D332030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F46697273742031302F4C656E677468203138392F4E20322F547970652F4F626A53746D3E3E73747265616D0D0A68DE2C8E510B82301485FFCA7D0FB973A22588E02CA987222A2A101F968D35582A36C2FE7D73FA78EF770EE75B018118280D204930FBD4A231B00C09665F79534FF382308830E7DD5628F99AD05A4C318FDAA3D05C7E20A058B48D61AC1D4A2FF463C7C0A75130362A070BA505B5493B78728F037F0B64F9FAB8D92C72AED5A3571E6BF5D3C1DBBC4708EE8C8575D6482D80E09E0F9398BF5CF97836E27DB58E78F975C21547BB5E75A6EDF13E4B5B97342D298DAABF0003001E8847A70D0A656E6473747265616D0D656E646F626A0D342030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F466972737420352F4C656E677468203133312F4E20312F547970652F4F626A53746D3E3E73747265616D0D0A68DE3234503050B0B1D1774A2C4E75CBCF2BD1777276097075D5764ECCC94C2ACAD475CACF49D177CD4BCE4FC9CC4BD70FCFCC73CC2BCE84F3DD328B8A4B9C33128B148C8DF441DA5D528B938B320B4AF28B142C800607E9FB242214049726955416A4EA871495A686801920026C6B78664A4946B18225488F9D1D40800100660D31AC0D0A656E6473747265616D0D656E646F626A0D352030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F466972737420352F4C656E6774682034312F4E20312F547970652F4F626A53746D3E3E73747265616D0D0A68DE3234543050B0B1D177CE2FCD2B5130D00FA92C48D5F72F2DC9C9CC4B2DB6B303083000892909A20D0A656E6473747265616D0D656E646F626A0D362030206F626A0D3C3C2F46696C7465722F466C6174654465636F64652F466972737420352F4C656E6774682035342F4E20312F547970652F4F626A53746D3E3E73747265616D0D0A68DE3234523050B0B1D177CE2FCD2B5130D2F7CE4C298E3634050A06291882C858FD90CA8254FD80C4F4D4623B3B80000300241A0CEF0D0A656E6473747265616D0D656E646F626A0D372030206F626A0D3C3C2F4465636F64655061726D733C3C2F436F6C756D6E7320352F507265646963746F722031323E3E2F46696C7465722F466C6174654465636F64652F49445B3C31333044443637363832373635303442423337374339444432354142434332363E3C32354344463033303236304631383430414234373435414635324336423041433E5D2F4C656E6774682035362F526F6F74203134203020522F53697A652031332F547970652F585265662F575B31203320315D3E3E73747265616D0D0A68DE62620001264666F37F0C4C40461888E46E05918CD260F20198048B304C02AAFCBB5302CC666004918CFFC1240384040830001E0707230D0A656E6473747265616D0D656E646F626A0D7374617274787265660D0A3131360D0A2525454F460D0A, 1, NULL, N'test', N'test', N'test', NULL, NULL, NULL, 12, CAST(23.00 AS Decimal(18, 2)), N'AC0037', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (38, N'Academic Council', N'DR. SAMRAT M R', NULL, N'9845184840', N'samrat_hegde@yahoo.co.in', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0038', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (39, N'Academic Council', N'DR. BHARATHI S H', NULL, N'9844394595', N'gdcribellary@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0039', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (40, N'Academic Council', N'DR. VIJAYANAND PUJARI', NULL, N'9542185209', N'dr.vjpj@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0040', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (41, N'Academic Council', N'DR. RAJASHEKAR S', NULL, N'9241033201', N'rajasekaranpharm@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0041', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (42, N'Academic Council', N'DR. BASAVARAJ D. NADAGOUDA', NULL, N'9448023966', N'drbasunadagouda@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0042', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (43, N'Academic Council', N'DR. KIRAN KUMAR REDDY B', NULL, N'9880764634', N'kkreddybnys@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0043', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (44, N'Academic Council', N'PROF. BHAGYALAKSHMI R', NULL, N'9738465046', N'spoorthygowda1@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0044', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (45, N'Academic Council', N'DR. ANU THOMAS', NULL, N'9880471831', N'anuthom5477@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0045', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (46, N'Academic Council', N'DR. JYOTHI RAMEGOWDA', NULL, N'9740361867', N'joe6gowda@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0046', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (47, N'Academic Council', N'DR. SUDHAKARA MUNISWAMAPPA', NULL, N'9845978858', N'sudhakarmop@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0047', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (48, N'Academic Council', N'DR. LAGHNA GOWDA', NULL, N'9986072069', N'drlaghnagowda@mradc.in', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0048', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (49, N'Academic Council', N'PROF. MANU B', NULL, N'8861347845', N'bmanusonu86@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AC0049', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (50, N'Senate Members', N'Dr. Dilip Kumar N R', CAST(N'1998-12-12' AS Date), N'9844288283', N'drdilip57@gmail.com', NULL, N'PanTest', N'AdharTest', N'Dr. Dilip Kumar N R', N'100223312563', N'IFSCTEST', N'TESTBANK', N'$2a$11$l2SL5X7n0NfY8Q3kZCZ2Muo3Gfsn/1jIppFolOcv05CTlP/3dYdmq', N'TESTBRANCH', NULL, NULL, NULL, 0, NULL, N'Road', N'test', N'TEST', NULL, NULL, NULL, 12, CAST(21.00 AS Decimal(18, 2)), N'SE0001', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (51, N'Senate Members', N'Dr. Anoop Nair', NULL, N'9886459375', N'anoop@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'$2a$11$C.NAG12eXDJjcHembZP/s.T6ihqm.8UOemyBruBTc6lO6r6G9WHSq', NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0002', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (52, N'Senate Members', N'Dr. G Manjunath Gowda', NULL, N'9066679444', N'manjunathgowda3141@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0003', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (53, N'Senate Members', N'Dr. Yashodha Darshan', NULL, N'9483043552', N'yash101@live.in', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0004', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (54, N'Senate Members', N'Dr. Sudharshan', NULL, N'9449104316', N'sudarshansajjan78@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0005', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (55, N'Senate Members', N'Dr. Suresh S Kanakannavar', CAST(N'2004-01-01' AS Date), N'9448033228', N'kanakannavar@gmail.com', NULL, N'TEST456', N'693265874561', N'Dr. Suresh S Kanakannavar', N'9999939399000', N'Test001', N'TEST12', N'$2a$11$6jeaqerS0dYxZelXPZqv.eob7G7NVYu.gZBmMMTFOqYaWJoYghZcq', N'TEST21', NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0006', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (56, N'Senate Members', N'Dr. K.C Shashidara', NULL, N'9448064613', N'kcshashidhara@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0007', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (57, N'Senate Members', N'Dr. Bharath Anche', NULL, N'8892746974', N'bharath.dr12@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0008', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (58, N'Senate Members', N'Dr. Srinivasan Velu', NULL, N'9448060250', N'drseena4333@yahoo.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0009', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (59, N'Senate Members', N'Dr. Mahendra. M', NULL, N'9980796290', N'drmahigowda@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0010', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (60, N'Senate Members', N'Dr. Arvind Katti', NULL, N'9481640062', N'dr.arvindkatti@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0011', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (61, N'Senate Members', N'Dr. Srinivas LD', NULL, N'7259920520', N'srinildpharma@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0012', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (62, N'Senate Members', N'Dr. Konaraddi J B', NULL, N'9739309658', N'dr_jk29@rediffmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0013', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (63, N'Senate Members', N'Dr. Naveen.S', NULL, N'9886634170', N'nvin73@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0014', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (64, N'Senate Members', N'Smt. Vaishali Sreejith', NULL, N'9845325069', N'vaishalisreejith9@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0015', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (65, N'Senate Members', N'Dr. Shashi Kumar H C', NULL, N'9980280485', N'drshashirrdcortho@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0016', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (66, N'Senate Members', N'Dr. Veeresh Karabasappa Hanchinal', CAST(N'2001-12-01' AS Date), N'9620151813', N'veeru@gmail.com', NULL, N'UJYHU9980P', N'456325415244', N'Dr. Veeresh Karabasappa Hanchinal', N'99999393000', N'TGHYU4453OH', N'TESTBANK', N'$2a$11$cY2VPrpUGEBAsOrbhw4wKOwmgrbyPgu8bFJww8TwvEqZZkuA1T16W', N'Test', NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0017', N'A008', N'D020', N'Shalakyatantra', N'4')
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (67, N'Senate Members', N'Sri. Melbin Michael Aeackal', NULL, N'9739215124', N'melbin000@gmail.com

', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0018', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (68, N'Senate Members', N'Dr. Mahendra. S', NULL, N'9845563431', N'mahendraortho@gmail.com ', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0019', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (69, N'Senate Members', N'Sri. Santosh Subhas Indi', NULL, N'8123374800', N'mr.santoshindi16@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0020', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (70, N'Senate Members', N'Dr. Kiran Kumar. K', NULL, N'9481965646', N'dr.kiran.ayur@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0021', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (71, N'Senate Members', N'Dr. Shiva Sharan. K ', NULL, N'9448144399', N'shivsharan@hotmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0022', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (72, N'Senate Members', N'Shri. T. Dileep Kumar ', NULL, N'9871188349', N'tdileep55@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0023', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (73, N'Senate Members', N'Dr. Sankanagoud Patil', NULL, N'9019587969', N'ayursank@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0024', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (74, N'Senate Members', N'Dr. Vinutha Rao', NULL, N'9980181331', N'drvinuthamvm@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0025', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (75, N'Senate Members', N'Dr. Nandeesh J', NULL, N'9845471377', N'nandeesh.aips@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0026', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (76, N'Senate Members', N'Dr. Basavaraj S Savadi', NULL, N'9845646885', N'drbssavadi123@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0027', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (77, N'Senate Members', N'Dr. Ummed Ram', NULL, N'9448089518', N'ramummed@gmail.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0028', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (78, N'Senate Members', N'Dr. Harish M R', NULL, N'9448323157', N'dermaharish@yahoo.com', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'SE0029', NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (82, N'Subject Expertise', N'Dr Suresh B Sonth', NULL, N'9448569200', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (83, N'Subject Expertise', N'Aishwarya M', NULL, N'9535724980', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (84, N'Subject Expertise', N'Test', CAST(N'2001-01-01' AS Date), N'9999999999', N'testadda@gmail.com', N'hgmjjm', N'ABHYT6659P', N'478965236548', N'test', N'TestAdda', N'SBIN0001234', N'TestAdda', N'$2a$11$6ls/x7Dm4WWYz7uoQD5wrewjv5wnpS4qOsTBQsSZi8bdxkW5UDQiy', N'TestAdda', NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (85, N'Academic Council', N'testAC', CAST(N'2001-01-01' AS Date), N'9874563210', N'test@gmail.com', NULL, N'IBHYT9987J', N'693265874253', N'testAC', N'252525525545', N'IKJU887600', N'Testbank', N'$2a$11$Ts03IicQHKnzi0SlT.Rxeeedv7byK/ONGL3w3.St8EMBmcNoZzovG', N'test0033', NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'AH297', N'ASSOC, PROFESSORS', N'of Public Health', NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (86, N'Senate Members', N'testSenet', CAST(N'2001-01-04' AS Date), N'8745632109', N'senet@gmail.com', NULL, N'DFGDK9963M', N'858585858558', N'testSenet', N'Abc123456789011200', N'TGHYU4453OH', N'datatest', N'$2a$11$sh2gg/daPLDWdNxOCrjEc.CHObvJS0/tuKw/UUdwu14qv3y9tSapW', N'testdata', NULL, NULL, NULL, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (87, N'Subject Expertise', N'Dr. Deepashree. C. L', NULL, N'9380333921', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (88, N'Subject Expertise', N'Mr.Rahul.E', NULL, N'9606820396', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (89, N'Subject Expertise', N'Dr. VB Gowda', NULL, N'9845259264', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (90, N'Subject Expertise', N'Dr. Shruthi. K. R', CAST(N'2000-01-01' AS Date), N'9845759889', N'kr@gmail.com', NULL, N'DFGDK9963K', N'656984586522', N'Dr. Shruthi. K. R', N'999993939933d22222', N'TGHYU4453OH', N'Testbank', N'$2a$11$m4Zci6sPDI8XMkTIaqDZEeX/fVdMdMQoFLCwqmforDeadza2uhzHq', N'test0033', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'D015', N'D011', N'Oral Pathology & Microbiology', N'2')
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (91, N'Subject Expertise', N'Dr. Deepashree. C. L', NULL, N'9380333921', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (92, N'Subject Expertise', N'Dr. Mouna Subbaramaiah,', NULL, N'9886456713', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (93, N'Subject Expertise', N'Sindhya S G', NULL, N'9538356631', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (94, N'Subject Expertise', N'Usha S', NULL, N'9164981432', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (95, N'Subject Expertise', N'Dr. Shruthi. K. R', NULL, N'9845759889', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (96, N'Subject Expertise', N'Dr. Shruthi. K. R', NULL, N'9845759889', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (97, N'Subject Expertise', N'Dr Jnaneshwar Sagar', NULL, N'9481109096', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (98, N'Subject Expertise', N'Eshwar Reddy', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (99, N'Subject Expertise', N'Usha S', NULL, N'9164981432', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (100, N'Subject Expertise', N'Dr Sneha', NULL, N'6363205835', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (101, N'Subject Expertise', N'Karthik G Kamath K', NULL, N'9164728708', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (102, N'Subject Expertise', N'Sindhya S G', NULL, N'9538356631', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (103, N'Subject Expertise', N'Dr Guruprasad N', NULL, N'9449458177', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (104, N'Subject Expertise', N'Shilpa Rani R', NULL, N'8296282308', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (105, N'Subject Expertise', N'Karthik G Kamath K', NULL, N'9164728708', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (106, N'Subject Expertise', N'Shubha K S', NULL, N'7899659302', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (107, N'Subject Expertise', N'Shilpa Rani R', NULL, N'8296282308', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (108, N'Subject Expertise', N'Prof.Lakshmi Nagamani N', NULL, N'9886662361', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (109, N'Subject Expertise', N'Prof. Moumita Chakraborty', NULL, N'7218490653', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (110, N'Subject Expertise', N'Mr. SHIVASHANKAR PAI B', NULL, N'7975353857', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (111, N'Subject Expertise', N'Dr.C.P.Chandrappa', NULL, N'9686114872', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (112, N'Subject Expertise', N'Bindu Prakash', NULL, N'9180504381', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (113, N'Subject Expertise', N'Dr.Amareshwara M Maligi', NULL, N'9845724114', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (114, N'Subject Expertise', N'Shilpa Rani R', NULL, N'8296282308', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (115, N'Subject Expertise', N'Dr.Aniruddh Kustagi', NULL, N'8904820339', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (116, N'Subject Expertise', N'Puneeth H R', NULL, N'9986975865', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (117, N'Subject Expertise', N'Kavya', NULL, N'9740702313', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (118, N'Subject Expertise', N'REMYA JOY', NULL, N'776081663', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (119, N'Subject Expertise', N'Bindu Prakash', NULL, N'8050438121', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (120, N'Subject Expertise', N'Prof. Ravi kumar T.N', NULL, N'9743375330', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (121, N'Subject Expertise', N'Dr.Lavanya.J', NULL, N'9480564475', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (122, N'Subject Expertise', N'Usha S', NULL, N'9164981432', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (123, N'Subject Expertise', N'Dr.Pampareddy B.Kollur', NULL, N'9448449833', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (124, N'Subject Expertise', N'Dr.Lavanya.J', NULL, N'9480564475', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (125, N'Subject Expertise', N'Dr. Mouna Subbaramaiah,', NULL, N'9886456713', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (126, N'Subject Expertise', N'Dr. Madhu KP', NULL, N'9380455534', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (127, N'Subject Expertise', N'Dr. Manjunath HK.', NULL, N'9980128523', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (128, N'Subject Expertise', N'Prof. Sara Mathew', NULL, N'8861427130', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (129, N'Subject Expertise', N'Prof.Lakshmi Nagamani N', NULL, N'9886662361', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (130, N'Subject Expertise', N'Dr Shankar Shetty', NULL, N'9886290454', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (131, N'Subject Expertise', N'Dr.avinasha.M', NULL, N'9686497660', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (132, N'Subject Expertise', N'Shubha K S', NULL, N'7899659302', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (133, N'Subject Expertise', N'Dr Purushotham R', NULL, N'9980007690', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (134, N'Subject Expertise', N'Dr. Sharath kumar shetty', NULL, N'9449970001', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (135, N'Subject Expertise', N'SEEMA SHETTIGAR', NULL, N'9988669100', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (136, N'Subject Expertise', N'Mr.Rahul.E', NULL, N'9606820396', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (137, N'Subject Expertise', N'Dr Priyanka M K', NULL, N'8951992349', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (138, N'Subject Expertise', N'Pushtela Arun Raj', NULL, N'9550305507', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (139, N'Subject Expertise', N'Nanda Kishore M,', NULL, N'9036391002', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (140, N'Subject Expertise', N'Aishwarya M', NULL, N'9535724980', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (141, N'Subject Expertise', N'Sindhya S G', NULL, N'9538356631', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (142, N'Subject Expertise', N'Dr Priyanka M K', NULL, N'8951992349', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (143, N'Subject Expertise', N'Nanda Kishore M,', NULL, N'9036391002', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (144, N'Subject Expertise', N'Dr.Spoorthi M', NULL, N'9740438747', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (145, N'Subject Expertise', N'Prof. Diney G Dsouza', NULL, N'8105638419', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (146, N'Subject Expertise', N'Bindu Prakash B N', NULL, N'8050438121', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (147, N'Subject Expertise', N'Naveen Kumar H.V', NULL, N'9141015963', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (148, N'Subject Expertise', N'Dr. Harish Bhat. K', NULL, N'9448725021', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (149, N'Subject Expertise', N'DR BASAVARAJ.K.N.', NULL, N'9845759889', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (150, N'Subject Expertise', N'Naveen Kumar H.V', NULL, N'9141015963', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (151, N'Subject Expertise', N'Dr. Sharath kumar shetty', NULL, N'9449970001', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (152, N'Subject Expertise', N'Shilpa Rani R', NULL, N'8296282308', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (153, N'Subject Expertise', N'DR. AVINASH KO', NULL, N'9483466597', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (154, N'Subject Expertise', N'DR. AVINASH KO', NULL, N'9483466597', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (155, N'Subject Expertise', N'Dr. Shruthi. K. R', NULL, N'9845759889', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (156, N'Subject Expertise', N'Prof. Sara Mathew', NULL, N'8861427130', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (157, N'Subject Expertise', N'Dr Priyanka M K', NULL, N'8951992349', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (158, N'Subject Expertise', N'SEEMA SHETTIGAR', NULL, N'9988669100', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (159, N'Subject Expertise', N'Dr. Shruthi. K. R', NULL, N'9845759889', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (160, N'Subject Expertise', N'Dr Ajay KT', NULL, N'9844324209', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (161, N'Subject Expertise', N'Dr.Athira Sreenivas', NULL, N'9400056955', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (162, N'Subject Expertise', N'Prof.Chaitra K.R', NULL, N'9986104772', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (163, N'Subject Expertise', N'Dr.Avinasha.M', NULL, N'9686497660', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (164, N'Subject Expertise', N'Dr. Harsha Prakash B.G', NULL, N'9972076646', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (165, N'Subject Expertise', N'Dr. Manjunath HK.', NULL, N'9980128523', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (166, N'Subject Expertise', N'Dr. Veena M K', NULL, N'9980606774', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (167, N'Subject Expertise', N'Dr Gayatri S Hegde', NULL, N'9880982848', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (168, N'Subject Expertise', N'Prof.Chaitra K.R', NULL, N'9986104772', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (169, N'Subject Expertise', N'Prof.Chaitra K.R', NULL, N'9986104772', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (170, N'Subject Expertise', N'Prof.Chaitra K.R', NULL, N'9986104772', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (171, N'Subject Expertise', N'Prof.Chaitra K.R', NULL, N'9986104772', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (172, N'Subject Expertise', N'Dr. Harish Bhat. K', NULL, N'9448725021', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (173, N'Subject Expertise', N'Shilpa Rani R', NULL, N'8296282308', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (174, N'Subject Expertise', N'Dr.Athira Sreenivas', NULL, N'9400056955', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (175, N'Subject Expertise', N'Dr. Pradeep H.N', NULL, N'9341798303', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (176, N'Subject Expertise', N'Prof. Moumita Chakraborty', NULL, N'7218490653', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (177, N'Subject Expertise', N'Dr. Indhudhar', NULL, N'9591072089', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (178, N'Subject Expertise', N'Dr. Shruthi. K. R', NULL, N'9845759889', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (179, N'Subject Expertise', N'Dr. Abdul Kaleem Bahadur', NULL, N'9900177058', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (180, N'Subject Expertise', N'Dr. Harsha Prakash B.G', NULL, N'9972076646', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (181, N'Subject Expertise', N'Dr. Abdul Kaleem Bahadur', NULL, N'9900177058', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (182, N'Subject Expertise', N'Dr Shankar Shetty', NULL, N'9886290454', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (183, N'Subject Expertise', N'Dr Venkappa S. Mantur', NULL, N'9986530484', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (184, N'Subject Expertise', N'Dr Gayatri S Hegde', NULL, N'9880982848', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (185, N'Subject Expertise', N'Pushtela Arun Raj', NULL, N'9550305507', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (186, N'Subject Expertise', N'Dr.Spoorthi M', NULL, N'9740438747', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (187, N'Subject Expertise', N'Dr Venkappa S. Mantur', NULL, N'9986530484', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (188, N'Subject Expertise', N'Dr Nagalakshmi C S', NULL, N'9241852058', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (189, N'Subject Expertise', N'Dr. Shruthi. K. R', NULL, N'9845759889', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (190, N'Subject Expertise', N'Dr. Rahul Roy', NULL, N'9742536112', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (191, N'Subject Expertise', N'Mr. Shivanakarappa C', NULL, N'9066101707', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (192, N'Subject Expertise', N'Dr. Shruthi. K. R', NULL, N'9845759889', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (193, N'Subject Expertise', N'REMYA JOY', NULL, N'7760816663', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (194, N'Subject Expertise', N'Dr Omkareshwar Patil', NULL, N'9449734071', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (195, N'Subject Expertise', N'Dr.Spoorthi M', NULL, N'9740438747', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (196, N'Subject Expertise', N'Dr Nagalakshmi C S', NULL, N'9241852058', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (197, N'Subject Expertise', N'Prof Pratijna Suhasini G R', NULL, N'9964144248', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (198, N'Subject Expertise', N'Dr. Dhanya Kumar,', NULL, N'9480564475', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (199, N'Subject Expertise', N'Dr Gayatri S Hegde', NULL, N'9880982848', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (200, N'Subject Expertise', N'Dr. Rahul Roy', NULL, N'9742536112', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (201, N'Subject Expertise', N'Syed Moinuddin', NULL, N'9945278563', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (202, N'Subject Expertise', N'DR. LEESHA SHARON', NULL, N'9731689204', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (203, N'Subject Expertise', N'JOSHI THOMAS JOHN', NULL, N'6351827607', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (204, N'Subject Expertise', N'DR. LEESHA SHARON', NULL, N'9731689204', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (205, N'Subject Expertise', N'Dr. Dhanya Kumar,', NULL, N'9243312478', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (206, N'Subject Expertise', N'OMKARESHWAR PATIL', NULL, N'9449734071', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (207, N'Subject Expertise', N'Mr. Shivanakarappa C', NULL, N'9066101707', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (208, N'Subject Expertise', N'Dr. Harsha Prakash B.G', NULL, N'9972076646', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (209, N'Subject Expertise', N'Dr.Pushpalatha.G', NULL, N'9739009216', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (210, N'Subject Expertise', N'Dr. Harsha Prakash B.G', NULL, N'9972076646', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (211, N'Subject Expertise', N'Dr.Pushpalatha.G', NULL, N'9739009216', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (212, N'Subject Expertise', N'Dr. Anand G. Valu', NULL, N'9916580812', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (213, N'Subject Expertise', N'Dr Yogaraje Gowda C V', NULL, N'9844076780', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (214, N'Subject Expertise', N'Mrs.Mahitha K C', NULL, N'9036744141', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (215, N'Subject Expertise', N'Dr.Diwakar Rao', NULL, N'9972015862', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (216, N'Subject Expertise', N'Dr.Diwakar Rao', NULL, N'9972015862', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (217, N'Subject Expertise', N'Dr.Sheethal K.C', NULL, N'9742299902', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (218, N'Subject Expertise', N'Mr.Joshi Thomas John', NULL, N'8000471304', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (219, N'Subject Expertise', N'Dr. Shruthi. K. R', NULL, N'9845759889', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (220, N'Subject Expertise', N'Dr. Dhanya Kumar,', NULL, N'9243312478', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (221, N'Subject Expertise', N'Dr, Indhudhara PB', NULL, N'9591072089', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (222, N'Subject Expertise', N'Dr. Puneeth HR', NULL, N'9986975865', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (223, N'Subject Expertise', N'Mr.Joshi Thomas John', NULL, N'8000471304', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (224, N'Subject Expertise', N'Dr. Madhu. S.D', NULL, N'9844673161', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (225, N'Subject Expertise', N'Chandraveni k', NULL, N'9731291468', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (226, N'Subject Expertise', N'Mr. Shivashankarappa C', NULL, N'9066101707', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (227, N'Subject Expertise', N'Dr.Lalitha.S', NULL, N'9900960640', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (228, N'Subject Expertise', N'Dr.Shivashankara. A. R', NULL, N'9880146133', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (229, N'Subject Expertise', N'Dr Vysakh PR', NULL, N'9513661858', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (230, N'Subject Expertise', N'Mr. Kalaimanidhandapani', NULL, N'8807488217', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (231, N'Subject Expertise', N'Dr.Sheethal K.C', NULL, N'9742299902', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (232, N'Subject Expertise', N'Dr. Ramya B.S', NULL, N'9480303156', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (233, N'Subject Expertise', N'Mr. Kalaimanidhandapani', NULL, N'8807488217', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (234, N'Subject Expertise', N'DR BASAVARAJ.K.N.', NULL, N'9448948262', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (235, N'Subject Expertise', N'Dr. Venkatesh Jayadutt', NULL, N'8904294870', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (236, N'Subject Expertise', N'Dr. Venkatesh Jayadutt', NULL, N'8904294870', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (237, N'Subject Expertise', N'Dr. Ramya B.S', NULL, N'9480303156', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (238, N'Subject Expertise', N'Shubha K S', NULL, N'7899659302', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (239, N'Subject Expertise', N'Dr. Rashmi Sreenivasmurthy', NULL, N'9480705668', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (240, N'Subject Expertise', N'DR BASAVARAJ.K.N.', NULL, N'9448948262', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (241, N'Subject Expertise', N'Dr. Ramya B.S', NULL, N'9480303156', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (242, N'Subject Expertise', N'Dr. Ramya B.S', NULL, N'9480303156', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (243, N'Subject Expertise', N'Dr.Shivashankara. A. R', NULL, N'9880146133', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (244, N'Subject Expertise', N'Dr Vysakh PR', NULL, N'9513661858', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (245, N'Subject Expertise', N'Prof Geethalakshmi I P', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (246, N'Subject Expertise', N'Dr.Lalitha.S', NULL, N'9900960640', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (247, N'Subject Expertise', N'Chandraveni k', NULL, N'9731291468', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (248, N'Subject Expertise', N'Dr. Rashmi Sreenivasmurthy', NULL, N'9480705668', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (249, N'Subject Expertise', N'Dr. Rahul Roy', NULL, N'9742536112', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (250, N'Subject Expertise', N'PRATHIKSHA SHETTY', NULL, N'7411789834', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (251, N'Subject Expertise', N'PRATHIKSHA SHETTY', NULL, N'7411789834', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (252, N'Subject Expertise', N'JOSHI THOMAS JOHN', NULL, N'6351827607', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (253, N'Subject Expertise', N'DR. LEESHA SHARON', NULL, N'9731689204', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (254, N'Subject Expertise', N'PRATHIKSHA SHETTY', NULL, N'7411789834', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (255, N'Subject Expertise', N'JOSHI THOMAS JOHN', NULL, N'6351827607', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (256, N'Subject Expertise', N'Prof Pratijna Suhasini G R', NULL, N'9964144248', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (257, N'Subject Expertise', N'Dr Vysakh PR', NULL, N'9513661858', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (258, N'Subject Expertise', N'Dr. Dhanya Kumar,', NULL, N'9686497660', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (259, N'Subject Expertise', N'Dr. Avinasha.M', NULL, N'9686497660', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (260, N'Subject Expertise', N'Dr Mahesh S Patil', NULL, N'9632311001', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (261, N'Subject Expertise', N'Dr. Mahesh Patil', NULL, N'9632311001', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (262, N'Subject Expertise', N'OMKARESHWAR PATIL', NULL, N'9449734071', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (263, N'Subject Expertise', N'Prof Geethalakshmi I P', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (264, N'Subject Expertise', N'Dr Vysakh PR', NULL, N'9513661858', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (265, N'Subject Expertise', N'Prof. Akshatha Alva', NULL, N'9980931791', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (266, N'Subject Expertise', N'Dr. Dhanya Kumar,', NULL, N'9243312478', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (267, N'Subject Expertise', N'Prof Shrivanitha', NULL, N'9481974422', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (268, N'Subject Expertise', N'Prof Shrivanitha', NULL, N'9481974422', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (269, N'Subject Expertise', N'Prof. Akshatha Alva', NULL, N'9980931791', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (270, N'Subject Expertise', N'PRATHIKSHA SHETTY', NULL, N'7411789834', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (271, N'Subject Expertise', N'Nijin', NULL, N'7022930807', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (272, N'Subject Expertise', N'REMYA S NAIR', NULL, N'9539938842', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (273, N'Subject Expertise', N'Nijin', NULL, N'7022930807', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (274, N'Subject Expertise', N'Bindu Prakash', NULL, N'9180504381', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (275, N'Subject Expertise', N'Muzamil Majid Lone', NULL, N'7006315126', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (276, N'Subject Expertise', N'REMYA S NAIR', NULL, N'9539938842', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (277, N'Subject Expertise', N'Dr Vysakh PR', NULL, N'9513661858', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (278, N'Subject Expertise', N'DR NANDHINI', NULL, N'9845889974', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (279, N'Subject Expertise', N'DR NANDHINI', NULL, N'9845889974', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (280, N'Subject Expertise', N'Dr. Srinivas K H', NULL, N'9448516554', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (281, N'Subject Expertise', N'Dr. Srinivas K H', NULL, N'9448516554', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (282, N'Subject Expertise', N'Dr. Puneeth HR', NULL, N'9986975865', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (283, N'Subject Expertise', N'Anusha P.Masudi ( A.k bennur).', NULL, N'9620412458', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (284, N'Subject Expertise', N'Anusha P.Masudi ( A.k bennur).', NULL, N'9620412458', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (285, N'Subject Expertise', N'Anusha P.Masudi ( A.k bennur).', NULL, N'9620412458', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (286, N'Subject Expertise', N'Anusha P.Masudi ( A.k bennur).', NULL, N'9620412458', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (287, N'Subject Expertise', N'Dr. Deepashree. C. L', NULL, N'9380333921', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (288, N'Subject Expertise', N'SREELATHA I', NULL, N'9739873977', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (289, N'Subject Expertise', N'SREELATHA I', NULL, N'9739873977', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (290, N'Subject Expertise', N'Shilpa Rani.R', NULL, N'8296282308', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (291, N'Subject Expertise', N'Dr Shwetha Ramu', NULL, N'9986731522', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (292, N'Subject Expertise', N'Dr YOGARAJA GOWDA C V', NULL, N'9844076780', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (293, N'Subject Expertise', N'Dr Vysakh PR', NULL, N'9513661858', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (294, N'Subject Expertise', N'Shilpa Rani R', NULL, N'8296282308', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (295, N'Subject Expertise', N'Kavya K', NULL, N'9886820475', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (296, N'Subject Expertise', N'Kavya K', NULL, N'9886820475', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (297, N'Subject Expertise', N'Prof Shripriya H', NULL, N'9591837893', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (298, N'Subject Expertise', N'Sachin BS', CAST(N'2001-12-12' AS Date), N'8197230326', N'sach@gmail.com', NULL, N'IKJUY6678I', N'098765433221', N'Sachin BS', N'999993939933', N'IKJU88765', N'TESTBANK', N'$2a$11$Kg8hXBcE0aCMOiFEPZE53eJhg/PyjVufzJWFMDW8lEAxb7OGzDE/y', N'test0031', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, N'N868', N'Senior Resident', N'Cardiac Anaesthesia
', NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (299, N'Subject Expertise', N'OMKARESHWAR PATIL', NULL, N'9449734071', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (300, N'Subject Expertise', N'DR. P. KATHYAYANI', NULL, N'9008246386', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (301, N'Subject Expertise', N'DR. P. KATHYAYANI', NULL, N'9008246386', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (302, N'Subject Expertise', N'Dr Shwetha Ramu', NULL, N'9986731522', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (303, N'Subject Expertise', N'Dr Sampat Kumar', NULL, N'8050297019', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (304, N'Subject Expertise', N'Sindhya SG', NULL, N'9538356631', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (305, N'Subject Expertise', N'OMKARESHWAR PATIL', NULL, N'9449734071', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (306, N'Subject Expertise', N'Shilpa Rani R', NULL, N'8296282308', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (307, N'Subject Expertise', N'Dr Mahesh S Patil', NULL, N'9632311001', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (308, N'Subject Expertise', N'Prof Shripriya H', NULL, N'9591837893', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (309, N'Subject Expertise', N'Prof Shripriya H', NULL, N'9591837893', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (310, N'Subject Expertise', N'Dr. Sushma S', NULL, N'9916915194', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (311, N'Subject Expertise', N'Dr VIKAS S JOSHI', NULL, N'9743259323', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (312, N'Subject Expertise', N'Hithaishi M k', NULL, N'9740951040', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (313, N'Subject Expertise', N'Dr VIKAS S JOSHI', NULL, N'9743259323', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (314, N'Subject Expertise', N'Dr Sampat Kumar', NULL, N'8050297019', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (315, N'Subject Expertise', N'Naveen Kumar H.V', NULL, N'9141015963', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (316, N'Subject Expertise', N'Dr. Vinay. P. S', NULL, N'8970313466', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (317, N'Subject Expertise', N'Dr. Vinay. P. S', NULL, N'8970313466', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (318, N'Subject Expertise', N'Mr. Shivashankarappa C', NULL, N'9066101707', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (319, N'Subject Expertise', N'Sudhir Joshilkar', NULL, N'9743249565', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (320, N'Subject Expertise', N'Sudhir Joshilkar', NULL, N'9743249565', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (321, N'Subject Expertise', N'Bindu Prakash', NULL, N'9180504381', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (322, N'Subject Expertise', N'Dr. Madhu KP', NULL, N'9380455534', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (323, N'Subject Expertise', N'Dr Supriya Gachinmath', NULL, N'9845583109', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (324, N'Subject Expertise', N'Dr. Madhu KP', NULL, N'9380455534', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (325, N'Subject Expertise', N'Praffulkumar Jadimath', NULL, N'9742934998', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (326, N'Subject Expertise', N'Rudrappa P T', NULL, N'9164101191', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (327, N'Subject Expertise', N'Jyosna Thayyil', NULL, N'8971968336', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (328, N'Subject Expertise', N'Praffulkumar Jadimath', NULL, N'9742934998', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (329, N'Subject Expertise', N'Praffulkumar Jadimath', NULL, N'9742934998', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (330, N'Subject Expertise', N'Salim Malik', NULL, N'948211764', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (331, N'Subject Expertise', N'Naveen Kumar H.V', NULL, N'9141015963', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (332, N'Subject Expertise', N'Salim Malik', NULL, N'948211746', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (333, N'Subject Expertise', N'Dr Supriya Gachinmath', NULL, N'9845583109', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (334, N'Subject Expertise', N'Mr. Shivashankarappa C', NULL, N'9066101707', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (335, N'Subject Expertise', N'Jyosna Thayyil', NULL, N'8971968336', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (336, N'Subject Expertise', N'Mr. Shivashankarappa C', NULL, N'9066101707', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (337, N'Subject Expertise', N'Dr. Sushma S', NULL, N'9916915194', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (338, N'Subject Expertise', N'Kavya K', NULL, N'9886820475', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (339, N'Subject Expertise', N'Dr YOGARAJA GOWDA C V', NULL, N'9844076780', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (340, N'Subject Expertise', N'Mekala,', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (341, N'Subject Expertise', N'Dr Eshwar singh ,', NULL, N'9482083279', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (342, N'Subject Expertise', N'Nanda Kishore M,', NULL, N'9036391002', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (343, N'Subject Expertise', N'Dr YOGARAJA GOWDA C V', NULL, N'9844076780', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (344, N'Subject Expertise', N'Dr M G Giriyappagoudar', NULL, N'9449416138', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (345, N'Subject Expertise', N'Dr. Jnaneshwara K B', NULL, N'9481109096', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (346, N'Subject Expertise', N'Dr Eshwar singh ,', NULL, N'9482083279', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (347, N'Subject Expertise', N'Dr YOGARAJA GOWDA C V', NULL, N'9844076780', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (348, N'Subject Expertise', N'Vidyarani RJ', NULL, N'7760091391', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
INSERT [dbo].[LIC_Inspection] ([Id], [TypeofMember], [Name], [DOB], [PhoneNumber], [Email], [Address], [PANNumber], [AadhaarNumber], [AccountHolderName], [AccountNumber], [IFSCCode], [BankName], [CreatedPassword], [BranchName], [CollegeName], [DateOfInspection], [AttendenceDoc], [IsCompleted], [AttendanceFilePath], [ModeOfTravel], [FromPlace], [ToPlace], [Return_fromPlace], [Return_ToPlace], [ReturnKilometers], [Kilometers], [TotalCost], [MemberCode], [collegeCode], [designationCode], [departmentCode], [Facultycode]) VALUES (349, N'Subject Expertise', N'Naveen Kumar H.V', NULL, N'9141015963', NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL)
SET IDENTITY_INSERT [dbo].[LIC_Inspection] OFF
GO
ALTER TABLE [dbo].[LIC_Inspection] ADD  CONSTRAINT [DF__LIC_Inspe__IsCom__6C8E1007]  DEFAULT ((0)) FOR [IsCompleted]
GO
