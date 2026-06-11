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

