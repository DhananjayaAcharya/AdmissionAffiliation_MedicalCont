namespace Admission_Affiliation.Models
{
    public class FacultyViewModel
    {
        public int FacultyId { get; set; }

        public string FacultyName { get; set; } = null!;

        public int? EmsFacultyId { get; set; }

        public string? FacultyAbbre { get; set; }

        public string? Status { get; set; }
    }
}
