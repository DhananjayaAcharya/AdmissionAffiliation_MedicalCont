using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class InfrastructureDetailsViewModel
    {
        public string FacultyCode { get; set; } = string.Empty;
        public string CollegeCode { get; set; } = string.Empty;

        [Required]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        public string TypeOfAffiliation { get; set; } = string.Empty;

        // 1. Particulars of the Local Inspection Committee (3 fixed rows)
        public List<InspectionCommitteeRowVM> InspectionCommittee { get; set; } = new();

        // 2. Fee paid details (dynamic rows)
        public List<FeePaidRowVM> FeePaidDetails { get; set; } = new();

        // k. Other Course/Observership (dynamic rows)
        public List<OtherCourseRowVM> OtherCourses { get; set; } = new();

        // B.a. OPD
        [Display(Name = "No of rooms for consultation")]
        public int? NoOfRoomsForConsultation { get; set; }

        public List<OpdRoomAreaRowVM> OpdRoomAreas { get; set; } = new(); // seeded with Consultation Rooms / Demonstration room / Minor OT

        [Display(Name = "Waiting area")]
        public decimal? WaitingAreaInSqM { get; set; }

        [Display(Name = "Space and arrangements")]
        public string? SpaceAndArrangements { get; set; }          // Adequate / Not Adequate

        [Display(Name = "If not adequate, reasons")]
        public string? IfNotAdequateReasons { get; set; }

        [Display(Name = "Dressing Room")]
        public string? DressingRoomAvailable { get; set; }         // Available / Not available

        [Display(Name = "Separate Minor OT Male & Female")]
        public string? SeparateMinorOtMaleFemale { get; set; }     // Available / Not available

        [Display(Name = "Per Rectal examination Room")]
        public string? PerRectalExamRoomAvailable { get; set; }    // Available / Not available

        [Display(Name = "Dressing Room")]
        public string? DressingRoom2Available { get; set; }        // Available / Not available (2nd entry per form)

        // b. Wards
        [Display(Name = "No of wards - Male")]
        public int? NoOfWardsMale { get; set; }

        [Display(Name = "No of wards - Female")]
        public int? NoOfWardsFemale { get; set; }

        public List<WardsParameterRowVM> WardsParameters { get; set; } = new(); // 3 fixed rows

        // c. Total & Distribution of Operation Theatre
        public List<OtRowVM> OperationTheatres { get; set; } = new(); // dynamic rows + Total row

        // e. Seminar Room
        [Display(Name = "Space and facility")]
        public string? SeminarSpaceAndFacility { get; set; }       // Adequate / Not Adequate

        [Display(Name = "Internet facility")]
        public string? SeminarInternetFacility { get; set; }       // Available / Not Available

        [Display(Name = "Audiovisual equipment details")]
        public string? SeminarAvdetails { get; set; }

        // h. Departmental Museum
        [Display(Name = "Space")]
        public string? MuseumSpace { get; set; }

        [Display(Name = "Total number of Specimens")]
        public int? MuseumTotalSpecimens { get; set; }

        [Display(Name = "Total number of Chart / Diagram")]
        public int? MuseumTotalChartDiagram { get; set; }

        // f. Library facility (Central vs Departmental)
        public int? NumberOfBooksCentral { get; set; }
        public int? NumberOfBooksDepartmental { get; set; }
        public int? BooksPurchasedLast3YrsCentral { get; set; }
        public int? BooksPurchasedLast3YrsDept { get; set; }
        public bool AnnexureAttached { get; set; }
        public int? TotalIndianJournalsCentral { get; set; }
        public int? TotalIndianJournalsDept { get; set; }
        public int? TotalForeignJournalsCentral { get; set; }
        public int? TotalForeignJournalsDept { get; set; }
        public string? ComputerWithInternetCentral { get; set; }
        public string? ComputerWithInternetDept { get; set; }
        public string? CentralLibraryTiming { get; set; }
        public string? CentralReadingRoomTiming { get; set; }

        // g. Departmental Research Lab
        public string? ResearchLabSpace { get; set; }
        public string? ResearchLabEquipment { get; set; }
        public string? ResearchProjectsCompletedPast3Yrs { get; set; }
        public string? ResearchProjectsInProgress { get; set; }

        // h. Equipment (8 fixed rows)
        public List<DeptEquipmentRowVM> Equipment { get; set; } = new();
    }

    public class InspectionCommitteeRowVM
    {
        public int SlNo { get; set; }
        public string? NameOfChairmanOrMember { get; set; }
        public string? CorrespondenceAddress { get; set; }
        public string? PhoneOffResMobile { get; set; }
        public string? Email { get; set; }
    }

    public class FeePaidRowVM
    {
        public int SlNo { get; set; }
        public string? Particulars { get; set; }
        public decimal? Amount { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? BankName { get; set; }
        public string? BankBranch { get; set; }
    }

    public class OtherCourseRowVM
    {
        public string? NameOfQualificationCourse { get; set; }
        public string? PermittedByMcinmc { get; set; }   // Yes / No
        public int? NumberOfAdmissionsPerYear { get; set; }
    }

    public class OpdRoomAreaRowVM
    {
        public string RoomType { get; set; } = string.Empty;   // Consultation Rooms / Demonstration room / Minor OT / custom
        public decimal? AreaInSqM { get; set; }
    }

    public class WardsParameterRowVM
    {
        public string ParameterName { get; set; } = string.Empty; // fixed label
        public string? Details { get; set; }
    }

    public class OtRowVM
    {
        public int? SlNo { get; set; }
        public string? DepartmentName { get; set; }
        public int? MajorOtTables { get; set; }
        public int? MinorOtTables { get; set; }
        public bool IsTotalRow { get; set; }
    }

    public class DeptEquipmentRowVM
    {
        public string NameOfEquipment { get; set; } = string.Empty; // fixed label
        public int? NumbersAvailable { get; set; }
        public string? FunctionalStatus { get; set; }
        public string? IsAdequate { get; set; }   // Yes / No
    }
}