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
(2, 2, 'AUDIO – CASSETTES'),
(2, 2, 'VIDEO – CASSETTES'),
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

    Location NVARCHAR(250) NOT NULL,

    Address NVARCHAR(500) NOT NULL,

    ManagedBy NVARCHAR(250) NOT NULL,

    StaffList NVARCHAR(500) NULL,

    PopulationServed INT NULL,

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
        REFERENCES TypeOfAffiliation(TypeId)
);

--------------------------------


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