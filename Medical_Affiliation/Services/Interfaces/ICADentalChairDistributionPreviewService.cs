using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface ICADentalChairDistributionPreviewService
    {
        Task<List<DentalChairVm>> GetDentalChairDistributionPreviewAsync();
    }
}
