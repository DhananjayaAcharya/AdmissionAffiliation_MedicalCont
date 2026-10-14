USE [Admission_Affiliation];
GO



ALTER TABLE [Admission_Affiliation].[dbo].[FinalDentalSubmission]
ADD IsLocked BIT NOT NULL
CONSTRAINT DF_FinalDentalSubmission_IsLocked DEFAULT (0);


select* from FinalDentalSubmission
where CollegeCode = 'd038'