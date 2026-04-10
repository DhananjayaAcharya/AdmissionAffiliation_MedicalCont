namespace Medical_Affiliation.Models
{
    public class SectionOfficerCourseWiseRguhsIntakeViewModel
    {
        public List<CollegeCourseIntakeDetail> collegeCourseIntakeDetails { get; set; }
        public List<RguhsIntakeChangeAndApproval> rguhsIntakeChangeAndApprovals { get; set; }
        public List<Faculty> faculties { get; set; }
        public string SelectedFacultyCode { get; set; }

    }

    public class IntakeUpdateVM
    {
        public string CollegeCode { get; set; }
        public string CourseCode { get; set; }
        public int NewIntake { get; set; }
        public string Reason { get; set; }
    }
}
