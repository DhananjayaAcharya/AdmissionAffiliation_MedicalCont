using Medical_Affiliation.Models;
using Medical_Affiliation.Services.Handlers;
using Medical_Affiliation.Services.Interfaces;

namespace Medical_Affiliation.Services.Faculty
{
    public class FacultyHospitalService : IHospitalService
    {
        private readonly IEnumerable<IFacultyHospitalHandler> _handlers;
        public FacultyHospitalService(IEnumerable<IFacultyHospitalHandler> handlers)
        {
            _handlers = handlers;
        }

        public Task<HospitalAffiliationCompositeViewModel> GetHospitalDetailsAsync(string collegeCode, int facultyCode, string CourseLevel)
        {
            var handler = _handlers.FirstOrDefault(h => h.FacultyId == facultyCode);

            if (handler == null)
                throw new NotImplementedException($"Faculty {facultyCode} not implemented");

            return handler.GetDetailsAsync(collegeCode);
        }
    }
}
