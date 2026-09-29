using Medical_Affiliation.DATA;
using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Medical_Affiliation.Services.Faculty
{
    public class ActionTakenDeficiencyReportService
    : IActionTakenDeficiencyReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserContext _userContext;

        public ActionTakenDeficiencyReportService(
            ApplicationDbContext context, 
            IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        public async Task<List<ActionTakenDeficiencyReportPreviewVM>> GetPreviewAsync()
        {

            string collegeCode = _userContext.CollegeCode;
            int facultyId = _userContext.FacultyId;
            int typeId = _userContext.TypeOfAffiliation;
            string courseLevel = _userContext.CourseLevel;

            return await _context.ActionTakenDeficiencyReports
                .AsNoTracking()
                .Where(x =>
                    x.CollegeCode == collegeCode &&
                    x.FacultyId == facultyId &&
                    x.TypeId == typeId &&
                    x.CourseLevel == courseLevel &&
                    x.IsActive)
                .OrderBy(x => x.ActionTakenDeficiencyReportId)
                .Select(x => new ActionTakenDeficiencyReportPreviewVM
                {
                    ActionTakenDeficiencyReportId =
                        x.ActionTakenDeficiencyReportId,

                    DeficiencyPointedOut =
                        x.DeficiencyPointedOut,

                    ExtentRemedied =
                        x.ExtentRemedied,

                    HasRelevantReport =
                        !string.IsNullOrWhiteSpace(x.RelevantReportPath)
                })
                .ToListAsync();
        }
    }
}
