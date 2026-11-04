
USE Admission_Affiliation;

DELETE FROM DentalConferencesAttended
WHERE IsActive = 0 AND CollegeCode = 'D006'

UPDATE CA_AcademicPerformance
SET CourseLevel = 'UG'
WHERE FacultyId = 2 and CollegeCode = 'd010' AND CourseLevel IS NULL