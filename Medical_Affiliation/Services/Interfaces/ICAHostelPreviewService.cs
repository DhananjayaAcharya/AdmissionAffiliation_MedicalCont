using Medical_Affiliation.Models;
using Medical_Affiliation.ViewModels;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface ICAHostelPreviewService
    {
        Task<AffHostelPreviewViewModel> GetHostelPreviewAsync();
    }
}