SELECT
    Id,
    CollegeCode,
    FacultyCode,
    AffiliationTypeId,
    CourseLevel
FROM dbo.DentalCollegeLandBuildingDetail
WHERE CourseLevel = 'UG'
  AND CollegeCode IN
  (
      SELECT CollegeCode
      FROM dbo.DentalCollegeLandBuildingDetail
      WHERE CourseLevel = 'UG'
      GROUP BY CollegeCode, FacultyCode
      HAVING
          COUNT(CASE WHEN AffiliationTypeId IS NULL THEN 1 END) > 0
          AND
          COUNT(CASE WHEN AffiliationTypeId IS NOT NULL THEN 1 END) > 0
  )
ORDER BY CollegeCode, FacultyCode, AffiliationTypeId, Id;

------------------------ COPY THE RECORDS -------------

UPDATE T
SET
    T.PrincipalStaffResidentialQuarter = S.PrincipalStaffResidentialQuarter,
    T.PrincipalStaffResidentialQuarterAreaSqFt = S.PrincipalStaffResidentialQuarterAreaSqFt,
    T.OtherStaffResidentialQuarter = S.OtherStaffResidentialQuarter,
    T.OtherStaffResidentialQuarterAreaSqFt = S.OtherStaffResidentialQuarterAreaSqFt,
    T.TeachingAncillaryStaffResidentialQuarter = S.TeachingAncillaryStaffResidentialQuarter,
    T.TeachingAncillaryStaffResidentialQuarterAreaSqFt = S.TeachingAncillaryStaffResidentialQuarterAreaSqFt
FROM dbo.DentalCollegeLandBuildingDetail T
INNER JOIN dbo.DentalCollegeLandBuildingDetail S
    ON S.Id = CASE T.Id
        WHEN 1023 THEN 1041
        WHEN 1022 THEN 1038
        WHEN 1016 THEN 1055
        WHEN 1026 THEN 1052
        WHEN 1009 THEN 1042
        WHEN 1020 THEN 1054
        WHEN 1013 THEN 1040
        WHEN 1029 THEN 1044
    END
WHERE T.Id IN (
    1023, 1022, 1016, 1026,
    1009, 1020, 1013, 1029
);

---------------------------------------------

SELECT
    Id,
    CollegeCode,
    FacultyCode,
    AffiliationTypeId,
    CourseLevel,
    PrincipalStaffResidentialQuarter,
    PrincipalStaffResidentialQuarterAreaSqFt,
    OtherStaffResidentialQuarter,
    OtherStaffResidentialQuarterAreaSqFt,
    TeachingAncillaryStaffResidentialQuarter,
    TeachingAncillaryStaffResidentialQuarterAreaSqFt
FROM dbo.DentalCollegeLandBuildingDetail
WHERE Id IN (
    1023,1041,
    1022,1038,
    1016,1055,
    1026,1052,
    1009,1042,
    1020,1054,
    1013,1040,
    1029,1044
)
ORDER BY CollegeCode, Id;

---------------------------------------------------

DELETE FROM dbo.DentalCollegeLandBuildingDetail
WHERE Id IN (
    1041,
    1038,
    1055,
    1052,
    1042,
    1054,
    1040,
    1044
);

-----------------------------------------

UPDATE dbo.DentalCollegeLandBuildingDetail
SET AffiliationTypeId = 2
WHERE Id IN (
    1023,
    1022,
    1016,
    1026,
    1009,
    1020,
    1013,
    1029
);

--------------------------------------------