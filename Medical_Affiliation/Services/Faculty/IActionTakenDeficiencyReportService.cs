using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface IActionTakenDeficiencyReportService
    {
        Task<List<ActionTakenDeficiencyReportPreviewVM>> GetPreviewAsync();
    }
}
