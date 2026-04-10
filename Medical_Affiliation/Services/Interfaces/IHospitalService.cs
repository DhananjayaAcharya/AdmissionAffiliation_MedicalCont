using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface IHospitalService
    {
       
        Task<HospitalAffiliationCompositeViewModel> GetHospitalDetailsAsync(string collegeCode, int facultyCode,string CourseLevel);
    }
}
