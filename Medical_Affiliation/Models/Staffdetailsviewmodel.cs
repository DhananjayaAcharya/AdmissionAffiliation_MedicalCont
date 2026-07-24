using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class StaffDetailsViewModel
    {
        public string FacultyCode { get; set; } = string.Empty;
        public string CollegeCode { get; set; } = string.Empty;

        [Required]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        public string TypeOfAffiliation { get; set; } = string.Empty;

        // F.i. Unit-wise faculty and Senior Resident details
        [Display(Name = "Unit No")]
        public string? UnitNo { get; set; }

        public List<StaffRowVM> StaffMembers { get; set; } = new();  // dynamic rows

        // F.ii. Total eligible faculties and Senior Residents (4 fixed rows)
        public List<EligibleFacultyRowVM> EligibleFaculty { get; set; } = new();

        // F.iii. No. of PG students presently studying in the Department (3 fixed rows)
        public List<PgStudentsYearRowVM> PgStudentsByYear { get; set; } = new();

        // H. List of Students appeared for exams during the previous years (dynamic rows)
        public List<StudentExamResultRowVM> ExamResults { get; set; } = new();
    }

    public class StaffRowVM
    {
        public int SrNo { get; set; }
        public string? Designation { get; set; }
        public string? Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime? JoiningDate { get; set; }

        public string? RelievedRetiredWorking { get; set; }   // Relieved / Retired / Working

        [DataType(DataType.Date)]
        public DateTime? RelievingRetirementDate { get; set; }

        public int? AttendanceDaysForYear { get; set; }
        public decimal? AttendancePercentage { get; set; }
        public string? PhoneNo { get; set; }
        public string? Email { get; set; }
    }

    public class EligibleFacultyRowVM
    {
        public string Designation { get; set; } = string.Empty;   // fixed label
        public int DesignationSequence { get; set; }

        public int? NumberOfFaculty { get; set; }
        public string? Names { get; set; }
        public int? TotalAdmissionSeats { get; set; }
        public string? IsAdequateForAdmission { get; set; }        // Adequate / Not Adequate
    }

    public class PgStudentsYearRowVM
    {
        public string YearLabel { get; set; } = string.Empty;     // fixed label
        public int YearSequence { get; set; }

        public int? NumberOfStudents { get; set; }
    }

    public class StudentExamResultRowVM
    {
        public int SlNo { get; set; }
        public string? StudentName { get; set; }
        public string? Result { get; set; }   // Pass / Fail
    }
}