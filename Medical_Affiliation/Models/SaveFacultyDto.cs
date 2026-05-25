namespace Medical_Affiliation.Models
{
    public class SaveFacultyDto
    {
        public int Id { get; set; }

        public string DepartmentCode { get; set; }
        public string NameOftheFaculty { get; set; }
        public string DesignationCode { get; set; }
        public string Dob { get; set; }
        public string DateOfAppointment { get; set; }

        // ── New fields added ────────────────────────────────────────────────
        public string MobileNo { get; set; }   // Business Primary Key
        public string PanNo { get; set; }
       
        public string StateCouncilRegNo { get; set; }
     
        public string AEBASAttendId { get; set; }
        public string ProfessionalQualification { get; set; }
        public string NatureOfEmployment { get; set; }
        public string TeachingExpInYrs { get; set; }
        public string PhotoFilePath { get; set; }

    }
    public class DeleteFacultyDto
    {
        public int Id { get; set; }
    }
}
