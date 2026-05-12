

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