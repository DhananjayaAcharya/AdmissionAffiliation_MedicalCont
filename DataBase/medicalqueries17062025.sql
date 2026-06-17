CREATE TABLE ContinuationTrustMemberDocuments
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FacultyCode VARCHAR(10) NOT NULL,
    CollegeCode VARCHAR(20) NOT NULL,
    RegisteredTrustMemberDetailsPath NVARCHAR(500) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME NULL
);

ALTER TABLE Aff_DeanTeachingExperience
ADD 
    OtherCollege NVARCHAR(250) NULL,
    FromDate DATE NULL,
    ToDate DATE NULL,
    ExpCollegeCode NVARCHAR(50) NULL;

ALTER TABLE Aff_PRINCIPALTEACHINGEXPERIENCE
ADD 
    OtherCollege NVARCHAR(250) NULL,
    FromDate DATE NULL,
    ToDate DATE NULL,
    ExpCollegeCode NVARCHAR(50) NULL;


ALTER TABLE Aff_DeanAdministrativeExperience
ADD ExpCollegeCode NVARCHAR(50) NULL,
    OtherCollege NVARCHAR(250) NULL;

ALTER TABLE Aff_PRINCIPALADMINISTRATIVEEXPERIENCE
ADD ExpCollegeCode NVARCHAR(50) NULL,
    OtherCollege NVARCHAR(250) NULL;

ALTER TABLE AFF_InstitutionsDetails
ADD GovAutonomousCertPath NVARCHAR(500) NULL,
    GovAutonomousCertNumber NVARCHAR(100) NULL;