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