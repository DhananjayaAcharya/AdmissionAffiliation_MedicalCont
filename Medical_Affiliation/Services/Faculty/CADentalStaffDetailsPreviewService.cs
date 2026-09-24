using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CADentalStaffDetailsPreviewService : ICADentalStaffDetailsPreviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public CADentalStaffDetailsPreviewService(
            ApplicationDbContext context,
            IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<DentalStaffDetailsPreviewVM?> GetDentalStaffDetailsPreviewAsync()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var affiliationTypeId = _userContext.TypeOfAffiliation;

            var courseLevel = _userContext.CourseLevel;

            // Dental only
            if (facultyCode != 2)
                return null;

            if (string.IsNullOrWhiteSpace(collegeCode))
                return null;

            courseLevel = courseLevel?.Trim().ToUpperInvariant();

            // =====================================================
            // STAFF DESIGNATION MASTER
            // =====================================================

            var designationMasters =
                await _context.MedCaMstStaffDesignations
                    .Where(x =>
                        x.FacultyCode ==
                        facultyCode.ToString())
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

            // =====================================================
            // STAFF PAY SCALE
            // =====================================================

            var savedPayScales =
                await _context.MedCaStaffParticulars
                    .Where(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode ==
                            facultyCode.ToString() &&
                        x.CourseLevel == courseLevel)
                    .ToListAsync();

            var payScaleList =
                designationMasters
                    .Select(d =>
                    {
                        var saved =
                            savedPayScales.FirstOrDefault(x =>
                                x.DesignationSlNo == d.SlNo);

                        return new Med_CA_StaffParticularsVM
                        {
                            DesignationSlNo = d.SlNo,
                            Designation = d.Designation,
                            PayScale = saved?.PayScale
                        };
                    })
                    .ToList();

            // =====================================================
            // OTHER STAFF PARTICULARS
            // =====================================================

            var staffOther =
                await _context.CaMedStaffParticularsOthers
                    .FirstOrDefaultAsync(x =>
                        x.CollegeCode == collegeCode &&
                        x.FacultyCode ==
                            facultyCode.ToString() &&
                        x.CourseLevel == courseLevel);

            var staffOtherVM =
                staffOther == null
                    ? new CA_Med_StaffParticularsOtherVM()
                    : new CA_Med_StaffParticularsOtherVM
                    {
                        Id = staffOther.Id,

                        CourseLevel =
                            staffOther.CourseLevel,

                        FacultyCode =
                            staffOther.FacultyCode,

                        CollegeCode =
                            staffOther.CollegeCode,

                        RegistrationNo =
                            staffOther.RegistrationNo,

                        SubFacultyCode =
                            staffOther.SubFacultyCode,

                        TeachersUpdatedInEMS =
                            staffOther.TeachersUpdatedInEms,

                        ExaminerDetailsAttached =
                            staffOther.ExaminerDetailsAttached,

                        ServiceRegisterMaintained =
                            staffOther.ServiceRegisterMaintained,

                        AcquittanceRegisterMaintained =
                            staffOther.AcquittanceRegisterMaintained,

                        ExaminerDetailsPdfName =
                            staffOther.ExaminerDetailsPdfPath,

                        AEBASLastThreeMonthsPdfName =
                            staffOther.AebaslastThreeMonthsPdfPath,

                        AEBASInspectionDayPdfName =
                            staffOther.AebasinspectionDayPdfPath,

                        ProvidentFundPdfName =
                            staffOther.ProvidentFundPdfPath,

                        ESIPdfName =
                            staffOther.EsipdfPath
                    };

            // =====================================================
            // FINAL VIEW MODEL
            // =====================================================

            return new DentalStaffDetailsPreviewVM
            {
                CourseLevel = courseLevel,

                CollegeCode = collegeCode,

                FacultyCode =
                    facultyCode.ToString(),

                StaffPayScaleList = payScaleList,

                StaffOther = staffOtherVM,

                ExaminerDetailsPdfName =
                    staffOther?.ExaminerDetailsPdfPath,

                AEBASLastThreeMonthsPdfName =
                    staffOther?.AebaslastThreeMonthsPdfPath,

                AEBASInspectionDayPdfName =
                    staffOther?.AebasinspectionDayPdfPath,

                ProvidentFundPdfName =
                    staffOther?.ProvidentFundPdfPath,

                ESIPdfName =
                    staffOther?.EsipdfPath
            };
        }
    }
}