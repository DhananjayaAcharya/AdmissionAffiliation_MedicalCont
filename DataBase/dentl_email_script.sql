UPDATE dbo.Affiliation_College_Master
SET CollegeEmail = CASE CollegeCode

    WHEN 'D001' THEN 'den.d405@rguhs.ac.in'
    WHEN 'D002' THEN 'den.d701@rguhs.ac.in'
    WHEN 'D003' THEN 'den.d301@rguhs.ac.in'
    WHEN 'D004' THEN 'den.d501@rguhs.ac.in'
    WHEN 'D005' THEN 'den.d016@rguhs.ac.in'
    WHEN 'D006' THEN 'den.d426@rguhs.ac.in'
    WHEN 'D008' THEN 'den.d427@rguhs.ac.in'
    WHEN 'D009' THEN 'den.d576@rguhs.ac.in'
    WHEN 'D010' THEN 'den.d010@rguhs.ac.in'
    WHEN 'D011' THEN 'den.d003@rguhs.ac.in'
    WHEN 'D012' THEN 'den.d503@rguhs.ac.in'
    WHEN 'D013' THEN 'den.d676@rguhs.ac.in'
    WHEN 'D014' THEN 'den.d251@rguhs.ac.in'
    WHEN 'D015' THEN 'den.d001@rguhs.ac.in'
    WHEN 'D016' THEN 'den.d277@rguhs.ac.in'
    WHEN 'D017' THEN 'den.d502@rguhs.ac.in'
    WHEN 'D018' THEN 'den.d601@rguhs.ac.in'
    WHEN 'D019' THEN 'den.d004@rguhs.ac.in'
    WHEN 'D020' THEN 'den.d005@rguhs.ac.in'
    WHEN 'D021' THEN 'den.d403@rguhs.ac.in'
    WHEN 'D022' THEN 'den.d017@rguhs.ac.in'
    WHEN 'D023' THEN 'den.d227@rguhs.ac.in'
    WHEN 'D024' THEN 'den.d008@rguhs.ac.in'
    WHEN 'D025' THEN 'den.d702@rguhs.ac.in'
    WHEN 'D026' THEN 'den.d009@rguhs.ac.in'
    WHEN 'D027' THEN 'den.d201@rguhs.ac.in'
    WHEN 'D028' THEN 'den.d011@rguhs.ac.in'
    WHEN 'D029' THEN 'den.d276@rguhs.ac.in'
    WHEN 'D030' THEN 'den.d726@rguhs.ac.in'
    WHEN 'D031' THEN 'den.d376@rguhs.ac.in'
    WHEN 'D032' THEN 'den.d526@rguhs.ac.in'
    WHEN 'D033' THEN 'den.d012@rguhs.ac.in'
    WHEN 'D034' THEN 'den.d406@rguhs.ac.in'
    WHEN 'D035' THEN 'den.d727@rguhs.ac.in'
    WHEN 'D036' THEN 'den.d677@rguhs.ac.in'
    WHEN 'D037' THEN 'den.d018@rguhs.ac.in'
    WHEN 'D038' THEN 'testtest@rguhs.ac.in'
    WHEN 'D039' THEN 'testtest@rguhs.ac.in'
    WHEN 'D040' THEN 'testtest@rguhs.ac.in'

END
WHERE FacultyCode = '2'
AND CollegeCode IN
(
    'D001','D002','D003','D004','D005',
    'D006','D008','D009','D010','D011',
    'D012','D013','D014','D015','D016',
    'D017','D018','D019','D020','D021',
    'D022','D023','D024','D025','D026',
    'D027','D028','D029','D030','D031',
    'D032','D033','D034','D035','D036',
    'D037','D038','D039','D040'
);