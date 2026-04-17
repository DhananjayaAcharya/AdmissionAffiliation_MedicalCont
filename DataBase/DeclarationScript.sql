USE Admission_Affiliation
GO
CREATE TABLE AffiliationFinalDeclarations
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CollegeCode NVARCHAR(100) NOT NULL,
    FacultyCode INT NOT NULL,
    AffiliationTypeId INT NOT NULL,
    PrincipalName NVARCHAR(150) NOT NULL,
    IsSubmitted BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    SubmittedDate DATETIME NULL,

    -- 🔗 FOREIGN KEYS
    CONSTRAINT FK_AffFinalDecl_College 
        FOREIGN KEY (CollegeCode) 
        REFERENCES [dbo].[Affiliation_College_Master](CollegeCode),

    CONSTRAINT FK_AffFinalDecl_Faculty 
        FOREIGN KEY (FacultyCode) 
        REFERENCES Faculty(FacultyId),

    CONSTRAINT FK_AffFinalDecl_Type 
        FOREIGN KEY (AffiliationTypeId) 
        REFERENCES TypeOfAffiliation(TypeId)
);

CREATE UNIQUE INDEX UX_AffFinalDeclarations
ON AffiliationFinalDeclarations (CollegeCode, FacultyCode, AffiliationTypeId);