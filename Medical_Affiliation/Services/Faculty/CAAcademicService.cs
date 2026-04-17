using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAAcademicService : ICAAcademicService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CAAcademicService(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<CA_Aff_AcademicMattersViewModel> GetAcademicMattersAsync()
        {
            var facultyId = _userContext.FacultyId;
            var collegeCode = _userContext.CollegeCode;
            var academicRows = await _context.CaAcademicPerformances
                .AsNoTracking()
                .Where(x => x.FacultyId == facultyId && x.CollegeCode == collegeCode)
                .Select(x => new AcademicPerformanceViewModel
                {
                    YearName = x.YearOfStudy != null ? x.YearOfStudy.YearName : null,
                    RegularStudents = x.RegularStudents,
                    RepeaterStudents = x.RepeaterStudents,
                    NumberOfStudentsPassed = x.NumberOfStudentsPassed,
                    PassPercentage = x.PassPercentage ?? 0,
                    FirstClassCount = x.FirstClassCount,
                    DistinctionCount = x.DistinctionCount,
                    Remarks = x.Remarks
                })
                .ToListAsync();

            var curriculums = await _context.CaCourseCurricula
                .AsNoTracking()
                .Where(x => x.FacultyId == facultyId && x.CollegeCode == collegeCode)
                .Select(x => new CourseCurriculumDisplayViewModel
                {
                    CourseCurriculumId = x.CourseCurriculumId,
                    CurriculumId = x.CurriculumId,
                    CurriculumName = x.Curriculum != null ? x.Curriculum.CurriculumName : null,
                    CurriculumDetails = x.CurriculumDetails,
                    FileName = x.PdfFileName,
                    FileId = x.CourseCurriculumId,
                    HasPdf = x.CurriculumPdfPath != null && x.CurriculumPdfPath.Length > 0,
                })
                .ToListAsync();

            var examSchemes = await _context.CaExaminationSchemes
                .AsNoTracking()
                .Where(x => x.FacultyId == facultyId && x.CollegeCode == collegeCode)
                .Select(x => new ExaminationSchemeViewModel
                {
                    SchemeCode = x.Scheme != null ? x.Scheme.SchemeCode : null,
                    NumberOfStudents = x.NumberOfStudents
                })
                .ToListAsync();

            var studentRecords = await _context.CaStudentRegisterRecords
                .AsNoTracking()
                .Where(x => x.FacultyId == facultyId && x.CollegeCode == collegeCode)
                .Select(x => new StudentRegisterRecordViewModel
                {
                    RegisterName = x.RegisterRecordNavigation != null ? x.RegisterRecordNavigation.RegisterName : "N/A",
                    IsExists = x.IsMaintained == true
                })
                .ToListAsync();

            return new CA_Aff_AcademicMattersViewModel
            {
                CollegeCode = collegeCode,
                FacultyId = facultyId,
                AcademicRows = academicRows,
                CourseCurriculumdvm = curriculums,
                ExaminationSchemes = examSchemes,
                StudentRegisterRecords = studentRecords
            };
        }



    }

}
