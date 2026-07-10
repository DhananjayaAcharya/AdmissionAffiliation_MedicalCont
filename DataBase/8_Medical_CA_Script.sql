ALTER TABLE DepartmentMaster
ADD CONSTRAINT UQ_DepartmentMaster_DepartmentCode
UNIQUE (DepartmentCode);


CREATE TABLE MstDepartmentConfiguration
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentCode VARCHAR(50) NOT NULL,
    MaximumUnits INT NOT NULL,
    DisplayOrder INT NULL,
    IsActive BIT NOT NULL DEFAULT(1),
    CreatedBy VARCHAR(50) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
    ModifiedBy VARCHAR(50) NULL,
    ModifiedDate DATETIME NULL,
    CONSTRAINT FK_MstDepartmentConfiguration_DepartmentMaster
        FOREIGN KEY (DepartmentCode)
        REFERENCES DepartmentMaster(DepartmentCode)
);


CREATE TABLE MstDepartmentICUType
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ICUTypeCode VARCHAR(20) NOT NULL,
    ICUTypeName VARCHAR(100) NOT NULL,
    DisplayOrder INT NULL,
    IsActive BIT NOT NULL DEFAULT(1),
    CreatedBy VARCHAR(50) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
    ModifiedBy VARCHAR(50) NULL,
    ModifiedDate DATETIME NULL,
    CONSTRAINT UQ_MstDepartmentICUType_ICUTypeCode
        UNIQUE (ICUTypeCode)
);

CREATE TABLE MstDepartmentICUMapping
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    DepartmentCode VARCHAR(50) NOT NULL,
    ICUTypeCode VARCHAR(20) NOT NULL,
    DisplayOrder INT NULL,
    IsActive BIT NOT NULL DEFAULT(1),
    CreatedBy VARCHAR(50) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
    ModifiedBy VARCHAR(50) NULL,
    ModifiedDate DATETIME NULL,
    CONSTRAINT FK_MstDepartmentICUMapping_DepartmentMaster
        FOREIGN KEY (DepartmentCode)
        REFERENCES DepartmentMaster(DepartmentCode),
    CONSTRAINT FK_MstDepartmentICUMapping_ICUType
        FOREIGN KEY (ICUTypeCode)
        REFERENCES MstDepartmentICUType(ICUTypeCode)
);


CREATE TABLE AffiliationDepartmentInformation
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    CollegeCode VARCHAR(20) NOT NULL,
    FacultyCode INT NOT NULL,
    DepartmentCode VARCHAR(50) NOT NULL,
    LopDate DATE NULL,
    YearsSinceStarted INT NULL,
    HeadOfDepartment NVARCHAR(200) NULL,
    ExistingPGIntake INT NULL,
    IncreaseAdmissionFrom INT NULL,
    IncreaseAdmissionTo INT NULL,
    TotalUnits INT NULL,
    TotalDepartmentBeds INT NULL,
    TotalICUBeds INT NULL,
    IsActive BIT NOT NULL DEFAULT(1),
    CreatedBy VARCHAR(50) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
    ModifiedBy VARCHAR(50) NULL,
    ModifiedDate DATETIME NULL,
    CONSTRAINT FK_AffiliationDepartmentInformation_Department
        FOREIGN KEY (DepartmentCode)
        REFERENCES DepartmentMaster(DepartmentCode)
);


CREATE TABLE AffiliationDepartmentUnitDetails
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    DepartmentInformationId BIGINT NOT NULL,
    UnitNumber INT NOT NULL,
    NumberOfBeds INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT(1),
    CreatedBy VARCHAR(50) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
    ModifiedBy VARCHAR(50) NULL,
    ModifiedDate DATETIME NULL,
    CONSTRAINT FK_AffiliationDepartmentUnitDetails_DepartmentInformation
        FOREIGN KEY (DepartmentInformationId)
        REFERENCES AffiliationDepartmentInformation(Id)
);

CREATE TABLE AffiliationDepartmentICUDetails
(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    DepartmentInformationId BIGINT NOT NULL,
    ICUTypeCode VARCHAR(20) NOT NULL,
    IsAvailable BIT NOT NULL DEFAULT(0),
    TotalBeds INT NULL,
    OccupiedBeds INT NULL,
    IsActive BIT NOT NULL DEFAULT(1),
    CreatedBy VARCHAR(50) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),
    ModifiedBy VARCHAR(50) NULL,
    ModifiedDate DATETIME NULL,
    CONSTRAINT FK_AffiliationDepartmentICUDetails_DepartmentInformation
        FOREIGN KEY (DepartmentInformationId)
        REFERENCES AffiliationDepartmentInformation(Id),
    CONSTRAINT FK_AffiliationDepartmentICUDetails_ICUType
        FOREIGN KEY (ICUTypeCode)
        REFERENCES MstDepartmentICUType(ICUTypeCode)
);


------------------------------


----------- DERMATOLOGY ----------

