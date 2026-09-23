using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface ICADentalLibraryService
    {
        Task<DentalLibraryDisplayViewModel> GetLibraryAsync();
        //Task<AccountAndFew>
    }

}
