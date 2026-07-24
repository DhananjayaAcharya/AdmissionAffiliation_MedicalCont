using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class PgCourseSummaryViewModel
    {
        // Identity (not user-editable, carried through hidden fields)
        public int PgcourseSummaryDetailId { get; set; }
        public string FacultyCode { get; set; } = string.Empty;
        public string CollegeCode { get; set; } = string.Empty;

        [Required]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        public string TypeOfAffiliation { get; set; } = string.Empty;

        // Header
        [Display(Name = "Date of Assessment")]
        [DataType(DataType.Date)]
        public DateTime? DateOfAssessment { get; set; }

        [Display(Name = "Name of assessor")]
        public string? AssessorName { get; set; }

        // 1. Name of Institution
        [Display(Name = "Name of Institution")]
        public string? InstitutionName { get; set; }

        [Display(Name = "Institution Category")]
        public string? InstitutionCategory { get; set; }   // 'Private' / 'Government'

        // 1. Director / Dean / Principal (Head of Institution)
        [Display(Name = "Designation")]
        public string? HeadOfInstitutionDesignation { get; set; }  // 'Director' / 'Dean' / 'Principal'

        [Display(Name = "Name")]
        public string? HeadOfInstitutionName { get; set; }

        [Display(Name = "Age & Date of Birth")]
        public string? HeadOfInstitutionAgeDob { get; set; }

        [Display(Name = "Teaching experience")]
        public string? HeadOfInstitutionTeachingExp { get; set; }

        [Display(Name = "PG Degree")]
        public string? HeadOfInstitutionPgdegree { get; set; }

        [Display(Name = "Recognized / Non-R")]
        public string? HeadOfInstitutionPgrecognition { get; set; }

        [Display(Name = "Subject")]
        public string? HeadOfInstitutionSubject { get; set; }

        // 2. Department inspected / Head of Department
        [Display(Name = "Department inspected")]
        public string? DepartmentInspected { get; set; }

        [Display(Name = "Name")]
        public string? Hodname { get; set; }

        [Display(Name = "Age & Date of Birth")]
        public string? HodageDob { get; set; }

        [Display(Name = "Teaching experience")]
        public string? HodteachingExp { get; set; }

        [Display(Name = "PG Degree")]
        public string? HodpgDegree { get; set; }

        [Display(Name = "Recognized / Non-R")]
        public string? HodpgRecognition { get; set; }

        // 3(a). Number of UG seats
        [Display(Name = "Number of UG seats")]
        public int? NumberOfUgseats { get; set; }

        // A. Particulars of the Institution/College (3 fixed rows)
        public List<ContactRowViewModel> Contacts { get; set; } = new();

        // 3(b). Date of last inspection for UG / PG / SS (3 fixed rows)
        public List<InspectionRowViewModel> Inspections { get; set; } = new();
    }

    public class ContactRowViewModel
    {
        public string EntityType { get; set; } = string.Empty;   // 'Institution/College','Director/Dean/Principal','Medical Superintendent'
        public int EntitySequence { get; set; }

        public string? Name { get; set; }
        public string? Address { get; set; }
        public string? State { get; set; }
        public string? PinCode { get; set; }
        public string? PhoneOffice { get; set; }
        public string? PhoneResidence { get; set; }
        public string? Fax { get; set; }
        public string? MobileNo { get; set; }
        public string? Email { get; set; }
    }

    public class InspectionRowViewModel
    {
        public string CourseLevel { get; set; } = string.Empty;  // 'UG' / 'PG' / 'SS'
        public int CourseLevelSequence { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfLastInspection { get; set; }
        public string? Purpose { get; set; }
        public string? Result { get; set; }
    }
}