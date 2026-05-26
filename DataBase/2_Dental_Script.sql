

--------- DENTAL SCRIPT 2 ----------
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

INSERT INTO [dbo].[DepartmentMaster]
(FacultyCode, DepartmentCode, DepartmentName)
VALUES
(2, 'DE001', 'Oral Medicine & Radiology'),
(2, 'DE002', 'Oral & Maxillofacial Surgery'),
(2, 'DE003', 'Conservative Dentistry & Endodontics'),
(2, 'DE004', 'Prosthodontics & Crown & Bridge'),
(2, 'DE005', 'Periodontology'),
(2, 'DE006', 'Orthodontics & Dentofacial Orthopedics'),
(2, 'DE007', 'Pedodontics & Preventive Dentistry'),
(2, 'DE008', 'Oral Pathology & Microbiology'),
(2, 'DE009', 'Public Health Dentistry');

-----------------

INSERT INTO [dbo].[Med_CA_MST_StaffDesignation]
(FacultyCode, Designation)
VALUES
(2, 'Professor'),
(2, 'Reader/Associate'),
(2, 'Lecturer/Assistant Professor');