// File: Models/NursingFacultyViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Medical_Affiliation.Models
{
    public class NursingFacultyViewModel
    {
        public int Id { get; set; }
        public string TeachingFacultyName { get; set; }
        public string Designation { get; set; }
        public string AadhaarNumber { get; set; }
        public string PANNumber { get; set; }

        // New fields
        public string Subject { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public DateOnly? RecognizedPgTeacherDate { get; set; }

        public List<SelectListItem>? Subjects { get; set; } = new();
    }

}
