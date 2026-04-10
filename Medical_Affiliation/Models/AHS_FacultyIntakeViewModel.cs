using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class AHS_FacultyIntakeViewModel
    {
        //[Required]
        //public string AffiliationType { get; set; } // Fresh/Additional/Increase

        [Required]
        public string CourseLevel { get; set; } // from Mst_Course.CourseLevel

        [Required]
        public string CourseCode { get; set; } // saved to DB
        public string IntakeDetails { get; set; }
        [Required]
        public string FreshOrIncrease { get; set; }    // If required


        // For display only - optional
        //public string CourseName { get; set; }

        // Uploads as IFormFile in view model; will convert to byte[] in controller
        public IFormFile RGUHSNotificationFile { get; set; }
        //public IFormFile INCUploadFile { get; set; }
        //public IFormFile KNMCUploadFile { get; set; }
        public IFormFile GOKUploadFile { get; set; }
    }
}

