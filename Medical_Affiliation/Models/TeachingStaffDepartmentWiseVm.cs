namespace Medical_Affiliation.Models
{
    public class TeachingStaffDepartmentWiseVm
    {
        public string? CollegeCode { get; set; }
        public string? FacultyCode { get; set; }
        public string? CourseLevel { get; set; }

        public List<DepartmentTeachingStaffVm> Departments { get; set; } = new();

        // 🔹 Non-Teaching Staff – simple list (ADDED HERE ONLY)
        // Non-Teaching simple list
        public List<NonTeachingStaffRow> NonTeachingStaff { get; set; } = new();
    }

    public class DepartmentTeachingStaffVm
    {
        public string? DepartmentCode { get; set; }
        public string? DepartmentName { get; set; }

        public List<TeachingStaffDepartmentWiseRow> Rows { get; set; } = new();

        // Non-Teaching simple list
        
    }

    public class TeachingStaffDepartmentWiseRow
    {
        public int Id { get; set; }

        public string? DesignationCode { get; set; }
        public string? DesignationName { get; set; }

        public DateTime? UGFrom { get; set; }
        public DateTime? UGTo { get; set; }

        public DateTime? PGFrom { get; set; }
        public DateTime? PGTo { get; set; }

        public decimal? TotalExperience { get; set; }
    }

    // 🔹 Non-Teaching row INSIDE SAME FILE (not separate model)
    public class NonTeachingStaffRow
    {
        public int Id { get; set; }

        public string? StaffName { get; set; }
        public string? Designation { get; set; }
        public string? MobileNumber { get; set; }
        public string? SalaryPaid { get; set; }
    }
}
