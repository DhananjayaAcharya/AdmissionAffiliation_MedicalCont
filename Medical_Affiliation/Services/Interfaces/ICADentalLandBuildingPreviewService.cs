using Medical_Affiliation.ViewModels;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface ICADentalLandBuildingPreviewService
    {
        Task<DentalCollegeLandBuildingViewModel> GetDentalLandBuildingPreviewAsync();
    }
}
