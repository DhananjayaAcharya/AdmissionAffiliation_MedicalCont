using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface ICAAcademicService
    {
        Task<CA_Aff_AcademicMattersViewModel> GetAcademicMattersAsync();
    }

}
