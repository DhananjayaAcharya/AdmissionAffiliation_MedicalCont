

--UPDATE DepartmentMaster
--SET DepartmentName = 'Orthodontics'
--WHERE DepartmentCode = 'DE006' AND FacultyCode = 2;


--UPDATE DepartmentMaster
--SET DepartmentName = 'Oral Pathology and Oal Micrology'
--WHERE DepartmentCode = 'DE008' AND FacultyCode = 2;

--INSERT INTO DepartmentMaster
--(FacultyCode, DepartmentCode, DepartmentName)
--VALUES
--(2, 'DE010', 'Dental Materials'),
--(2, 'DE011', 'Dental Anatomy, Embryology and Oral Histology');


----DELETE FROM DepartmentMaster WHERE FacultyCode=2 AND DepartmentCode='DE011'

----------------------------------

SELECT * FROM [dbo].[Affiliation_College_Master]
where facultycode = 1

SELECT * FROM DesignationMaster WHERE FacultyCode=2

--DELETE FROM DesignationMaster WHERE Id IN (61, 62, 63, 64, 65)

INSERT INTO DesignationMaster
(FacultyCode, DesignationCode, DesignationName, DesignationOrder)
VALUES
(2, 'D015', 'Professor and Head', 6),
(2, 'D016', 'Principal', 7),
(2, 'DO17', 'Vice Principal', 8);

------------------------------------------

ALTER TABLE [dbo].[Medical_SkillsLaboratory]
ADD AnnualBdsIntake INT NULL;

ALTER TABLE [Medical_SkillsLaboratory]
ALTER COLUMN AnnualMbbsIntake INT NULL;

---------------------------------------

CREATE TABLE DeptWisePublications
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CollegeCode VARCHAR(20) NOT NULL,
    FacultyCode INT NOT NULL,
    DeptCode VARCHAR(50) NOT NULL,
    DeptName NVARCHAR(200) NOT NULL,
    PublicationsCount INT NOT NULL DEFAULT 0,
    PublicationPath NVARCHAR(500) NULL,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedDate DATETIME NULL,
    CONSTRAINT FK_DeptWisePublications_Faculty
        FOREIGN KEY (FacultyCode)
        REFERENCES Faculty(FacultyId)
);


--ALTER TABLE DeptWisePublications
--ALTER COLUMN DeptCode VARCHAR(50) NOT NULL;


SELECT * FROM DeptWisePublications