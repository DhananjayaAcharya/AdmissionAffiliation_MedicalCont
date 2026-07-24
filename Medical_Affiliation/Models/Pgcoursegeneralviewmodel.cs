using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class PgCourseGeneralViewModel
    {
        // Identity (not user-editable, carried through hidden fields)
        public int PgcourseGeneralDetailId { get; set; }
        public string FacultyCode { get; set; } = string.Empty;
        public string CollegeCode { get; set; } = string.Empty;

        [Required]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        public string TypeOfAffiliation { get; set; } = string.Empty;

        [Required]
        public string AcademicYear { get; set; } = "2026-27";

        // a to h
        [Display(Name = "Date of LoP when PG course was first Permitted")]
        [DataType(DataType.Date)]
        public DateTime? LoPDate { get; set; }

        [Display(Name = "Number of years since start of PG course")]
        public int? YearsSinceStart { get; set; }

        [Display(Name = "Name of the Head of Department")]
        public string? HodName { get; set; }

        [Display(Name = "Number of PG Admissions (existing seats)")]
        public int? ExistingSeats { get; set; }

        [Display(Name = "Increase of Admissions applied for - From")]
        public int? IncreaseSeatsFrom { get; set; }

        [Display(Name = "Increase of Admissions applied for - To")]
        public int? IncreaseSeatsTo { get; set; }

        [Display(Name = "Total number of Units")]
        public int? TotalUnits { get; set; }

        [Display(Name = "Number of beds in the Department")]
        public int? DepartmentBeds { get; set; }

        [Display(Name = "Total ICU/HDU beds in the department")]
        public int? TotalICUHDUBeds { get; set; }

        // i. Unit-wise beds (fixed 8 rows, Unit-I to Unit-VIII)
        public List<UnitBedRowViewModel> Units { get; set; } = new();

        // j. ICU (fixed 2 rows: SICU, Post op ward / HDU)
        public List<IcuRowViewModel> IcuDetails { get; set; } = new();
    }

    public class UnitBedRowViewModel
    {
        public string UnitName { get; set; } = string.Empty;   // 'Unit-I' ... 'Unit-VIII'
        public int UnitSequence { get; set; }
        public int? NumberOfBeds { get; set; }
    }

    public class IcuRowViewModel
    {
        public string IcuType { get; set; } = string.Empty;    // 'Surgical ICU (SICU)', 'Post. op ward / HDU'
        public bool IsAvailable { get; set; }
        public int? TotalBeds { get; set; }
        public int? BedOccupancyOnInspectionDay { get; set; }
    }
}