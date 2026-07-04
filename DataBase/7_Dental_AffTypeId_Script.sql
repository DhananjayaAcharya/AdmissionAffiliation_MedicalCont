ALTER TABLE DentalCollegeLandBuildingDetail
ADD Latitude  decimal(9,6) NULL,
    Longitude decimal(9,6) NULL;

ALTER TABLE DentalCollegeLandBuildingDetail
ADD CourseLevel VARCHAR(10) NULL;

UPDATE DentalCollegeLandBuildingDetail
SET CourseLevel = 'UG';
------------------------------

ALTER TABLE DentalCollegeLandBuildingDetail
ADD AffiliationTypeId INT NULL;

UPDATE DentalCollegeLandBuildingDetail
SET AffiliationTypeId = 2;

ALTER TABLE DentalCollegeLandBuildingDetail
ADD CONSTRAINT FK_DentalCollegeLandBuildingDetail_TypeOfAffiliation
FOREIGN KEY (AffiliationTypeId)
REFERENCES TypeOfAffiliation(TypeId);

----------------------

ALTER TABLE DentalCollegeLandBuildingDetail
DROP CONSTRAINT UQ_DentalCollegeLandBuildingDetail;

ALTER TABLE DentalCollegeLandBuildingDetail
ADD CONSTRAINT UQ_DentalCollegeLandBuildingDetail
UNIQUE (CollegeCode, FacultyCode, AffiliationTypeId, CourseLevel);

--------------------------

ALTER TABLE Medical_SkillsLaboratory
ADD AffiliationTypeId INT NULL;

ALTER TABLE Medical_SkillsLaboratory
ADD CONSTRAINT FK_Medical_SkillsLaboratory_TypeOfAffiliation
FOREIGN KEY (AffiliationTypeId)
REFERENCES TypeOfAffiliation(TypeId);

UPDATE Medical_SkillsLaboratory
SET AffiliationTypeId = 2
WHERE AffiliationTypeId IS NULL;

--------------------------------

ALTER TABLE DentalChairs
ADD AffiliationTypeId INT NULL;

ALTER TABLE DentalChairs
ADD CONSTRAINT FK_DentalChairs_TypeOfAffiliation
FOREIGN KEY (AffiliationTypeId)
REFERENCES TypeOfAffiliation(TypeId);

UPDATE DentalChairs
SET AffiliationTypeId = 2
WHERE AffiliationTypeId IS NULL;

--------------------------------


USE [Admission_Affiliation]
GO
/****** Object:  Table [dbo].[AcademicIntakeYearWise]    Script Date: 7/4/2026 12:05:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcademicIntakeYearWise](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CollegeCode] [nvarchar](20) NOT NULL,
	[FacultyCode] [nvarchar](20) NOT NULL,
	[CourseCode] [nvarchar](20) NOT NULL,
	[AcademicYear] [nvarchar](20) NOT NULL,
	[ExistingIntake] [int] NULL,
	[AdditionalIntake] [int] NULL,
	[TotalIntake] [int] NULL,
	[ApprovalType] [nvarchar](50) NULL,
	[LopDate] [date] NULL,
	[DocumentPath] [nvarchar](500) NULL,
	[CreatedDate] [datetime] NULL,
	[ModifiedDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcademicYearMaster]    Script Date: 7/4/2026 12:05:41 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcademicYearMaster](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AcademicYear] [nvarchar](20) NULL,
	[IsActive] [bit] NULL,
	[DisplayOrder] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[AcademicYearMaster] ON 
GO
INSERT [dbo].[AcademicYearMaster] ([Id], [AcademicYear], [IsActive], [DisplayOrder]) VALUES (4, N'AY 2024-25', 1, 1)
GO
INSERT [dbo].[AcademicYearMaster] ([Id], [AcademicYear], [IsActive], [DisplayOrder]) VALUES (5, N'AY 2025-26', 1, 2)
GO
INSERT [dbo].[AcademicYearMaster] ([Id], [AcademicYear], [IsActive], [DisplayOrder]) VALUES (6, N'AY 2026-27', 1, 3)
GO
INSERT [dbo].[AcademicYearMaster] ([Id], [AcademicYear], [IsActive], [DisplayOrder]) VALUES (7, N'AY 2027-28', 1, 4)
GO
INSERT [dbo].[AcademicYearMaster] ([Id], [AcademicYear], [IsActive], [DisplayOrder]) VALUES (8, N'AY 2028-29', 0, 5)
GO
SET IDENTITY_INSERT [dbo].[AcademicYearMaster] OFF
GO
ALTER TABLE [dbo].[AcademicIntakeYearWise] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO
ALTER TABLE [dbo].[AcademicYearMaster] ADD  DEFAULT ((1)) FOR [IsActive]
GO

-------------------

ALTER TABLE DentalInfrastructure
ADD CourseLevel VARCHAR(10) NULL;

UPDATE DentalInfrastructure
SET CourseLevel = 'UG';


ALTER TABLE DentalInfrastructure
ADD CONSTRAINT UQ_DentalInfrastructure
UNIQUE
(
    CollegeCode,
    FacultyCode,
    AffiliationTypeId,
    CourseLevel,
    RequirementId,
    SeatSlab
);
---------------------------------

delete from [dbo].[DentalInfrastructure] 
where CollegeCode = 'd038' and AffiliationTypeId = 2 and courselevel = 'pg'

EXEC sp_helpindex '[DentalCollegeLandBuildingDetail]';

EXEC sp_helpindex 'DentalInfrastructure';


SELECT
    CollegeCode,
    FacultyCode,
    AffiliationTypeId,
    CourseLevel,
    RequirementId,
    SeatSlab,
    COUNT(*) AS DuplicateCount
FROM DentalInfrastructure
GROUP BY
    CollegeCode,
    FacultyCode,
    AffiliationTypeId,
    CourseLevel,
    RequirementId,
    SeatSlab
HAVING COUNT(*) > 1;


SELECT *
FROM DentalInfrastructure
WHERE CollegeCode = 'D038'
  AND FacultyCode = 2
  AND AffiliationTypeId = 2
  AND CourseLevel = 'PG'
  AND RequirementId = 2
  AND SeatSlab = 100
ORDER BY Id;

DELETE FROM DentalInfrastructure
WHERE Id = 1065;