using Medical_Affiliation.Models;

public interface IFacultyHospitalHandler
{
    int FacultyId { get; }

    Task<HospitalAffiliationCompositeViewModel> GetDetailsAsync(
        string collegeCode, string courseLevel);
}