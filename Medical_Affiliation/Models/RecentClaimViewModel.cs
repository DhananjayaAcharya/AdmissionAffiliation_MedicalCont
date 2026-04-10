namespace Medical_Affiliation.Models
{
    public class RecentClaimViewModel
    {
        public string MemberName { get; set; }
        public string CollegeCode { get; set; }
        public string ModeOfTravel { get; set; }
        public decimal? TotalCost { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ApprovalStatus { get; set; }
        public string Remarks { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public string? CollegeName { get; set; }
    }
}