INSERT INTO MstDepartmentConfiguration
(
    DepartmentCode,
    MaximumUnits,
    DisplayOrder,
    IsActive,
    CreatedBy
)
VALUES
('MD011', 1, 1, 1, 'Admin'),
('MD011', 2, 2, 1, 'Admin'),
('MD011', 3, 3, 1, 'Admin'),
('MD011', 4, 4, 1, 'Admin')
;

------------ GENERAL SURGERY -------------

INSERT INTO MstDepartmentConfiguration
(
    DepartmentCode,
    MaximumUnits,
    DisplayOrder,
    IsActive,
    CreatedBy
)
VALUES
('MD013', 1, 1, 1, 'Admin'),
('MD013', 2, 2, 1, 'Admin'),
('MD013', 3, 3, 1, 'Admin'),
('MD013', 4, 4, 1, 'Admin'),
('MD013', 5, 5, 1, 'Admin'),
('MD013', 6, 6, 1, 'Admin'),
('MD013', 7, 7, 1, 'Admin'),
('MD013', 8, 8, 1, 'Admin')
;

----------- OPHTHALMOLOGY ------------

INSERT INTO MstDepartmentConfiguration
(
    DepartmentCode,
    MaximumUnits,
    DisplayOrder,
    IsActive,
    CreatedBy
)
VALUES
('MD016', 1, 1, 1, 'Admin'),
('MD016', 2, 2, 1, 'Admin'),
('MD016', 3, 3, 1, 'Admin'),
('MD016', 4, 4, 1, 'Admin')
;

------------ OTORHINOLARYNGOLOGY --------


INSERT INTO MstDepartmentConfiguration
(
    DepartmentCode,
    MaximumUnits,
    DisplayOrder,
    IsActive,
    CreatedBy
)
VALUES
('MD015', 1, 1, 1, 'Admin'),
('MD015', 2, 2, 1, 'Admin'),
('MD015', 3, 3, 1, 'Admin')
;


------------------------------ ICU MASTER ------------

INSERT INTO MstDepartmentICUType
(
    ICUTypeCode,
    ICUTypeName,
    DisplayOrder,
    IsActive,
    CreatedBy
)
VALUES
('ICU',  'ICU(Intensive Care Unit)',            1, 1, 'Admin'),
('MICU', 'MICU(Medical Intensive Care Unit)',    2, 1, 'Admin'),
('SICU', 'SICU(Surgical Intensive Care Unit)',   3, 1, 'Admin'),
('CCU',  'CCU(Coronary Care Unit)',             4, 1, 'Admin'),
('NICU', 'NICU(Neonatal Intensive Care Unit)',   5, 1, 'Admin'),
('PICU', 'PICU(Pediatric Intensive Care Unit)',  6, 1, 'Admin'),
('HDU',  'HDU(High Dependency Unit)',           7, 1, 'Admin'),
('Post. op ward/HDU',  'Post. op ward/HDU',     8, 1, 'Admin');

---------------- ICU MAPPING MASTER ------------

-------------- MAP - DERMATOLOGY ---------

INSERT INTO MstDepartmentICUMapping
(
    DepartmentCode,
    ICUTypeCode,
    DisplayOrder,
    IsActive,
    CreatedBy
)
VALUES
('MD011', 'ICU', 1, 1, 'Admin'),
('MD011', 'HDU', 2, 1, 'Admin');

------------- MAP - GENERAL SURGERY ------------

INSERT INTO MstDepartmentICUMapping
(
    DepartmentCode,
    ICUTypeCode,
    DisplayOrder,
    IsActive,
    CreatedBy
)
VALUES

('MD013', 'SICU', 1, 1, 'Admin'),
('MD013', 'Post. op ward/HDU', 2, 1, 'Admin');


----------- MAP - ORTHOPEADICS ------------



INSERT INTO MstDepartmentICUMapping
(
    DepartmentCode,
    ICUTypeCode,
    DisplayOrder,
    IsActive,
    CreatedBy
)
VALUES

('MD014', 'SICU', 1, 1, 'Admin'),
('MD014', 'Post. op ward/HDU', 2, 1, 'Admin');


----------- MAP - OPHTHALMOLOGY --------------

INSERT INTO MstDepartmentICUMapping
(
    DepartmentCode,
    ICUTypeCode,
    DisplayOrder,
    IsActive,
    CreatedBy
)
VALUES

('MD016', 'SICU', 1, 1, 'Admin'),
('MD016', 'Post. op ward/HDU', 2, 1, 'Admin');

----------- MAP - OTORHINOLARYNGOLOGY ----

INSERT INTO MstDepartmentICUMapping
(
    DepartmentCode,
    ICUTypeCode,
    DisplayOrder,
    IsActive,
    CreatedBy
)
VALUES

('MD015', 'SICU', 1, 1, 'Admin'),
('MD015', 'Post. op ward/HDU', 2, 1, 'Admin');

-------- 

------------- 
EXEC sp_helpindex '[dbo].[DepartmentMaster]';