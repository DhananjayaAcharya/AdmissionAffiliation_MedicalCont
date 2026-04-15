using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface ICAVehicleService
    {
        Task<VehicleDetailListDisplayViewModel> GetVehicleDetailsAsync();
        //Task<AccountAndFew>
    }

}
