using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAAcademicPerformancePreviewService : CAPreviewServiceBase, ICAAcademicPerformancePreviewService
    {
        public CAAcademicPerformancePreviewService( ApplicationDbContext context,  IUserContext userContext) : base(context, userContext)
        {
        }

        public async Task<AcademicPerformanceVM> GetAcademicPerformancePreviewAsync()
        {
            var vm = new AcademicPerformanceVM();

            // ============================================================
            // ACADEMIC PERFORMANCE
            // ============================================================

            var yearMasters = await _context.CaMstYearOfStudies
                .OrderBy(x => x.YearOfStudyId)
                .ToListAsync();

            var academicRows = await _context.CaAcademicPerformances
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyId == FacultyCode &&
                    (string.IsNullOrEmpty(x.CourseLevel) ||
                     x.CourseLevel == CourseLevel))
                .ToListAsync();

            vm.AcademicRows = yearMasters
                .Select(year =>
                {
                    var saved = academicRows.FirstOrDefault(x =>
                        x.YearOfStudyId == year.YearOfStudyId);

                    return new AcademicPerformanceRowVM
                    {
                        YearOfStudyId = year.YearOfStudyId,
                        YearName = year.YearName,
                        RegularStudents = saved?.RegularStudents,
                        RepeaterStudents = saved?.RepeaterStudents,
                        NumberOfStudentsPassed = saved?.NumberOfStudentsPassed,
                        PassPercentage = saved?.PassPercentage,
                        FirstClassCount = saved?.FirstClassCount,
                        DistinctionCount = saved?.DistinctionCount,
                        Remarks = saved?.Remarks
                    };
                })
                .ToList();

            // ============================================================
            // COURSE CURRICULUM
            // ============================================================

            var curriculumMasters = await _context.CaMstCourseCurricula
                .OrderBy(x => x.CurriculumId)
                .ToListAsync();

            var savedCurriculums = await _context.CaCourseCurricula
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyId == FacultyCode &&
                    (string.IsNullOrEmpty(x.CourseLevel) ||
                     x.CourseLevel == CourseLevel))
                .ToListAsync();

            vm.CourseCurriculums = curriculumMasters
                .Select(master =>
                {
                    var saved = savedCurriculums.FirstOrDefault(x =>
                        x.CurriculumId == master.CurriculumId);

                    return new CourseCurriculumVM
                    {
                        CurriculumId = master.CurriculumId,
                        CurriculumName = master.CurriculumName,
                        CurriculumDetails = saved?.CurriculumDetails,
                        HasPdf = !string.IsNullOrWhiteSpace(saved?.CurriculumPdfPath)
                    };
                })
                .ToList();

            // ============================================================
            // EXAMINATION SCHEMES
            // ============================================================

            var schemeMasters = await _context.CaMstExaminationSchemes
                .OrderBy(x => x.SchemeId)
                .ToListAsync();

            var savedSchemes = await _context.CaExaminationSchemes
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyId == FacultyCode)
                .ToListAsync();

            vm.ExaminationSchemes = schemeMasters
                .Select(master =>
                {
                    var saved = savedSchemes.FirstOrDefault(x =>
                        x.SchemeId == master.SchemeId);

                    return new ExaminationSchemeVM
                    {
                        SchemeId = master.SchemeId,
                        SchemeCode = master.SchemeCode,
                        NumberOfStudents = saved?.NumberOfStudents
                    };
                })
                .ToList();

            // ============================================================
            // STUDENT REGISTER RECORDS
            // ============================================================

            var registerMasters = await _context.CaMstRegisterRecords
                .OrderBy(x => x.RegisterRecordId)
                .ToListAsync();

            var savedRegisters = await _context.CaStudentRegisterRecords
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyId == FacultyCode &&
                    (string.IsNullOrEmpty(x.CourseLevel) ||
                     x.CourseLevel == CourseLevel))
                .ToListAsync();

            vm.StudentRegisterRecords = registerMasters
                .Select(master =>
                {
                    var saved = savedRegisters.FirstOrDefault(x =>
                        x.RegisterRecordId == master.RegisterRecordId);

                    return new StudentRegisterRecordVM
                    {
                        RegisterRecordId = master.RegisterRecordId,
                        RegisterName = master.RegisterName,
                        IsMaintained = saved?.IsMaintained
                    };
                })
                .ToList();

            // ============================================================
            // PG ACADEMIC PERFORMANCE
            // ============================================================

            var pgSubjects = await _context.MstCourses
                .Where(x => x.CourseLevel.ToUpper() == "PG")
                .Select(x => new
                {
                    SubjectCode = x.CourseCode.ToString(),
                    SubjectName = x.SubjectName
                })
                .ToListAsync();

            var pgAcademicRows = await _context.CaAcademicPerformances
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyId == FacultyCode &&
                    x.CourseLevel == "PG")
                .ToListAsync();

            vm.PgAcademicPerformances = pgSubjects
                .Where(s => pgAcademicRows.Any(a => a.Subject == s.SubjectCode))
                .Select(subject => new PgAcademicPerformanceVM
                {
                    SubjectCode = subject.SubjectCode,
                    SubjectName = subject.SubjectName,

                    YearData = yearMasters
                        .Take(3)
                        .Select(year =>
                        {
                            var saved = pgAcademicRows.FirstOrDefault(x =>
                                x.Subject == subject.SubjectCode &&
                                x.YearOfStudyId == year.YearOfStudyId);

                            return new PgAcademicPerformanceRowVM
                            {
                                YearOfStudyId = year.YearOfStudyId,
                                YearName = year.YearName,
                                RegularStudents = saved?.RegularStudents,
                                RepeaterStudents = saved?.RepeaterStudents,
                                NumberOfStudentsPassed = saved?.NumberOfStudentsPassed,
                                PassPercentage = saved?.PassPercentage,
                                FirstClassCount = saved?.FirstClassCount,
                                DistinctionCount = saved?.DistinctionCount,
                                Remarks = saved?.Remarks
                            };
                        })
                        .ToList()
                })
                .ToList();

            // ============================================================
            // ACCOUNT & FEE DETAILS
            // ============================================================

            var accountDetails = await _context.MedCaAccountAndFeeDetails
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode.ToString())
                .OrderBy(x => x.CourseLevel)
                .ToListAsync();

            vm.AccountAndFeeDetails = accountDetails
                .Select(x => new AccountAndFeeDetailsVM
                {
                    CourseLevel = x.CourseLevel,

                    AuthorityNameAddress = x.AuthorityNameAddress,
                    AuthorityContact = x.AuthorityContact,

                    RecurrentAnnual = x.RecurrentAnnual,
                    NonRecurrentAnnual = x.NonRecurrentAnnual,
                    Deposits = x.Deposits,

                    TuitionFee = x.TuitionFee,
                    SportsFee = x.SportsFee,
                    UnionFee = x.UnionFee,
                    LibraryFee = x.LibraryFee,
                    OtherFee = x.OtherFee,
                    TotalFee = x.TotalFee,

                    AccountBooksMaintained = x.AccountBooksMaintained,
                    AccountsAudited = x.AccountsAudited,
                    DonationLevied = x.DonationLevied,

                    HasGoverningCouncilPdf =
                        !string.IsNullOrWhiteSpace(x.GoverningCouncilPdfPath),

                    HasAccountSummaryPdf =
                        !string.IsNullOrWhiteSpace(x.AccountSummaryPdfPath),

                    HasAuditedStatementPdf =
                        !string.IsNullOrWhiteSpace(x.AuditedStatementPdfPath),

                    HasDonationPdf =
                        !string.IsNullOrWhiteSpace(x.DonationPdfPath)
                })
                .ToList();

            // ============================================================
            // STAFF DETAILS
            // ============================================================

            var designationMasters = await _context.MedCaMstStaffDesignations
                .Where(x => x.FacultyCode == FacultyCode.ToString())
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            string commonLevel = CourseLevel;

            if (string.IsNullOrWhiteSpace(commonLevel))
            {
                commonLevel = "UG";
            }

            var savedPayScales = await _context.MedCaStaffParticulars
                .Where(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode.ToString() &&
                    x.CourseLevel == commonLevel)
                .ToListAsync();

            var staffOther = await _context.CaMedStaffParticularsOthers
                .FirstOrDefaultAsync(x =>
                    x.CollegeCode == CollegeCode &&
                    x.FacultyCode == FacultyCode.ToString() &&
                    x.CourseLevel == commonLevel);

            vm.StaffDetails = new StaffDetailsPreviewVM
            {
                StaffPayScales = designationMasters
                    .Select(d =>
                    {
                        var saved = savedPayScales
                            .FirstOrDefault(x => x.DesignationSlNo == d.SlNo);

                        return new StaffPayScalePreviewVM
                        {
                            DesignationSlNo = d.SlNo,
                            Designation = d.Designation,
                            PayScale = saved?.PayScale
                        };
                    })
                    .ToList(),

                StaffOther = staffOther == null
                    ? new StaffOtherPreviewVM()
                    : new StaffOtherPreviewVM
                    {
                        TeachersUpdatedInEMS = staffOther.TeachersUpdatedInEms,
                        ExaminerDetailsAttached = staffOther.ExaminerDetailsAttached,
                        ServiceRegisterMaintained = staffOther.ServiceRegisterMaintained,
                        AcquittanceRegisterMaintained = staffOther.AcquittanceRegisterMaintained,

                        HasExaminerDetailsPdf1 =
                            !string.IsNullOrWhiteSpace(staffOther.ExaminerDetailsPdfPath),

                        HasExaminerDetailsPdf2 =
                            !string.IsNullOrWhiteSpace(staffOther.ExaminerDetailsPdfPath2),

                        HasExaminerDetailsPdf3 =
                            !string.IsNullOrWhiteSpace(staffOther.ExaminerDetailsPdfPath3),

                        HasExaminerDetailsPdf4 =
                            !string.IsNullOrWhiteSpace(staffOther.ExaminerDetailsPdfPath4),

                        HasExaminerDetailsPdf5 =
                            !string.IsNullOrWhiteSpace(staffOther.ExaminerDetailsPdfPath5),

                        HasAEBASLastThreeMonthsPdf =
                            !string.IsNullOrWhiteSpace(staffOther.AebaslastThreeMonthsPdfPath),

                        HasAEBASInspectionDayPdf =
                            !string.IsNullOrWhiteSpace(staffOther.AebasinspectionDayPdfPath),

                        HasProvidentFundPdf =
                            !string.IsNullOrWhiteSpace(staffOther.ProvidentFundPdfPath),

                        HasESIPdf =
                            !string.IsNullOrWhiteSpace(staffOther.EsipdfPath)
                    }
            };

            return vm;
        }
    }
}