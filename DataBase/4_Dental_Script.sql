CREATE TABLE AffiliationOthersCollegeMaster
(
    Id INT IDENTITY(1,1) PRIMARY KEY,

    CollegeCode VARCHAR(20) NOT NULL UNIQUE, -- OTH001, OTH002...

    FacultyCode INT NOT NULL,

    CollegeName NVARCHAR(500) NOT NULL,

    CollegeTown NVARCHAR(250) NOT NULL,

    StateName NVARCHAR(250) NULL,

    DistrictName NVARCHAR(250) NULL,

    TalukName NVARCHAR(250) NULL,

    CreatedDate DATETIME NOT NULL DEFAULT(GETDATE()),

    IsActive BIT NOT NULL DEFAULT(1)
);

ALTER TABLE AffiliationOthersCollegeMaster
ADD CONSTRAINT FK_AffiliationOthersCollegeMaster_Faculty
FOREIGN KEY (FacultyCode)
REFERENCES Faculty(FacultyId);


CREATE TABLE VehicleRequestLog
(
    Id INT IDENTITY PRIMARY KEY,
    CollegeCode VARCHAR(20),
    VehicleRegNo VARCHAR(50),
    RequestTime DATETIME
)

ALTER TABLE Aff_DeanTeachingExperience
ADD UgCollegeCode VARCHAR(20) NULL,
    PgCollegeCode VARCHAR(20) NULL;

     ALTER TABLE Aff_PrincipalTeachingExperience
ADD UgCollegeCode VARCHAR(20) NULL,
    PgCollegeCode VARCHAR(20) NULL;

ALTER TABLE [dbo].[AFF_HostelDetails]
ADD MenHostelAreaSqFt VARCHAR(50) NULL,
    WomenHostelAreaSqFt VARCHAR(50) NULL;

------------

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


    --------------

    SELECT * FROM AffiliationOthersCollegeMaster

    --DELETE FROM AffiliationOthersCollegeMaster where id = 15

    SELECT * FROM [dbo].[TeachingStaffDepartmentWiseDetails]
    WHERE CollegeCode = 'd038'

    SELECT * FROM DesignationMaster WHERE FacultyCode=2

    SELECT * FROM DepartmentMaster WHERE FacultyCode = 2 ORDER BY DepartmentName

    SELECT * FROM DepartmentWiseFacultyMaster WHERE FacultyCode=2

    SELECT * FROM SeatSlabMaster WHERE FacultyCode=2