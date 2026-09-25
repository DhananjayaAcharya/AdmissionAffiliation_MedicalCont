 
 
UPDATE CA_CourseCurriculum
SET CourseLevel = 'UG'
where FacultyId = 2 and CourseLevel is null;

UPDATE [DentalCollegeLandBuildingDetail]
SET CourseLevel = 'UG'
where FacultyCode = 2 and CourseLevel is null;

UPDATE DentalInfrastructure
SET CourseLevel = 'UG' 
where FacultyCode = 2  and CourseLevel is null;

UPDATE AFF_HostelDetails
SET CourseLevel = 'UG' 
where FacultyCode = 2  and CourseLevel is null;


UPDATE Medical_DepartmentOfficesMeu
SET CourseLevel = 'UG' 
where FacultyCode = 2  and CourseLevel is null;