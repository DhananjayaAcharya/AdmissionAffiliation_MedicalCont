ALTER TABLE HospitalDetailForAffiliation
ADD parentHospitalExists BIT NULL;

ALTER TABLE HospitalDetailForAffiliation
ALTER COLUMN HospitalParentSupportingDoc NVARCHAR(500);

ALTER TABLE [dbo].[HealthCenterCHP]
ALTER COLUMN [ServicesRendered] VARCHAR(500);

ALTER TABLE HealthCenterCHP
ALTER COLUMN FieldType VARCHAR(50);


ALTER TABLE [dbo].[AcademicMatter]
ALTER COLUMN [AntiRaggingCommitteeFile] VARCHAR(255) NULL;

ALTER TABLE [dbo].[AcademicMatter]
ALTER COLUMN [CEU_MembersFile] VARCHAR(255) NULL;

ALTER TABLE [dbo].[AcademicMatter]
ALTER COLUMN [CEU_ProgramsFile] VARCHAR(255) NULL;

ALTER TABLE [dbo].[AcademicMatter]
ALTER COLUMN [CEU_MembersFile] VARCHAR(255) NULL;

ALTER TABLE [dbo].[AcademicMatter]
ALTER COLUMN [AcademicCommitteeFile] VARCHAR(255) NULL;

ALTER TABLE [dbo].[AcademicMatter]
ALTER COLUMN [IndexedJournalsFile] VARCHAR(255) NULL;

ALTER TABLE [dbo].[AcademicMatter]
ALTER COLUMN [FundedStaffListFile] VARCHAR(255) NULL;