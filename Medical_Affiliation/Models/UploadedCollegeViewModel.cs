namespace Medical_Affiliation.Models
{
    // ── University Dashboard dropdown ────────────────────────────────────────
    public class UploadedCollegeViewModel
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }
    }

    // ── Upload row returned as JSON to the table ─────────────────────────────
    public class CollegeUploadDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string UploadedDate { get; set; }   // "2026-04-28"
        public string UploadedBy { get; set; }
        public string Status { get; set; }   // "Pending" | "Approved"
        public string FileUrl { get; set; }   // relative URL for iframe
    }

    // ── Faculty row returned as JSON for the approval letter ─────────────────
    public class UgFacultyDto
    {
        public int SlNo { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string Dob { get; set; }

        // ── New fields added ────────────────────────────────────────────────
        public string MobileNo { get; set; }
      

        public string PanNo { get; set; }

        public string StateCouncilRegNo { get; set; }
        // In UgFacultyDto — add these properties:
        public string AEBASAttendId { get; set; }
        public string ProfessionalQualification { get; set; }
        public string NatureOfEmployment { get; set; }  // "Contract" | "Permanent"
        public string TeachingExpInYrs { get; set; }
        public string PhotoFilePath { get; set; }

    }
}