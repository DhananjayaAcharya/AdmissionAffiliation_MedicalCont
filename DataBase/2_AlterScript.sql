

ALTER TABLE [dbo].[Affiliation_College_Master]
ADD Status bit default 1;


UPDATE Affiliation_College_Master
set status = 0
where CollegeCode not in ('M016', 'M011', 'M019', 'Test1');

ALTER TABLE [dbo].[TblRguhsFacultyUser]
ADD IsAdmin bit default 0;

UPDATE [dbo].[TblRguhsFacultyUser]
SET IsAdmin = 1
WHERE UserName = 'SuperAdmin';



--ALTER TABLE CA_SS_Permission
--DROP COLUMN FileData;

--ALTER TABLE CA_SS_Permission
--ADD FilePath nvarchar(500) NULL;

------------------------------------------------------------

--ALTER TABLE CA_SS_AffiliationGrantedYear
--DROP COLUMN FileData;

--ALTER TABLE CA_SS_AffiliationGrantedYear
--ADD FilePath nvarchar(500) NULL;

---------------------------------------------------

--ALTER TABLE CA_SS_OtherCoursesConducted
--DROP COLUMN DocumentData;

--ALTER TABLE CA_SS_OtherCoursesConducted
--ADD DocumentPath nvarchar(500) NULL;
