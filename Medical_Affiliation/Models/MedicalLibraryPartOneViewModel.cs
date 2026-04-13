using System.ComponentModel.DataAnnotations;

namespace Medical_Affiliation.Models
{
    public class CA_Aff_MedicalLibraryViewModel1
    {
        public string? CollegeCode { get; set; }

        public int? FacultyCode { get; set; }

        public int? AffiliationType { get; set; }

        public string? RegistrationNo { get; set; }

        public bool? IsFirstLogin { get; set; }

        // ===============================
        // SECTION 1 – LIBRARY SERVICES
        // ===============================

        public List<LibraryServiceRowViewModel1> LibraryServices { get; set; }
            = new List<LibraryServiceRowViewModel1>();

        // ===============================
        // SECTION 2 – USAGE REPORT
        // ===============================

        public IFormFile? UsageReportPdf { get; set; }
        public string? ExistingUsageReportFileName { get; set; }
        public int UsageReportId { get; set; }

        // ===============================
        // SECTION 3 – LIBRARY STAFF
        // ===============================

        public List<LibraryStaffViewModel1> LibraryStaff { get; set; }
            = new List<LibraryStaffViewModel1>();

        // ===============================
        // SECTION 4 – DEPARTMENTAL LIBRARY
        // ===============================

        public List<DepartmentLibraryViewModel1> DepartmentLibraries { get; set; }
            = new List<DepartmentLibraryViewModel1>();

        // ===============================
        // SECTION 5 – OTHER DETAILS
        // ===============================
        public MedicalLibraryOtherDetailsViewModel1 OtherDetails { get; set; }
            = new MedicalLibraryOtherDetailsViewModel1();


    }

    public class LibraryServiceRowViewModel1
    {
        public int? ServiceId { get; set; }

        public string? IsAvailable { get; set; }   // Yes / No
        public string ServiceName   { get; set; }

        // Mandatory ONLY for User Education Programme (ServiceId = 6)
        public IFormFile? UploadedPdf { get; set; }

        public int LibraryServiceId { get; set; }

        public bool HasPdf { get; set; }
        public string? ExistingFileName { get; set; }

    }

    public class LibraryStaffViewModel1
    {
        public int? Id { get; set; }

        public string? StaffName { get; set; }

        public string? Designation { get; set; }

        public string? Qualification { get; set; }

        public int? Experience { get; set; }

        public string? Category { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
    public class DepartmentLibraryViewModel1
    {
        public string? DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public int? TotalBooks { get; set; }

        public int? BooksAddedInYear { get; set; }

        public int? CurrentJournals { get; set; }

        public string? LibraryStaff { get; set; }

        public string? LibraryStaff1 { get; set; }
        public string? LibraryStaff2 { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class MedicalLibraryOtherDetailsViewModel1
    {
        public int? DigitalValuationId { get; set; }

        // Section 8
        public string? HasDigitalValuationCentre { get; set; }


        public int? NoOfSystems { get; set; }


        public string? HasStableInternet { get; set; }


        public string? HasCccameraSystem { get; set; }

        // Section 9
        public IFormFile? SpecialFeaturesPdf { get; set; }

        public bool HasSpecialFeatures { get; set; }

        public string? UploadedFileName { get; set; }

        public DateTime? CreatedDate { get; set; }

        public bool? HasSpecialFeaturesPdf { get; set; }

        public string? SpecialFeaturesQuestion { get; set; }


    }
}
