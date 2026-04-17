SELECT TOP (1000) [YearOfStudyId]
      ,[YearName]
  FROM [Admission_Affiliation].[dbo].[CA_MST_YearOfStudy]

  SELECT * FROM [dbo].[CA_AcademicPerformance];

SELECT 
    tc.CONSTRAINT_NAME,
    tc.CONSTRAINT_TYPE,
    kcu.COLUMN_NAME
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
    ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
WHERE tc.TABLE_NAME = 'CA_AcademicPerformance';

SELECT * FROM [dbo].[CA_MST_RegisterRecord];


SELECT * FROM [dbo].[MST_Hospital_Type];

SELECT * FROM [dbo].[MST_HospitalOwnedBy];

SELECT * FROM [dbo].[HospitalDetailForAffiliation];

SELECT * FROM [dbo].[HealthCenterCHP];

DELETE FROM HealthCenterCHP;

delete from [dbo].[HospitalDetailForAffiliation];

SELECT * FROM [dbo].[Mst_Administration];

SELECT * FROM [dbo].[Mst_FieldType_CHP];

SELECT * FROM [dbo].[Mst_FPA_AdopAff_Type];

SELECT * FROM [dbo].[Affiliation_Payment];

SELECT * FROM [dbo].[AcademicMatter];

SELECT * FROM [dbo].[FacultyDetail];

SELECT * FROM CA_Progress;

SELECT * FROM [dbo].[AFF_InstitutionsDetails];

SELECT * FROM SeatSlabMaster; 

SELECT* FROM MST_IndoorBedsDepartmentMaster;

SELECT * FROM MST_IndoorBedsOccupancyMaster;

DELETE FROM [dbo].[AffiliationFinalDeclarations];

EXEC sp_help '[dbo].[CA_AcademicPerformance]'

use AffiliationDB

SELECT* FROM [dbo].[AffiliationCollegeMaster] WHERE FacultyCode = 3 order by collegecode;