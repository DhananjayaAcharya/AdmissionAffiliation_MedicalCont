using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Medical_Affiliation.Models
{
    public class DentalFieldPracticeAreaViewModel
    {
        // ============================================================
        // Primary Key
        // ============================================================

        public int DentalFieldPracticeAreaId { get; set; }


        // ============================================================
        // Session Controlled Fields
        // ============================================================

        public int FacultyId { get; set; }

        public string? CollegeCode { get; set; }

        public int TypeId { get; set; }

        public string? CourseLevel { get; set; }


        // ============================================================
        // Field Type
        // Rural Field / Urban Field
        // ============================================================

        [Required(ErrorMessage = "Please select Field Type.")]
        public int? FieldTypeId { get; set; }

        public List<DropdownItem> FieldTypes { get; set; } = new();


        // ============================================================
        // Dental Field Practice Area Details
        // ============================================================

        // a. Location and Address

        [Required(ErrorMessage = "Please enter Location.")]
        [StringLength(
            250,
            ErrorMessage = "Location cannot exceed 250 characters."
        )]
        public string? Location { get; set; }


        [Required(ErrorMessage = "Please enter Address.")]
        [StringLength(
            500,
            ErrorMessage = "Address cannot exceed 500 characters."
        )]
        public string? Address { get; set; }


        // ============================================================
        // b. Managed By
        // ============================================================

        [Required(ErrorMessage = "Please enter Managed By.")]
        [StringLength(
            250,
            ErrorMessage = "Managed By cannot exceed 250 characters."
        )]
        public string? ManagedBy { get; set; }


        // ============================================================
        // c. Staff
        // ============================================================

        // New uploaded staff list
        public IFormFile? StaffListFile { get; set; }

        // Existing uploaded file path
        public string? StaffList { get; set; }


        // ============================================================
        // d. Population Served
        // ============================================================

        [Required(ErrorMessage = "Please enter Population Served.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Population Served must be greater than 0."
        )]
        public int? PopulationServed { get; set; }


        // ============================================================
        // e. Activities and Services Provided
        // ============================================================

        [Required(ErrorMessage = "Please enter Activities and Services provided.")]
        [StringLength(
            2000,
            ErrorMessage = "Activities and Services cannot exceed 2000 characters."
        )]
        public string? ActivitiesAndServices { get; set; }


        // ============================================================
        // f. Records Maintained
        // ============================================================

        [Required(ErrorMessage = "Please enter Records Maintained.")]
        [StringLength(
            2000,
            ErrorMessage = "Records Maintained cannot exceed 2000 characters."
        )]
        public string? RecordsMaintained { get; set; }


        // ============================================================
        // g. Equipments Available
        // ============================================================

        [Required(ErrorMessage = "Please enter Equipments Available.")]
        [StringLength(
            2000,
            ErrorMessage = "Equipments Available cannot exceed 2000 characters."
        )]
        public string? EquipmentsAvailable { get; set; }


        // ============================================================
        // h(i). Residential / Non-Residential Training Activities
        // ============================================================

        [Required(ErrorMessage = "Please enter Training Activities.")]
        [StringLength(
            2000,
            ErrorMessage = "Training Activities cannot exceed 2000 characters."
        )]
        public string? TrainingActivities { get; set; }


        // ============================================================
        // h(ii). How Supervision is Done
        // ============================================================

        [Required(ErrorMessage = "Please enter Supervision details.")]
        [StringLength(
            2000,
            ErrorMessage = "Supervision details cannot exceed 2000 characters."
        )]
        public string? SupervisionMethod { get; set; }


        // ============================================================
        // h(iii). Accommodation Available
        // ============================================================

        [Required(ErrorMessage = "Please enter Accommodation details.")]
        [StringLength(
            2000,
            ErrorMessage = "Accommodation details cannot exceed 2000 characters."
        )]
        public string? TraineeSupervisorAccommodation { get; set; }


        // ============================================================
        // Existing Records
        // ============================================================

        public List<DentalFieldPracticeArea> DentalFieldPracticeRecords { get; set; }
            = new();
    }
}