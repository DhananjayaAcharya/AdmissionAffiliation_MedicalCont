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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CAFacultyDesigNonTeachingService(
            ApplicationDbContext context,
            IUserContext userContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userContext = userContext;
            _httpContextAccessor = httpContextAccessor;
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

            return await (from f in _context.UgFacultyDetails.AsNoTracking()
                join department in _context.DepartmentMastersForUgs.AsNoTracking()
                    on new { Code = f.DepartmentCode, Faculty = facultyId }
                    equals new { Code = department.DepartmentCode, Faculty = department.FacultyCode }
                    into departments
                from department in departments.DefaultIfEmpty()
                join designation in _context.UgdesignationMasters.AsNoTracking()
                    on f.DesignationCode equals designation.DesignationId
                    into designations
                from designation in designations.DefaultIfEmpty()
                where f.CollegeCode == collegeCode
                orderby f.DepartmentCode, f.NameOftheFaculty
                select new FacultyDetailDisplayVM
                {
                    Id = f.Id,
                    NameOfFaculty = f.NameOftheFaculty ?? string.Empty,
                    DepartmentCode = f.DepartmentCode,
                    DepartmentName = department.DepartmentName ?? f.DepartmentCode,
                    DesignationCode = f.DesignationCode,
                    DesignationName = designation.DesignationName ?? f.DesignationCode,
                    Dob = f.Dob,
                    DateOfAppointment = f.DateOfAppointment,
                    AadhaarNo = f.AadhaarNo,
                    PanNo = f.Panno,
                    Mobile = f.MobileNo ?? string.Empty,
                    Email = f.EmailId ?? string.Empty,
                    StateCouncilRegNo = f.StateCouncilRegNo,
                    AebasAttendId = f.AebasattendId,
                    ProfessionalQualification = f.ProfessionalQualification,
                    NatureOfEmployment = f.NatureOfEmployment,
                    TeachingExpInYrs = f.TeachingExpInYrs,
                    PhotoFilePath = f.PhotoFilePath
                })
                .ToListAsync();
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
            var collegeCode = _httpContextAccessor.HttpContext?.Session.GetString("CollegeCode")
                              ?? _userContext.CollegeCode;
            var courseLevel = (_httpContextAccessor.HttpContext?.Session.GetString("CourseLevel")
                               ?? _httpContextAccessor.HttpContext?.Session.GetString("SelectedCourseLevel")
                               ?? _userContext.CourseLevel)
                .Trim()
                .ToUpperInvariant();

            var rows = await _context.NonTeachingStaffDetails
                .AsNoTracking()
                .Where(s => s.CollegeCode == collegeCode &&
                            s.CourseLevel != null &&
                            s.CourseLevel.Trim().ToUpper() == courseLevel)
                .OrderBy(s => s.Id)
                .ToListAsync();

            return rows.Select(s => new NonTeachingStaffDisplayVM
            {
                StaffId = s.Id,
                StaffName = s.StaffName ?? string.Empty,
                Designation = s.Designation ?? string.Empty,
                MobileNumber = s.MobileNumber,
                SalaryPaid = decimal.TryParse(s.SalaryPaid, out var salary) ? salary : 0,
                PfProvided = false,
                EsiProvided = false,
                ServiceRegisterMaintained = false,
                SalaryAcquaintanceRegister = false
            }).ToList();
        }

    }
}
