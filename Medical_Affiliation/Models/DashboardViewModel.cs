namespace Medical_Affiliation.Models
{
    public class DashboardViewModel
    {
        public string CollegeCode { get; set; }
        public string CollegeName { get; set; }

        // Profile fields surfaced to the dashboard
        public string? FacultyName { get; set; }
        public string? CollegeLogoPath { get; set; }
        public string? PrincipalName { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Website { get; set; }
        public int? EstablishedYear { get; set; }

        public string? DistrictId { get; set; }
        public string? TalukId { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public List<DistrictMaster> Districts { get; set; } = new();

        public List<DashboardLateSubmissionFeeViewModel> LateSubmissionFees { get; set; } = new();

        // Application Overview panel (applied / progress / payments)
        public ApplicationOverviewViewModel ApplicationOverview { get; set; } = new();
    }

    public class DashboardLateSubmissionFeeViewModel
    {
        public int Id { get; set; }
        public int AffiliationTypeId { get; set; }
        public string? CourseLevel { get; set; }
        public DateTime? ActivationDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class ApplicationOverviewItem
    {
        public string TypeId { get; set; } = "";
        public string TypeName { get; set; } = "";
        public string CourseLevel { get; set; } = "";
        public int CourseCount { get; set; }
        public string? RegistrationNumber { get; set; }
        public DateTime? SubmittedOn { get; set; }

        public bool IsSubmitted { get; set; }
        public bool IsPaid { get; set; }
        public decimal AmountPaid { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }

        // 0 = nothing done, 50 = only one of submitted/paid, 100 = both
        public int Percent => IsSubmitted && IsPaid ? 100 : (IsSubmitted || IsPaid) ? 50 : 0;

        public string StatusText =>
            IsSubmitted && IsPaid ? "Completed" :
            IsSubmitted ? "Payment pending" :
            IsPaid ? "Paid, not submitted" : "Not started";

        public string StatusCss =>
            IsSubmitted && IsPaid ? "ok" : IsSubmitted ? "warn" : "bad";
    }

    public class ApplicationOverviewViewModel
    {
        public List<ApplicationOverviewItem> Items { get; set; } = new();
        public int TotalApplications => Items.Count;
        public int CompletedCount => Items.Count(i => i.IsSubmitted && i.IsPaid);
        public int PendingCount => Items.Count - CompletedCount;
        public decimal TotalPaid => Items.Sum(i => i.AmountPaid);
    }
}