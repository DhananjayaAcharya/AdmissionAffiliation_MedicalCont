namespace Medical_Affiliation.Models
{
    public class CollegeDetailsViewModel
    {
        public int FacultyCode { get; set; }
        public string CollegeName { get; set; }
        public string CollegeCode { get; set; }
        

        public List<IntakeDetail> IntakeDetails { get; set; } = new();

        public List<CourseMaster> Courses { get; set; }
        public List<string> CourseLevels { get; set; } = new();
        public List<FreshOrIncreaseMaster> FreshOrIncreaseMasters { get; set; } = new();

        public List<CollegeIntakeDetailViewModel> CollegeIntakeDetailViewModel { get; set; } = new();
        public List<Faculty> FacultyMaster {  get; set; } = new();

    }

    public class CollegeIntakeDetailViewModel
    {
        public int Id { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
        public string Degree { get; set; } = string.Empty;

        public string FacultyCode { get; set; }
        public int NumberOfSeats { get; set; }
        public string FreshOrContinuationName { get; set; }  // Flattened from navigation
        //public int? InstitutionId { get; set; }

        public string SelectedDegree { get; set; }
        public string SelectedCourse { get; set; }
        public string SelectedFreshOrContinuation { get; set; }
        public List<string> CourseLevels { get; set; } = new();
        public List<FreshOrIncreaseMaster> FreshOrIncreaseMasters { get; set; } = new();
        public List<CourseMaster> Courses { get; set; }
        public bool? IsCorrect { get; set; }
        public bool? IsIncorrect { get; set; }
        public IFormFile DocumentFile { get; set; } // For binding uploaded file
        public byte[] DocumentBytes { get; set; }
        public string Remarks { get; set; }
        public int ExistingIntake { get; set; }


    }

}
