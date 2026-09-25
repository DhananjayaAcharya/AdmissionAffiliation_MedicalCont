using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Medical_Affiliation.Models
{
    public class ActionTakenDeficiencyReportViewModel
    {
        public int ActionTakenDeficiencyReportId { get; set; }

        // ============================================================
        // SESSION CONTROLLED FIELDS
        // ============================================================

        // FacultyCode itself represents FacultyId
        public string? FacultyCode { get; set; }

        public string? CollegeCode { get; set; }

        public int TypeId { get; set; }

        public string? CourseLevel { get; set; }


        // ============================================================
        // ACTION TAKEN DEFICIENCY DETAILS
        // ============================================================

        [Required(ErrorMessage = "Please enter the deficiency pointed out.")]
        public string? DeficiencyPointedOut { get; set; }

        [Required(ErrorMessage = "Please enter the extent to which the deficiency was remedied.")]
        public string? ExtentRemedied { get; set; }


        // ============================================================
        // REPORT UPLOAD
        // ============================================================

        // New uploaded file
        public IFormFile? RelevantReportFile { get; set; }

        // Existing uploaded file path
        public string? RelevantReportPath { get; set; }

        
        // ============================================================
        // EXISTING RECORDS
        // ============================================================

        public List<ActionTakenDeficiencyReport> ActionTakenDeficiencyRecords { get; set; }
            = new();
    }
}