using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_Med_ResearchPublicationsDetailsVM
    {
        // ================= 4.a =================
        [Required(ErrorMessage = "Number of publications is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Enter a valid number")]
        public int PublicationsNo { get; set; }

        public string? CourseLevel { get; set; }
        // ❌ NO [Required] here
        public IFormFile? PublicationsPdf { get; set; }
        public string? PublicationsPdfName { get; set; }

        // ================= 4.b =================
        //[Required(ErrorMessage = "PI Type is required")]
        //public string PI { get; set; } = string.Empty;

        //[Required(ErrorMessage = "RGUHS Funded is required")]
        //[Range(0, int.MaxValue)]
        //public int RGUHSFunded { get; set; }

        //[Required(ErrorMessage = "External Body Funding is required")]
        //[Range(0, int.MaxValue)]
        //public int ExternalBodyFunding { get; set; }

        

        //code added by ram on 23-01-2023

        // ================= 4.b (Students + Faculty) =================

        [Required(ErrorMessage = "Students - RGUHS Funded is required")]
        public int? StudentsRGUHSFunded { get; set; }

        [Required(ErrorMessage = "Students - External Body Funding is required")]
        public int? StudentsExternalBodyFunding { get; set; }

        public IFormFile? StudentsProjectsPdf { get; set; }
        public string? StudentsProjectsPdfName { get; set; }


        [Required(ErrorMessage = "Faculty - RGUHS Funded is required")]
        public int? FacultyRGUHSFunded { get; set; }

        [Required(ErrorMessage = "Faculty - External Body Funding is required")]
        public int? FacultyExternalBodyFunding { get; set; }

        public IFormFile? FacultyProjectsPdf { get; set; }
        public string? FacultyProjectsPdfName { get; set; }




        // ❌ NO [Required]
        //public IFormFile? ProjectsPdf { get; set; }
        //public string? ProjectsPdfName { get; set; }

        // ================= 4.c =================
        // ❌ NO [Required]
        public IFormFile? ClinicalTrialsPdf { get; set; }
        public string? ClinicalTrialsPdfName { get; set; }

        // ================= 4.d =================
        public List<CA_Med_Lib_OtherAcademicActivitiesVM> OtherActivities { get; set; } = new();
        public List<DepartmentMaster> Departments { get; set; } = new();
        public List<CaMstMedOtherAcademicActivity> ActivityMasters { get; set; } = new();
        public CA_Med_Lib_OtherActivityPostVM? OtherActivityToEdit { get; set; }

        // ================= 5 =================
        public List<CA_Med_Lib_CommitteeVM> Committees { get; set; } = new();
        public Dictionary<string, CaMedResearchPublicationsDetail> CourseData { get; set; }

    }
}
