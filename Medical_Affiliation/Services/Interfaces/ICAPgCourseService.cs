using Medical_Affiliation.Models;

namespace Medical_Affiliation.Services.Interfaces
{
    public interface ICAPgCourseService
    {
        Task<AffiliationPgCourseDisplayVM> GetPgCourseDetailsAsync();

        Task<List<PgCourseVm>> GetDegreeCourses();
        Task<List<PgCourseVm>> GetDiplomaCourses();
        Task<List<PgCourseParticularsVm>> GetPgCoursesParticulars();
        Task<List<PgCoursesGokVM>> GetPgCoursesForGOK();
        Task<List<PgCoursesWithRGUHSPermission>> GetPgCoursesWithRguhsPermission();
        Task<List<OtherCoursesPermittedByNMC>> GetOtherDeptCoursesPermittedByNmc();
        Task<LICinspectionVM> GetLicInspectionDetails();
    }
}
