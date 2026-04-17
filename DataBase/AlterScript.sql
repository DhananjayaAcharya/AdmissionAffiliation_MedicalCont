

ALTER TABLE [dbo].[TblRguhsFacultyUser]
ADD IsFinance BIT NULL;

ALTER TABLE [dbo].[TblRguhsFacultyUser]
ADD FinanceDesignation VARCHAR(10) NULL;


ALTER TABLE [dbo].[TblRguhsFacultyUser]
ADD DesignationDescription VARCHAR(200) NULL;

ALTER TABLE [dbo].[TblRguhsFacultyUser]
ADD  IsSection BIT NULL;
-------------------------------------------

ALTER TABLE [dbo].[MedicalInstituteDetail]
ADD Taluk NVARCHAR(50) NULL;

ALTER TABLE [dbo].[MedicalInstituteDetail]
ADD District NVARCHAR(50) NULL;

---------------------------------------------
ALTER TABLE [dbo].[HospitalDetailsForAffiliation]
ADD CourseLevel NVARCHAR(20) NULL;

ALTER TABLE [dbo].[AffiliatedHospitalDocuments]
ADD CourseLevel NVARCHAR(20) NULL;

ALTER TABLE [dbo].[HospitalFacilities]
ADD CourseLevel NVARCHAR(20) NULL;

ALTER TABLE [dbo].[AFF_InstitutionsDetails]
ADD CourseLevel NVARCHAR(20) NULL;

--------------------------------------

ALTER TABLE [dbo].[CA_StudentRegisterRecords]
ALTER COLUMN IsMaintained BIT;


--------------------------------------------


INSERT INTO [dbo].[MST_Hospital_Type]
(FacultyCode, HospitalType)
VALUES
(1, 'Own'),
(1, 'Parent');

--------------------------------------------------
INSERT INTO  [dbo].[MST_HospitalOwnedBy]
(FacultyCode, OwnedBy)
VALUES
(1, 'Trust/Society/Missionary'),
(1, 'Trust member with MOU'),
(1, 'District Hospital'),
(1, 'Taluk hospital/Community Health Center');

---------------------------------------------------

ALTER TABLE [dbo].[CA_AcademicPerformance]
ADD CONSTRAINT FK_AcademicPerformance_Year
FOREIGN KEY (YearOfStudyId)
REFERENCES [dbo].[CA_MST_YearOfStudy](YearOfStudyId);

ALTER TABLE CA_ExaminationScheme
ADD CONSTRAINT FK_ExamScheme_Scheme
FOREIGN KEY (SchemeId)
REFERENCES CA_MST_ExaminationScheme(SchemeId);


ALTER TABLE CA_StudentRegisterRecords
ADD CONSTRAINT FK_StudentRegister_Record
FOREIGN KEY (RegisterRecordId)
REFERENCES CA_MST_RegisterRecord(RegisterRecordId);

ALTER TABLE [dbo].[CA_CourseCurriculum]
ADD CONSTRAINT FK_CourseCurriculum_Master
FOREIGN KEY ([CurriculumId])
REFERENCES [dbo].[CA_MST_CourseCurriculum](CurriculumId);

----------------------------------------------------------

USE [Admission_Affiliation]
GO

/****** Object:  Table [dbo].[CA_Progress]    Script Date: 13-04-2026 16:13:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[CA_Progress](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CollegeCode] [nvarchar](50) NULL,
	[CourseLevel] [nvarchar](10) NULL,
	[StepKey] [nvarchar](100) NULL,
	[IsCompleted] [bit] NULL,
	[UpdatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_CA] UNIQUE NONCLUSTERED 
(
	[CollegeCode] ASC,
	[CourseLevel] ASC,
	[StepKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[CA_Progress] ADD  DEFAULT ((1)) FOR [IsCompleted]
GO

ALTER TABLE [dbo].[CA_Progress] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO

--------------------------------------------


ALTER TABLE [dbo].[TeachingStaffDepartmentWiseDetails]
ADD UGCollegeCode NVARCHAR(10) NULL;


ALTER TABLE [dbo].[TeachingStaffDepartmentWiseDetails]
ADD PGCollegeCode NVARCHAR(10) NULL;