
INSERT INTO CA_MST_VD_VehicleFor
(FacultyCode, VehicleForCode, VehicleForName)
VALUES
(2,	'INT',	'For Interns'),
(2,	'STU', 'For Students'),
(2,	'AMB',	'Ambulance');


--------------------------------------

SELECT * FROM CA_MST_LibraryEquipmentsType;

INSERT INTO CA_MST_Med_LibraryItems
(FacultyCode, ItemName)
values
(2,	'Books'),
(2,	'Current Journals (No. of Titles)'),
(2,	'Bound Volumes of Journals'),
(2,	'Monographs'),
(2,	'Govt. Publications'),
(2,	'Thesis / Dissertation'),
(2,	'Number of computers available'),
(2,	'e-journals'),
(2,	'e-books');

------------------------------------

INSERT INTO CA_MST_LibraryEquipmentsType
(FacultyCode, TypeOfEquipment)
VALUES
(2,	'Type of Computer'),
(2,	'E-Mail'),
(2,	'Connected to any network'),
(2,	'Photocopying Machine'),
(2,	'Microfilm reader'),
(2,	'Audio Visual'),
(2,	'Telephone'),
(2,	'Telex'),
(2,	'Fax'),
(2,	'Bindery');

---------------------------

select * from CA_MST_Med_LibTechnicalProcess;

INSERT INTO CA_MST_Med_LibTechnicalProcess
(FacultyCode, ProcessName)
VALUES
(2,	'Classification scheme used'),
(2,	'Subject Headings used'),
(2,	'Cataloguing Code used'),
(2,	'Type of Catalogue used');

---------------------------------------

SELECT * FROM MedicalAlliedDisciplineDetail

 
Select * from [dbo].[CA_MST_Med_LibraryEquipments];

INSERT INTO [CA_MST_Med_LibraryEquipments]
(FacultyCode, EquipmentName)
VALUES
(2,	'Photocopying Machine'),
(2,	'Audio Visual');

--------------------------------