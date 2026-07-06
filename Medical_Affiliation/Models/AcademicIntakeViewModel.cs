namespace Medical_Affiliation.Models
{
    public class AcademicIntakeViewModel
    {
        public List<CourseIntakeViewModel> Courses { get; set; } = new();
    }

    public class CourseIntakeViewModel
    {
        public int CourseCode { get; set; }

        public string CourseName { get; set; }

        // Current approved intake shown in card header
        public int CurrentApprovedIntake { get; set; }

        // Previous approved details (from old tables)
        public int? PreviousExistingIntake { get; set; }

        public int? PreviousAdditionalIntake { get; set; }

        public int? PreviousTotalIntake { get; set; }

        public DateTime? PreviousLopDate { get; set; }

        // Future year-wise entries
        public List<AcademicYearIntakeVm> YearWiseIntakes { get; set; }
            = new();
    }

    public class AcademicYearIntakeVm
    {
        public int? Id { get; set; }

        public string AcademicYear { get; set; } // AY 2026-27

        public int ExistingIntake { get; set; }

        public int AdditionalIntake { get; set; }

        public int TotalIntake { get; set; }

        public string? ApprovalAuthority { get; set; }

        public DateTime? LopDate { get; set; }

        public IFormFile? Document { get; set; }

        // for displaying already uploaded file
        public string? DocumentPath { get; set; }
    }

    public class CourseIntakeViewModel1
    {
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string CourseLevel { get; set; }
        public bool IsAlreadyOffered { get; set; }
        public bool AddCourseRequested { get; set; }
        public int? RequestedCourseIntake { get; set; }
    }

    public class CollegeCourseListViewModel1
    {
        public string CollegeCode { get; set; }
        public string FacultyCode { get; set; }
        public string TypeOfAffiliation { get; set; }
        public string CollegeName { get; set; }
        public List<CourseIntakeViewModel1> AllCourses { get; set; } = new();
        // Courses already requested by the college (read-only display on the page)
        public List<CourseIntakeViewModel1> RequestedCourses { get; set; } = new();
    }
}
