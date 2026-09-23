using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface ICAVehiclePreviewService
    {
        Task<VehicleDetailPreviewViewModel> GetVehiclePreviewAsync();
    }
}
