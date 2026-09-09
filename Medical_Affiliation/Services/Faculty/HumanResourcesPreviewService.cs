using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class HumanResourcesPreviewService : IHumanResourcesPreviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private string CollegeCode =>
            _httpContextAccessor.HttpContext?.Session.GetString("CollegeCode") ?? string.Empty;

        private string FacultyCode =>
            _httpContextAccessor.HttpContext?.Session.GetString("FacultyCode") ?? string.Empty;

        public HumanResourcesPreviewService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<HumanResourcesVM> GetHumanResourcesPreviewAsync()
        {
            var vm = new HumanResourcesVM();

            // ============================================================
            // FACULTY DETAILS
            // ============================================================

            var facultyDetails = await _context.FacultyDetails
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode &&
                    x.IsRemoved != true)
                .OrderBy(x => x.DepartmentDetails)
                .ThenBy(x => x.NameOfFaculty)
                .ToListAsync();

            // Masters
            var designationMasters = await _context.DesignationMasters
                .Where(x => x.FacultyCode.ToString() == FacultyCode)
                .ToListAsync();

            var departmentMasters = await _context.MstCourses
                .Where(x => x.FacultyCode.ToString() == FacultyCode && x.SubjectName != null)
                .ToListAsync();

            vm.FacultyDetails = facultyDetails
                .Select(x =>
                {
                    var designation = designationMasters
                        .FirstOrDefault(d => d.DesignationCode == x.Designation);

                    var department = departmentMasters
                        .FirstOrDefault(d => d.CourseCode.ToString() == x.DepartmentDetails);

                    return new FacultyDetailsPreviewVM
                    {
                        FacultyDetailId = x.Id,
                        NameOfFaculty = x.NameOfFaculty,

                        Designation = designation?.DesignationName ?? x.Designation,

                        // Subject Name instead of Course Code
                        Department = department?.SubjectName ?? x.DepartmentDetails,

                        Mobile = x.Mobile,
                        Email = x.Email,

                        RecognizedPGTeacher = x.RecognizedPgTeacher,
                        RecognizedPhDTeacher = x.RecognizedPhDteacher,

                        IsExaminer = x.IsExaminer,
                        ExaminerFor = x.ExaminerFor,

                        LitigationPending = x.LitigationPending,

                        From = x.From,
                        To = x.To,

                        HasPGRecognitionDocument =
                            !string.IsNullOrWhiteSpace(x.GuideRecognitionDocPath),

                        HasPhDRecognitionDocument =
                            !string.IsNullOrWhiteSpace(x.PhDrecognitionDocPath),

                        HasLitigationDocument =
                            !string.IsNullOrWhiteSpace(x.LitigationDocPath)
                    };
                })
                .ToList();

            // Teaching Faculty Department-wise

            // Non-Teaching Faculty Details

            return vm;
        }
    }
}
