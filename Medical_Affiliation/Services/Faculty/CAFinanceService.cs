using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Medical_Affiliation.Services.Faculty
{
    public class CAFinanceService : ICAFinanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;
        public CAFinanceService(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }


        public async Task<FinanceViewModel> GetFinanceDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var accDetails = await GetAccountAndFeeDetails();
            var staff = await GetStaffParticularDetails();
            var otherStaff = await GetOtherStaffParticularDetails();

            var mainvm = new FinanceViewModel
            {
                medCaAccountAndFee = accDetails,
                staffParticularsVM = new MedCaStaffParticularListDisplayViewModel { StaffParticulars = staff },
                otherStaffParticularsVM = otherStaff
            };

            return mainvm;
        }

        public async Task<MedCaAccountAndFeeDetailDisplayViewModel> GetAccountAndFeeDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var AccAndFeeDetail = await _context.MedCaAccountAndFeeDetails.AsNoTracking().Where(e => e.CollegeCode == collegeCode).FirstOrDefaultAsync();
            var model = new MedCaAccountAndFeeDetailDisplayViewModel
            {
                Id = AccAndFeeDetail.Id,
                CollegeCode = AccAndFeeDetail.CollegeCode,
                FacultyCode = AccAndFeeDetail.FacultyCode,
                AuditedStatementPdfName = AccAndFeeDetail.AuditedStatementPdfName,
                AuthorityNameAddress = AccAndFeeDetail.AuthorityNameAddress,
                AuthorityContact = AccAndFeeDetail.AuthorityContact,
                RecurrentAnnual = AccAndFeeDetail.RecurrentAnnual,
                NonRecurrentAnnual = AccAndFeeDetail.NonRecurrentAnnual,
                Deposits = AccAndFeeDetail.Deposits,
                TuitionFee = AccAndFeeDetail.TuitionFee,
                SportsFee = AccAndFeeDetail.SportsFee,
                UnionFee = AccAndFeeDetail.UnionFee,
                LibraryFee = AccAndFeeDetail.LibraryFee,
                OtherFee = AccAndFeeDetail.OtherFee,
                TotalFee = AccAndFeeDetail.TotalFee,
                AccountBooksMaintained = AccAndFeeDetail.AccountBooksMaintained,
                AccountSummaryPdfName = AccAndFeeDetail.AccountSummaryPdfName,
                HasAuditedStatementPdf = AccAndFeeDetail.AuditedStatementPdfPath.Length > 0,
                HasAccountSummaryPdf = AccAndFeeDetail.AccountSummaryPdfPath.Length > 0,
                HasGoverningCouncilPdf = AccAndFeeDetail.GoverningCouncilPdfPath.Length > 0,
            };

            return model;

        }

        public async Task<List<MedCaStaffParticularDisplayViewModel>> GetStaffParticularDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;

            var staffParticulars = await (from det in _context.MedCaStaffParticulars
                                          join mst in _context.MedCaMstStaffDesignations
                                          on det.DesignationSlNo equals mst.SlNo
                                          where det.CollegeCode == collegeCode && det.FacultyCode == facultyCode.ToString()
                                          join clg in _context.AffiliationCollegeMasters
                                          on det.CollegeCode equals clg.CollegeCode
                                          select new MedCaStaffParticularDisplayViewModel
                                          {
                                              Id = det.Id,
                                              CollegeName = clg.CollegeName,
                                              DesignationName = mst.Designation,
                                              PayScale = det.PayScale
                                          }
                                          ).AsNoTracking().ToListAsync();
            return staffParticulars;
        }

        public async Task<CaMedStaffParticularsOtherDisplayViewModel> GetOtherStaffParticularDetails()
        {
            var collegeCode = _userContext.CollegeCode;
            var facultyCode = _userContext.FacultyId;
            var othersp = await (from other in _context.CaMedStaffParticularsOthers
                                 join clg in _context.AffiliationCollegeMasters
                                 on other.CollegeCode equals collegeCode
                                 where other.CollegeCode == collegeCode && other.FacultyCode == facultyCode.ToString()
                                 select new CaMedStaffParticularsOtherDisplayViewModel
                                 {
                                     Id = other.Id,
                                     CollegeName = clg.CollegeName,
                                     TeachersUpdatedInEms = other.TeachersUpdatedInEms == "Y",
                                     ExaminerDetailsAttached = other.ExaminerDetailsAttached == "Y",
                                     ExaminerDetailsPdfName = other.ExaminerDetailsPdfName,
                                     HasExaminerDetailsPdf = other.ExaminerDetailsPdfPath.Length > 0,
                                     HasAebasInspectionDayPdf = other.AebasinspectionDayPdfPath.Length > 0,
                                     HasAebasLastThreeMonthsPdf = other.AebaslastThreeMonthsPdfPath.Length > 0,
                                     ServiceRegisterMaintained = other.ServiceRegisterMaintained == "Y",
                                     ProvidentFundPdfName = other.ProvidentFundPdfName,
                                     HasProvidentFundPdf = other.ProvidentFundPdfPath.Length > 0,
                                     HasEsipdf = other.EsipdfPath.Length > 0,
                                     AcquittanceRegisterMaintained = other.AcquittanceRegisterMaintained == "Y",


                                 }
                                 ).AsNoTracking().FirstOrDefaultAsync();

            return othersp;
        }
    }
}
