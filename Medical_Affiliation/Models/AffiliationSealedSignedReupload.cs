using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Medical_Affiliation.Models
{
    [Table("AffiliationSealedSignedReupload")]
    public class AffiliationSealedSignedReupload
    {
        [Key]
        public int Id { get; set; }

        public string? FacultyCode { get; set; }

        public string? CollegeCode { get; set; }

        public string? TypeOfApplication { get; set; }

        public string? ReuploadedDoc { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
