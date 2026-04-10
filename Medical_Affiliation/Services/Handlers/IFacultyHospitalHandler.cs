using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Handlers
{
    public interface IFacultyHospitalHandler
    {
        int FacultyId { get; }
        Task<HospitalAffiliationCompositeViewModel> GetDetailsAsync(string collegeCode);
    }
}
