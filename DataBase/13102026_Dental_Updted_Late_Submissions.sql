
UPDATE MstDentalFeeTypes
SET ActivationDate = '2026-10-23T23:59:00'
WHERE FacultyCode = 2
  AND FeeType = 'Late Submission Fee'
  AND CourseLevel IN ('UG', 'PG')
  AND AffiliationTypeId = 2;
GO

UPDATE MstDentalFeeTypes
set IsActive = 0
WHERE FeeType like '%late%'
