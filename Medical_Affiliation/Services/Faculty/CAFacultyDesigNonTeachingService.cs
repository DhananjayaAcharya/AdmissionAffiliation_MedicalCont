using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAFacultyDesigNonTeachingService:ICAFacultyDesigNonTeaching
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CAFacultyDesigNonTeachingService(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<FacultyDesigNonTeachDisplayVM> GetFacultyDesigNonTeachingAsync()
        {
            var collegeCode = _userContext.CollegeCode;

            return new FacultyDesigNonTeachDisplayVM
            {
                collegeCode = collegeCode,
                FacultyDetailDisplayVM = await GetFacultyDetails(),
                CollegeDesignationDisplayVM = await GetCollegeDesignationDetails(),
                NonTeachingStaffSectionVM =  new NonTeachingStaffSectionVM
                {
                    Staffs = await GetNonTeachingStaffDetailsAsync()
                }
            };
        }


        public async Task<List<FacultyDetailDisplayVM>> GetFacultyDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyId = _userContext.FacultyId;

            var data = await (
                from f in _context.FacultyDetails
                join d in _context.DesignationMasters
                    on new { Code = f.Designation, Faculty = facultyId }
                    equals new { Code = d.DesignationCode, Faculty = d.FacultyCode }
                    into desig
                from d in desig.DefaultIfEmpty()

                join c in _context.MstCourses
                    on f.DepartmentDetails equals c.CourseCode.ToString()
                    into course
                from c in course.DefaultIfEmpty()

                where f.CollegeCode == collegeCode
                      && f.FacultyCode == facultyId.ToString()
                      && (f.IsRemoved == null || f.IsRemoved == false)

                orderby d.DesignationOrder, f.NameOfFaculty

                select new FacultyDetailDisplayVM
                {
                    NameOfFaculty = f.NameOfFaculty,
                    Subject = c != null ? c.SubjectName : f.Subject,
                    Course = c.CourseName.Trim(),
                    Designation = d != null ? d.DesignationName : f.Designation,

                    RecognizedPgTeacher = f.RecognizedPgTeacher,
                    RecognizedPhDteacher = f.RecognizedPhDteacher,
                    LitigationPending = f.LitigationPending,

                    Mobile = f.Mobile,
                    Email = f.Email,
                    DepartmentDetails = f.DepartmentDetails,

                    HasGuideRecognitionDoc = f.GuideRecognitionDocPath != null,
                    HasPhDRecognitionDoc = f.PhDrecognitionDocPath != null,
                    HasLitigationDoc = f.LitigationDocPath != null
                }
            ).ToListAsync();

            return data;
        }

        public async Task<List<CollegeDesignationDepartmentGroupVM>> GetCollegeDesignationDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyId = _userContext.FacultyId;

            var data = await (from cd in _context.CollegeDesignationDetails
                              join ss in _context.SeatSlabMasters
                              on cd.SeatSlabId equals ss.SeatSlabId into seatSlab

                              from ss in seatSlab.DefaultIfEmpty()   // LEFT JOIN (safe)
                              where cd.CollegeCode == collegeCode
                                 && cd.FacultyCode == facultyId.ToString()
                              orderby cd.Department, cd.Designation
                              select new CollegeDesignationDisplayVM
                              {
                                  Designation = cd.Designation,
                                  DesignationCode = cd.DesignationCode,
                                  Department = cd.Department,
                                  DepartmentCode = cd.DepartmentCode,
                                  SeatSlabId = cd.SeatSlabId, // or ss.SeatSlabName if needed
                                  SeatSlab = ss.SeatSlab,
                                  RequiredIntake = cd.RequiredIntake,
                                  AvailableIntake = cd.AvailableIntake
                              }
                             ).ToListAsync();


            return data
                .GroupBy(x => x.Department)
                .Select(g => new CollegeDesignationDepartmentGroupVM
                {
                    Department = g.Key,
                    Designations = g.ToList()
                })
                .ToList();
        }

        public async Task<List<NonTeachingStaffDisplayVM>> GetNonTeachingStaffDetailsAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId.ToString();

            var data = await (
             from s in _context.AffNonTeachingStaffs

             join d in _context.DesignationMasters
                 on new
                 {
                     Code = s.Designation.Trim(),
                     Faculty = s.FacultyCode.Trim()
                 }
                 equals new
                 {
                     Code = d.DesignationCode.Trim(),
                     Faculty = d.FacultyCode.ToString()
                 }
                 into desig
                from d in desig.DefaultIfEmpty()

                where s.CollegeCode == collegeCode
                    && s.FacultyCode == facultyCode

                orderby d.DesignationOrder, s.StaffName

                select new NonTeachingStaffDisplayVM
                {
                    StaffName = s.StaffName,

                    Designation = d != null
                        ? d.DesignationName
                        : s.Designation, // fallback is IMPORTANT

                    MobileNumber = s.MobileNumber,
                    SalaryPaid = s.SalaryPaid,

                    PfProvided = s.PfProvided,
                    EsiProvided = s.EsiProvided,
                    ServiceRegisterMaintained = s.ServiceRegisterMaintained,
                    SalaryAcquaintanceRegister = s.SalaryAcquaintanceRegister
                }
            ).ToListAsync();

            return data;
        }

    }
}
