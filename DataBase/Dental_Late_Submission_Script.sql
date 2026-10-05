

UPDATE [Admission_Affiliation].[dbo].[CA_AcademicPerformance]
SET FacultyId = 2
WHERE CollegeCode LIKE 'D%' AND FacultyId = 1;

---------------- APPLICATION LATE SUBMISSION FEE ----


ALTER TABLE MstDentalFeeTypes
ADD ActivationDate DATETIME NULL;
GO

INSERT INTO MstDentalFeeTypes
( FacultyCode, FeeType, CourseLevel, DisplayOrder, AffiliationTypeId,IsActive, ActivationDate, CreatedBy, CreatedDate)
VALUES
( 2, 'Late Submission Fee', 'UG', 6, 2, 1, '2026-10-13T18:30:00', 'Admin', GETDATE()),
( 2, 'Late Submission Fee', 'PG', 6, 2, 1, '2026-10-13T18:30:00', 'Admin', GETDATE());
GO

-----------------------------------------------

INSERT INTO MstDentalFeeStructure
( FacultyCode, FeeTypeId, CourseName, CourseCode, CourseLevel, AmountToBePaid, CalculationType, AffiliationTypeId, IsActive, CreatedBy, CreatedDate
)
SELECT
    2,
    ft.Id,
    CASE
        WHEN ft.CourseLevel = 'UG' THEN 'BDS'
        WHEN ft.CourseLevel = 'PG' THEN 'MDS'
    END,
    NULL,
    ft.CourseLevel,
    100000,
    'Fixed',
    2,
    1,
    'Admin',
    GETDATE()
FROM MstDentalFeeTypes ft
WHERE ft.FacultyCode = 2
  AND ft.FeeType = 'Late Submission Fee'
  AND ft.CourseLevel IN ('UG', 'PG')
  AND ft.AffiliationTypeId = 2;

-----------------------TESTING WORKED--------------

--  UPDATE MstDentalFeeTypes
--SET ActivationDate = '2026-10-01T12:25:00'
--WHERE FacultyCode = 2
--  AND FeeType = 'Late Submission Fee'
--  AND CourseLevel IN ('UG', 'PG')
--  AND AffiliationTypeId = 2;

-----------------------ORIGINAL ------------------------
--UPDATE MstDentalFeeTypes
--SET ActivationDate = '2026-10-12T18:30:00'
--WHERE FacultyCode = 2
--  AND FeeType = 'Late Submission Fee'
--  AND CourseLevel IN ('UG', 'PG')
--  AND AffiliationTypeId = 2;


BEGIN TRANSACTION;

BEGIN TRY

    -- Step 1: Update existing records and set AffiliationTypeId = 2
    UPDATE ExistingRecord
    SET
        ExistingRecord.PrincipalStaffResidentialQuarter =
            ExtraRecord.PrincipalStaffResidentialQuarter,

        ExistingRecord.PrincipalStaffResidentialQuarterAreaSqFt =
            ExtraRecord.PrincipalStaffResidentialQuarterAreaSqFt,

        ExistingRecord.OtherStaffResidentialQuarter =
            ExtraRecord.OtherStaffResidentialQuarter,

        ExistingRecord.OtherStaffResidentialQuarterAreaSqFt =
            ExtraRecord.OtherStaffResidentialQuarterAreaSqFt,

        ExistingRecord.TeachingAncillaryStaffResidentialQuarter =
            ExtraRecord.TeachingAncillaryStaffResidentialQuarter,

        ExistingRecord.TeachingAncillaryStaffResidentialQuarterAreaSqFt =
            ExtraRecord.TeachingAncillaryStaffResidentialQuarterAreaSqFt,

        ExistingRecord.AffiliationTypeId = 2,
        ExistingRecord.ModifiedOn = GETDATE()

    FROM dbo.DentalCollegeLandBuildingDetail AS ExistingRecord
    INNER JOIN dbo.DentalCollegeLandBuildingDetail AS ExtraRecord
        ON ExistingRecord.CollegeCode = ExtraRecord.CollegeCode
       AND ExistingRecord.FacultyCode = ExtraRecord.FacultyCode

    WHERE ExtraRecord.FacultyCode = 2
      AND ExtraRecord.CollegeCode LIKE 'D%'
      AND ExistingRecord.Id <> ExtraRecord.Id
      AND ExistingRecord.AffiliationTypeId IS NULL;


    -- Step 2: Delete only the extra records whose existing records were updated
    DELETE ExtraRecord
    FROM dbo.DentalCollegeLandBuildingDetail AS ExtraRecord
    WHERE (ExtraRecord.AffiliationTypeId = 2
           OR ExtraRecord.AffiliationTypeId IS NULL)
      AND ExtraRecord.FacultyCode = 2
      AND ExtraRecord.CollegeCode LIKE 'D%'
      AND EXISTS
      (
          SELECT 1
          FROM dbo.DentalCollegeLandBuildingDetail AS ExistingRecord
          WHERE ExistingRecord.CollegeCode = ExtraRecord.CollegeCode
            AND ExistingRecord.FacultyCode = ExtraRecord.FacultyCode
            AND ExistingRecord.Id <> ExtraRecord.Id
            AND ExistingRecord.AffiliationTypeId = 2
            AND ExistingRecord.ModifiedOn IS NOT NULL
      );


    -- Step 3: Verify the remaining records
    SELECT
        Id,
        CollegeCode,
        FacultyCode,
        AffiliationTypeId,
        PrincipalStaffResidentialQuarter,
        PrincipalStaffResidentialQuarterAreaSqFt,
        OtherStaffResidentialQuarter,
        OtherStaffResidentialQuarterAreaSqFt,
        TeachingAncillaryStaffResidentialQuarter,
        TeachingAncillaryStaffResidentialQuarterAreaSqFt,
        ModifiedOn
    FROM dbo.DentalCollegeLandBuildingDetail
    WHERE FacultyCode = 2
      AND CollegeCode LIKE 'D%'
    ORDER BY CollegeCode, Id;


    COMMIT TRANSACTION;

END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;


-----------------------------

UPDATE dbo.DentalCollegeLandBuildingDetail
SET
    AffiliationTypeId = 2,
    ModifiedOn = GETDATE()
WHERE AffiliationTypeId IS NULL
  AND FacultyCode = 2
  AND CollegeCode LIKE 'D%';