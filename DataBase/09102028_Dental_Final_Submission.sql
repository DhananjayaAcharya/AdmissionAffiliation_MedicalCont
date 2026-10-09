USE [Admission_Affiliation];
GO

/* ============================================================
   FINAL DENTAL SUBMISSION
   ============================================================ */

CREATE TABLE dbo.FinalDentalSubmission
(
    Id INT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_FinalDentalSubmission
        PRIMARY KEY CLUSTERED,

    CollegeCode NVARCHAR(100) NOT NULL,

    FacultyCode INT NOT NULL,

    CourseLevel VARCHAR(10) NOT NULL,

    AffiliationTypeId INT NOT NULL,

    AcademicYear VARCHAR(20) NOT NULL,

    ApplicationNumber VARCHAR(50) NOT NULL,

    PrincipalConsent BIT NOT NULL
        CONSTRAINT DF_FinalDentalSubmission_PrincipalConsent
        DEFAULT (0),

    /* ========================================================
       FOREIGN KEYS
       ======================================================== */

    CONSTRAINT FK_FinalDentalSubmission_College
        FOREIGN KEY (CollegeCode)
        REFERENCES dbo.Affiliation_College_Master (CollegeCode),

    CONSTRAINT FK_FinalDentalSubmission_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES dbo.Faculty (FacultyId),

    CONSTRAINT FK_FinalDentalSubmission_AffiliationType
        FOREIGN KEY (AffiliationTypeId)
        REFERENCES dbo.TypeOfAffiliation (TypeId)
);
GO


/* ============================================================
   UNIQUE INDEX
   Prevent duplicate submission for the same:

   College
   + Faculty
   + Course Level
   + Affiliation Type
   + Academic Year
   ============================================================ */

CREATE UNIQUE NONCLUSTERED INDEX UX_FinalDentalSubmission_Submission
ON dbo.FinalDentalSubmission
(
    CollegeCode,
    FacultyCode,
    CourseLevel,
    AffiliationTypeId,
    AcademicYear
);
GO


/* ============================================================
   APPLICATION NUMBER INDEX
   ============================================================ */

CREATE NONCLUSTERED INDEX IX_FinalDentalSubmission_ApplicationNumber
ON dbo.FinalDentalSubmission
(
    ApplicationNumber
);
GO


/* ============================================================
   FACULTY / COURSE / ACADEMIC YEAR INDEX
   ============================================================ */

CREATE NONCLUSTERED INDEX IX_FinalDentalSubmission_Faculty_Course_Year
ON dbo.FinalDentalSubmission
(
    FacultyCode,
    CourseLevel,
    AcademicYear
);
GO


USE [Admission_Affiliation];
GO

-- 1. Add IsActive column
ALTER TABLE dbo.FinalDentalSubmission
ADD IsActive BIT NOT NULL
    CONSTRAINT DF_FinalDentalSubmission_IsActive
    DEFAULT (1);
GO

-- 2. Add CreatedDate column
ALTER TABLE dbo.FinalDentalSubmission
ADD CreatedDate DATETIME NOT NULL
    CONSTRAINT DF_FinalDentalSubmission_CreatedDate
    DEFAULT (GETDATE());
GO

-- 3. Add ModifiedDate column
ALTER TABLE dbo.FinalDentalSubmission
ADD ModifiedDate DATETIME NULL;
GO

-- 4. Add SubmittedDate column
ALTER TABLE dbo.FinalDentalSubmission
ADD SubmittedDate DATETIME NULL;
GO

-- 5. Add CreatedBy column
ALTER TABLE dbo.FinalDentalSubmission
ADD CreatedBy NVARCHAR(100) NULL;
GO

-- 6. Add ModifiedBy column
ALTER TABLE dbo.FinalDentalSubmission
ADD ModifiedBy NVARCHAR(100) NULL;
GO