using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface ICALibraryService
    {
        Task<MedicalLibraryDisplayViewModel> GetLibraryAsync();
        //Task<AccountAndFew>
    }

}
