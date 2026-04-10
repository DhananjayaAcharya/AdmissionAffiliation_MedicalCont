namespace Medical_Affiliation.Models
{
    public class AffiliationPgCourseViewModel
    {
        public string CollegeCode { get; set; }
        public int TypeOfAffiliation { get; set; }


        public List<PgCourseVm> PgDegreeCourses { get; set; } = new();
        public List<PgCourseVm> PgDiplomaCourses { get; set; } = new();
        public List<PgCourseParticularsVm> AllCourses { get; set; } = new();

        public List<PgCoursesGokVM> PgCoursesGOK {  get; set; } = new();
        public List<PgCoursesWithRGUHSPermission> PgCoursesRguhs { get; set; } = new();

        public List<OtherCoursesPermittedByNMC> OtherCoursesPermittedByNMC { get; set; } = new();
        public LICinspectionVM LicInspectionVm { get; set; }

    }

    public class PgCourseVm
    {
        public string CollegeCode { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CourseLevel { get; set; }
        public string CoursePrefix { get; set; }

        public int? CollegeIntake { get; set; }
        public int? RguhsIntake { get; set; }

    }

    public class PgCourseParticularsVm : PgCourseVm
    {

        public DateOnly? DateofLOP { get; set; }
        public DateOnly? DateofRecognitionByNMC { get; set; }

    }

    public class PgCourseParticularsPostVm
    {
        public string CollegeCode { get; set; }

        public List<PgCourseParticularsVm> Courses { get; set; } = new();
    }

    public class PgCoursesGokVM  : PgCourseVm
    {

        public DateOnly? DateofGOK { get; set; }
        public IFormFile GOKDocumentFile { get; set; }
        public byte[] GokDocBytes { get; set; }

        public bool HasGOKDocument { get; set; }
        public string? AcademicYear { get; set; }
        public long? GOKDocumentSize { get; set; }

    }

    public class PgCoursesWithRGUHSPermission: PgCourseVm
    {
        public IFormFile RGUHSDocumentFile { get; set; }
        public byte[] RGUHSDocBytes { get; set; }
        public bool HasRguhsDocument { get; set; }
    }

    public class OtherCoursesPermittedByNMC : PgCourseVm
    {
        public bool PermissionByNMC { get; set; }
        public IFormFile NMCdocumentFile { get; set; }
        public byte[] NMCdocBytes { get; set; }
        public bool HasNMCdocument { get; set; }
        public int? AdmissionsPerYear { get; set; }
        public string FacultyCode { get; set; }
    }

    public class LICinspectionVM
    {
        public DateOnly? PreviousInspectionDate { get; set; }
        public string ActionTaken { get; set; }
    }

}
