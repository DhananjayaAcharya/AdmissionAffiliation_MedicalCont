namespace Medical_Affiliation.Models
{
    public class LICCollegeDashboardViewModel
    {
        public string CollegeName { get; set; }

        public string? FacultyCode { get; set; }
        public string CollegeCode { get; set; }

        public LICMemberSummary SenateMember { get; set; }

        public LICMemberSummary ACMember { get; set; }

        public LICMemberSummary SubjectExpert { get; set; }

        public decimal TotalCollegeClaim { get; set; }

        public string ApprovalStatus { get; set; }
        public string LicApprovalFileName { get; set; }
        public string? Remarks { get; set; }

    }

    /////////////////////////////////////////////////////////////////
    ///



    // ─── Faculty ───────────────────────────────────────────────────────────────
    public class FacultyModel
    {
        public int FacultyId { get; set; }
        public string FacultyName { get; set; }
        public string EMS_FacultyId { get; set; }
        public string Faculty_Abbre { get; set; }
        public string Status { get; set; }
    }

    // ─── Inspection College Row ────────────────────────────────────────────────
    public class InspectionCollegeModel1
    {
        public int Id { get; set; }
        public string AcademicYear { get; set; }

        // AC Member
        public string ACMember { get; set; }
        public string AcMember_Phno { get; set; }

        // Senate Member
        public string SenetMember { get; set; }
        public string SenetMember_PhNo { get; set; }

        // Subject Expertise (Faculty)
        public string SubjectExpertise { get; set; }
        public string SubjectExpertise_PhNo { get; set; }

        public string FacultyCode { get; set; }
        public string CollegeName { get; set; }
        public string CollegePlace { get; set; }
        public string CollegeCode { get; set; }
        public string SE_RevisedOrder { get; set; }

        // Joined from Faculty table
        public string FacultyName { get; set; }
        public string FacultyAbbreviation { get; set; }

        // Claim status matched by mobile number
        public string ClaimStatus { get; set; }   // "Completed" | "Pending" | "Not Assigned"
        public string ClaimMobile { get; set; }
    }

    // ─── Dashboard ViewModel ───────────────────────────────────────────────────
    public class LICDashboardViewModel1
    {
        // Logged-in member info (passed from session)
        public string Name { get; set; }
        public string TypeofMember { get; set; }   // "AC Members" | "Senate Members" | "Subject Expertise"
        public string MobileNumber { get; set; }

        // Dashboard data
        public List<InspectionCollegeModel1> Colleges { get; set; } = new();

        // Summary counts
        public int TotalColleges => Colleges?.Count ?? 0;
        public int TotalFaculty { get; set; }
        public int ClaimsCompleted => Colleges?.Count(c => c.ClaimStatus == "Completed") ?? 0;
        public int ClaimsPending => Colleges?.Count(c => c.ClaimStatus == "Pending") ?? 0;
        public int NotAssigned => Colleges?.Count(c => c.ClaimStatus == "Not Assigned") ?? 0;

        // Filter inputs (bound on POST)
        public string SearchTerm { get; set; }
        public string StatusFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages { get; set; }
    }
}